-- Investment Portfolio Management Database Schema
-- Schéma de base de données pour la gestion de portefeuille d'investissement
-- Created for CDPQ-style institutional investment management
-- Créé pour la gestion d'investissements institutionnels de style CDPQ

USE master;
GO

-- Create the database if it doesn't exist
-- Créer la base de données si elle n'existe pas
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'InvestmentPortfolioManager')
BEGIN
    CREATE DATABASE InvestmentPortfolioManager;
END
GO

USE InvestmentPortfolioManager;
GO

-- Security and Users tables / Tables de sécurité et utilisateurs
CREATE TABLE Users (
    UserId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Username NVARCHAR(100) NOT NULL UNIQUE,
    Email NVARCHAR(255) NOT NULL UNIQUE,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    -- Nom / Prénom
    PasswordHash NVARCHAR(500) NOT NULL,
    Role NVARCHAR(50) NOT NULL CHECK (Role IN ('Admin', 'PortfolioManager', 'Analyst', 'ReadOnly')),
    -- Rôle: Admin, Gestionnaire de portefeuille, Analyste, Lecture seule
    PreferredLanguage CHAR(2) DEFAULT 'EN' CHECK (PreferredLanguage IN ('EN', 'FR')),
    -- Langue préférée: EN ou FR
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE()
);

-- Asset Classes and Securities / Classes d'actifs et titres
CREATE TABLE AssetClasses (
    AssetClassId INT IDENTITY(1,1) PRIMARY KEY,
    Code NVARCHAR(10) NOT NULL UNIQUE,
    NameEN NVARCHAR(100) NOT NULL,
    NameFR NVARCHAR(100) NOT NULL,
    -- Nom en anglais et français
    Description NVARCHAR(500),
    IsActive BIT DEFAULT 1
);

CREATE TABLE Currencies (
    CurrencyId INT IDENTITY(1,1) PRIMARY KEY,
    Code CHAR(3) NOT NULL UNIQUE, -- ISO 4217
    NameEN NVARCHAR(50) NOT NULL,
    NameFR NVARCHAR(50) NOT NULL,
    Symbol NVARCHAR(5),
    IsActive BIT DEFAULT 1
);

CREATE TABLE Countries (
    CountryId INT IDENTITY(1,1) PRIMARY KEY,
    Code CHAR(2) NOT NULL UNIQUE, -- ISO 3166-1 alpha-2
    NameEN NVARCHAR(100) NOT NULL,
    NameFR NVARCHAR(100) NOT NULL,
    Region NVARCHAR(50),
    IsActive BIT DEFAULT 1
);

CREATE TABLE Sectors (
    SectorId INT IDENTITY(1,1) PRIMARY KEY,
    Code NVARCHAR(10) NOT NULL UNIQUE,
    NameEN NVARCHAR(100) NOT NULL,
    NameFR NVARCHAR(100) NOT NULL,
    -- Secteur économique
    IsActive BIT DEFAULT 1
);

CREATE TABLE Securities (
    SecurityId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Symbol NVARCHAR(20) NOT NULL UNIQUE,
    ISIN NVARCHAR(12), -- International Securities Identification Number
    CUSIP NVARCHAR(9), -- Committee on Uniform Securities Identification Procedures
    NameEN NVARCHAR(255) NOT NULL,
    NameFR NVARCHAR(255) NOT NULL,
    SecurityType NVARCHAR(50) NOT NULL CHECK (SecurityType IN ('Stock', 'Bond', 'ETF', 'MutualFund', 'Option', 'Future', 'Alternative')),
    -- Type: Action, Obligation, FNB, Fonds commun, Option, Contrat à terme, Alternatif
    AssetClassId INT NOT NULL REFERENCES AssetClasses(AssetClassId),
    SectorId INT REFERENCES Sectors(SectorId),
    CountryId INT REFERENCES Countries(CountryId),
    CurrencyId INT NOT NULL REFERENCES Currencies(CurrencyId),
    Exchange NVARCHAR(50),
    -- Bourse
    FaceValue DECIMAL(18,4),
    -- Valeur nominale
    MaturityDate DATE, -- For bonds / Pour les obligations
    CouponRate DECIMAL(6,4), -- For bonds / Taux de coupon pour les obligations
    DividendYield DECIMAL(6,4), -- For stocks / Rendement du dividende pour les actions
    Beta DECIMAL(8,4), -- Market beta / Bêta de marché
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE()
);

