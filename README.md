# TestPilot – AI-Powered API Testing Platform

[![TestPilot CI](https://github.com/placeholder/TestPilot/actions/workflows/ci.yml/badge.svg)](https://github.com)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![.NET 8.0](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/)
[![Angular 18](https://img.shields.io/badge/Angular-18+-red.svg)](https://angular.dev/)
[![SQL Server](https://img.shields.io/badge/Database-SQL%20Server-0078D4.svg)](https://www.microsoft.com/sql-server)
[![Azure Ready](https://img.shields.io/badge/Cloud-Microsoft%20Azure-0089D6.svg)](https://azure.microsoft.com/)

> **TestPilot** is a full-stack, enterprise-grade API testing platform built to demonstrate production-ready software engineering skills relevant for roles at organizations like **Spektra Systems**. It integrates intuitive request authoring, automated assertion verification, hierarchical collection management, audit history tracking, and AI-assisted test-case generation.

---

## 🏛️ System Architecture

TestPilot strictly follows **Clean / Onion Architecture** on the backend and **Feature-Driven Modular Architecture** on the Angular frontend.

```
                    +------------------------------------+
                    |        Angular Frontend (SPA)      |
                    |  - Standalone Components (v18+)    |
                    |  - Reactive Signals & Forms        |
                    |  - Interceptors (JWT, Errors)      |
                    +-----------------+------------------+
                                      | HTTPS REST + JWT
                                      v
                    +------------------------------------+
                    |    TestPilot.API (ASP.NET Core)    |
                    |  - Thin Controllers                |
                    |  - RFC 7807 Problem Details        |
                    |  - JWT Authentication & CORS       |
                    |  - Swagger / OpenAPI Docs          |
                    +-----------------+------------------+
                                      |
                                      v
                    +------------------------------------+
                    |       TestPilot.Application        |
                    |  - Use Cases & Service Contracts   |
                    |  - DTOs & Schema Validations       |
                    |  - Assertion Evaluator Engine      |
                    +--------+------------------+--------+
                             |                  |
            +----------------+                  +----------------+
            v                                                    v
+-----------------------+                            +-----------------------+
|   TestPilot.Domain    |                            |TestPilot.Infrastructure|
| - Entities (Users,    |                            | - EF Core DbContext   |
|   Collections,        |                            | - SQL Server / InMem  |
|   Requests, Tests)    |                            | - SSRF-Safe HttpClient|
| - Domain Enums        |                            | - PBKDF2 PasswordHash |
| - Zero Dependencies   |                            | - AI Suggestion Engine|
+-----------------------+                            +-----------------------+
```

---

## ⚡ Core Feature Highlights

| Module | Feature Capabilities |
|---|---|
| 🔐 **Authentication** | Cryptographically salted PBKDF2 password hashing (100k iterations), signed JWT tokens, claim validation, protected routes. |
| 🛠️ **Request Builder** | Multi-method HTTP support (`GET`, `POST`, `PUT`, `PATCH`, `DELETE`), header manager, query parameter builder, JSON body editor. |
| 🛡️ **SSRF Security** | Automated DNS resolution checks blocking loopback (`127.0.0.1`), private networks (`10.0.0.0/8`, `192.168.0.0/16`), and cloud metadata (`169.254.169.254`). |
| 📁 **Collections** | Hierarchical grouping of requests with strict user ownership enforcement and cascading deletes. |
| 🧪 **Test Runner** | Automated assertion evaluation: HTTP status code, SLA response latency (< threshold), deep JSONPath equality, field existence, and body substring search. |
| 🤖 **AI Assistant** | AI-suggested boundary test assertions with sensitive header redaction (`[REDACTED]`) and strict schema validation. |
| 📊 **Dashboard & Audit** | Paginated execution logs, real-time aggregate pass rate calculations, and Markdown / Postman v2.1 documentation export. |

---

## 📂 Repository Directory Layout

```
TestPilot/
├── .github/workflows/ci.yml       # GitHub Actions CI pipeline
├── AGENTS.md                      # AI agent development rules & security guardrails
├── docs/                          # Comprehensive technical documentation
│   ├── REQUIREMENTS.md            # Functional & Non-functional specs
│   ├── ARCHITECTURE.md            # Clean Architecture patterns & component breakdown
│   ├── DATABASE-DESIGN.md         # Relational SQL schema, tables, and ERD
│   ├── API-DOCUMENTATION.md       # Versioned /api/v1 REST endpoint guide
│   ├── TEST-CASES.md              # Quality assurance & security test matrix
│   ├── AZURE-DEPLOYMENT.md        # Azure App Service, Static Web Apps, SQL guide
│   └── INTERVIEW-PREPARATION.md   # 10 core placement interview answers
├── backend/                       # .NET 8 Multi-Project Solution
│   ├── TestPilot.sln
│   ├── TestPilot.Domain/          # Core domain models and enums
│   ├── TestPilot.Application/     # Application services, DTOs, interfaces
│   ├── TestPilot.Infrastructure/  # EF Core, SSRF HttpClient, PBKDF2, AI
│   ├── TestPilot.API/             # Controllers, middlewares, Program.cs
│   └── TestPilot.Tests/           # xUnit Unit & Integration test suites
└── frontend/testpilot-ui/         # Modern Angular 18 Standalone Application
    ├── src/app/core/              # Guards, interceptors, services
    ├── src/app/features/          # Auth, dashboard, request-builder, test-runner
    ├── src/app/shared/            # Header, layout, common widgets
    ├── e2e/                       # Playwright end-to-end tests
    └── playwright.config.ts       # Playwright browser configurations
```

---

## 🚀 Getting Started Locally

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js v20+ or v24+](https://nodejs.org/) with `npm`
- [SQL Server](https://www.microsoft.com/sql-server) or LocalDB (in-memory mode supported out of the box for testing)

### 1. Backend Setup (.NET 8 Web API)
```bash
cd backend
dotnet restore
dotnet build
dotnet run --project TestPilot.API
```
- API Endpoint: `http://localhost:5123` / `https://localhost:7123`
- Interactive Swagger UI: `http://localhost:5123/swagger`

### 2. Frontend Setup (Angular 18)
```bash
cd frontend/testpilot-ui
npm install
npm start
```
- Frontend Web App: `http://localhost:4200`

---

## 🧪 Automated Testing

```bash
# Run all backend unit and integration tests (xUnit)
cd backend
dotnet test --verbosity normal

# Run frontend end-to-end tests (Playwright)
cd frontend/testpilot-ui
npx playwright test
```

---

## 💼 Placement & Interview Readiness
Review the [Interview Preparation Guide](docs/INTERVIEW-PREPARATION.md) for detailed explanations of design choices, SSRF protections, JWT authentication, and Azure cloud deployments.

---

## 📄 License
This project is open-source and available under the [MIT License](LICENSE).
