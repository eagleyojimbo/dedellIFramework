﻿using Agent.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agent.Commands
{
    public class Run : AgentCommand
    {
        public override string Name => "run";

        public override string Execute(AgentTask task)
        {
            var binaryName = task.Arguments[0];
            var args = string.Join(" ", task.Arguments.Skip(1));

            return Internals.Execute.ExecuteCommand(binaryName, args);
        }
    }
}
