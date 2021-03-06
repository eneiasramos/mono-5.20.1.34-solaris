#pragma warning disable 1591
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.235
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Microsoft.Web.Helpers
{
    using System;
    using System.Collections.Generic;
    
    #line 3 "..\..\Facebook.cshtml"
    using System.Collections.Specialized;
    
    #line default
    #line hidden
    
    #line 4 "..\..\Facebook.cshtml"
    using System.Globalization;
    
    #line default
    #line hidden
    using System.IO;
    using System.Linq;
    using System.Net;
    
    #line 5 "..\..\Facebook.cshtml"
    using System.Security;
    
    #line default
    #line hidden
    
    #line 6 "..\..\Facebook.cshtml"
    using System.Text;
    
    #line default
    #line hidden
    using System.Web;
    
    #line 7 "..\..\Facebook.cshtml"
    using System.Web.Helpers;
    
    #line default
    #line hidden
    using System.Web.Security;
    using System.Web.UI;
    using System.Web.WebPages;
    using System.Web.WebPages.Html;
    
    #line 8 "..\..\Facebook.cshtml"
    using System.Web.WebPages.Scope;
    
    #line default
    #line hidden
    
    #line 9 "..\..\Facebook.cshtml"
    using WebMatrix.Data;
    
    #line default
    #line hidden
    
    #line 10 "..\..\Facebook.cshtml"
    using WebMatrix.WebData;
    
    #line default
    #line hidden
    
    public class Facebook : System.Web.WebPages.HelperPage
    {
        
        #line 12 "..\..\Facebook.cshtml"

    
    private const string FacebookCredentialsTableName = "webpages_FacebookCredentials";
    private const string FacebookCredentialsIdColumn = "FacebookId";
    private const string FacebookCredentialsUserIdColumn = "UserId";

    private const string DefaultUserIdColumn = "UserId";
    private const string DefaultUserNameColumn = "email";
    private const string DefaultUserTableName = "UserProfile";

    private const string DefaultFacebookPerms = "email";
    private const string DefaultCallbackUrl = "~/Facebook/Login";
    private const string FacebookApiProfileUrl = "https://graph.facebook.com/me";
    private const string FacebookCookieAccessToken = "access_token";

    private static readonly object _isInitializedKey = new object();
    private static readonly object _membershipDBNameKey = new object();
    private static readonly object _appIdKey = new object();
    private static readonly object _appSecretKey = new object();
    private static readonly object _language = new object();

    public static bool HasMembershipIntegration {
        get {
            return !MembershipDBName.IsEmpty();
        }
    }

    public static bool IsFacebookUserAuthenticated {
        get {
            return !GetFacebookCookieInfo(HttpContext, "uid").IsEmpty();
        }
    }

    public static bool IsFacebookUserAssociated {
        get {
            return !GetAssociatedMembershipUserName().IsEmpty();
        }
    }

    public static bool IsInitialized {
        get {
            return (bool)(ScopeStorage.CurrentScope[_isInitializedKey] ?? false);
        }

        private set {
            ScopeStorage.CurrentScope[_isInitializedKey] = value;
        }
    }

    public static string MembershipDBName {
        get {
            return (string)(ScopeStorage.CurrentScope[_membershipDBNameKey] ?? "");
        }

        set {
            ScopeStorage.CurrentScope[_membershipDBNameKey] = value;
        }
    }

    public static string AppId {
        get {
            return (string)(ScopeStorage.CurrentScope[_appIdKey] ?? "");
        }

        set {
            ScopeStorage.CurrentScope[_appIdKey] = value;
        }
    }

    public static string AppSecret {
        get {
            return (string)(ScopeStorage.CurrentScope[_appSecretKey] ?? "");
        }

        set {
            ScopeStorage.CurrentScope[_appSecretKey] = value;
        }
    }

    public static string Language {
        get {
            return (string)(ScopeStorage.CurrentScope[_language] ?? "en_US");
        }

        set {
            ScopeStorage.CurrentScope[_language] = value;
        }
    }
    
    private static HttpContextBase HttpContext {
        get {
            return new HttpContextWrapper(System.Web.HttpContext.Current);
        }
    }

    ///<summary>
    /// Initialize the helper with your Facebook application settings. 
    /// 
    /// If the 'membershipDBName' parameter is specified, Facebook membership integration will be enabled, 
    /// allowing users to register and associate their Facebook user account (identified with the e-mail) 
    /// with your site membership and the WebSecurity helper. 
    /// In this case, the helper will initialize the WebSecurity WebMatrix helper automatically (if not done previously) 
    /// and the store the Facebook membership information in the 'membershipDbName' database.    
    ///</summary>
    ///<param name="appId">Facebook application id.</param>        
    ///<param name="appSecret">Facebook application secret.</param>   
    ///<param name="membershipDbName">Name of the database used for storing the membership data.</param>   
    public static void Initialize(string appId, string appSecret, string membershipDbName = "") {
        AppId = appId;
        AppSecret = appSecret;
        IsInitialized = true;

        if (!membershipDbName.IsEmpty()) {
            MembershipDBName = membershipDbName;

            InitializeMembershipProviderIfNeeded();
            InitializeFacebookTableIfNeeded();
        }
    }

    ///<summary>
    /// Retrieves the Facebook profile of current logged in user.
    ///</summary>
    public static UserProfile GetFacebookUserProfile() {
        var accessToken = GetFacebookCookieInfo(HttpContext, FacebookCookieAccessToken);

        if (accessToken.IsEmpty()) {
            return null;
        }

        var userProfileUrl = new Uri(new UrlBuilder(FacebookApiProfileUrl).AddParam(FacebookCookieAccessToken, accessToken));

        using (var client = new WebClient()) {
            using (var receiveStream = client.OpenRead(userProfileUrl)) {
                var result = new StreamReader(receiveStream).ReadToEnd();
                var profile = Json.Decode<UserProfile>(result);

                return profile;
            }
        }
    }

    ///<summary>
    /// Associates the specified User Name (e.g. email, depending on your membership model) with the current Facebook User Id from the logged user.
    ///</summary>
    ///<param name="userName">The user name to associate the current logged-in facebook account to.</param>
    public static void AssociateMembershipAccount(string userName) {
        if (!IsFacebookUserAuthenticated) {
            throw new InvalidOperationException("No Facebook user is authenticated.");
        }

        if (IsFacebookUserAssociated) {
            throw new InvalidOperationException("The authenticated Facebook user is already associated to a membership account.");
        }

        using (var db = Database.Open(MembershipDBName)) {
            var facebookUserId = GetFacebookCookieInfo(HttpContext, "uid").As<long>();

            var userId = WebSecurity.GetUserId(userName);
            db.Execute(String.Format(CultureInfo.InvariantCulture, "INSERT INTO {0} ({1}, {2}) VALUES (@0, @1)", FacebookCredentialsTableName, FacebookCredentialsUserIdColumn, FacebookCredentialsIdColumn), 
                userId, facebookUserId);

            // User is registered in the application
            FormsAuthentication.SetAuthCookie(userName, false);
        }
    }

    public static bool MembershipLogin() {
        var user = GetAssociatedMembershipUserName();

        if (!user.IsEmpty()) {
            // User is registered in the application
            FormsAuthentication.SetAuthCookie(user, false);
            return true;
        }
        else {
            return false;
        }
    }

    private static void InitializeMembershipProviderIfNeeded() {
        var provider = GetMembershipProvider();

        if (IsMembershipProviderInitialized(provider)) {
            WebSecurity.InitializeDatabaseConnection(MembershipDBName, DefaultUserTableName, DefaultUserIdColumn, DefaultUserNameColumn, true);
        }
    }

    private static void InitializeFacebookTableIfNeeded() {
        using (var db = Database.Open(MembershipDBName)) {
            var table = db.QuerySingle("SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @0", FacebookCredentialsTableName);

            if (table == null) {
                db.Execute(String.Format(CultureInfo.InvariantCulture, "CREATE TABLE {0} ({1} INT NOT NULL, {2} BIGINT NOT NULL)", FacebookCredentialsTableName, FacebookCredentialsUserIdColumn, FacebookCredentialsIdColumn));
            }
        }
    }

    private static string GetAssociatedMembershipUserName() {
        var userName = "";

        if (IsFacebookUserAuthenticated) {
            using (var db = Database.Open(MembershipDBName)) {
                var userId = db.QueryValue(String.Format(CultureInfo.InvariantCulture, "SELECT {0} FROM {1} WHERE {2} = LOWER(@0)", FacebookCredentialsUserIdColumn, FacebookCredentialsTableName, FacebookCredentialsIdColumn),
                    GetFacebookCookieInfo(HttpContext, "uid"));
                if (userId != null) {
                    userName = GetUserName(userId);
                }
            }
        }

        return userName;
    }

    private static string GetUserName(int userId) {
        var userName = "";

        using (var db = Database.Open(MembershipDBName)) {
            var provider = GetMembershipProvider();
            userName = db.QueryValue(String.Format(CultureInfo.InvariantCulture, "SELECT {0} FROM {1} WHERE {2} = @0", provider.UserNameColumn, provider.UserTableName, provider.UserIdColumn), userId);
        }

        return userName;
    }

    private static SimpleMembershipProvider GetMembershipProvider() {
        var provider = Membership.Provider as SimpleMembershipProvider;

        if (provider == null) {
            throw new InvalidOperationException("Simple Membership Provider not found.");
        }

        return provider;
    }

    private static bool IsMembershipProviderInitialized(SimpleMembershipProvider provider) {
        return provider.UserTableName.IsEmpty() || provider.UserIdColumn.IsEmpty() || provider.UserNameColumn.IsEmpty();
    }

    internal static string GetFacebookCookieInfo(HttpContextBase httpContext, string key) {
        var request = httpContext.Request;
        var name = "fbs_" + AppId;

        if (request.Cookies[name] != null) {
            var value = request.Cookies[name].Value;
            var args = HttpUtility.ParseQueryString(value.Replace("\"", ""));

            if (!IsFacebookCookieValid(args)) {
                throw new InvalidOperationException("Invalid Facebook cookie.");
            }

            return args[key];
        }
        else {
            return "";
        }
    }

    private static bool IsFacebookCookieValid(NameValueCollection args) {
        var payload = new StringBuilder();
        var keys = args.AllKeys;
        Array.Sort(keys);
        foreach (var key in keys) {
            if (!key.Equals("sig", StringComparison.OrdinalIgnoreCase)) {
                payload.AppendFormat("{0}={1}", key, args[key]);
            }
        }
        
        payload.Append(AppSecret);

        // Review: The HMAC uses MD5 which is not cryptographically secure. We need to investigate other Facebook authentication methods.
        var signature = new StringBuilder();
        using (var md5 = System.Security.Cryptography.MD5CryptoServiceProvider.Create()) {
            var hash = md5.ComputeHash(Encoding.ASCII.GetBytes(payload.ToString()));
            for (int i = 0; i < hash.Length; i++) {
                signature.Append(hash[i].ToString("X2", CultureInfo.InvariantCulture));
            }
        }
        return String.Equals(args["sig"], signature.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    public class UserProfile {
        public string Id { get; set; }
        public string Name { get; set; }
        public string First_Name { get; set; }
        public string Last_Name { get; set; }
        public string Link { get; set; }
        public string Bio { get; set; }
        public string Gender { get; set; }
        public string Email { get; set; }
        public string Timezone { get; set; }
        public string Locale { get; set; }
        public string Updated_Time { get; set; }
    }

    private static IHtmlString RawJS(string text) {
        return new HtmlString(HttpUtility.JavaScriptStringEncode(text));
    }

        #line default
        #line hidden

public static System.Web.WebPages.HelperResult GetInitializationScripts() {
return new System.Web.WebPages.HelperResult(__razor_helper_writer => {



#line 316 "..\..\Facebook.cshtml"
                                    

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "    <div id=\"fb-root\"></div>\r\n");



WriteLiteralTo(@__razor_helper_writer, "    <script type=\"text/javascript\">\r\n        window.fbAsyncInit = function () {\r\n" +
"            FB.init({ appId: \'");



#line 320 "..\..\Facebook.cshtml"
WriteTo(@__razor_helper_writer, RawJS(AppId));

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "\', status: true, cookie: true, xfbml: true });\r\n        };\r\n        (function () " +
"{\r\n            var e = document.createElement(\'script\'); e.async = true;\r\n      " +
"      e.src = document.location.protocol +\r\n            \'//connect.facebook.net/" +
"");



#line 325 "..\..\Facebook.cshtml"
     WriteTo(@__razor_helper_writer, RawJS(Language));

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "/all.js\';\r\n            document.getElementById(\'fb-root\').appendChild(e);\r\n      " +
"  } ());\r\n\r\n        function loginRedirect(url) { window.location = url; }\r\n    " +
"</script>\r\n");



#line 331 "..\..\Facebook.cshtml"

#line default
#line hidden

});

}


public static System.Web.WebPages.HelperResult LoginButton(
        string registerUrl,
        string returnUrl = "~/",
        string callbackUrl = DefaultCallbackUrl,
        string buttonText = "",
        bool autoLogoutLink = false,
        string size = "medium",
        string length = "long",
        bool showFaces = false,
        string extendedPermissions = "") {
return new System.Web.WebPages.HelperResult(__razor_helper_writer => {



#line 366 "..\..\Facebook.cshtml"
                                          

    var redirectUrl = new UrlBuilder(callbackUrl)
                            .AddParam("registerUrl", new UrlBuilder(registerUrl))
                            .AddParam("returnUrl", new UrlBuilder(returnUrl));
    var onLogin = String.Format(CultureInfo.InvariantCulture, "loginRedirect('{0}')", RawJS(redirectUrl));
    extendedPermissions = extendedPermissions.IsEmpty() ? "email" : String.Concat("email,", extendedPermissions);


#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "    <fb:login-button autologoutlink=\"");



#line 374 "..\..\Facebook.cshtml"
      WriteTo(@__razor_helper_writer, autoLogoutLink);

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "\" size=\"");



#line 374 "..\..\Facebook.cshtml"
                             WriteTo(@__razor_helper_writer, size);

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "\" length=\"");



#line 374 "..\..\Facebook.cshtml"
                                            WriteTo(@__razor_helper_writer, length);

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "\" onlogin=\"");



#line 374 "..\..\Facebook.cshtml"
                                                              WriteTo(@__razor_helper_writer, onLogin);

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "\" show-faces=\"");



