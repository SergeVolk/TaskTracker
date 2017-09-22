using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace TaskTracker.Model
{
    [Serializable]
    [DataContract(IsReference = true)]
    public partial class Project
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Project()
        {
            this.Task = new List<Task>(); 
        }

        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string ShortName { get; set; }

        [DataMember]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual IList<Task> Task { get; set; }
    }
}
