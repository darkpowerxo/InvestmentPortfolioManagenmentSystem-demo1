# API Architecture Documentation

## REST API Design

The Investment Portfolio Management System exposes a comprehensive REST API following RESTful principles and industry best practices. The API provides complete CRUD operations for all domain entities plus specialized endpoints for reporting and analytics.

## API Architecture Overview

```mermaid
graph TB
    subgraph "Client Applications"
        WEB[Web Frontend]
        MOBILE[Mobile App]
        EXTERNAL[External Systems]
    end
    
    subgraph "API Gateway Layer"
        AUTH[Authentication Middleware]
        CORS[CORS Middleware]
        RATE[Rate Limiting]
        LOG[Request Logging]
    end
    
    subgraph "Controller Layer"
        PORT_CTRL[PortfoliosController]
        SEC_CTRL[SecuritiesController]
        TRANS_CTRL[TransactionsController]
        POS_CTRL[PositionsController]
        DASH_CTRL[DashboardController]
        REPORT_CTRL[ReportsController]
        AUTH_CTRL[AuthController]
    end
    
    subgraph "Application Services"
        SERVICES[Business Services]
        DTO[Data Transfer Objects]
        MAPPERS[Entity Mappers]
    end
    
    WEB --> AUTH
    MOBILE --> AUTH
    EXTERNAL --> AUTH
    
    AUTH --> CORS
    CORS --> RATE
    RATE --> LOG
    
    LOG --> PORT_CTRL
    LOG --> SEC_CTRL
    LOG --> TRANS_CTRL
    LOG --> POS_CTRL
    LOG --> DASH_CTRL
    LOG --> REPORT_CTRL
    LOG --> AUTH_CTRL
    
    PORT_CTRL --> SERVICES
    SEC_CTRL --> SERVICES
    TRANS_CTRL --> SERVICES
    POS_CTRL --> SERVICES
    DASH_CTRL --> SERVICES
    REPORT_CTRL --> SERVICES
    
    SERVICES --> DTO
    SERVICES --> MAPPERS
```

## Authentication Flow

```mermaid
sequenceDiagram
    participant Client
    participant AuthController
    participant AuthService
    participant UserRepository
    participant JwtService
    participant Database
    
    Client->>AuthController: POST /api/auth/login
    Note over Client,AuthController: { username, password }
    
    AuthController->>AuthService: ValidateUserAsync()
    AuthService->>UserRepository: GetByUsernameAsync()
    UserRepository->>Database: SELECT user
    Database-->>UserRepository: User data
    UserRepository-->>AuthService: User entity
    
    AuthService->>AuthService: VerifyPassword()
    AuthService->>JwtService: GenerateToken()
    JwtService-->>AuthService: JWT Token
    AuthService-->>AuthController: AuthResult
    
    AuthController-->>Client: 200 OK
    Note over AuthController,Client: { token, user, expires }
    
    Note over Client: Store token for subsequent requests
    
    Client->>AuthController: GET /api/portfolios
    Note over Client,AuthController: Authorization: Bearer {token}
    
    AuthController->>JwtService: ValidateToken()
    JwtService-->>AuthController: ClaimsPrincipal
    AuthController->>AuthController: Execute endpoint
    AuthController-->>Client: Portfolio data
```

## Controller Architecture Pattern

