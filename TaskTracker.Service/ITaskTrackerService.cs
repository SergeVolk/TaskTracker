using System;
using System.Collections.Generic;
using System.ServiceModel;

using TaskTracker.Model;
using TaskTracker.Filters;
using TaskTracker.SyntaxUtils;

namespace TaskTracker.Service
{
    [ServiceContract]
    public interface ITaskTrackerService : IDisposable
    {
        [OperationContract]
        IEnumerable<User> GetUsers(PropertySelector<User> propertiesToInclude = null);

        [OperationContract]
        IEnumerable<Project> GetProjects(PropertySelector<Project> propertiesToInclude = null);

        [OperationContract]
        IEnumerable<TaskType> GetTaskTypes();
            
        [OperationContract]
        IEnumerable<Task> GetTasks(TaskFilter filter = null, PropertySelector<Task> sel = null);
        
        [OperationContract]
        IEnumerable<Task> GetOpenTasksOfUser(int userId, PropertySelector<Task> propertiesToInclude = null);

        [OperationContract]
        IEnumerable<Task> GetOpenTasksOfProject(int projectId, PropertySelector<Task> propertiesToInclude = null);
        
        [OperationContract]
        IEnumerable<Stage> GetStages(int level, PropertySelector<Stage> propertiesToInclude = null, bool applySelectionToEntireGraph = false);
        
        [OperationContract]
        IEnumerable<Tuple<Stage, int>> GetStagesWithMaxActivities(int stageLimit, PropertySelector<Stage> propertiesToInclude = null);
        
        [OperationContract]
        IEnumerable<Tuple<Stage, int>> GetStagesWithMaxTasks(int stageLimit, PropertySelector<Stage> propertiesToInclude = null);

        [OperationContract]
        double GetTotalActivityTimeOfStage(int stageId);
        
        [OperationContract]
        Task FindTask(int taskId, PropertySelector<Task> propertiesToInclude = null);

        [OperationContract]
        Stage FindStage(int stageId, PropertySelector<Stage> propertiesToInclude = null);
        
        [OperationContract]
        TaskType FindTaskType(int taskTypeId);
        
        [OperationContract(Name = "AddTask")]
        void Add(Task task);

        [OperationContract(Name = "AddActivity")]
        void Add(Activity activity);

        [OperationContract(Name = "AddStage")]
        void Add(Stage stage);

        [OperationContract(Name = "AddProjects")]
        void Add(IEnumerable<Project> projects);

        [OperationContract(Name = "AddUsers")]
        void Add(IEnumerable<User> users);

        [OperationContract(Name = "AddTaskTypes")]
        void Add(IEnumerable<TaskType> taskTypes);
        
        [OperationContract(Name = "UpdateTask")]
        void Update(Task task);
        
        [OperationContract(Name = "UpdateActivity")]
        void Update(Activity activity);
        
        [OperationContract(Name = "UpdateStage")]
        void Update(Stage stage);
        
        [OperationContract]
        void SetTaskStatus(int taskId, Status newStatus);
            
        [OperationContract]
        void AddTaskToStage(int taskId, int stageId);
        
        [OperationContract]
        void RemoveTaskFromStage(int taskId, int stageId);
        
        [OperationContract]
        void BeginTransaction();

        [OperationContract]
        void CommitTransaction();

        [OperationContract]
        void RollbackTransaction();
    }
}
