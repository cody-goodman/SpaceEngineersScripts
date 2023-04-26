using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        int ALTITUDE_TRIGGER_NAME_PARAM = 0;
        int MERGE_BLOCK_NAME_PARAM = 1;
        int TIMER_BLOCK_NAME_PARAM = 2;
        int PARACHUTE_TRIGGER_VALUE_PARAM = 3;

        public void Main(string argument)
        {
            string[] args = argument.Split(';');

            // List<IMyBatteryBlock> beaconBatteries = new List<IMyBatteryBlock>();
            // GridTerminalSystem.GetBlocksOfType(beaconBatteries, block => block.IsSameConstructAs(Me) && block.CustomName.StartsWith(args[BEACON_BATTERY_PREFIX_PARAM]));
            IMyShipMergeBlock mergeBlock = (IMyShipMergeBlock)GridTerminalSystem.GetBlockWithName(args[MERGE_BLOCK_NAME_PARAM]);
            IMyProgrammableBlock altitudeTrigger = (IMyProgrammableBlock)GridTerminalSystem.GetBlockWithName(args[ALTITUDE_TRIGGER_NAME_PARAM]);
            IMyTimerBlock timerBlock = (IMyTimerBlock)GridTerminalSystem.GetBlockWithName(args[TIMER_BLOCK_NAME_PARAM]);

            altitudeTrigger.TryRun(args[TIMER_BLOCK_NAME_PARAM] + ";" + args[PARACHUTE_TRIGGER_VALUE_PARAM]);
            mergeBlock.Enabled = false;

        }
    }
}
