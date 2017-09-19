using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace TaskTracker.Filters
{
    [Serializable]
    [DataContract]
    public class TaskFilter
    {
        public TaskFilter()
        { }

        [DataMember]
        public List<string> Statuses { get; set; }

        [DataMember]
        public List<string> Projects { get; set; }

        [DataMember]
        public List<string> Priorities { get; set; }
    }
}
