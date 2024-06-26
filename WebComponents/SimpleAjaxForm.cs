﻿using Microsoft.AspNetCore.Mvc.Rendering;

/*
* CarrotCake CMS (MVC Core)
* http://www.carrotware.com/
*
* Copyright 2015, 2023, Samantha Copeland
* Dual licensed under the MIT or GPL Version 3 licenses.
*
* Date: June 2023
*/

namespace Carrotware.Web.UI.Components {

	public enum AjaxFormResultMode {
		Replace,
		Before,
		After,
	}

	public class SimpleAjaxFormOptions {

		public SimpleAjaxFormOptions() {
			this.FormId = "frmAjax";
			this.UpdateTargetId = "divAjax";
		}

		public string FormId { get; set; } = "frmAjax";
		public string UpdateTargetId { get; set; } = "divAjax";
		public string OnBegin { get; set; } = string.Empty;
		public string OnComplete { get; set; } = string.Empty;
		public string OnSuccess { get; set; } = string.Empty;
		public string OnFailure { get; set; } = "__OnAjaxRequestFailure";
		public AjaxFormResultMode Mode { get; set; } = AjaxFormResultMode.Replace;
		public FormMethod Method { get; set; } = FormMethod.Post;
	}

	//===================
	public class SimpleAjaxForm : IDisposable {
		private string _tagName = "form";
		private HtmlTag _tag = new HtmlTag();
		private IHtmlHelper _helper;

		public SimpleAjaxForm(IHtmlHelper helper, SimpleAjaxFormOptions options, object? routeValues = null, object? attributes = null) {
			SetAjaxForm(helper, options, routeValues, attributes);

			RenderOpen();
		}

		public SimpleAjaxForm(IHtmlHelper helper, SimpleAjaxFormOptions options, object? attributes = null) {
			SetAjaxForm(helper, options, null, attributes);

			RenderOpen();
		}

		public SimpleAjaxForm(IHtmlHelper helper, SimpleAjaxFormOptions options) {
			SetAjaxForm(helper, options, null, null);

			RenderOpen();
		}

		private void RenderOpen() {
			_helper.ViewContext.Writer.Write(_tag.OpenTag() + Environment.NewLine);
			_helper.PreserveViewPath();
		}

		public void SetAjaxForm(IHtmlHelper helper, SimpleAjaxFormOptions options, object? routeValues, object? attributes) {
			_helper = helper;
			_tag = new HtmlTag(_tagName);
			_tag.SetAttribute("id", options.FormId);
			_tag.MergeAttributes(attributes);

			var acts = new List<string>();

			var area = string.Empty;
			var ctrl = string.Empty;
			var action = string.Empty;

			if (_helper.ViewContext != null) {
				var routes = _helper.ViewContext.RouteData.Values;

				if (routes != null) {
					if (routes.ContainsKey("area")) {
						area = routes["area"].ToString();
					}
					if (routes.ContainsKey("action")) {
						action = routes["action"].ToString();
					}
					if (routes.ContainsKey("controller")) {
						ctrl = routes["controller"].ToString();
					}
				}
			}

			// perform overrides if present
			var routesAttr = routeValues.ToAttributeDictionary();
			if (routesAttr != null) {
				if (routesAttr.ContainsKey("area")) {
					area = routesAttr["area"].ToString();
				}
				if (routesAttr.ContainsKey("action")) {
					action = routesAttr["action"].ToString();
				}
				if (routesAttr.ContainsKey("controller")) {
					ctrl = routesAttr["controller"].ToString();
				}
			}

			acts.Add(area);
			acts.Add(ctrl);
			acts.Add(action);
			var act = "/" + string.Join("/", acts.Where(x => x.Length > 1));

			var formInfo = new Dictionary<string, object>();
			formInfo.Add("action", act);
			formInfo.Add("method", options.Method.ToString().ToLowerInvariant());

			formInfo.Add("data-ajax", "true");
			formInfo.Add("data-ajax-method", options.Method.ToString().ToUpperInvariant());
			formInfo.Add("data-ajax-update", $"#{options.UpdateTargetId.Replace("#", "")}");

			formInfo.Add("data-ajax-mode", options.Mode.ToString().ToLowerInvariant());

			if (!string.IsNullOrWhiteSpace(options.OnBegin)) {
				formInfo.Add("data-ajax-begin", options.OnBegin);
			}
			if (!string.IsNullOrWhiteSpace(options.OnComplete)) {
				formInfo.Add("data-ajax-complete", options.OnComplete);
			}
			if (!string.IsNullOrWhiteSpace(options.OnSuccess)) {
				formInfo.Add("data-ajax-success", options.OnSuccess);
			}
			if (!string.IsNullOrWhiteSpace(options.OnFailure)) {
				formInfo.Add("data-ajax-failure", options.OnFailure);
			}

			_tag.MergeAttributes(formInfo);
		}

		public void Dispose() {
			_helper.ViewContext.Writer.Write(_tag.CloseTag());
		}
	}
}