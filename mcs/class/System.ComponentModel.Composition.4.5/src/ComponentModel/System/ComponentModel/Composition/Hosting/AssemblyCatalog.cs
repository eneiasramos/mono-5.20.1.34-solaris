// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Primitives;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Threading;
using Microsoft.Internal;

namespace System.ComponentModel.Composition.Hosting
{
    /// <summary>
    ///     An immutable ComposablePartCatalog created from a managed code assembly.
    /// </summary>
    /// <remarks>
    ///     This type is thread safe.
    /// </remarks>
    [DebuggerTypeProxy(typeof(AssemblyCatalogDebuggerProxy))]
    public class AssemblyCatalog : ComposablePartCatalog, ICompositionElement
    {
        private readonly object _thisLock = new object();
        private readonly ICompositionElement _definitionOrigin;
        private volatile Assembly _assembly = null;
        private volatile ComposablePartCatalog _innerCatalog = null;
        private int _isDisposed = 0;

#if FEATURE_REFLECTIONCONTEXT
        private ReflectionContext _reflectionContext = default(ReflectionContext);
#endif //FEATURE_REFLECTIONCONTEXT

#if FEATURE_REFLECTIONFILEIO
        /// <summary>
        ///     Initializes a new instance of the <see cref="AssemblyCatalog"/> class 
        ///     with the specified code base.
        /// </summary>
        /// <param name="codeBase">
        ///     A <see cref="String"/> containing the code base of the assembly containing the
        ///     attributed <see cref="Type"/> objects to add to the <see cref="AssemblyCatalog"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="codeBase"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="codeBase"/> is a zero-length string, contains only white space, 
        ///     or contains one or more invalid characters as defined by <see cref="Path.InvalidPathChars"/>.
        /// </exception>
        /// <exception cref="PathTooLongException">
        ///     The specified path, file name, or both exceed the system-defined maximum length. 
        /// </exception>
        /// <exception cref="SecurityException">
        ///     The caller does not have path discovery permission. 
        /// </exception>
        /// <exception cref="FileNotFoundException">
        ///     <paramref name="codeBase"/> is not found.
        /// </exception>
        /// <exception cref="FileLoadException ">
        ///     <paramref name="codeBase"/> could not be loaded.
        ///     <para>
        ///         -or-
        ///     </para>
        ///     <paramref name="codeBase"/> specified a directory.
        /// </exception>
        /// <exception cref="BadImageFormatException">
        ///     <paramref name="codeBase"/> is not a valid assembly
        ///     -or- 
        ///     Version 2.0 or later of the common language runtime is currently loaded 
        ///     and <paramref name="codeBase"/> was compiled with a later version. 
        /// </exception>
        /// <remarks>
        ///     The assembly referenced by <paramref langword="codeBase"/> is loaded into the Load context.
        /// </remarks>
        public AssemblyCatalog(string codeBase)
        {
            Requires.NotNullOrEmpty(codeBase, "codeBase");

            InitializeAssemblyCatalog(LoadAssembly(codeBase));
            this._definitionOrigin = this;
        }
#endif //FEATURE_REFLECTIONFILEIO

#if FEATURE_REFLECTIONCONTEXT
        /// <summary>
        ///     Initializes a new instance of the <see cref="AssemblyCatalog"/> class 
        ///     with the specified code base.
        /// </summary>
        /// <param name="codeBase">
        ///     A <see cref="String"/> containing the code base of the assembly containing the
        ///     attributed <see cref="Type"/> objects to add to the <see cref="AssemblyCatalog"/>.
        /// </param>
        /// <param name="reflectionContext">
        ///     The <see cref="ReflectionContext"/> a context used by the catalog when 
        ///     interpreting the types to inject attributes into the type definition<see cref="AssemblyCatalog"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="codeBase"/> is <see langword="null"/>.
        ///     <para>
        ///         -or-
        ///     </para>
        ///     <paramref name="reflectionContext"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="codeBase"/> is a zero-length string, contains only white space, 
        ///     or contains one or more invalid characters as defined by <see cref="Path.InvalidPathChars"/>.
        /// </exception>
        /// <exception cref="PathTooLongException">
        ///     The specified path, file name, or both exceed the system-defined maximum length. 
        /// </exception>
        /// <exception cref="SecurityException">
        ///     The caller does not have path discovery permission. 
        /// </exception>
        /// <exception cref="FileNotFoundException">
        ///     <paramref name="codeBase"/> is not found.
        /// </exception>
        /// <exception cref="FileLoadException ">
        ///     <paramref name="codeBase"/> could not be loaded.
        ///     <para>
        ///         -or-
        ///     </para>
        ///     <paramref name="codeBase"/> specified a directory.
        /// </exception>
        /// <exception cref="BadImageFormatException">
        ///     <paramref name="codeBase"/> is not a valid assembly
        ///     -or- 
        ///     Version 2.0 or later of the common language runtime is currently loaded 
        ///     and <paramref name="codeBase"/> was compiled with a later version. 
        /// </exception>
        /// <remarks>
        ///     The assembly referenced by <paramref langword="codeBase"/> is loaded into the Load context.
        /// </remarks>
        public AssemblyCatalog(string codeBase, ReflectionContext reflectionContext)
        {
            Requires.NotNullOrEmpty(codeBase, "codeBase");
            Requires.NotNull(reflectionContext, "reflectionContext");

            InitializeAssemblyCatalog(LoadAssembly(codeBase));
            this._reflectionContext = reflectionContext;
            this._definitionOrigin = this;
        }
#endif //FEATURE_REFLECTIONCONTEXT

#if FEATURE_REFLECTIONFILEIO
        /// <summary>
        ///     Initializes a new instance of the <see cref="AssemblyCatalog"/> class 
        ///     with the specified code base.
        /// </summary>
        /// <param name="codeBase">
        ///     A <see cref="String"/> containing the code base of the assembly containing the
        ///     attributed <see cref="Type"/> objects to add to the <see cref="AssemblyCatalog"/>.
        /// </param>
        /// <param name="definitionOrigin">
        ///     The <see cref="ICompositionElement"/> CompositionElement used by Diagnostics to identify the source for parts.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="codeBase"/> is <see langword="null"/>.
        ///     <para>
        ///         -or-
        ///     </para>
        ///     <paramref name="definitionOrigin"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="codeBase"/> is a zero-length string, contains only white space, 
        ///     or contains one or more invalid characters as defined by <see cref="Path.InvalidPathChars"/>.
        /// </exception>
        /// <exception cref="PathTooLongException">
        ///     The specified path, file name, or both exceed the system-defined maximum length. 
        /// </exception>
        /// <exception cref="SecurityException">
        ///     The caller does not have path discovery permission. 
        /// </exception>
        /// <exception cref="FileNotFoundException">
        ///     <paramref name="codeBase"/> is not found.
        /// </exception>
        /// <exception cref="FileLoadException ">
        ///     <paramref name="codeBase"/> could not be loaded.
        ///     <para>
        ///         -or-
        ///     </para>
        ///     <paramref name="codeBase"/> specified a directory.
        /// </exception>
        /// <exception cref="BadImageFormatException">
        ///     <paramref name="codeBase"/> is not a valid assembly
        ///     -or- 
        ///     Version 2.0 or later of the common language runtime is currently loaded 
        ///     and <paramref name="codeBase"/> was compiled with a later version. 
        /// </exception>
        /// <remarks>
        ///     The assembly referenced by <paramref langword="codeBase"/> is loaded into the Load context.
        /// </remarks>
        public AssemblyCatalog(string codeBase, ICompositionElement definitionOrigin)
        {
            Requires.NotNullOrEmpty(codeBase, "codeBase");
            Requires.NotNull(definitionOrigin, "definitionOrigin");

            InitializeAssemblyCatalog(LoadAssembly(codeBase));
            this._definitionOrigin = definitionOrigin;
        }
#endif //FEATURE_REFLECTIONFILEIO

#if FEATURE_REFLECTIONFILEIO && FEATURE_REFLECTIONCONTEXT
        /// <summary>
        ///     Initializes a new instance of the <see cref="AssemblyCatalog"/> class 
        ///     with the specified code base.
        /// </summary>
        /// <param name="codeBase">
        ///     A <see cref="String"/> containing the code base of the assembly containing the
        ///     attributed <see cref="Type"/> objects to add to the <see cref="AssemblyCatalog"/>.
        /// </param>
        /// <param name="reflectionContext">
        ///     The <see cref="ReflectionContext"/> a context used by the catalog when 
        ///     interpreting the types to inject attributes into the type definition<see cref="AssemblyCatalog"/>.
        /// </param>
        /// <param name="definitionOrigin">
        ///     The <see cref="ICompositionElement"/> CompositionElement used by Diagnostics to identify the source for parts.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="codeBase"/> is <see langword="null"/>.
        ///     <para>
        ///         -or-
        ///     </para>
        ///     <paramref name="reflectionContext"/> is <see langword="null"/>.
        ///     <para>
        ///         -or-
        ///     </para>
        ///     <paramref name="definitionOrigin"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="codeBase"/> is a zero-length string, contains only white space, 
        ///     or contains one or more invalid characters as defined by <see cref="Path.InvalidPathChars"/>.
        /// </exception>
        /// <exception cref="PathTooLongException">
        ///     The specified path, file name, or both exceed the system-defined maximum length. 
        /// </exception>
        /// <exception cref="SecurityException">
        ///     The caller does not have path discovery permission. 
        /// </exception>
        /// <exception cref="FileNotFoundException">
        ///     <paramref name="codeBase"/> is not found.
        /// </exception>
        /// <exception cref="FileLoadException ">
        ///     <paramref name="codeBase"/> could not be loaded.
        ///     <para>
        ///         -or-
        ///     </para>
        ///     <paramref name="codeBase"/> specified a directory.
        /// </exception>
        /// <exception cref="BadImageFormatException">
        ///     <paramref name="codeBase"/> is not a valid assembly
        ///     -or- 
        ///     Version 2.0 or later of the common language runtime is currently loaded 
        ///     and <paramref name="codeBase"/> was compiled with a later version. 
        /// </exception>
        /// <remarks>
        ///     The assembly referenced by <paramref langword="codeBase"/> is loaded into the Load context.
        /// </remarks>
        public AssemblyCatalog(string codeBase, ReflectionContext reflectionContext, ICompositionElement definitionOrigin)
        {
            Requires.NotNullOrEmpty(codeBase, "codeBase");
            Requires.NotNull(reflectionContext, "reflectionContext");
            Requires.NotNull(definitionOrigin, "definitionOrigin");

            InitializeAssemblyCatalog(LoadAssembly(codeBase));
            this._reflectionContext = reflectionContext;
            this._definitionOrigin = definitionOrigin;
        }
#endif //FEATURE_REFLECTIONFILEIO && FEATURE_REFLECTIONCONTEXT

#if FEATURE_REFLECTIONCONTEXT
        /// <summary>
        ///     Initializes a new instance of the <see cref="AssemblyCatalog"/> class 
        ///     with the specified assembly and reflection context.
        /// </summary>
        /// <param name="assembly">
        ///     The <see cref="Assembly"/> containing the attributed <see cref="Type"/> objects to 
        ///     add to the <see cref="AssemblyCatalog"/>.
        /// </param>
        /// <param name="reflectionContext">
        ///     The <see cref="ReflectionContext"/> a context used by the catalog when 
        ///     interpreting the types to inject attributes into the type definition.
        /// </param>
        /// <exception cref="ArgumentException">
        ///     <paramref name="assembly"/> is <see langword="null"/>.
        ///     <para>
        ///         -or-
        ///     </para>    
        ///     <paramref name="assembly"/> was loaded in the reflection-only context.
        ///     <para>
        ///         -or-
        ///     </para>    
        ///     <paramref name="reflectionContext"/> is <see langword="null"/>.
        /// </exception>
        public AssemblyCatalog(Assembly assembly, ReflectionContext reflectionContext)
        {
            Requires.NotNull(assembly, "assembly");
            Requires.NotNull(reflectionContext, "reflectionContext");

            InitializeAssemblyCatalog(assembly);
            this._reflectionContext = reflectionContext;
            this._definitionOrigin = this;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="AssemblyCatalog"/> class 
        ///     with the specified assembly, reflectionContext and definitionOrigin.
        /// </summary>
        /// <param name="assembly">
        ///     The <see cref="Assembly"/> containing the attributed <see cref="Type"/> objects to 
        ///     add to the <see cref="AssemblyCatalog"/>.
        /// </param>
        /// <param name="reflectionContext">
        ///     The <see cref="ReflectionContext"/> a context used by the catalog when 
        ///     interpreting the types to inject attributes into the type definition.
        /// </param>
        /// <param name="definitionOrigin">
        ///     The <see cref="ICompositionElement"/> CompositionElement used by Diagnostics to identify the source for parts.
        /// </param>
        /// <exception cref="ArgumentException">
        ///     <paramref name="assembly"/> is <see langword="null"/>.
        ///     <para>
        ///         -or-
        ///     </para>    
        ///     <paramref name="assembly"/> was loaded in the reflection-only context.
        ///     <para>
        ///         -or-
        ///     </para>    
        ///     <paramref name="reflectionContext"/> is <see langword="null"/>.
        ///     <para>
        ///         -or-
        ///     </para>    
        ///     <paramref name="definitionOrigin"/> is <see langword="null"/>.
        /// </exception>
        public AssemblyCatalog(Assembly assembly, ReflectionContext reflectionContext, ICompositionElement definitionOrigin)
        {
            Requires.NotNull(assembly, "assembly");
            Requires.NotNull(reflectionContext, "reflectionContext");
            Requires.NotNull(definitionOrigin, "definitionOrigin");

            InitializeAssemblyCatalog(assembly);
            this._reflectionContext = reflectionContext;
            this._definitionOrigin = definitionOrigin;
        }
#endif //FEATURE_REFLECTIONCONTEXT

        /// <summary>
        ///     Initializes a new instance of the <see cref="AssemblyCatalog"/> class 
        ///     with the specified assembly.
        /// </summary>
        /// <param name="assembly">
        ///     The <see cref="Assembly"/> containing the attributed <see cref="Type"/> objects to 
        ///     add to the <see cref="AssemblyCatalog"/>.
        /// </param>
        /// <exception cref="ArgumentException">
        ///     <paramref name="assembly"/> is <see langword="null"/>.
        ///     <para>
        ///         -or-
        ///     </para>    
        ///     <paramref name="assembly"/> was loaded in the reflection-only context.
        /// </exception>
        public AssemblyCatalog(Assembly assembly)
        {
            Requires.NotNull(assembly, "assembly");

            InitializeAssemblyCatalog(assembly);
            this._definitionOrigin = this;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="AssemblyCatalog"/> class 
        ///     with the specified assembly.
        /// </summary>
        /// <param name="assembly">
        ///     The <see cref="Assembly"/> containing the attributed <see cref="Type"/> objects to 
        ///     add to the <see cref="AssemblyCatalog"/>.
        /// </param>
        /// <param name="definitionOrigin">
        ///     The <see cref="ICompositionElement"/> CompositionElement used by Diagnostics to identify the source for parts.
        /// </param>
        /// <exception cref="ArgumentException">
        ///     <paramref name="assembly"/> is <see langword="null"/>.
        ///     <para>
        ///         -or-
        ///     </para>    
        ///     <paramref name="assembly"/> was loaded in the reflection-only context.
        ///     <para>
        ///         -or-
        ///     </para>    
        ///     <paramref name="definitionOrigin"/> is <see langword="null"/>.
        /// </exception>
        public AssemblyCatalog(Assembly assembly, ICompositionElement definitionOrigin)
        {
            Requires.NotNull(assembly, "assembly");
            Requires.NotNull(definitionOrigin, "definitionOrigin");

            InitializeAssemblyCatalog(assembly);
            this._definitionOrigin = definitionOrigin;
        }

        private void InitializeAssemblyCatalog(Assembly assembly)
        {
#if FEATURE_REFLECTIONONLY
            if (assembly.ReflectionOnly)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Strings.Argument_AssemblyReflectionOnly, "assembly"), "assembly");
            }
#endif //FEATURE_REFLECTIONONLY
            this._assembly = assembly;
        }

