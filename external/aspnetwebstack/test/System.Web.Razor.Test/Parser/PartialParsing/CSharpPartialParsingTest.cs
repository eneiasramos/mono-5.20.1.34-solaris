// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Web.Razor.Parser;
using System.Web.Razor.Parser.SyntaxTree;
using System.Web.Razor.Test.Framework;
using System.Web.Razor.Text;
using System.Web.WebPages.TestUtils;
using Xunit;

namespace System.Web.Razor.Test.Parser.PartialParsing
{
    public class CSharpPartialParsingTest : PartialParsingTestBase<CSharpRazorCodeLanguage>
    {
        [Fact]
        public void ImplicitExpressionProvisionallyAcceptsDeleteOfIdentifierPartsIfDotRemains()
        {
            var factory = SpanFactory.CreateCsHtml();
            StringTextBuffer changed = new StringTextBuffer("foo @User. baz");
            StringTextBuffer old = new StringTextBuffer("foo @User.Name baz");
            RunPartialParseTest(new TextChange(10, 4, old, 0, changed),
                new MarkupBlock(
                    factory.Markup("foo "),
                    new ExpressionBlock(
                        factory.CodeTransition(),
                        factory.Code("User.").AsImplicitExpression(CSharpCodeParser.DefaultKeywords).Accepts(AcceptedCharacters.NonWhiteSpace)),
                    factory.Markup(" baz")),
                additionalFlags: PartialParseResult.Provisional);
        }

        [Fact]
        public void ImplicitExpressionAcceptsDeleteOfIdentifierPartsIfSomeOfIdentifierRemains()
        {
            var factory = SpanFactory.CreateCsHtml();
            StringTextBuffer changed = new StringTextBuffer("foo @Us baz");
            StringTextBuffer old = new StringTextBuffer("foo @User baz");
            RunPartialParseTest(new TextChange(7, 2, old, 0, changed),
                new MarkupBlock(
                    factory.Markup("foo "),
                    new ExpressionBlock(
                        factory.CodeTransition(),
                        factory.Code("Us").AsImplicitExpression(CSharpCodeParser.DefaultKeywords).Accepts(AcceptedCharacters.NonWhiteSpace)),
                    factory.Markup(" baz")));
        }

        [Fact]
        public void ImplicitExpressionProvisionallyAcceptsMultipleInsertionIfItCausesIdentifierExpansionAndTrailingDot()
        {
            var factory = SpanFactory.CreateCsHtml();
            StringTextBuffer changed = new StringTextBuffer("foo @User. baz");
            StringTextBuffer old = new StringTextBuffer("foo @U baz");
            RunPartialParseTest(new TextChange(6, 0, old, 4, changed),
                new MarkupBlock(
                    factory.Markup("foo "),
                    new ExpressionBlock(
                        factory.CodeTransition(),
                        factory.Code("User.").AsImplicitExpression(CSharpCodeParser.DefaultKeywords).Accepts(AcceptedCharacters.NonWhiteSpace)),
                    factory.Markup(" baz")),
                additionalFlags: PartialParseResult.Provisional);
        }

        [Fact]
        public void ImplicitExpressionAcceptsMultipleInsertionIfItOnlyCausesIdentifierExpansion()
        {
            var factory = SpanFactory.CreateCsHtml();
            StringTextBuffer changed = new StringTextBuffer("foo @barbiz baz");
            StringTextBuffer old = new StringTextBuffer("foo @bar baz");
            RunPartialParseTest(new TextChange(8, 0, old, 3, changed),
                new MarkupBlock(
                    factory.Markup("foo "),
                    new ExpressionBlock(
                        factory.CodeTransition(),
                        factory.Code("barbiz").AsImplicitExpression(CSharpCodeParser.DefaultKeywords).Accepts(AcceptedCharacters.NonWhiteSpace)),
                    factory.Markup(" baz")));
        }

