using System;

using TaskTracker.Model;
using TaskTracker.ExceptionUtils;

namespace TaskTracker.Model.Utils
{
    public static class StageUtils
    {
        private static Stage CreateStage(int level, Stage parent, string name) 
        {
            ArgumentValidation.ThrowIfLess(level, 0, nameof(level));
            ArgumentValidation.ThrowIfNullOrEmpty(name, nameof(name));

            return new Stage
            {
                Level = level,
                ParentStage = parent,
                Name = name
            };            
        }

        public static Stage CreateTopLevelStage(string name)
        {
            return CreateStage(0, null, name);
        }

        public static Stage AddSubStage(this Stage stage, string name)
        {
            var result = CreateStage(stage.Level + 1, stage, name);
            stage.SubStages.Add(result);
            return result;
        }

        public static void VisitAll(this Stage stage, Action<Stage> action)
        {
            ArgumentValidation.ThrowIfNull(action, nameof(action));

            action(stage);
            foreach (var item in stage.SubStages)
            {
                item.VisitAll(action);
            }
        }
    }
}