#line 374 "..\..\Facebook.cshtml"
                                                                                    WriteTo(@__razor_helper_writer, showFaces);

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "\" perms=\"");



#line 374 "..\..\Facebook.cshtml"
                                                                                                       WriteTo(@__razor_helper_writer, extendedPermissions);

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "\">");



#line 374 "..\..\Facebook.cshtml"
                                                                                                                             WriteTo(@__razor_helper_writer, buttonText);

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "</fb:login-button>        \r\n");



#line 375 "..\..\Facebook.cshtml"

#line default
#line hidden

});

}


public static System.Web.WebPages.HelperResult LoginButtonTagOnly(
    string buttonText = "",
    bool autoLogoutLink = false,
    string size = "long",
    string length = "short",
    string onLogin = "",
    bool showFaces = false,
    string extendedPermissions = "") {
return new System.Web.WebPages.HelperResult(__razor_helper_writer => {



#line 403 "..\..\Facebook.cshtml"
                                      

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "    <fb:login-button autologoutlink=\"");



#line 404 "..\..\Facebook.cshtml"
      WriteTo(@__razor_helper_writer, autoLogoutLink);

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "\" size=\"");



#line 404 "..\..\Facebook.cshtml"
                             WriteTo(@__razor_helper_writer, size);

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "\" length=\"");



