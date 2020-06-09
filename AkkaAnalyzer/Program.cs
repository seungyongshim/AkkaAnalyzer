using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.MSBuild;

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
                    var compilation = await project.GetCompilationAsync();

                    Console.WriteLine(compilation.AssemblyName);

                    foreach (var syntax in compilation.SyntaxTrees)
                    {
                        var model = compilation.GetSemanticModel(syntax);

                        
                    }
                    
                }

            }
        }
    }
}
