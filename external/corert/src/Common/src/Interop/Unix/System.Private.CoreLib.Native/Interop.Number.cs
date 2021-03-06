// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

internal static partial class Interop
{
    internal unsafe partial class Sys
    {
#if !MONO
        [DllImport(Interop.Libraries.CoreLibNative, EntryPoint = "CoreLibNative_DoubleToString")]
#else
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
#endif
        internal static extern unsafe int DoubleToString(double value, byte* format, byte* buffer, int bufferLength);
    }
}
