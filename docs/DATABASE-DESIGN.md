# TestPilot – Database Design & Schema

## 1. Overview
TestPilot uses SQL Server (with support for SQLite / LocalDB in local test environments) via Entity Framework Core. All relational models enforce foreign key constraints, indexes on query paths, and auditing timestamps (`CreatedAt`, `UpdatedAt`).

---

## 2. Entity-Relationship Diagram (ERD)

```
+--------------------+           +-----------------------+
|       USERS        | 1       * |      COLLECTIONS      |
+--------------------+-----------+-----------------------+
| PK UserId          |           | PK CollectionId       |
|    Name            |           | FK UserId             |
|    Email (Unique)  |           |    Name               |
|    PasswordHash    |           |    Description        |
|    CreatedAt       |           |    CreatedAt          |
+---------+----------+           +-----------+-----------+
          | 1                                | 1
          |                                  |
          | *                                | *
+---------v----------+           +-----------v-----------+
|  REQUEST_HISTORY   |           |     API_REQUESTS      |
+--------------------+           +-----------------------+
| PK HistoryId       |           | PK RequestId          |
| FK UserId          |           | FK CollectionId       |
| FK RequestId (opt) |           |    Name               |
|    Method          |           |    Url                |
|    Url             |           |    Method             |
|    StatusCode      |           |    Body               |
|    ResponseTimeMs  |           |    CreatedAt          |
|    ResponseBody    |           +-----+-----+-----+-----+
|    ExecutedAt      |                 |     |     |
+--------------------+                 |     |     |
                                       |     |     |
            +--------------------------+     |     +--------------------------+
            | 1                              | 1                              | 1
            | *                              | *                              | *
+-----------v-----------+        +-----------v-----------+        +-----------v-----------+
|    REQUEST_HEADERS    |        |   QUERY_PARAMETERS    |        |      TEST_CASES       |
+-----------------------+        +-----------------------+        +-----------------------+
| PK HeaderId           |        | PK ParameterId        |        | PK TestCaseId         |
| FK RequestId          |        | FK RequestId          |        | FK RequestId          |
|    HeaderKey          |        |    ParamKey           |        |    Name               |
|    HeaderValue        |        |    ParamValue         |        |    AssertionType      |
|    IsSensitive        |        +-----------------------+        |    ExpectedValue      |
+-----------------------+                                         |    JsonPath           |
                                                                  +-----------+-----------+
                                                                              |
                                         +------------------------------------+
                                         |
                                         | evaluated_in
                                         v
+-----------------------+ 1            * +-----------------------+
|       TEST_RUNS       |----------------+     TEST_RESULTS      |
+-----------------------+                +-----------------------+
| PK TestRunId          |                | PK ResultId           |
| FK RequestId          |                | FK TestRunId          |
|    StatusCode         |                | FK TestCaseId         |
|    ResponseTimeMs     |                |    Passed             |
|    Passed             |                |    ActualValue        |
|    ExecutedAt         |                |    ErrorMessage       |
+-----------------------+                +-----------------------+
```

---

## 3. Detailed Table Specifications

### 3.1 `Users`
| Column | Type | Constraints | Description |
|---|---|---|---|
| `UserId` | `uniqueidentifier` | PK, Default `NEWID()` | Unique identifier |
| `Name` | `nvarchar(150)` | Not Null | User's full name |
| `Email` | `nvarchar(255)` | Not Null, Unique Index | Unique email address |
| `PasswordHash` | `nvarchar(max)` | Not Null | Hashed password |
| `CreatedAt` | `datetime2` | Not Null, Default `GETUTCDATE()` | Creation timestamp |

### 3.2 `Collections`
| Column | Type | Constraints | Description |
|---|---|---|---|
| `CollectionId` | `uniqueidentifier` | PK | Collection ID |
| `UserId` | `uniqueidentifier` | FK -> Users.UserId | Owner ID |
| `Name` | `nvarchar(150)` | Not Null | Collection name |
| `Description` | `nvarchar(500)` | Nullable | Optional description |
| `CreatedAt` | `datetime2` | Not Null | Creation timestamp |

### 3.3 `ApiRequests`
| Column | Type | Constraints | Description |
|---|---|---|---|
| `RequestId` | `uniqueidentifier` | PK | Request ID |
| `CollectionId` | `uniqueidentifier` | FK -> Collections.CollectionId | Parent collection |
| `Name` | `nvarchar(150)` | Not Null | Request name |
| `Url` | `nvarchar(2048)` | Not Null | Target endpoint |
| `Method` | `nvarchar(10)` | Not Null | GET, POST, PUT, DELETE, etc. |
| `Body` | `nvarchar(max)` | Nullable | Request body (JSON/text) |
| `CreatedAt` | `datetime2` | Not Null | Creation timestamp |

