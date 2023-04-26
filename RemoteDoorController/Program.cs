using Sandbox.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        MyCommandLine _commandLine = new MyCommandLine();
        string BROADCAST_TAG = "rcdoors";
        public void Main(string argument, UpdateType updateSource)
        {
            if (_commandLine.TryParse(argument)) {
                if (_commandLine.ArgumentCount != 1)
                {
                    Echo("Invalid number of arguments. Expected 1 but received: " + _commandLine.ArgumentCount);
                }
                string doorName = _commandLine.Argument(0);
                string message = _commandLine.Switch("single") ? "single" : "group";
                if (_commandLine.Switch("open"))
                    message = message + ";" + doorName + ";open";
                else if (_commandLine.Switch("close"))
                    message = message + ";" + doorName + ";close";
                else
                {
                    message = message + ";" + doorName + ";toggle";
                }
                Echo("Sending Message: " + message);
                IGC.SendBroadcastMessage(BROADCAST_TAG, message);
            }
        }
    }
}
