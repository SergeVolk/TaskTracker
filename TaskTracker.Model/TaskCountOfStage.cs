using System;
using System.Runtime.Serialization;

namespace TaskTracker.Model
{
    [Serializable]
    [DataContract]
    public partial class TaskCountOfStage
    {
        [DataMember]
        public int StageId { get; set; }

        [DataMember]
        public int? TaskCount { get; set; }
    }
}
