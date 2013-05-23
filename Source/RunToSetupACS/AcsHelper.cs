#region Copyright, Author Details and Related Context
//<notice lastUpdateOn="5/23/2013">
//  <assembly>RunToSetupACS</assembly>
//  <description>A Simple ACS Demo - ACS Setup Utility</description>
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
using Newtonsoft.Json.Linq;
using Nito.AsyncEx;
using RunToSetupACS.ACS.Management;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace RunToSetupACS
{
    public static class AcsHelper
    {
        public const string ACSHOSTNAME = "accesscontrol.windows.net";
        public const string ACSMANAGEMENTSERVICE = "v2/mgmt/service/";

        private static string cachedSwtToken;

        public static ManagementService CreateManagementServiceClient(
            string acsNamespace, Credentials credentials)
        {
            Contract.Requires(acsNamespace.IsAcsNamespace());
            Contract.Requires(credentials != null);

            var endpoint = string.Format(
                CultureInfo.InvariantCulture, "https://{0}.{1}/{2}",
                acsNamespace, ACSHOSTNAME, ACSMANAGEMENTSERVICE);

            var managementService = new ManagementService(new Uri(endpoint));

            managementService.SendingRequest += (s, e) =>
            {
                if (cachedSwtToken == null)
                {
                    cachedSwtToken = AsyncContext.Run<string>(() =>
                        GetTokenFromACS(acsNamespace, credentials));
                }

                e.Request.Headers.Add(
                    HttpRequestHeader.Authorization, "Bearer " + cachedSwtToken);
            };

            return managementService;
        }

        private async static Task<string> GetTokenFromACS(string acsNamespace,
            Credentials credentials)
        {
            var client = new HttpClient();

            client.BaseAddress = new Uri(string.Format(CultureInfo.CurrentCulture,
                "https://{0}.{1}", acsNamespace, ACSHOSTNAME));

            var content = new FormUrlEncodedContent(new[] 
            {
                new KeyValuePair<string, string>("grant_type",
                    "client_credentials"),
                new KeyValuePair<string, string>("client_id",
                    credentials.UserName),
                new KeyValuePair<string, string>("client_secret", 
                    credentials.Password),
                new KeyValuePair<string, string>("scope", 
                    client.BaseAddress + ACSMANAGEMENTSERVICE)
            });

            var response = await client.PostAsync("/v2/OAuth2-13", content);

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            var data = JObject.Parse(json);

            return data["access_token"].ToString();
        }
    }
}
