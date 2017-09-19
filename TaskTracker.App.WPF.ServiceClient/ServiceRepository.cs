using System;
using System.Collections.Generic;

using TaskTracker.Model;
using TaskTracker.Repository;
using TaskTracker.Service;
using TaskTracker.SyntaxUtils;
using TaskTracker.ExceptionUtils;
using TaskTracker.Filters;

namespace TaskTracker.App.WPF.ServiceClient
{
    public class ServiceRepository : IRepository
    {
        private ITaskTrackerService service;

        public ServiceRepository(ITaskTrackerService service)
        {
            ArgumentValidation.ThrowIfNull(service, nameof(service));
            this.service = service;
        }        

        public void Add(Activity activity)
        {
            ArgumentValidation.ThrowIfNull(activity, nameof(activity));
            service.Add(activity);
        }

        public void Add(Stage stage)
        {
            ArgumentValidation.ThrowIfNull(stage, nameof(stage));
            service.Add(stage);
        }

        public void Add(Task task)
        {
            ArgumentValidation.ThrowIfNull(task, nameof(task));
            service.Add(task);
        }

        public void AddTaskToStage(int taskId, int stageId)
        {
            ArgumentValidation.ThrowIfLess(taskId, 0, nameof(taskId));
            ArgumentValidation.ThrowIfLess(stageId, 0, nameof(stageId));
            service.AddTaskToStage(taskId, stageId);
        }

        public Stage FindStage(int stageId, PropertySelector<Stage> propertiesToInclude = null)
        {
            ArgumentValidation.ThrowIfLess(stageId, 0, nameof(stageId));
            return service.FindStage(stageId, propertiesToInclude);
        }

        public Task FindTask(int taskId, PropertySelector<Task> propertiesToInclude = null)
        {
            ArgumentValidation.ThrowIfLess(taskId, 0, nameof(taskId));
            return service.FindTask(taskId, propertiesToInclude);
        }

        public TaskType FindTaskType(int taskTypeId)
        {
            ArgumentValidation.ThrowIfLess(taskTypeId, 0, nameof(taskTypeId));
            return service.FindTaskType(taskTypeId);
        }

        public IEnumerable<Task> GetOpenTasksOfProject(int projectId, PropertySelector<Task> propertiesToInclude = null)
        {
            ArgumentValidation.ThrowIfLess(projectId, 0, nameof(projectId));
            return service.GetOpenTasksOfProject(projectId, propertiesToInclude);
        }

        public IEnumerable<Task> GetOpenTasksOfUser(int userId, PropertySelector<Task> propertiesToInclude = null)
        {
            ArgumentValidation.ThrowIfLess(userId, 0, nameof(userId));
            return service.GetOpenTasksOfUser(userId, propertiesToInclude);
        }

        public IEnumerable<Project> GetProjects(PropertySelector<Project> propertiesToInclude = null)
        {
            return service.GetProjects(propertiesToInclude);
        }

        public IEnumerable<Stage> GetStages(int level, PropertySelector<Stage> propertiesToInclude = null, bool applySelectionToEntireGraph = false)
        {
            ArgumentValidation.ThrowIfLess(level, 0, nameof(level));
            return service.GetStages(level, propertiesToInclude, applySelectionToEntireGraph);
        }

        public IEnumerable<Tuple<Stage, int>> GetStagesWithMaxActivities(int stageLimit, PropertySelector<Stage> propertiesToInclude = null)
        {
            ArgumentValidation.ThrowIfLess(stageLimit, 0, nameof(stageLimit));
            return service.GetStagesWithMaxActivities(stageLimit, propertiesToInclude);
        }

        public IEnumerable<Tuple<Stage, int>> GetStagesWithMaxTasks(int stageLimit, PropertySelector<Stage> propertiesToInclude = null)
        {
            ArgumentValidation.ThrowIfLess(stageLimit, 0, nameof(stageLimit));
            return service.GetStagesWithMaxTasks(stageLimit, propertiesToInclude);
        }

        public IEnumerable<Task> GetTasks(TaskFilter filter = null, PropertySelector<Task> sel = null)
        {
            return service.GetTasks(filter, sel);
        }

        public IEnumerable<TaskType> GetTaskTypes()
        {
            return service.GetTaskTypes();
        }

        public double GetTotalActivityTimeOfStage(int stageId)
        {
            ArgumentValidation.ThrowIfLess(stageId, 0, nameof(stageId));
            return service.GetTotalActivityTimeOfStage(stageId);
        }

        public IEnumerable<User> GetUsers(PropertySelector<User> propertiesToInclude = null)
        {
            return service.GetUsers(propertiesToInclude);
        }

        public void GroupOperations(RepositoryOperations operations)
        {
            ArgumentValidation.ThrowIfNull(operations, nameof(operations));
            var opId = service.BeginGroupOperation();
            try
            {
                operations(this);
            }
            finally
            {
                service.EndGroupOperation(opId);
            }            
        }

        public void RemoveTaskFromStage(int taskId, int stageId)
        {
            ArgumentValidation.ThrowIfLess(taskId, 0, nameof(taskId));
            ArgumentValidation.ThrowIfLess(stageId, 0, nameof(stageId));
            service.RemoveTaskFromStage(taskId, stageId);
        }

        public void SetTaskStatus(int taskId, Status newStatus)
        {
            ArgumentValidation.ThrowIfLess(taskId, 0, nameof(taskId));
            service.SetTaskStatus(taskId, newStatus);
        }

        public void Update(Stage stage)
        {
            ArgumentValidation.ThrowIfNull(stage, nameof(stage));
            service.Update(stage);
        }

        public void Update(Activity activity)
        {
            ArgumentValidation.ThrowIfNull(activity, nameof(activity));
            service.Update(activity);
        }

        public void Update(Task task)
        {
            ArgumentValidation.ThrowIfNull(task, nameof(task));
            service.Update(task);
        }
    }
}
