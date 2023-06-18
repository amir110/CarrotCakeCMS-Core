﻿using System.Collections.Generic;

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

	public interface IWidgetLimitedProperties {
		List<string> LimitedPropertyList { get; }
	}
}