        [Fact]
        public void ImplicitExpressionAcceptsIdentifierExpansionAtEndOfNonWhitespaceCharacters()
        {
            var factory = SpanFactory.CreateCsHtml();
            StringTextBuffer changed = new StringTextBuffer(@"@{
    @food
}");
            StringTextBuffer old = new StringTextBuffer(@"@{
    @foo
}");
            RunPartialParseTest(new TextChange(12, 0, old, 1, changed),
                new MarkupBlock(
                    factory.EmptyHtml(),
                    new StatementBlock(
                        factory.CodeTransition(),
                        factory.MetaCode("{").Accepts(AcceptedCharacters.None),
                        factory.Code("\r\n    ").AsStatement(),
                        new ExpressionBlock(
                            factory.CodeTransition(),
                            factory.Code("food")
                                   .AsImplicitExpression(CSharpCodeParser.DefaultKeywords, acceptTrailingDot: true)
                                   .Accepts(AcceptedCharacters.NonWhiteSpace)),
                        factory.Code("\r\n").AsStatement(),
                        factory.MetaCode("}").Accepts(AcceptedCharacters.None)),
                    factory.EmptyHtml()));
        }

        [Fact]
        public void ImplicitExpressionAcceptsIdentifierAfterDotAtEndOfNonWhitespaceCharacters()
        {
            var factory = SpanFactory.CreateCsHtml();
            StringTextBuffer changed = new StringTextBuffer(@"@{
    @foo.d
}");
            StringTextBuffer old = new StringTextBuffer(@"@{
    @foo.
}");
            RunPartialParseTest(new TextChange(13, 0, old, 1, changed),
                new MarkupBlock(
                    factory.EmptyHtml(),
                    new StatementBlock(
                        factory.CodeTransition(),
                        factory.MetaCode("{").Accepts(AcceptedCharacters.None),
                        factory.Code("\r\n    ").AsStatement(),
                        new ExpressionBlock(
                            factory.CodeTransition(),
                            factory.Code("foo.d")
                                   .AsImplicitExpression(CSharpCodeParser.DefaultKeywords, acceptTrailingDot: true)
                                   .Accepts(AcceptedCharacters.NonWhiteSpace)),
                        factory.Code("\r\n").AsStatement(),
                        factory.MetaCode("}").Accepts(AcceptedCharacters.None)),
                    factory.EmptyHtml()));
        }

        [Fact]
        public void ImplicitExpressionAcceptsDotAtEndOfNonWhitespaceCharacters()
        {
            var factory = SpanFactory.CreateCsHtml();
            StringTextBuffer changed = new StringTextBuffer(@"@{
    @foo.
}");
            StringTextBuffer old = new StringTextBuffer(@"@{
    @foo
}");
            RunPartialParseTest(new TextChange(12, 0, old, 1, changed),
                new MarkupBlock(
                    factory.EmptyHtml(),
                    new StatementBlock(
                        factory.CodeTransition(),
                        factory.MetaCode("{").Accepts(AcceptedCharacters.None),
                        factory.Code("\r\n    ").AsStatement(),
                        new ExpressionBlock(
                            factory.CodeTransition(),
                            factory.Code(@"foo.")
                                   .AsImplicitExpression(CSharpCodeParser.DefaultKeywords, acceptTrailingDot: true)
                                   .Accepts(AcceptedCharacters.NonWhiteSpace)),
                        factory.Code("\r\n").AsStatement(),
                        factory.MetaCode("}").Accepts(AcceptedCharacters.None)),
                    factory.EmptyHtml()));
        }

        [Fact]
        public void ImplicitExpressionRejectsChangeWhichWouldHaveBeenAcceptedIfLastChangeWasProvisionallyAcceptedOnDifferentSpan()
        {
            var factory = SpanFactory.CreateCsHtml();

            // Arrange
            TextChange dotTyped = new TextChange(8, 0, new StringTextBuffer("foo @foo @bar"), 1, new StringTextBuffer("foo @foo. @bar"));
            TextChange charTyped = new TextChange(14, 0, new StringTextBuffer("foo @foo. @bar"), 1, new StringTextBuffer("foo @foo. @barb"));
            TestParserManager manager = CreateParserManager();
            manager.InitializeWithDocument(dotTyped.OldBuffer);

            // Apply the dot change
            Assert.Equal(PartialParseResult.Provisional | PartialParseResult.Accepted, manager.CheckForStructureChangesAndWait(dotTyped));

            // Act (apply the identifier start char change)
            PartialParseResult result = manager.CheckForStructureChangesAndWait(charTyped);

            // Assert
            Assert.Equal(PartialParseResult.Rejected, result);
            Assert.False(manager.Parser.LastResultProvisional, "LastResultProvisional flag should have been cleared but it was not");
            ParserTestBase.EvaluateParseTree(manager.Parser.CurrentParseTree,
                new MarkupBlock(
                    factory.Markup("foo "),
                    new ExpressionBlock(
                        factory.CodeTransition(),
                        factory.Code("foo")
                               .AsImplicitExpression(CSharpCodeParser.DefaultKeywords)
                               .Accepts(AcceptedCharacters.NonWhiteSpace)),
                    factory.Markup(". "),
                    new ExpressionBlock(
                        factory.CodeTransition(),
                        factory.Code("barb")
                               .AsImplicitExpression(CSharpCodeParser.DefaultKeywords)
                               .Accepts(AcceptedCharacters.NonWhiteSpace)),
                    factory.EmptyHtml()));
        }