        /// <summary>
        ///     Returns the export definitions that match the constraint defined by the specified definition.
        /// </summary>
        /// <param name="definition">
        ///     The <see cref="ImportDefinition"/> that defines the conditions of the 
        ///     <see cref="ExportDefinition"/> objects to return.
        /// </param>
        /// <returns>
        ///     An <see cref="IEnumerable{T}"/> of <see cref="Tuple{T1, T2}"/> containing the 
        ///     <see cref="ExportDefinition"/> objects and their associated 
        ///     <see cref="ComposablePartDefinition"/> for objects that match the constraint defined 
        ///     by <paramref name="definition"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="definition"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        ///     The <see cref="ComposablePartCatalog"/> has been disposed of.
        /// </exception>
        /// <remarks>
        ///     <note type="inheritinfo">
        ///         Overriders of this property should never return <see langword="null"/>, if no 
        ///         <see cref="ExportDefinition"/> match the conditions defined by 
        ///         <paramref name="definition"/>, return an empty <see cref="IEnumerable{T}"/>.
        ///     </note>
        /// </remarks>
        [SuppressMessage("Microsoft.Contracts", "CC1055", Justification = "Precondition is being validated in the call to inner catalog")]  
        public override IEnumerable<Tuple<ComposablePartDefinition, ExportDefinition>> GetExports(ImportDefinition definition)
        {
            return this.InnerCatalog.GetExports(definition);
        }

