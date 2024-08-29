using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Models
{
    [DataContract]
    // The fields have been deserialized as lower case, which is why the first letter is not capital.
    public class AgentTask
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }
        
        [DataMember(Name = "command")]
        public string Command { get; set; }
        
        [DataMember(Name = "arguments")]
        public string[] Arguments { get; set; }
        
        [DataMember(Name = "file")]
        public byte[] File { get; set; }
    }
}