```mermaid
classDiagram
    class BaseController {
        <<abstract>>
        #ILogger logger
        #IMapper mapper
        +HandleErrorResponse(Exception ex) IActionResult
        +ValidateModel() bool
        +GetUserId() int
    }
    
    class PortfoliosController {
        -IPortfolioService portfolioService
        +GetAllAsync() Task~IActionResult~
        +GetByIdAsync(int id) Task~IActionResult~
        +CreateAsync(PortfolioCreateDto dto) Task~IActionResult~
        +UpdateAsync(int id, PortfolioUpdateDto dto) Task~IActionResult~
        +DeleteAsync(int id) Task~IActionResult~
        +GetPortfolioSummaryAsync(int id) Task~IActionResult~
    }
    
    class DashboardController {
        -IDashboardService dashboardService
        +GetDashboardSummaryAsync() Task~IActionResult~
        +GetPerformanceDashboardAsync(int portfolioId) Task~IActionResult~
        +GetRiskDashboardAsync(int portfolioId) Task~IActionResult~
        +GetAllocationDashboardAsync(int portfolioId) Task~IActionResult~
    }
    
    class ReportsController {
        -IPdfReportService pdfService
        -IExcelExportService excelService
        +GeneratePortfolioStatementAsync(int portfolioId) Task~IActionResult~
        +ExportPortfolioDataAsync(int portfolioId) Task~IActionResult~
    }
    
    BaseController <|-- PortfoliosController
    BaseController <|-- DashboardController
    BaseController <|-- ReportsController
```

## API Endpoint Mapping

### Core Entity Endpoints

```mermaid
flowchart LR
    subgraph "Portfolio Management"
        P1[GET /api/portfolios]
        P2[GET /api/portfolios/{id}]
        P3[POST /api/portfolios]
        P4[PUT /api/portfolios/{id}]
        P5[DELETE /api/portfolios/{id}]
    end
    
    subgraph "Position Management"
        POS1[GET /api/positions/portfolio/{portfolioId}]
        POS2[GET /api/positions/{id}]
        POS3[POST /api/positions]
        POS4[PUT /api/positions/{id}]
        POS5[DELETE /api/positions/{id}]
    end
    
    subgraph "Transaction Management"
        T1[GET /api/transactions/portfolio/{portfolioId}]
        T2[GET /api/transactions/{id}]
        T3[POST /api/transactions]
        T4[PUT /api/transactions/{id}]
        T5[DELETE /api/transactions/{id}]
    end
    
    subgraph "Security Management"
        S1[GET /api/securities]
        S2[GET /api/securities/{id}]
        S3[POST /api/securities]
        S4[PUT /api/securities/{id}]
        S5[GET /api/securities/search?query=]
    end
```

### Analytics & Reporting Endpoints

```mermaid
flowchart TD
    subgraph "Dashboard Analytics"
        D1[GET /api/dashboard/summary]
        D2[GET /api/dashboard/performance/{portfolioId}]
        D3[GET /api/dashboard/risk/{portfolioId}]
        D4[GET /api/dashboard/allocation/{portfolioId}]
        D5[GET /api/dashboard/transactions/{portfolioId}]
        D6[GET /api/dashboard/multi-portfolio]
    end
    
    subgraph "Report Generation"
        R1[GET /api/reports/pdf/portfolio-statement/{portfolioId}]
        R2[GET /api/reports/pdf/performance/{portfolioId}]
        R3[GET /api/reports/pdf/risk-analysis/{portfolioId}]
        R4[GET /api/reports/excel/portfolio/{portfolioId}]
        R5[GET /api/reports/excel/transactions/{portfolioId}]
    end
    
    subgraph "Performance Analytics"
        PERF1[GET /api/performance/{portfolioId}/metrics]
        PERF2[GET /api/performance/{portfolioId}/history]
        PERF3[GET /api/performance/comparison]
    end
```

## Request/Response Patterns

### Standard CRUD Pattern

```mermaid
sequenceDiagram
    participant Client
    participant Controller
    participant Service
    participant Repository
    participant Database
    
    Note over Client,Database: CREATE Operation
    Client->>Controller: POST /api/portfolios
    Controller->>Controller: Validate DTO
    Controller->>Service: CreateAsync(dto)
    Service->>Service: Map to Entity
    Service->>Repository: AddAsync(entity)
    Repository->>Database: INSERT
    Database-->>Repository: Generated ID
    Repository-->>Service: Entity with ID
    Service->>Service: Map to Response DTO
    Service-->>Controller: Response DTO
    Controller-->>Client: 201 Created + Location Header
    
    Note over Client,Database: READ Operation
    Client->>Controller: GET /api/portfolios/{id}
    Controller->>Service: GetByIdAsync(id)
    Service->>Repository: GetByIdAsync(id)
    Repository->>Database: SELECT
    Database-->>Repository: Entity data
    Repository-->>Service: Entity
    Service->>Service: Map to DTO
    Service-->>Controller: DTO
    Controller-->>Client: 200 OK + Data
    
    Note over Client,Database: UPDATE Operation
    Client->>Controller: PUT /api/portfolios/{id}
    Controller->>Controller: Validate DTO
    Controller->>Service: UpdateAsync(id, dto)
    Service->>Repository: GetByIdAsync(id)
    Service->>Service: Update Entity Properties
    Service->>Repository: UpdateAsync(entity)
    Repository->>Database: UPDATE
    Service-->>Controller: Updated DTO
    Controller-->>Client: 200 OK + Updated Data
```

