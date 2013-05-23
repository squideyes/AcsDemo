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

using System.Text.RegularExpressions;

namespace AcsDemo.Shared
{
    public static class StringValidators
    {
        public static bool IsAcsNamespace(this string value)
        {
            var namespaceRegex = new Regex(
                @"^[A-Za-z0-9][A-Za-z0-9\-]{4,48}[A-Za-z0-9]$");

            return namespaceRegex.IsMatch(value);
        }
    }
}
