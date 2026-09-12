using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UWGame.Control.Commands
{
    /// <summary>
    /// receives commands, saves them in a queue, executes them, logs the result...
    /// </summary>
    public class CommandInvoker
    {

        private List<Command> commands = new List<Command>();

        public void Execute(Command command)
        {
            command.Execute(true);
        }

        public void Store(Command command)
        {
            commands.Add(command); // not sure if neeeded...
        }


    }
}
