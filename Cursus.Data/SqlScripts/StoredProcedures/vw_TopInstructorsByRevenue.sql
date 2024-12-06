IF OBJECT_ID('dbo.vw_TopInstructorsByRevenue', 'V') IS NOT NULL
    DROP VIEW dbo.vw_TopInstructorsByRevenue;
GO

CREATE VIEW dbo.vw_TopInstructorsByRevenue
WITH SCHEMABINDING
AS
SELECT 
    ii.UserId AS InstructorId,
    u.UserName AS InstructorName,
    SUM(o.PaidAmount) AS TotalEarnings,
    COUNT_BIG(*) AS TotalSales
FROM dbo.[Order] o
INNER JOIN dbo.CartItems ci ON o.CartId = ci.CartId
INNER JOIN dbo.Courses c ON ci.CourseId = c.Id
INNER JOIN dbo.InstructorInfos ii ON c.InstructorInfoId = ii.Id
INNER JOIN dbo.AspNetUsers u ON ii.UserId = u.Id
WHERE o.Status = 1
GROUP BY ii.UserId, u.UserName;
GO
CREATE UNIQUE CLUSTERED INDEX IDX_vw_TopInstructorsByRevenue 
ON dbo.vw_TopInstructorsByRevenue (InstructorId);
GO
