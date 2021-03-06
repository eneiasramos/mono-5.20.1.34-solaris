// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Globalization;
using System.Reflection;
using Microsoft.Internal;

namespace System.ComponentModel.Composition
{
    internal static class ExceptionBuilder
    {
        public static Exception CreateDiscoveryException(string messageFormat, params string[] arguments)
        {
            // DiscoveryError (Dev10:602872): This should go through the discovery error reporting when 
            // we add a way to report discovery errors properly.
            return new InvalidOperationException(Format(messageFormat, arguments));
        }

        public static ArgumentException CreateContainsNullElement(string parameterName)
        {
            Assumes.NotNull(parameterName);

            string message = Format(Strings.Argument_NullElement, parameterName);

            return new ArgumentException(message, parameterName);
        }

        public static ObjectDisposedException CreateObjectDisposed(object instance)
        {
            Assumes.NotNull(instance);

            return new ObjectDisposedException(instance.GetType().ToString());
        }

        public static NotImplementedException CreateNotOverriddenByDerived(string memberName)
        {
            Assumes.NotNullOrEmpty(memberName);

            string message = Format(Strings.NotImplemented_NotOverriddenByDerived, memberName);

            return new NotImplementedException(message);
        }

        public static ArgumentException CreateExportDefinitionNotOnThisComposablePart(string parameterName)
        {
            Assumes.NotNullOrEmpty(parameterName);

            string message = Format(Strings.ExportDefinitionNotOnThisComposablePart, parameterName);

            return new ArgumentException(message, parameterName);
        }

        public static ArgumentException CreateImportDefinitionNotOnThisComposablePart(string parameterName)
        {
            Assumes.NotNullOrEmpty(parameterName);

            string message = Format(Strings.ImportDefinitionNotOnThisComposablePart, parameterName);

            return new ArgumentException(message, parameterName);
        }

        public static CompositionException CreateCannotGetExportedValue(ComposablePart part, ExportDefinition definition, Exception innerException)
        {
            Assumes.NotNull(part, definition, innerException);

            return new CompositionException(
                ErrorBuilder.CreateCannotGetExportedValue(part, definition, innerException));
        }

        public static ArgumentException CreateReflectionModelInvalidPartDefinition(string parameterName, Type partDefinitionType)
        {
            Assumes.NotNullOrEmpty(parameterName);
            Assumes.NotNull(partDefinitionType);

            return new ArgumentException(string.Format(CultureInfo.CurrentCulture, Strings.ReflectionModel_InvalidPartDefinition, partDefinitionType), parameterName);
        }

        public static ArgumentException ExportFactory_TooManyGenericParameters(string typeName)
        {
            Assumes.NotNullOrEmpty(typeName);

            string message = Format(Strings.ExportFactory_TooManyGenericParameters, typeName);

            return new ArgumentException(message, typeName);
        }

        private static string Format(string format, params string[] arguments)
        {
            return String.Format(CultureInfo.CurrentCulture, format, arguments);
        }
    }
}