#line 404 "..\..\Facebook.cshtml"
                                            WriteTo(@__razor_helper_writer, length);

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "\" onlogin=\"");



#line 404 "..\..\Facebook.cshtml"
                                                              WriteTo(@__razor_helper_writer, onLogin);

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "\" show-faces=\"");



#line 404 "..\..\Facebook.cshtml"
                                                                                    WriteTo(@__razor_helper_writer, showFaces);

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "\" perms=\"");



#line 404 "..\..\Facebook.cshtml"
                                                                                                       WriteTo(@__razor_helper_writer, extendedPermissions);

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "\">");



#line 404 "..\..\Facebook.cshtml"
                                                                                                                             WriteTo(@__razor_helper_writer, buttonText);

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "</fb:login-button>\r\n");



#line 405 "..\..\Facebook.cshtml"

#line default
#line hidden

});

}


public static System.Web.WebPages.HelperResult LikeButton(
            string href = "",
            string buttonLayout = "standard",
            bool showFaces = true,
            int width = 450,
            int height = 80,
            string action = "like",
            string font = "",
            string colorScheme = "light",
            string refLabel = ""
            ) {
return new System.Web.WebPages.HelperResult(__razor_helper_writer => {



#line 445 "..\..\Facebook.cshtml"
                   
                
    if (href.IsEmpty()) {
        href = Request.Url.OriginalString;
    }

    var src = new UrlBuilder("http://www.facebook.com/plugins/like.php")
                    .AddParam("href", href)
                    .AddParam("layout", buttonLayout)
                    .AddParam("show_faces", showFaces)
                    .AddParam("width", width)
                    .AddParam("action", action)
                    .AddParam("colorscheme", colorScheme)
                    .AddParam("height", height)
                    .AddParam("font", font)
                    .AddParam("locale", Language)
                    .AddParam("ref", refLabel);
        

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "    <iframe src=\"");



#line 463 "..\..\Facebook.cshtml"
WriteTo(@__razor_helper_writer, src);

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "\" scrolling=\"no\" frameborder=\"0\" style=\"border:none; overflow:hidden; width:");



#line 463 "..\..\Facebook.cshtml"
                                                                   WriteTo(@__razor_helper_writer, width);

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "px; height:");



