using System;
using System.Collections.Generic;

using TaskTracker.Model;
using TaskTracker.Filters;
using TaskTracker.SyntaxUtils;

namespace TaskTracker.Repository
{
    public abstract class RepositoryFactory
    {
        public abstract IRepository CreateRepository(string connectionString);
    }

    public delegate void RepositoryOperations(IRepository repository);

    public interface IRepository
    {
        IEnumerable<User> GetUsers(PropertySelector<User> propertiesToInclude = null);

        IEnumerable<Project> GetProjects(PropertySelector<Project> propertiesToInclude = null);

        IEnumerable<TaskType> GetTaskTypes();   

        IEnumerable<Task> GetTasks(TaskFilter filter = null, PropertySelector<Task> sel = null);        

        IEnumerable<Task> GetOpenTasksOfUser(int userId, PropertySelector<Task> propertiesToInclude = null);

        IEnumerable<Task> GetOpenTasksOfProject(int projectId, PropertySelector<Task> propertiesToInclude = null);

        IEnumerable<Stage> GetStages(int level, PropertySelector<Stage> propertiesToInclude = null, bool applySelectionToEntireGraph = false);

        IEnumerable<Tuple<Stage, int>> GetStagesWithMaxActivities(int stageLimit, PropertySelector<Stage> propertiesToInclude = null);

        IEnumerable<Tuple<Stage, int>> GetStagesWithMaxTasks(int stageLimit, PropertySelector<Stage> propertiesToInclude = null);

        double GetTotalActivityTimeOfStage(int stageId);

        Task FindTask(int taskId, PropertySelector<Task> propertiesToInclude = null);

        Stage FindStage(int stageId, PropertySelector<Stage> propertiesToInclude = null);

        TaskType FindTaskType(int taskTypeId);

        void Add(Task task);

        void Add(Activity activity);

        void Add(Stage stage);

        void Update(Task task);

        void Update(Activity activity);

        void Update(Stage stage);

        void SetTaskStatus(int taskId, Status newStatus);

        void AddTaskToStage(int taskId, int stageId);

        void RemoveTaskFromStage(int taskId, int stageId);

        void GroupOperations(RepositoryOperations operations);
    }
}
