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
                    tag = _commandLine.Argument(0);
                    Echo("Using Supplied Tag: " + tag);
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
            string baseName = GetBaseName(block, tag, oldTag);
            string name = baseName + GetNumber(names, baseName);
            if (_commandLine.Switch("suffix"))
                block.CustomName = name + " " + tag;
            else
                block.CustomName = tag + " " + name;
        }

        private string GetBaseName(IMyTerminalBlock block, string tag, string oldTag)
        {

            if (_commandLine.Switch("resetNames"))
                return block.DefinitionDisplayNameText;

            // Remove old tag if present
            string baseName = String.IsNullOrWhiteSpace(oldTag) ? block.CustomName : block.CustomName.Replace(oldTag, String.Empty);
            // Remove any existing duplicates of current tag
            baseName = String.IsNullOrWhiteSpace(tag) ? baseName : baseName.Replace(tag, String.Empty);

            // Strip numbers off the end
            System.Text.RegularExpressions.Regex.Replace(baseName, "\\s\\d+$", "");
            return baseName.Trim();

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
