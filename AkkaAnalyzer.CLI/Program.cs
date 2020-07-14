using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AkkaAnalyzer.Report;
using CommandLine;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis.MSBuild;

namespace AkkaAnalyzer
{
    internal class Program
    {
        public class Options
        {
            [Option('r', "read-dir", Required = true, HelpText = "Input Directory to be processed.")]
            public string InputDirectory { get; set; }

            [Option('f', "filter", Separator = ',', Required = false, HelpText = "Ignore solution names.")]
            public IEnumerable<string> IgnoreSolutions { get; set; }
        }

        private static async Task Main(string[] args)
        {
            var options = new Options();
            Parser.Default.ParseArguments<Options>(args).WithParsed(x =>
            {
                options.InputDirectory = x.InputDirectory;
                options.IgnoreSolutions = x.IgnoreSolutions;
            });


            MSBuildLocator.RegisterDefaults();
            var directoryInfo = new DirectoryInfo($"{options.InputDirectory}");
            var slnFiles = directoryInfo.GetFiles("*.sln", SearchOption.AllDirectories);

            var akkaAnalyzerReporter = new AkkaAnalyzerReporter();

            foreach ((var slnFile, var i) in slnFiles
                .Where(x => !options.IgnoreSolutions.Any(a => Regex.IsMatch(x.Name, a.Trim())))
                .Select((x, i) => (x, i)))
            {
                Console.WriteLine($"# {i + 1}. {Path.GetFileNameWithoutExtension(slnFile.FullName)}");
                using var msWorkspace = MSBuildWorkspace.Create();

                var diagramGenerator = new DiagramGenerator(slnFile.FullName, msWorkspace, akkaAnalyzerReporter);
                await diagramGenerator.ProcessSolution().ConfigureAwait(false);
            }

            //File.WriteAllText("Archtecture.md", await akkaAnalyzerReporter.ReportArchtecture().ConfigureAwait(false));
            File.WriteAllText("Archtecture.md", await akkaAnalyzerReporter.ReportCSV().ConfigureAwait(false));

        }
    }
}
