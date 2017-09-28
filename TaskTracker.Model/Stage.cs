using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace TaskTracker.Model
{
    /// <summary>
    /// This class is intended for time scheduling of tasks.
    /// It can be considered as some generalization of the term "Sprint": 
    /// - Time range of a stage is arbitrary and optional.
    /// - Stage supports any number of sub-stages.
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public partial class Stage
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Stage()
        {
            this.Task = new List<Task>(); 
            this.SubStages = new List<Stage>(); 
        }

        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public DateTime? StartTime { get; set; }

        [DataMember]
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// Defines the depth of the current stage relatively the root stage.
        /// The root stage has Level = 0
        /// </summary>
        [DataMember]
        public int Level { get; set; }

        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Defines a friendly name of the stage. E.g. "Week#01-Development"
        /// </summary>
        [DataMember]
        public string Name { get; set; }
    
        /// <summary>
        /// Contains tasks assigned to this stage.
        /// </summary>
        /// <remarks>
        /// Tasks of sub-stages are not included.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [DataMember]
        public virtual IList<Task> Task { get; set; }

        /// <summary>
        /// Contains child stages of this stage.
        /// </summary>
        /// <remarks>
        /// The model is not currently require that time interval of child tasks are fully included inside of time interval of their parent stage.
        /// The programmer should consider to implement some validation of time intervals in his code.
        /// </remarks>
        [DataMember]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual IList<Stage> SubStages { get; set; }

        /// <summary>
        /// Contains a reference to a parent stage.
        /// For the root stage, it is nil.
        /// </summary>
        [DataMember]
        public virtual Stage ParentStage { get; set; }
    }
}
