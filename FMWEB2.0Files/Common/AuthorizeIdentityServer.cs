using AssetCare.Common;
using AssetCare.Model.Models;
using Microsoft.AspNet.Identity;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.Owin.Security.OpenIdConnect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;

namespace AssetCare
{
    public class AuthorizeOverride : System.Web.Mvc.AuthorizeAttribute
    {
        public string ReferredPages { get; set; } = null;
        protected override void HandleUnauthorizedRequest(System.Web.Mvc.AuthorizationContext filterContext)
        {
            if (HttpContext.Current.User == null)
            {
                filterContext.Result = new System.Web.Mvc.RedirectResult("~/");
                return;
            }

            if (filterContext == null)
            {
                throw new ArgumentNullException("filterContext");
            }

            //Intercept results where person is authenticated but still doesn't have permissions
            if (filterContext.RequestContext.HttpContext.User.Identity.IsAuthenticated)
            {
                filterContext.Result = new System.Web.Mvc.RedirectResult("~/Unauthorized");
                return;
            }

            base.HandleUnauthorizedRequest(filterContext);
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            Roles = Roles.ToLower();
            // check for proper authorization
            var isAuthorized = base.AuthorizeCore(httpContext);
            if (!isAuthorized)
            {
                return false;
            }

            return true;
        }

        public override void OnAuthorization(System.Web.Mvc.AuthorizationContext filterContext)
        {
            if (HttpContext.Current.User == null)
            {
                filterContext.Result = new System.Web.Mvc.RedirectResult("~/");
                return;
            }

            if (filterContext == null)
            {
                throw new ArgumentNullException("filterContext");
            }

            if (string.IsNullOrEmpty(Roles) && string.IsNullOrEmpty(ReferredPages))
            {
                var actionDescriptor = filterContext.ActionDescriptor;

                if (actionDescriptor == null)
                    throw new ArgumentNullException("filterContext.ActionDescriptor");

                //var actionName = actionDescriptor.ActionName;

                var controllerDescriptor = actionDescriptor.ControllerDescriptor;

                if (controllerDescriptor == null)
                    throw new ArgumentNullException("filterContext.ActionDescriptor.controllerDescriptor");

                var controllerName = controllerDescriptor.ControllerName;

                var rolesStr = ApiRulesLoader.GetMvcControllerRules(controllerName);

                if (!string.IsNullOrEmpty(rolesStr))
                    Roles = rolesStr.ToLower();
            }

            if (string.IsNullOrEmpty(Roles) && !string.IsNullOrEmpty(ReferredPages))
            {
                var rolesStr = PageRulesLoader.GetRolesByPages(ReferredPages.Split(new char[] { ',' }));
                if (!string.IsNullOrEmpty(rolesStr))
                    Roles = rolesStr.ToLower();
            }

            // is reset on every request, must be repopped before auth
            SetLocalRoles SetLocal = new SetLocalRoles();
            SetLocal.SetSingleRole();

            base.OnAuthorization(filterContext);

            if (filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                using (var context = new AssetCareContext())
                {
                    // determine user
                    System.Security.Claims.ClaimsIdentity identityClaims = (System.Security.Claims.ClaimsIdentity)HttpContext.Current.User.Identity;
                    string claimUserName = identityClaims.FindFirstValue("preferred_username");
                    var userDetails = context.Users.FirstOrDefault(x => x.UserCode == claimUserName);

                    // set default agency in session if nothing set
                    if (HttpContext.Current.Session["AgencyDetails"] == null)
                    {
                        if (userDetails.UserAgencies != null && userDetails.UserAgencies.FirstOrDefault(y => y.DefaultAgencyID == true) != null)
                        {
                            var usersDefaultAgencyId = userDetails.UserAgencies.FirstOrDefault(y => y.DefaultAgencyID == true).AgencyID;
                            var usersDefaultAgencyDetails = context.Agencies.FirstOrDefault(x => x.AgencyId == usersDefaultAgencyId);
                            KeyValuePair<int, string> agencyDetails = new KeyValuePair<int, string>(usersDefaultAgencyDetails.AgencyId, usersDefaultAgencyDetails.AgencyName);
                            HttpContext.Current.Session["AgencyDetails"] = agencyDetails;
                        }
                    }


                    // update currentuser viewbag
                    filterContext.Controller.ViewData["CurrentUser"] = new Model.ViewModels.UserViewModel()
                    {
                        UserID = userDetails.UserID,
                        FirstName = identityClaims.FindFirstValue("given_name"),
                        LastName = identityClaims.FindFirstValue("family_name"),
                        Email = identityClaims.FindFirstValue("email"),
                        RoleDescNew = userDetails.Role.RoleDescNew,
                        UserCode = claimUserName
                    };
                }
            }
        }
    }

