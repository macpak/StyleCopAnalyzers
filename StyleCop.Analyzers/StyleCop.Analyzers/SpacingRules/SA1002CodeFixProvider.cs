﻿namespace StyleCop.Analyzers.SpacingRules
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;

    /// <summary>
    /// Implements a code fix for <see cref="SA1002SemicolonsMustBeSpacedCorrectly"/>.
    /// </summary>
    /// <remarks>
    /// <para>To fix a violation of this rule, ensure that the semicolon is followed by a single space, and is not
    /// preceded by any space.</para>
    /// </remarks>
    [ExportCodeFixProvider(nameof(SA1002CodeFixProvider), LanguageNames.CSharp)]
    [Shared]
    public class SA1002CodeFixProvider : CodeFixProvider
    {
        private static readonly ImmutableArray<string> _fixableDiagnostics =
            ImmutableArray.Create(SA1002SemicolonsMustBeSpacedCorrectly.DiagnosticId);

        /// <inheritdoc/>
        public override ImmutableArray<string> GetFixableDiagnosticIds()
        {
            return _fixableDiagnostics;
        }

        /// <inheritdoc/>
        public override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        /// <inheritdoc/>
        public override async Task ComputeFixesAsync(CodeFixContext context)
        {
            foreach (var diagnostic in context.Diagnostics)
            {
                if (!diagnostic.Id.Equals(SA1002SemicolonsMustBeSpacedCorrectly.DiagnosticId))
                    continue;

                var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
                SyntaxToken token = root.FindToken(diagnostic.Location.SourceSpan.Start);
                if (!token.IsKind(SyntaxKind.SemicolonToken))
                    continue;

                Dictionary<SyntaxToken, SyntaxToken> replacements = new Dictionary<SyntaxToken, SyntaxToken>();

                // check for a following space
                bool missingFollowingSpace = true;
                if (token.HasTrailingTrivia)
                {
                    if (token.TrailingTrivia.First().IsKind(SyntaxKind.WhitespaceTrivia))
                        missingFollowingSpace = false;
                    else if (token.TrailingTrivia.First().IsKind(SyntaxKind.EndOfLineTrivia))
                        missingFollowingSpace = false;
                }

                bool firstInLine = token.HasLeadingTrivia || token.GetLocation()?.GetMappedLineSpan().StartLinePosition.Character == 0;
                if (!firstInLine)
                {
                    SyntaxToken precedingToken = token.GetPreviousToken();
                    if (precedingToken.TrailingTrivia.Any(SyntaxKind.WhitespaceTrivia))
                    {
                        SyntaxToken corrected = precedingToken.WithoutLeadingWhitespace();
                        replacements[precedingToken] = corrected;
                    }
                }

                if (missingFollowingSpace)
                {
                    SyntaxToken intermediate = token.WithoutTrailingWhitespace();
                    SyntaxToken corrected = intermediate.WithTrailingTrivia(intermediate.TrailingTrivia.Insert(0, SyntaxFactory.Whitespace(" ")));
                    replacements[token] = corrected;
                }

                var transformed = root.ReplaceTokens(replacements.Keys, (original, maybeRewritten) => replacements[original]);
                Document updatedDocument = context.Document.WithSyntaxRoot(transformed);
                context.RegisterFix(CodeAction.Create("Fix spacing", updatedDocument), diagnostic);
            }
        }
    }
}
