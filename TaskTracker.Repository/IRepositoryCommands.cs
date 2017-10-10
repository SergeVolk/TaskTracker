using System;
using System.Collections.Generic;

using TaskTracker.Model;

namespace TaskTracker.Repository
{
    /// <summary>
    /// This interface contains methods that allow executing commands on repository.    
    /// </summary>
    /// <remarks>
    /// Currently, it contains methods for all model entities.
    /// </remarks>
    public interface IRepositoryCommands
    {
        /// <summary>
        /// Add the provided task to the repository.
        /// </summary>
        /// <remarks>
        /// The task Id must be set to 0. The repository is responsible for generating a proper Id.
        /// The task object is not updated with the generated Id.
        /// </remarks>
        void Add(Task task);

        /// <summary>
        /// Add the provided activity to the repository.
        /// </summary>
        /// <remarks>
        /// The activity Id must be set to 0. The repository is responsible for generating a proper Id.
        /// The activity object is not updated with the generated Id.
        /// </remarks>
        void Add(Activity activity);

        /// <summary>
        /// Add the provided stage to the repository.
        /// </summary>
        /// <remarks>
        /// The stage Id must be set to 0. The repository is responsible for generating a proper Id.
        /// The stage object is not updated with the generated Id.
        /// </remarks>
        void Add(Stage stage);

        /// <summary>
        /// Add the provided projects to the repository.
        /// </summary>
        /// <remarks>
        /// Ids of the provided projects must be set to 0. The repository is responsible for generating proper Ids.
        /// The objects are not updated with the generated Ids.
        void Add(IEnumerable<Project> projects);

        /// <summary>
        /// Add the provided users to the repository.
        /// </summary>
        /// <remarks>
        /// Ids of the provided users must be set to 0. The repository is responsible for generating proper Ids.
        /// The objects are not updated with the generated Ids.
        void Add(IEnumerable<User> users);

        /// <summary>
        /// Add the provided task types to the repository.
        /// </summary>
        /// <remarks>
        /// Ids of the provided task types must be set to 0. The repository is responsible for generating proper Ids.
        /// The objects are not updated with the generated Ids.
        void Add(IEnumerable<TaskType> taskTypes);

        /// <summary>
        /// Update the stored task using the provided object.
        /// </summary>
        /// <remarks>
        /// The method updates all properties of the stored task with the values from the provided object.
        /// In case there is no task in the repository having the same Id as the provided object, an exception will be thrown.
        /// </remarks>
        void Update(Task task);

        /// <summary>
        /// Update the stored activity using the provided object.
        /// </summary>
        /// <remarks>
        /// The method updates all properties of the stored activity with the values from the provided object.
        /// In case there is no activity in the repository having the same Id as the provided object, an exception will be thrown.
        /// </remarks>
        void Update(Activity activity);

        /// <summary>
        /// Update the stored stage using the provided object.
        /// </summary>
        /// <remarks>
        /// The method updates all properties of the stored stage with the values from the provided object.
        /// In case there is no stage in the repository having the same Id as the provided object, an exception will be thrown.
        /// </remarks>
        void Update(Stage stage);

        /// <summary>
        /// Update status of the task defined by the provided Id.
        /// </summary>
        /// <remarks>
        /// In case there is no task corresponding to the Id, an exception will be thrown.
        /// </remarks>
        void SetTaskStatus(int taskId, Status newStatus);

        /// <summary>
        /// Add the task defined by the provided "taskId" to the list of tasks assigned to the stage defined by the provided "stageId".
        /// </summary>
        /// <remarks>
        /// In case the task or stage identified by the provided Id-s, an exception will be thrown.
        /// </remarks>
        void AddTaskToStage(int taskId, int stageId);

        /// <summary>
        /// Remove the task defined by the provided "taskId" from the list of tasks assigned to the stage defined by the provided "stageId".
        /// </summary>
        /// <remarks>
        /// In case the task or stage identified by the provided Id-s, an exception will be thrown.
        /// </remarks>
        void RemoveTaskFromStage(int taskId, int stageId);
    }
}
