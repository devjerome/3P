﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Ce code a été généré par un outil.
//     Version du runtime :4.0.30319.42000
//
//     Les modifications apportées à ce fichier peuvent provoquer un comportement incorrect et seront perdues si
//     le code est régénéré.
// </auto-generated>
//------------------------------------------------------------------------------

namespace YamuiFramework.Resources {
    using System;
    
    
    /// <summary>
    ///   Une classe de ressource fortement typée destinée, entre autres, à la consultation des chaînes localisées.
    /// </summary>
    // Cette classe a été générée automatiquement par la classe StronglyTypedResourceBuilder
    // à l'aide d'un outil, tel que ResGen ou Visual Studio.
    // Pour ajouter ou supprimer un membre, modifiez votre fichier .ResX, puis réexécutez ResGen
    // avec l'option /str ou régénérez votre projet VS.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Retourne l'instance ResourceManager mise en cache utilisée par cette classe.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("YamuiFramework.Resources.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Remplace la propriété CurrentUICulture du thread actuel pour toutes
        ///   les recherches de ressources à l'aide de cette classe de ressource fortement typée.
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
        ///   Recherche une chaîne localisée semblable à html {
        ///    padding: 0;
        ///    margin: 0;
        ///    Font: 12px &quot;Segoe UI&quot;;
        ///    color: %FGcolor%;
        ///}
        ///
        ///body {
        ///    padding: 0;
        ///    margin: 0;
        ///}
        ///
        ////* this decorates the htmllabels */
        ///.yamui-text { 
        ///    Font: 12px &quot;Segoe UI&quot;;
        ///    color: %FGcolor%;
        ///    margin: 0;
        ///    padding: 0;
        ///}
        ///
        ////*This is the class of the HtmlToolTip*/
        ///.yamui-tooltip {
        ///    border: solid 1px %FORMBORDER%;
        ///    background-color: %BGcolor%;
        ///    color: %FGcolor%;
        ///    padding: 5px;
        ///    Font: 12px &quot;Segoe UI&quot;;
        ///}.
        /// </summary>
        internal static string BaseStyleSheet {
            get {
                return ResourceManager.GetString("BaseStyleSheet", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Recherche une ressource localisée de type System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap Error {
            get {
                object obj = ResourceManager.GetObject("Error", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Recherche une ressource localisée de type System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap HighImportance {
            get {
                object obj = ResourceManager.GetObject("HighImportance", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Recherche une ressource localisée de type System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap Info {
            get {
                object obj = ResourceManager.GetObject("Info", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Recherche une ressource localisée de type System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap Ok {
            get {
                object obj = ResourceManager.GetObject("Ok", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Recherche une ressource localisée de type System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap Question {
            get {
                object obj = ResourceManager.GetObject("Question", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Recherche une ressource localisée de type System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap QuestionShield {
            get {
                object obj = ResourceManager.GetObject("QuestionShield", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Recherche une ressource localisée de type System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap Services {
            get {
                object obj = ResourceManager.GetObject("Services", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Recherche une chaîne localisée semblable à &lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-8&quot;?&gt;
        ///&lt;Root&gt;
        ///  &lt;Item&gt;
        ///    &lt;ThemeName Value=&quot;The classic&quot; /&gt;
        ///    &lt;PageBackGroundImage Value=&quot;&quot; /&gt;
        ///    &lt;ThemeAccentColor Value=&quot;#1BA1E2&quot; /&gt;
        ///    &lt;FormBack Value=&quot;#E6E6E6&quot; /&gt;
        ///    &lt;FormFore Value=&quot;#1E1E1E&quot; /&gt;
        ///    &lt;FormBorder Value=&quot;#647687&quot; /&gt;
        ///    &lt;ScrollBarNormalBack Value=&quot;#CCCCCC&quot; /&gt;
        ///    &lt;ScrollThumbNormalBack Value=&quot;#666666&quot; /&gt;
        ///    &lt;ScrollBarHoverBack Value=&quot;#CCCCCC&quot; /&gt;
        ///    &lt;ScrollThumbHoverBack Value=&quot;#252526&quot; /&gt;
        ///    &lt;ScrollBarDisabledBack Value=&quot;#E6E6E6&quot; /&gt;
        ///   [le reste de la chaîne a été tronqué]&quot;;.
        /// </summary>
        internal static string themesXml {
            get {
                return ResourceManager.GetString("themesXml", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Recherche une ressource localisée de type System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap Warning {
            get {
                object obj = ResourceManager.GetObject("Warning", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Recherche une ressource localisée de type System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap WarningShield {
            get {
                object obj = ResourceManager.GetObject("WarningShield", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
    }
}
