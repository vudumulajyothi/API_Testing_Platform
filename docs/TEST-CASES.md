# TestPilot – Test Strategy & Test Cases

## 1. Test Strategy Overview
Quality assurance in TestPilot spans:
1. **Unit Testing**: Validating domain logic, assertion evaluations, DTO validations, and URL safety analyzers without external dependencies.
2. **Integration Testing**: Testing API controllers and database persistence using in-memory or mock databases.
3. **Security Testing**: Validating SSRF protection, IP blacklisting, secret masking, and authorization boundaries.
4. **End-to-End (E2E) Testing**: Exercising the full Angular frontend user journeys.

---

## 2. Test Cases Matrix

| Test ID | Module | Scenario | Expected Outcome |
|---|---|---|---|
| **TC-AUTH-01** | Auth | Register with valid unique email | 201 Created with JWT token and user info |
| **TC-AUTH-02** | Auth | Register with existing email | 400 Bad Request ("Email already registered") |
| **TC-AUTH-03** | Auth | Login with incorrect password | 401 Unauthorized |
| **TC-AUTH-04** | Auth | Access protected route without token | 401 Unauthorized |
| **TC-SEC-01** | Security | Execute request to `http://localhost:5000` in Prod | 400 Bad Request ("Loopback address blocked") |
| **TC-SEC-02** | Security | Execute request to `http://169.254.169.254/latest/meta-data` | 400 Bad Request ("Cloud metadata IP blocked") |
| **TC-SEC-03** | Security | Execute request to private IP `http://192.168.1.1` | 400 Bad Request ("Private IP blocked") |
| **TC-SEC-04** | Security | Execute request with sensitive headers | Masked values in saved history log |
| **TC-EXEC-01** | Execution | GET request to valid public API | 200 OK with valid headers, body, duration |
| **TC-EXEC-02** | Execution | POST request with JSON payload | Body transmitted accurately, response recorded |
| **TC-EXEC-03** | Execution | Request exceeding 30s timeout | 408 Request Timeout / handled gracefully |
| **TC-TEST-01** | Runner | Assertion: StatusCode equals 200 (actual 200) | Test marked Passed |
| **TC-TEST-02** | Runner | Assertion: StatusCode equals 200 (actual 404) | Test marked Failed with descriptive mismatch |
| **TC-TEST-03** | Runner | Assertion: ResponseTime < 500ms (actual 320ms) | Test marked Passed |
| **TC-TEST-04** | Runner | Assertion: ResponseTime < 200ms (actual 450ms) | Test marked Failed |
| **TC-TEST-05** | Runner | Assertion: JSON path `id == 1` | Correct evaluation against parsed JSON body |
| **TC-COLL-01** | Ownership | User A attempts to edit User B's collection | 403 Forbidden or 404 Not Found |
