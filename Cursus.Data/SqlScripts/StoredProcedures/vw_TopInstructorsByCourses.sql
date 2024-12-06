IF OBJECT_ID('dbo.vw_TopInstructorsByCourses', 'V') IS NOT NULL
    DROP VIEW dbo.vw_TopInstructorsByCourses;
GO

CREATE VIEW dbo.vw_TopInstructorsByCourses
WITH SCHEMABINDING
AS
SELECT 
    ii.Id AS InstructorId,
    u.UserName AS InstructorName,
    COUNT_BIG(ci.CourseId) AS TotalCoursesSold,
    COUNT_BIG(*) AS TotalRecords
FROM dbo.[Order] o
INNER JOIN dbo.CartItems ci ON o.CartId = ci.CartId
INNER JOIN dbo.Courses c ON ci.CourseId = c.Id
INNER JOIN dbo.InstructorInfos ii ON c.InstructorInfoId = ii.Id
INNER JOIN dbo.AspNetUsers u ON ii.UserId = u.Id
WHERE o.Status = 1
GROUP BY ii.Id, u.UserName;
GO

-- Tạo chỉ mục duy nhất cho view
CREATE UNIQUE CLUSTERED INDEX IDX_vw_TopInstructorsByCourses 
ON dbo.vw_TopInstructorsByCourses (InstructorId);
GO
