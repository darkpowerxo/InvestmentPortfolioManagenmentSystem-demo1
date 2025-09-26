-- Investment Portfolio Management Sample Data
-- Données d'exemple pour la gestion de portefeuille d'investissement
-- Sample data representing realistic CDPQ-style institutional investment scenarios
-- Données d'exemple représentant des scénarios d'investissement institutionnel de style CDPQ

USE InvestmentPortfolioManager;
GO

-- Sample Users / Utilisateurs d'exemple
INSERT INTO Users (UserId, Username, Email, FirstName, LastName, PasswordHash, Role, PreferredLanguage) VALUES
(NEWID(), 'jdupont', 'jean.dupont@cdpq.com', 'Jean', 'Dupont', 'hashed_password_1', 'PortfolioManager', 'FR'),
(NEWID(), 'msmith', 'mary.smith@cdpq.com', 'Mary', 'Smith', 'hashed_password_2', 'PortfolioManager', 'EN'),
(NEWID(), 'ptremblay', 'pierre.tremblay@cdpq.com', 'Pierre', 'Tremblay', 'hashed_password_3', 'Analyst', 'FR'),
(NEWID(), 'sjohnson', 'sarah.johnson@cdpq.com', 'Sarah', 'Johnson', 'hashed_password_4', 'Analyst', 'EN'),
(NEWID(), 'admin', 'admin@cdpq.com', 'System', 'Administrator', 'hashed_password_5', 'Admin', 'EN');

-- Sample Securities representing typical CDPQ holdings
-- Titres d'exemple représentant les positions typiques de la CDPQ
DECLARE @EquityClassId INT = (SELECT AssetClassId FROM AssetClasses WHERE Code = 'EQ');
DECLARE @FixedIncomeClassId INT = (SELECT AssetClassId FROM AssetClasses WHERE Code = 'FI');
DECLARE @AlternativeClassId INT = (SELECT AssetClassId FROM AssetClasses WHERE Code = 'ALT');
DECLARE @CashClassId INT = (SELECT AssetClassId FROM AssetClasses WHERE Code = 'CASH');
DECLARE @REITClassId INT = (SELECT AssetClassId FROM AssetClasses WHERE Code = 'REIT');

DECLARE @CAD INT = (SELECT CurrencyId FROM Currencies WHERE Code = 'CAD');
DECLARE @USD INT = (SELECT CurrencyId FROM Currencies WHERE Code = 'USD');
DECLARE @EUR INT = (SELECT CurrencyId FROM Currencies WHERE Code = 'EUR');

DECLARE @TechSector INT = (SELECT SectorId FROM Sectors WHERE Code = 'TECH');
DECLARE @FinancialSector INT = (SELECT SectorId FROM Sectors WHERE Code = 'FINL');
DECLARE @EnergyS3ector INT = (SELECT SectorId FROM Sectors WHERE Code = 'ENRG');
DECLARE @HealthcareSector INT = (SELECT SectorId FROM Sectors WHERE Code = 'HLTH');
DECLARE @IndustrialSector INT = (SELECT SectorId FROM Sectors WHERE Code = 'INDU');

DECLARE @Canada INT = (SELECT CountryId FROM Countries WHERE Code = 'CA');
DECLARE @USA INT = (SELECT CountryId FROM Countries WHERE Code = 'US');
DECLARE @Germany INT = (SELECT CountryId FROM Countries WHERE Code = 'DE');