        private ComposablePartCatalog InnerCatalog
        {
            get
            {
                this.ThrowIfDisposed();

                if (this._innerCatalog == null)
                {
#if FEATURE_REFLECTIONCONTEXT
                    var catalogReflectionContextAttribute = this._assembly.GetFirstAttribute<CatalogReflectionContextAttribute>();
                    var assembly = (catalogReflectionContextAttribute != null) 
                        ? catalogReflectionContextAttribute.CreateReflectionContext().MapAssembly(this._assembly)
                        : this._assembly;
#else
                    var assembly = this._assembly;
#endif //FEATURE_REFLECTIONCONTEXT
                    lock (this._thisLock)
                    {
                        if (this._innerCatalog == null)
                        {
#if FEATURE_REFLECTIONCONTEXT
                            var catalog = (this._reflectionContext != null) 
                                ? new TypeCatalog(assembly.GetTypes(), this._reflectionContext, this._definitionOrigin)
                                : new TypeCatalog(assembly.GetTypes(), this._definitionOrigin);
#else
                            var catalog = new TypeCatalog(assembly.GetTypes(), this._definitionOrigin);
#endif //FEATURE_REFLECTIONCONTEXT
                            Thread.MemoryBarrier();
                            this._innerCatalog = catalog;
                        }
                    }
                }
                return this._innerCatalog;
            }
        }

