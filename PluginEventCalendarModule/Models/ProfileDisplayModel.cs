﻿using CarrotCake.CMS.Plugins.EventCalendarModule.Data;
using Carrotware.Web.UI.Components;

namespace CarrotCake.CMS.Plugins.EventCalendarModule.Models {

	public class ProfileDisplayModel {

		public ProfileDisplayModel() {
			this.SelectedValue = -2;
		}

		public ProfileDisplayModel(Guid siteId) : this() {
			this.SiteID = siteId;

			var model = new PagedData<ViewCalendarEventProfile>();
			model.InitOrderByDescending(x => x.EventStartDate);

			this.Profiles = model;
		}

		public void Load() {
			var model = this.Profiles;
			model.ToggleSort();
			var srt = model.ParseSort();

			this.Years = CalendarHelper.GetYears(this.SiteID);

			var query = CalendarHelper.GetProfileView(this.SiteID, this.SelectedValue).SortByParm(srt.SortField, srt.SortDirection);

			model.DataSource = query.ToList();
			model.TotalRecords = query.Count();
		}

		public Guid SiteID { get; set; }

		public int SelectedValue { get; set; }
		public Dictionary<int, string> Years { get; set; }
		public PagedData<ViewCalendarEventProfile> Profiles { get; set; }
	}
}