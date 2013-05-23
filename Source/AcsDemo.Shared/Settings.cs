#region Copyright, Author Details and Related Context
//<notice lastUpdateOn="5/23/2013">
//  <assembly>AcsDemo.Shared</assembly>
//  <description>A Simple ACS Demo - Shared Classes</description>
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

using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace AcsDemo.Shared
{
    public static class Settings
    {
        public static void Init(string fileName)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(fileName));
            Contract.Requires(File.Exists(fileName));

            var doc = XDocument.Load(fileName);

            var root = doc.Element("settings");

            AcsNamespace = root.Element("acsNamespace").Value;
            TokenLifetime = (int)root.Element("tokenLifetime");

            var keyLifetime = (int)root.Element("keyLifetime");
            var servicePassword = root.Element("servicePassword").Value;
            var signingPassword = root.Element("signingPassword").Value;
            var namePrefix = root.Element("namePrefix").Value;
            var authority = new Uri(root.Element("authority").Value);

            Contract.Assert(AcsNamespace.IsAcsNamespace());
            Contract.Assert(TokenLifetime >= 1);
            Contract.Assert(keyLifetime >= 1);
            Contract.Assert(!string.IsNullOrWhiteSpace(servicePassword));
            Contract.Assert(!string.IsNullOrWhiteSpace(signingPassword));
            Contract.Assert(!string.IsNullOrWhiteSpace(namePrefix));
            Contract.Assert(authority.IsAbsoluteUri);
            Contract.Assert(authority.AbsolutePath == "/");

            Realm = new Uri(authority.AbsoluteUri + "Users/");

            RelyingParty = namePrefix + " Relying Party";
            RuleGroup = namePrefix + " Rule Group";

            KeyStartDate = DateTime.UtcNow.Date;
            KeyEndDate = KeyStartDate.AddMonths(keyLifetime);

            ServiceIdentity = new Credentials(
                 namePrefix + " Service Identity", servicePassword);

            TokenSigningKey = Encoding.UTF8.GetBytes(signingPassword);
        }

        public static string AcsNamespace { get; private set; }
        public static int TokenLifetime { get; private set; }
        public static Uri Realm { get; private set; }
        public static string RelyingParty { get; private set; }
        public static string RuleGroup { get; private set; }
        public static DateTime KeyStartDate { get; private set; }
        public static DateTime KeyEndDate { get; private set; }
        public static Credentials ServiceIdentity { get; private set; }
        public static byte[] TokenSigningKey { get; private set; }
    }
}
