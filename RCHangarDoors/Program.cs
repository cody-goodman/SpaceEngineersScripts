using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

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
    