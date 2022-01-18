create database ExpertSystem;

use ExpertSystem;

create table Companies (
	CompanyId int IDENTITY(1,1) PRIMARY KEY,
	CompanyName varchar(255),
	LastUpdateDate Date
)


create table HistoricalPrices (
	PriceId int IDENTITY(1,1) PRIMARY KEY,
	CompanyId INT NOT NULL,
	OpeningPrice  float(24),
    ClosingPrice  float(24),
    HighestPrice  float(24),
    LowestPrice  float(24),
    Volume bigint,
	CONSTRAINT FK_CompanyId
    FOREIGN KEY (CompanyId)
    REFERENCES  Companies(CompanyId)
);

GO

ALTER TABLE HistoricalPrices
Add PriceDate date;

GO

create table Predictions (
	PredictionId int IDENTITY(1,1) PRIMARY KEY,
	CompanyId INT NOT NULL,
	PredictedBuyPrice float(24),
	PredictedSellPrice  float(24),
    ActualBuyPrice  float(24),
    ActualSellPrice  float(24),
	StartDate date,
	EndDate date,
	CONSTRAINT FK_PredictionsCompanyId
    FOREIGN KEY (CompanyId)
    REFERENCES  Companies(CompanyId)
);


insert into dbo.Companies(CompanyName) VALUES ('AAPL');
insert into dbo.Companies(CompanyName) VALUES ('rds-a');
insert into dbo.Companies(CompanyName) VALUES ('^ndq');
insert into dbo.Companies(CompanyName) VALUES ('nke');
insert into dbo.Companies(CompanyName) VALUES ('^dji');