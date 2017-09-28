using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace TaskTracker.Model
{
    /// <summary>
    /// This class contains info about an user of the TaskTracker system.
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public partial class User
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public User()
        {
            this.CreatedTask = new List<Task>(); 
            this.Task = new List<Task>();
            this.Activity = new List<Activity>();
        }

        [DataMember]
        public int Id { get; set; }

        /// <summary>
        /// Defines user's name.
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Contains tasks created by this user.
        /// </summary>
        [DataMember]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual IList<Task> CreatedTask { get; set; }

        /// <summary>
        /// Contains tasks assigned to this user.
        /// </summary>
        [DataMember]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual IList<Task> Task { get; set; }
        
        /// <summary>
        /// Contains list of current activities this user is working on.
        /// Completed activities are not included.
        /// </summary>
        [DataMember]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual IList<Activity> Activity { get; set; }
    }
}
