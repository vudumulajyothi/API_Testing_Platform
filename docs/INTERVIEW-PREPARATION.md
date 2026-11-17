# TestPilot – Technical Interview Preparation Guide
*Specific to the Spektra Systems Role (.NET, Angular, Azure, AI-Assisted Development)*

---

### 1. Project Introduction Elevator Pitch (30–60 Seconds)
> *"TestPilot is an enterprise-grade, AI-powered API testing platform developed using Angular 18+, ASP.NET Core Web API, and SQL Server. It empowers developers and QA engineers to craft and execute HTTP requests, organize collections, automate response assertions (status code, latency, and deep JSON property paths), audit request histories, and leverage AI to suggest boundary test cases. I engineered the backend adhering strictly to Clean Architecture, implemented robust SSRF guardrails and cryptographic password hashing, authored xUnit and Playwright test suites, and configured automated CI pipelines."*

---

### 2. Core Technical Questions & Sample High-Scoring Answers

#### Q1: Why did you choose ASP.NET Core and Clean Architecture?
**Answer:**
- **ASP.NET Core**: Chosen for its high-throughput Kestrel web server, built-in dependency injection, native asynchronous I/O (`async/await`), first-class support for JWT bearer authentication, and enterprise cross-platform deployment capabilities on Azure Linux containers.
- **Clean / Layered Architecture**:
  - `TestPilot.Domain`: Houses core entities (`User`, `Collection`, `ApiRequest`, `TestCase`) and domain enums with **zero** external dependencies.
  - `TestPilot.Application`: Contains business logic, DTOs, and interface contracts (`IRequestExecutionService`, `IAuthService`).
  - `TestPilot.Infrastructure`: Encapsulates EF Core database access, external HTTP client execution, and security implementations.
  - `TestPilot.API`: Keeps controllers thin by delegating execution to the application layer and formatting responses according to RFC 7807 Problem Details.
  - *Key Benefit*: Isolates business logic from UI and infrastructure concerns, making the codebase highly testable and maintainable.

---

#### Q2: What is the fundamental difference between GET and POST?
**Answer:**
- **Idempotence & Safety**: A `GET` request is safe and idempotent—it retrieves data without mutating server state and can be repeated indefinitely without side effects. A `POST` request is neither safe nor idempotent; it typically submits an entity causing state changes (such as creating a user or recording an execution).
- **Payload Transmission**: `GET` transmits parameters via the URL query string (length-constrained by browsers and web servers and cached in logs/history). `POST` transmits payloads in the request body (allowing structured JSON/XML payloads and larger sizes).
- **In TestPilot**: Both methods are executed through a unified safe HTTP client that automatically attaches URL query strings for `GET` and UTF-8 encoded `StringContent` for `POST`/`PUT`/`PATCH`.

---

#### Q3: How does JWT authentication work and how do you protect endpoints?
**Answer:**
- **Token Generation**: Upon successful login, the server generates a signed JSON Web Token (JWT) consisting of three Base64URL parts: Header, Payload (claims: `sub`, `email`, `name`, `jti`), and Signature. The signature is computed using `HMAC-SHA256` with a secure server-side secret.
- **Token Verification**: On subsequent API calls, the frontend's `jwtInterceptor` attaches `Authorization: Bearer <token>`. ASP.NET Core's `JwtBearerHandler` validates the cryptographic signature, expiration (`exp`), issuer, and audience without needing a database roundtrip.
- **Controller Protection**: Endpoints are decorated with `[Authorize]`. Custom ownership checks verify that the authenticated user ID from `User.FindFirstValue(ClaimTypes.NameIdentifier)` owns the requested resource before returning data.

---

#### Q4: How does Angular communicate with ASP.NET Core?
**Answer:**
- **Service Layer Isolation**: Components never invoke `HttpClient` directly. All network interaction is mediated through injectable services (`ApiService`, `AuthService`).
- **Functional Interceptors**:
  - `jwtInterceptor`: Clones outgoing requests and appends the Bearer token if present in `localStorage`.
  - `errorInterceptor`: Catches global HTTP error responses; on `401 Unauthorized`, it clears local tokens and navigates to `/auth/login`.
