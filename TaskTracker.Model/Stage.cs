using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace TaskTracker.Model
{
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

        [DataMember]
        public int Level { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public string Name { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [DataMember]
        public virtual IList<Task> Task { get; set; }

        [DataMember]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual IList<Stage> SubStages { get; set; }

        [DataMember]
        public virtual Stage ParentStage { get; set; }
    }
}
