// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Threading;
using System.Web.Razor.Editor;
using System.Web.Razor.Parser;
using System.Web.Razor.Parser.SyntaxTree;
using System.Web.Razor.Resources;
using System.Web.Razor.Test.Framework;
using System.Web.Razor.Test.Utils;
using System.Web.Razor.Text;
using System.Web.WebPages.TestUtils;
using Microsoft.CSharp;
using Moq;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Razor.Test.Editor
{
    public class RazorEditorParserTest
    {
        private static readonly TestFile SimpleCSHTMLDocument = TestFile.Create("DesignTime.Simple.cshtml");
        private static readonly TestFile SimpleCSHTMLDocumentGenerated = TestFile.Create("DesignTime.Simple.txt");
        private const string TestLinePragmaFileName = "C:\\This\\Path\\Is\\Just\\For\\Line\\Pragmas.cshtml";

        [Fact]
        public void ConstructorRequiresNonNullHost()
        {
            Assert.ThrowsArgumentNull(() => new RazorEditorParser(null, TestLinePragmaFileName),
                                          "host");
        }

        [Fact]
        public void ConstructorRequiresNonNullPhysicalPath()
        {
            Assert.ThrowsArgumentNullOrEmptyString(() => new RazorEditorParser(CreateHost(), null),
                                                 "sourceFileName");
        }

        [Fact]
        public void ConstructorRequiresNonEmptyPhysicalPath()
        {
            Assert.ThrowsArgumentNullOrEmptyString(() => new RazorEditorParser(CreateHost(), String.Empty),
                                                 "sourceFileName");
        }

        [Fact]
        public void TreesAreDifferentReturnsTrueIfTreeStructureIsDifferent()
        {
            var factory = SpanFactory.CreateCsHtml();
            Block original = new MarkupBlock(
                factory.Markup("<p>"),
                new ExpressionBlock(
                    factory.CodeTransition()),
                factory.Markup("</p>"));
            Block modified = new MarkupBlock(
                factory.Markup("<p>"),
                new ExpressionBlock(
                    factory.CodeTransition("@"),
                    factory.Code("f")
                           .AsImplicitExpression(CSharpCodeParser.DefaultKeywords, acceptTrailingDot: false)),
                factory.Markup("</p>"));
            ITextBuffer oldBuffer = new StringTextBuffer("<p>@</p>");
            ITextBuffer newBuffer = new StringTextBuffer("<p>@f</p>");
            Assert.True(RazorEditorParser.TreesAreDifferent(
                original, modified, new[] {
                    new TextChange(position: 4, oldLength: 0, oldBuffer: oldBuffer, newLength: 1, newBuffer: newBuffer)
                }));
        }

        [Fact]
        public void TreesAreDifferentReturnsFalseIfTreeStructureIsSame()
        {
            var factory = SpanFactory.CreateCsHtml();
            Block original = new MarkupBlock(
                factory.Markup("<p>"),
                new ExpressionBlock(
                    factory.CodeTransition(),
                    factory.Code("f")
                           .AsImplicitExpression(CSharpCodeParser.DefaultKeywords, acceptTrailingDot: false)),
                factory.Markup("</p>"));
            factory.Reset();
            Block modified = new MarkupBlock(
                factory.Markup("<p>"),
                new ExpressionBlock(
                    factory.CodeTransition(),
                    factory.Code("foo")
                           .AsImplicitExpression(CSharpCodeParser.DefaultKeywords, acceptTrailingDot: false)),
                factory.Markup("</p>"));
            original.LinkNodes();
            modified.LinkNodes();
            ITextBuffer oldBuffer = new StringTextBuffer("<p>@f</p>");
            ITextBuffer newBuffer = new StringTextBuffer("<p>@foo</p>");
            Assert.False(RazorEditorParser.TreesAreDifferent(
                original, modified, new[] {
                    new TextChange(position: 5, oldLength: 0, oldBuffer: oldBuffer, newLength: 2, newBuffer: newBuffer)
                }));
        }

        [Fact]
        public void CheckForStructureChangesRequiresNonNullBufferInChange()
        {
            TextChange change = new TextChange();
            Assert.ThrowsArgument(
                () => new RazorEditorParser(
                    CreateHost(),
                    "C:\\Foo.cshtml").CheckForStructureChanges(change),
                "change",
                String.Format(RazorResources.Structure_Member_CannotBeNull, "Buffer", "TextChange"));
        }

        private static RazorEngineHost CreateHost()
        {
            return new RazorEngineHost(new CSharpRazorCodeLanguage()) { DesignTimeMode = true };
        }

        [Fact]
        public void CheckForStructureChangesStartsReparseAndFiresDocumentParseCompletedEventIfNoAdditionalChangesQueued()
        {
            // Arrange
            using (RazorEditorParser parser = CreateClientParser())
            {
                StringTextBuffer input = new StringTextBuffer(SimpleCSHTMLDocument.ReadAllText());

                DocumentParseCompleteEventArgs capturedArgs = null;
                ManualResetEventSlim parseComplete = new ManualResetEventSlim(false);

                parser.DocumentParseComplete += (sender, args) =>
                {
                    capturedArgs = args;
                    parseComplete.Set();
                };

                // Act
                parser.CheckForStructureChanges(new TextChange(0, 0, new StringTextBuffer(String.Empty), input.Length, input));

                // Assert
                MiscUtils.DoWithTimeoutIfNotDebugging(parseComplete.Wait);
                Assert.Equal(
                    SimpleCSHTMLDocumentGenerated.ReadAllText(),
                    MiscUtils.StripRuntimeVersion(
                        capturedArgs.GeneratorResults.GeneratedCode.GenerateCode<CSharpCodeProvider>()
                    ));
            }
        }

        [Fact]
        public void CheckForStructureChangesStartsFullReparseIfChangeOverlapsMultipleSpans()
        {
            // Arrange
            RazorEditorParser parser = new RazorEditorParser(CreateHost(), TestLinePragmaFileName);
            ITextBuffer original = new StringTextBuffer("Foo @bar Baz");
            ITextBuffer changed = new StringTextBuffer("Foo @bap Daz");
            TextChange change = new TextChange(7, 3, original, 3, changed);

            ManualResetEventSlim parseComplete = new ManualResetEventSlim();
            int parseCount = 0;
            parser.DocumentParseComplete += (sender, args) =>
            {
                Interlocked.Increment(ref parseCount);
                parseComplete.Set();
            };

            Assert.Equal(PartialParseResult.Rejected, parser.CheckForStructureChanges(new TextChange(0, 0, new StringTextBuffer(String.Empty), 12, original)));
            MiscUtils.DoWithTimeoutIfNotDebugging(parseComplete.Wait); // Wait for the parse to finish
            parseComplete.Reset();

            // Act
            PartialParseResult result = parser.CheckForStructureChanges(change);

            // Assert
            Assert.Equal(PartialParseResult.Rejected, result);
            MiscUtils.DoWithTimeoutIfNotDebugging(parseComplete.Wait);
            Assert.Equal(2, parseCount);
        }

        private static void SetupMockWorker(Mock<RazorEditorParser> parser, ManualResetEventSlim running)
        {
            SetUpMockWorker(parser, running, null);
        }

        private static void SetUpMockWorker(Mock<RazorEditorParser> parser, ManualResetEventSlim running, Action<int> backgroundThreadIdReceiver)
        {
            parser.Setup(p => p.CreateBackgroundTask(It.IsAny<RazorEngineHost>(), It.IsAny<string>(), It.IsAny<TextChange>()))
                  .Returns<RazorEngineHost, string, TextChange>((host, fileName, change) =>
                  {
                      var task = new Mock<BackgroundParseTask>(new RazorTemplateEngine(host), fileName, change) { CallBase = true };
                      task.Setup(t => t.Run(It.IsAny<CancellationToken>()))
                          .Callback<CancellationToken>((token) =>
                          {
                              if (backgroundThreadIdReceiver != null)
                              {
                                  backgroundThreadIdReceiver(Thread.CurrentThread.ManagedThreadId);
                              }
                              running.Set();
                              while (!token.IsCancellationRequested)
                              {
                              }
                          });
                      return task.Object;
                  });
        }

        private TextChange CreateDummyChange()
        {
            return new TextChange(0, 0, new StringTextBuffer(String.Empty), 3, new StringTextBuffer("foo"));
        }

        private static Mock<RazorEditorParser> CreateMockParser()
        {
            return new Mock<RazorEditorParser>(CreateHost(), TestLinePragmaFileName) { CallBase = true };
        }

        private static RazorEditorParser CreateClientParser()
        {
            return new RazorEditorParser(CreateHost(), TestLinePragmaFileName);
        }
    }
}
