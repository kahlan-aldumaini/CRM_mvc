﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace CRM_mvc.Resources.Email {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class TableHeaderEmail {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal TableHeaderEmail() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("CRM_mvc.Resources.Email.TableHeaderEmail", typeof(TableHeaderEmail).Assembly);
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
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to رقم العميل.
        /// </summary>
        public static string CustomerPhoneNumber {
            get {
                return ResourceManager.GetString("CustomerPhoneNumber", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to العمليات.
        /// </summary>
        public static string Processes {
            get {
                return ResourceManager.GetString("Processes", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to تاريخ الإرسال.
        /// </summary>
        public static string SendDate {
            get {
                return ResourceManager.GetString("SendDate", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to اسم المرسل.
        /// </summary>
        public static string SenderName {
            get {
                return ResourceManager.GetString("SenderName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to العنوان.
        /// </summary>
        public static string Title {
            get {
                return ResourceManager.GetString("Title", resourceCulture);
            }
        }
    }
}