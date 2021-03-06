//
// ExpandSegmentCollectionTests.cs
//
// Author:
//   Eric Maupin  <me@ermau.com>
//
// Copyright (c) 2009 Eric Maupin (http://www.ermau.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Data.Services;
using System.Linq.Expressions;
using NUnit.Framework;

namespace MonoTests.System.Data.Services {
	[TestFixture]
	public class ExpandSegmentCollectionTests {
		[Test]
		public void CtorCapacity()
		{
			var esc = new ExpandSegmentCollection (20);
			Assert.AreEqual (20, esc.Capacity);
		}

		[Test]
		public void HasFilterInit()
		{
			var esc = new ExpandSegmentCollection();
			Assert.IsFalse (esc.HasFilter);
		}

		[Test]
		public void HasFilterAddNull()
		{
			var esc = new ExpandSegmentCollection();
			esc.Add (new ExpandSegment ("first", null));
			Assert.IsFalse (esc.HasFilter);
		}

		[Test]
		public void HasFilterAddWithFilter()
		{
			var esc = new ExpandSegmentCollection();
			esc.Add (new ExpandSegment ("first", null));

			var param = Expression.Parameter (typeof (bool), "b");
			var filter = Expression.Lambda (param, param);
			var filteredSegment = new ExpandSegment ("second", filter);
			esc.Add (filteredSegment);
			Assert.IsTrue (esc.HasFilter);
		}

		[Test]
		public void HasFilterRemoveFiltered()
		{
			var esc = new ExpandSegmentCollection();
			esc.Add (new ExpandSegment ("first", null));

			var param = Expression.Parameter (typeof (bool), "b");
			var filter = Expression.Lambda (param, param);
			var filteredSegment = new ExpandSegment ("second", filter);
			esc.Add (filteredSegment);
			esc.Remove (filteredSegment);

			Assert.IsFalse (esc.HasFilter);
		}

		[Test]
		public void HasFilterRemoveFilteredMultiple()
		{
			var esc = new ExpandSegmentCollection();
			esc.Add (new ExpandSegment ("first", null));

			var param = Expression.Parameter (typeof (bool), "b");
			var filter = Expression.Lambda (param, param);
			var filteredSegment = new ExpandSegment ("second", filter);
			esc.Add (filteredSegment);
			esc.Add (filteredSegment);

			esc.Remove (filteredSegment);
			Assert.IsTrue (esc.HasFilter);

			esc.Remove (filteredSegment);
			Assert.IsFalse (esc.HasFilter);
		}
	}
}
