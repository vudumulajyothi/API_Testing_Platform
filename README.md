# TestPilot – AI-Powered API Testing Platform

[![TestPilot CI](https://github.com/placeholder/TestPilot/actions/workflows/ci.yml/badge.svg)](https://github.com)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![.NET 8.0](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/)
[![Angular](https://img.shields.io/badge/Angular-18+-red.svg)](https://angular.dev/)

> **TestPilot** is a modern, placement-ready API testing platform that integrates HTTP request authoring, response inspection, automated assertion execution, test history auditing, and AI-assisted test-case generation.

---

## 🎯 Key Features

- **Intuitive Request Builder**: Craft `GET`, `POST`, `PUT`, `PATCH`, and `DELETE` requests with dynamic query parameters, custom headers, and JSON body editor.
- **High-Fidelity Response Inspection**: Inspect status codes, latency in milliseconds, payload sizes, response headers, and formatted JSON.
- **SSRF-Safe Execution Engine**: Enterprise-grade security preventing loopback exploitation, private network scanning, and DNS rebinding attacks.
- **Collection Management**: Organize endpoints into logical collections with ownership-based access control.
- **Automated Assertions & Test Runner**: Define status code checks, response-time thresholds, and JSON field validations.
- **AI Test Assistant**: Suggest edge-case test assertions automatically based on API endpoints and sample payloads.
- **Test History & Reports**: Track request audits, pass/fail trends, and exportable API documentation.

---

## 🏗️ Architecture

TestPilot follows **Clean / Onion Architecture** principles:

```
TestPilot/
├── backend/
│   ├── TestPilot.API/            # Controllers, Middlewares, DI Setup, Swagger
│   ├── TestPilot.Application/    # Use cases, DTOs, Service Interfaces
│   ├── TestPilot.Domain/         # Domain entities, Enums, Business rules
│   ├── TestPilot.Infrastructure/ # EF Core, SQL Server, Safe HttpClient, AI Provider
│   └── TestPilot.Tests/          # Unit & Integration Tests (xUnit)
└── frontend/
    └── testpilot-ui/             # Angular Standalone SPA with Reactive UI
```

---

## 🚀 Getting Started

### Prerequisites
- [.NET 8.0 or 10.0 SDK](https://dotnet.microsoft.com/download)
- [Node.js (v20+ or v24+)](https://nodejs.org/)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/) (or LocalDB / SQLite for local development)

### Backend Setup
```bash
cd backend
dotnet restore
dotnet build
dotnet run --project TestPilot.API
```
Backend will be available at: `https://localhost:7123` / `http://localhost:5123` with Swagger UI at `/swagger`.

### Frontend Setup
```bash
cd frontend/testpilot-ui
npm install
npm start
```
Frontend will be available at: `http://localhost:4200`.

---

## 🧪 Testing

```bash
# Run backend tests
cd backend
dotnet test

# Run frontend tests
cd frontend/testpilot-ui
npm test
```

---

## 🛡️ Security Highlights
- Strict SSRF protections against internal network scanning.
- Masking of sensitive tokens (`Authorization`, `Cookie`) in stored histories.
- JWT Bearer authentication with strong password hashing.
- CORS restricted to registered frontend origins.

---

## 📄 License
This project is open-source and available under the [MIT License](LICENSE).
