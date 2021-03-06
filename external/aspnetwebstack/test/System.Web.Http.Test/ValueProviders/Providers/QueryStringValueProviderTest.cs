// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Specialized;
using Xunit;

namespace System.Web.Http.ValueProviders.Providers
{
    public class QueryStringValueProviderTest
    {
        [Fact]
        public void ParseQueryString_Null()
        {
            // Act
            NameValueCollection result = QueryStringValueProvider.ParseQueryString(null);

            // Assert
            Assert.Equal(0, result.Count);
        }

        [Fact]
        public void ParseQueryString_SingleNamelessValue()
        {
            // Arrange
            Uri uri = new Uri("http://localhost/?key");

            // Act
            NameValueCollection result = QueryStringValueProvider.ParseQueryString(uri);

            // Assert
            string key = Assert.Single(result) as string;
            Assert.Equal("key", key);
            Assert.Equal("", result[key]);
        }

        [Fact]
        public void ParseQueryString_SingleNamedValue()
        {
            // Arrange
            Uri uri = new Uri("http://localhost/?key1=value1");

            // Act
            NameValueCollection result = QueryStringValueProvider.ParseQueryString(uri);

            // Assert
            string key = Assert.Single(result) as string;
            Assert.Equal("key1", key);
            Assert.Equal("value1", result[key]);
        }

        [Fact]
        public void ParseQueryString_TwoNamedValues()
        {
            // Arrange
            Uri uri = new Uri("http://localhost/?key1=value1&key2=value2");

            // Act
            NameValueCollection result = QueryStringValueProvider.ParseQueryString(uri);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("value1", result["key1"]);
            Assert.Equal("value2", result["key2"]);
        }

        [Fact]
        public void ParseQueryString_MixedNamedAndUnnamedValues()
        {
            // Arrange
            Uri uri = new Uri("http://localhost/?key1=value1&key2");

            // Act
            NameValueCollection result = QueryStringValueProvider.ParseQueryString(uri);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("value1", result["key1"]);
            Assert.Equal("", result["key2"]);
            Assert.Equal(null, result[""]);
        }

        [Fact]
        public void ParseQueryString_MultipleValuesForSingleName()
        {
            // Arrange
            Uri uri = new Uri("http://localhost/?key1=value1&key1=value2");

            // Act
            NameValueCollection result = QueryStringValueProvider.ParseQueryString(uri);

            // Assert
            Assert.Equal("value1,value2", result["key1"]);
            Assert.Equal(new[] { "value1", "value2" }, result.GetValues("key1"));
        }

        [Fact]
        public void ParseQueryString_LeadingAmpersand()
        {
            // Arrange
            Uri uri = new Uri("http://localhost/?&key1=value1");

            // Act
            NameValueCollection result = QueryStringValueProvider.ParseQueryString(uri);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("value1", result["key1"]);
            Assert.Equal("", result[""]);
        }

        [Fact]
        public void ParseQueryString_IntermediateDoubleAmpersand()
        {
            // Arrange
            Uri uri = new Uri("http://localhost/?key1=value1&&key2=value2");

            // Act
            NameValueCollection result = QueryStringValueProvider.ParseQueryString(uri);

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Equal("value1", result["key1"]);
            Assert.Equal("value2", result["key2"]);
            Assert.Equal("", result[""]);
        }

        [Fact]
        public void ParseQueryString_TrailingAmpersand()
        {
            // Arrange
            Uri uri = new Uri("http://localhost/?key1=value1&");

            // Act
            NameValueCollection result = QueryStringValueProvider.ParseQueryString(uri);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("value1", result["key1"]);
            Assert.Equal("", result[""]);
        }

        [Fact]
        public void ParseQueryString_EncodedUrlValues()
        {
            // Arrange
            Uri uri = new Uri("http://localhost/?key%31=value%31");

            // Act
            NameValueCollection result = QueryStringValueProvider.ParseQueryString(uri);

            // Assert
            Assert.Single(result);
            Assert.Equal("value1", result["key1"]);
        }
    }
}
