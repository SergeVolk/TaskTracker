using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading;
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
            if (repository == null)
                throw new ArgumentNullException(nameof(repository));

            this.repository = repository;
            //this.groupOperations = new ThreadSafeList<Guid>();
        }

        public void Add(Activity activity)
        {
            repository.Add(activity);
        }

        public void Add(Stage stage)
        {
            repository.Add(stage);
        }

        public void Add(Task task)
        {
            repository.Add(task);
        }

        public void AddTaskToStage(int taskId, int stageId)
        {
            repository.AddTaskToStage(taskId, stageId);
        }

        public Stage FindStage(int stageId, SelectedProperties<Stage> propertiesToInclude = null)
        {
            return repository.FindStage(stageId, propertiesToInclude);
        }

        public Task FindTask(int taskId, SelectedProperties<Task> propertiesToInclude = null)
        {
            return repository.FindTask(taskId, propertiesToInclude);
        }

        public TaskType FindTaskType(int taskTypeId)
        {
            return repository.FindTaskType(taskTypeId);
        }

        public IEnumerable<Task> GetOpenTasksOfProject(int projectId, SelectedProperties<Task> propertiesToInclude = null)
        {
            return repository.GetOpenTasksOfProject(projectId, propertiesToInclude);
        }

        public IEnumerable<Task> GetOpenTasksOfUser(int userId, SelectedProperties<Task> propertiesToInclude = null)
        {
            return repository.GetOpenTasksOfUser(userId, propertiesToInclude);
        }

        public IEnumerable<Project> GetProjects(SelectedProperties<Project> propertiesToInclude = null)
        {
            return repository.GetProjects(propertiesToInclude);
        }

        public IEnumerable<Stage> GetStages(int level, SelectedProperties<Stage> propertiesToInclude = null, bool applySelectionToEntireGraph = false)
        {
            return repository.GetStages(level, propertiesToInclude, applySelectionToEntireGraph);
        }

        public IEnumerable<Tuple<Stage, int>> GetStagesWithMaxActivities(int stageLimit, SelectedProperties<Stage> propertiesToInclude = null)
        {
            return repository.GetStagesWithMaxActivities(stageLimit, propertiesToInclude);
        }

        public IEnumerable<Tuple<Stage, int>> GetStagesWithMaxTasks(int stageLimit, SelectedProperties<Stage> propertiesToInclude = null)
        {
            return repository.GetStagesWithMaxTasks(stageLimit, propertiesToInclude);
        }        

        public IEnumerable<Task> GetTasks(TaskFilter filter = null, SelectedProperties<Task> sel = null)
        {
            return repository.GetTasks(filter, sel);
        }

        public IEnumerable<TaskType> GetTaskTypes()
        {
            return repository.GetTaskTypes();
        }

        public double GetTotalActivityTimeOfStage(int stageId)
        {
            return repository.GetTotalActivityTimeOfStage(stageId);
        }

        public IEnumerable<User> GetUsers(SelectedProperties<User> propertiesToInclude = null)
        {
            return repository.GetUsers(propertiesToInclude);            
        }        

        public Guid BeginGroupOperation()
        {            
            return Guid.NewGuid();
        }

        public void EndGroupOperation(Guid operationId)
        {            
        }

        public void Dispose()
        {            
            repository = null;
        }

        public void RemoveTaskFromStage(int taskId, int stageId)
        {
            repository.RemoveTaskFromStage(taskId, stageId);
        }

        public void SetTaskStatus(int taskId, Status newStatus)
        {
            repository.SetTaskStatus(taskId, newStatus);
        }

        public void Update(Stage stage)
        {
            repository.Update(stage);
        }

        public void Update(Activity activity)
        {
            repository.Update(activity);
        }

        public void Update(Task task)
        {
            repository.Update(task);
        }
    }
}