    public class AuthorizeAPIOverride : System.Web.Http.AuthorizeAttribute
    {
        public string ReferredPages { get; set; } = null;
        public override void OnAuthorization(System.Web.Http.Controllers.HttpActionContext filterContext)
        {
            if (HttpContext.Current.User == null)
            {
                var response = filterContext.Request.CreateResponse(HttpStatusCode.Redirect);
                response.Headers.Location = new Uri("~/");
                filterContext.Response = response;
                return;
            }

            if (filterContext == null)
            {
                throw new ArgumentNullException("filterContext");
            }

            if (string.IsNullOrEmpty(Roles) && string.IsNullOrEmpty(ReferredPages))
            {
                var actionDescriptor = filterContext.ActionDescriptor;

                if (actionDescriptor == null)
                    throw new ArgumentNullException("filterContext.ActionDescriptor");

                //var actionName = actionDescriptor.ActionName;

                var controllerDescriptor = actionDescriptor.ControllerDescriptor;

                if (controllerDescriptor == null)
                    throw new ArgumentNullException("filterContext.ActionDescriptor.controllerDescriptor");

                var controllerName = controllerDescriptor.ControllerName;

                var rolesStr = ApiRulesLoader.GetApiRules(controllerName);

                if (!string.IsNullOrEmpty(rolesStr))
                    Roles = rolesStr.ToLower();
            }

            if (string.IsNullOrEmpty(Roles) && !string.IsNullOrEmpty(ReferredPages))
            {
                var rolesStr = PageRulesLoader.GetRolesByPages(ReferredPages.Split(new char[] { ',' }));
                if (!string.IsNullOrEmpty(rolesStr))
                    Roles = rolesStr.ToLower();
            }

            if (!string.IsNullOrEmpty(Roles))
                Roles = Roles.ToLower();

            // is reset on every request, must be repopped before auth
            SetLocalRoles SetLocal = new SetLocalRoles();
            SetLocal.SetSingleRole();

            base.OnAuthorization(filterContext);

            //filterContext.RequestContext.Principal.Identity.IsAuthenticated;
        }
    }

