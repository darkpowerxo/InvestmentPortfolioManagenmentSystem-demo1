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
    Merger = 6,
    SpinOff = 7,
    CashDeposit = 8,
    CashWithdrawal = 9,
    FeePayment = 10
}

public enum PortfolioType
{
    Equity = 1,
    FixedIncome = 2,
    Balanced = 3,
    Alternative = 4,
    Custom = 5
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