#line 463 "..\..\Facebook.cshtml"
                                                                                      WriteTo(@__razor_helper_writer, height);

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "px;\" allowTransparency=\"true\"></iframe>\r\n");



#line 464 "..\..\Facebook.cshtml"

#line default
#line hidden

});

}


public static System.Web.WebPages.HelperResult Comments(
            string xid = "",
            int width = 550,
            int numPosts = 10,
            bool reverseOrder = false,
            bool removeRoundedBox = false) {
return new System.Web.WebPages.HelperResult(__razor_helper_writer => {



#line 489 "..\..\Facebook.cshtml"
                                            

#line default
#line hidden



#line 490 "..\..\Facebook.cshtml"
WriteLiteralTo(@__razor_helper_writer, "    <fb:comments ");

#line default
#line hidden


#line 490 "..\..\Facebook.cshtml"
                  if (!xid.IsEmpty()) {
#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, " ");

WriteLiteralTo(@__razor_helper_writer, "xid=\"");



#line 490 "..\..\Facebook.cshtml"
                    WriteTo(@__razor_helper_writer, xid);

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "\" ");

WriteLiteralTo(@__razor_helper_writer, " ");



#line 490 "..\..\Facebook.cshtml"
                                                                 }
#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "numposts=\"");



#line 490 "..\..\Facebook.cshtml"
                                             WriteTo(@__razor_helper_writer, numPosts);

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "\" width=\"");



#line 490 "..\..\Facebook.cshtml"
                                                               WriteTo(@__razor_helper_writer, width);

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "\" reverse=\"");



