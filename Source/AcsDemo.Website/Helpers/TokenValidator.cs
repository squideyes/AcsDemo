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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace AcsDemo.Website
{
    internal class TokenValidator
    {
        private string signingKey;
        private string issuer;
        private string audience;

        public TokenValidator(string serviceNamespace, string audience,
            string signingKey)
        {
            this.signingKey = signingKey;

            this.issuer = string.Format(
                "https://{0}.accesscontrol.windows.net/",
                serviceNamespace.ToLowerInvariant());

            this.audience = audience;
        }

        public bool Validate(string token)
        {
            if (!IsHMACValid(token, Convert.FromBase64String(signingKey)))
                return false;

            if (IsExpired(token))
                return false;

            if (!IsIssuerTrusted(token))
                return false;

            if (!IsAudienceTrusted(token))
                return false;

            return true;
        }

        public Dictionary<string, string> GetKeyValues(string token)
        {
            const string BADKEYVALUE =
                "\"{0}\" is does not contain a key/value pair!";

            const string REPEATEDKEYVALUE =
                "The \"{0}\" key/value pair was unexpectedly repeated!";

            if (string.IsNullOrEmpty(token))
                throw new ArgumentException();

            return token.Split('&').Aggregate(new Dictionary<string, string>(),
                (dict, rawKeyValue) =>
                {
                    if (rawKeyValue == string.Empty)
                        return dict;

                    var keyValue = rawKeyValue.Split('=');

                    if (keyValue.Length != 2)
                        throw new ArgumentException(BADKEYVALUE);

                    if (dict.ContainsKey(keyValue[0]) == true)
                    {
                        throw new ArgumentException(
                            REPEATEDKEYVALUE, keyValue[0]);
                    }

                    dict.Add(HttpUtility.UrlDecode(keyValue[0]),
                        HttpUtility.UrlDecode(keyValue[1]));

                    return dict;
                });
        }

        private static ulong GetTimeStamp()
        {
            return Convert.ToUInt64((DateTime.UtcNow - 
                new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds);
        }

        private bool IsAudienceTrusted(string token)
        {
            var keyValues = GetKeyValues(token);

            string audience;

            keyValues.TryGetValue("Audience", out audience);

            if (!string.IsNullOrEmpty(audience))
            {
                if (audience.Equals(this.audience, StringComparison.Ordinal))
                    return true;
            }

            return false;
        }

        private bool IsIssuerTrusted(string token)
        {
            var keyValues = GetKeyValues(token);

            string issuerName;

            keyValues.TryGetValue("Issuer", out issuerName);

            if (!string.IsNullOrEmpty(issuerName))
            {
                if (issuerName.Equals(this.issuer, StringComparison.Ordinal))
                    return true;
            }

            return false;
        }

        private bool IsHMACValid(string swt, byte[] sha256HMACKey)
        {
            var swtWithSignature = swt.Split(new string[] { "&HMACSHA256=" }, 
                StringSplitOptions.None);

            if ((swtWithSignature == null) || (swtWithSignature.Length != 2))
                return false;

            var hmac = new HMACSHA256(sha256HMACKey);

            var locallyGeneratedSignatureInBytes = hmac.ComputeHash(
                Encoding.ASCII.GetBytes(swtWithSignature[0]));

            var locallyGeneratedSignature = HttpUtility.UrlEncode(
                Convert.ToBase64String(locallyGeneratedSignatureInBytes));

            return locallyGeneratedSignature == swtWithSignature[1];
        }

        private bool IsExpired(string swt)
        {
            try
            {
                var expiresOnValue = GetKeyValues(swt)["ExpiresOn"];
                var expiresOn = Convert.ToUInt64(expiresOnValue);
                var currentTime = Convert.ToUInt64(GetTimeStamp());

                if (currentTime > expiresOn)
                    return true;

                return false;
            }
            catch (KeyNotFoundException)
            {
                throw new ArgumentException();
            }
        }
    }
}
