using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Models
{
    public abstract class CommModule
    {
        // Abstract method to start the communication module. Implemented by HttpCommModule
        public abstract Task Start();

        // Abtract method to stop the communication module in case of a cancel request. Implemented by HttpCommModule
        public abstract void Stop();
        
        // Holds the metadata about the agent, to be
        // initialized during the communication module setup
        protected AgentMetadata AgentMetadata;

        // Concurrent queues to handle inbound and outbound tasks
        protected ConcurrentQueue<AgentTask> Inbound = new ConcurrentQueue<AgentTask>();
        protected ConcurrentQueue<AgentTaskResult> Outbound = new ConcurrentQueue<AgentTaskResult>();

        // Initializes the communication module with the provided agent metadata (generated in program.cs)
        public virtual void Init(AgentMetadata metadata)
        {
            AgentMetadata = metadata;
        }

        // Attempts to retrieve all tasks from the inbound queue. Returns true if there were tasks. False otherwise.
        public bool RecvData(out IEnumerable<AgentTask> tasks)
        {
            if (Inbound.IsEmpty)
            {
                tasks = null;
                return false;
            }

            var list = new List<AgentTask>();
            while (Inbound.TryDequeue(out var task))
            {
                list.Add(task);
            }

            tasks = list;
            return true;
        }
        // Adds data to the Outbound Queue
        public void SendData(AgentTaskResult result)
        {
            Outbound.Enqueue(result);
        }

        protected IEnumerable<AgentTaskResult> GetOutbound()
        {
            var outbound = new List<AgentTaskResult>();
            
            while (Outbound.TryDequeue(out var taskResult))
            {
                outbound.Add(taskResult);
            }
            return outbound;
        }
    }
}
