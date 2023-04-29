using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using VRage.Game.ModAPI.Ingame.Utilities;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        MyIni config = new MyIni();
        bool modeLocked = false;
        string fuelTankTag = "fuel";
        string haulTankTag = "haul";
        float unloadThresholdPercent = 50f;
        float transportStockpileThresholPercent = 100f;
        List<IMyGasTank> fuelTanks = new List<IMyGasTank>();
        List<IMyGasTank> haulTanks = new List<IMyGasTank>();
        List<IMyShipConnector> connectors = new List<IMyShipConnector>();

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
            Init();
        }

        public void Main(string argument, UpdateType updateSource)
        {
            // If an argument is passed in, force the mode to whatever the argument specifies
            if (!string.IsNullOrWhiteSpace(argument))
            {
                modeLocked = true;
                string mode = argument.ToLower();
                if (mode.Equals("load"))
                {
                    LoadGasTanks();
                    Echo("Mode Locked: " + "load");
                }
                else if (mode.Equals("unload"))
                {
                    UnloadGasTanks();
                    Echo("Mode Locked: " + "unload");
                }
                else if (mode.Equals("transport"))
                {
                    TransportTanks();
                    Echo("Mode Locked: " + "transport");
                }
                else if (mode.Equals("unlock"))
                {
                    modeLocked = false;
                    Echo("Mode Unlocked");
                }
                else if (mode.Equals("reset"))
                    Init();
                else
                    Echo("Unrecognized Mode: " + argument);
            }
            else if (!modeLocked)
            {
                if (IsDocked())
                {
                    // If docked go to either load or unload mode dependening on current hydro storage.

                    float percentGasStorage = GetPercentStorage();
                    if (percentGasStorage > unloadThresholdPercent)
                    {
                        Echo("Unloading Tanks because storage is above " + unloadThresholdPercent + "% threshold");
                        UnloadGasTanks();
                    }
                    else
                    {
                        Echo("Loading Tanks because storage is below " + unloadThresholdPercent + "% threshold");
                        LoadGasTanks();
                    }
                }
                else
                {
                    TransportTanks();
                }
            }
        }

        private void Init()
        {
            modeLocked = false;
            MyIniParseResult result;
            if (!config.TryParse(Me.CustomData, out result))
                throw new Exception("Failed to Parse Configuration: " + config.ToString());

            fuelTankTag = config.Get("config", "fuelTankTag").ToString(fuelTankTag);
            haulTankTag = config.Get("config", "haulTankTag").ToString(haulTankTag);
            unloadThresholdPercent = float.Parse(config.Get("config", "unloadThresholdPercent").ToString("50"));
            transportStockpileThresholPercent = float.Parse(config.Get("config", "transportStockpileThresholPercent").ToString("100"));

            GridTerminalSystem.GetBlocksOfType(haulTanks, tank => ContainsTag(tank, haulTankTag));
            GridTerminalSystem.GetBlocksOfType(fuelTanks, tank => ContainsTag(tank, fuelTankTag));
            GridTerminalSystem.GetBlocksOfType(connectors, connector => Me.IsSameConstructAs(connector));
            Echo("Managing " + haulTanks.Count + " hauling tanks");
            Echo("Managing " + fuelTanks.Count + " fuel tanks");
        }

        private float GetPercentStorage()
        {
            float gasCapacity = haulTanks.Select(tank => tank.Capacity).Sum();
            float currentGasStorage = (float)haulTanks.Select(tank => tank.Capacity * tank.FilledRatio).Sum();
            return currentGasStorage / gasCapacity * 100;
        }

        private Boolean ContainsTag(IMyGasTank tank, string tag)
        {
            return tank.CustomName.ToLower().Contains(fuelTankTag.ToLower()) && Me.IsSameConstructAs(tank);
        }

        private void TransportTanks()
        {
            fuelTanks.ForEach(tank => tank.Stockpile = false);

            float percentStorage = GetPercentStorage();
            if (percentStorage >= transportStockpileThresholPercent)
                haulTanks.ForEach(tank => tank.Stockpile = true);
            else
                haulTanks.ForEach(tank => tank.Stockpile = false);
        }

        private void LoadGasTanks()
        {
            fuelTanks.ForEach(tank => tank.Stockpile = true);
            haulTanks.ForEach(tank => tank.Stockpile = true);
        }

        private void UnloadGasTanks()
        {
            fuelTanks.ForEach(tank => tank.Stockpile = true);
            haulTanks.ForEach(tank => tank.Stockpile = false);
        }

        private bool IsDocked()
        {
            return connectors.Exists(connector => connector.Status == MyShipConnectorStatus.Connected);
        }
    }
}
