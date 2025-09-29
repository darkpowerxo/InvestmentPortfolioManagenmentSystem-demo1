# Domain Model Architecture

## Overview

The domain model represents the core business entities and their relationships in the Investment Portfolio Management System. It follows Domain-Driven Design (DDD) principles with rich domain entities, value objects, and business rules.

## Entity Relationship Diagram

```mermaid
erDiagram
    User ||--o{ Portfolio : "manages"
    Portfolio ||--o{ Position : "contains"
    Portfolio ||--o{ Transaction : "records"
    Portfolio ||--o{ PerformanceMetric : "tracks"
    Portfolio ||--o{ TradeOrder : "places"
    Security ||--o{ Position : "held as"
    Security ||--o{ Transaction : "involves"
    Security ||--o{ MarketData : "priced by"
    Security ||--o{ TradeOrder : "orders for"
    
    User {
        int Id PK
        string Username
        string Email
        string PasswordHash
        UserRole Role
        datetime CreatedAt
        bool IsActive
    }
    
    Portfolio {
        int Id PK
        string Name
        string Description
        PortfolioType Type
        RiskLevel RiskLevel
        string BaseCurrency
        int ManagerId FK
        decimal InitialValue
        decimal CurrentValue
        decimal CashBalance
        datetime InceptionDate
        decimal TargetEquityAllocation
        decimal TargetBondAllocation
        decimal TargetAlternativeAllocation
        decimal TargetCashAllocation
        bool IsActive
    }
    
    Position {
        int Id PK
        int PortfolioId FK
        int SecurityId FK
        decimal Quantity
        decimal AverageCost
        decimal CurrentPrice
        decimal MarketValue
        decimal UnrealizedGainLoss
        decimal UnrealizedGainLossPercent
        datetime FirstPurchaseDate
        datetime LastTransactionDate
        decimal AllocationPercentage
    }
    
    Security {
        int Id PK
        string Symbol
        string Name
        SecurityType Type
        string Description
        string Sector
        string Industry
        string Region
        string Currency
        decimal CurrentPrice
        decimal Beta
        decimal DividendYield
        decimal MarketCap
        bool IsActive
    }
    
    Transaction {
        int Id PK
        int PortfolioId FK
        int SecurityId FK
        TransactionType Type
        decimal Quantity
        decimal Price
        decimal Commission
        decimal NetAmount
        datetime TransactionDate
        string Reference
        string Notes
    }
    
    PerformanceMetric {
        int Id PK
        int PortfolioId FK
        datetime CalculationDate
        decimal DailyReturn
        decimal WeeklyReturn
        decimal MonthlyReturn
        decimal YearToDateReturn
        decimal OneYearReturn
        decimal Volatility
        decimal SharpeRatio
        decimal Beta
        decimal Alpha
        decimal MaxDrawdown
        decimal VaR95
        decimal VaR99
    }
    
    MarketData {
        int Id PK
        int SecurityId FK
        datetime Date
        decimal OpenPrice
        decimal HighPrice
        decimal LowPrice
        decimal ClosePrice
        decimal AdjustedClose
        long Volume
        datetime CreatedAt
    }
    
    TradeOrder {
        int Id PK
        int PortfolioId FK
        int SecurityId FK
        OrderType Type
        OrderSide Side
        decimal Quantity
        decimal Price
        OrderStatus Status
        datetime OrderDate
        datetime ExpiryDate
        string OrderReference
    }
```

## Domain Entities Deep Dive

### Portfolio Entity

The Portfolio is the central aggregate root representing an investment portfolio.

```mermaid
classDiagram
    class Portfolio {
        <<AggregateRoot>>
        +int Id
        +string Name
        +string Description
        +PortfolioType Type
        +RiskLevel RiskLevel
        +string BaseCurrency
        +int ManagerId
        +decimal InitialValue
        +decimal CurrentValue
        +decimal CashBalance
        +DateTime InceptionDate
        +decimal TargetEquityAllocation
        +decimal TargetBondAllocation
        +decimal TargetAlternativeAllocation
        +decimal TargetCashAllocation
        +bool IsActive
        +DateTime CreatedAt
        +DateTime UpdatedAt
        
        +CalculateTotalValue() decimal
        +GetAllocationDeviation() AllocationDeviation
        +ValidateTargetAllocations() bool
        +CanPlaceOrder(decimal amount) bool
    }
    
    class PortfolioType {
        <<Enumeration>>
        PublicEquity
        FixedIncome
        Infrastructure
        RealEstate
        PrivateEquity
        Alternative
        Custom
    }
    
    class RiskLevel {
        <<Enumeration>>
        Conservative
        ModerateConservative
        Moderate
        ModerateAggressive
        Aggressive
    }
    
    Portfolio --> PortfolioType
    Portfolio --> RiskLevel
```

