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
        List<IMyBatteryBlock> batteries = new List<IMyBatteryBlock>();
        List<IMyPowerProducer> powerProducers = new List<IMyPowerProducer>();
        List<IMyGasTank> hydrogenTanks = new List<IMyGasTank>();
        float maxPowerStorage;
        float maxHydrogenStorage;
        string status = "";
        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
            Init();
        }

        public void Main(string argument, UpdateType updateSource)
        {
            if (updateSource == UpdateType.Terminal && "reset".Equals(argument, StringComparison.OrdinalIgnoreCase))
            {
                Init();
            }

            float currentHydrogenStorage = (float)hydrogenTanks.Select(tank => tank.Capacity * tank.FilledRatio).Sum();
            float percentHydrogenStorage = currentHydrogenStorage / maxHydrogenStorage * 100;

            float currentPowerStorage = batteries.Select(battery => battery.CurrentStoredPower).Sum();
            float percentPowerStorage = currentPowerStorage / maxPowerStorage;
            if (percentPowerStorage < 0.5f)
            {
                powerProducers.ForEach(powerProducer => powerProducer.Enabled = true);
                status = "Running";
            }
            else if (percentPowerStorage > 0.95)
            {
                powerProducers.ForEach(powerProducer => powerProducer.Enabled = false);
                status = "OFF";
            }
            WriteStatus(status, percentPowerStorage, percentHydrogenStorage);

        }

        private void WriteStatus(string status, float percentPowerStorage, float percentHydrogenStorage)
        {
            IMyTextSurface surface = Me.GetSurface(0);
            surface.ContentType = ContentType.TEXT_AND_IMAGE;
            surface.FontSize = 1.5f;
            surface.Alignment = TextAlignment.CENTER;
            surface.WriteText("Stored Power: " + percentPowerStorage.ToString("n3") + "%" + "\n\nHydrogen Storage: " + percentHydrogenStorage.ToString("n3") + "%" + "\n\nPower Production: " + status);
        }

        private void Init()
        {
            GridTerminalSystem.GetBlocksOfType(batteries, block => block.IsSameConstructAs(Me));
            GridTerminalSystem.GetBlocksOfType(powerProducers, IsNonBatteryPowerProducer);
            GridTerminalSystem.GetBlocksOfType(hydrogenTanks, IsHydrogenTank);
            status = powerProducers.Any(block => block.Enabled) ? "RUNNING" : "OFF";
            maxPowerStorage = batteries.Select(battery => battery.MaxStoredPower).Sum();
            maxHydrogenStorage = hydrogenTanks.Select(tank => tank.Capacity).Sum();
        }

        private bool IsHydrogenTank(IMyGasTank gasTank)
        {
            return gasTank.BlockDefinition.SubtypeName.ToLower().Contains("hydrogen") && gasTank.IsSameConstructAs(Me);
        }

        private bool IsNonBatteryPowerProducer(IMyPowerProducer block)
        {
            return !block.BlockDefinition.SubtypeName.ToLower().Contains("battery") && block.IsSameConstructAs(Me);
        }
    }
}