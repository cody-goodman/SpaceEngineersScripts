using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using VRage.Game.ModAPI.Ingame.Utilities;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {

        MyCommandLine _commandLine = new MyCommandLine();

        public void Main(String arguments)
        {
            string tag = null;
            string oldTag = null;
            if (_commandLine.TryParse(arguments))
            {
                if (_commandLine.ArgumentCount > 0)
                {
                    tag = "<" + _commandLine.Argument(0) + ">";
                    Echo("Using Supplied Name as Tag: " + tag);
                }
                else
                {
                    tag = "<" + Me.CubeGrid.CustomName + ">";
                    Echo("Using grid name as tag: " + tag);
                }
                if (_commandLine.ArgumentCount > 1)
                {
                    oldTag = _commandLine.Argument(1);
                    Echo("Replacing old tag: " + oldTag);
                }

            }
            else
            {
                if (!String.IsNullOrEmpty(arguments))
                {
                    throw new Exception("Invalid Arguments: " + arguments);
                }
                tag = "<" + Me.CubeGrid.CustomName + ">";
                Echo("Using grid name as tag");
            }
            List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType(blocks, block => block.IsSameConstructAs(Me));
            Dictionary<string, int> names = new Dictionary<string, int>();
            blocks.ForEach(block => RenameBlock(block, tag, oldTag, names));
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
            string baseName;
            if (!String.IsNullOrWhiteSpace(oldTag))
            {
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
            }
            else
            {
                baseName = block.CustomName;
            }
            // if reset numbers switch is active then trim any numbers from the end of the string
            if (_commandLine.Switch("resetNumbers")) System.Text.RegularExpressions.Regex.Replace(baseName, "\\s\\d+$", "");
            return baseName;

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