#line 490 "..\..\Facebook.cshtml"
                                                                                WriteTo(@__razor_helper_writer, reverseOrder);

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "\" simple=\"");



#line 490 "..\..\Facebook.cshtml"
                                                                                                       WriteTo(@__razor_helper_writer, removeRoundedBox);

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "\" ></fb:comments>\r\n");



#line 491 "..\..\Facebook.cshtml"

#line default
#line hidden

});

}


public static System.Web.WebPages.HelperResult Recommendations(
            string site = "",
            int width = 300,
            int height = 300,
            bool showHeader = true,
            string colorScheme = "light",
            string font = "",
            string borderColor = "",
            string filter = "",
            string refLabel = ""
    ) {
return new System.Web.WebPages.HelperResult(__razor_helper_writer => {



#line 531 "..\..\Facebook.cshtml"
       	
        if (site.IsEmpty()) {
            site = Request.Url.Host;
        }

    var src = new UrlBuilder("http://www.facebook.com/plugins/recommendations.php")
                    .AddParam("site", site)
                    .AddParam("width", width)
                    .AddParam("height", height)
                    .AddParam("header", showHeader)
                    .AddParam("colorscheme", colorScheme)
                    .AddParam("font", font)
                    .AddParam("border_color", borderColor)
                    .AddParam("filter", filter)
                    .AddParam("ref", refLabel)
                    .AddParam("locale", Language);
    

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "    <iframe src=\"");



#line 548 "..\..\Facebook.cshtml"
WriteTo(@__razor_helper_writer, src);

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "\" scrolling=\"no\" frameborder=\"0\" style=\"border:none; overflow:hidden; width:");



#line 548 "..\..\Facebook.cshtml"
                                                                   WriteTo(@__razor_helper_writer, width);

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "px; height:");



