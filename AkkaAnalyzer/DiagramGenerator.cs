using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.MSBuild;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AkkaAnalyzer
{
    class DiagramGenerator
    {
        #region [Fields & Properties]
        private readonly Solution _solution;

        private readonly ConcurrentDictionary<MethodDeclarationSyntax, List<MethodDeclarationSyntax>> _methodDeclarationSyntaxes
            = new ConcurrentDictionary<MethodDeclarationSyntax, List<MethodDeclarationSyntax>>();

        private readonly ConcurrentDictionary<MethodDeclarationSyntax, Dictionary<int, MethodDeclarationSyntax>> _methodOrder
            = new ConcurrentDictionary<MethodDeclarationSyntax, Dictionary<int, MethodDeclarationSyntax>>();
        #endregion

        #region [Constructor]
        public DiagramGenerator(string solutionPath, MSBuildWorkspace workspace)
        {
            _solution = workspace.OpenSolutionAsync(solutionPath).Result;
        }
        #endregion

        #region [process the tree]
        private async Task ProcessCompilation(Compilation compilation)
        {
            compilation.GetSymbolsWithName()

            var symbols = syntaxTree.GetRoot().GetAllSymbols(compilation).ToList();

            var list = symbols
                .Where(x => x.Name.Equals("Tell"))
                .FirstOrDefault();

            var tellcallers = await SymbolFinder.FindCallersAsync(list, _solution);

            foreach (var tellcaller in tellcallers)
            {

            }

            var trees = compilation.SyntaxTrees;

            foreach (var tree in trees)
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

        private async Task ProcessClass(ClassDeclarationSyntax @class, Compilation compilation, SyntaxTree syntaxTree)
        {
            var methods = @class.DescendantNodes().OfType<MethodDeclarationSyntax>();

            foreach (var method in methods)
            {
                await ProcessMethod(method, compilation, syntaxTree);
            }
        }

        private async Task ProcessMethod(MethodDeclarationSyntax method, Compilation compilation, SyntaxTree syntaxTree)
        {
            foreach (var item in method.HasRecieveCallee())
            {
                var symbols = item.Parent.GetAllSymbols(compilation)
                    .Where(x => x.Name.Equals(item.ValueText))
                    .ToList();
                    

                foreach (var symbol in symbols)
                {
                    Console.WriteLine($"- {symbol.ToDisplayString()}");
                }
                //var model = compilation.GetSemanticModel(syntaxTree);
                //var methodSymbol = model.GetDeclaredSymbol(method);
                //var methodSymbol = model.GetDeclaredSymbol(item.Parent.Parent.Parent);
                //var array = methodSymbol.DeclaringSyntaxReferences.ToArray();
            }
            
            // https://stackoverflow.com/questions/55118805/extract-called-method-information-using-roslyn


        }

        private async Task<List<MethodDeclarationSyntax>> GetCallingMethodsAsync(ISymbol methodSymbol, MethodDeclarationSyntax method)
        {
            var references = new List<MethodDeclarationSyntax>();

            var referencingSymbols = await SymbolFinder.FindCallersAsync(methodSymbol, _solution);
            var referencingSymbolsList = referencingSymbols as IList<SymbolCallerInfo> ?? referencingSymbols.ToList();

            if (!referencingSymbolsList.Any(s => s.Locations.Any()))
            {
                return references;
            }

            foreach (var referenceSymbol in referencingSymbolsList)
            {
                foreach (var location in referenceSymbol.Locations)
                {
                    var position = location.SourceSpan.Start;
                    var root = await location.SourceTree.GetRootAsync();
                    var nodes = root.FindToken(position).Parent.AncestorsAndSelf().OfType<MethodDeclarationSyntax>();

                    var methodDeclarationSyntaxes = nodes as MethodDeclarationSyntax[] ?? nodes.ToArray();
                    references.AddRange(methodDeclarationSyntaxes);
                }
            }

            return references;
        }

        public void GenerateDiagramFromRoot()
        {
            MethodDeclarationSyntax root = null;
            foreach (var key in _methodDeclarationSyntaxes.Keys)
            {
                if (!_methodDeclarationSyntaxes.Values.Any(value => value.Contains(key)))
                {
                    // then we have a method that's not being called by anything
                    root = key;
                    break;
                }
            }

            if (root != null)
            {
                PrintMethodInfo(root);
            }
        }

        public void PrintMethodInfo(MethodDeclarationSyntax callingMethod)
        {
            if (!_methodDeclarationSyntaxes.ContainsKey(callingMethod))
            {
                return;
            }

            var calledMethods = _methodOrder[callingMethod];
            var orderedCalledMethods = calledMethods.OrderBy(kvp => kvp.Key);

            foreach (var kvp in orderedCalledMethods)
            {
                var calledMethod = kvp.Value;
                ClassDeclarationSyntax callingClass = null;
                ClassDeclarationSyntax calledClass = null;

                if (!SyntaxNodeHelper.TryGetParentSyntax(callingMethod, out callingClass) ||
                    !SyntaxNodeHelper.TryGetParentSyntax(calledMethod, out calledClass))
                {
                    continue;
                }

                PrintOutgoingCallInfo(
                          calledClass
                        , callingClass
                        , callingMethod
                        , calledMethod
                    );

                if (callingMethod != calledMethod)
                {
                    PrintMethodInfo(calledMethod);
                }

                PrintReturnCallInfo(
                          calledClass
                        , callingClass
                        , callingMethod
                        , calledMethod
                    );
            }
        }

        private static void PrintOutgoingCallInfo(
              ClassDeclarationSyntax classBeingCalled
            , ClassDeclarationSyntax callingClass
            , MethodDeclarationSyntax callingMethod
            , MethodDeclarationSyntax calledMethod
            , bool includeCalledMethodArguments = false)
        {
            var callingMethodName = callingMethod.Identifier.ToFullString();
            var calledMethodReturnType = calledMethod.ReturnType.ToFullString();
            var calledMethodName = calledMethod.Identifier.ToFullString();
            var calledMethodArguments = calledMethod.ParameterList.ToFullString();
            var calledMethodModifiers = calledMethod.Modifiers.ToString();
            var calledMethodConstraints = calledMethod.ConstraintClauses.ToFullString();
            var actedUpon = classBeingCalled.Identifier.ValueText;
            var actor = callingClass.Identifier.ValueText;
            var calledMethodTypeParameters = calledMethod.TypeParameterList != null
                ? calledMethod.TypeParameterList.ToFullString()
                : String.Empty;
            var callingMethodTypeParameters = callingMethod.TypeParameterList != null
                ? callingMethod.TypeParameterList.ToFullString()
                : String.Empty;

            var callInfo = callingMethodName + callingMethodTypeParameters + " => " + calledMethodModifiers + " " + calledMethodReturnType + calledMethodName + calledMethodTypeParameters;

            if (includeCalledMethodArguments)
            {
                callInfo += calledMethodArguments;
            }

            callInfo += calledMethodConstraints;

            string info
                = BuildOutgoingCallInfo(actor
                                        , actedUpon
                                        , callInfo);

            Console.Write(info);
        }

        private static void PrintReturnCallInfo(
              ClassDeclarationSyntax classBeingCalled
            , ClassDeclarationSyntax callingClass
            , MethodDeclarationSyntax callingMethod
            , MethodDeclarationSyntax calledMethod)
        {

            var actedUpon = classBeingCalled.Identifier.ValueText;
            var actor = callingClass.Identifier.ValueText;
            var callerName = callingMethod.Identifier.ToFullString();
            var callingMethodTypeParameters = callingMethod.TypeParameterList != null
                ? callingMethod.TypeParameterList.ToFullString()
                : String.Empty;
            var calledMethodTypeParameters = calledMethod.TypeParameterList != null
                ? calledMethod.TypeParameterList.ToFullString()
                : String.Empty;

            var calledMethodInfo = calledMethod.Identifier.ToFullString() + calledMethodTypeParameters;

            callerName += callingMethodTypeParameters;

            var returnCallInfo = calledMethod.ReturnType.ToString();

            var returnMethodParameters = calledMethod.ParameterList.Parameters;
            foreach (var parameter in returnMethodParameters)
            {
                if (parameter.Modifiers.Any(m => m.Text == "out"))
                {
                    returnCallInfo += "," + parameter.ToFullString();
                }
            }

            string info = BuildReturnCallInfo(
                  actor
                , actedUpon
                , calledMethodInfo
                , callerName
                , returnCallInfo);

            Console.Write(info);
        }

        private static string BuildOutgoingCallInfo(string actor, string actedUpon, string callInfo)
        {
            const string calls = "->";
            const string descriptionSeparator = ": ";

            string callingInfo = actor + calls + actedUpon + descriptionSeparator + callInfo;

            callingInfo = callingInfo.RemoveNewLines(true);

            string result = callingInfo + Environment.NewLine;

            return result;
        }

        private static string BuildReturnCallInfo(string actor, string actedUpon, string calledMethodInfo, string callerName, string returnInfo)
        {
            const string returns = "-->";
            const string descriptionSeparator = ": ";

            string returningInfo = actedUpon + returns + actor + descriptionSeparator + calledMethodInfo + " returns " + returnInfo + " to " + callerName;
            returningInfo = returningInfo.RemoveNewLines(true);

            string result = returningInfo + Environment.NewLine;

            return result;
        }

        public async Task ProcessSolution()
        {
            foreach (Project project in _solution.Projects)
            {
                Compilation compilation = await project.GetCompilationAsync();
                await ProcessCompilation(compilation);
            }
        }

        #endregion
    }

    public static class StringEx
    {
        public static string RemoveNewLines(this string stringWithNewLines, bool cleanWhitespace = false)
        {
            string stringWithoutNewLines = null;
            List<char> splitElementList = Environment.NewLine.ToCharArray().ToList();

            if (cleanWhitespace)
            {
                splitElementList.AddRange(" ".ToCharArray().ToList());
            }

            char[] splitElements = splitElementList.ToArray();

            var stringElements = stringWithNewLines.Split(splitElements, StringSplitOptions.RemoveEmptyEntries);
            if (stringElements.Any())
            {
                stringWithoutNewLines = stringElements.Aggregate(stringWithoutNewLines, (current, element) => current + (current == null ? element : " " + element));
            }

            return stringWithoutNewLines ?? stringWithNewLines;
        }
    }
}