        [Fact]
        public void ImplicitExpressionAcceptsIdentifierTypedAfterDotIfLastChangeWasProvisionalAcceptanceOfDot()
        {
            var factory = SpanFactory.CreateCsHtml();

            // Arrange
            TextChange dotTyped = new TextChange(8, 0, new StringTextBuffer("foo @foo bar"), 1, new StringTextBuffer("foo @foo. bar"));
            TextChange charTyped = new TextChange(9, 0, new StringTextBuffer("foo @foo. bar"), 1, new StringTextBuffer("foo @foo.b bar"));
            TestParserManager manager = CreateParserManager();
            manager.InitializeWithDocument(dotTyped.OldBuffer);

            // Apply the dot change
            Assert.Equal(PartialParseResult.Provisional | PartialParseResult.Accepted, manager.CheckForStructureChangesAndWait(dotTyped));

            // Act (apply the identifier start char change)
            PartialParseResult result = manager.CheckForStructureChangesAndWait(charTyped);

            // Assert
            Assert.Equal(PartialParseResult.Accepted, result);
            Assert.False(manager.Parser.LastResultProvisional, "LastResultProvisional flag should have been cleared but it was not");
            ParserTestBase.EvaluateParseTree(manager.Parser.CurrentParseTree,
                new MarkupBlock(
                    factory.Markup("foo "),
                    new ExpressionBlock(
                        factory.CodeTransition(),
                        factory.Code("foo.b")
                               .AsImplicitExpression(CSharpCodeParser.DefaultKeywords)
                               .Accepts(AcceptedCharacters.NonWhiteSpace)),
                    factory.Markup(" bar")));
        }

        [Fact]
        public void ImplicitExpressionProvisionallyAcceptsDotAfterIdentifierInMarkup()
        {
            var factory = SpanFactory.CreateCsHtml();
            StringTextBuffer changed = new StringTextBuffer("foo @foo. bar");
            StringTextBuffer old = new StringTextBuffer("foo @foo bar");
            RunPartialParseTest(new TextChange(8, 0, old, 1, changed),
                new MarkupBlock(
                    factory.Markup("foo "),
                    new ExpressionBlock(
                        factory.CodeTransition(),
                        factory.Code("foo.")
                               .AsImplicitExpression(CSharpCodeParser.DefaultKeywords)
                               .Accepts(AcceptedCharacters.NonWhiteSpace)),
                    factory.Markup(" bar")),
                additionalFlags: PartialParseResult.Provisional);
        }

        [Fact]
        public void ImplicitExpressionAcceptsAdditionalIdentifierCharactersIfEndOfSpanIsIdentifier()
        {
            var factory = SpanFactory.CreateCsHtml();
            StringTextBuffer changed = new StringTextBuffer("foo @foob bar");
            StringTextBuffer old = new StringTextBuffer("foo @foo bar");
            RunPartialParseTest(new TextChange(8, 0, old, 1, changed),
                new MarkupBlock(
                    factory.Markup("foo "),
                    new ExpressionBlock(
                        factory.CodeTransition(),
                        factory.Code("foob")
                               .AsImplicitExpression(CSharpCodeParser.DefaultKeywords)
                               .Accepts(AcceptedCharacters.NonWhiteSpace)),
                    factory.Markup(" bar")));
        }

        [Fact]
        public void ImplicitExpressionAcceptsAdditionalIdentifierStartCharactersIfEndOfSpanIsDot()
        {
            var factory = SpanFactory.CreateCsHtml();
            StringTextBuffer changed = new StringTextBuffer("@{@foo.b}");
            StringTextBuffer old = new StringTextBuffer("@{@foo.}");
            RunPartialParseTest(new TextChange(7, 0, old, 1, changed),
                new MarkupBlock(
                    factory.EmptyHtml(),
                    new StatementBlock(
                        factory.CodeTransition(),
                        factory.MetaCode("{").Accepts(AcceptedCharacters.None),
                        factory.EmptyCSharp().AsStatement(),
                        new ExpressionBlock(
                            factory.CodeTransition(),
                            factory.Code("foo.b")
                                   .AsImplicitExpression(CSharpCodeParser.DefaultKeywords, acceptTrailingDot: true)
                                   .Accepts(AcceptedCharacters.NonWhiteSpace)),
                        factory.EmptyCSharp().AsStatement(),
                        factory.MetaCode("}").Accepts(AcceptedCharacters.None)),
                    factory.EmptyHtml()));
        }