#line 548 "..\..\Facebook.cshtml"
                                                                                      WriteTo(@__razor_helper_writer, height);

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "px;\" allowTransparency=\"true\"></iframe>\r\n");



#line 549 "..\..\Facebook.cshtml"

#line default
#line hidden

});

}


public static System.Web.WebPages.HelperResult LikeBox(
            string href,
            int width = 292,
            int height = 587,
            string colorScheme = "light",
            int connections = 10,
            bool showStream = true,
            bool showHeader = true) {
return new System.Web.WebPages.HelperResult(__razor_helper_writer => {



#line 581 "..\..\Facebook.cshtml"
                                        
    
    var src = new UrlBuilder("http://www.facebook.com/plugins/recommendations.php")
                    .AddParam("href", href)
                    .AddParam("width", width)
                    .AddParam("height", height)
                    .AddParam("header", showHeader)
                    .AddParam("colorscheme", colorScheme)
                    .AddParam("connections", connections)
                    .AddParam("stream", showStream)
                    .AddParam("header", showHeader)
                    .AddParam("locale", Language);
                                 

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "    <iframe src=\"");



#line 594 "..\..\Facebook.cshtml"
WriteTo(@__razor_helper_writer, src);

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "\" scrolling=\"no\" frameborder=\"0\" style=\"border:none; overflow:hidden; width:");



#line 594 "..\..\Facebook.cshtml"
                                                                   WriteTo(@__razor_helper_writer, width);

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "px; height:");



#line 594 "..\..\Facebook.cshtml"
                                                                                      WriteTo(@__razor_helper_writer, height);

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "px;\" allowTransparency=\"true\"></iframe>                \r\n");



#line 595 "..\..\Facebook.cshtml"

#line default
#line hidden

});

}


public static System.Web.WebPages.HelperResult Facepile(
            int maxRows = 1,
            int width = 200) {
return new System.Web.WebPages.HelperResult(__razor_helper_writer => {



#line 610 "..\..\Facebook.cshtml"
                              

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "    <fb:facepile max-rows=\"");



#line 611 "..\..\Facebook.cshtml"
WriteTo(@__razor_helper_writer, maxRows);

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "\" width=\"");



#line 611 "..\..\Facebook.cshtml"
             WriteTo(@__razor_helper_writer, width);

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "\"></fb:facepile>\r\n");



#line 612 "..\..\Facebook.cshtml"

#line default
#line hidden

});

}


public static System.Web.WebPages.HelperResult LiveStream(
            int width = 400,
            int height = 500,
            string xid = "",
            string viaUrl = "",
            bool alwaysPostToFriends = false) {
return new System.Web.WebPages.HelperResult(__razor_helper_writer => {



#line 637 "..\..\Facebook.cshtml"
                                               
       
                
    var builder = new UrlBuilder("http://www.facebook.com/plugins/live_stream_box.php")
        .AddParam("app_id", AppId)
        .AddParam("width", width)
        .AddParam("height", height)
        .AddParam("always_post_to_friends", alwaysPostToFriends)
        .AddParam("locale", Language);

    if (!xid.IsEmpty()) {
        builder.AddParam("xid", xid);
        builder.AddParam("via_url", viaUrl);
    }
    

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "    <iframe src=\"");



#line 652 "..\..\Facebook.cshtml"
WriteTo(@__razor_helper_writer, builder);

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "\" scrolling=\"no\" frameborder=\"0\" style=\"border:none; overflow:hidden; width:");



#line 652 "..\..\Facebook.cshtml"
                                                                       WriteTo(@__razor_helper_writer, width);

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "px; height:");