- **Reactive State & Signals**: The UI consumes service observables via RxJS or reactive Angular Signals (`signal()`), ensuring modern change detection and clean component lifecycles.

---

#### Q5: What is Entity Framework Core and how do you optimize its performance?
**Answer:**
- **EF Core**: An Object-Relational Mapper (ORM) that maps C# domain entities to SQL Server tables using Fluent API mappings, tracking changes and automating SQL generation.
- **Performance Optimizations in TestPilot**:
  - **Read-Only Queries**: Applied `.AsNoTracking()` on all query operations (`GetUserCollectionsAsync`, `GetRequestByIdAsync`, `GetDashboardSummaryAsync`) to eliminate change-tracker overhead and memory allocations.
  - **Strategic Indexing**: Unique index on `Users.Email` and foreign key indexes on `CollectionId`, `RequestId`, and `UserId` to optimize join lookups.
  - **Explicit Projections**: Used `.Select()` projections to fetch only required scalar properties instead of querying entire graphs.
  - **Streaming Data Limits**: Truncated large response bodies prior to writing into relational `RequestHistory` table records.

---

#### Q6: How do you prevent Server-Side Request Forgery (SSRF)?
**Answer:**
- **The Threat**: In an API testing platform, a malicious user could submit requests to internal endpoints (such as `http://localhost:5000`, `http://192.168.1.1`, or `http://169.254.169.254`) to probe internal network infrastructure or steal cloud instance metadata credentials.
- **Defense in TestPilot (`SsrfProtectionService`)**:
  1. **Scheme Validation**: Enforces only `http://` and `https://` schemes.
  2. **Hostname & IP Resolution**: Resolves the hostname to IP addresses via DNS *prior* to opening any connection.
  3. **IP Range Blacklisting**: Inspects every resolved IP and blocks:
     - Loopback (`127.0.0.0/8`, `::1`)
     - Cloud metadata endpoints (`169.254.169.254`) and link-local (`169.254.0.0/16`)
     - RFC 1918 private subnets (`10.0.0.0/8`, `172.16.0.0/12`, `192.168.0.0/16`, `fc00::/7`)
  4. **Credential Isolation**: Strips internal platform Authorization tokens before dispatching requests to external endpoints.

---

#### Q7: What is the difference between unit and integration testing in TestPilot?
**Answer:**
- **Unit Tests (`xUnit` + `FluentAssertions`)**: Test isolated units of code in memory with mocked dependencies.
  - *Examples*: `PasswordHasherServiceTests` (hashing logic, salt randomness), `AssertionEvaluationTests` (JSONPath parsing, latency threshold math), `SsrfProtectionServiceTests` (IP subnet checks).
- **Integration Tests (`Microsoft.AspNetCore.Mvc.Testing`)**: Test the complete ASP.NET Core pipeline including routing, middleware, model binding, authorization, and database persistence using `CustomWebApplicationFactory` with an in-memory database.
  - *Examples*: `AuthAndExecutionIntegrationTests` exercises user registration -> JWT generation -> protected endpoint access -> unauthorized 401 rejection.

---

#### Q8: How did you use AI coding tools (Claude Code / Antigravity) responsibly?
**Answer:**
- *"I treated AI as a collaborative pair programmer rather than an unchecked generator. I guided the implementation feature-by-feature using clear engineering specifications (AGENTS.md). I reviewed every proposed architecture layer, ensured sensitive tokens were sanitized, added automated unit/integration tests to verify behavior, and validated that all code conformed to Clean Architecture principles."*

---

#### Q9: How do you deploy and monitor this stack on Microsoft Azure?
**Answer:**
- **Deployment Topology**:
  - Frontend hosted on **Azure Static Web Apps** with global edge CDN distribution.
  - Backend API hosted on **Azure App Service** (Linux container runtime with .NET 8).
  - Database on **Azure SQL Database** with encrypted TDS connections and firewall rules.
- **CI/CD Pipeline**: GitHub Actions builds the .NET solution, executes all unit tests, lints the Angular application, and deploys artifacts upon pull requests into `main`.
- **Monitoring**: Azure Application Insights and Azure Monitor capture request rates, latency, failed requests, and server exceptions in real time.
