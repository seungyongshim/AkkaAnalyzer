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

            foreach((var slnFile, int i) in slnFiles.Select((x, i) =>(x, i + 1)))
            {
                Console.WriteLine($"# {i}. {Path.GetFileNameWithoutExtension(slnFile.FullName)}");
                using var msWorkspace = MSBuildWorkspace.Create();

                // https://github.com/jpollard-cs/Diagrams/blob/master/Diagrams/Program.cs
                var diagramGenerator = new DiagramGenerator(slnFile.FullName, msWorkspace);
                await diagramGenerator.ProcessSolution();
                //diagramGenerator.GenerateDiagramFromRoot();
                //Console.ReadKey();
            }
        }
    }
}
