// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Web.Razor.Text;
using System.Web.WebPages.TestUtils;
using Moq;
using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Razor.Test.Text
{
    public class TextChangeTest
    {
        [Fact]
        public void ConstructorRequiresNonNegativeOldPosition()
        {
            Assert.ThrowsArgumentOutOfRange(() => new TextChange(-1, 0, new Mock<ITextBuffer>().Object, 0, 0, new Mock<ITextBuffer>().Object), "oldPosition", "Value must be greater than or equal to 0.");
        }

        [Fact]
        public void ConstructorRequiresNonNegativeNewPosition()
        {
            Assert.ThrowsArgumentOutOfRange(() => new TextChange(0, 0, new Mock<ITextBuffer>().Object, -1, 0, new Mock<ITextBuffer>().Object), "newPosition", "Value must be greater than or equal to 0.");
        }

        [Fact]
        public void ConstructorRequiresNonNegativeOldLength()
        {
            Assert.ThrowsArgumentOutOfRange(() => new TextChange(0, -1, new Mock<ITextBuffer>().Object, 0, 0, new Mock<ITextBuffer>().Object), "oldLength", "Value must be greater than or equal to 0.");
        }

        [Fact]
        public void ConstructorRequiresNonNegativeNewLength()
        {
            Assert.ThrowsArgumentOutOfRange(() => new TextChange(0, 0, new Mock<ITextBuffer>().Object, 0, -1, new Mock<ITextBuffer>().Object), "newLength", "Value must be greater than or equal to 0.");
        }

        [Fact]
        public void ConstructorRequiresNonNullOldBuffer()
        {
            Assert.ThrowsArgumentNull(() => new TextChange(0, 0, null, 0, 0, new Mock<ITextBuffer>().Object), "oldBuffer");
        }

        [Fact]
        public void ConstructorRequiresNonNullNewBuffer()
        {
            Assert.ThrowsArgumentNull(() => new TextChange(0, 0, new Mock<ITextBuffer>().Object, 0, 0, null), "newBuffer");
        }

        [Fact]
        public void ConstructorInitializesProperties()
        {
            // Act
            ITextBuffer oldBuffer = new Mock<ITextBuffer>().Object;
            ITextBuffer newBuffer = new Mock<ITextBuffer>().Object;
            TextChange change = new TextChange(42, 24, oldBuffer, 1337, newBuffer);

            // Assert
            Assert.Equal(42, change.OldPosition);
            Assert.Equal(24, change.OldLength);
            Assert.Equal(1337, change.NewLength);
            Assert.Same(newBuffer, change.NewBuffer);
            Assert.Same(oldBuffer, change.OldBuffer);
        }

        [Fact]
        public void TestIsDelete()
        {
            // Arrange 
            ITextBuffer oldBuffer = new Mock<ITextBuffer>().Object;
            ITextBuffer newBuffer = new Mock<ITextBuffer>().Object;
            TextChange change = new TextChange(0, 1, oldBuffer, 0, newBuffer);

            // Assert
            Assert.True(change.IsDelete);
        }

        [Fact]
        public void TestIsInsert()
        {
            // Arrange 
            ITextBuffer oldBuffer = new Mock<ITextBuffer>().Object;
            ITextBuffer newBuffer = new Mock<ITextBuffer>().Object;
            TextChange change = new TextChange(0, 0, oldBuffer, 35, newBuffer);

            // Assert
            Assert.True(change.IsInsert);
        }

        [Fact]
        public void TestIsReplace()
        {
            // Arrange 
            ITextBuffer oldBuffer = new Mock<ITextBuffer>().Object;
            ITextBuffer newBuffer = new Mock<ITextBuffer>().Object;
            TextChange change = new TextChange(0, 5, oldBuffer, 10, newBuffer);

            // Assert
            Assert.True(change.IsReplace);
        }

        [Fact]
        public void OldTextReturnsOldSpanFromOldBuffer()
        {
            // Arrange
            var newBuffer = new StringTextBuffer("test");
            var oldBuffer = new StringTextBuffer("text");
            var textChange = new TextChange(2, 1, oldBuffer, 1, newBuffer);

            // Act
            string text = textChange.OldText;

            // Assert
            Assert.Equal("x", text);
        }

        [Fact]
        public void NewTextWithInsertReturnsChangedTextFromBuffer()
        {
            // Arrange
            var newBuffer = new StringTextBuffer("test");
            var oldBuffer = new StringTextBuffer("");
            var textChange = new TextChange(0, 0, oldBuffer, 3, newBuffer);

            // Act
            string text = textChange.NewText;

            // Assert
            Assert.Equal("tes", text);
        }

        [Fact]
        public void NewTextWithDeleteReturnsEmptyString()
        {
            // Arrange
            var newBuffer = new StringTextBuffer("test");
            var oldBuffer = new StringTextBuffer("");
            var textChange = new TextChange(1, 1, oldBuffer, 0, newBuffer);

            // Act
            string text = textChange.NewText;

            // Assert
            Assert.Equal(String.Empty, text);
        }

        [Fact]
        public void NewTextWithReplaceReturnsChangedTextFromBuffer()
        {
            // Arrange
            var newBuffer = new StringTextBuffer("test");
            var oldBuffer = new StringTextBuffer("");
            var textChange = new TextChange(2, 2, oldBuffer, 1, newBuffer);

            // Act
            string text = textChange.NewText;

            // Assert
            Assert.Equal("s", text);
        }

        [Fact]
        public void ApplyChangeWithInsertedTextReturnsNewContentWithChangeApplied()
        {
            // Arrange
            var newBuffer = new StringTextBuffer("test");
            var oldBuffer = new StringTextBuffer("");
            var textChange = new TextChange(0, 0, oldBuffer, 3, newBuffer);

            // Act
            string text = textChange.ApplyChange("abcd", 0);

            // Assert
            Assert.Equal("tesabcd", text);
        }

        [Fact]
        public void ApplyChangeWithRemovedTextReturnsNewContentWithChangeApplied()
        {
            // Arrange
            var newBuffer = new StringTextBuffer("abcdefg");
            var oldBuffer = new StringTextBuffer("");
            var textChange = new TextChange(1, 1, oldBuffer, 0, newBuffer);

            // Act
            string text = textChange.ApplyChange("abcdefg", 1);

            // Assert
            Assert.Equal("bcdefg", text);
        }

        [Fact]
        public void ApplyChangeWithReplacedTextReturnsNewContentWithChangeApplied()
        {
            // Arrange
            var newBuffer = new StringTextBuffer("abcdefg");
            var oldBuffer = new StringTextBuffer("");
            var textChange = new TextChange(1, 1, oldBuffer, 2, newBuffer);

            // Act
            string text = textChange.ApplyChange("abcdefg", 1);

            // Assert
            Assert.Equal("bcbcdefg", text);
        }

        [Fact]
        public void NormalizeFixesUpIntelliSenseStyleReplacements()
        {
            // Arrange
            var newBuffer = new StringTextBuffer("Date.");
            var oldBuffer = new StringTextBuffer("Date");
            var original = new TextChange(0, 4, oldBuffer, 5, newBuffer);

            // Act
            TextChange normalized = original.Normalize();

            // Assert
            Assert.Equal(new TextChange(4, 0, oldBuffer, 1, newBuffer), normalized);
        }

        [Fact]
        public void NormalizeDoesntAffectChangesWithoutCommonPrefixes()
        {
            // Arrange
            var newBuffer = new StringTextBuffer("DateTime.");
            var oldBuffer = new StringTextBuffer("Date.");
            var original = new TextChange(0, 5, oldBuffer, 9, newBuffer);

            // Act
            TextChange normalized = original.Normalize();

            // Assert
            Assert.Equal(original, normalized);
        }

        [Fact]
        public void NormalizeDoesntAffectShrinkingReplacements()
        {
            // Arrange
            var newBuffer = new StringTextBuffer("D");
            var oldBuffer = new StringTextBuffer("DateTime");
            var original = new TextChange(0, 8, oldBuffer, 1, newBuffer);

            // Act
            TextChange normalized = original.Normalize();

            // Assert
            Assert.Equal(original, normalized);
        }
    }
}
