﻿namespace StyleCop.Analyzers.OrderingRules
{
    using System;
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    /// <summary>
    /// The using-alias directives within a C# code file are not sorted alphabetically by alias name.
    /// </summary>
    /// <remarks>
    /// <para>A violation of this rule occurs when the using-alias directives are not sorted alphabetically by alias
    /// name. Sorting the using-alias directives alphabetically can make the code cleaner and easier to read, and can
    /// help make it easier to identify the namespaces that are being used by the code.</para>
    /// </remarks>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SA1211UsingAliasDirectivesMustBeOrderedAlphabeticallyByAliasName : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "SA1211";
        internal const string Title = "Using alias directives must be ordered alphabetically by alias name";
        internal const string MessageFormat = "Using alias directive for '{0}' must appear before using alias directive for '{1}'";
        internal const string Category = "StyleCop.CSharp.OrderingRules";
        internal const string Description = "The using-alias directives within a C# code file are not sorted alphabetically by alias name.";
        internal const string HelpLink = "http://www.stylecop.com/docs/SA1211.html";

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
            context.RegisterSyntaxNodeAction(HandleUsingDirectiveSyntax, SyntaxKind.UsingDirective);
        }

        private void HandleUsingDirectiveSyntax(SyntaxNodeAnalysisContext context)
        {
            UsingDirectiveSyntax syntax = context.Node as UsingDirectiveSyntax;
            if (syntax.Alias?.Name?.IsMissing != false)
                return;

            CompilationUnitSyntax compilationUnit = syntax.Parent as CompilationUnitSyntax;
            SyntaxList<UsingDirectiveSyntax>? usingDirectives = compilationUnit?.Usings;
            if (!usingDirectives.HasValue)
            {
                NamespaceDeclarationSyntax namespaceDeclaration = syntax.Parent as NamespaceDeclarationSyntax;
                usingDirectives = namespaceDeclaration?.Usings;
            }

            if (!usingDirectives.HasValue)
                return;

            foreach (var usingDirective in usingDirectives)
            {
                // we are only interested in nodes before the current node
                if (usingDirective == syntax)
                    continue;

                // only interested in using alias directives
                if (usingDirective.Alias?.Name?.IsMissing != false)
                    continue;

                string alias = syntax.Alias.Name.ToString();
                string precedingAlias = usingDirective.Alias.Name.ToString();
                if (string.Compare(alias, precedingAlias, StringComparison.OrdinalIgnoreCase) >= 0)
                    continue;


                // Using alias directive for '{alias}' must appear before using alias directive for '{precedingAlias}'
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, syntax.GetLocation(), alias, precedingAlias));
                break;
            }
        }
    }
}