    public class SetLocalRoles
    {
        private const string ROLE_CLAIM_TYPE = "role";
        private const string SCHOOL_PRINCIPAL_ROLE_CLAIM_VALUE = "school.principal";
        private const string SCHOOL_USER_ROLE_CLAIM_VALUE = "school.user";
        private const string DET_ROLE_CLAIM_TYPE = "detRoles";
        private const string DET_LOCATION_CLAIM_TYPE = "detLocations";
        private const string SAML2_PROVIDER = "saml2p";
        public void SetSingleRole()
        {
            System.Security.Claims.ClaimsIdentity identityClaims = (System.Security.Claims.ClaimsIdentity)HttpContext.Current.User.Identity;

            //if (identityClaims.FindFirstValue("id_token") != null && identityClaims.FindFirstValue("local_roles") == null)
            if (identityClaims.FindFirstValue("id_token") != null)
            {
                // everything needs to be a consistent case for the replacement role checking
                if ((identityClaims).FindFirstValue("role") != null)
                {
                    IList<System.Security.Claims.Claim> originalRoles = new List<System.Security.Claims.Claim>();
                    // record and remove first so that the readded entries don't get done again
                    var roleItems = identityClaims.Claims.Where(x => x.Type == "role").ToList();
                    foreach (var item in roleItems)
                    {
                        originalRoles.Add(item);
                        identityClaims.RemoveClaim(item);
                    }
                    // add the role claims back but with case corrected
                    foreach (var item in originalRoles)
                    {
                        identityClaims.AddClaim(new System.Security.Claims.Claim(item.Type, item.Value.ToLower()));
                    }
                }

                using (var context = new AssetCareContext())
                {
                    var userCode = (identityClaims).FindFirstValue("preferred_username");
                    //int timeout = 20;
                    //int.TryParse(System.Configuration.ConfigurationManager.AppSettings["AssetCareSettings:ID4SessionTimeout"], out timeout);
                    //FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(
                    //                                1,                                     // ticket version
                    //                                userCode,                              // authenticated username
                    //                                DateTime.Now,                          // issueDate
                    //                                DateTime.Now.AddMinutes(timeout),           // expiryDate
                    //                                false,                          // true to persist across browser sessions
                    //                                string.Empty,                              // can be used to store additional user data
                    //                                FormsAuthentication.FormsCookiePath);
                    //string encryptedTicket = System.Web.Security.FormsAuthentication.Encrypt(ticket);

                    //// Add the cookie to the request to save it
                    //HttpCookie cookie = new HttpCookie("_key", encryptedTicket);
                    //cookie.HttpOnly = true;
                    //cookie.Secure = true;
                    //cookie.Path += ";SameSite=Lax";
                    //cookie.Domain = ".nsw.gov.au";
                    //HttpContext.Current.Response.Cookies.Add(cookie);

                    var user = context.Users.FirstOrDefault(x => x.UserCode == userCode && x.Active == true);
                    if (user == null)
                    {
                        throw new ArgumentNullException($"Cannot find active user by usercode {userCode}");
                    }
                    //if (user == null)
                    //{
                    //    var idp = identityClaims.FindFirstValue("idp");
                    //    var schoolUserRole = identityClaims.Claims.FirstOrDefault(c => c.Type == ROLE_CLAIM_TYPE && (c.Value == SCHOOL_PRINCIPAL_ROLE_CLAIM_VALUE || c.Value == SCHOOL_USER_ROLE_CLAIM_VALUE));
                    //    if ((idp != null) && SAML2_PROVIDER.Equals(idp) && (schoolUserRole != null))
                    //    {
                    //        var locations = identityClaims.Claims.Where(c => c.Type == DET_LOCATION_CLAIM_TYPE);
                    //        if (!locations.Any())
                    //            throw new ArgumentNullException($"Cannot find active school for school user {userCode}");

                    //        //create school user now
                    //        var sysUser = context.Users.FirstOrDefault(x => x.UserCode == "TestAdmin1" && x.Active == true);
                    //        var model = new UserViewModel();
                    //        model.UserCode = userCode;
                    //        model.RoleID = (SCHOOL_PRINCIPAL_ROLE_CLAIM_VALUE.Equals(schoolUserRole.Value)) ? "SM" : "F";
                    //        var emailParts = userCode.Split(new char[] { '@' });
                    //        var nameParts = emailParts[0].Split(new char[] { '.' });
                    //        model.FirstName = (nameParts.Length > 0) ? nameParts[0] : userCode;
                    //        model.LastName = (nameParts.Length > 1) ? nameParts[1] : "";
                    //        model.CreatedBy = sysUser.UserID;
                    //        model.CreatedDate = DateTime.Now;
                    //        model.Active = true;
                    //        model.Deleted = false;
                    //        var newUser = Mapper.Map<User>(model);
                    //        context.Users.Add(newUser);
                    //        context.SaveChanges();
                    //        var lastLocation = locations.Last();
                    //        foreach (var location in locations)
                    //        {
                    //            var schoolCode = Convert.ToInt32(location.Value);
                    //            var school = context.Properties.FirstOrDefault(p => p.SchoolId == schoolCode);
                    //            if (school == null) continue;
                    //            //add user agency
                    //            var newUserAgency = new UserAgency();
                    //            newUserAgency.UserID = newUser.UserID;
                    //            newUserAgency.AgencyID = school.AgencyId;
                    //            newUserAgency.DefaultAgencyID = (location == lastLocation);
                    //            context.UserAgencies.Add(newUserAgency);
                    //            //add user property
                    //            var newUserProperty = new UserProperty();
                    //            newUserProperty.UserID = newUser.UserID;
                    //            newUserProperty.PropertyID = school.PropertyId;
                    //            newUserProperty.AgencyID = school.AgencyId;
                    //            context.UserProperties.Add(newUserProperty);
                    //        }
                    //        context.SaveChanges();
                    //        user = context.Users.Include("Role").FirstOrDefault(x => x.UserCode == userCode && x.Active == true);
                    //    }
                    //    else
                    //        throw new ArgumentNullException($"Cannot find active user by usercode {userCode}");
                    //}

                    var internalRole = user.Role.RoleID;
                    identityClaims.AddClaim(new System.Security.Claims.Claim("local_roles", Guid.NewGuid().ToString()));
                    identityClaims.AddClaim(new System.Security.Claims.Claim("role", context.Roles.FirstOrDefault(x => x.RoleID == internalRole).RoleCode.ToLower()));

                    if (user.UserAgencies != null)
                    {
                        var userAgencyIdList = string.Join(",", user.UserAgencies.Select(a => a.AgencyID.ToString()));
                        identityClaims.AddClaim(new System.Security.Claims.Claim("agency_list", userAgencyIdList));
                    }

                    // roles *DON'T* use rolerules
                    //internalRoles = context.RolesRules.Where(x => x.RoleID == internalRole).Select(x => x.RoleIDAllowed).ToList();

                    //var customClaims = new List<System.Security.Claims.Claim>();

                    //foreach (var item in internalRoles)
                    //{
                    //    customClaims.Add(new System.Security.Claims.Claim("role", context.Roles.FirstOrDefault(x => x.RoleID == item).RoleCode.ToLower()));
                    //}

                    //identityClaims.AddClaims(customClaims);
                }
            }
        }
    }

