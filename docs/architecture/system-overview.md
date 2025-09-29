# Investment Portfolio Management System - Architecture Overview

## System Overview

The Investment Portfolio Management System is a comprehensive financial platform designed for the Caisse de dépôt et placement du Québec (CDPQ), Canada's second-largest pension fund. The system provides complete portfolio management capabilities with professional reporting, analytics, and multi-language support (English/French).

## System Architecture

The Investment Portfolio Management System follows **Clean Architecture** principles with clear separation of concerns and dependency inversion. The system is designed for institutional investment management with enterprise-grade requirements and is now **COMPLETE** with all major features implemented for production use.

### High-Level Architecture

```mermaid
graph TB
    subgraph "Presentation Layer"
        UI[React Frontend]
        API[REST API Controllers]
        AUTH[JWT Authentication]
    end
    
    subgraph "Application Layer"
        UC[Use Cases/Services]
        DTOs[Data Transfer Objects]
        INTER[Service Interfaces]
    end
    
    subgraph "Domain Layer"
        ENT[Entities]
        VO[Value Objects]
        ENUM[Enums]
        BR[Business Rules]
    end
    
    subgraph "Infrastructure Layer"
        REPO[Repositories]
        DB[Entity Framework]
        EXT[External Services]
        CACHE[Caching]
    end
    
    subgraph "External Dependencies"
        SQLDB[(SQL Server)]
        MARKET[Market Data APIs]
        PDF[iText7 PDF Generator]
        EXCEL[EPPlus Excel]
    end
    
    UI --> API
    API --> AUTH
    API --> UC
    UC --> INTER
    UC --> DTOs
    UC --> ENT
    ENT --> VO
    ENT --> ENUM
    UC --> REPO
    REPO --> DB
    DB --> SQLDB
    EXT --> MARKET
    UC --> PDF
    UC --> EXCEL
    REPO --> CACHE
```

### Clean Architecture Layers

#### 1. Domain Layer (Core)
The innermost layer containing business entities and rules.

```mermaid
classDiagram
    class Portfolio {
        +int Id
        +string Name
        +PortfolioType Type
        +RiskLevel RiskLevel
        +decimal CurrentValue
        +decimal CashBalance
        +DateTime InceptionDate
        +bool IsActive
        +ICollection~Position~ Positions
        +ICollection~Transaction~ Transactions
    }
    
    class Position {
        +int Id
        +int PortfolioId
        +int SecurityId
        +decimal Quantity
        +decimal AverageCost
        +decimal CurrentPrice
        +decimal MarketValue
        +decimal UnrealizedGainLoss
        +Security Security
    }
    
    class Security {
        +int Id
        +string Symbol
        +string Name
        +SecurityType Type
        +string Sector
        +string Region
        +decimal CurrentPrice
        +ICollection~MarketData~ MarketData
    }
    
    class Transaction {
        +int Id
        +int PortfolioId
        +int SecurityId
        +TransactionType Type
        +decimal Quantity
        +decimal Price
        +decimal NetAmount
        +DateTime TransactionDate
    }
    
    Portfolio ||--o{ Position : contains
    Portfolio ||--o{ Transaction : has
    Security ||--o{ Position : referenced_by
    Security ||--o{ Transaction : involved_in
```

#### 2. Application Layer (Business Logic)
Contains use cases, services, and application-specific business rules.

```mermaid
graph TD
    subgraph "Core Services"
        PS[PortfolioService]
        POS[PositionService]
        TS[TransactionService]
        PMS[PerformanceMetricsService]
    end
    
    subgraph "Financial Services"
        FCS[FinancialCalculationsService]
        MDS[MarketDataService]
        TPS[TransactionProcessingService]
        RMS[RiskManagementService]
    end
    
    subgraph "Reporting Services"
        PDFS[PdfReportService]
        EXS[ExcelExportService]
        DS[DashboardService]
    end
    
    subgraph "Infrastructure Interfaces"
        REPO[IRepository Interfaces]
        UOW[IUnitOfWork]
        EXT[External Service Interfaces]
    end
    
    PS --> REPO
    POS --> REPO
    TS --> TPS
    TPS --> REPO
    FCS --> MDS
    PDFS --> PS
    PDFS --> POS
    EXS --> PS
    DS --> PS
    DS --> PMS
    
    PS --> UOW
    TS --> UOW
```