#line 652 "..\..\Facebook.cshtml"
                                                                                          WriteTo(@__razor_helper_writer, height);

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "px;\" allowTransparency=\"true\"></iframe>\r\n");



#line 653 "..\..\Facebook.cshtml"

#line default
#line hidden

});

}


public static System.Web.WebPages.HelperResult ActivityFeed(
        string site = "",
        int width = 300,
        int height = 300,
        bool showHeader = true,
        string colorScheme = "light",
        string font = "",
        string borderColor = "",
        bool showRecommendations = false) {
return new System.Web.WebPages.HelperResult(__razor_helper_writer => {



#line 689 "..\..\Facebook.cshtml"
                                           
    if (site.IsEmpty()) {
        site = Request.Url.Host;
    }

    var src = new UrlBuilder("http://www.facebook.com/plugins/activity.php")
                    .AddParam("site", site)
                    .AddParam("width", width)
                    .AddParam("height", height)
                    .AddParam("header", showHeader)
                    .AddParam("colorscheme", colorScheme)
                    .AddParam("font", font)
                    .AddParam("border_color", borderColor)
                    .AddParam("recommendations", showRecommendations)
                    .AddParam("locale", Language);                
                    

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "    <iframe src=\"");



#line 705 "..\..\Facebook.cshtml"
WriteTo(@__razor_helper_writer, src);

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "\" scrolling=\"no\" frameborder=\"0\" style=\"border:none; overflow:hidden; width:300px" +
"; height:300px;\" allowTransparency=\"true\"></iframe>\r\n");



#line 706 "..\..\Facebook.cshtml"

#line default
#line hidden

});

}


public static System.Web.WebPages.HelperResult OpenGraphRequiredProperties(
    string siteName,
    string title,
    string type,
    string url,
    string imageUrl,
    string description = "") {
return new System.Web.WebPages.HelperResult(__razor_helper_writer => {



#line 736 "..\..\Facebook.cshtml"
                                

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "    <meta property=\"og:site_name\" content=\"");



#line 737 "..\..\Facebook.cshtml"
            WriteTo(@__razor_helper_writer, siteName);

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "\"/>\r\n");



WriteLiteralTo(@__razor_helper_writer, "    <meta property=\"fb:app_id\" content=\"");



#line 738 "..\..\Facebook.cshtml"
         WriteTo(@__razor_helper_writer, AppId);

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "\"/>     \r\n");



WriteLiteralTo(@__razor_helper_writer, "    <meta property=\"og:title\" content=\"");



#line 739 "..\..\Facebook.cshtml"
        WriteTo(@__razor_helper_writer, title);

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "\"/>\r\n");



WriteLiteralTo(@__razor_helper_writer, "    <meta property=\"og:type\" content=\"");



#line 740 "..\..\Facebook.cshtml"
       WriteTo(@__razor_helper_writer, type);

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "\"/>\r\n");



WriteLiteralTo(@__razor_helper_writer, "    <meta property=\"og:url\" content=\"");



#line 741 "..\..\Facebook.cshtml"
      WriteTo(@__razor_helper_writer, url);

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "\"/>\r\n");



WriteLiteralTo(@__razor_helper_writer, "    <meta property=\"og:image\" content=\"");



#line 742 "..\..\Facebook.cshtml"
        WriteTo(@__razor_helper_writer, imageUrl);

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "\"/>        \r\n");



#line 743 "..\..\Facebook.cshtml"
        if (!description.IsEmpty()) {

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "    <meta property=\"og:description\" content=\"");



#line 744 "..\..\Facebook.cshtml"
              WriteTo(@__razor_helper_writer, description);

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "\"/>\r\n");



#line 745 "..\..\Facebook.cshtml"
        }

#line default
#line hidden

});

}


