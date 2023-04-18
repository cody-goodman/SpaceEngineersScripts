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
        public void Main(string argument, UpdateType updateSource)
        {
            List<IMyShipConnector> connectors = new List<IMyShipConnector>();
            GridTerminalSystem.GetBlocksOfType(connectors, block => block.IsSameConstructAs(Me));

            if (connectors.Exists(IsReadyToLock))
            {
                Dock(connectors.Find(IsReadyToLock));
            }
            else if (connectors.Exists(IsLocked))
            {
                Undock(connectors.Find(IsLocked));
            }else
            {
                Reset();
            }
        }

        private void Reset()
        {
            List<IMyBatteryBlock> batteries = new List<IMyBatteryBlock>();
            GridTerminalSystem.GetBlocksOfType(batteries, block => block.IsSameConstructAs(Me));

            List<IMyGasTank> gasTanks = new List<IMyGasTank>();
            GridTerminalSystem.GetBlocksOfType(gasTanks, block => block.IsSameConstructAs(Me));

            List<IMyThrust> thrusters = new List<IMyThrust>();
            GridTerminalSystem.GetBlocksOfType(thrusters, block => block.IsSameConstructAs(Me));

            List<IMyLightingBlock> lights = new List<IMyLightingBlock>();
            GridTerminalSystem.GetBlocksOfType(lights, blocks => blocks.IsSameConstructAs(Me));

            batteries.ForEach(battery => battery.ChargeMode = ChargeMode.Auto);
            gasTanks.ForEach(gasTank => gasTank.Stockpile = false);
            thrusters.ForEach(thruster => thruster.Enabled = true);
        }

        private void Dock(IMyShipConnector connector)
        {
            List<IMyBatteryBlock> batteries = new List<IMyBatteryBlock>();
            GridTerminalSystem.GetBlocksOfType(batteries, block => block.IsSameConstructAs(Me));

            List<IMyGasTank> gasTanks = new List<IMyGasTank>();
            GridTerminalSystem.GetBlocksOfType(gasTanks, block => block.IsSameConstructAs(Me));

            List<IMyThrust> thrusters = new List<IMyThrust>();
            GridTerminalSystem.GetBlocksOfType(thrusters, block => block.IsSameConstructAs(Me));

            List<IMyLightingBlock> lights = new List<IMyLightingBlock>();
            GridTerminalSystem.GetBlocksOfType(lights, blocks => blocks.IsSameConstructAs(Me));

            connector.Connect();

            batteries.ForEach(battery => battery.ChargeMode = ChargeMode.Recharge);
            gasTanks.ForEach(gasTank => gasTank.Stockpile = true);
            thrusters.ForEach(thruster => thruster.Enabled = false);
            lights.ForEach(light => light.Enabled = false);
            
        }

        private void Undock(IMyShipConnector connector)
        {
            Reset();
            connector.Disconnect();
        }

        bool IsReadyToLock(IMyShipConnector connector)
        {
            return connector.Status == MyShipConnectorStatus.Connectable;
        }

        bool IsLocked(IMyShipConnector connector)
        {
            return connector.Status == MyShipConnectorStatus.Connected;
        }
    }
}
