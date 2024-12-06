IF OBJECT_ID('dbo.vw_TotalSalesRevenue', 'V') IS NOT NULL
    DROP VIEW dbo.vw_TotalSalesRevenue;
GO
CREATE VIEW dbo.vw_TotalSalesRevenue
WITH SCHEMABINDING AS
SELECT 
    COUNT_BIG(*) AS RecordCount,
    COUNT_BIG(o.OrderId) AS TotalSales, 
    SUM(CASE WHEN o.Status = 1 THEN o.PaidAmount ELSE 0 END) AS TotalRevenue, 
    o.Status,
    CONVERT(DATE, o.DateCreated) AS DateCreated 
FROM dbo.[Order] o
WHERE o.Status = 1
GROUP BY o.Status, CONVERT(DATE, o.DateCreated);
GO
CREATE UNIQUE CLUSTERED INDEX IDX_vw_TotalSalesRevenue 
ON dbo.vw_TotalSalesRevenue (Status, DateCreated);
GO
