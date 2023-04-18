using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
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
        List<IMyBatteryBlock> baseBatteries = new List<IMyBatteryBlock>();
        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
            GridTerminalSystem.GetBlocksOfType(baseBatteries, block => block.IsSameConstructAs(Me));
        }

        public void Main(string argument)
        {
            // Get all batteries on current grid.
            if (argument.Equals("reset"))
            {
                baseBatteries.Clear();
                GridTerminalSystem.GetBlocksOfType(baseBatteries, block => block.IsSameConstructAs(Me));
            }
            baseBatteries.Sort((b1, b2) => b1.CurrentStoredPower.CompareTo(b2.CurrentStoredPower));

            // Determine total max input of all connected batteries ready for fast charge.
            List<IMyBatteryBlock> connectedBatteries = new List<IMyBatteryBlock>();
            GridTerminalSystem.GetBlocksOfType(connectedBatteries, block => !block.IsSameConstructAs(Me) && BatteryNeedsFastCharging(block));
            float targetMaxInput = connectedBatteries.Select(battery => battery.MaxInput).Aggregate(0f, (sum, input) => sum += input);

            // Get All discharging batteries, set low power batteries to Auto and determine total output of remaining discharging batteries.
            List<IMyBatteryBlock> dischargingBaseBatteries = baseBatteries.FindAll(battery => battery.ChargeMode == ChargeMode.Discharge);
            dischargingBaseBatteries.FindAll(battery => GetPercentCharged(battery) < 5f).ForEach(battery => battery.ChargeMode = ChargeMode.Auto);
            dischargingBaseBatteries.RemoveAll(battery => battery.ChargeMode != ChargeMode.Discharge);

            float currentDischarge = dischargingBaseBatteries.Select(battery => battery.MaxOutput).Aggregate(0f, (sum, output) => sum += output);

            // If current discharge is less than the target switch more base batteries to discharge, otherwise reduce batteries on dischage mode until minimum require to hit target.
            if (targetMaxInput > currentDischarge)
            {
                IncreaseOutput(targetMaxInput, ref currentDischarge);

            }
            else if (CanReduceOutput(targetMaxInput, currentDischarge, dischargingBaseBatteries))
            {
                DecreaseOutput(targetMaxInput, dischargingBaseBatteries, currentDischarge);
            }
        }

        private void DecreaseOutput(float targetMaxInput, List<IMyBatteryBlock> dischargingBaseBatteries, float currentDischarge)
        {
            dischargingBaseBatteries.Sort((b1, b2) => b2.MaxOutput.CompareTo(b1.MaxOutput));

            int index = 0;
            while ((currentDischarge - dischargingBaseBatteries[index].MaxOutput) > targetMaxInput)
            {
                if (index < dischargingBaseBatteries.Count)
                {
                    dischargingBaseBatteries[index].ChargeMode = ChargeMode.Auto;
                }
                else
                {
                    break;
                }
            }
        }

        private void IncreaseOutput(float targetMaxInput, ref float currentDischarge)
        {
            List<IMyBatteryBlock> availableBatteries = baseBatteries.FindAll(battery => battery.ChargeMode == ChargeMode.Auto);
            availableBatteries.Sort((b1, b2) => b1.CurrentStoredPower.CompareTo(b2.CurrentStoredPower));
            int index = 0;
            while (targetMaxInput > currentDischarge)
            {
                if (index < availableBatteries.Count)
                {
                    IMyBatteryBlock battery = availableBatteries[index];
                    battery.ChargeMode = ChargeMode.Discharge;
                    currentDischarge += battery.MaxOutput;
                }
                else
                {
                    break;
                }
            }
        }

        private bool CanReduceOutput(float targetMaxInput, float currentDischarge, List<IMyBatteryBlock> dischargingBaseBatteries)
        {
            // If removing the smallest output from discharging batteries is still greater than targetMaxInput then we can turn off at least one battery.
            float minDischarge = dischargingBaseBatteries.Min(battery => battery.MaxOutput);
            return (currentDischarge - minDischarge) > targetMaxInput;
        }

        private Boolean BatteryNeedsFastCharging(IMyBatteryBlock battery) => battery.ChargeMode == ChargeMode.Recharge
                && battery.CurrentStoredPower < battery.MaxStoredPower;

        private float GetPercentCharged(IMyBatteryBlock battery)
        {
            return (battery.CurrentStoredPower / battery.MaxStoredPower) * 100f;
        }
    }
}
