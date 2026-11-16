# TestPilot Development Instructions

## Project Overview
TestPilot is an enterprise-ready, AI-powered API testing platform designed to streamline API request execution, automated assertion testing, collection management, test history tracking, and AI-assisted test-case generation.

## Backend Guidelines (.NET / C#)
- **Framework**: Use C# and modern ASP.NET Core Web API.
- **Architecture**: Strictly adhere to Clean / Layered Architecture:
  - `TestPilot.Domain`: Pure domain entities, value objects, and enums (no external dependencies).
  - `TestPilot.Application`: Use cases, service contracts, DTOs, mappings, business validations.
  - `TestPilot.Infrastructure`: EF Core DbContext, repository implementations, external HTTP execution engine, LLM integration, and database migrations.
  - `TestPilot.API`: Thin controllers, route definitions, middleware, filters, dependency injection setup, and Swagger.
  - `TestPilot.Tests`: Unit tests (xUnit + Moq/FluentAssertions) and integration tests.
- **Data Access**: Use Entity Framework Core with SQL Server. Ensure appropriate indexes, foreign keys, and cascading delete safeguards.
- **Dependency Injection**: Register all application and infrastructure services cleanly using extension methods.
- **Error Handling**: Use centralized exception handling middleware to return RFC 7807 Problem Details. Never leak stack traces in production.
- **Validation**: Validate all incoming DTOs before reaching the domain layer.

## Frontend Guidelines (Angular / TypeScript)
- **Framework**: Angular 18+ utilizing Standalone Components, reactive signals, and modern control flow (`@if`, `@for`, `@switch`).
- **Architecture**:
  - `core/`: Singletons, authentication guards, HTTP interceptors (JWT bearer, error handling), core models.
  - `shared/`: Reusable presentational components, pipes, directives, and UI utility widgets.
  - `features/`: Isolated feature modules/routes (`auth`, `dashboard`, `request-builder`, `collections`, `test-runner`, `history`, `reports`).
- **State & Communication**: Use Angular reactive forms (`FormGroup`, `FormControl`) and state services with RxJS / Signals. Never invoke `HttpClient` directly inside components.
- **User Experience**: Handle loading spinners, empty states, and validation feedback gracefully.

## Code Quality & Standards
- Follow standard C# and TypeScript naming conventions (PascalCase for C# classes/methods, camelCase for TS variables/methods).
- Keep controllers thin; do not put business or execution logic inside controllers.
- Never hardcode secrets, passwords, or API keys in source code or repository commits.
- Run and maintain automated unit/integration tests before finalizing features.
- Avoid modifying unrelated files during task execution.

## Security Posture & Guardrails
- **SSRF Prevention**: Block loopback (`127.0.0.1`, `::1`), private networks (`10.0.0.0/8`, `172.16.0.0/12`, `192.168.0.0/16`), link-local (`169.254.0.0/16`), and cloud metadata IP endpoints (`169.254.169.254`) in production request execution.
- **Isolation**: Prevent forwarding the platform's internal JWT or authorization headers to third-party tested endpoints.
- **Resource Limits**: Enforce request timeouts (default 30s) and maximum response size caps (e.g., 5MB) to prevent denial of service.
- **Data Masking**: Mask sensitive header values (e.g., `Authorization`, `Cookie`, `X-Api-Key`) when storing in request history and when sending payloads to the AI suggestion service.
- **Data Ownership**: Strictly verify user ownership for collections, requests, test cases, and history records at the service layer.
