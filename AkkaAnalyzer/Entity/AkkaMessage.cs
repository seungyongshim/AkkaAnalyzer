using System.Collections.Generic;

namespace AkkaAnalyzer
{
    class AkkaMessage 
    {
        public string Name { get; set; }

        public List<string> Senders { get; set; }

        public List<string> Receivers { get; set; }

        public override int GetHashCode() => System.HashCode.Combine(Name);
    }
}