### Error Handling Pattern

```mermaid
flowchart TD
    REQUEST[Incoming Request]
    VALIDATE{Validate Input}
    AUTHORIZE{Check Authorization}
    BUSINESS{Execute Business Logic}
    SUCCESS[Return Success Response]
    
    VALIDATION_ERROR[400 Bad Request]
    AUTH_ERROR[401 Unauthorized]
    FORBIDDEN_ERROR[403 Forbidden]
    NOT_FOUND_ERROR[404 Not Found]
    BUSINESS_ERROR[422 Unprocessable Entity]
    SERVER_ERROR[500 Internal Server Error]
    
    REQUEST --> VALIDATE
    VALIDATE -->|Invalid| VALIDATION_ERROR
    VALIDATE -->|Valid| AUTHORIZE
    AUTHORIZE -->|Not Authenticated| AUTH_ERROR
    AUTHORIZE -->|No Permission| FORBIDDEN_ERROR
    AUTHORIZE -->|Authorized| BUSINESS
    BUSINESS -->|Success| SUCCESS
    BUSINESS -->|Not Found| NOT_FOUND_ERROR
    BUSINESS -->|Business Rule Violation| BUSINESS_ERROR
    BUSINESS -->|System Error| SERVER_ERROR
    
    VALIDATION_ERROR --> RESPONSE[Error Response]
    AUTH_ERROR --> RESPONSE
    FORBIDDEN_ERROR --> RESPONSE
    NOT_FOUND_ERROR --> RESPONSE
    BUSINESS_ERROR --> RESPONSE
    SERVER_ERROR --> RESPONSE
    SUCCESS --> RESPONSE
```

## Data Transfer Objects (DTOs)

### Portfolio DTOs Hierarchy

```mermaid
classDiagram
    class PortfolioDto {
        +int Id
        +string Name
        +string Description
        +string Type
        +string RiskLevel
        +decimal CurrentValue
        +decimal CashBalance
        +DateTime InceptionDate
        +List~PositionSummaryDto~ Positions
        +PerformanceSummaryDto Performance
    }
    
    class PortfolioCreateDto {
        +string Name
        +string Description
        +string Type
        +string RiskLevel
        +string BaseCurrency
        +decimal InitialValue
        +decimal TargetEquityAllocation
        +decimal TargetBondAllocation
        +decimal TargetAlternativeAllocation
    }
    
    class PortfolioUpdateDto {
        +string Name
        +string Description
        +decimal TargetEquityAllocation
        +decimal TargetBondAllocation
        +decimal TargetAlternativeAllocation
        +decimal TargetCashAllocation
    }
    
    class PortfolioSummaryDto {
        +int Id
        +string Name
        +decimal CurrentValue
        +decimal TotalReturn
        +decimal DailyChange
        +int PositionCount
        +DateTime LastUpdated
    }
    
    PortfolioDto --> PositionSummaryDto
    PortfolioDto --> PerformanceSummaryDto
```

### Response Wrapper Pattern