#### 3. Infrastructure Layer (Data Access)
Implements interfaces defined in the application layer.

```mermaid
graph LR
    subgraph "Repository Pattern"
        GR[Generic Repository]
        PR[Portfolio Repository]
        SR[Security Repository]
        TR[Transaction Repository]
        PMR[Performance Repository]
    end
    
    subgraph "Data Context"
        EF[Entity Framework Context]
        CONF[Entity Configurations]
        SEED[Seed Data Service]
    end
    
    subgraph "External Services"
        MARKET[Market Data Service]
        CACHE[Caching Service]
        LOG[Logging Service]
    end
    
    GR --> EF
    PR --> GR
    SR --> GR
    TR --> GR
    PMR --> GR
    EF --> CONF
    EF --> SEED
```

#### 4. API Layer (Presentation)
REST API controllers and authentication.

```mermaid
sequenceDiagram
    participant Client
    participant AuthController
    participant PortfolioController
    participant PortfolioService
    participant Repository
    participant Database
    
    Client->>AuthController: POST /api/auth/login
    AuthController-->>Client: JWT Token
    
    Client->>PortfolioController: GET /api/portfolios (with JWT)
    PortfolioController->>PortfolioService: GetAllAsync()
    PortfolioService->>Repository: GetAllAsync()
    Repository->>Database: SELECT * FROM Portfolios
    Database-->>Repository: Portfolio Data
    Repository-->>PortfolioService: Portfolio Entities
    PortfolioService-->>PortfolioController: Portfolio DTOs
    PortfolioController-->>Client: JSON Response
```

## Technology Stack

### Backend Technologies

#### Core Framework
- **.NET 9.0**: Modern C# development platform with latest performance improvements
- **Entity Framework Core**: Advanced ORM for database operations and migrations
- **SQL Server**: Production-grade relational database with enterprise features

#### Authentication & Security
- **JWT Authentication**: Secure token-based authentication with refresh tokens
- **Role-Based Authorization**: Fine-grained access control for different user types
- **HTTPS Enforcement**: Encrypted communication for all API endpoints

#### Reporting & Export
- **iText7**: Professional PDF report generation with CDPQ corporate branding
- **EPPlus**: Advanced Excel export with charts, formatting, and multi-worksheet support
- **Financial Calculations**: Complex financial metrics and performance analytics

#### Testing & Quality
- **xUnit**: Comprehensive unit testing framework
- **83 Passing Tests**: Complete test coverage ensuring system reliability
- **Integration Testing**: End-to-end workflow validation

#### Localization
- **Multi-Language Support**: English/French bilingual interface
- **Cultural Formatting**: Currency, dates, and numbers in local formats
- **Dynamic Language Switching**: Runtime language changes without restart

### Technology Implementation Diagram

```mermaid
graph TB
    subgraph "Frontend Technologies"
        REACT[React 18]
        TS[TypeScript]
        MUI[Material-UI]
        CHARTS[Chart.js]
    end
    
    subgraph "Backend Technologies"
        DOTNET[.NET 9.0]
        EF[Entity Framework Core]
        JWT_AUTH[JWT Authentication]
        ITEXT[iText7 PDF]
        EPPLUS[EPPlus Excel]
    end
    
    subgraph "Database & Storage"
        SQLSERVER[SQL Server]
        AZURE_SQL[Azure SQL Database]
        BLOB[Azure Blob Storage]
    end
    
    subgraph "Infrastructure"
        AZURE_APP[Azure App Service]
        KEY_VAULT[Azure Key Vault]
        APP_INSIGHTS[Application Insights]
    end
    
    REACT --> DOTNET
    TS --> REACT
    MUI --> REACT
    CHARTS --> REACT
    
    DOTNET --> EF
    DOTNET --> JWT_AUTH
    DOTNET --> ITEXT
    DOTNET --> EPPLUS
    
    EF --> SQLSERVER
    SQLSERVER --> AZURE_SQL
    ITEXT --> BLOB
    
    DOTNET --> AZURE_APP
    JWT_AUTH --> KEY_VAULT
    AZURE_APP --> APP_INSIGHTS
```

### System Completion Status

The Investment Portfolio Management System is now **COMPLETE** with all major features implemented:

