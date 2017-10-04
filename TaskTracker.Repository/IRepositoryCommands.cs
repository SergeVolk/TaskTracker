using System;
using System.Collections.Generic;

using TaskTracker.Model;

namespace TaskTracker.Repository
{
    public interface IRepositoryCommands
    {
        void Add(Task task);

        void Add(Activity activity);

        void Add(Stage stage);

        void Add(IEnumerable<Project> projects);

        void Add(IEnumerable<User> users);

        void Add(IEnumerable<TaskType> taskTypes);

        void Update(Task task);

        void Update(Activity activity);

        void Update(Stage stage);

        void SetTaskStatus(int taskId, Status newStatus);

        void AddTaskToStage(int taskId, int stageId);

        void RemoveTaskFromStage(int taskId, int stageId);
    }
}
