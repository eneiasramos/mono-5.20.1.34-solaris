// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Http.Controllers;
using System.Web.Http.Internal;

namespace System.Web.Http.ModelBinding.Binders
{
    public sealed class DictionaryModelBinderProvider : ModelBinderProvider
    {
        public override IModelBinder GetBinder(HttpActionContext actionContext, ModelBindingContext bindingContext)
        {
            ModelBindingHelper.ValidateBindingContext(bindingContext);

            if (bindingContext.ValueProvider.ContainsPrefix(bindingContext.ModelName))
            {
                return CollectionModelBinderUtil.GetGenericBinder(typeof(IDictionary<,>), typeof(Dictionary<,>), typeof(DictionaryModelBinder<,>), bindingContext.ModelMetadata);
            }
            else
            {
                return null;
            }
        }
    }
}
