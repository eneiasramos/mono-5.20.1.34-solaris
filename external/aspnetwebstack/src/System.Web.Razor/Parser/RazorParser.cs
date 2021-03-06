// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Razor.Parser.SyntaxTree;
using System.Web.Razor.Text;

namespace System.Web.Razor.Parser
{
    public class RazorParser
    {
        public RazorParser(ParserBase codeParser, ParserBase markupParser)
        {
            if (codeParser == null)
            {
                throw new ArgumentNullException("codeParser");
            }
            if (markupParser == null)
            {
                throw new ArgumentNullException("markupParser");
            }

            MarkupParser = markupParser;
            CodeParser = codeParser;
            Optimizers = new List<ISyntaxTreeRewriter>()
            {
                // Move whitespace from start of expression block to markup
                new WhiteSpaceRewriter(MarkupParser.BuildSpan),
                // Collapse conditional attributes where the entire value is literal
                new ConditionalAttributeCollapser(MarkupParser.BuildSpan),
                // Collapse sibling Markup Spans
                // TODO: Fix and restore Markup Collapser post-beta.
                //new MarkupCollapser(MarkupParser.BuildSpan)
            };
        }

        internal ParserBase CodeParser { get; private set; }
        internal ParserBase MarkupParser { get; private set; }
        internal IList<ISyntaxTreeRewriter> Optimizers { get; private set; }

        public bool DesignTimeMode { get; set; }

        public virtual void Parse(TextReader input, ParserVisitor visitor)
        {
            Parse(new SeekableTextReader(input), visitor);
        }

        public virtual ParserResults Parse(TextReader input)
        {
            return ParseCore(new SeekableTextReader(input));
        }

        public virtual ParserResults Parse(ITextDocument input)
        {
            return ParseCore(input);
        }

#pragma warning disable 0618
        [Obsolete("Lookahead-based readers have been deprecated, use overrides which accept a TextReader or ITextDocument instead")]
        public virtual void Parse(LookaheadTextReader input, ParserVisitor visitor)
        {
            ParserResults results = ParseCore(new SeekableTextReader(input));

            // Replay the results on the visitor
            visitor.Visit(results);
        }

        [Obsolete("Lookahead-based readers have been deprecated, use overrides which accept a TextReader or ITextDocument instead")]
        public virtual ParserResults Parse(LookaheadTextReader input)
        {
            return ParseCore(new SeekableTextReader(input));
        }
#pragma warning restore 0618

        public virtual Task CreateParseTask(TextReader input, Action<Span> spanCallback, Action<RazorError> errorCallback)
        {
            return CreateParseTask(input, new CallbackVisitor(spanCallback, errorCallback));
        }

        public virtual Task CreateParseTask(TextReader input, Action<Span> spanCallback, Action<RazorError> errorCallback, SynchronizationContext context)
        {
            return CreateParseTask(input, new CallbackVisitor(spanCallback, errorCallback) { SynchronizationContext = context });
        }

        public virtual Task CreateParseTask(TextReader input, Action<Span> spanCallback, Action<RazorError> errorCallback, CancellationToken cancelToken)
        {
            return CreateParseTask(input, new CallbackVisitor(spanCallback, errorCallback) { CancelToken = cancelToken });
        }

        public virtual Task CreateParseTask(TextReader input, Action<Span> spanCallback, Action<RazorError> errorCallback, SynchronizationContext context, CancellationToken cancelToken)
        {
            return CreateParseTask(input, new CallbackVisitor(spanCallback, errorCallback)
            {
                SynchronizationContext = context,
                CancelToken = cancelToken
            });
        }

        [SuppressMessage("Microsoft.WebAPI", "CR4002:DoNotConstructTaskInstances", Justification = "This rule is not applicable to this assembly.")]
        public virtual Task CreateParseTask(TextReader input,
                                            ParserVisitor consumer)
        {
            return new Task(() =>
            {
                try
                {
                    Parse(input, consumer);
                }
                catch (OperationCanceledException)
                {
                    return; // Just return if we're cancelled.
                }
            });
        }

        private ParserResults ParseCore(ITextDocument input)
        {
            // Setup the parser context
            ParserContext context = new ParserContext(input, CodeParser, MarkupParser, MarkupParser)
            {
                DesignTimeMode = DesignTimeMode
            };

            MarkupParser.Context = context;
            CodeParser.Context = context;

            // Execute the parse
            MarkupParser.ParseDocument();

            // Get the result
            ParserResults results = context.CompleteParse();

            // Rewrite whitespace if supported
            Block current = results.Document;
            foreach (ISyntaxTreeRewriter rewriter in Optimizers)
            {
                current = rewriter.Rewrite(current);
            }

            // Return the new result
            return new ParserResults(current, results.ParserErrors);
        }
    }
}
