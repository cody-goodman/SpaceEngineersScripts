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

        public void Main(String arguments)
        {
            string tag = "";
            string oldTag = "";
            if (_commandLine.TryParse(arguments))
            {
                if (_commandLine.ArgumentCount > 0)
                {
                    tag = "<" + _commandLine.Argument(0) + ">";
                }
                else
                {
                    tag = "<" + Me.CubeGrid.CustomName + ">";
                }
                if (_commandLine.ArgumentCount > 1)
                {
                    oldTag = _commandLine.Argument(1);
                }
                List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
                GridTerminalSystem.GetBlocksOfType(blocks, block => block.IsSameConstructAs(Me));
                Dictionary<string, int> names = new Dictionary<string, int>();
                blocks.ForEach(block => RenameBlock(block, tag, oldTag, names));
            }
            else
            {
                Echo("Invalid Arguments: " + arguments);
            }
        }

        private void RenameBlock(IMyTerminalBlock block, string tag, string oldTag, Dictionary<string, int> names)
        {
            string baseName = GetBaseName(block, oldTag);
            block.CustomName = tag + " " + baseName + GetNumber(names, baseName);
        }

        private string GetBaseName(IMyTerminalBlock block, string oldTag)
        {
            if (_commandLine.Switch("resetNames"))
            {
                return block.DefinitionDisplayNameText;
            }
            else
            {
                string baseName;
                int index = block.CustomName.LastIndexOf(oldTag);
                index = index > 0 ? index : block.CustomName.LastIndexOf("<" + oldTag + ">");
                if (index > 0)
                {
                    baseName = block.CustomName.Substring(index + 1);
                }
                else
                {
                    baseName = block.CustomName;
                }
                // if reset numbers switch is active then trim any numbers from the end of the string
                if (_commandLine.Switch("resetNumbers")) System.Text.RegularExpressions.Regex.Replace(baseName, "\\s\\d+$", "");
                return baseName;
            }
        }

        private string GetNumber(Dictionary<string, int> names, string blockName)
        {
            int count = names.GetValueOrDefault(blockName, 0);
            if (count == 0)
            {
                names.Add(blockName, 2);
                return "";
            }
            else
            {
                names[blockName] = names[blockName] + 1;
                return " " + count.ToString();
            }
        }
    }
}
