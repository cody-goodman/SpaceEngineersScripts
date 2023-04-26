using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;
using VRage.Game.ModAPI.Ingame.Utilities;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        MyCommandLine _commandLine = new MyCommandLine();

        public void Main(string argument)
        {
            if (_commandLine.TryParse(argument) && _commandLine.ArgumentCount != 1)
            {
                List<IMyAirtightHangarDoor> hangarDoors = new List<IMyAirtightHangarDoor>();
                GridTerminalSystem.GetBlockGroupWithName(_commandLine.Argument(0)).GetBlocksOfType(hangarDoors);
                hangarDoors.ForEach(hangarDoor =>
                {
                    if (_commandLine.Switch("open"))
                    {
                        hangarDoor.OpenDoor();
                    }
                    else if (_commandLine.Switch("close"))
                    {
                        hangarDoor.CloseDoor();
                    }
                    else
                    {
                        hangarDoor.ToggleDoor();
                    }
                });
            }
            else
            {
                Echo("Invalid Argument: " + argument);
            }
        }
    }
}
