using AkkaAnalyzerReport;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis.MSBuild;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AkkaAnalyzer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            MSBuildLocator.RegisterDefaults();
            DirectoryInfo directoryInfo = new DirectoryInfo(@"C:\Workspace\mls-application");
            var slnFiles = directoryInfo.GetFiles("*.sln", SearchOption.AllDirectories);

            var akkaAnalyzerReporter = new AkkaAnalyzerReporter();

            foreach ((var slnFile, int i) in slnFiles.Select((x, i) => (x, i)))
            {
                Console.WriteLine($"# {i+1}. {Path.GetFileNameWithoutExtension(slnFile.FullName)}");
                using var msWorkspace = MSBuildWorkspace.Create();

                // https://github.com/jpollard-cs/Diagrams/blob/master/Diagrams/Program.cs
                var diagramGenerator = new DiagramGenerator(slnFile.FullName, msWorkspace, akkaAnalyzerReporter);
                await diagramGenerator.ProcessSolution();
            }

            Console.WriteLine("Akka Messages");
            Console.WriteLine(akkaAnalyzerReporter.ReportMessages());
        }
    }
}