    //// work around for known issue with katana cookie manager and asp.net cookie manager conflicting
    //// http://katanaproject.codeplex.com/wikipage?title=System.Web%20response%20cookie%20integration%20issues&referringTitle=Documentation
    //public class SystemWebCookieManager : ICookieManager
    //{
    //    public string GetRequestCookie(IOwinContext context, string key)
    //    {
    //        if (context == null)
    //        {
    //            throw new ArgumentNullException("context");
    //        }

    //        var webContext = context.Get<HttpContextBase>(typeof(HttpContextBase).FullName);
    //        var cookie = webContext.Request.Cookies[key];
    //        return cookie == null ? null : cookie.Value;
    //    }

    //    public void AppendResponseCookie(IOwinContext context, string key, string value, CookieOptions options)
    //    {
    //        if (context == null)
    //        {
    //            throw new ArgumentNullException("context");
    //        }
    //        if (options == null)
    //        {
    //            throw new ArgumentNullException("options");
    //        }

    //        var webContext = context.Get<HttpContextBase>(typeof(HttpContextBase).FullName);

    //        bool domainHasValue = !string.IsNullOrEmpty(options.Domain);
    //        bool pathHasValue = !string.IsNullOrEmpty(options.Path);
    //        bool expiresHasValue = options.Expires.HasValue;

    //        var cookie = new HttpCookie(key, value);
    //        if (domainHasValue)
    //        {
    //            cookie.Domain = options.Domain;
    //        }
    //        if (pathHasValue)
    //        {
    //            cookie.Path = options.Path;
    //        }
    //        if (expiresHasValue)
    //        {
    //            cookie.Expires = options.Expires.Value;
    //        }
    //        if (options.Secure)
    //        {
    //            cookie.Secure = true;
    //        }
    //        if (options.HttpOnly)
    //        {
    //            cookie.HttpOnly = true;
    //        }

    //        webContext.Response.AppendCookie(cookie);
    //    }

