// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.ComponentModel
{
    /// <summary>
    ///    <para>Specifies the default property for a component.</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class DefaultPropertyAttribute : Attribute
    {
        /// <summary>
        ///    <para>
        ///       Initializes a new instance of
        ///       the <see cref='System.ComponentModel.DefaultPropertyAttribute'/> class.
        ///    </para>
        /// </summary>
        public DefaultPropertyAttribute(string name)
        {
            Name = name;
        }

        /// <summary>
        ///    <para>
        ///       Gets the name of the default property for the component this attribute is
        ///       bound to.
        ///    </para>
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///    <para>
        ///       Specifies the default value for the <see cref='System.ComponentModel.DefaultPropertyAttribute'/>, which is <see langword='null'/>. This
        ///    <see langword='static '/>field is read-only. 
        ///    </para>
        /// </summary>
        public static readonly DefaultPropertyAttribute Default = new DefaultPropertyAttribute(null);

        public override bool Equals(object obj)
        {
            DefaultPropertyAttribute other = obj as DefaultPropertyAttribute;
            return (other != null) && other.Name == Name;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
