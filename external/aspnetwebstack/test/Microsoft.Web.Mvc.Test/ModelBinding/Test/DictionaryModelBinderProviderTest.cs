// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Mvc;
using Microsoft.Web.UnitTestUtil;
using Xunit;

namespace Microsoft.Web.Mvc.ModelBinding.Test
{
    public class DictionaryModelBinderProviderTest
    {
        [Fact]
        public void GetBinder_CorrectModelTypeAndValueProviderEntries_ReturnsBinder()
        {
            // Arrange
            ExtensibleModelBindingContext bindingContext = new ExtensibleModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(IDictionary<int, string>)),
                ModelName = "foo",
                ValueProvider = new SimpleValueProvider
                {
                    { "foo[0]", "42" },
                }
            };

            DictionaryModelBinderProvider binderProvider = new DictionaryModelBinderProvider();

            // Act
            IExtensibleModelBinder binder = binderProvider.GetBinder(null, bindingContext);

            // Assert
            Assert.IsType<DictionaryModelBinder<int, string>>(binder);
        }

        [Fact]
        public void GetBinder_ModelTypeIsIncorrect_ReturnsNull()
        {
            // Arrange
            ExtensibleModelBindingContext bindingContext = new ExtensibleModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(int)),
                ModelName = "foo",
                ValueProvider = new SimpleValueProvider
                {
                    { "foo[0]", "42" },
                }
            };

            DictionaryModelBinderProvider binderProvider = new DictionaryModelBinderProvider();

            // Act
            IExtensibleModelBinder binder = binderProvider.GetBinder(null, bindingContext);

            // Assert
            Assert.Null(binder);
        }

        [Fact]
        public void GetBinder_ValueProviderDoesNotContainPrefix_ReturnsNull()
        {
            // Arrange
            ExtensibleModelBindingContext bindingContext = new ExtensibleModelBindingContext
            {
                ModelMetadata = new EmptyModelMetadataProvider().GetMetadataForType(null, typeof(IDictionary<int, string>)),
                ModelName = "foo",
                ValueProvider = new SimpleValueProvider()
            };

            DictionaryModelBinderProvider binderProvider = new DictionaryModelBinderProvider();

            // Act
            IExtensibleModelBinder binder = binderProvider.GetBinder(null, bindingContext);

            // Assert
            Assert.Null(binder);
        }
    }
}