public static System.Web.WebPages.HelperResult OpenGraphLocationProperties(
    string latitude = "",
    string longitude = "",
    string streetAddress = "",
    string locality = "",
    string region = "",
    string postalCode = "",
    string countryName = "") {
return new System.Web.WebPages.HelperResult(__razor_helper_writer => {



#line 764 "..\..\Facebook.cshtml"
                                      
        if (!latitude.IsEmpty()) {

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "    <meta property=\"og:latitude\" content=\"");



#line 766 "..\..\Facebook.cshtml"
           WriteTo(@__razor_helper_writer, latitude);

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "\"/>\r\n");



#line 767 "..\..\Facebook.cshtml"
        }
        if (!longitude.IsEmpty()) {

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "    <meta property=\"og:longitude\" content=\"");



#line 769 "..\..\Facebook.cshtml"
            WriteTo(@__razor_helper_writer, longitude);

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "\"/>\r\n");



#line 770 "..\..\Facebook.cshtml"
        }
        if (!streetAddress.IsEmpty()) {

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "    <meta property=\"og:street-address\" content=\"");



#line 772 "..\..\Facebook.cshtml"
                 WriteTo(@__razor_helper_writer, streetAddress);

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "\"/>\r\n");



#line 773 "..\..\Facebook.cshtml"
        }
        if (!locality.IsEmpty()) {

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "    <meta property=\"og:locality\" content=\"");



#line 775 "..\..\Facebook.cshtml"
           WriteTo(@__razor_helper_writer, locality);

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "\"/>\r\n");



#line 776 "..\..\Facebook.cshtml"
        }
        if (!region.IsEmpty()) {

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "    <meta property=\"og:region\" content=\"");



#line 778 "..\..\Facebook.cshtml"
         WriteTo(@__razor_helper_writer, region);

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "\"/>\r\n");



#line 779 "..\..\Facebook.cshtml"
        }
        if (!postalCode.IsEmpty()) {

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "    <meta property=\"og:postal-code\" content=\"");



#line 781 "..\..\Facebook.cshtml"
              WriteTo(@__razor_helper_writer, postalCode);

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "\"/>\r\n");



#line 782 "..\..\Facebook.cshtml"
        }
        if (!countryName.IsEmpty()) {

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "    <meta property=\"og:country-name\" content=\"");



#line 784 "..\..\Facebook.cshtml"
               WriteTo(@__razor_helper_writer, countryName);

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "\"/>\r\n");



#line 785 "..\..\Facebook.cshtml"
        }

#line default
#line hidden

});

}


public static System.Web.WebPages.HelperResult OpenGraphContactProperties(
    string email = "",
    string phoneNumber = "",
    string faxNumber = "") {
return new System.Web.WebPages.HelperResult(__razor_helper_writer => {



#line 800 "..\..\Facebook.cshtml"
                                    
    
        if (!email.IsEmpty()) {

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "    <meta property=\"og:email\" content=\"");



#line 803 "..\..\Facebook.cshtml"
        WriteTo(@__razor_helper_writer, email);

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "\"/>\r\n");



#line 804 "..\..\Facebook.cshtml"
        }
        if (!phoneNumber.IsEmpty()) {

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "    <meta property=\"og:phone_number\" content=\"");



#line 806 "..\..\Facebook.cshtml"
               WriteTo(@__razor_helper_writer, phoneNumber);

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "\"/>\r\n");



#line 807 "..\..\Facebook.cshtml"
        }
        if (!faxNumber.IsEmpty()) {

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "    <meta property=\"og:fax_number\" content=\"");



#line 809 "..\..\Facebook.cshtml"
             WriteTo(@__razor_helper_writer, faxNumber);

#line default
#line hidden

WriteLiteralTo(@__razor_helper_writer, "\"/>\r\n");



#line 810 "..\..\Facebook.cshtml"
        }
    

#line default
#line hidden

});

}


public static System.Web.WebPages.HelperResult FbmlNamespaces() {
return new System.Web.WebPages.HelperResult(__razor_helper_writer => {



WriteLiteralTo(@__razor_helper_writer, "xmlns:fb=\"http://www.facebook.com/2008/fbml\" xmlns:og=\"http://opengraphprotocol.o" +
"rg/schema/\"");



#line 819 "..\..\Facebook.cshtml"
                                                                                                                                   
#line default
#line hidden

});

                                                                                                                                   }


        public Facebook()
        {
        }
    }
}
#pragma warning restore 1591
