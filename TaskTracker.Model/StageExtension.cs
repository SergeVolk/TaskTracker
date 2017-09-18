using System;

namespace TaskTracker.Model
{
    public partial class Stage
    {
        private Stage(int level, Stage parent, string name) : this()
        {
            this.Level = level;
            this.ParentStage = parent;
            this.Name = name;
        }

        public static Stage CreateTopLevelStage(string name)
        {
            return new Stage(0, null, name);
        }

        public Stage AddSubStage(string name)
        {
            var result = new Stage(Level + 1, this, name);
            SubStages.Add(result);
            return result;
        }

        public void VisitAll(Action<Stage> action)
        {
            action(this);
            foreach (var item in SubStages)
            {
                item.VisitAll(action);
            }
        }
    }
}
