using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AkkaAnalyzerReport
{
    public class AkkaAnalyzerReporter
    {
        Dictionary<string, AkkaMessage> _akkaMessages = new Dictionary<string, AkkaMessage>();

        public string ReportMessages()
        {
            StringBuilder sb = new StringBuilder();

            foreach ((var msg, int i) in _akkaMessages.Values.Select((x, i) => (x, i)))
            {
                sb.AppendLine($"## {msg.Name}");

                if (msg.Senders.Count() > 0)
                {
                    sb.AppendLine($"- Sender");
                }

                foreach (var caller in msg.Senders.Distinct())
                {
                    sb.AppendLine($"  - {caller} ({msg.Senders.Where(x => x.Equals(caller)).Count()})");
                }

                if (msg.Receivers.Count() > 0)
                {
                    sb.AppendLine($"- Receiver");
                }
                    
                foreach (var item in msg.Receivers.Distinct())
                {
                    sb.AppendLine($"  - {item} ({msg.Receivers.Where(x => x.Equals(item)).Count()})");
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }

        public void AddMessageCaller(string caller, string msgName)
        {
            if (_akkaMessages.TryGetValue(msgName, out var akkaMessage))
            {
                akkaMessage.Senders.Add(caller);
            }
            else
            {
                _akkaMessages.Add(msgName, new AkkaMessage(msgName, caller, null));
            }
        }

        public void AddMessageReceiver(string receiver, string msgName)
        {
            if (_akkaMessages.TryGetValue(msgName, out var akkaMessage))
            {
                akkaMessage.Receivers.Add(receiver);
            }
            else
            {
                _akkaMessages.Add(msgName, new AkkaMessage(msgName, null, receiver));
            }
        }
    }
}