-- Canadian Equities / Actions canadiennes
INSERT INTO Securities (SecurityId, Symbol, ISIN, NameEN, NameFR, SecurityType, AssetClassId, SectorId, CountryId, CurrencyId, Exchange, Beta, DividendYield) VALUES
(NEWID(), 'SHOP.TO', 'CA82509L1076', 'Shopify Inc.', 'Shopify Inc.', 'Stock', @EquityClassId, @TechSector, @Canada, @CAD, 'TSX', 1.45, 0.00),
(NEWID(), 'RY.TO', 'CA7800871021', 'Royal Bank of Canada', 'Banque Royale du Canada', 'Stock', @EquityClassId, @FinancialSector, @Canada, @CAD, 'TSX', 1.05, 0.0425),
(NEWID(), 'CNR.TO', 'CA1363751027', 'Canadian National Railway', 'Compagnie des chemins de fer nationaux du Canada', 'Stock', @EquityClassId, @IndustrialSector, @Canada, @CAD, 'TSX', 0.85, 0.0195),
(NEWID(), 'ENB.TO', 'CA29250N1050', 'Enbridge Inc.', 'Enbridge Inc.', 'Stock', @EquityClassId, @EnergyS3ector, @Canada, @CAD, 'TSX', 0.75, 0.065),
(NEWID(), 'XIU.TO', 'CA4823001071', 'iShares Core S&P Total Canadian Stock Market ETF', 'FNB iShares Core S&P du marché boursier canadien total', 'ETF', @EquityClassId, NULL, @Canada, @CAD, 'TSX', 1.00, 0.024);

-- US Equities / Actions américaines
INSERT INTO Securities (SecurityId, Symbol, ISIN, NameEN, NameFR, SecurityType, AssetClassId, SectorId, CountryId, CurrencyId, Exchange, Beta, DividendYield) VALUES
(NEWID(), 'AAPL', 'US0378331005', 'Apple Inc.', 'Apple Inc.', 'Stock', @EquityClassId, @TechSector, @USA, @USD, 'NASDAQ', 1.25, 0.0044),
(NEWID(), 'MSFT', 'US5949181045', 'Microsoft Corporation', 'Microsoft Corporation', 'Stock', @EquityClassId, @TechSector, @USA, @USD, 'NASDAQ', 1.15, 0.0068),
(NEWID(), 'GOOGL', 'US02079K3059', 'Alphabet Inc.', 'Alphabet Inc.', 'Stock', @EquityClassId, @TechSector, @USA, @USD, 'NASDAQ', 1.05, 0.00),
(NEWID(), 'JNJ', 'US4781601046', 'Johnson & Johnson', 'Johnson & Johnson', 'Stock', @EquityClassId, @HealthcareSector, @USA, @USD, 'NYSE', 0.68, 0.0275),
(NEWID(), 'SPY', 'US78462F1030', 'SPDR S&P 500 ETF Trust', 'FNB SPDR S&P 500', 'ETF', @EquityClassId, NULL, @USA, @USD, 'NYSE', 1.00, 0.013);

-- Fixed Income Securities / Titres à revenu fixe
INSERT INTO Securities (SecurityId, Symbol, ISIN, NameEN, NameFR, SecurityType, AssetClassId, SectorId, CountryId, CurrencyId, Exchange, FaceValue, MaturityDate, CouponRate) VALUES
(NEWID(), 'CAN.GOV.2030', 'CA135087ZT45', 'Government of Canada Bond 2.75% 2030', 'Obligation du gouvernement du Canada 2,75 % 2030', 'Bond', @FixedIncomeClassId, NULL, @Canada, @CAD, 'OTC', 1000.00, '2030-06-01', 0.0275),
(NEWID(), 'CAN.GOV.2035', 'CA135087ZU28', 'Government of Canada Bond 3.25% 2035', 'Obligation du gouvernement du Canada 3,25 % 2035', 'Bond', @FixedIncomeClassId, NULL, @Canada, @CAD, 'OTC', 1000.00, '2035-06-01', 0.0325),
(NEWID(), 'US.TREAS.2033', 'US912810TM62', 'US Treasury Bond 4.0% 2033', 'Obligation du Trésor américain 4,0 % 2033', 'Bond', @FixedIncomeClassId, NULL, @USA, @USD, 'OTC', 1000.00, '2033-11-15', 0.040),
(NEWID(), 'XBB.TO', 'CA4823071019', 'iShares Core Canadian Universe Bond Index ETF', 'FNB iShares Core de l\'indice obligataire universel canadien', 'ETF', @FixedIncomeClassId, NULL, @Canada, @CAD, 'TSX', NULL, NULL, NULL);