```mermaid
classDiagram
    class ApiResponse~T~ {
        +T Data
        +bool Success
        +string Message
        +List~string~ Errors
        +Dictionary~string,object~ Metadata
        +DateTime Timestamp
    }
    
    class PagedResponse~T~ {
        +List~T~ Data
        +int PageNumber
        +int PageSize
        +int TotalRecords
        +int TotalPages
        +bool HasNextPage
        +bool HasPreviousPage
    }
    
    class ErrorResponse {
        +string Type
        +string Title
        +int Status
        +string Detail
        +string Instance
        +Dictionary~string,object~ Extensions
    }
    
    ApiResponse --> PagedResponse : inherits
    ApiResponse --> ErrorResponse : error cases
```

## API Versioning Strategy

```mermaid
graph TD
    subgraph "URL Path Versioning"
        V1[/api/v1/portfolios]
        V2[/api/v2/portfolios]
        V3[/api/v3/portfolios]
    end
    
    subgraph "Header Versioning"
        H1[X-API-Version: 1.0]
        H2[X-API-Version: 2.0]
        H3[Accept: application/vnd.api+json;version=1]
    end
    
    subgraph "Version Management"
        CURRENT[Current Version: v1]
        DEPRECATED[Deprecated Versions]
        SUNSET[Sunset Policy]
    end
    
    V1 --> CURRENT
    V2 --> DEPRECATED
    V3 --> SUNSET
```

## Content Negotiation

```mermaid
flowchart TD
    REQUEST[Client Request]
    ACCEPT{Check Accept Header}
    
    JSON[application/json]
    XML[application/xml]
    PDF[application/pdf]
    EXCEL[application/vnd.openxmlformats-officedocument.spreadsheetml.sheet]
    CSV[text/csv]
    
    JSON_RESPONSE[Return JSON Response]
    XML_RESPONSE[Return XML Response]
    PDF_RESPONSE[Generate PDF Report]
    EXCEL_RESPONSE[Generate Excel Export]
    CSV_RESPONSE[Generate CSV Export]
    
    REQUEST --> ACCEPT
    ACCEPT -->|application/json| JSON
    ACCEPT -->|application/xml| XML
    ACCEPT -->|application/pdf| PDF
    ACCEPT -->|Excel MIME| EXCEL
    ACCEPT -->|text/csv| CSV
    
    JSON --> JSON_RESPONSE
    XML --> XML_RESPONSE
    PDF --> PDF_RESPONSE
    EXCEL --> EXCEL_RESPONSE
    CSV --> CSV_RESPONSE
```

## API Security Implementation

### JWT Token Structure

```mermaid
graph LR
    subgraph "JWT Token Structure"
        HEADER[Header]
        PAYLOAD[Payload]
        SIGNATURE[Signature]
    end
    
    subgraph "Header Contents"
        ALG[Algorithm: HS256]
        TYP[Type: JWT]
    end
    
    subgraph "Payload Contents"
        SUB[Subject: User ID]
        ROLE[Role: Portfolio Manager]
        EXP[Expiration: Timestamp]
        IAT[Issued At: Timestamp]
        CUSTOM[Custom Claims]
    end
    
    HEADER --> ALG
    HEADER --> TYP
    PAYLOAD --> SUB
    PAYLOAD --> ROLE
    PAYLOAD --> EXP
    PAYLOAD --> IAT
    PAYLOAD --> CUSTOM
```

### Rate Limiting Strategy

```mermaid
graph TD
    subgraph "Rate Limiting Tiers"
        ANON[Anonymous: 100/hour]
        AUTH[Authenticated: 1000/hour]
        PREMIUM[Premium: 10000/hour]
        INTERNAL[Internal: Unlimited]
    end
    
    subgraph "Rate Limiting Algorithms"
        TOKEN_BUCKET[Token Bucket]
        SLIDING_WINDOW[Sliding Window]
        FIXED_WINDOW[Fixed Window]
    end
    
    subgraph "Response Headers"
        LIMIT[X-RateLimit-Limit]
        REMAINING[X-RateLimit-Remaining]
        RESET[X-RateLimit-Reset]
        RETRY[Retry-After]
    end
    
    ANON --> TOKEN_BUCKET
    AUTH --> SLIDING_WINDOW
    PREMIUM --> SLIDING_WINDOW
    INTERNAL --> FIXED_WINDOW
    
    TOKEN_BUCKET --> LIMIT
    SLIDING_WINDOW --> REMAINING
    FIXED_WINDOW --> RESET
```

