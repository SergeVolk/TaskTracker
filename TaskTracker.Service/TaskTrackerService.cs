using System;
using System.Collections.Generic;

using TaskTracker.Common;
using TaskTracker.Model;
using TaskTracker.Repository;

namespace TaskTracker.Service
{
    public class TaskTrackerService : ITaskTrackerService
    {
        /*private class ThreadSafeList<T>
        {
            private List<T> list;

            public ThreadSafeList()
            {
                this.list = new List<T>();
            }

            public List<T> Lock()
            {
                Monitor.Enter(list);
                return list;
            }

            public void Unlock()
            {
                Monitor.Exit(list);
            }

            public int Count()
            {
                var list = Lock();
                try
                {
                    return list.Count;
                }
                finally
                {
                    Unlock();
                }

            }
        }*/

        private IRepository repository;
        //private ThreadSafeList<Guid> groupOperations;

        public TaskTrackerService(IRepository repository)
        {
            ArgumentValidation.ThrowIfNull(repository, nameof(repository));
            
            this.repository = repository;
            //this.groupOperations = new ThreadSafeList<Guid>();
        }

        public void Add(Activity activity)
        {
            ArgumentValidation.ThrowIfNull(activity, nameof(activity));
            repository.Add(activity);
        }

        public void Add(Stage stage)
        {
            ArgumentValidation.ThrowIfNull(stage, nameof(stage));
            repository.Add(stage);
        }

        public void Add(Task task)
        {
            ArgumentValidation.ThrowIfNull(task, nameof(task));
            repository.Add(task);
        }

        public void AddTaskToStage(int taskId, int stageId)
        {
            ArgumentValidation.ThrowIfLess(taskId, 0, nameof(taskId));
            ArgumentValidation.ThrowIfLess(stageId, 0, nameof(stageId));

            repository.AddTaskToStage(taskId, stageId);
        }

        public Stage FindStage(int stageId, PropertySelector<Stage> propertiesToInclude = null)
        {
            ArgumentValidation.ThrowIfLess(stageId, 0, nameof(stageId));
            return repository.FindStage(stageId, propertiesToInclude);
        }

        public Task FindTask(int taskId, PropertySelector<Task> propertiesToInclude = null)
        {
            ArgumentValidation.ThrowIfLess(taskId, 0, nameof(taskId));
            return repository.FindTask(taskId, propertiesToInclude);
        }

        public TaskType FindTaskType(int taskTypeId)
        {
            ArgumentValidation.ThrowIfLess(taskTypeId, 0, nameof(taskTypeId));
            return repository.FindTaskType(taskTypeId);
        }

        public IEnumerable<Task> GetOpenTasksOfProject(int projectId, PropertySelector<Task> propertiesToInclude = null)
        {
            ArgumentValidation.ThrowIfLess(projectId, 0, nameof(projectId));
            return repository.GetOpenTasksOfProject(projectId, propertiesToInclude);
        }

        public IEnumerable<Task> GetOpenTasksOfUser(int userId, PropertySelector<Task> propertiesToInclude = null)
        {
            ArgumentValidation.ThrowIfLess(userId, 0, nameof(userId));
            return repository.GetOpenTasksOfUser(userId, propertiesToInclude);
        }

        public IEnumerable<Project> GetProjects(PropertySelector<Project> propertiesToInclude = null)
        {
            return repository.GetProjects(propertiesToInclude);
        }

        public IEnumerable<Stage> GetStages(int level, PropertySelector<Stage> propertiesToInclude = null, bool applySelectionToEntireGraph = false)
        {
            ArgumentValidation.ThrowIfLess(level, 0, nameof(level));
            return repository.GetStages(level, propertiesToInclude, applySelectionToEntireGraph);
        }

        public IEnumerable<Tuple<Stage, int>> GetStagesWithMaxActivities(int stageLimit, PropertySelector<Stage> propertiesToInclude = null)
        {
            ArgumentValidation.ThrowIfLess(stageLimit, 0, nameof(stageLimit));
            return repository.GetStagesWithMaxActivities(stageLimit, propertiesToInclude);
        }

        public IEnumerable<Tuple<Stage, int>> GetStagesWithMaxTasks(int stageLimit, PropertySelector<Stage> propertiesToInclude = null)
        {
            ArgumentValidation.ThrowIfLess(stageLimit, 0, nameof(stageLimit));
            return repository.GetStagesWithMaxTasks(stageLimit, propertiesToInclude);
        }        

        public IEnumerable<Task> GetTasks(TaskFilter filter = null, PropertySelector<Task> sel = null)
        {
            return repository.GetTasks(filter, sel);
        }

        public IEnumerable<TaskType> GetTaskTypes()
        {
            return repository.GetTaskTypes();
        }

        public double GetTotalActivityTimeOfStage(int stageId)
        {
            ArgumentValidation.ThrowIfLess(stageId, 0, nameof(stageId));
            return repository.GetTotalActivityTimeOfStage(stageId);
        }

        public IEnumerable<User> GetUsers(PropertySelector<User> propertiesToInclude = null)
        {
            return repository.GetUsers(propertiesToInclude);            
        }        

        public Guid BeginGroupOperation()
        {            
            return Guid.NewGuid();
        }

        public void EndGroupOperation(Guid operationId)
        {
            ArgumentValidation.ThrowIf(
                operationId.Equals(Guid.Empty), 
                () => new[] { $"Argument '{operationId}' has invalid value: {operationId.ToString()}." });

            // empty
        }

        public void Dispose()
        {            
            repository = null;
        }

        public void RemoveTaskFromStage(int taskId, int stageId)
        {
            ArgumentValidation.ThrowIfLess(taskId, 0, nameof(taskId));
            ArgumentValidation.ThrowIfLess(stageId, 0, nameof(stageId));
            repository.RemoveTaskFromStage(taskId, stageId);
        }

        public void SetTaskStatus(int taskId, Status newStatus)
        {
            ArgumentValidation.ThrowIfLess(taskId, 0, nameof(taskId));
            repository.SetTaskStatus(taskId, newStatus);
        }

        public void Update(Stage stage)
        {
            ArgumentValidation.ThrowIfNull(stage, nameof(stage));
            repository.Update(stage);
        }

        public void Update(Activity activity)
        {
            ArgumentValidation.ThrowIfNull(activity, nameof(activity));
            repository.Update(activity);
        }

        public void Update(Task task)
        {
            ArgumentValidation.ThrowIfNull(task, nameof(task));
            repository.Update(task);
        }
    }
}
