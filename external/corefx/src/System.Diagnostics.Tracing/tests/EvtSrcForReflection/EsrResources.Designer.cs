//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34014
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace EvtSrcForReflection {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class EsrResources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal EsrResources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("EvtSrcForReflection.EsrResources", typeof(EsrResources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Event0 fired..
        /// </summary>
        internal static string event_Event0 {
            get {
                return ResourceManager.GetString("event_Event0", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to EventDateTime fired: DateTime passed in: &lt;{0}&gt;.
        /// </summary>
        internal static string event_EventDateTime {
            get {
                return ResourceManager.GetString("event_EventDateTime", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to EventI fired: {0}.
        /// </summary>
        internal static string event_EventI {
            get {
                return ResourceManager.GetString("event_EventI", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Int arg after byte array: {1}.
        /// </summary>
        internal static string event_EventWithByteArrArg {
            get {
                return ResourceManager.GetString("event_EventWithByteArrArg", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {{msg}}=&quot;{0}!&quot; percentage={1}%.
        /// </summary>
        internal static string event_EventWithMoreEscapingMessage {
            get {
                return ResourceManager.GetString("event_EventWithMoreEscapingMessage", resourceCulture);
            }
        }
    }
}
