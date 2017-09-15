CREATE PROCEDURE [dbo].[GetOpenTasksOfProject]
	@projectId int
AS
BEGIN
	SET NOCOUNT ON;

    SELECT *
    FROM [dbo].TaskSet as tasks
    WHERE tasks.[Status] = 0 AND tasks.[Project_Id] = @projectId
END