-- Alternative Investments / Investissements alternatifs
INSERT INTO Securities (SecurityId, Symbol, ISIN, NameEN, NameFR, SecurityType, AssetClassId, SectorId, CountryId, CurrencyId, Exchange) VALUES
(NEWID(), 'PRIV.EQUITY.1', NULL, 'Private Equity Fund I', 'Fonds de capital-investissement I', 'Alternative', @AlternativeClassId, NULL, @Canada, @CAD, 'OTC'),
(NEWID(), 'INFRA.FUND.1', NULL, 'Infrastructure Investment Fund', 'Fonds d\'investissement en infrastructure', 'Alternative', @AlternativeClassId, NULL, @Canada, @CAD, 'OTC'),
(NEWID(), 'HEDGE.FUND.1', NULL, 'Multi-Strategy Hedge Fund', 'Fonds spéculatif multi-stratégies', 'Alternative', @AlternativeClassId, NULL, @USA, @USD, 'OTC');

-- Real Estate / Immobilier
INSERT INTO Securities (SecurityId, Symbol, ISIN, NameEN, NameFR, SecurityType, AssetClassId, SectorId, CountryId, CurrencyId, Exchange, DividendYield) VALUES
(NEWID(), 'REI.UN.TO', 'CA7549141043', 'RioCan Real Estate Investment Trust', 'Fiducie de placement immobilier RioCan', 'Stock', @REITClassId, NULL, @Canada, @CAD, 'TSX', 0.055),
(NEWID(), 'VNQ', 'US9229087690', 'Vanguard Real Estate ETF', 'FNB immobilier Vanguard', 'ETF', @REITClassId, NULL, @USA, @USD, 'NYSE', 0.035);

-- Create sample portfolios / Créer des portefeuilles d'exemple
DECLARE @PortfolioManager1 UNIQUEIDENTIFIER = (SELECT UserId FROM Users WHERE Username = 'jdupont');
DECLARE @PortfolioManager2 UNIQUEIDENTIFIER = (SELECT UserId FROM Users WHERE Username = 'msmith');
DECLARE @BenchmarkSecurity UNIQUEIDENTIFIER = (SELECT SecurityId FROM Securities WHERE Symbol = 'XIU.TO');

DECLARE @Portfolio1 UNIQUEIDENTIFIER = NEWID();
DECLARE @Portfolio2 UNIQUEIDENTIFIER = NEWID();
DECLARE @Portfolio3 UNIQUEIDENTIFIER = NEWID();

INSERT INTO Portfolios (PortfolioId, Name, Description, PortfolioManagerId, BaseCurrencyId, InceptionDate, BenchmarkSecurityId) VALUES
(@Portfolio1, 'Canadian Equity Growth Fund', 'Fonds de croissance d''actions canadiennes', @PortfolioManager1, @CAD, '2020-01-01', @BenchmarkSecurity),
(@Portfolio2, 'Global Balanced Portfolio', 'Portefeuille équilibré mondial', @PortfolioManager2, @CAD, '2019-06-01', @BenchmarkSecurity),
(@Portfolio3, 'Fixed Income Conservative', 'Revenu fixe conservateur', @PortfolioManager1, @CAD, '2021-03-01', NULL);

-- Sample Holdings for Portfolio 1 (Canadian Equity Growth)
-- Positions d'exemple pour le Portefeuille 1 (Croissance d'actions canadiennes)
INSERT INTO Holdings (PortfolioId, SecurityId, Quantity, AverageCost, CurrentPrice) VALUES
(@Portfolio1, (SELECT SecurityId FROM Securities WHERE Symbol = 'SHOP.TO'), 5000, 85.50, 92.30),
(@Portfolio1, (SELECT SecurityId FROM Securities WHERE Symbol = 'RY.TO'), 10000, 125.75, 132.45),
(@Portfolio1, (SELECT SecurityId FROM Securities WHERE Symbol = 'CNR.TO'), 8000, 145.20, 148.90),
(@Portfolio1, (SELECT SecurityId FROM Securities WHERE Symbol = 'ENB.TO'), 12000, 52.80, 54.65),
(@Portfolio1, (SELECT SecurityId FROM Securities WHERE Symbol = 'XIU.TO'), 15000, 28.95, 30.12);

