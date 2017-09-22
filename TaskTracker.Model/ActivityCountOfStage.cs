namespace TaskTracker.Model
{
    using System;
    using System.Runtime.Serialization;

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
