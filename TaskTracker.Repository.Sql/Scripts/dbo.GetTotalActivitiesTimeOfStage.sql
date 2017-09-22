CREATE PROCEDURE [dbo].[GetTotalActivitiesTimeOfStage]
	@stageId INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT SUM(DATEDIFF(SS, a.StartTime, ISNULL(a.EndTime, GETDATE())) * 1.0 / 60.0)
	FROM dbo.StageTask st 
		JOIN dbo.ActivitySet a 
		ON st.Stage_Id = @stageId AND st.Task_Id = a.Task_Id;
END