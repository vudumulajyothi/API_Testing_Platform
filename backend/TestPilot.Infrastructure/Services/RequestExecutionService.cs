using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Web;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TestPilot.Application.DTOs.Execution;
using TestPilot.Application.Interfaces;
using TestPilot.Domain.Entities;
using TestPilot.Domain.Enums;

namespace TestPilot.Infrastructure.Services;

public class RequestExecutionService : IRequestExecutionService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ISsrfProtectionService _ssrfProtectionService;
    private readonly IApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<RequestExecutionService> _logger;

    private const int MaxResponseSizeBytes = 5 * 1024 * 1024; // 5 MB
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(30);

    public RequestExecutionService(
        IHttpClientFactory httpClientFactory,
        ISsrfProtectionService ssrfProtectionService,
        IApplicationDbContext context,
        IConfiguration configuration,
        ILogger<RequestExecutionService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _ssrfProtectionService = ssrfProtectionService;
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<ExecuteResponseDto> ExecuteAsync(ExecuteRequestDto request, Guid? userId = null)
    {
        var allowLocalhost = _configuration.GetValue<bool>("Security:AllowLocalhostInDevelopment");

        // 1. SSRF URL Validation
        await _ssrfProtectionService.ValidateUrlAsync(request.Url, allowLocalhost);

        // 2. Build Full Target URL with Query Parameters
        var targetUri = BuildUriWithQueryParams(request.Url, request.QueryParams);

        // 3. Prepare HttpRequestMessage
        var httpMethod = MapHttpMethod(request.Method);
        using var httpRequest = new HttpRequestMessage(httpMethod, targetUri);

        // 4. Attach Headers (Insulate internal authentication tokens)
        AttachHeaders(httpRequest, request.Headers);

        // 5. Attach Request Body (if applicable)
        if (ShouldIncludeBody(request.Method) && !string.IsNullOrEmpty(request.Body))
        {
            httpRequest.Content = new StringContent(request.Body, Encoding.UTF8, "application/json");
        }

        // 6. Send Request with Precise Timing
        var client = _httpClientFactory.CreateClient("TestPilotClient");
        using var cts = new CancellationTokenSource(DefaultTimeout);

        var stopwatch = Stopwatch.StartNew();
        HttpResponseMessage response;

        try
        {
            response = await client.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cts.Token);
        }
        catch (OperationCanceledException) when (!cts.Token.IsCancellationRequested)
        {
            throw new TimeoutException($"Request to '{request.Url}' timed out after {DefaultTimeout.TotalSeconds} seconds.");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "HTTP request failed for URL: {Url}", request.Url);
            throw new InvalidOperationException($"Unable to connect to target endpoint '{request.Url}': {ex.Message}", ex);
        }

        // 7. Read Response Content with Size Safeguard
        var rawBody = await ReadResponseContentSafelyAsync(response.Content, cts.Token);
        stopwatch.Stop();

        var elapsedMs = stopwatch.ElapsedMilliseconds;

        // 8. Capture Response Headers
        var responseHeaders = new Dictionary<string, string>();
        foreach (var header in response.Headers)
        {
            responseHeaders[header.Key] = string.Join(", ", header.Value);
        }
        if (response.Content != null)
        {
            foreach (var header in response.Content.Headers)
            {
                responseHeaders[header.Key] = string.Join(", ", header.Value);
            }
        }

        // 9. Format/Parse Body
        object? parsedBody = null;
        if (!string.IsNullOrWhiteSpace(rawBody))
        {
            try
            {
                parsedBody = JsonSerializer.Deserialize<JsonElement>(rawBody);
            }
            catch
            {
                parsedBody = rawBody;
            }
        }

        var resultDto = new ExecuteResponseDto
        {
            StatusCode = (int)response.StatusCode,
            StatusText = response.StatusCode.ToString(),
            ResponseTimeMs = elapsedMs,
            ContentLength = response.Content?.Headers.ContentLength ?? (long)(rawBody?.Length ?? 0),
            Headers = responseHeaders,
            Body = parsedBody,
            RawBody = rawBody,
            IsSuccessStatusCode = response.IsSuccessStatusCode
        };

        // 10. Audit History Logging (if authenticated)
        if (userId.HasValue)
        {
            await LogRequestHistoryAsync(userId.Value, request, resultDto);
        }

        return resultDto;
    }

    private static Uri BuildUriWithQueryParams(string baseUrl, List<KeyValuePairDto>? queryParams)
    {
        if (queryParams == null || queryParams.Count == 0)
        {
            return new Uri(baseUrl);
        }

        var uriBuilder = new UriBuilder(baseUrl);
        var query = HttpUtility.ParseQueryString(uriBuilder.Query);

        foreach (var param in queryParams.Where(p => !string.IsNullOrWhiteSpace(p.Key)))
        {
            query[param.Key.Trim()] = param.Value ?? string.Empty;
        }

        uriBuilder.Query = query.ToString();
        return uriBuilder.Uri;
    }

    private static void AttachHeaders(HttpRequestMessage request, List<KeyValuePairDto>? headers)
    {
        if (headers == null) return;

        foreach (var h in headers.Where(h => !string.IsNullOrWhiteSpace(h.Key)))
        {
            var key = h.Key.Trim();
            var val = h.Value ?? string.Empty;

            // Strip pseudo-headers or headers managed by framework
            if (key.Equals("Host", StringComparison.OrdinalIgnoreCase) ||
                key.Equals("Connection", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            // Try adding to request headers; if it fails, try content headers
            if (!request.Headers.TryAddWithoutValidation(key, val))
            {
                request.Content?.Headers.TryAddWithoutValidation(key, val);
            }
        }
    }

    private static async Task<string> ReadResponseContentSafelyAsync(HttpContent? content, CancellationToken ct)
    {
        if (content == null) return string.Empty;

        using var stream = await content.ReadAsStreamAsync(ct);
        using var reader = new StreamReader(stream, Encoding.UTF8);

        var buffer = new char[8192];
        var sb = new StringBuilder();
        int bytesRead;

        while ((bytesRead = await reader.ReadAsync(buffer, 0, buffer.Length)) > 0)
        {
            if (sb.Length + bytesRead > MaxResponseSizeBytes)
            {
                sb.Append(buffer, 0, MaxResponseSizeBytes - sb.Length);
                sb.Append("\n... [Response payload truncated: exceeded 5MB cap]");
                break;
            }
            sb.Append(buffer, 0, bytesRead);
        }

        return sb.ToString();
    }

    private static HttpMethod MapHttpMethod(HttpMethodType type) => type switch
    {
        HttpMethodType.GET => HttpMethod.Get,
        HttpMethodType.POST => HttpMethod.Post,
        HttpMethodType.PUT => HttpMethod.Put,
        HttpMethodType.PATCH => HttpMethod.Patch,
        HttpMethodType.DELETE => HttpMethod.Delete,
        HttpMethodType.HEAD => HttpMethod.Head,
        HttpMethodType.OPTIONS => HttpMethod.Options,
        _ => HttpMethod.Get
    };

    private static bool ShouldIncludeBody(HttpMethodType type)
    {
        return type is HttpMethodType.POST or HttpMethodType.PUT or HttpMethodType.PATCH;
    }

    private async Task LogRequestHistoryAsync(Guid userId, ExecuteRequestDto req, ExecuteResponseDto res)
    {
        try
        {
            // Truncate response body for history table
            var truncatedBody = res.RawBody;
            if (truncatedBody != null && truncatedBody.Length > 10_000)
            {
                truncatedBody = truncatedBody[..10_000] + "... [truncated]";
            }

            var history = new RequestHistory
            {
                HistoryId = Guid.NewGuid(),
                UserId = userId,
                RequestId = req.RequestId,
                Method = req.Method,
                Url = req.Url,
                StatusCode = res.StatusCode,
                ResponseTimeMs = res.ResponseTimeMs,
                ResponseBody = truncatedBody,
                ExecutedAt = DateTime.UtcNow
            };

            await _context.RequestHistories.AddAsync(history);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to record request history for user '{UserId}'", userId);
        }
    }
}
