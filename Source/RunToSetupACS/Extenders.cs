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

using RunToSetupACS.ACS.Management;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RunToSetupACS
{
    public static class Extenders
    {
        private enum RelyingPartyAddressType
        {
            Error,
            Realm,
            Reply
        }

        public static void DeleteRelyingPartyByRealmIfExists(this ManagementService svc, string realm)
        {
            var rpAddress = svc.RelyingPartyAddresses.Expand("RelyingParty").
                Where(rpa => rpa.Address == realm && rpa.EndpointType == 
                    RelyingPartyAddressType.Realm.ToString()).FirstOrDefault();

            if (rpAddress != null)
                svc.DeleteObject(rpAddress.RelyingParty);
        }

        public static void DeleteServiceIdentityIfExists(this ManagementService svc, string name)
        {
            var serviceIdentities = svc.ServiceIdentities.Where(si => si.Name == name);

            foreach (var si in serviceIdentities)
                svc.DeleteObject(si);
        }

        public static void DeleteRuleGroupByNameIfExists(this ManagementService svc, string name)
        {
            var ruleGroup = svc.RuleGroups.Where(m => m.Name == name).FirstOrDefault();
            
            if (ruleGroup != null)
                svc.DeleteObject(ruleGroup);
        }
    }
}
