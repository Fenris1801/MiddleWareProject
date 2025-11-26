using System.Collections.Generic;

namespace HeavyClient.Commands
{
    public class CommandDispatcher
    {
        private readonly Dictionary<string, ICommand> _commands = new Dictionary<string, ICommand>();

        public void Register(string key, ICommand command)
        {
            _commands[key] = command;
        }

        public bool Execute(string key)
        {
            if (!_commands.TryGetValue(key, out var cmd))
                return false;

            cmd.Execute();
            return true;
        }
    }
}
