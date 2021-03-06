// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.ComponentModel.Composition.Primitives;

namespace System.ComponentModel.Composition
{
    /// <summary>
    ///     Specifies metadata for a type to be used as a <see cref="ComposablePartDefinition"/> and
    ///     <see cref="ComposablePart"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class PartMetadataAttribute : Attribute
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PartMetadataAttribute"/> with the 
        ///     specified name and metadata value.
        /// </summary>
        /// <param name="name">
        ///     A <see cref="String"/> containing the name of the metadata value; or 
        ///     <see langword="null"/> to use an empty string ("").
        /// </param>
        /// <param name="value">
        ///     An <see cref="object"/> containing the metadata value. This can be 
        ///     <see langword="null"/>.
        /// </param>
        public PartMetadataAttribute(string name, object value)
        {
            this.Name = name ?? string.Empty;
            this.Value = value;
        }

        /// <summary>
        ///     Gets the name of the metadata value.
        /// </summary>
        /// <value>
        ///     A <see cref="String"/> containing the name of the metadata value.
        /// </value>
        public string Name
        {
            get;
            private set;
        }

        /// <summary>
        ///     Gets the metadata value.
        /// </summary>
        /// <value>
        ///     An <see cref="object"/> containing the metadata value.
        /// </value>
        public object Value
        {
            get;
            private set;
        }
    }
}