-- Market Data / Données de marché
CREATE TABLE MarketData (
    MarketDataId BIGINT IDENTITY(1,1) PRIMARY KEY,
    SecurityId UNIQUEIDENTIFIER NOT NULL REFERENCES Securities(SecurityId),
    Date DATE NOT NULL,
    OpenPrice DECIMAL(18,4),
    HighPrice DECIMAL(18,4),
    LowPrice DECIMAL(18,4),
    ClosePrice DECIMAL(18,4) NOT NULL,
    Volume BIGINT,
    AdjustedClose DECIMAL(18,4),
    -- Prix ajusté pour les dividendes et fractionnements
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    CONSTRAINT UK_MarketData_Security_Date UNIQUE (SecurityId, Date)
);

-- Exchange Rates / Taux de change
CREATE TABLE ExchangeRates (
    ExchangeRateId BIGINT IDENTITY(1,1) PRIMARY KEY,
    FromCurrencyId INT NOT NULL REFERENCES Currencies(CurrencyId),
    ToCurrencyId INT NOT NULL REFERENCES Currencies(CurrencyId),
    Date DATE NOT NULL,
    Rate DECIMAL(18,8) NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    CONSTRAINT UK_ExchangeRates_Currencies_Date UNIQUE (FromCurrencyId, ToCurrencyId, Date)
);

-- Portfolios and Holdings / Portefeuilles et positions
CREATE TABLE Portfolios (
    PortfolioId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(255) NOT NULL,
    Description NVARCHAR(1000),
    PortfolioManagerId UNIQUEIDENTIFIER NOT NULL REFERENCES Users(UserId),
    -- Gestionnaire de portefeuille
    BaseCurrencyId INT NOT NULL REFERENCES Currencies(CurrencyId),
    -- Devise de base
    InceptionDate DATE NOT NULL,
    -- Date de création
    BenchmarkSecurityId UNIQUEIDENTIFIER REFERENCES Securities(SecurityId),
    -- Indice de référence
    Status NVARCHAR(20) DEFAULT 'Active' CHECK (Status IN ('Active', 'Closed', 'Suspended')),
    -- Statut: Actif, Fermé, Suspendu
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 DEFAULT GETUTCDATE()
);

CREATE TABLE Holdings (
    HoldingId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    PortfolioId UNIQUEIDENTIFIER NOT NULL REFERENCES Portfolios(PortfolioId),
    SecurityId UNIQUEIDENTIFIER NOT NULL REFERENCES Securities(SecurityId),
    Quantity DECIMAL(18,6) NOT NULL,
    -- Quantité (peut être fractionnelle)
    AverageCost DECIMAL(18,4) NOT NULL,
    -- Coût moyen
    CurrentPrice DECIMAL(18,4),
    -- Prix actuel
    MarketValue DECIMAL(18,2) AS (Quantity * CurrentPrice) PERSISTED,
    -- Valeur marchande calculée
    UnrealizedGainLoss DECIMAL(18,2) AS (Quantity * (CurrentPrice - AverageCost)) PERSISTED,
    -- Gain/perte non réalisé
    LastUpdated DATETIME2 DEFAULT GETUTCDATE(),
    CONSTRAINT UK_Holdings_Portfolio_Security UNIQUE (PortfolioId, SecurityId)
);

