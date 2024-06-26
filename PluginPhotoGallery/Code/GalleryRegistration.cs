﻿using CarrotCake.CMS.Plugins.PhotoGallery.Data;
using Carrotware.CMS.Interface;
using Microsoft.EntityFrameworkCore;

/*
* CarrotCake CMS (MVC Core)
* http://www.carrotware.com/
*
* Copyright 2015, 2023, Samantha Copeland
* Dual licensed under the MIT or GPL Version 3 licenses.
*
* Date: June 2023
*/

namespace CarrotCake.CMS.Plugins.PhotoGallery.Code {

	public class GalleryRegistration : BaseWidgetLoader {

		public override void LoadWidgets(IServiceCollection services) {
			base.LoadWidgets(services);

			services.AddTransient(typeof(Controllers.HomeController));
			services.AddTransient(typeof(Controllers.AdminController));

			var config = services.BuildServiceProvider().GetRequiredService<IConfigurationRoot>();
			services.AddDbContext<GalleryContext>(opt => opt.UseSqlServer(config.GetConnectionString("CarrotwareCMS")));
		}

		public override void RegisterWidgets(WebApplication app) {
			base.RegisterWidgets(app);

			app.MigrateDatabase();
		}
	}
}