### 3.4 `RequestHeaders`
| Column | Type | Constraints | Description |
|---|---|---|---|
| `HeaderId` | `uniqueidentifier` | PK | Header ID |
| `RequestId` | `uniqueidentifier` | FK -> ApiRequests.RequestId | Request reference |
| `HeaderKey` | `nvarchar(150)` | Not Null | Header name |
| `HeaderValue` | `nvarchar(1000)` | Not Null | Header value |
| `IsSensitive` | `bit` | Not Null, Default 0 | Flag for secret masking |

### 3.5 `QueryParameters`
| Column | Type | Constraints | Description |
|---|---|---|---|
| `ParameterId` | `uniqueidentifier` | PK | Parameter ID |
| `RequestId` | `uniqueidentifier` | FK -> ApiRequests.RequestId | Request reference |
| `ParamKey` | `nvarchar(150)` | Not Null | Query parameter key |
| `ParamValue` | `nvarchar(1000)` | Not Null | Query parameter value |

### 3.6 `TestCases`
| Column | Type | Constraints | Description |
|---|---|---|---|
| `TestCaseId` | `uniqueidentifier` | PK | Test case ID |
| `RequestId` | `uniqueidentifier` | FK -> ApiRequests.RequestId | Target request |
| `Name` | `nvarchar(200)` | Not Null | Name of the assertion |
| `AssertionType` | `nvarchar(50)` | Not Null | StatusCode, ResponseTime, JsonField |
| `ExpectedValue` | `nvarchar(500)` | Not Null | Expected outcome value |
| `JsonPath` | `nvarchar(200)` | Nullable | Field path for JSON assertions |

### 3.7 `TestRuns`
| Column | Type | Constraints | Description |
|---|---|---|---|
| `TestRunId` | `uniqueidentifier` | PK | Test run ID |
| `RequestId` | `uniqueidentifier` | FK -> ApiRequests.RequestId | Tested request |
| `StatusCode` | `int` | Not Null | HTTP status received |
| `ResponseTimeMs`| `bigint` | Not Null | Latency in ms |
| `Passed` | `bit` | Not Null | Did all assertions pass? |
| `ExecutedAt` | `datetime2` | Not Null | Timestamp of execution |

### 3.8 `TestResults`
| Column | Type | Constraints | Description |
|---|---|---|---|
| `ResultId` | `uniqueidentifier` | PK | Result ID |
| `TestRunId` | `uniqueidentifier` | FK -> TestRuns.TestRunId | Associated test run |
| `TestCaseId` | `uniqueidentifier` | FK -> TestCases.TestCaseId | Associated test case |
| `Passed` | `bit` | Not Null | Individual assertion passed? |
| `ActualValue` | `nvarchar(500)` | Nullable | Actual value captured |
| `ErrorMessage` | `nvarchar(1000)`| Nullable | Reason for failure |

### 3.9 `RequestHistory`
| Column | Type | Constraints | Description |
|---|---|---|---|
| `HistoryId` | `uniqueidentifier` | PK | History record ID |
| `UserId` | `uniqueidentifier` | FK -> Users.UserId | User who triggered execution |
| `RequestId` | `uniqueidentifier` | Nullable, FK -> ApiRequests | Associated request if saved |
| `Method` | `nvarchar(10)` | Not Null | HTTP method executed |
| `Url` | `nvarchar(2048)` | Not Null | Executed URL |
| `StatusCode` | `int` | Not Null | Status code returned |
| `ResponseTimeMs`| `bigint` | Not Null | Latency |
| `ResponseBody` | `nvarchar(max)` | Nullable | Truncated response body |
| `ExecutedAt` | `datetime2` | Not Null | Timestamp |

### 3.10 `AiSuggestions`
| Column | Type | Constraints | Description |
|---|---|---|---|
| `SuggestionId` | `uniqueidentifier` | PK | Suggestion ID |
| `RequestId` | `uniqueidentifier` | FK -> ApiRequests.RequestId | Request referenced |
| `SuggestionText`| `nvarchar(max)`| Not Null | Generated assertion JSON |
| `CreatedAt` | `datetime2` | Not Null | Timestamp |
