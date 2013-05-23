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
using System;

namespace RunToSetupACS
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("This utility will add a new Passthrough Token Signing Key, Rule Group, Service");
            Console.WriteLine("Identity and Relying Party to your ACS account, based upon the settings you've");
            Console.WriteLine("entered in the Settings.xml file, found under the Setup+README folder; right");
            Console.WriteLine("off the solution root.");

            Console.WriteLine();

            Console.Write("Have you updated the \"Settings.xml\" file with appropriate values ([Y]/N)?");

            var cki = Console.ReadKey();

            if (cki.Key != ConsoleKey.Y && cki.Key != ConsoleKey.Enter)
                return;

            Console.WriteLine();
            Console.WriteLine();

            try
            {
                Settings.Init("../../../../Settings/Settings.xml");

                var prompt = "Enter your ACS Management Service Password";

                if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.MgmtSvcPassword))
                    prompt += " (or press [Enter] to use last)";

                Console.WriteLine(prompt);

                var mgmtSvcPassword = Console.ReadLine();

                if (mgmtSvcPassword == "")
                    mgmtSvcPassword = Properties.Settings.Default.MgmtSvcPassword;

                if (string.IsNullOrWhiteSpace(mgmtSvcPassword))
                    throw new Exception("A valid Managment Service Password is required!");

                var svc = AcsHelper.CreateManagementServiceClient(Settings.AcsNamespace, 
                    new Credentials("ManagementClient", mgmtSvcPassword));

                Console.WriteLine();

                Console.Write("Deleting Exiting ACS Entries...");
                svc.DeleteExistingEntries(
                    Settings.Realm,
                    Settings.ServiceIdentity,
                    Settings.RuleGroup);
                Console.WriteLine("SUCCESS!");

                Console.Write("Adding a Relying Party and Token Signing Key...");
                var relyingParty = svc.AddRelyingParty(
                    Settings.Realm,
                    Settings.RelyingParty,
                    Settings.KeyStartDate,
                    Settings.KeyEndDate,
                    Settings.TokenSigningKey,
                    Settings.TokenLifetime);
                Console.WriteLine("SUCCESS!");

                Console.Write("Adding a Passthrough Rule...");
                var ruleGroup = svc.AddPassthroughRule(
                    relyingParty,
                    Settings.RuleGroup);
                Console.WriteLine("SUCCESS!");

                Console.Write("Adding a Service Identity...");
                svc.AddIdentity(
                    Settings.ServiceIdentity,
                    Settings.KeyStartDate,
                    Settings.KeyEndDate);
                Console.WriteLine("SUCCESS!");

                Console.WriteLine();

                Console.WriteLine("Assuming that the entries in the \"Settings.xml\" file were good, as well as the");
                Console.WriteLine("the Namespace and Password that you typed in, then ACS should be configured to");
                Console.WriteLine("work with your application!");

                Properties.Settings.Default.MgmtSvcPassword = mgmtSvcPassword;
                Properties.Settings.Default.Save();
            }
            catch (Exception error)
            {
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine(error.Message);
            }

            Console.WriteLine();
            Console.Write("Press any key to continue...");

            Console.ReadKey();
        }
    }
}