-- Sample Holdings for Portfolio 2 (Global Balanced)
-- Positions d'exemple pour le Portefeuille 2 (Équilibré mondial)
INSERT INTO Holdings (PortfolioId, SecurityId, Quantity, AverageCost, CurrentPrice) VALUES
(@Portfolio2, (SELECT SecurityId FROM Securities WHERE Symbol = 'AAPL'), 3000, 180.25, 195.50),
(@Portfolio2, (SELECT SecurityId FROM Securities WHERE Symbol = 'MSFT'), 2500, 320.80, 340.15),
(@Portfolio2, (SELECT SecurityId FROM Securities WHERE Symbol = 'SPY'), 8000, 420.50, 445.75),
(@Portfolio2, (SELECT SecurityId FROM Securities WHERE Symbol = 'XBB.TO'), 20000, 25.40, 24.85),
(@Portfolio2, (SELECT SecurityId FROM Securities WHERE Symbol = 'VNQ'), 5000, 95.20, 98.75),
(@Portfolio2, (SELECT SecurityId FROM Securities WHERE Symbol = 'PRIV.EQUITY.1'), 1, 5000000.00, 5750000.00),
(@Portfolio2, (SELECT SecurityId FROM Securities WHERE Symbol = 'INFRA.FUND.1'), 1, 8000000.00, 8950000.00);

-- Sample Holdings for Portfolio 3 (Fixed Income Conservative)
-- Positions d'exemple pour le Portefeuille 3 (Revenu fixe conservateur)
INSERT INTO Holdings (PortfolioId, SecurityId, Quantity, AverageCost, CurrentPrice) VALUES
(@Portfolio3, (SELECT SecurityId FROM Securities WHERE Symbol = 'CAN.GOV.2030'), 50000, 98.50, 96.75),
(@Portfolio3, (SELECT SecurityId FROM Securities WHERE Symbol = 'CAN.GOV.2035'), 30000, 92.25, 89.80),
(@Portfolio3, (SELECT SecurityId FROM Securities WHERE Symbol = 'US.TREAS.2033'), 25000, 102.15, 104.25),
(@Portfolio3, (SELECT SecurityId FROM Securities WHERE Symbol = 'XBB.TO'), 40000, 25.40, 24.85);

-- Sample Market Data for the last 30 days
-- Données de marché d'exemple pour les 30 derniers jours
DECLARE @StartDate DATE = DATEADD(DAY, -30, GETDATE());
DECLARE @CurrentDate DATE = @StartDate;

-- Generate sample market data for key securities
-- Générer des données de marché d'exemple pour les titres clés
WHILE @CurrentDate <= GETDATE()
BEGIN
    -- SHOP.TO price simulation
    INSERT INTO MarketData (SecurityId, Date, OpenPrice, HighPrice, LowPrice, ClosePrice, Volume, AdjustedClose)
    VALUES (
        (SELECT SecurityId FROM Securities WHERE Symbol = 'SHOP.TO'),
        @CurrentDate,
        90 + (RAND() * 10),
        92 + (RAND() * 8),
        88 + (RAND() * 6),
        89 + (RAND() * 6),
        1000000 + (RAND() * 500000),
        89 + (RAND() * 6)
    );
    
    -- RY.TO price simulation
    INSERT INTO MarketData (SecurityId, Date, OpenPrice, HighPrice, LowPrice, ClosePrice, Volume, AdjustedClose)
    VALUES (
        (SELECT SecurityId FROM Securities WHERE Symbol = 'RY.TO'),
        @CurrentDate,
        130 + (RAND() * 6),
        132 + (RAND() * 4),
        128 + (RAND() * 4),
        130 + (RAND() * 4),
        800000 + (RAND() * 300000),
        130 + (RAND() * 4)
    );
    
    SET @CurrentDate = DATEADD(DAY, 1, @CurrentDate);