-- Transactions / Transactions
CREATE TABLE Transactions (
    TransactionId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    PortfolioId UNIQUEIDENTIFIER NOT NULL REFERENCES Portfolios(PortfolioId),
    SecurityId UNIQUEIDENTIFIER NOT NULL REFERENCES Securities(SecurityId),
    TransactionType NVARCHAR(20) NOT NULL CHECK (TransactionType IN ('Buy', 'Sell', 'Dividend', 'Interest', 'Split', 'Merger')),
    -- Type: Achat, Vente, Dividende, Intérêt, Fractionnement, Fusion
    Quantity DECIMAL(18,6) NOT NULL,
    Price DECIMAL(18,4) NOT NULL,
    GrossAmount DECIMAL(18,2) AS (ABS(Quantity) * Price) PERSISTED,
    -- Montant brut
    Commission DECIMAL(18,2) DEFAULT 0,
    Fees DECIMAL(18,2) DEFAULT 0,
    NetAmount DECIMAL(18,2) AS (
        CASE 
            WHEN TransactionType IN ('Buy') THEN -1 * (ABS(Quantity) * Price + Commission + Fees)
            WHEN TransactionType IN ('Sell') THEN (ABS(Quantity) * Price - Commission - Fees)
            ELSE (Quantity * Price)
        END
    ) PERSISTED,
    -- Montant net
    TradeDate DATE NOT NULL,
    SettlementDate DATE NOT NULL,
    ExecutedBy UNIQUEIDENTIFIER REFERENCES Users(UserId),
    Notes NVARCHAR(1000),
    Status NVARCHAR(20) DEFAULT 'Executed' CHECK (Status IN ('Pending', 'Executed', 'Cancelled', 'Failed')),
    -- Statut: En attente, Exécuté, Annulé, Échoué
    CreatedAt DATETIME2 DEFAULT GETUTCDATE()
);

-- Risk Management / Gestion des risques
CREATE TABLE RiskMetrics (
    RiskMetricId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    PortfolioId UNIQUEIDENTIFIER NOT NULL REFERENCES Portfolios(PortfolioId),
    Date DATE NOT NULL,
    TotalValue DECIMAL(18,2) NOT NULL,
    -- Valeur totale
    ValueAtRisk95 DECIMAL(18,2),
    -- VaR à 95%
    ValueAtRisk99 DECIMAL(18,2),
    -- VaR à 99%
    ConditionalVaR95 DECIMAL(18,2),
    -- CVaR à 95%
    VolatilityAnnualized DECIMAL(8,4),
    -- Volatilité annualisée
    SharpeRatio DECIMAL(8,4),
    -- Ratio de Sharpe
    SortinoRatio DECIMAL(8,4),
    -- Ratio de Sortino
    MaxDrawdown DECIMAL(8,4),
    -- Drawdown maximum
    Beta DECIMAL(8,4),
    -- Bêta du portefeuille
    Alpha DECIMAL(8,4),
    -- Alpha du portefeuille
    TrackingError DECIMAL(8,4),
    -- Erreur de suivi
    InformationRatio DECIMAL(8,4),
    -- Ratio d'information
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    CONSTRAINT UK_RiskMetrics_Portfolio_Date UNIQUE (PortfolioId, Date)
);

