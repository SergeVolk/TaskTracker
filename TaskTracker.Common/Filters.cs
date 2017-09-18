using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TaskTracker.Common
{
    [Serializable]
    [DataContract]
    public class TaskFilter
    {
        public TaskFilter()
        {}

        [DataMember]
        public List<string> Statuses { get; set; }

        [DataMember]
        public List<string> Projects { get; set; }

        [DataMember]
        public List<string> Priorities { get; set; }
    }
}
