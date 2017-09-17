//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TaskTracker.Model
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Linq;

    [Serializable]
    [DataContract(IsReference = true)]
    public partial class Task
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Task()
        {
            this.Activity = new HashSet<Activity>().ToList();
            this.Stage = new HashSet<Stage>().ToList();
        }

        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Summary { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public Priority Priority { get; set; }
        public Nullable<double> Estimation { get; set; }

        [DataMember]
        public Status Status { get; set; }

        [DataMember]
        public int TaskTypeId { get; set; }

        [DataMember]
        public virtual Project Project { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [DataMember]
        public virtual List<Activity> Activity { get; set; }

        [DataMember]
        public virtual User Creator { get; set; }

        [DataMember]
        public virtual User Assignee { get; set; }

        [DataMember]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual List<Stage> Stage { get; set; }
    }
}
