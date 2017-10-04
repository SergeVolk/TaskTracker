using System;
using System.Collections.Generic;

using TaskTracker.Model;
using TaskTracker.Repository;
using TaskTracker.ExceptionUtils;
using TaskTracker.SyntaxUtils;
using TaskTracker.Filters;

namespace TaskTracker.Service
{
    public class TaskTrackerService : ITaskTrackerService
    {
        private IRepositoryQueries repositoryQueries;
        private ITransactionalRepositoryCommands repositoryCommands;
        private IRepositoryTransaction transaction;

        public TaskTrackerService(IRepositoryQueries repositoryQueries, ITransactionalRepositoryCommands repositoryCommands)
        {
            ArgumentValidation.ThrowIfNull(repositoryQueries, nameof(repositoryQueries));
            ArgumentValidation.ThrowIfNull(repositoryCommands, nameof(repositoryCommands));

            this.repositoryQueries = repositoryQueries;
            this.repositoryCommands = repositoryCommands;
        }

        public void Add(Activity activity)
        {
            ArgumentValidation.ThrowIfNull(activity, nameof(activity));
            GetRepositoryCommands().Add(activity);
        }

        public void Add(Stage stage)
        {
            ArgumentValidation.ThrowIfNull(stage, nameof(stage));
            GetRepositoryCommands().Add(stage);
        }

        public void Add(Task task)
        {
            ArgumentValidation.ThrowIfNull(task, nameof(task));
            GetRepositoryCommands().Add(task);
        }

        public void Add(IEnumerable<Project> projects)
        {
            ArgumentValidation.ThrowIfNull(projects, nameof(projects));
            GetRepositoryCommands().Add(projects);
        }

        public void Add(IEnumerable<User> users)
        {
            ArgumentValidation.ThrowIfNull(users, nameof(users));
            GetRepositoryCommands().Add(users);
        }

        public void Add(IEnumerable<TaskType> taskTypes)
        {
            ArgumentValidation.ThrowIfNull(taskTypes, nameof(taskTypes));
            GetRepositoryCommands().Add(taskTypes);
        }

        public void AddTaskToStage(int taskId, int stageId)
        {
            ArgumentValidation.ThrowIfLess(taskId, 0, nameof(taskId));
            ArgumentValidation.ThrowIfLess(stageId, 0, nameof(stageId));

            GetRepositoryCommands().AddTaskToStage(taskId, stageId);
        }

        public Stage FindStage(int stageId, PropertySelector<Stage> propertiesToInclude = null)
        {
            ArgumentValidation.ThrowIfLess(stageId, 0, nameof(stageId));
            return repositoryQueries.FindStage(stageId, propertiesToInclude);
        }

        public Task FindTask(int taskId, PropertySelector<Task> propertiesToInclude = null)
        {
            ArgumentValidation.ThrowIfLess(taskId, 0, nameof(taskId));
            return repositoryQueries.FindTask(taskId, propertiesToInclude);
        }

        public TaskType FindTaskType(int taskTypeId)
        {
            ArgumentValidation.ThrowIfLess(taskTypeId, 0, nameof(taskTypeId));
            return repositoryQueries.FindTaskType(taskTypeId);
        }

        public IEnumerable<Task> GetOpenTasksOfProject(int projectId, PropertySelector<Task> propertiesToInclude = null)
        {
            ArgumentValidation.ThrowIfLess(projectId, 0, nameof(projectId));
            return repositoryQueries.GetOpenTasksOfProject(projectId, propertiesToInclude);
        }

        public IEnumerable<Task> GetOpenTasksOfUser(int userId, PropertySelector<Task> propertiesToInclude = null)
        {
            ArgumentValidation.ThrowIfLess(userId, 0, nameof(userId));
            return repositoryQueries.GetOpenTasksOfUser(userId, propertiesToInclude);
        }

        public IEnumerable<Project> GetProjects(PropertySelector<Project> propertiesToInclude = null)
        {
            return repositoryQueries.GetProjects(propertiesToInclude);
        }

        public IEnumerable<Stage> GetStages(int level, PropertySelector<Stage> propertiesToInclude = null, bool applySelectionToEntireGraph = false)
        {
            ArgumentValidation.ThrowIfLess(level, 0, nameof(level));
            return repositoryQueries.GetStages(level, propertiesToInclude, applySelectionToEntireGraph);
        }

        public IEnumerable<Tuple<Stage, int>> GetStagesWithMaxActivities(int stageLimit, PropertySelector<Stage> propertiesToInclude = null)
        {
            ArgumentValidation.ThrowIfLess(stageLimit, 0, nameof(stageLimit));
            return repositoryQueries.GetStagesWithMaxActivities(stageLimit, propertiesToInclude);
        }

        public IEnumerable<Tuple<Stage, int>> GetStagesWithMaxTasks(int stageLimit, PropertySelector<Stage> propertiesToInclude = null)
        {
            ArgumentValidation.ThrowIfLess(stageLimit, 0, nameof(stageLimit));
            return repositoryQueries.GetStagesWithMaxTasks(stageLimit, propertiesToInclude);
        }        

        public IEnumerable<Task> GetTasks(TaskFilter filter = null, PropertySelector<Task> sel = null)
        {
            return repositoryQueries.GetTasks(filter, sel);
        }

        public IEnumerable<TaskType> GetTaskTypes()
        {
            return repositoryQueries.GetTaskTypes();
        }

        public double GetTotalActivityTimeOfStage(int stageId)
        {
            ArgumentValidation.ThrowIfLess(stageId, 0, nameof(stageId));
            return repositoryQueries.GetTotalActivityTimeOfStage(stageId);
        }

        public IEnumerable<User> GetUsers(PropertySelector<User> propertiesToInclude = null)
        {
            return repositoryQueries.GetUsers(propertiesToInclude);            
        }        

        public void BeginTransaction()
        {
            transaction = repositoryCommands.BeginTransaction();
        }

        public void CommitTransaction()
        {
            transaction.CommitTransaction();
            transaction = null;
        }

        public void RollbackTransaction()
        {
            transaction.RollbackTransaction();
            transaction = null;
        }

        public void Dispose()
        {
            repositoryQueries = null;
            repositoryCommands = null;
            transaction = null;
        }

        public void RemoveTaskFromStage(int taskId, int stageId)
        {
            ArgumentValidation.ThrowIfLess(taskId, 0, nameof(taskId));
            ArgumentValidation.ThrowIfLess(stageId, 0, nameof(stageId));
            GetRepositoryCommands().RemoveTaskFromStage(taskId, stageId);
        }

        public void SetTaskStatus(int taskId, Status newStatus)
        {
            ArgumentValidation.ThrowIfLess(taskId, 0, nameof(taskId));
            GetRepositoryCommands().SetTaskStatus(taskId, newStatus);
        }

        public void Update(Stage stage)
        {
            ArgumentValidation.ThrowIfNull(stage, nameof(stage));
            GetRepositoryCommands().Update(stage);
        }

        public void Update(Activity activity)
        {
            ArgumentValidation.ThrowIfNull(activity, nameof(activity));
            GetRepositoryCommands().Update(activity);
        }

        public void Update(Task task)
        {
            ArgumentValidation.ThrowIfNull(task, nameof(task));
            GetRepositoryCommands().Update(task);
        }

        private IRepositoryCommands GetRepositoryCommands()
        {
            return transaction ?? repositoryCommands as IRepositoryCommands;
        }
    }
}
