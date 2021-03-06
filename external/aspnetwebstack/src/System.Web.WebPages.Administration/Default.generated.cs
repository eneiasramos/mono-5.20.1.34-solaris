#pragma warning disable 1591
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.225
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace System.Web.WebPages.Administration
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Web;
    using System.Web.Helpers;
    using System.Web.Security;
    using System.Web.UI;
    using System.Web.WebPages;
    using System.Web.WebPages.Html;
    using System.Web.WebPages.Administration.PackageManager;
    
    [System.Web.WebPages.PageVirtualPathAttribute("~/Default.cshtml")]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("RazorSingleFileGenerator", "1.0.0.0")]
    public class Default_cshtml : System.Web.WebPages.WebPage
    {
#line hidden

                    // Resolve package relative syntax
                    // Also, if it comes from a static embedded resource, change the path accordingly
                    public override string Href(string virtualPath, params object[] pathParts) {
                        virtualPath = ApplicationPart.ProcessVirtualPath(GetType().Assembly, VirtualPath, virtualPath);
                        return base.Href(virtualPath, pathParts);
                    }
        public Default_cshtml()
        {
        }
        protected System.Web.HttpApplication ApplicationInstance
        {
            get
            {
                return ((System.Web.HttpApplication)(Context.ApplicationInstance));
            }
        }
        public override void Execute()
        {


WriteLiteral("\r\n\r\n");



      
    Page.Title = AdminResources.Modules;
    var adminModules = from p in SiteAdmin.Modules
                       where !p.StartPageVirtualPath.Equals(SiteAdmin.AdminVirtualPath, StringComparison.OrdinalIgnoreCase)
                       orderby p.DisplayName
                       select p;


WriteLiteral("\r\n");


 if (!adminModules.Any() && !PackageManagerModule.Available) {

WriteLiteral("    <h3>");


   Write(AdminResources.NoAdminModulesInstalled);

WriteLiteral("</h3>\r\n");


}
else if (PackageManagerModule.Available && !adminModules.Any()) {
    // If no other module is available, take the user directly to the package manager
    Response.Redirect(Href("packages"));
    return;
}
else {

WriteLiteral("    <ul class=\"modules\">\r\n");


     if (PackageManagerModule.Available) {

WriteLiteral("        <li>    \r\n            <a href=\"");


                Write(Href("packages"));

WriteLiteral("\" title=\"");


                                          Write(PackageManagerModule.ModuleName);

WriteLiteral("\"><strong>");


                                                                                    Write(PackageManagerModule.ModuleName);

WriteLiteral("</strong></a>\r\n            <div class=\"description\">");


                                Write(PackageManagerModule.ModuleDescription);

WriteLiteral("</div>\r\n        </li>\r\n");


    }


     if (adminModules.Any()) {
        foreach (var module in adminModules) {

WriteLiteral("            <li>\r\n                <a href=\"");


                    Write(Href(module.StartPageVirtualPath));

WriteLiteral("\"><strong>");


                                                                Write(module.DisplayName);

WriteLiteral("</strong></a>\r\n                <div class=\"description\">\r\n                    ");


               Write(module.Description);

WriteLiteral("\r\n                </div>\r\n            </li>\r\n");


        }
    }

WriteLiteral("    </ul>\r\n");


}

        }
    }
}
#pragma warning restore 1591
