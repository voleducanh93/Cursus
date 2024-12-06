IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'GetDashboardMetrics')
    DROP PROCEDURE GetDashboardMetrics;
GO

CREATE PROCEDURE GetDashboardMetrics
    @CurrentStartDate DATE = NULL,
    @CurrentEndDate DATE = NULL,
    @PreviousStartDate DATE = NULL,
    @PreviousEndDate DATE = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        SUM(CASE WHEN DateCreated BETWEEN @CurrentStartDate AND @CurrentEndDate THEN TotalSales ELSE 0 END) AS CurrentTotalSales,
        SUM(CASE WHEN DateCreated BETWEEN @CurrentStartDate AND @CurrentEndDate THEN TotalRevenue ELSE 0 END) AS CurrentTotalRevenue,
        SUM(CASE WHEN DateCreated BETWEEN @PreviousStartDate AND @PreviousEndDate THEN TotalSales ELSE 0 END) AS PreviousTotalSales,
        SUM(CASE WHEN DateCreated BETWEEN @PreviousStartDate AND @PreviousEndDate THEN TotalRevenue ELSE 0 END) AS PreviousTotalRevenue
    FROM dbo.vw_TotalSalesRevenue WITH (NOLOCK);
END;
GO
