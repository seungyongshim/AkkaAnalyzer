using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace AkkaAnalyzer.Report.Entity
{
    class MessageInfo
    {
        public MessageInfo(string name, (string, Location) sender, string receiver)
        {
            Name = name;
            if (sender != default)
            {
                Senders.Add(sender);
            }

            if (!string.IsNullOrEmpty(receiver))
            {
                Receivers.Add(receiver);
            }
        }

        public string Name { get; set; }

        public List<(string caller, Location loc)> Senders { get; set; } = new List<(string, Location)>();

        public List<string> Receivers { get; set; } = new List<string>();
    }
}