**Business Rules:**
- Total target allocations must equal 100%
- Current value cannot be negative
- Cash balance can be negative (leverage scenarios)
- Portfolio must have a valid manager

### Position Entity

Represents a holding of a specific security within a portfolio.

```mermaid
classDiagram
    class Position {
        <<Entity>>
        +int Id
        +int PortfolioId
        +int SecurityId
        +decimal Quantity
        +decimal AverageCost
        +decimal CurrentPrice
        +decimal MarketValue
        +decimal UnrealizedGainLoss
        +decimal UnrealizedGainLossPercent
        +DateTime FirstPurchaseDate
        +DateTime LastTransactionDate
        +decimal AllocationPercentage
        
        +CalculateMarketValue() decimal
        +CalculateUnrealizedGainLoss() decimal
        +UpdateAverageCost(decimal newQuantity, decimal newPrice) void
        +CanSell(decimal quantity) bool
    }
    
    class PositionCalculations {
        <<ValueObject>>
        +decimal CostBasis
        +decimal TotalReturn
        +decimal AnnualizedReturn
        +decimal DaysHeld
    }
    
    Position --> PositionCalculations : calculates
```

**Business Rules:**
- Quantity must be positive for long positions
- Average cost must be positive
- Market value is calculated as Quantity × Current Price
- Unrealized gain/loss = Market Value - (Quantity × Average Cost)

### Transaction Entity

Records all portfolio transactions with full audit trail.

```mermaid
classDiagram
    class Transaction {
        <<Entity>>
        +int Id
        +int PortfolioId
        +int SecurityId
        +TransactionType Type
        +decimal Quantity
        +decimal Price
        +decimal Commission
        +decimal NetAmount
        +DateTime TransactionDate
        +string Reference
        +string Notes
        
        +CalculateNetAmount() decimal
        +ValidateTransaction() ValidationResult
        +GetImpactOnCash() decimal
    }
    
    class TransactionType {
        <<Enumeration>>
        Buy
        Sell
        Dividend
        Interest
        Split
        Merger
        SpinOff
        CashDeposit
        CashWithdrawal
        Fee
        Transfer
    }
    
    Transaction --> TransactionType
```

**Business Rules:**
- Buy transactions decrease cash balance
- Sell transactions increase cash balance
- Dividend and interest transactions increase cash balance
- Commission is always positive
- Net amount calculation varies by transaction type

### Security Entity

Represents tradeable financial instruments.

```mermaid
classDiagram
    class Security {
        <<Entity>>
        +int Id
        +string Symbol
        +string Name
        +SecurityType Type
        +string Description
        +string Sector
        +string Industry
        +string Region
        +string Currency
        +decimal CurrentPrice
        +decimal Beta
        +decimal DividendYield
        +decimal MarketCap
        +bool IsActive
        
        +GetLatestPrice() decimal
        +CalculateBeta() decimal
        +GetPriceHistory(DateTime from, DateTime to) List~MarketData~
    }
    
    class SecurityType {
        <<Enumeration>>
        Stock
        Bond
        ETF
        MutualFund
        Option
        Future
        Currency
        Commodity
        REIT
        PrivateEquity
        HedgeFund
        Cash
    }
    
    Security --> SecurityType
```

## Domain Services

### Portfolio Aggregation Service

```mermaid
graph TD
    subgraph "Portfolio Calculations"
        TOTAL_VALUE[Calculate Total Value]
        ALLOCATION[Calculate Actual Allocations]
        PERFORMANCE[Calculate Performance]
        RISK[Calculate Risk Metrics]
    end
    
    subgraph "Data Sources"
        POSITIONS[Positions]
        TRANSACTIONS[Transactions]
        MARKET_DATA[Market Data]
        BENCHMARKS[Benchmark Data]
    end
    
    subgraph "Outputs"
        PORTFOLIO_VALUE[Portfolio Valuation]
        ALLOCATION_REPORT[Allocation Analysis]
        PERFORMANCE_METRICS[Performance Report]
        RISK_ASSESSMENT[Risk Analysis]
    end
    
    POSITIONS --> TOTAL_VALUE
    POSITIONS --> ALLOCATION
    TRANSACTIONS --> PERFORMANCE
    MARKET_DATA --> TOTAL_VALUE
    MARKET_DATA --> PERFORMANCE
    BENCHMARKS --> PERFORMANCE
    
    TOTAL_VALUE --> PORTFOLIO_VALUE
    ALLOCATION --> ALLOCATION_REPORT
    PERFORMANCE --> PERFORMANCE_METRICS
    RISK --> RISK_ASSESSMENT
```

### Transaction Processing Domain Service

