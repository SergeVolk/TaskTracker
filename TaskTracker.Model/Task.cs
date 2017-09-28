using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;

namespace TaskTracker.Model
{
    /// <summary>
    /// The central class of the TaskTracker model.
    /// It describes a well-defined amount of work that can be done by someone and has various attributes such as priority/status/etc.
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public partial class Task
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Task()
        {
            this.Activity = new List<Activity>();
            this.Stage = new List<Stage>();
        }

        [DataMember]
        public int Id { get; set; }

        /// <summary>
        /// Defines a short (in several words) description of the task.
        /// </summary>
        [DataMember]
        public string Summary { get; set; }

        /// <summary>
        /// Defines a full description of the task.
        /// </summary>
        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public Priority Priority { get; set; }

        /// <summary>
        /// Defines the current time estimation of the task in minutes. Optional.
        /// </summary>
        [DataMember]
        public double? Estimation { get; set; }

        [DataMember]
        public Status Status { get; set; }

        /// <summary>
        /// This property allows linking of the task with some category of tasks (or task type). 
        /// Each task type has own Id (See "TaskType" for more information).
        /// The property is set to Id of one of existing task types. 
        /// </summary>
        [DataMember]
        public int TaskTypeId { get; set; }

        /// <summary>
        /// Defines project of the task.
        /// </summary>
        [DataMember]
        public virtual Project Project { get; set; }

        /// <summary>
        /// Contains list of all activities executing or executed for this task.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [DataMember]
        public virtual IList<Activity> Activity { get; set; }

        /// <summary>
        /// Defines the user who has created this task.
        /// </summary>
        [DataMember]
        public virtual User Creator { get; set; }

        /// <summary>
        /// Defines the user this task is currently assigned to.
        /// </summary>
        [DataMember]
        public virtual User Assignee { get; set; }

        /// <summary>
        /// Defines the list of stages this task belongs to.
        /// A signle task can be scheduled for any number of stages. 
        /// The programmer is responsible to make the scheduling-logic consistent according to his needs.
        /// </summary>
        [DataMember]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual IList<Stage> Stage { get; set; }
    }
}
