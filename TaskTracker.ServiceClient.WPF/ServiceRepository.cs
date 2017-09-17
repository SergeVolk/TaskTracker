using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
//using System.Threading.Tasks;
using TaskTracker.Common;
using TaskTracker.Model;
using TaskTracker.Repository;
using TaskTracker.Service;

namespace TaskTracker.ServiceClient.WPF
{
    public class ServiceRepository : IRepository
    {
        private ITaskTrackerService service;

        public ServiceRepository(ITaskTrackerService service)
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service));

            this.service = service;
        }        

        public void Add(Activity activity)
        {
            service.Add(activity);
        }

        public void Add(Stage stage)
        {
            service.Add(stage);
        }

        public void Add(Task task)
        {
            service.Add(task);
        }

        public void AddTaskToStage(int taskId, int stageId)
        {
            service.AddTaskToStage(taskId, stageId);
        }

        public Stage FindStage(int stageId, SelectedProperties<Stage> propertiesToInclude = null)
        {
            return service.FindStage(stageId, propertiesToInclude);
        }

        public Task FindTask(int taskId, SelectedProperties<Task> propertiesToInclude = null)
        {
            return service.FindTask(taskId, propertiesToInclude);
        }

        public TaskType FindTaskType(int taskTypeId)
        {
            return service.FindTaskType(taskTypeId);
        }

        public IEnumerable<Task> GetOpenTasksOfProject(int projectId, SelectedProperties<Task> propertiesToInclude = null)
        {
            return service.GetOpenTasksOfProject(projectId, propertiesToInclude);
        }

        public IEnumerable<Task> GetOpenTasksOfUser(int userId, SelectedProperties<Task> propertiesToInclude = null)
        {
            return service.GetOpenTasksOfUser(userId, propertiesToInclude);
        }

        public IEnumerable<Project> GetProjects(SelectedProperties<Project> propertiesToInclude = null)
        {
            return service.GetProjects(propertiesToInclude);
        }

        public IEnumerable<Stage> GetStages(int level, SelectedProperties<Stage> propertiesToInclude = null, bool applySelectionToEntireGraph = false)
        {
            return service.GetStages(level, propertiesToInclude, applySelectionToEntireGraph);
        }

        public IEnumerable<Tuple<Stage, int>> GetStagesWithMaxActivities(int stageLimit, SelectedProperties<Stage> propertiesToInclude = null)
        {
            return service.GetStagesWithMaxActivities(stageLimit, propertiesToInclude);
        }

        public IEnumerable<Tuple<Stage, int>> GetStagesWithMaxTasks(int stageLimit, SelectedProperties<Stage> propertiesToInclude = null)
        {         
            return service.GetStagesWithMaxTasks(stageLimit, propertiesToInclude);
        }


        public IEnumerable<Task> GetTasks(TaskFilter filter = null, SelectedProperties<Task> sel = null)
        {
            return service.GetTasks(filter, sel);
        }

        public IEnumerable<TaskType> GetTaskTypes()
        {
            return service.GetTaskTypes();
        }

        public double GetTotalActivityTimeOfStage(int stageId)
        {
            return service.GetTotalActivityTimeOfStage(stageId);
        }

        public IEnumerable<User> GetUsers(SelectedProperties<User> propertiesToInclude = null)
        {
            return service.GetUsers(propertiesToInclude);
        }

        public void GroupOperations(RepositoryOperations operations)
        {
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
            service.RemoveTaskFromStage(taskId, stageId);
        }

        public void SetTaskStatus(int taskId, Status newStatus)
        {
            service.SetTaskStatus(taskId, newStatus);
        }

        public void Update(Stage stage)
        {
            service.Update(stage);
        }

        public void Update(Activity activity)
        {
            service.Update(activity);
        }

        public void Update(Task task)
        {
            service.Update(task);
        }
    }
}
