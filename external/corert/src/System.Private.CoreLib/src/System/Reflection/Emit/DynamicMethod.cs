// Licensed to the.NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Globalization;

namespace System.Reflection.Emit
{
    public sealed class DynamicMethod : MethodInfo
    {
        public DynamicMethod(string name, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] parameterTypes, Module m, bool skipVisibility)
        {
            ReflectionEmitThrower.ThrowPlatformNotSupportedException();
        }

        public DynamicMethod(string name, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] parameterTypes, Type owner, bool skipVisibility)
        {
            ReflectionEmitThrower.ThrowPlatformNotSupportedException();
        }

        public DynamicMethod(string name, Type returnType, Type[] parameterTypes)
        {
            ReflectionEmitThrower.ThrowPlatformNotSupportedException();
        }

        public DynamicMethod(string name, Type returnType, Type[] parameterTypes, bool restrictedSkipVisibility)
        {
            ReflectionEmitThrower.ThrowPlatformNotSupportedException();
        }

        public DynamicMethod(string name, Type returnType, Type[] parameterTypes, Module m)
        {
            ReflectionEmitThrower.ThrowPlatformNotSupportedException();
        }

        public DynamicMethod(string name, Type returnType, Type[] parameterTypes, Module m, bool skipVisibility)
        {
            ReflectionEmitThrower.ThrowPlatformNotSupportedException();
        }

        public DynamicMethod(string name, Type returnType, Type[] parameterTypes, Type owner)
        {
            ReflectionEmitThrower.ThrowPlatformNotSupportedException();
        }

        public DynamicMethod(string name, Type returnType, Type[] parameterTypes, Type owner, bool skipVisibility)
        {
            ReflectionEmitThrower.ThrowPlatformNotSupportedException();
        }

        public override MethodAttributes Attributes
        {
            get
            {
                return default;
            }
        }

        public override CallingConventions CallingConvention
        {
            get
            {
                return default;
            }
        }

        public override Type DeclaringType
        {
            get
            {
                return default;
            }
        }

        public bool InitLocals
        {
            get
            {
                return default;
            }
            set
            {
            }
        }

        public override RuntimeMethodHandle MethodHandle
        {
            get
            {
                return default;
            }
        }

        public override string Name
        {
            get
            {
                return default;
            }
        }

        public override Type ReflectedType
        {
            get
            {
                return default;
            }
        }

        public override ParameterInfo ReturnParameter
        {
            get
            {
                return default;
            }
        }

        public override Type ReturnType
        {
            get
            {
                return default;
            }
        }

        public override ICustomAttributeProvider ReturnTypeCustomAttributes
        {
            get
            {
                return default;
            }
        }

        public sealed override Delegate CreateDelegate(Type delegateType)
        {
            return default;
        }

        public sealed override Delegate CreateDelegate(Type delegateType, object target)
        {
            return default;
        }

        public override MethodInfo GetBaseDefinition()
        {
            return default;
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            return default;
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return default;
        }

        public ILGenerator GetILGenerator()
        {
            return default;
        }

        public ILGenerator GetILGenerator(int streamSize)
        {
            return default;
        }

        public override MethodImplAttributes GetMethodImplementationFlags()
        {
            return default;
        }

        public override ParameterInfo[] GetParameters()
        {
            return default;
        }

        public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
        {
            return default;
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return default;
        }

        public override string ToString()
        {
            return default;
        }
    }
}
