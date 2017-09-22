CREATE PROCEDURE [dbo].[GetStagesWithMaxTasks]
	@stageLimit INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT TOP(@stageLimit) Stage_Id as StageId, COUNT(*) as TaskCount
	FROM dbo.StageTask st
	GROUP BY st.Stage_Id
	ORDER BY TaskCount DESC;
END