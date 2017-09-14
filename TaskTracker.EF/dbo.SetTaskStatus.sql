CREATE PROCEDURE [dbo].[SetTaskStatus]
	@taskId int,
	@newStatus int
AS
BEGIN
    SET NOCOUNT ON;

    Update [dbo].[TaskSet]
    SET [Status] = @newStatus
    where [Id] = @taskId
END
