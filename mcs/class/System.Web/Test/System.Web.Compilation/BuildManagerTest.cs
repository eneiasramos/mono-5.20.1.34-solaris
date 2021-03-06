//
// BuildManagerTest.cs
//
// Author:
//      Marek Habersack <mhabersack@novell.com>
//
// Copyright (C) 2010 Novell, Inc (http://novell.com)
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
using System;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.Compilation;
using System.Web.Hosting;

using NUnit.Framework;


namespace MonoTests.System.Web.Compilation
{
	[TestFixture]
	[Serializable]
	public class BuildManagerTest
	{
		[Test]
		[Ignore ("Pending investigation if it is indeed the correct test.")]
		public void GetGlobalAsaxType ()
		{
			Assert.Throws<InvalidOperationException> (() => {
				BuildManager.GetGlobalAsaxType ();
			}, "#A1");
		}

		[Test]
		public void TargetFramework ()
		{
			Assert.AreEqual (".NETFramework,Version=v4.0", BuildManager.TargetFramework.FullName, "#A1-1");
			Assert.AreEqual (".NETFramework", BuildManager.TargetFramework.Identifier, "#A1-2");
			Assert.AreEqual ("", BuildManager.TargetFramework.Profile, "#A1-3");
			Assert.AreEqual (new Version (4, 0), BuildManager.TargetFramework.Version, "#A1-4");
		}
	}
}
