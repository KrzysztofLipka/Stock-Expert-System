USE [ExpertSystem]
GO

/****** Object:  StoredProcedure [dbo].[Add_Prediction]    Script Date: 19.01.2022 00:40:18 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO




CREATE PROC [dbo].[Add_Prediction] (
	@CompanyName varchar(255),
	@PredictedBuyPrice  float(24),
    @PredictedSellPrice  float(24),
    @ActualBuyPrice  float(24),
    @ActualSellPrice  float(24),
	@StartDate date,
	@EndDate date
)
AS

BEGIN 
SET NOCOUNT ON

INSERT INTO dbo.Predictions(
	CompanyId, PredictedBuyPrice, PredictedSellPrice, ActualBuyPrice, ActualSellPrice, StartDate, EndDate)
	SELECT (SELECT CompanyId FROM Companies WHERE CompanyName = @CompanyName), @PredictedBuyPrice, @PredictedSellPrice, @ActualBuyPrice, @ActualSellPrice, @StartDate, @EndDate

	SELECT SCOPE_IDENTITY() AS PredictionId
	END

GO


USE [ExpertSystem]
GO

/****** Object:  StoredProcedure [dbo].[Add_Price]    Script Date: 19.01.2022 00:41:04 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROC [dbo].[Add_Price] (
	@CompanyName varchar(255),
	@OpeningPrice  float(24),
    @ClosingPrice  float(24),
    @HighestPrice  float(24),
    @LowestPrice  float(24),
    @Volume bigint
)
AS

BEGIN 
SET NOCOUNT ON

INSERT INTO dbo.HistoricalPrices(
	CompanyId, OpeningPrice, ClosingPrice, HighestPrice, LowestPrice, Volume)
	SELECT (SELECT CompanyId FROM Companies WHERE CompanyName = @CompanyName), @OpeningPrice, @ClosingPrice, @HighestPrice, @LowestPrice, @Volume 

	END

GO


USE [ExpertSystem]
GO

/****** Object:  StoredProcedure [dbo].[GetClosingPrices]    Script Date: 19.01.2022 00:41:20 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROC [dbo].[GetClosingPrices] (
	@CompanyName varchar(255)
)
AS

BEGIN 
SET NOCOUNT ON

SELECT ClosingPrice, PriceDate FROM dbo.HistoricalPrices INNER JOIN dbo.Companies 
ON dbo.HistoricalPrices.CompanyId = dbo.Companies.CompanyId
WHERE dbo.Companies.CompanyName = @CompanyName ORDER BY PriceDate ASC
 

END

GO


USE [ExpertSystem]
GO

/****** Object:  StoredProcedure [dbo].[GetClosingPricesWithMaxDate]    Script Date: 19.01.2022 00:41:49 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



CREATE PROC [dbo].[GetClosingPricesWithMaxDate] (
	@CompanyName varchar(255),
	@MaxDate date
)
AS

BEGIN 
SET NOCOUNT ON

SELECT ClosingPrice, PriceDate FROM dbo.HistoricalPrices INNER JOIN dbo.Companies 
ON dbo.HistoricalPrices.CompanyId = dbo.Companies.CompanyId
WHERE dbo.Companies.CompanyName = @CompanyName AND PriceDate < @MaxDate 
ORDER BY PriceDate ASC
 

END

GO


USE [ExpertSystem]
GO

/****** Object:  StoredProcedure [dbo].[GetClosingPricesWithMinDate]    Script Date: 19.01.2022 00:42:04 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO





CREATE PROC [dbo].[GetClosingPricesWithMinDate] (
	@CompanyName varchar(255),
	@MinDate date
)
AS

BEGIN 
SET NOCOUNT ON

SELECT ClosingPrice, PriceDate FROM dbo.HistoricalPrices INNER JOIN dbo.Companies 
ON dbo.HistoricalPrices.CompanyId = dbo.Companies.CompanyId
WHERE dbo.Companies.CompanyName = @CompanyName AND PriceDate > @MinDate 
ORDER BY PriceDate ASC
 

END

GO




USE [ExpertSystem]
GO

/****** Object:  StoredProcedure [dbo].[GetClosingPricesWithMinMaxDate]    Script Date: 19.01.2022 00:42:29 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO




CREATE PROC [dbo].[GetClosingPricesWithMinMaxDate] (
	@CompanyName varchar(255),
	@MinDate date,
	@MaxDate date
)
AS

BEGIN 
SET NOCOUNT ON

SELECT ClosingPrice, PriceDate FROM dbo.HistoricalPrices INNER JOIN dbo.Companies 
ON dbo.HistoricalPrices.CompanyId = dbo.Companies.CompanyId
WHERE dbo.Companies.CompanyName = @CompanyName AND PriceDate > @MinDate AND PriceDate < @MaxDate
ORDER BY PriceDate ASC
 

END

GO


USE [ExpertSystem]
GO

/****** Object:  StoredProcedure [dbo].[GetCompanyId]    Script Date: 19.01.2022 00:42:56 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO




CREATE PROC [dbo].[GetCompanyId](
	@CompanyName varchar(255)
)

AS

	BEGIN

	SELECT CompanyId FROM dbo.Companies WHERE dbo.Companies.CompanyName = @CompanyName

	END

GO


USE [ExpertSystem]
GO

/****** Object:  StoredProcedure [dbo].[GetLastClosingPrices]    Script Date: 19.01.2022 00:43:23 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



CREATE PROC [dbo].[GetLastClosingPrices] (
	@CompanyName varchar(255),
	@NumberOfLastRows int
)
AS

BEGIN 
SET NOCOUNT ON

DECLARE @NumberOfRows INT;
DECLARE @MyCounter INT;

SET @NumberOfRows = 
( SELECT COUNT(*) FROM dbo.HistoricalPrices where CompanyId IN (select CompanyId from dbo.Companies where CompanyName= @CompanyName));

SET @MyCounter = @NumberOfRows - @NumberOfLastRows;

With Testing AS( 
SELECT ClosingPrice, PriceDate,
    ROW_NUMBER() OVER (ORDER BY PriceDate) AS 'RowNumber'
    FROM dbo.HistoricalPrices) 
	
	Select *From Testing Where RowNumber> @MyCounter AND @CompanyName IN (select @CompanyName from dbo.Companies where CompanyName= @CompanyName);

 

END

GO




USE [ExpertSystem]
GO

/****** Object:  StoredProcedure [dbo].[GetLastPriceForCompany]    Script Date: 19.01.2022 00:43:39 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROC [dbo].[GetLastPriceForCompany] (
	@CompanyName varchar(255)
	
)
AS

BEGIN 
SET NOCOUNT ON

SELECT TOP 1 PriceDate from dbo.HistoricalPrices WHERE CompanyId  = (SELECT CompanyId FROM Companies WHERE CompanyName = @CompanyName)  ORDER BY PriceDate Desc ;


	END

GO


USE [ExpertSystem]
GO

/****** Object:  StoredProcedure [dbo].[GetPrices]    Script Date: 19.01.2022 00:43:52 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO



CREATE PROC [dbo].[GetPrices] (
	@CompanyName varchar(255)
)
AS

BEGIN 
SET NOCOUNT ON

SELECT * FROM dbo.HistoricalPrices INNER JOIN dbo.Companies 
ON dbo.HistoricalPrices.CompanyId = dbo.Companies.CompanyId
WHERE dbo.Companies.CompanyName = @CompanyName
 

END

GO