✅ **Portfolio Management**: Full CRUD operations with risk profiling  
✅ **Position Tracking**: Real-time holdings with market valuations  
✅ **Transaction Processing**: Complete transaction lifecycle management  
✅ **Performance Analytics**: Comprehensive performance metrics and calculations  
✅ **Professional Reporting**: PDF reports with CDPQ branding  
✅ **Excel Export**: Multi-worksheet exports with charts and formatting  
✅ **Financial Dashboard**: Interactive analytics with multiple dashboard views  
✅ **Multi-Language Support**: English and French localization  
✅ **Authentication & Security**: JWT-based secure authentication  
✅ **Clean Architecture**: Well-structured, maintainable codebase  
✅ **Comprehensive Testing**: 83 passing tests ensuring system reliability  

The system is **production-ready** and provides a complete investment portfolio management solution suitable for institutional investors like CDPQ.

## Data Flow Architecture

### Transaction Processing Flow

```mermaid
flowchart TD
    START([Transaction Request])
    VALIDATE{Validate Transaction}
    PROCESS[Process Transaction]
    UPDATE_POS[Update Position]
    UPDATE_CASH[Update Cash Balance]
    CALC_PERF[Calculate Performance]
    SAVE[Save to Database]
    NOTIFY[Send Notifications]
    END([Transaction Complete])
    ERROR[Return Error]
    
    START --> VALIDATE
    VALIDATE -->|Valid| PROCESS
    VALIDATE -->|Invalid| ERROR
    PROCESS --> UPDATE_POS
    UPDATE_POS --> UPDATE_CASH
    UPDATE_CASH --> CALC_PERF
    CALC_PERF --> SAVE
    SAVE --> NOTIFY
    NOTIFY --> END
```

### Reporting Data Flow

```mermaid
flowchart LR
    subgraph "Data Sources"
        PORT[Portfolios]
        POS[Positions]
        TRANS[Transactions]
        PERF[Performance Metrics]
        MARKET[Market Data]
    end
    
    subgraph "Processing Layer"
        AGG[Data Aggregation]
        CALC[Calculations]
        FORMAT[Formatting]
    end
    
    subgraph "Output Formats"
        PDF[PDF Reports]
        EXCEL[Excel Exports]
        DASH[Dashboard JSON]
    end
    
    PORT --> AGG
    POS --> AGG
    TRANS --> AGG
    PERF --> AGG
    MARKET --> AGG
    
    AGG --> CALC
    CALC --> FORMAT
    
    FORMAT --> PDF
    FORMAT --> EXCEL
    FORMAT --> DASH
```

## Dependency Management

### Dependency Inversion Principle

```mermaid
graph TB
    subgraph "High Level Modules"
        CONTROLLER[Controllers]
        SERVICE[Application Services]
    end
    
    subgraph "Abstractions"
        ISERVICE[IService Interfaces]
        IREPO[IRepository Interfaces]
    end
    
    subgraph "Low Level Modules"
        REPO[Repository Implementations]
        EF[Entity Framework]
        DB[(Database)]
    end
    
    CONTROLLER --> ISERVICE
    SERVICE --> IREPO
    ISERVICE -.-> SERVICE
    IREPO -.-> REPO
    REPO --> EF
    EF --> DB
    
    style ISERVICE fill:#e1f5fe
    style IREPO fill:#e1f5fe
```

### Service Registration (Dependency Injection)

```mermaid
graph TD
    subgraph "Program.cs Configuration"
        REG[Service Registration]
        CONFIG[Configuration Binding]
        MIDDLEWARE[Middleware Pipeline]
    end
    
    subgraph "Service Lifetimes"
        SINGLETON[Singleton Services]
        SCOPED[Scoped Services]
        TRANSIENT[Transient Services]
    end
    
    subgraph "External Dependencies"
        DB_CONN[Database Connection]
        JWT_CONFIG[JWT Configuration]
        CORS[CORS Policies]
    end
    
    REG --> SINGLETON
    REG --> SCOPED
    REG --> TRANSIENT
    
    CONFIG --> DB_CONN
    CONFIG --> JWT_CONFIG
    CONFIG --> CORS
    
    SINGLETON -.->|"DbContext, Caching"| SCOPED
    SCOPED -.->|"Business Services"| TRANSIENT
    TRANSIENT -.->|"Utilities, Helpers"| REG
```

## Security Architecture

### Authentication & Authorization Flow

