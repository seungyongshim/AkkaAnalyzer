using System.Collections.Generic;

namespace AkkaAnalyzerReport
{
    class AkkaMessage
    {
        public AkkaMessage(string name, string sender, string receiver)
        {
            Name = name;
            if (!string.IsNullOrEmpty(sender))
            {
                Senders.Add(sender);
            }

            if (!string.IsNullOrEmpty(receiver))
            {
                Receivers.Add(receiver);
            }
        }

        public string Name { get; set; }

        public List<string> Senders { get; set; } = new List<string>();

        public List<string> Receivers { get; set; } = new List<string>();
    }
}
