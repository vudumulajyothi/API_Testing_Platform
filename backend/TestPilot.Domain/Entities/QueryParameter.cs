namespace TestPilot.Domain.Entities;

public class QueryParameter
{
    public Guid ParameterId { get; set; } = Guid.NewGuid();
    public Guid RequestId { get; set; }
    public ApiRequest Request { get; set; } = null!;
    public string ParamKey { get; set; } = string.Empty;
    public string ParamValue { get; set; } = string.Empty;
}
