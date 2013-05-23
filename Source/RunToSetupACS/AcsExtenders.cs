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
using RunToSetupACS.ACS.Management;
using System;
using System.Data.Services.Client;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Text;

namespace RunToSetupACS
{
    public static class AcsExtenders
    {
        public static void DeleteExistingEntries(this ManagementService svc, Uri realm,
            Credentials serviceIdentity, string ruleGroupName)
        {
            Contract.Requires(svc != null);
            Contract.Requires(realm != null);
            Contract.Requires(realm.IsAbsoluteUri);
            Contract.Requires(realm.AbsolutePath == "/");
            Contract.Requires(serviceIdentity != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(ruleGroupName));

            svc.DeleteRelyingPartyByRealmIfExists(realm.AbsoluteUri);
            svc.DeleteServiceIdentityIfExists(serviceIdentity.UserName);
            svc.DeleteRuleGroupByNameIfExists(ruleGroupName);

            svc.SaveChanges(SaveChangesOptions.Batch);
        }

        public static RelyingParty AddRelyingParty(this ManagementService svc, Uri realm,
            string relyingPartyName, DateTime startDate, DateTime endDate, 
            byte[] tokenSigningKey, int tokenLifetime)
        {
            Contract.Requires(svc != null);
            Contract.Requires(realm != null);
            Contract.Requires(realm.IsAbsoluteUri);
            Contract.Requires(realm.AbsolutePath == "/");
            Contract.Requires(!string.IsNullOrWhiteSpace(relyingPartyName));
            Contract.Requires(startDate != default(DateTime));
            Contract.Requires(endDate > startDate);
            Contract.Requires(tokenSigningKey != null);
            Contract.Requires(tokenLifetime >= 1);
            
            var relyingParty = new RelyingParty()
            {
                Name = relyingPartyName,
                AsymmetricTokenEncryptionRequired = false,
                TokenType = "SWT",
                TokenLifetime = tokenLifetime  
            };

            svc.AddToRelyingParties(relyingParty);

            var relyingPartyAddress = new RelyingPartyAddress()
            {
                Address = realm.AbsoluteUri,
                EndpointType = "Realm"
            };

            svc.AddRelatedObject(relyingParty, "RelyingPartyAddresses", relyingPartyAddress);

            var relyingPartyKey = new RelyingPartyKey()
            {
                StartDate = startDate,
                EndDate = endDate,
                Type = "Symmetric",
                Usage = "Signing",
                IsPrimary = true,
                Value = tokenSigningKey
            };

            svc.AddRelatedObject(relyingParty, "RelyingPartyKeys", relyingPartyKey);

            svc.SaveChanges(SaveChangesOptions.Batch);

            return relyingParty;
        }

        public static RuleGroup AddPassthroughRule(this ManagementService svc,
            RelyingParty relyingParty, string ruleGroupName)
        {
            Contract.Requires(svc != null);
            Contract.Requires(relyingParty != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(ruleGroupName));

            var ruleGroup = new RuleGroup() { Name = ruleGroupName };

            svc.AddToRuleGroups(ruleGroup);

            svc.SaveChanges(SaveChangesOptions.Batch);

            var localAuthority = svc.Issuers.Where(
                m => m.Name == "LOCAL AUTHORITY").FirstOrDefault();

            var passthrough = new Rule();

            passthrough.Description = "Passthough all ACS claims";

            svc.AddToRules(passthrough);
            svc.SetLink(passthrough, "RuleGroup", ruleGroup);
            svc.SetLink(passthrough, "Issuer", localAuthority);

            var rprg = new RelyingPartyRuleGroup();

            svc.AddToRelyingPartyRuleGroups(rprg);

            svc.AddLink(relyingParty, "RelyingPartyRuleGroups", rprg);
            svc.AddLink(ruleGroup, "RelyingPartyRuleGroups", rprg);

            svc.SaveChanges(SaveChangesOptions.Batch);

            return ruleGroup;
        }

        public static void AddIdentity(this ManagementService svc, 
            Credentials serviceIdentity, DateTime startDate, DateTime endDate)
        {
            Contract.Requires(svc != null);
            Contract.Requires(serviceIdentity != null);
            Contract.Requires(startDate != default(DateTime));
            Contract.Requires(endDate > startDate);

            var sid = new ServiceIdentity()
            {
                Name = serviceIdentity.UserName
            };

            var key = new ServiceIdentityKey()
            {
                StartDate = startDate,
                EndDate = endDate,
                Type = "Password",
                Usage = "Password",
                Value = Encoding.UTF8.GetBytes(serviceIdentity.Password),
                DisplayName = string.Format(CultureInfo.InvariantCulture,
                    "{0} key for {1}", "Password", serviceIdentity.UserName)
            };

            svc.AddToServiceIdentities(sid);

            svc.AddRelatedObject(sid, "ServiceIdentityKeys", key);

            svc.SaveChanges(SaveChangesOptions.Batch);
        }
    }
}
