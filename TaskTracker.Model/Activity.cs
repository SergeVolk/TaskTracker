using System;
using System.Runtime.Serialization;

namespace TaskTracker.Model
{
    /// <summary>
    /// This class represents a piece of work (or activity) executing or executed by an user as part of a task assigned to him.
    /// It includes such data as start- and end- time, activity's description, and references to the target Task and assigned User. 
    /// </summary>    
    /// <remarks>
    /// In case the property EndTime is null, it means that the activity is not yet completed. Otherwise, it is.
    /// </remarks>
    [Serializable]
    [DataContract(IsReference = true)]
    public partial class Activity
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public DateTime StartTime { get; set; }

        [DataMember]
        public DateTime? EndTime { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public virtual Task Task { get; set; }

        [DataMember]
        public virtual User User { get; set; }
    }
}
