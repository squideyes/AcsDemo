#region Copyright, Author Details and Related Context
//<notice lastUpdateOn="5/23/2013">
//  <assembly>AcsDemo.Client</assembly>
//  <description>A Simple ACS Demo - Client</description>
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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;

namespace AcsDemo.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Settings.Init("../../../../Settings/Settings.xml");

                var token = AsyncContext.Run<string>(() => 
                    GetTokenFromACS(Settings.Realm.AbsoluteUri));

                Console.WriteLine("Received token from ACS: {0}", token);

                var client = new HttpClient();

                var requestMessage =
                    new HttpRequestMessage(HttpMethod.Get, Settings.Realm);

                requestMessage.Headers.Add("Authorization",
                    string.Format("OAuth2 access_token=\"{0}\"", token));

                var response = AsyncContext.Run<HttpResponseMessage>(() =>
                    client.SendAsync(requestMessage));

                var json = AsyncContext.Run<string>(() =>
                    response.Content.ReadAsStringAsync());

                var data = JsonConvert.DeserializeObject<List<User>>(json);

                Console.WriteLine();
                Console.WriteLine("Got {0} Users", data.Count);
            }
            catch (Exception error)
            {
                Console.WriteLine();
                Console.WriteLine(error.Message);
            }

            Console.WriteLine();
            Console.Write("Press any key to continue...");

            Console.ReadKey();
        }

        private async static Task<string> GetTokenFromACS(string scope)
        {
            var client = new HttpClient();

            client.BaseAddress = new Uri(string.Format(CultureInfo.CurrentCulture,
                "https://{0}.accesscontrol.windows.net", Settings.AcsNamespace));

            var content = new FormUrlEncodedContent(new[] 
            {
                new KeyValuePair<string, string>("grant_type",
                    "client_credentials"),
                new KeyValuePair<string, string>("client_id",
                    Settings.ServiceIdentity.UserName),
                new KeyValuePair<string, string>("client_secret", 
                    Settings.ServiceIdentity.Password),
                new KeyValuePair<string, string>("scope", 
                    scope)
            });

            var response = await client.PostAsync("/v2/OAuth2-13", content);

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            var data = JObject.Parse(json);

            return data["access_token"].ToString();
        }
    }
}
