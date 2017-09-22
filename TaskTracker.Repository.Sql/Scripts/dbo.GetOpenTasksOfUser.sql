CREATE PROCEDURE [dbo].[GetOpenTasksOfUser]
	@userId int
AS
BEGIN
	SET NOCOUNT ON;

    SELECT *
    FROM [dbo].TaskSet as tasks
    WHERE tasks.[Status] = 0 AND tasks.[Assignee_Id] = @userId
END