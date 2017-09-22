using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;

namespace TaskTracker.Model
{
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

        [DataMember]
        public string Summary { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public Priority Priority { get; set; }
        public double? Estimation { get; set; }

        [DataMember]
        public Status Status { get; set; }

        [DataMember]
        public int TaskTypeId { get; set; }

        [DataMember]
        public virtual Project Project { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [DataMember]
        public virtual IList<Activity> Activity { get; set; }

        [DataMember]
        public virtual User Creator { get; set; }

        [DataMember]
        public virtual User Assignee { get; set; }

        [DataMember]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual IList<Stage> Stage { get; set; }
    }
}
