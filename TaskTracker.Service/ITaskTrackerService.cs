using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using TaskTracker.Common;
using TaskTracker.Model;

namespace TaskTracker.Service
{
    [ServiceContract]
    public interface ITaskTrackerService
    {
        [OperationContract]
        IEnumerable<User> GetUsers(SelectedProperties<User> propertiesToInclude = null);

        [OperationContract]
        IEnumerable<Project> GetProjects(SelectedProperties<Project> propertiesToInclude = null);

        [OperationContract]
        IEnumerable<TaskType> GetTaskTypes();
            
        [OperationContract]
        IEnumerable<Task> GetTasks(TaskFilter filter = null, SelectedProperties<Task> sel = null);
        
        [OperationContract]
        IEnumerable<Task> GetOpenTasksOfUser(int userId, SelectedProperties<Task> propertiesToInclude = null);

        [OperationContract]
        IEnumerable<Task> GetOpenTasksOfProject(int projectId, SelectedProperties<Task> propertiesToInclude = null);
        
        [OperationContract]
        IEnumerable<Stage> GetStages(int level, SelectedProperties<Stage> propertiesToInclude = null, bool applySelectionToEntireGraph = false);
        
        [OperationContract]
        IEnumerable<Tuple<Stage, int>> GetStagesWithMaxActivities(int stageLimit, SelectedProperties<Stage> propertiesToInclude = null);
        
        [OperationContract]
        IEnumerable<Tuple<Stage, int>> GetStagesWithMaxTasks(int stageLimit, SelectedProperties<Stage> propertiesToInclude = null);

        [OperationContract]
        double GetTotalActivityTimeOfStage(int stageId);
        
        [OperationContract]
        Task FindTask(int taskId, SelectedProperties<Task> propertiesToInclude = null);

        [OperationContract]
        Stage FindStage(int stageId, SelectedProperties<Stage> propertiesToInclude = null);
        
        [OperationContract]
        TaskType FindTaskType(int taskTypeId);
        
        [OperationContract(Name = "AddTask")]
        void Add(Task task);

        [OperationContract(Name = "AddActivity")]
        void Add(Activity activity);

        [OperationContract(Name = "AddStage")]
        void Add(Stage stage);
        
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
        Guid BeginGroupOperation();

        [OperationContract]
        void EndGroupOperation(Guid operationId);
    }
}
