CREATE OR ALTER PROCEDURE GetTopInstructorsByRevenue
    @TopN INT,
    @StartDate DATE,
    @EndDate DATE,
    @PageNumber INT,
    @PageSize INT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

    WITH TopInstructors AS (
        SELECT 
            ii.UserId AS InstructorId,
            u.UserName AS InstructorName,
            SUM(o.PaidAmount) AS TotalEarnings,
            MAX(o.DateCreated) AS LastOrderDate
        FROM dbo.[Order] o
        INNER JOIN dbo.CartItems ci ON o.CartId = ci.CartId
        INNER JOIN dbo.Courses c ON ci.CourseId = c.Id
        INNER JOIN dbo.InstructorInfos ii ON c.InstructorInfoId = ii.Id
        INNER JOIN dbo.AspNetUsers u ON ii.UserId = u.Id
        WHERE o.Status = 1 AND (o.DateCreated >= @StartDate OR @StartDate IS NULL)
            AND (o.DateCreated <= @EndDate OR @EndDate IS NULL)
        GROUP BY ii.UserId, u.UserName
        ORDER BY TotalEarnings DESC
        OFFSET 0 ROWS FETCH NEXT @TopN ROWS ONLY
    )
    SELECT 
        InstructorId,
        InstructorName,
        TotalEarnings,
        LastOrderDate
    FROM TopInstructors
    ORDER BY TotalEarnings DESC
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END;
GO