        /// <summary>
        ///     Gets the assembly containing the attributed types contained within the assembly
        ///     catalog.
        /// </summary>
        /// <value>
        ///     The <see cref="Assembly"/> containing the attributed <see cref="Type"/> objects
        ///     contained within the <see cref="AssemblyCatalog"/>.
        /// </value>
        public Assembly Assembly
        {
            get 
            {
                Contract.Ensures(Contract.Result<Assembly>() != null);
                
                return this._assembly; 
            }
        }

        /// <summary>
        ///     Gets the display name of the assembly catalog.
        /// </summary>
        /// <value>
        ///     A <see cref="String"/> containing a human-readable display name of the <see cref="AssemblyCatalog"/>.
        /// </value>
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        string ICompositionElement.DisplayName
        {
            get { return this.GetDisplayName(); }
        }

        /// <summary>
        ///     Gets the composition element from which the assembly catalog originated.
        /// </summary>
        /// <value>
        ///     This property always returns <see langword="null"/>.
        /// </value>
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        ICompositionElement ICompositionElement.Origin
        {
            get { return null; }
        }


        /// <summary>
        ///     Returns a string representation of the assembly catalog.
        /// </summary>
        /// <returns>
        ///     A <see cref="String"/> containing the string representation of the <see cref="AssemblyCatalog"/>.
        /// </returns>
        public override string ToString()
        {
            return this.GetDisplayName();
        }