```mermaid
stateDiagram-v2
    [*] --> Pending : Create Order
    Pending --> Validated : Validate Order
    Validated --> Executed : Execute Trade
    Executed --> Settled : Settlement
    Settled --> [*] : Complete
    
    Pending --> Cancelled : Cancel Order
    Validated --> Cancelled : Cancel Order
    Cancelled --> [*]
    
    Validated --> Failed : Execution Failed
    Failed --> [*]
    
    note right of Validated
        Business Rules:
        - Sufficient cash for buys
        - Sufficient quantity for sells
        - Market hours validation
        - Risk limits check
    end note
```

## Value Objects

### Money Value Object

```mermaid
classDiagram
    class Money {
        <<ValueObject>>
        +decimal Amount
        +string Currency
        
        +Add(Money other) Money
        +Subtract(Money other) Money
        +Multiply(decimal factor) Money
        +ConvertTo(string targetCurrency) Money
        +Equals(Money other) bool
        +ToString() string
    }
    
    class CurrencyRate {
        <<ValueObject>>
        +string FromCurrency
        +string ToCurrency
        +decimal Rate
        +DateTime AsOfDate
    }
    
    Money --> CurrencyRate : uses
```

### Risk Metrics Value Object

```mermaid
classDiagram
    class RiskMetrics {
        <<ValueObject>>
        +decimal Volatility
        +decimal Beta
        +decimal Alpha
        +decimal SharpeRatio
        +decimal SortinoRatio
        +decimal MaxDrawdown
        +decimal VaR95
        +decimal VaR99
        +decimal TrackingError
        
        +CalculateRiskAdjustedReturn() decimal
        +CompareToTarget(RiskMetrics target) RiskComparison
    }
    
    class RiskComparison {
        <<ValueObject>>
        +decimal VolatilityDifference
        +RiskLevel RelativeRiskLevel
        +string RiskAssessment
    }
    
    RiskMetrics --> RiskComparison : creates
```

## Domain Events

### Portfolio Events

```mermaid
sequenceDiagram
    participant Portfolio
    participant DomainEventDispatcher
    participant EventHandlers
    participant PerformanceCalculator
    participant RiskAnalyzer
    
    Portfolio->>DomainEventDispatcher: PositionUpdated Event
    DomainEventDispatcher->>EventHandlers: Dispatch Event
    EventHandlers->>PerformanceCalculator: Recalculate Performance
    EventHandlers->>RiskAnalyzer: Update Risk Metrics
    
    Portfolio->>DomainEventDispatcher: TransactionExecuted Event
    DomainEventDispatcher->>EventHandlers: Dispatch Event
    EventHandlers->>PerformanceCalculator: Update Returns
    
    Portfolio->>DomainEventDispatcher: AllocationChanged Event
    DomainEventDispatcher->>EventHandlers: Dispatch Event
    EventHandlers->>RiskAnalyzer: Check Compliance
```

## Aggregates and Consistency Boundaries

```mermaid
graph TB
    subgraph "Portfolio Aggregate"
        PORT[Portfolio Root]
        POS[Positions]
        TRANS[Transactions]
        PERF[Performance Metrics]
        ORDERS[Trade Orders]
    end
    
    subgraph "Security Aggregate"
        SEC[Security Root]
        MARKET[Market Data]
        CORP[Corporate Actions]
    end
    
    subgraph "User Aggregate"
        USER[User Root]
        PREF[Preferences]
        ROLE[Roles & Permissions]
    end
    
    PORT --> POS
    PORT --> TRANS
    PORT --> PERF
    PORT --> ORDERS
    
    SEC --> MARKET
    SEC --> CORP
    
    USER --> PREF
    USER --> ROLE
    
    PORT -.-> SEC : references
    USER -.-> PORT : manages
```

**Consistency Rules:**
- All changes within an aggregate are atomic
- Cross-aggregate operations use eventual consistency
- Domain events ensure consistency across aggregates
- Business rules are enforced at aggregate boundaries

## Business Rules Matrix

| Entity | Business Rule | Validation | Constraint |
|--------|---------------|------------|------------|
| Portfolio | Target allocations = 100% | Sum validation | Database constraint |
| Position | Quantity > 0 for holdings | Domain validation | Check constraint |
| Transaction | Sufficient cash for buys | Business service | Application logic |
| Security | Unique symbol per exchange | Domain validation | Unique index |
| User | Unique email address | Domain validation | Unique constraint |
| MarketData | No future dates | Domain validation | Check constraint |

This domain model provides:

- **Rich Behavior**: Entities contain business logic, not just data
- **Consistency**: Business rules are enforced at appropriate boundaries  
- **Immutability**: Value objects are immutable for thread safety
- **Events**: Domain events enable loose coupling between aggregates
- **Validation**: Multi-layered validation from domain to database
- **Encapsulation**: Internal state is protected through proper encapsulation