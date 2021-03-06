// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Globalization;
using System.Web.Razor.Text;

namespace System.Web.Razor.Parser.SyntaxTree
{
    public class RazorError : IEquatable<RazorError>
    {
        public RazorError(string message, SourceLocation location)
            : this(message, location, 1)
        {
        }

        public RazorError(string message, int absoluteIndex, int lineIndex, int columnIndex)
            : this(message, new SourceLocation(absoluteIndex, lineIndex, columnIndex))
        {
        }

        public RazorError(string message, SourceLocation location, int length)
        {
            Message = message;
            Location = location;
            Length = length;
        }

        public RazorError(string message, int absoluteIndex, int lineIndex, int columnIndex, int length)
            : this(message, new SourceLocation(absoluteIndex, lineIndex, columnIndex), length)
        {
        }

        public string Message { get; private set; }
        public SourceLocation Location { get; private set; }
        public int Length { get; private set; }

        public override string ToString()
        {
            return String.Format(CultureInfo.CurrentCulture, "Error @ {0}({2}) - [{1}]", Location, Message, Length);
        }

        public override bool Equals(object obj)
        {
            RazorError err = obj as RazorError;
            return (err != null) && (Equals(err));
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public bool Equals(RazorError other)
        {
            return String.Equals(other.Message, Message, StringComparison.Ordinal) &&
                   Location.Equals(other.Location);
        }
    }
}
