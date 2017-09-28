using System;

namespace TaskTracker.Model
{    
    /// <summary>
    /// This class is intended for additional, custom categorization of tasks by introducing a type of a task.
    /// Each Task includes a property of this type. 
    /// The programmer can define own task types according to his needs.
    /// </summary>
    /// <remarks>
    /// Currently, there are 2 task types in the TaskTracker applications: 
    /// - "Accomplishable": tasks that having a time point when one determine that it is completed.
    /// - "Continuous": tasks that do not have an end-point.
    /// </remarks>
    public partial class TaskType
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
