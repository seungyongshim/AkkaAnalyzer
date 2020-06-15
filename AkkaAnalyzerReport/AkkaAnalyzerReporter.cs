using AkkaAnalyzer.Report.Entity;
using LanguageExt;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AkkaAnalyzer.Report
{
    public class AkkaAnalyzerReporter
    {
        Dictionary<string, ActorInfo> _actorInfos = new Dictionary<string, ActorInfo>();
        Dictionary<string, MessageInfo> _messageInfos = new Dictionary<string, MessageInfo>();

        

        public void AddMessageCaller(string caller, string msgName, Location location = null)
        {
            if (_actorInfos.TryGetValue(caller, out var actorInfo))
            {
                actorInfo.ReceiveMessages.Add(msgName);
            }
            else
            {
                _actorInfos.Add(caller, new ActorInfo(caller, (msgName, location), null));
            }

            if (_messageInfos.TryGetValue(msgName, out var akkaMessage))
            {
                akkaMessage.Senders.Add((caller, location));
            }
            else
            {
                _messageInfos.Add(msgName, new MessageInfo(msgName, (caller, location), null));
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
                _actorInfos.Add(receiver, new ActorInfo(receiver, default, msgName));
            }

            if (_messageInfos.TryGetValue(msgName, out var messageInfo))
            {
                messageInfo.Receivers.Add(receiver);
            }
            else
            {
                _messageInfos.Add(msgName, new MessageInfo(msgName, default, receiver));
            }
        }

        public async Task<string> ReportArchtecture()
        {
            StringBuilder sb = new StringBuilder();


            foreach (var actor in _actorInfos.Values)
            {
                sb.AppendLine($"{{id:{actor.Name.GetHashCode()}, name:'{actor.Name.Split('.').Last()}', label:'{actor.Name}', group: 'team' }},");
            }

            foreach (var msg in _messageInfos.Values)
            {
                foreach (var actors in from sender in msg.Senders.Select(x => x.caller).Distinct()
                                       from receiver in msg.Receivers.Distinct()
                                       select (sender, receiver))
                {
                    sb.AppendLine($"{{source:{actors.sender.GetHashCode()}, target:{actors.receiver.GetHashCode()}, type:'{msg.Name.Split('.').Last()}' }},");
                }
            }

            sb.AppendLine($"# 1. Messages");

            foreach ((var msg, var i) in _messageInfos.Values.Select((x, i) => (x, i)))
            {
                sb.AppendLine($"## {msg.Name}");

                sb.AppendLine("```mermaid");
                sb.AppendLine("graph LR");
                sb.AppendLine("linkStyle default interpolate basis");

                foreach ((var item, var itemIdx) in msg.Senders
                    .Select(x => x.caller.Split('.').Last())
                    .Distinct().Select((x, i) => (x, i)))
                {
                    sb.AppendLine($"  A{itemIdx}([{item}]) --- B[{msg.Name.Split('.').Last()}]");

                }

                foreach ((var item, var itemIdx) in msg.Receivers
                    .Select(x => x.Split('.').Last())
                    .Distinct().Select((x, i) => (x, i)))
                {
                    sb.AppendLine($"  B[{msg.Name.Split('.').Last()}] --> C{itemIdx}([{item}])");
                }

                sb.AppendLine("```");


                if (msg.Senders.Any())
                {
                    sb.AppendLine($"- Sender");
                }

                foreach (var item in msg.Senders.Distinct().Select(x => x.caller))
                {
                    sb.AppendLine($"  - [{item}](#{item.Replace(".", string.Empty).ToLower()}) ({msg.Senders.Where(x => x.caller.Equals(item)).Count()})");
                    foreach (var loc in msg.Senders.Where( x => x.caller == item).Select(x => x.loc))
                    {
                        sb.AppendLine($"     - {await loc.GetLineSpan().AsTask().Select(x => x.Path + "#L" + x.StartLinePosition.Line)}");
                    }
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

                foreach ((var item, var itemIdx) in msg.SendMessages.Distinct().Select(x => x.sendMsg).Select((x, i) => (x, i)))
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

                foreach (var item in msg.SendMessages.Select(x => x.sendMsg).Distinct())
                {
                    sb.AppendLine($"  - [{item}](#{item.Replace(".", string.Empty).ToLower()}) ({msg.SendMessages.Where(x => x.Equals(item)).Count()})");
                    foreach (var loc in msg.SendMessages.Where(x => x.sendMsg == item).Select(x => x.loc))
                    {
                        sb.AppendLine($"     - {await loc.GetLineSpan().AsTask().Select(x => x.Path + "#L" + x.StartLinePosition.Line)}");
                    }
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