CREATE TABLE PortfolioPerformance (
    PerformanceId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    PortfolioId UNIQUEIDENTIFIER NOT NULL REFERENCES Portfolios(PortfolioId),
    Date DATE NOT NULL,
    TotalReturn1D DECIMAL(8,4),
    -- Rendement 1 jour
    TotalReturn1W DECIMAL(8,4),
    -- Rendement 1 semaine
    TotalReturn1M DECIMAL(8,4),
    -- Rendement 1 mois
    TotalReturn3M DECIMAL(8,4),
    -- Rendement 3 mois
    TotalReturn6M DECIMAL(8,4),
    -- Rendement 6 mois
    TotalReturn1Y DECIMAL(8,4),
    -- Rendement 1 an
    TotalReturnYTD DECIMAL(8,4),
    -- Rendement depuis le début de l'année
    TotalReturnInception DECIMAL(8,4),
    -- Rendement depuis la création
    BenchmarkReturn1D DECIMAL(8,4),
    BenchmarkReturn1W DECIMAL(8,4),
    BenchmarkReturn1M DECIMAL(8,4),
    BenchmarkReturn3M DECIMAL(8,4),
    BenchmarkReturn6M DECIMAL(8,4),
    BenchmarkReturn1Y DECIMAL(8,4),
    BenchmarkReturnYTD DECIMAL(8,4),
    BenchmarkReturnInception DECIMAL(8,4),
    -- Rendements de l'indice de référence
    ActiveReturn1D DECIMAL(8,4),
    ActiveReturn1W DECIMAL(8,4),
    ActiveReturn1M DECIMAL(8,4),
    ActiveReturn3M DECIMAL(8,4),
    ActiveReturn6M DECIMAL(8,4),
    ActiveReturn1Y DECIMAL(8,4),
    ActiveReturnYTD DECIMAL(8,4),
    ActiveReturnInception DECIMAL(8,4),
    -- Rendements actifs (portefeuille - référence)
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    CONSTRAINT UK_PortfolioPerformance_Portfolio_Date UNIQUE (PortfolioId, Date)
);

-- Asset Allocation / Allocation d'actifs
CREATE TABLE AssetAllocation (
    AllocationId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    PortfolioId UNIQUEIDENTIFIER NOT NULL REFERENCES Portfolios(PortfolioId),
    Date DATE NOT NULL,
    AssetClassId INT NOT NULL REFERENCES AssetClasses(AssetClassId),
    MarketValue DECIMAL(18,2) NOT NULL,
    Percentage DECIMAL(5,2) NOT NULL,
    -- Pourcentage de l'allocation
    TargetPercentage DECIMAL(5,2),
    -- Pourcentage cible
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    CONSTRAINT UK_AssetAllocation_Portfolio_Date_Class UNIQUE (PortfolioId, Date, AssetClassId)
);

-- Risk Alerts / Alertes de risque
CREATE TABLE RiskAlerts (
    AlertId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    PortfolioId UNIQUEIDENTIFIER NOT NULL REFERENCES Portfolios(PortfolioId),
    AlertType NVARCHAR(50) NOT NULL CHECK (AlertType IN ('VaR_Breach', 'Concentration', 'Volatility', 'Drawdown', 'Compliance')),
    -- Type d'alerte: Dépassement VaR, Concentration, Volatilité, Drawdown, Conformité
    Severity NVARCHAR(20) NOT NULL CHECK (Severity IN ('Low', 'Medium', 'High', 'Critical')),
    -- Gravité: Faible, Moyenne, Élevée, Critique
    Message NVARCHAR(1000) NOT NULL,
    Threshold DECIMAL(18,4),
    -- Seuil
    ActualValue DECIMAL(18,4),
    -- Valeur actuelle
    Status NVARCHAR(20) DEFAULT 'Active' CHECK (Status IN ('Active', 'Acknowledged', 'Resolved')),
    -- Statut: Actif, Accusé réception, Résolu
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    AcknowledgedAt DATETIME2,
    AcknowledgedBy UNIQUEIDENTIFIER REFERENCES Users(UserId),
    ResolvedAt DATETIME2,
    ResolvedBy UNIQUEIDENTIFIER REFERENCES Users(UserId)
);

-- Stress Testing / Tests de stress
CREATE TABLE StressTestScenarios (
    ScenarioId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Name NVARCHAR(255) NOT NULL,
    Description NVARCHAR(1000),
    ScenarioType NVARCHAR(50) NOT NULL CHECK (ScenarioType IN ('Market_Crash', 'Interest_Rate', 'Currency', 'Credit', 'Liquidity', 'Custom')),
    -- Type: Krach boursier, Taux d'intérêt, Change, Crédit, Liquidité, Personnalisé
    Parameters NVARCHAR(MAX), -- JSON format for scenario parameters
    -- Paramètres au format JSON
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    CreatedBy UNIQUEIDENTIFIER NOT NULL REFERENCES Users(UserId)
);