## Performance Optimization

### Caching Strategy

```mermaid
flowchart TD
    REQUEST[API Request]
    CACHE_CHECK{Check Cache}
    CACHE_HIT[Return Cached Data]
    CACHE_MISS[Process Request]
    UPDATE_CACHE[Update Cache]
    RESPONSE[Return Response]
    
    subgraph "Cache Layers"
        MEMORY[In-Memory Cache]
        REDIS[Distributed Cache]
        DATABASE[Database Cache]
    end
    
    subgraph "Cache Keys"
        USER_KEY[user:{id}:portfolios]
        PORTFOLIO_KEY[portfolio:{id}:summary]
        MARKET_KEY[market:prices:{date}]
    end
    
    REQUEST --> CACHE_CHECK
    CACHE_CHECK -->|Hit| CACHE_HIT
    CACHE_CHECK -->|Miss| CACHE_MISS
    CACHE_MISS --> UPDATE_CACHE
    UPDATE_CACHE --> RESPONSE
    CACHE_HIT --> RESPONSE
    
    CACHE_CHECK --> MEMORY
    CACHE_CHECK --> REDIS
    CACHE_CHECK --> DATABASE
```

### Query Optimization

```mermaid
graph TD
    subgraph "Query Patterns"
        EAGER[Eager Loading]
        LAZY[Lazy Loading]
        EXPLICIT[Explicit Loading]
        PROJECTION[Projection Queries]
    end
    
    subgraph "Optimization Techniques"
        INDEX[Database Indexes]
        PAGINATION[Pagination]
        FILTERING[Query Filtering]
        SORTING[Efficient Sorting]
    end
    
    subgraph "Performance Monitoring"
        LOGGING[Query Logging]
        METRICS[Performance Metrics]
        PROFILING[Query Profiling]
    end
    
    EAGER --> INDEX
    LAZY --> PAGINATION
    EXPLICIT --> FILTERING
    PROJECTION --> SORTING
    
    INDEX --> LOGGING
    PAGINATION --> METRICS
    FILTERING --> PROFILING
```

## API Documentation Standards

### OpenAPI/Swagger Configuration

```mermaid
graph TB
    subgraph "Documentation Generation"
        CONTROLLERS[Controller Actions]
        ATTRIBUTES[XML Documentation]
        SCHEMAS[DTO Schemas]
        EXAMPLES[Request/Response Examples]
    end
    
    subgraph "Swagger UI Features"
        INTERACTIVE[Interactive Testing]
        AUTH_TEST[Authentication Testing]
        SCHEMA_BROWSER[Schema Browser]
        EXPORT[Export Options]
    end
    
    subgraph "Documentation Output"
        SWAGGER_JSON[swagger.json]
        SWAGGER_UI[Swagger UI]
        REDOC[ReDoc]
        POSTMAN[Postman Collection]
    end
    
    CONTROLLERS --> INTERACTIVE
    ATTRIBUTES --> AUTH_TEST
    SCHEMAS --> SCHEMA_BROWSER
    EXAMPLES --> EXPORT
    
    INTERACTIVE --> SWAGGER_JSON
    AUTH_TEST --> SWAGGER_UI
    SCHEMA_BROWSER --> REDOC
    EXPORT --> POSTMAN
```

## Comprehensive API Endpoints Reference

### Portfolio Management Endpoints

```http
GET    /api/portfolios                    # List all portfolios
GET    /api/portfolios/{id}               # Get portfolio details
POST   /api/portfolios                    # Create new portfolio
PUT    /api/portfolios/{id}               # Update portfolio
DELETE /api/portfolios/{id}               # Delete portfolio
```

