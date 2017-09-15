CREATE PROCEDURE [dbo].[GetStagesWithMaxActivities]
	@stageLimit INT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @ActivitiesInStage TABLE (
		StageId INT,
		ActivityCount INT		
	);

	INSERT INTO @ActivitiesInStage 
		SELECT Stage_Id, COUNT(*)
		FROM dbo.StageTask AS st
			JOIN dbo.ActivitySet AS a ON st.Task_Id = a.Task_Id
		GROUP BY st.Stage_Id;

	SELECT TOP(@stageLimit) StageId, ActivityCount
	FROM @ActivitiesInStage
	WHERE 
		ActivityCount = 
			(SELECT MAX(ActivityCount)
			 FROM @ActivitiesInStage);
END