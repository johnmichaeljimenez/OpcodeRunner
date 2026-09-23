using OpcodeEngine.Core;

namespace OpcodeEngine.Commands
{
    public class Stop : Command
    {
        [CommandParameter]
        private string scriptId = "";

        public override void OnEnter()
        {
            Engine.Stop(scriptId);
        }
    }
}