        [Fact]
        public void ImplicitExpressionAcceptsDotIfTrailingDotsAreAllowed()
        {
            var factory = SpanFactory.CreateCsHtml();
            StringTextBuffer changed = new StringTextBuffer("@{@foo.}");
            StringTextBuffer old = new StringTextBuffer("@{@foo}");
            RunPartialParseTest(new TextChange(6, 0, old, 1, changed),
                new MarkupBlock(
                    factory.EmptyHtml(),
                    new StatementBlock(
                        factory.CodeTransition(),
                        factory.MetaCode("{").Accepts(AcceptedCharacters.None),
                        factory.EmptyCSharp().AsStatement(),
                        new ExpressionBlock(
                            factory.CodeTransition(),
                            factory.Code("foo.")
                                   .AsImplicitExpression(CSharpCodeParser.DefaultKeywords, acceptTrailingDot: true)
                                   .Accepts(AcceptedCharacters.NonWhiteSpace)),
                        factory.EmptyCSharp().AsStatement(),
                        factory.MetaCode("}").Accepts(AcceptedCharacters.None)),
                    factory.EmptyHtml()));
        }

        [Fact]
        public void ImplicitExpressionCorrectlyTriggersReparseIfIfKeywordTyped()
        {
            RunTypeKeywordTest("if");
        }

        [Fact]
        public void ImplicitExpressionCorrectlyTriggersReparseIfDoKeywordTyped()
        {
            RunTypeKeywordTest("do");
        }

        [Fact]
        public void ImplicitExpressionCorrectlyTriggersReparseIfTryKeywordTyped()
        {
            RunTypeKeywordTest("try");
        }

        [Fact]
        public void ImplicitExpressionCorrectlyTriggersReparseIfForKeywordTyped()
        {
            RunTypeKeywordTest("for");
        }

        [Fact]
        public void ImplicitExpressionCorrectlyTriggersReparseIfForEachKeywordTyped()
        {
            RunTypeKeywordTest("foreach");
        }

        [Fact]
        public void ImplicitExpressionCorrectlyTriggersReparseIfWhileKeywordTyped()
        {
            RunTypeKeywordTest("while");
        }

        [Fact]
        public void ImplicitExpressionCorrectlyTriggersReparseIfSwitchKeywordTyped()
        {
            RunTypeKeywordTest("switch");
        }

        [Fact]
        public void ImplicitExpressionCorrectlyTriggersReparseIfLockKeywordTyped()
        {
            RunTypeKeywordTest("lock");
        }

        [Fact]
        public void ImplicitExpressionCorrectlyTriggersReparseIfUsingKeywordTyped()
        {
            RunTypeKeywordTest("using");
        }

        [Fact]
        public void ImplicitExpressionCorrectlyTriggersReparseIfSectionKeywordTyped()
        {
            RunTypeKeywordTest("section");
        }

        [Fact]
        public void ImplicitExpressionCorrectlyTriggersReparseIfInheritsKeywordTyped()
        {
            RunTypeKeywordTest("inherits");
        }

        [Fact]
        public void ImplicitExpressionCorrectlyTriggersReparseIfHelperKeywordTyped()
        {
            RunTypeKeywordTest("helper");
        }

        [Fact]
        public void ImplicitExpressionCorrectlyTriggersReparseIfFunctionsKeywordTyped()
        {
            RunTypeKeywordTest("functions");
        }

        [Fact]
        public void ImplicitExpressionCorrectlyTriggersReparseIfNamespaceKeywordTyped()
        {
            RunTypeKeywordTest("namespace");
        }

        [Fact]
        public void ImplicitExpressionCorrectlyTriggersReparseIfClassKeywordTyped()
        {
            RunTypeKeywordTest("class");
        }

        [Fact]
        public void ImplicitExpressionCorrectlyTriggersReparseIfLayoutKeywordTyped()
        {
            RunTypeKeywordTest("layout");
        }
    }
}
