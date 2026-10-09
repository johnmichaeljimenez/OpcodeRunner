using YesodScript.Engine.Core;

namespace YesodScript.Engine.Commands
{
    public class Stop : Command
    {
        [CommandParameter]
        private string scriptId = "";

        public override void OnEnter()
        {
            VM.Stop(scriptId);
        }
    }
}