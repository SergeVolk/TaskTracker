using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace TaskTracker.Filters
{
    /// <summary>
    /// This class defines criterios for task filtering.
    /// It is currently used in the task-query methods of IRepository.
    /// This class's properties define tasks with what parameters should be included in the result.
    /// E.g. if Statuses={Open, Closed}, it means only open and closed tasks are requested.
    /// In case a property is nil, it has no any effect on the filtering.
    /// In case a property is an empty list - no tasks will be returned (regardless of other properties)
    /// </summary>
    /// <remarks>
    /// Most likely in future this class will be redesigned or removed due to the unstable approach of transfering enum values. 
    /// </remarks>
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
