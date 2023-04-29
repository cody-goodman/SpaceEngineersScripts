using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame.Utilities;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        MyIni config = new MyIni();
        float MIN_HYDROGEN_STORAGE = 0f;
        float MIN_POWER_STORAGE = 50f;
        float GENERATOR_CUTOFF = 95f;
        int MAX_GENERATORS;

        List<IMyBatteryBlock> batteries = new List<IMyBatteryBlock>();
        List<IMyPowerProducer> generators = new List<IMyPowerProducer>();
        List<IMyGasTank> hydrogenTanks = new List<IMyGasTank>();
        float powerCapacity;
        float hydrogenCapacity;
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
            float percentHydrogenStorage = currentHydrogenStorage / hydrogenCapacity * 100;

            float currentPowerStorage = batteries.Select(battery => battery.CurrentStoredPower).Sum();
            float percentPowerStorage = (currentPowerStorage / powerCapacity) * 100;
            if (percentPowerStorage < MIN_POWER_STORAGE && percentHydrogenStorage > MIN_HYDROGEN_STORAGE)
            {
                EnableGenerators();
                status = "Generators: " + generators.FindAll(block => block.Enabled).Count;
            }
            else if (percentPowerStorage > GENERATOR_CUTOFF)
            {
                generators.ForEach(generator => generator.Enabled = false);
                status = "Battery";
            }
            else if (percentHydrogenStorage < MIN_HYDROGEN_STORAGE)
            {
                generators.ForEach(generator => generator.Enabled = false);
                status = "Storing Hydro";
            }
            WriteStatus(status, percentPowerStorage, percentHydrogenStorage);

        }

        private void EnableGenerators()
        {
            int count = generators.Count(block => block.Enabled);
            if (count > MAX_GENERATORS)
            {
                foreach (IMyPowerProducer generator in generators.FindAll(generator => generator.Enabled))
                {
                    generator.Enabled = false;
                    if (--count <= MAX_GENERATORS)
                    {
                        return;
                    }
                }
            }
            else if (count < MAX_GENERATORS)
            {
                foreach (IMyPowerProducer generator in generators.FindAll(generator => !generator.Enabled))
                {
                    generator.Enabled = true;
                    if (++count >= MAX_GENERATORS)
                    {
                        return;
                    }

                }
            }
        }

        private void WriteStatus(string status, float percentPowerStorage, float percentHydrogenStorage)
        {
            IMyTextSurface surface = Me.GetSurface(0);
            surface.ContentType = ContentType.TEXT_AND_IMAGE;
            surface.FontSize = 1.2f;
            surface.Alignment = TextAlignment.CENTER;
            surface.WriteText("Stored Power: " + percentPowerStorage.ToString("n3") + "%" + "\n\nHydro Storage: " + percentHydrogenStorage.ToString("n3") + "%" + "\n\nStatus: " + status);
        }

        private void Init()
        {
            MyIniParseResult result;
            if (!config.TryParse(Me.CustomData, out result))
                throw new Exception("Failed to Parse Configuration: " + config.ToString());

            MIN_HYDROGEN_STORAGE = float.Parse(config.Get("config", "minHydrogenStorage").ToString("0"));
            Echo("Min hydrogen storage: " + MIN_HYDROGEN_STORAGE.ToString("n1") + "%");

            MIN_POWER_STORAGE = float.Parse(config.Get("config", "minPowerStorage").ToString("50"));
            Echo("Min power storage: " + MIN_POWER_STORAGE.ToString("n1") + "%");

            GENERATOR_CUTOFF = float.Parse(config.Get("config", "generatorCutoff").ToString("95"));
            Echo("Generator cut off: " + GENERATOR_CUTOFF.ToString("n1") + "%");

            MAX_GENERATORS = config.Get("config", "maxGenerators").ToInt32(int.MaxValue);
            Echo("Max running generators: " + MAX_GENERATORS);

            GridTerminalSystem.GetBlocksOfType(batteries, block => block.IsSameConstructAs(Me));
            Echo("Managing " + batteries.Count + " batteries");

            GridTerminalSystem.GetBlocksOfType(generators, IsReactorOrEngine);
            Echo("Managing " + generators.Count + " generators");

            GridTerminalSystem.GetBlocksOfType(hydrogenTanks, IsHydrogenTank);
            Echo("Managing " + hydrogenTanks.Count + " hydrogen tanks");

            status = generators.Any(block => block.Enabled) ? "RUNNING" : "OFF";
            powerCapacity = batteries.Select(battery => battery.MaxStoredPower).Sum();
            Echo("Power capacity: " + powerCapacity.ToString("n2") + "MW");

            hydrogenCapacity = hydrogenTanks.Select(tank => tank.Capacity).Sum();
            Echo("Hydrogen Capacity: " + hydrogenCapacity.ToString("n2") + "L");
        }

        private bool IsHydrogenTank(IMyGasTank gasTank)
        {
            return gasTank.BlockDefinition.SubtypeName.ToLower().Contains("hydrogen") && gasTank.IsSameConstructAs(Me);
        }

        private bool IsReactorOrEngine(IMyPowerProducer block)
        {
            string subtype = block.BlockDefinition.SubtypeName.ToLower();
            return Me.IsSameConstructAs(block) && (subtype.Contains("engine") || subtype.Contains("reactor"));
        }
    }
}