using System;
using System.Runtime.Serialization;

namespace TaskTracker.Model
{
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