```mermaid
sequenceDiagram
    participant User
    participant Frontend
    participant API Gateway
    participant Auth Service
    participant Business Service
    participant Database
    
    User->>Frontend: Login Request
    Frontend->>API Gateway: POST /api/auth/login
    API Gateway->>Auth Service: Validate Credentials
    Auth Service->>Database: Query User
    Database-->>Auth Service: User Data
    Auth Service-->>API Gateway: JWT Token
    API Gateway-->>Frontend: JWT Token + User Info
    Frontend-->>User: Login Success
    
    Note over Frontend: Store JWT in memory/storage
    
    User->>Frontend: Access Protected Resource
    Frontend->>API Gateway: Request with JWT Header
    API Gateway->>Auth Service: Validate JWT
    Auth Service-->>API Gateway: Token Valid
    API Gateway->>Business Service: Execute Business Logic
    Business Service->>Database: Data Operation
    Database-->>Business Service: Data Result
    Business Service-->>API Gateway: Business Result
    API Gateway-->>Frontend: Response Data
    Frontend-->>User: Display Result
```

## Performance Considerations

### Caching Strategy

```mermaid
graph TD
    subgraph "Cache Layers"
        L1[L1: In-Memory Cache]
        L2[L2: Distributed Cache]
        L3[L3: Database Cache]
    end
    
    subgraph "Cache Types"
        SEC[Security Data]
        MARKET[Market Data]
        PERF[Performance Metrics]
        USER[User Sessions]
    end
    
    subgraph "Cache Invalidation"
        TIME[Time-based Expiry]
        EVENT[Event-based Invalidation]
        MANUAL[Manual Refresh]
    end
    
    SEC --> L1
    MARKET --> L2
    PERF --> L2
    USER --> L1
    
    L1 --> TIME
    L2 --> EVENT
    L3 --> MANUAL
```

### Database Optimization

```mermaid
erDiagram
    Portfolio ||--o{ Position : "has many"
    Portfolio ||--o{ Transaction : "contains"
    Portfolio ||--o{ PerformanceMetric : "tracks"
    Security ||--o{ Position : "referenced in"
    Security ||--o{ Transaction : "involved in"
    Security ||--o{ MarketData : "has pricing"
    User ||--o{ Portfolio : "manages"
    
    Portfolio {
        int Id PK "Clustered Index"
        int ManagerId FK "Index"
        bool IsActive "Index"
        datetime CreatedAt "Index"
    }
    
    Position {
        int Id PK "Clustered Index"
        int PortfolioId FK "Index"
        int SecurityId FK "Index"
        decimal Quantity "Check > 0"
    }
    
    Transaction {
        int Id PK "Clustered Index"
        int PortfolioId FK "Index"
        datetime TransactionDate "Index"
        int TransactionType "Index"
    }
```

## Error Handling & Resilience

### Exception Handling Strategy

```mermaid
flowchart TD
    REQUEST[HTTP Request]
    VALIDATE{Input Validation}
    BUSINESS{Business Logic}
    DATABASE{Database Operation}
    RESPONSE[HTTP Response]
    
    GLOBAL_HANDLER[Global Exception Handler]
    BUSINESS_EX[Business Exception]
    VALIDATION_EX[Validation Exception]
    DB_EX[Database Exception]
    SYSTEM_EX[System Exception]
    
    REQUEST --> VALIDATE
    VALIDATE -->|Invalid| VALIDATION_EX
    VALIDATE -->|Valid| BUSINESS
    BUSINESS -->|Business Rule Violated| BUSINESS_EX
    BUSINESS -->|Valid| DATABASE
    DATABASE -->|DB Error| DB_EX
    DATABASE -->|System Error| SYSTEM_EX
    DATABASE -->|Success| RESPONSE
    
    VALIDATION_EX --> GLOBAL_HANDLER
    BUSINESS_EX --> GLOBAL_HANDLER
    DB_EX --> GLOBAL_HANDLER
    SYSTEM_EX --> GLOBAL_HANDLER
    
    GLOBAL_HANDLER --> RESPONSE
```

This architecture ensures:

- **Separation of Concerns**: Each layer has distinct responsibilities
- **Testability**: Dependencies are injected and can be mocked
- **Maintainability**: Business logic is isolated from infrastructure
- **Scalability**: Stateless design with caching strategies
- **Security**: JWT authentication with proper validation
- **Performance**: Optimized database queries and caching
- **Reliability**: Comprehensive error handling and logging