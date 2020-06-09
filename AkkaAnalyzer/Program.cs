using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.MSBuild;
using static Generator.Generator;

namespace AkkaAnalyzer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            MSBuildLocator.RegisterDefaults();
            DirectoryInfo directoryInfo = new DirectoryInfo(@"C:\Workspace\mls-application");
            var slnFiles = directoryInfo.GetFiles("*.sln", SearchOption.AllDirectories);

            foreach(var slnFile in slnFiles)
            {
                using var msWorkspace = MSBuildWorkspace.Create();

                var solution = await msWorkspace.OpenSolutionAsync(slnFile.FullName);

                foreach (var project in solution.Projects)
                {
                    List<MetadataReference> metadata = new List<MetadataReference>();
                    foreach (var item in project.Documents)
                    {
                        var metaref = MetadataReference.CreateFromFile(item.FilePath);
                        metadata.Add(metaref);
                    }

                    var compilation = await project.GetCompilationAsync();

                    Console.WriteLine(compilation.AssemblyName);
                    var symbols = GetSymbols(compilation, metadata);

                    foreach (var symbol in symbols)
                    {
                        symbol.
                        Console.WriteLine($"  {symbol.Name}");
                    }
                }

            }
        }
    }
}
