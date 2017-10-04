using System;
using System.Collections.Generic;

using TaskTracker.Filters;
using TaskTracker.Model;
using TaskTracker.SyntaxUtils;

namespace TaskTracker.Repository
{
    /// <summary>
    /// This interface contains methods for performing queries to repository.    
    /// </summary>
    /// <remarks>
    /// Currently, it contains query methods for all model entities.
    /// </remarks>
    public interface IRepositoryQueries
    {
        /// <summary>
        /// Returns all users from the repository.
        /// </summary>
        /// <param name="propertiesToInclude">Defines properties of users that must be initialized.</param>
        /// <remarks>
        /// The programmer should specify what properties are required using the parameter <paramref name="propertiesToInclude"/>.
        /// If this parameter is null it is not guaranteed that all properties of returned Users are initialized.
        /// </remarks>
        IEnumerable<User> GetUsers(PropertySelector<User> propertiesToInclude = null);

        /// <summary>
        /// Returns all projects from the repository.
        /// </summary>
        /// <param name="propertiesToInclude">Defines properties of projects that must be initialized.</param>
        /// <remarks>
        /// The programmer should specify what properties are required using the parameter <paramref name="propertiesToInclude"/>.
        /// If this parameter is null it is not guaranteed that all properties of returned projects are initialized.
        /// </remarks>
        IEnumerable<Project> GetProjects(PropertySelector<Project> propertiesToInclude = null);

        /// <summary>
        /// Returns all task-types from the repository.
        /// </summary>
        IEnumerable<TaskType> GetTaskTypes();

        /// <summary>
        /// Returns tasks allowed by the filter from the repository.
        /// </summary>
        /// <param name="filter">Defines what tasks are returned. If null, the method returns all tasks.</param>
        /// <param name="propertiesToInclude">Defines properties of tasks that must be initialized.</param>
        /// <remarks>
        /// The programmer should specify what properties are required using the parameter <paramref name="propertiesToInclude"/>.
        /// If this parameter is null it is not guaranteed that all properties of returned tasks are initialized.
        /// </remarks>
        IEnumerable<Task> GetTasks(TaskFilter filter = null, PropertySelector<Task> sel = null);

        /// <summary>
        /// Returns tasks having status "Open" and assigned to the provided user.
        /// </summary>
        /// <param name="userId">Defines user whose open tasks are returned.</param>
        /// <param name="propertiesToInclude">Defines properties of tasks that must be initialized.</param>
        /// <remarks>
        /// The programmer should specify what properties are required using the parameter <paramref name="propertiesToInclude"/>.
        /// If this parameter is null it is not guaranteed that all properties of returned tasks are initialized.
        /// </remarks>
        IEnumerable<Task> GetOpenTasksOfUser(int userId, PropertySelector<Task> propertiesToInclude = null);

        /// <summary>
        /// Returns tasks having status "Open" and assigned to the provided project.
        /// </summary>
        /// <param name="projectId">Defines project whose open tasks are returned.</param>
        /// <param name="propertiesToInclude">Defines properties of tasks that must be initialized.</param>
        /// <remarks>
        /// The programmer should specify what properties are required using the parameter <paramref name="propertiesToInclude"/>.
        /// If this parameter is null it is not guaranteed that all properties of returned tasks are initialized.
        /// </remarks>
        IEnumerable<Task> GetOpenTasksOfProject(int projectId, PropertySelector<Task> propertiesToInclude = null);

        /// <summary>
        /// Returns stages of the specified level from the repository.
        /// </summary>
        /// <param name="level">Defines stages of what level are returned. If 0, the top level stages are returned.</param>
        /// <param name="propertiesToInclude">Defines properties of stages that must be initialized.</param>
        /// <param name="applySelectionToEntireGraph">
        /// If true, properties of all sub-stages are initialized according to the parameter <paramref name="propertiesToInclude"/>.
        /// If false, any initialization of sub-stages are not guaranteed.
        /// </param>
        /// <remarks>
        /// The programmer should specify what properties are required using the parameter <paramref name="propertiesToInclude"/>.
        /// If this parameter is null it is not guaranteed that all properties of returned stages are initialized.
        /// </remarks>
        IEnumerable<Stage> GetStages(int level, PropertySelector<Stage> propertiesToInclude = null, bool applySelectionToEntireGraph = false);

        /// <summary>
        /// Returns top of stages having maximal number of activities.
        /// </summary>
        /// <param name="stageLimit">Defines the limit of stages to be returned.</param>
        /// <param name="propertiesToInclude">Defines properties of stages that must be initialized.</param>
        /// <returns>Returns pairs of stage with the number of activities it contains.</returns>
        /// <remarks>
        /// The programmer should specify what properties are required using the parameter <paramref name="propertiesToInclude"/>.
        /// If this parameter is null it is not guaranteed that all properties of returned stages are initialized.
        /// </remarks>
        IEnumerable<Tuple<Stage, int>> GetStagesWithMaxActivities(int stageLimit, PropertySelector<Stage> propertiesToInclude = null);

        /// <summary>
        /// Returns top of stages having maximal number of tasks.
        /// </summary>
        /// <param name="stageLimit">Defines the limit of stages to be returned.</param>
        /// <param name="propertiesToInclude">Defines properties of stages that must be initialized.</param>
        /// <returns>Returns pairs of stage with the number of tasks it contains.</returns>
        /// <remarks>
        /// The programmer should specify what properties are required using the parameter <paramref name="propertiesToInclude"/>.
        /// If this parameter is null it is not guaranteed that all properties of returned stages are initialized.
        /// </remarks>
        IEnumerable<Tuple<Stage, int>> GetStagesWithMaxTasks(int stageLimit, PropertySelector<Stage> propertiesToInclude = null);

        /// <summary>
        /// Returns the total time (in minutes) of all activities performed for the provided stage.
        /// </summary>
        /// <param name="stageId">Defines the stage whose total time of activities are returned.</param>
        /// <remarks>
        /// For incomplete activities, this method calculates the total time passed to the moment of the call.
        /// </remarks>
        double GetTotalActivityTimeOfStage(int stageId);

        /// <summary>
        /// Returns existing task by Id or throw exception if not or more than one found.
        /// </summary>
        /// <param name="taskId">Id of the task to be found.</param>
        /// <param name="propertiesToInclude">Defines properties of task that must be initialized.</param>
        /// <remarks>
        /// The programmer should specify what properties are required using the parameter <paramref name="propertiesToInclude"/>.
        /// If this parameter is null it is not guaranteed that all properties of returned task are initialized.
        /// </remarks>
        Task FindTask(int taskId, PropertySelector<Task> propertiesToInclude = null);

        /// <summary>
        /// Returns existing stage by Id or throw exception if not or more than one found.
        /// </summary>
        /// <param name="stageId">Id of the stage to be found.</param>
        /// <param name="propertiesToInclude">Defines properties of stage that must be initialized.</param>
        /// <remarks>
        /// The programmer should specify what properties are required using the parameter <paramref name="propertiesToInclude"/>.
        /// If this parameter is null it is not guaranteed that all properties of returned stage are initialized.
        /// </remarks>
        Stage FindStage(int stageId, PropertySelector<Stage> propertiesToInclude = null);

        /// <summary>
        /// Returns existing Task Type by Id or throw exception if not or more than one found.
        /// </summary>
        /// <param name="taskTypeId">Id of the task type to be found.</param>
        TaskType FindTaskType(int taskTypeId);
    }
}
