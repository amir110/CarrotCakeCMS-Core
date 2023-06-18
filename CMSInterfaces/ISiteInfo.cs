﻿using System;

/*
* CarrotCake CMS (MVC Core)
* http://www.carrotware.com/
*
* Copyright 2015, 2023, Samantha Copeland
* Dual licensed under the MIT or GPL Version 3 licenses.
*
* Date: June 2023
*/

namespace Carrotware.CMS.Interface {

	public interface ICarrotSite {
		Guid SiteID { get; set; }
		string? SiteName { get; set; }
		string? SiteTagline { get; set; }
		string? MainUrl { get; set; }
		string? TimeZone { get; set; }
		Guid? BlogRootContentId { get; set; }
	}
}