**Portfolio Request/Response Example:**
```json
// POST /api/portfolios - Create Portfolio
{
  "name": "Conservative Growth Fund",
  "description": "Low-risk portfolio for conservative investors",
  "riskLevel": "Conservative",
  "inceptionDate": "2024-01-01",
  "targetEquityAllocation": 0.30,
  "targetBondAllocation": 0.60,
  "targetCashAllocation": 0.10,
  "targetAlternativeAllocation": 0.00
}

// Response 201 Created
{
  "id": 1,
  "name": "Conservative Growth Fund",
  "currentValue": 0.00,
  "totalReturn": 0.00,
  "totalReturnPercentage": 0.00,
  "isActive": true
}
```

### Position Management Endpoints

```http
GET    /api/positions/portfolio/{portfolioId}  # Get portfolio positions
POST   /api/positions                          # Create new position
PUT    /api/positions/{id}                     # Update position
DELETE /api/positions/{id}                     # Delete position
```

### Transaction Processing Endpoints

```http
GET    /api/transactions/portfolio/{portfolioId}  # Get portfolio transactions
POST   /api/transactions                          # Create new transaction
PUT    /api/transactions/{id}                     # Update transaction
DELETE /api/transactions/{id}                     # Delete transaction
```

**Transaction Request Example:**
```json
// POST /api/transactions - Buy Transaction
{
  "portfolioId": 1,
  "securityId": 100,
  "transactionType": "Buy",
  "quantity": 100,
  "price": 25.50,
  "transactionDate": "2024-01-15",
  "notes": "Initial purchase of AAPL"
}
```

### Reporting Endpoints

```http
GET /api/reports/pdf/portfolio-statement/{portfolioId}  # PDF portfolio statement
GET /api/reports/pdf/performance/{portfolioId}          # PDF performance report
GET /api/reports/excel/portfolio/{portfolioId}          # Excel portfolio export
GET /api/reports/excel/transactions/{portfolioId}       # Excel transaction export
```

### Dashboard Analytics Endpoints

```http
GET /api/dashboard/summary                    # Dashboard summary with key metrics
GET /api/dashboard/performance/{portfolioId}  # Performance dashboard
GET /api/dashboard/risk/{portfolioId}         # Risk analysis dashboard
GET /api/dashboard/allocation/{portfolioId}   # Allocation dashboard
GET /api/dashboard/transactions/{portfolioId} # Transaction analytics
GET /api/dashboard/multi-portfolio            # Multi-portfolio comparison
```

**Dashboard Response Example:**
```json
// GET /api/dashboard/summary
{
  "totalPortfolios": 5,
  "totalValue": 10500000.00,
  "totalReturn": 850000.00,
  "totalReturnPercentage": 8.82,
  "topPerformers": [
    {
      "portfolioName": "Growth Fund",
      "returnPercentage": 12.5,
      "value": 2500000.00
    }
  ],
  "recentTransactions": [
    {
      "portfolioName": "Conservative Fund",
      "securitySymbol": "MSFT",
      "transactionType": "Buy",
      "amount": 50000.00,
      "date": "2024-01-15"
    }
  ]
}
```

### Authentication Endpoints

```http
POST /api/auth/login     # User authentication
POST /api/auth/register  # User registration
POST /api/auth/refresh   # Token refresh
```

**Authentication Flow:**
```json
// POST /api/auth/login
{
  "username": "portfolio.manager@cdpq.com",
  "password": "SecurePassword123!"
}

// Response 200 OK
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": 1,
    "username": "portfolio.manager@cdpq.com",
    "firstName": "John",
    "lastName": "Doe",
    "role": "PortfolioManager"
  },
  "expires": "2024-01-16T10:00:00Z"
}
```

This API architecture provides:

- **RESTful Design**: Consistent resource-based URLs and HTTP methods
- **Comprehensive Coverage**: Full CRUD operations plus specialized endpoints
- **Security**: JWT authentication with role-based authorization
- **Performance**: Caching, pagination, and query optimization
- **Documentation**: Interactive Swagger/OpenAPI documentation
- **Error Handling**: Consistent error responses with proper HTTP status codes
- **Versioning**: Future-proof API versioning strategy
- **Content Negotiation**: Multiple response formats (JSON, PDF, Excel)