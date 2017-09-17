using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using TaskTracker.Model;
using TaskTracker.Common;
using System.Reflection;

namespace TaskTracker.Repository
{
    public abstract class RepositoryFactory
    {
        public abstract IRepository CreateRepository(string connectionString);
    }

    public delegate void RepositoryOperations(IRepository repository);

    public interface IRepository
    {
        IEnumerable<User> GetUsers(SelectedProperties<User> propertiesToInclude = null);

        IEnumerable<Project> GetProjects(SelectedProperties<Project> propertiesToInclude = null);

        IEnumerable<TaskType> GetTaskTypes();   

        IEnumerable<Task> GetTasks(TaskFilter filter = null, SelectedProperties<Task> sel = null);        

        IEnumerable<Task> GetOpenTasksOfUser(int userId, SelectedProperties<Task> propertiesToInclude = null);

        IEnumerable<Task> GetOpenTasksOfProject(int projectId, SelectedProperties<Task> propertiesToInclude = null);

        IEnumerable<Stage> GetStages(int level, SelectedProperties<Stage> propertiesToInclude = null, bool applySelectionToEntireGraph = false);

        IEnumerable<Tuple<Stage, int>> GetStagesWithMaxActivities(int stageLimit, SelectedProperties<Stage> propertiesToInclude = null);
        IEnumerable<Tuple<Stage, int>> GetStagesWithMaxTasks(int stageLimit, SelectedProperties<Stage> propertiesToInclude = null);
        double GetTotalActivityTimeOfStage(int stageId);

        Task FindTask(int taskId, SelectedProperties<Task> propertiesToInclude = null);

        Stage FindStage(int stageId, SelectedProperties<Stage> propertiesToInclude = null);

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