END;

-- Sample Exchange Rates
-- Taux de change d'exemple
SET @CurrentDate = DATEADD(DAY, -30, GETDATE());
WHILE @CurrentDate <= GETDATE()
BEGIN
    INSERT INTO ExchangeRates (FromCurrencyId, ToCurrencyId, Date, Rate) VALUES
    (@USD, @CAD, @CurrentDate, 1.35 + (RAND() * 0.1 - 0.05)),
    (@CAD, @USD, @CurrentDate, 0.74 + (RAND() * 0.05 - 0.025)),
    (@EUR, @CAD, @CurrentDate, 1.48 + (RAND() * 0.08 - 0.04));
    
    SET @CurrentDate = DATEADD(DAY, 1, @CurrentDate);
END;

-- Sample Transactions
-- Transactions d'exemple
INSERT INTO Transactions (PortfolioId, SecurityId, TransactionType, Quantity, Price, Commission, Fees, TradeDate, SettlementDate, ExecutedBy) VALUES
(@Portfolio1, (SELECT SecurityId FROM Securities WHERE Symbol = 'SHOP.TO'), 'Buy', 5000, 85.50, 25.00, 10.00, DATEADD(DAY, -20, GETDATE()), DATEADD(DAY, -18, GETDATE()), @PortfolioManager1),
(@Portfolio1, (SELECT SecurityId FROM Securities WHERE Symbol = 'RY.TO'), 'Buy', 10000, 125.75, 50.00, 15.00, DATEADD(DAY, -15, GETDATE()), DATEADD(DAY, -13, GETDATE()), @PortfolioManager1),
(@Portfolio2, (SELECT SecurityId FROM Securities WHERE Symbol = 'AAPL'), 'Buy', 3000, 180.25, 30.00, 12.00, DATEADD(DAY, -10, GETDATE()), DATEADD(DAY, -8, GETDATE()), @PortfolioManager2),
(@Portfolio2, (SELECT SecurityId FROM Securities WHERE Symbol = 'AAPL'), 'Sell', 500, 195.50, 15.00, 8.00, DATEADD(DAY, -5, GETDATE()), DATEADD(DAY, -3, GETDATE()), @PortfolioManager2);

-- Sample Risk Metrics
-- Métriques de risque d'exemple
INSERT INTO RiskMetrics (PortfolioId, Date, TotalValue, ValueAtRisk95, ValueAtRisk99, ConditionalVaR95, VolatilityAnnualized, SharpeRatio, SortinoRatio, MaxDrawdown, Beta, Alpha, TrackingError, InformationRatio) VALUES
(@Portfolio1, GETDATE(), 3250000.00, 162500.00, 227500.00, 292500.00, 0.1845, 1.35, 1.89, -0.0875, 1.12, 0.0245, 0.0325, 0.75),
(@Portfolio2, GETDATE(), 8750000.00, 350000.00, 525000.00, 665000.00, 0.1235, 1.58, 2.12, -0.0545, 0.95, 0.0178, 0.0285, 0.62),
(@Portfolio3, GETDATE(), 4150000.00, 83000.00, 124500.00, 149400.00, 0.0485, 0.89, 1.24, -0.0225, 0.25, -0.0065, 0.0125, -0.52);

