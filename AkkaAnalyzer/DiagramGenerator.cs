using AkkaAnalyzer.Report;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.MSBuild;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AkkaAnalyzer
{
    public class DiagramGenerator
    {
        private readonly Solution _solution;

        private readonly ConcurrentDictionary<MethodDeclarationSyntax, List<MethodDeclarationSyntax>> _methodDeclarationSyntaxes
            = new ConcurrentDictionary<MethodDeclarationSyntax, List<MethodDeclarationSyntax>>();

        private readonly ConcurrentDictionary<MethodDeclarationSyntax, Dictionary<int, MethodDeclarationSyntax>> _methodOrder
            = new ConcurrentDictionary<MethodDeclarationSyntax, Dictionary<int, MethodDeclarationSyntax>>();

        public async Task ProcessSolution()
        {

            foreach (Project project in _solution.Projects)
            {
                Compilation compilation = await project.GetCompilationAsync();
                await ProcessCompilation(compilation);
            }
        }

        public DiagramGenerator(string solutionPath, MSBuildWorkspace workspace)
        {
            _solution = workspace.OpenSolutionAsync(solutionPath).Result;

        }

        public DiagramGenerator(string solutionPath, MSBuildWorkspace workspace, AkkaAnalyzerReporter akkaAnalyzerReporter) : this(solutionPath, workspace)
        {
            _akkaAnalyzerReporter = akkaAnalyzerReporter;
        }

        public AkkaAnalyzerReporter _akkaAnalyzerReporter { get; }
        public IEnumerable<string> IgnoreProjects { get; set; }

        private async Task ProcessCompilation(Compilation compilation)
        {
            // Tell 찾기
            await FindTell(compilation);

            foreach (var tree in compilation.SyntaxTrees)
            {
                var root = await tree.GetRootAsync();
                var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();
                SyntaxTree treeCopy = tree;
                foreach (var @class in classes)
                {
                    if (@class.IsReceiveActor())
                    {
                        Console.WriteLine($"## {@class.GetFullName()}");

                        await ProcessClass(@class, compilation, treeCopy);
                        Console.WriteLine();
                    }
                }
            }
        }

        private async Task FindTell(Compilation compilation)
        {
            var symbolTell = compilation.FindSymbol(x => x.Name.Equals("Tell"));

            if (symbolTell is null) return;

            var callerTells = await SymbolFinder.FindCallersAsync(symbolTell, _solution);

            foreach (var caller in callerTells)
            {
                foreach (var location in caller.Locations)
                {
                    if (location.IsInSource)
                    {
                        // .Tell을 기준으로 arguments 위치를 찾는다.
                        var node = location.SourceTree.GetRoot()
                                                      .FindToken(location.SourceSpan.Start)
                                                      .GetNextToken().Parent;
                        try
                        {
                            var argumentsSymbols = node.GetAllSymbols(compilation)
                                                       .ToArray();

                            
                            switch (argumentsSymbols.First())
                            {
                                case ISymbol x when x.Name.Equals(".ctor"):
                                    var msg = argumentsSymbols.Skip(1).First();
                                    _akkaAnalyzerReporter.AddMessageCaller($"{caller.CallingSymbol.ContainingType}", $"{msg.OriginalDefinition}", location);
                                    break;
                                case ILocalSymbol x:
                                    _akkaAnalyzerReporter.AddMessageCaller($"{caller.CallingSymbol.ContainingType}", $"{x.Type}", location);
                                    break;
                                case IMethodSymbol x:
                                    _akkaAnalyzerReporter.AddMessageCaller($"{caller.CallingSymbol.ContainingType}", $"{x.ReturnType}", location);
                                    break;
                                case ISymbol x:
                                    _akkaAnalyzerReporter.AddMessageCaller($"{caller.CallingSymbol.ContainingType}", $"{x.OriginalDefinition}", location);
                                    break;
                                }
                        }
                        catch
                        {

                        }
                    }
                }
            }
        }



        private async Task ProcessClass(ClassDeclarationSyntax @class, Compilation compilation, SyntaxTree syntaxTree)
        {
            var methods = @class.DescendantNodes().OfType<BaseMethodDeclarationSyntax>();

            foreach (var method in methods)
            {
                foreach (var item in method.HasRecieveCallee())
                {
                    var symbols = item.Parent.GetAllSymbols(compilation)
                        .Where(x => x.Name.Equals(item.ValueText))
                        .OfType<IMethodSymbol>()
                        .ToList();

                    foreach (var methodsymbol in symbols)
                    {
                        Console.WriteLine($"- {methodsymbol.TypeArguments[0].ToDisplayString()}");
                        _akkaAnalyzerReporter.AddMessageReceiver(@class.GetFullName(), methodsymbol.TypeArguments[0].ToDisplayString());
                    }
                }
            }
        }
    }
}
