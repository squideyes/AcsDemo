#region Copyright, Author Details and Related Context
//<notice lastUpdateOn="5/23/2013">
//  <assembly>AcsDemo.Website</assembly>
//  <description>A Simple ACS Demo - WCF Service Host Website</description>
//  <copyright>
//    Copyright (C) 2013 Louis S. Berman

//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.

//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.

//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see http://www.gnu.org/licenses/.
//  </copyright>
//  <author>
//    <fullName>Louis S. Berman</fullName>
//    <email>louis@squideyes.com</email>
//    <website>http://squideyes.com</website>
//  </author>
//</notice>
#endregion 

using AcsDemo.Shared;
using System;
using System.IO;
using System.Net;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Web;
using System.Web.Hosting;
using System.Web.Routing;

namespace AcsDemo.Website
{
    public class Global : HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            var fileName = Path.Combine(HostingEnvironment.MapPath("~/"), 
                "../../Settings/Settings.xml");

            Settings.Init(fileName);

            var factory = new WebServiceHostFactory();

            RouteTable.Routes.Add(
                new ServiceRoute("Users", factory, typeof(UserService)));
        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {
            var authorization = HttpContext.Current.Request.Headers["Authorization"];

            if (string.IsNullOrEmpty(authorization))
                throw new WebFaultException(HttpStatusCode.Unauthorized);

            if (!authorization.StartsWith("OAuth2 "))
                throw new WebFaultException(HttpStatusCode.Unauthorized);

            var nameValuePair = authorization.Substring(
                "OAuth2 ".Length).Split(new char[] { '=' }, 2);

            if (nameValuePair.Length != 2 ||
                nameValuePair[0] != "access_token" ||
                !nameValuePair[1].StartsWith("\"") ||
                !nameValuePair[1].EndsWith("\""))
            {
                throw new WebFaultException(HttpStatusCode.Unauthorized);
            }

            var token = nameValuePair[1].Substring(1, nameValuePair[1].Length - 2);

            var tokenSigningKey = Convert.ToBase64String(Settings.TokenSigningKey);

            var validator = new TokenValidator(
                Settings.AcsNamespace, Settings.Realm.AbsoluteUri, tokenSigningKey); 

            if (!validator.Validate(token))
                throw new WebFaultException(HttpStatusCode.Unauthorized);
        }
    }
}