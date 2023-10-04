CREATE TABLE [dbo].[Loads]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [Date] TIMESTAMP NOT NULL, 
    [ActualTotalLoad] FLOAT NULL, 
    [PSEForecastedTotalLoad] FLOAT NULL, 
    [PPForecastedTotalLoad] FLOAT NULL
)
