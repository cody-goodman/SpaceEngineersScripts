using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;
using System.Linq;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        string TAG = "rcdoors";
        string lastError = "";
        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
        }

        public void Main(string argument, UpdateType updateSource)
        {
            Echo("Last Error: \n\n" + lastError);
            IMyBroadcastListener listener = IGC.RegisterBroadcastListener(TAG);
            while (listener.HasPendingMessage)
            {
                MyIGCMessage message = listener.AcceptMessage();
                Echo("Receiving Message: " + message.Data.ToString());
                if (message.Data is string)
                {
                    string data = message.Data.ToString();
                    string[] splits = message.Data.ToString().Split(';');
                    if (splits.Count() != 3)
                        lastError = "Received malform message: " + data + "\n\nFrom: " + GridTerminalSystem.GetBlockWithId(message.Source).CustomName;
                    else
                        Handle(splits[0], splits[1], splits[2]);
                }
                else
                {
                    lastError = "Received Message With Invalid Data Type From: " + message.Source;
                }
            }
        }

        private void Handle(string type, string name, string action)
        {
            List<IMyDoor> doors = new List<IMyDoor>();
            if (type.Equals("group"))
            {
                IMyBlockGroup group = GridTerminalSystem.GetBlockGroupWithName(name);
                group.GetBlocksOfType(doors);
            }
            else if (type.Equals("single"))
            {
                IMyDoor door = (IMyDoor)GridTerminalSystem.GetBlockWithName(name);
                doors.Add(door);
            }
            else
            {
                lastError = "Received Invalid Type: " + type + "\n\nMust be either \'single\' or \'group\'";
            }
            Handle(doors, action);
        }

        private void Handle(List<IMyDoor> doors, string action)
        {
            if (action.Equals("open"))
            {
                doors.ForEach(door => door.OpenDoor());
            }
            else if (action.Equals("close"))
            {
                doors.ForEach(door => door.CloseDoor());
            }
            else if (action.Equals("toggle"))
            {
                doors.ForEach(door => door.ToggleDoor());
            }
            else
            {
                lastError = "Unrecognized door action: " + action + "\n\nMust be \'open' or \'close\' or \'toggle\'";
            }
        }

    }
}