    //    public void DeleteCookie(IOwinContext context, string key, CookieOptions options)
    //    {
    //        if (context == null)
    //        {
    //            throw new ArgumentNullException("context");
    //        }
    //        if (options == null)
    //        {
    //            throw new ArgumentNullException("options");
    //        }

    //        AppendResponseCookie(
    //            context,
    //            key,
    //            string.Empty,
    //            new CookieOptions
    //            {
    //                Path = options.Path,
    //                Domain = options.Domain,
    //                Expires = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc),
    //            });
    //    }
    //}

    /// <summary>
    /// Patched to fix the issue with too many nonce cookies described here: https://github.com/IdentityServer/IdentityServer3/issues/1124
    /// Deletes all nonce cookies that weren't the current one
    /// </summary>
    public static class OpenIdConnectAuthenticationPatchedMiddlewareExtension
    {
        public static Owin.IAppBuilder UseOpenIdConnectAuthenticationPatched(this Owin.IAppBuilder app, Microsoft.Owin.Security.OpenIdConnect.OpenIdConnectAuthenticationOptions openIdConnectOptions)
        {
            if (app == null)
            {
                throw new System.ArgumentNullException("app");
            }
            if (openIdConnectOptions == null)
            {
                throw new System.ArgumentNullException("openIdConnectOptions");
            }
            System.Type type = typeof(OpenIdConnectAuthenticationPatchedMiddleware);
            object[] objArray = new object[] { app, openIdConnectOptions };
            return app.Use(type, objArray);
        }
    }

    public class OpenIdConnectAuthenticationPatchedMiddleware : OpenIdConnectAuthenticationMiddleware
    {
        private readonly Microsoft.Owin.Logging.ILogger _logger;

        public OpenIdConnectAuthenticationPatchedMiddleware(Microsoft.Owin.OwinMiddleware next, Owin.IAppBuilder app, Microsoft.Owin.Security.OpenIdConnect.OpenIdConnectAuthenticationOptions options)
                : base(next, app, options)
        {
            this._logger = Microsoft.Owin.Logging.AppBuilderLoggerExtensions.CreateLogger<OpenIdConnectAuthenticationPatchedMiddleware>(app);
        }

        protected override Microsoft.Owin.Security.Infrastructure.AuthenticationHandler<OpenIdConnectAuthenticationOptions> CreateHandler()
        {
            return new SawtoothOpenIdConnectAuthenticationHandler(_logger);
        }

        public class SawtoothOpenIdConnectAuthenticationHandler : OpenIdConnectAuthenticationHandler
        {
            public SawtoothOpenIdConnectAuthenticationHandler(Microsoft.Owin.Logging.ILogger logger)
                : base(logger) { }

            protected override void RememberNonce(OpenIdConnectMessage message, string nonce)
            {
                // First, set the new nonce using the base implementation
                base.RememberNonce(message, nonce);

                // Then clean up old nonces (except the one we just set)
                var currentNonceKey = OpenIdConnectAuthenticationDefaults.CookiePrefix + "nonce." + nonce;
                var oldNonces = Request.Cookies.Where(kvp =>
                    kvp.Key.StartsWith(OpenIdConnectAuthenticationDefaults.CookiePrefix + "nonce") &&
                    kvp.Key != currentNonceKey);

                if (oldNonces.Any())
                {
                    //Microsoft.Owin.CookieOptions cookieOptions = new Microsoft.Owin.CookieOptions
                    //{
                    //    HttpOnly = true,
                    //    Secure = Request.IsSecure,
                    //    Path = Request.PathBase.HasValue ? Request.PathBase.Value : "/"
                    //};
                    //foreach (KeyValuePair<string, string> oldNonce in oldNonces)
                    //{
                    //    Response.Cookies.Delete(oldNonce.Key, cookieOptions);
                    //}
                    foreach (KeyValuePair<string, string> oldNonce in oldNonces)
                    {
                            var expired = new HttpCookie(oldNonce.Key)
                            {
                                Expires = DateTime.UtcNow.AddDays(-1),
                                Value = "",
                                Path = "/"
                            };
                            HttpContext.Current.Response.Cookies.Add(expired);
                    }
                }
            }
        }
    }
}