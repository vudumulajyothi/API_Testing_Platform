# TestPilot – REST API Documentation

Base URL: `/api/v1`

---

## 1. Authentication Endpoints

### 1.1 User Registration
- **Method**: `POST`
- **Route**: `/api/v1/auth/register`
- **Request Body**:
```json
{
  "name": "Jane Doe",
  "email": "jane@example.com",
  "password": "SecurePassword123!"
}
```
- **Response (201 Created)**:
```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Jane Doe",
  "email": "jane@example.com",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

### 1.2 User Login
- **Method**: `POST`
- **Route**: `/api/v1/auth/login`
- **Request Body**:
```json
{
  "email": "jane@example.com",
  "password": "SecurePassword123!"
}
```
- **Response (200 OK)**:
```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Jane Doe",
  "email": "jane@example.com",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

---

## 2. Request Execution

### 2.1 Execute API Request
- **Method**: `POST`
- **Route**: `/api/v1/execute`
- **Header**: `Authorization: Bearer <token>`
- **Request Body**:
```json
{
  "method": "GET",
  "url": "https://jsonplaceholder.typicode.com/posts/1",
  "headers": [
    { "key": "Accept", "value": "application/json" }
  ],
  "queryParams": [
    { "key": "debug", "value": "true" }
  ],
  "body": null
}
```
- **Response (200 OK)**:
```json
{
  "statusCode": 200,
  "statusText": "OK",
  "responseTimeMs": 142,
  "contentLength": 292,
  "headers": {
    "content-type": "application/json; charset=utf-8"
  },
  "body": {
    "userId": 1,
    "id": 1,
    "title": "sunt aut facere repellat provident occaecati excepturi optio reprehenderit",
    "body": "quia et suscipit..."
  }
}
```

---

## 3. Collections & Saved Requests

### 3.1 List Collections
- **Method**: `GET`
- **Route**: `/api/v1/collections`
- **Header**: `Authorization: Bearer <token>`

### 3.2 Create Collection
- **Method**: `POST`
- **Route**: `/api/v1/collections`
- **Request Body**:
```json
{
  "name": "E-Commerce API Tests",
  "description": "Integration assertions for product catalog"
}
```

### 3.3 Save API Request
- **Method**: `POST`
- **Route**: `/api/v1/requests`
- **Request Body**:
```json
{
  "collectionId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Get Product Details",
  "url": "https://api.example.com/products/123",
  "method": "GET",
  "body": null,
  "headers": [{ "headerKey": "Accept", "headerValue": "application/json", "isSensitive": false }],
  "queryParams": []
}
```

---

## 4. Automated Testing & Assertions

### 4.1 Add Test Case Assertion
- **Method**: `POST`
- **Route**: `/api/v1/requests/{id}/tests`
- **Request Body**:
```json
{
  "name": "Ensure 200 OK",
  "assertionType": "StatusCode",
  "expectedValue": "200"
}
```

### 4.2 Run Saved Tests for a Request
- **Method**: `POST`
- **Route**: `/api/v1/requests/{id}/run-tests`
- **Response (200 OK)**:
```json
{
  "testRunId": "b1e604f2-9599-4788-b2a8-1fa9900c5054",
  "requestId": "4d168e3f-6ce7-4589-a279-f1fb5cbf0a74",
  "statusCode": 200,
  "responseTimeMs": 150,
  "passed": true,
  "results": [
    {
      "testCaseId": "73c98894-96cf-448e-8a03-7cb7d55986ca",
      "name": "Ensure 200 OK",
      "passed": true,
      "actualValue": "200",
      "errorMessage": null
    }
  ]
}
```

---

## 5. History & Dashboard

### 5.1 Request History
- **Method**: `GET`
- **Route**: `/api/v1/history?page=1&pageSize=20`

### 5.2 Dashboard Summary
- **Method**: `GET`
- **Route**: `/api/v1/dashboard/summary`
- **Response (200 OK)**:
```json
{
  "totalRequests": 128,
  "totalCollections": 8,
  "totalTestRuns": 45,
  "passRatePercentage": 93.3,
  "recentRuns": []
}
```
