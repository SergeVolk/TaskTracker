namespace TaskTracker.Model
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// This class contains info about a number of activities performed in a stage.
    /// It is used to return result from some stored procedures.
    /// </summary>
    [Serializable]
    [DataContract]
    public partial class ActivityCountOfStage
    {
        [DataMember]
        public int? StageId { get; set; }

        [DataMember]
        public int? ActivityCount { get; set; }
    }
}
