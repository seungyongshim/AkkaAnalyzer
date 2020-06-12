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
                    sb.AppendLine($"  - [{item}](../../Archtecture/Actors.md#{item.Replace(".", string.Empty).ToLower()}) ({msg.Senders.Where(x => x.Equals(item)).Count()})");
                }

                if (msg.Receivers.Count() > 0)
                {
                    sb.AppendLine($"- Receiver");
                }

                foreach (var item in msg.Receivers.Distinct())
                {
                    sb.AppendLine($"  - [{item}](../../Archtecture/Actors.md#{item.Replace(".", string.Empty).ToLower()}) ({msg.Receivers.Where(x => x.Equals(item)).Count()})");
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
                    sb.AppendLine($"  - [{item}](../../Archtecture/Messages.md#{item.Replace(".", string.Empty).ToLower()}) ({msg.SendMessages.Where(x => x.Equals(item)).Count()})");
                }

                if (msg.ReceiveMessages.Count() > 0)
                {
                    sb.AppendLine($"- Receive Messages");
                }

                foreach (var item in msg.ReceiveMessages.Distinct())
                {
                    sb.AppendLine($"  - [{item}](../../Archtecture/Messages.md#{item.Replace(".", string.Empty).ToLower()}) ({msg.ReceiveMessages.Where(x => x.Equals(item)).Count()})");
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }

        public string ReportArchtecture()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"# 1. Messages");

            foreach ((var msg, var i) in _messageInfos.Values.Select((x, i) => (x, i)))
            {
                sb.AppendLine($"## {msg.Name}");

                sb.AppendLine("```mermaid");
                sb.AppendLine("graph LR");
                sb.AppendLine("linkStyle default interpolate basis");

                StringBuilder clickbuilder = new StringBuilder();

                foreach ((var item, var itemIdx) in msg.Senders
                    .Select(x => x.Split('.').Last())
                    .Distinct().Select((x, i) => (x, i)))
                {
                    sb.AppendLine($"  A{itemIdx}([{item}]) --- B[{msg.Name.Split('.').Last()}]");
                    clickbuilder.AppendLine($"click A{itemIdx} \"#{item.Replace(".", string.Empty).ToLower()}\"");

                }

                foreach ((var item, var itemIdx) in msg.Receivers
                    .Select(x => x.Split('.').Last())
                    .Distinct().Select((x, i) => (x, i)))
                {
                    sb.AppendLine($"  B[{msg.Name.Split('.').Last()}] --> C{itemIdx}([{item}])");
                    clickbuilder.AppendLine($"click C{itemIdx} \"#{item.Replace(".", string.Empty).ToLower()}\"");
                }

                sb.AppendLine(clickbuilder.ToString());

                sb.AppendLine("```");


                if (msg.Senders.Any())
                {
                    sb.AppendLine($"- Sender");
                }

                foreach (var item in msg.Senders.Distinct())
                {
                    sb.AppendLine($"  - [{item}](#{item.Replace(".", string.Empty).ToLower()}) ({msg.Senders.Where(x => x.Equals(item)).Count()})");
                }

                if (msg.Receivers.Any())
                {
                    sb.AppendLine($"- Receiver");
                }

                foreach (var item in msg.Receivers.Distinct())
                {
                    sb.AppendLine($"  - [{item}](#{item.Replace(".", string.Empty).ToLower()}) ({msg.Receivers.Where(x => x.Equals(item)).Count()})");
                }

                sb.AppendLine();
            }


            sb.AppendLine($"# 2. Actors");

            foreach ((var msg, var i) in _actorInfos.Values.Select((x, i) => (x, i)))
            {
                sb.AppendLine($"## {msg.Name}");

                sb.AppendLine("```mermaid");
                sb.AppendLine("graph LR");
                sb.AppendLine("linkStyle default interpolate basis");

                StringBuilder clickbuilder = new StringBuilder();

                foreach ((var item, var itemIdx) in msg.ReceiveMessages.Distinct().Select((x, i) => (x, i)))
                {
                    sb.AppendLine($"  A{itemIdx}[{item.Split('.').Last()}] --> B(({msg.Name.Split('.').Last()}))");
                    clickbuilder.AppendLine($"click A{itemIdx} \"#{item.Replace(".", string.Empty).ToLower()}\"");

                }

                foreach ((var item, var itemIdx) in msg.SendMessages.Distinct().Select((x, i) => (x, i)))
                {
                    sb.AppendLine($"  B(({msg.Name.Split('.').Last()})) --> C{itemIdx}[{item.Split('.').Last()}]");
                    clickbuilder.AppendLine($"click C{itemIdx} \"#{item.Replace(".", string.Empty).ToLower()}\"");
                }

                sb.AppendLine(clickbuilder.ToString());

                sb.AppendLine("```");

                if (msg.SendMessages.Any())
                {
                    sb.AppendLine($"- Send Messages");
                }

                foreach (var item in msg.SendMessages.Distinct())
                {
                    sb.AppendLine($"  - [{item}](#{item.Replace(".", string.Empty).ToLower()}) ({msg.SendMessages.Where(x => x.Equals(item)).Count()})");
                }

                if (msg.ReceiveMessages.Any())
                {
                    sb.AppendLine($"- Receive Messages");
                }

                foreach (var item in msg.ReceiveMessages.Distinct())
                {
                    sb.AppendLine($"  - [{item}](#{item.Replace(".", string.Empty).ToLower()}) ({msg.ReceiveMessages.Where(x => x.Equals(item)).Count()})");
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}
