namespace GTweak.Properties {
    using System;
    
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("GTweak.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        internal static byte[] Blank {
            get {
                object obj = ResourceManager.GetObject("Blank", resourceCulture);
                return ((byte[])(obj));
            }
        }
        
        internal static byte[] GTweak {
            get {
                object obj = ResourceManager.GetObject("GTweak", resourceCulture);
                return ((byte[])(obj));
            }
        }
        
        internal static byte[] hosts {
            get {
                object obj = ResourceManager.GetObject("hosts", resourceCulture);
                return ((byte[])(obj));
            }
        }
        
        internal static byte[] Sound {
            get {
                object obj = ResourceManager.GetObject("Sound", resourceCulture);
                return ((byte[])(obj));
            }
        }
        
        internal static byte[] Ultimate_Performance_pow {
            get {
                object obj = ResourceManager.GetObject("Ultimate_Performance_pow", resourceCulture);
                return ((byte[])(obj));
            }
        }
    }
}
