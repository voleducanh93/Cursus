CREATE OR ALTER PROCEDURE GetTopInstructorsByCourses
    @TopN INT,                     -- Số lượng top instructors cần lấy
    @StartDate DATE = NULL,        -- Ngày bắt đầu (có thể NULL để không lọc)
    @EndDate DATE = NULL,          -- Ngày kết thúc (có thể NULL để không lọc)
    @PageNumber INT = 1,           -- Trang hiện tại
    @PageSize INT = 10             -- Số lượng bản ghi trên mỗi trang
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

    -- Tính toán dữ liệu
    WITH RankedData AS (
        SELECT
            ROW_NUMBER() OVER (
                ORDER BY COUNT(ci.CourseId) DESC, MAX(o.DateCreated) DESC
            ) AS RowNum,
            ii.UserId AS InstructorId,
            u.UserName AS InstructorName,
            COUNT(ci.CourseId) AS TotalCoursesSold, -- Tổng số khóa học bán được
            MAX(o.DateCreated) AS LastOrderDate     -- Ngày giao dịch gần nhất
        FROM dbo.[Order] o
        INNER JOIN dbo.CartItems ci ON o.CartId = ci.CartId
        INNER JOIN dbo.Courses c ON ci.CourseId = c.Id
        INNER JOIN dbo.InstructorInfos ii ON c.InstructorInfoId = ii.Id
        INNER JOIN dbo.AspNetUsers u ON ii.UserId = u.Id
        WHERE o.Status = 1 -- Chỉ tính đơn hàng thành công
          AND (@StartDate IS NULL OR o.DateCreated >= @StartDate)
          AND (@EndDate IS NULL OR o.DateCreated <= @EndDate)
        GROUP BY ii.UserId, u.UserName
    )
    -- Lọc dữ liệu dựa theo TopN và phân trang
    SELECT 
        InstructorId,
        InstructorName,
        TotalCoursesSold,
        LastOrderDate
    FROM RankedData
    WHERE RowNum BETWEEN 1 AND @TopN
    ORDER BY RowNum
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
END;
GO
