using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.WebPages;
using System.Security.Claims;
using System.Security.Principal;
using AutoMapper;
using System.Linq.Expressions;

namespace AssetCare.Common
{
    public static class UserExtension
    {
        public static bool IsInRoles(this IPrincipal user, string rolesRequired)
        {
            if (string.IsNullOrEmpty(rolesRequired)) return true;
            var rolesDemanded = rolesRequired.Split(new char[] { ',' }).Select(r => r?.Trim()?.ToLower());
            var userIdentity = (ClaimsIdentity)user.Identity;
            var claims = userIdentity.Claims;
            //var roleClaimType = userIdentity.RoleClaimType;
            var userRoles = claims.Where(c => c.Type == Constants.Role).Select(c => c.Value).ToList();
            userRoles = SetAdminShadowRole(userRoles);
            return (rolesDemanded.Count() == 0 || userRoles.Intersect<string>(rolesDemanded).Any());
        }

        public static IMappingExpression<TSource, TDestination> Flatten<TSource, TDestination, TSourceMember>(
           this IMappingExpression<TSource, TDestination> map,
           Expression<Func<TSource, TSourceMember>> source)
        {
            var innerSourceProperties =
                from sp in typeof(TSourceMember).GetProperties()
                where sp.CanRead
                join dp in typeof(TDestination).GetProperties()
                on sp.Name equals dp.Name
                where dp.CanWrite
                select sp;
            foreach (var innerSourceProperty in innerSourceProperties)
            {
                var innerProperty = Expression.Convert(Expression.Property(source.Body, innerSourceProperty), typeof(object));
                var mapFrom = Expression.Lambda<Func<TSource, object>>(innerProperty, source.Parameters);
                map.ForMember(innerSourceProperty.Name, c => c.MapFrom(mapFrom));
            }
            return map;
        }

        public static string UserCode(this IPrincipal user)
        {
            var userIdentity = (ClaimsIdentity)user.Identity;
            var claims = userIdentity.Claims;
            var userCode = claims.Where(c => c.Type == Constants.PreferredUserName).FirstOrDefault()?.Value;
            return userCode;
        }

        public static string UserEmail(this IPrincipal user)
        {
            var userIdentity = (ClaimsIdentity)user.Identity;
            var claims = userIdentity.Claims;
            var email = claims.Where(c => c.Type == "email").FirstOrDefault()?.Value;
            return email;
        }
             

        public static bool IsSchoolUser(this IPrincipal user)
        {
            var rolesRequired = "SCHOOL,SCHOOLMGR";
            var rolesDemanded = rolesRequired.Split(new char[] { ',' }).Select(r => r.ToLower());
            var userIdentity = (ClaimsIdentity)user.Identity;
            var claims = userIdentity.Claims;
            var userRoles = claims.Where(c => c.Type == Constants.Role).Select(c => c.Value).ToList();
            return (rolesDemanded.Count() == 0 || userRoles.Intersect<string>(rolesDemanded).Any());
        }

        public static string UserRole(this IPrincipal user)
        {
            var userIdentity = (ClaimsIdentity)user.Identity;
            var claims = userIdentity.Claims;
            var userRoles = claims.Where(c => c.Type == Constants.Role).Select(c => c.Value).ToList();
            return userRoles.Last().ToUpper();
        }

        public static List<string> SetAdminShadowRole(this IPrincipal user, List<string> userRoles)
        {
            return SetAdminShadowRole(userRoles);
        }

        private static List<string> SetAdminShadowRole(List<string> userRoles)
        {
            if (HttpContext.Current.Session["adminShadowRoleCode"] != null)
            {
                userRoles[1] = HttpContext.Current.Session["adminShadowRoleCode"].ToString().ToLower();
            }
            return userRoles;
        }
    }


}