-- Sample Performance Data
-- Données de performance d'exemple
INSERT INTO PortfolioPerformance (PortfolioId, Date, TotalReturn1D, TotalReturn1W, TotalReturn1M, TotalReturn3M, TotalReturn6M, TotalReturn1Y, TotalReturnYTD, TotalReturnInception, BenchmarkReturn1D, BenchmarkReturn1W, BenchmarkReturn1M, BenchmarkReturn3M, BenchmarkReturn6M, BenchmarkReturn1Y, BenchmarkReturnYTD, BenchmarkReturnInception, ActiveReturn1D, ActiveReturn1W, ActiveReturn1M, ActiveReturn3M, ActiveReturn6M, ActiveReturn1Y, ActiveReturnYTD, ActiveReturnInception) VALUES
(@Portfolio1, GETDATE(), 0.0125, 0.0285, 0.0545, 0.1245, 0.2155, 0.1875, 0.1625, 0.2845, 0.0095, 0.0245, 0.0485, 0.1185, 0.1995, 0.1685, 0.1485, 0.2545, 0.0030, 0.0040, 0.0060, 0.0060, 0.0160, 0.0190, 0.0140, 0.0300),
(@Portfolio2, GETDATE(), 0.0085, 0.0195, 0.0425, 0.0985, 0.1685, 0.1545, 0.1385, 0.2245, 0.0095, 0.0245, 0.0485, 0.1185, 0.1995, 0.1685, 0.1485, 0.2545, -0.0010, -0.0050, -0.0060, -0.0200, -0.0310, -0.0140, -0.0100, -0.0300),
(@Portfolio3, GETDATE(), 0.0025, 0.0065, 0.0145, 0.0385, 0.0685, 0.0545, 0.0485, 0.0945, 0.0095, 0.0245, 0.0485, 0.1185, 0.1995, 0.1685, 0.1485, 0.2545, -0.0070, -0.0180, -0.0340, -0.0800, -0.1310, -0.1140, -0.1000, -0.1600);

-- Sample Asset Allocation
-- Allocation d'actifs d'exemple
INSERT INTO AssetAllocation (PortfolioId, Date, AssetClassId, MarketValue, Percentage, TargetPercentage) VALUES
(@Portfolio1, GETDATE(), @EquityClassId, 3250000.00, 100.00, 100.00),
(@Portfolio2, GETDATE(), @EquityClassId, 5250000.00, 60.00, 65.00),
(@Portfolio2, GETDATE(), @FixedIncomeClassId, 1750000.00, 20.00, 20.00),
(@Portfolio2, GETDATE(), @AlternativeClassId, 1750000.00, 20.00, 15.00),
(@Portfolio3, GETDATE(), @FixedIncomeClassId, 4150000.00, 100.00, 100.00);

-- Sample Stress Test Scenarios
-- Scénarios de test de stress d'exemple
DECLARE @MarketCrashScenario UNIQUEIDENTIFIER = NEWID();
DECLARE @InterestRateScenario UNIQUEIDENTIFIER = NEWID();

INSERT INTO StressTestScenarios (ScenarioId, Name, Description, ScenarioType, Parameters, CreatedBy) VALUES
(@MarketCrashScenario, 'Market Crash -30%', 'Krach boursier de -30%', 'Market_Crash', '{"equity_shock": -0.30, "bond_shock": -0.05, "alternative_shock": -0.25}', (SELECT UserId FROM Users WHERE Username = 'admin')),
(@InterestRateScenario, 'Interest Rate Rise +200bp', 'Hausse des taux d''intérêt de +200 points de base', 'Interest_Rate', '{"rate_shock": 0.02, "duration_impact": true}', (SELECT UserId FROM Users WHERE Username = 'admin'));

-- Sample Stress Test Results
-- Résultats de test de stress d'exemple
INSERT INTO StressTestResults (PortfolioId, ScenarioId, Date, CurrentValue, StressedValue) VALUES
(@Portfolio1, @MarketCrashScenario, GETDATE(), 3250000.00, 2275000.00),
(@Portfolio2, @MarketCrashScenario, GETDATE(), 8750000.00, 6650000.00),
(@Portfolio3, @InterestRateScenario, GETDATE(), 4150000.00, 3940000.00);

-- Sample Risk Alerts
-- Alertes de risque d'exemple
INSERT INTO RiskAlerts (PortfolioId, AlertType, Severity, Message, Threshold, ActualValue) VALUES
(@Portfolio2, 'Concentration', 'Medium', 'Alternative investments allocation exceeds target by 5%', 15.00, 20.00),
(@Portfolio1, 'Volatility', 'Low', 'Portfolio volatility slightly above normal range', 16.00, 18.45);

GO

PRINT 'Sample data insertion completed successfully.';
PRINT 'Insertion des données d''exemple terminée avec succès.';