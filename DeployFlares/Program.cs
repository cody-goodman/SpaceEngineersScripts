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
            if (_commandLine.TryParse(argument))
            {

                List<IMyBlockGroup> flares = new List<IMyBlockGroup>();
                GridTerminalSystem.GetBlockGroups(flares, group => group.Name.Contains(_commandLine.Argument(0)));
                if (_commandLine.Switch("-all"))
                {
                    flares.ForEach(DeployFlare);
                }
                else if (flares.Count > 0)
                {
                    DeployFlare(flares[0]);
                }
                else
                {
                    Echo("No Flares Found");
                }
            }
            else
            {
                Echo("Invalid Argument String: " + argument);
            }
        }

        private void DeployFlare(IMyBlockGroup flare)
        {
            List<IMyThrust> thrusters = new List<IMyThrust>();
            List<IMyShipMergeBlock> mergeBlocks = new List<IMyShipMergeBlock>();
            List<IMyBatteryBlock> batteries = new List<IMyBatteryBlock>();
            List<IMyDecoy> decoys = new List<IMyDecoy>();

            flare.GetBlocksOfType(thrusters);
            flare.GetBlocksOfType(mergeBlocks);
            flare.GetBlocksOfType(batteries);
            flare.GetBlocksOfType(decoys);

            batteries.ForEach(battery =>
            {
                battery.Enabled = true;
                battery.ChargeMode = ChargeMode.Auto;
            });
            thrusters.ForEach(thruster =>
            {
                thruster.Enabled = true;
                thruster.ThrustOverridePercentage = 1;
            });
            decoys.ForEach(decoy => decoy.Enabled = true);
            mergeBlocks.ForEach(mergeBlock => mergeBlock.Enabled = false);
        }
    }
}
