using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AkkaAnalyzer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            MSBuildLocator.RegisterDefaults();
            DirectoryInfo directoryInfo = new DirectoryInfo(@"C:\Workspace\mls-application");
            var slnFiles = directoryInfo.GetFiles("*.sln", SearchOption.AllDirectories);

            var akkaMessages = new Dictionary<string, AkkaMessage>();

            foreach ((var slnFile, int i) in slnFiles.Select((x, i) => (x, i + 1)))
            {
                Console.WriteLine($"# {i}. {Path.GetFileNameWithoutExtension(slnFile.FullName)}");
                using var msWorkspace = MSBuildWorkspace.Create();
                
                // https://github.com/jpollard-cs/Diagrams/blob/master/Diagrams/Program.cs
                var diagramGenerator = new DiagramGenerator(slnFile.FullName, msWorkspace, akkaMessages);
                await diagramGenerator.ProcessSolution();
            }

            Console.WriteLine($"# Messages");

            foreach ((var msg, int i) in akkaMessages.Values.Select((x, i) => (x, i)))
            {

                Console.WriteLine($"## {msg.Name}");
                foreach(var caller in msg.Senders.Distinct())
                {
                    Console.WriteLine($"- {caller} ({msg.Senders.Where(x => x.Equals(caller)).Count()})");
                }
                Console.WriteLine();
            }
        }
    }
}
