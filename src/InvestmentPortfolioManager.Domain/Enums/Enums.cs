namespace InvestmentPortfolioManager.Domain.Enums;

public enum SecurityType
{
    Stock = 1,
    Bond = 2,
    ETF = 3,
    MutualFund = 4,
    Option = 5,
    Future = 6,
    Currency = 7,
    Commodity = 8,
    REIT = 9,
    PrivateEquity = 10,
    HedgeFund = 11,
    Cash = 12
}

public enum TransactionType
{
    Buy = 1,
    Sell = 2,
    Dividend = 3,
    Interest = 4,
    Split = 5,
    StockSplit = 5, // Alias for Split
    Merger = 6,
    SpinOff = 7,
    CashDeposit = 8,
    CashWithdrawal = 9,
    FeePayment = 10
}

public enum PortfolioType
{
    Growth = 1,
    Conservative = 2,
    Balanced = 3,
    Income = 4,
    Alternative = 5,
    Custom = 6
}

public enum RiskLevel
{
    Conservative = 1,
    ModerateConservative = 2,
    Moderate = 3,
    ModerateAggressive = 4,
    Aggressive = 5
}

public enum OrderStatus
{
    Pending = 1,
    PartiallyFilled = 2,
    Filled = 3,
    Cancelled = 4,
    Rejected = 5,
    Expired = 6
}

public enum OrderType
{
    Market = 1,
    Limit = 2,
    Stop = 3,
    StopLimit = 4
}

public enum OrderSide
{
    Buy = 1,
    Sell = 2
}

public enum UserRole
{
    Administrator = 1,
    PortfolioManager = 2,
    Analyst = 3,
    RiskManager = 4,
    Trader = 5,
    Viewer = 6
}