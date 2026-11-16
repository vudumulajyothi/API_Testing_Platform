# TestPilot – Requirements Specification

## 1. Introduction
TestPilot is an AI-powered API testing platform designed to simplify API exploration, request authoring, response inspection, automated assertion verification, collection management, and test reporting.

---

## 2. Functional Requirements

### 2.1 User Management & Authentication
- **FR-AUTH-01**: Users must be able to register with their name, email, and password.
- **FR-AUTH-02**: Passwords must be hashed using a cryptographically secure algorithm (e.g., ASP.NET Core PasswordHasher / PBKDF2 with SHA-256).
- **FR-AUTH-03**: Users must be able to authenticate and receive a signed JWT token containing identity claims.
- **FR-AUTH-04**: Secure routes must validate the JWT bearer token and enforce user ownership.

### 2.2 Request Execution Engine
- **FR-EXEC-01**: Support standard HTTP methods: `GET`, `POST`, `PUT`, `PATCH`, `DELETE`, `HEAD`, `OPTIONS`.
- **FR-EXEC-02**: Support query parameters and custom headers.
- **FR-EXEC-03**: Support raw JSON request body editing and validation.
- **FR-EXEC-04**: Capture and return execution metrics:
  - HTTP Status Code & Status Text.
  - Response duration (latency in milliseconds).
  - Response headers.
  - Formatted response body (JSON, text, HTML).
  - Response payload byte size.
- **FR-EXEC-05**: Maintain an immutable request execution history linked to the user.

### 2.3 Collections & Request Management
- **FR-COLL-01**: Users can create, update, list, and delete custom API Collections.
- **FR-COLL-02**: Requests can be saved inside collections with names, descriptions, parameters, and headers.
- **FR-COLL-03**: Collections can be duplicated or exported into JSON format.

### 2.4 Automated Assertions & Test Runner
- **FR-TEST-01**: Users can configure assertions on saved requests:
  - Status Code equals expected value (e.g., `200`, `201`, `204`).
  - Response time is less than maximum threshold (e.g., `< 1000ms`).
  - JSON property path equality (e.g., `data.id == 1`).
  - JSON property exists / not empty.
- **FR-TEST-02**: Execute test suites sequentially and record execution runs (`TestRun` and `TestResult`).
- **FR-TEST-03**: Provide visual indicators for passed and failed assertions.

### 2.5 AI-Assisted Test Case Generation
- **FR-AI-01**: Provide an AI assistant endpoint that accepts request details and sample responses.
- **FR-AI-02**: Generate recommended edge-case and boundary assertions.
- **FR-AI-03**: Users must review and explicitly accept AI suggestions before saving them as test cases.

### 2.6 Dashboard & Reporting
- **FR-DASH-01**: Display high-level metrics: total requests sent, total collections, recent test runs, overall pass/fail rate.
- **FR-DASH-02**: Provide filtered views of recent test executions and history logs.

---

## 3. Non-Functional Requirements

### 3.1 Security & SSRF Protection
- **NFR-SEC-01**: Server-Side Request Forgery (SSRF) Safeguards:
  - Disallow resolution to internal private IP ranges (`10.0.0.0/8`, `172.16.0.0/12`, `192.168.0.0/16`, `127.0.0.0/8`).
  - Disallow cloud metadata endpoints (`169.254.169.254`).
  - Validate IP addresses on redirects to prevent DNS rebinding.
- **NFR-SEC-02**: Sensitive Data Masking:
  - Sanitize headers like `Authorization`, `Cookie`, `X-Api-Key` prior to saving history or sending payloads to LLMs.
- **NFR-SEC-03**: Rate Limiting & Timeouts:
  - Enforce a 30-second execution timeout per request.
  - Limit response capture payload to a maximum of 5MB.

### 3.2 Performance & Scalability
- **NFR-PERF-01**: Backend API latency for management operations should be under 150ms.
- **NFR-PERF-02**: Database indexes applied on foreign keys and user lookup fields (`UserId`, `CollectionId`, `RequestId`).

### 3.3 Usability & Maintainability
- **NFR-MAINT-01**: Clean Architecture separating Domain, Application, Infrastructure, and Presentation.
- **NFR-MAINT-02**: Reactive and responsive Angular frontend for desktop and mobile viewports.