        protected override void Dispose(bool disposing)
        {                
            try
            {
                if (Interlocked.CompareExchange(ref this._isDisposed, 1, 0) == 0)
                {
                    if (disposing)
                    {
                        if (this._innerCatalog != null)
                        {
                            this._innerCatalog.Dispose();
                        }
                    }
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        public override IEnumerator<ComposablePartDefinition> GetEnumerator()
        {
            return this.InnerCatalog.GetEnumerator();
        }

        private void ThrowIfDisposed()
        {
            if (this._isDisposed == 1)
            {
                throw ExceptionBuilder.CreateObjectDisposed(this);
            }
        }

        private string GetDisplayName()
        {
            return string.Format(CultureInfo.CurrentCulture,
                                "{0} (Assembly=\"{1}\")",   // NOLOC
                                GetType().Name, 
                                this.Assembly.FullName);
        }

#if FEATURE_REFLECTIONFILEIO
        private static Assembly LoadAssembly(string codeBase)
        {
            Requires.NotNullOrEmpty(codeBase, "codeBase");

            AssemblyName assemblyName;

            try
            {
                assemblyName = AssemblyName.GetAssemblyName(codeBase);
            }
            catch (ArgumentException)
            {
                assemblyName = new AssemblyName();
                assemblyName.CodeBase = codeBase;
            }

            return Assembly.Load(assemblyName);
        }
#endif //FEATURE_REFLECTIONFILEIO
    }
}