CREATE TABLE StressTestResults (
    ResultId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    PortfolioId UNIQUEIDENTIFIER NOT NULL REFERENCES Portfolios(PortfolioId),
    ScenarioId UNIQUEIDENTIFIER NOT NULL REFERENCES StressTestScenarios(ScenarioId),
    Date DATE NOT NULL,
    CurrentValue DECIMAL(18,2) NOT NULL,
    -- Valeur actuelle
    StressedValue DECIMAL(18,2) NOT NULL,
    -- Valeur sous stress
    PnL DECIMAL(18,2) AS (StressedValue - CurrentValue) PERSISTED,
    -- Profit et perte
    PnLPercentage DECIMAL(8,4) AS ((StressedValue - CurrentValue) / CurrentValue * 100) PERSISTED,
    -- P&L en pourcentage
    CreatedAt DATETIME2 DEFAULT GETUTCDATE()
);

-- Create indexes for performance / Créer des index pour la performance
CREATE INDEX IX_MarketData_Security_Date ON MarketData(SecurityId, Date DESC);
CREATE INDEX IX_Transactions_Portfolio_Date ON Transactions(PortfolioId, TradeDate DESC);
CREATE INDEX IX_Holdings_Portfolio ON Holdings(PortfolioId);
CREATE INDEX IX_RiskMetrics_Portfolio_Date ON RiskMetrics(PortfolioId, Date DESC);
CREATE INDEX IX_PortfolioPerformance_Portfolio_Date ON PortfolioPerformance(PortfolioId, Date DESC);
CREATE INDEX IX_RiskAlerts_Portfolio_Status ON RiskAlerts(PortfolioId, Status);
CREATE INDEX IX_AssetAllocation_Portfolio_Date ON AssetAllocation(PortfolioId, Date DESC);

-- Insert reference data / Insérer les données de référence
INSERT INTO AssetClasses (Code, NameEN, NameFR) VALUES
('EQ', 'Equities', 'Actions'),
('FI', 'Fixed Income', 'Revenu fixe'),
('ALT', 'Alternatives', 'Alternatifs'),
('CASH', 'Cash', 'Liquidités'),
('REIT', 'Real Estate', 'Immobilier'),
('COMM', 'Commodities', 'Matières premières');

INSERT INTO Currencies (Code, NameEN, NameFR, Symbol) VALUES
('CAD', 'Canadian Dollar', 'Dollar canadien', '$'),
('USD', 'US Dollar', 'Dollar américain', '$'),
('EUR', 'Euro', 'Euro', '€'),
('GBP', 'British Pound', 'Livre sterling', '£'),
('JPY', 'Japanese Yen', 'Yen japonais', '¥'),
('CHF', 'Swiss Franc', 'Franc suisse', 'CHF');

INSERT INTO Countries (Code, NameEN, NameFR, Region) VALUES
('CA', 'Canada', 'Canada', 'North America'),
('US', 'United States', 'États-Unis', 'North America'),
('GB', 'United Kingdom', 'Royaume-Uni', 'Europe'),
('DE', 'Germany', 'Allemagne', 'Europe'),
('FR', 'France', 'France', 'Europe'),
('JP', 'Japan', 'Japon', 'Asia'),
('CH', 'Switzerland', 'Suisse', 'Europe');

INSERT INTO Sectors (Code, NameEN, NameFR) VALUES
('TECH', 'Technology', 'Technologie'),
('FINL', 'Financial', 'Financier'),
('HLTH', 'Healthcare', 'Santé'),
('CONS', 'Consumer', 'Consommation'),
('INDU', 'Industrial', 'Industriel'),
('ENRG', 'Energy', 'Énergie'),
('UTIL', 'Utilities', 'Services publics'),
('TELE', 'Telecommunications', 'Télécommunications'),
('MATE', 'Materials', 'Matériaux'),
('REAL', 'Real Estate', 'Immobilier');

GO