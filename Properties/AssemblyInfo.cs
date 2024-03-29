﻿/*
*  This file is part of fsh-filetype, a filetype plug-in for Paint.NET
*  that loads and saves FSH images.
*
*  Copyright (C) 2009, 2010, 2011, 2012, 2014, 2015, 2023 Nicholas Hayes
*
*  This program is free software: you can redistribute it and/or modify
*  it under the terms of the GNU General Public License as published by
*  the Free Software Foundation, either version 3 of the License, or
*  (at your option) any later version.
*
*  This program is distributed in the hope that it will be useful,
*  but WITHOUT ANY WARRANTY; without even the implied warranty of
*  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
*  GNU General Public License for more details.
*
*  You should have received a copy of the GNU General Public License
*  along with this program.  If not, see <http://www.gnu.org/licenses/>.
*
*/

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Electronic Arts® Fsh FileType plugin for Paint.NET")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]

#if SC4D
[assembly: AssemblyCompany("null45")]
#else
[assembly: AssemblyCompany("null54")]
#endif

[assembly: AssemblyProduct("FSHfiletype")]

#if SC4D
[assembly: AssemblyCopyright("Copyright © 2009-2015 Nicholas Hayes")]
#else
[assembly: AssemblyCopyright("Copyright © 2009-2015 Nicholas Hayes (aka null54)")]
#endif

[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("22f1fdbd-cc99-41af-8d32-95e83c4807a0")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
[assembly: AssemblyVersion("1.2.4.0")]
[assembly: AssemblyFileVersion("1.2.4.0")]
