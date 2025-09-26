# Investment Portfolio Manager

A comprehensive full-stack investment portfolio management application built with C# and .NET, designed to showcase skills relevant to institutional investment management such as those required at CDPQ (Caisse de dépôt et placement du Québec).

## Architecture Overview

The application follows Clean Architecture principles with clear separation of concerns:

```
├── src/
│   ├── InvestmentPortfolioManager.Domain/         # Core business entities and value objects
│   ├── InvestmentPortfolioManager.Application/    # Use cases and business logic
│   ├── InvestmentPortfolioManager.Infrastructure/ # Data access and external services
│   └── InvestmentPortfolioManager.API/            # Web API and presentation layer
├── tests/
│   └── InvestmentPortfolioManager.Tests/          # Unit and integration tests
└── frontend/                                      # React-based dashboard (to be added)
```

## Core Features

### Portfolio Analytics Dashboard
- Real-time portfolio performance metrics (returns, risk metrics, Sharpe ratio)
- Asset allocation visualization across investment classes
- Value at Risk (VaR) and comprehensive risk metrics
- Performance attribution analysis vs benchmark indices

### Market Data Integration
- Simulated real-time market data feeds
- Historical price data processing and storage
- Technical indicators (moving averages, volatility)
- Data quality management and missing data handling

### Transaction Processing
- Trade order management (buy/sell securities)
- Transaction validation and compliance checks
- Cost calculation and impact on returns
- Trade confirmations and settlement reports

### Risk Management
- Portfolio stress testing scenarios
- Asset correlation matrices
- Exposure analysis (sector, geography, currency)
- Risk threshold breach alerting

## Technical Stack

- **Backend**: C# / .NET 9.0
- **Database**: SQL Server with Entity Framework Core
- **Authentication**: JWT-based with role-based access
- **API Documentation**: Swagger/OpenAPI
- **Frontend**: React with TypeScript (planned)
- **Testing**: xUnit with comprehensive coverage

## Financial Calculations Included

- Portfolio returns (time-weighted and money-weighted)
- Beta calculation against market indices
- Black-Scholes option pricing model
- Fixed income analytics (duration, convexity, yield)
- Currency conversion and FX impact analysis

## Internationalization

- Multi-language support (French/English)
- Localized financial data formats
- Cultural-specific date and number formatting

## Getting Started

### Prerequisites
- .NET 9.0 SDK or later
- SQL Server or SQL Server Express
- Node.js (for frontend development)

### Building the Application

```bash
# Clone the repository
git clone <repository-url>
cd InvestmentPortfolioManager

# Restore dependencies and build
dotnet restore
dotnet build

# Run tests
dotnet test

# Start the API
cd src/InvestmentPortfolioManager.API
dotnet run
```

The API will be available at `https://localhost:7000` with Swagger documentation at `/swagger`.

## Demo Scenarios

The application includes realistic demo data representing institutional investment scenarios:

1. **Sample Portfolio**: 20-30 positions across equities, bonds, and alternatives
2. **Market Shock Analysis**: Impact simulation on portfolio value
3. **Rebalancing Recommendations**: Based on target allocations
4. **Monthly Performance Reports**: With detailed attribution analysis

## Development Status

This project is currently under active development. Check the issues and project board for current progress and planned features.

## License

This project is for demonstration purposes and educational use.
