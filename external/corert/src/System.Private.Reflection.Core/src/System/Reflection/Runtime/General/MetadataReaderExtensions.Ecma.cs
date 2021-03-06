// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Runtime.Assemblies;

using Internal.LowLevelLinq;
using Internal.Reflection.Core;

using Internal.Runtime.Augments;

using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;

namespace System.Reflection.Runtime.General
{
    //
    // Collect various metadata reading tasks for better chunking...
    //
    internal static class EcmaMetadataReaderExtensions
    {
        //
        // Used to split methods between DeclaredMethods and DeclaredConstructors.
        //
        public static bool IsConstructor(this MethodDefinitionHandle methodHandle, MetadataReader reader)
        {
            MethodDefinition method = reader.GetMethodDefinition(methodHandle);
            return EcmaMetadataHelpers.IsConstructor(ref method, reader);
        }

        public static PrimitiveTypeCode GetPrimitiveTypeCode(this Type type)
        {
            if (type == CommonRuntimeTypes.Object)
                return PrimitiveTypeCode.Object;
            else if (type == CommonRuntimeTypes.Boolean)
                return PrimitiveTypeCode.Boolean;
            else if (type == CommonRuntimeTypes.Char)
                return PrimitiveTypeCode.Char;
            else if (type == CommonRuntimeTypes.Double)
                return PrimitiveTypeCode.Double;
            else if (type == CommonRuntimeTypes.Single)
                return PrimitiveTypeCode.Single;
            else if (type == CommonRuntimeTypes.Int16)
                return PrimitiveTypeCode.Int16;
            else if (type == CommonRuntimeTypes.Int32)
                return PrimitiveTypeCode.Int32;
            else if (type == CommonRuntimeTypes.Int64)
                return PrimitiveTypeCode.Int64;
            else if (type == CommonRuntimeTypes.SByte)
                return PrimitiveTypeCode.SByte;
            else if (type == CommonRuntimeTypes.UInt16)
                return PrimitiveTypeCode.UInt16;
            else if (type == CommonRuntimeTypes.UInt32)
                return PrimitiveTypeCode.UInt32;
            else if (type == CommonRuntimeTypes.UInt64)
                return PrimitiveTypeCode.UInt64;
            else if (type == CommonRuntimeTypes.Byte)
                return PrimitiveTypeCode.Byte;
            else if (type == CommonRuntimeTypes.IntPtr)
                return PrimitiveTypeCode.IntPtr;
            else if (type == CommonRuntimeTypes.UIntPtr)
                return PrimitiveTypeCode.UIntPtr;
            else if (type == CommonRuntimeTypes.String)
                return PrimitiveTypeCode.String;
            else if (type == CommonRuntimeTypes.Void)
                return PrimitiveTypeCode.Void;
            
            throw new ArgumentException();
        }

        public static Type GetRuntimeType(this PrimitiveTypeCode primitiveCode)
        {
            switch(primitiveCode)
            {
                case PrimitiveTypeCode.Boolean:
                    return CommonRuntimeTypes.Boolean;
                case PrimitiveTypeCode.Byte:
                    return CommonRuntimeTypes.Byte;
                case PrimitiveTypeCode.Char:
                    return CommonRuntimeTypes.Char;
                case PrimitiveTypeCode.Double:
                    return CommonRuntimeTypes.Double;
                case PrimitiveTypeCode.Int16:
                    return CommonRuntimeTypes.Int16;
                case PrimitiveTypeCode.Int32:
                    return CommonRuntimeTypes.Int32;
                case PrimitiveTypeCode.Int64:
                    return CommonRuntimeTypes.Int64;
                case PrimitiveTypeCode.IntPtr:
                    return CommonRuntimeTypes.IntPtr;
                case PrimitiveTypeCode.Object:
                    return CommonRuntimeTypes.Object;
                case PrimitiveTypeCode.SByte:
                    return CommonRuntimeTypes.SByte;
                case PrimitiveTypeCode.Single:
                    return CommonRuntimeTypes.Single;
                case PrimitiveTypeCode.String:
                    return CommonRuntimeTypes.String;
                case PrimitiveTypeCode.TypedReference:
                    throw new PlatformNotSupportedException();
                case PrimitiveTypeCode.UInt16:
                    return CommonRuntimeTypes.UInt16;
                case PrimitiveTypeCode.UInt32:
                    return CommonRuntimeTypes.UInt32;
                case PrimitiveTypeCode.UInt64:
                    return CommonRuntimeTypes.UInt64;
                case PrimitiveTypeCode.UIntPtr:
                    return CommonRuntimeTypes.UIntPtr;
                case PrimitiveTypeCode.Void:
                    return CommonRuntimeTypes.Void;
            }

            throw new BadImageFormatException();
        }        
    }
}
