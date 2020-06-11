using AkkaAnalyzer.Report.Entity;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AkkaAnalyzer.Report
{
    public class AkkaAnalyzerReporter
    {
        Dictionary<string, ActorInfo> _actorInfos = new Dictionary<string, ActorInfo>();
        Dictionary<string, MessageInfo> _messageInfos = new Dictionary<string, MessageInfo>();

        

        public void AddMessageCaller(string caller, string msgName)
        {
            if (_actorInfos.TryGetValue(caller, out var actorInfo))
            {
                actorInfo.ReceiveMessages.Add(msgName);
            }
            else
            {
                _actorInfos.Add(caller, new ActorInfo(caller, msgName, null));
            }

            if (_messageInfos.TryGetValue(msgName, out var akkaMessage))
            {
                akkaMessage.Senders.Add(caller);
            }
            else
            {
                _messageInfos.Add(msgName, new MessageInfo(msgName, caller, null));
            }
        }

        public void AddMessageReceiver(string receiver, string msgName)
        {
            if (_actorInfos.TryGetValue(receiver, out var actorInfo))
            {
                actorInfo.ReceiveMessages.Add(msgName);
            }
            else
            {
                _actorInfos.Add(receiver, new ActorInfo(receiver, null, msgName));
            }

            if (_messageInfos.TryGetValue(msgName, out var messageInfo))
            {
                messageInfo.Receivers.Add(receiver);
            }
            else
            {
                _messageInfos.Add(msgName, new MessageInfo(msgName, null, receiver));
            }
        }

        public string ReportMessages()
        {
            StringBuilder sb = new StringBuilder();

            foreach ((var msg, var i) in _messageInfos.Values.Select((x, i) => (x, i)))
            {
                sb.AppendLine($"## {msg.Name}");

                if (msg.Senders.Count() > 0)
                {
                    sb.AppendLine($"- Sender");
                }

                foreach (var item in msg.Senders.Distinct())
                {
                    sb.AppendLine($"  - [{item}](./actors.md#{item}) ({msg.Senders.Where(x => x.Equals(item)).Count()})");
                }

                if (msg.Receivers.Count() > 0)
                {
                    sb.AppendLine($"- Receiver");
                }

                foreach (var item in msg.Receivers.Distinct())
                {
                    sb.AppendLine($"  - [{item}](./actors.md#{item}) ({msg.Receivers.Where(x => x.Equals(item)).Count()})");
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }

        public string ReportActors()
        {
            StringBuilder sb = new StringBuilder();

            foreach ((var msg, var i) in _actorInfos.Values.Select((x, i) => (x, i)))
            {
                sb.AppendLine($"## {msg.Name}");

                if (msg.SendMessages.Count() > 0)
                {
                    sb.AppendLine($"- Send Messages");
                }

                foreach (var item in msg.SendMessages.Distinct())
                {
                    sb.AppendLine($"  - [{item}](./messages.md#{item}) ({msg.SendMessages.Where(x => x.Equals(item)).Count()})");
                }

                if (msg.ReceiveMessages.Count() > 0)
                {
                    sb.AppendLine($"- Receive Messages");
                }

                foreach (var item in msg.ReceiveMessages.Distinct())
                {
                    sb.AppendLine($"  - [{item}](./messages.md#{item}) ({msg.ReceiveMessages.Where(x => x.Equals(item)).Count()})");
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}
