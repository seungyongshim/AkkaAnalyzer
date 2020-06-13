using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace AkkaAnalyzer.Report.Entity
{
    public class ActorInfo
    {
        public ActorInfo(string name, (string, Location) sendMessage, string receiveMessage)
        {
            Name = name;

            if (sendMessage != default)
            {
                SendMessages.Add(sendMessage);
            }

            if (!string.IsNullOrEmpty(receiveMessage))
            {
                ReceiveMessages.Add(receiveMessage);
            }
        }

        public string Name { get; set; }

        public List<string> ReceiveMessages { get; set; } = new List<string>();

        public List<(string sendMsg, Location loc)> SendMessages { get; set; } = new List<(string, Location)>();
    }
}
