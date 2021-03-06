﻿namespace StyleCop.Analyzers.ReadabilityRules
{
    using System.Collections.Immutable;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    /// <summary>
    /// The C# code contains a region within the body of a code element.
    /// </summary>
    /// <remarks>
    /// <para>A violation of this rule occurs whenever a region is placed within the body of a code element. In many
    /// editors, including Visual Studio, the region will appear collapsed by default, hiding the code within the
    /// region. It is generally a bad practice to hide code within the body of an element, as this can lead to bad
    /// decisions as the code is maintained over time.</para>
    /// </remarks>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SA1123DoNotPlaceRegionsWithinElements : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "SA1123";
        internal const string Title = "Do not place regions within elements";
        internal const string MessageFormat = "Region must not be located within a code element.";
        internal const string Category = "StyleCop.CSharp.ReadabilityRules";
        internal const string Description = "The C# code contains a region within the body of a code element.";
        internal const string HelpLink = "http://www.stylecop.com/docs/SA1123.html";

        public static readonly DiagnosticDescriptor Descriptor =
            new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, AnalyzerConstants.DisabledNoTests, Description, HelpLink);

        private static readonly ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics =
            ImmutableArray.Create(Descriptor);

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return _supportedDiagnostics;
            }
        }

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxTreeAction(HandleSyntaxTree);
        }

        private void HandleSyntaxTree(SyntaxTreeAnalysisContext context)
        {
            SyntaxNode root = context.Tree.GetCompilationUnitRoot(context.CancellationToken);
            foreach (var trivia in root.DescendantTrivia(descendIntoTrivia: true))
            {
                switch (trivia.CSharpKind())
                {
                case SyntaxKind.RegionDirectiveTrivia:
                    HandleRegionDirectiveTrivia(context, trivia);
                    break;

                default:
                    break;
                }
            }
        }

        private void HandleRegionDirectiveTrivia(SyntaxTreeAnalysisContext context, SyntaxTrivia trivia)
        {
            BlockSyntax blockSyntax = trivia.Token.Parent.AncestorsAndSelf().OfType<BlockSyntax>().FirstOrDefault();
            if (blockSyntax == null)
                return;

            // Region must not be located within a code element.
            context.ReportDiagnostic(Diagnostic.Create(Descriptor, trivia.GetLocation()));
        }
    }
}
