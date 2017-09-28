using System;
using System.Runtime.Serialization;

namespace TaskTracker.Model
{
    /// <summary>
    /// This class contains info about a number of tasks assigned to a stage.
    /// It is used to return result from some stored procedures.
    /// </summary>
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
