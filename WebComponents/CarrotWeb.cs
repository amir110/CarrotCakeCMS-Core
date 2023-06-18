﻿using Carrotware.Web.UI.Components.Controllers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.ResponseCaching;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;
using System.Web;
using System.Xml.Linq;

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

	public static class CarrotWebHelper {

		public static CarrotWebHelp CarrotWeb(this HtmlHelper<dynamic> htmlHelper) {
			return new CarrotWebHelp(htmlHelper);
		}

		public static CarrotWebHelp CarrotWeb(this IHtmlHelper htmlHelper) {
			return new CarrotWebHelp(htmlHelper);
		}

		private static IHttpContextAccessor _httpContextAccessor;
		private static IWebHostEnvironment _webHostEnvironment;
		private static IConfigurationRoot _configuration;
		private static IServiceCollection _services;
		private static IServiceProvider _serviceProvider;
		private static SignInManager<IdentityUser> _signinmanager;
		private static UserManager<IdentityUser> _usermanager;
		private static IMemoryCache _memoryCache;

		public static void Configure(IConfigurationRoot configuration, IWebHostEnvironment environment, IServiceCollection services) {
			_configuration = configuration;
			_webHostEnvironment = environment;
			_services = services;

			services.AddMemoryCache();

			_serviceProvider = services.BuildServiceProvider();
			_httpContextAccessor = _serviceProvider.GetRequiredService<IHttpContextAccessor>();
			_memoryCache = _serviceProvider.GetRequiredService<IMemoryCache>();

			try {
				_signinmanager = _serviceProvider.GetRequiredService<SignInManager<IdentityUser>>();
				_usermanager = _serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
			} catch (Exception ex) { }
		}

		public static void RoutesSetup(WebApplication app) {
			Assembly asmbly = typeof(CarrotWebHelper).Assembly;
			string _areaName = string.Empty;

			var _namespaces = asmbly.GetTypes().Select(t => t.Namespace)
								.Where(x => !string.IsNullOrEmpty(x))
								.Distinct().ToList();

			string assemblyName = asmbly.ManifestModule.Name;
			_areaName = assemblyName.Substring(0, assemblyName.Length - 4);

			string home = nameof(HomeController).Replace("Controller", "");

			app.MapControllerRoute(
					name: _areaName + "_GetImageThumb",
					pattern: UrlPaths.ThumbnailPath + "/{id?}",
					defaults: new { controller = home, action = nameof(HomeController.GetImageThumb) });

			app.MapControllerRoute(
					name: _areaName + "_GetCaptchaImage",
					pattern: UrlPaths.CaptchaPath + "/{id?}",
					defaults: new { controller = home, action = nameof(HomeController.GetCaptchaImage) });

			app.MapControllerRoute(
					name: _areaName + "_GetCarrotHelp",
					pattern: UrlPaths.HelperPath + "/{id?}",
					defaults: new { controller = home, action = nameof(HomeController.GetCarrotHelp) });

			app.MapControllerRoute(
					name: _areaName + "_GetCarrotCalendarCss",
					pattern: UrlPaths.CalendarStylePath + "/{id?}",
					defaults: new { controller = home, action = nameof(HomeController.GetCarrotCalendarCss) });

			app.MapControllerRoute(
					name: _areaName + "_GetWebResource",
					pattern: UrlPaths.ResourcePath + "/{id?}",
					defaults: new { controller = home, action = nameof(HomeController.GetWebResource) });
		}

		public static IUrlHelper GetUrlHelper(this IHtmlHelper htmlHelper) {
			var urlHelperFactory = htmlHelper.ViewContext.HttpContext.RequestServices.GetRequiredService<IUrlHelperFactory>();
			return urlHelperFactory.GetUrlHelper(htmlHelper.ViewContext);
		}

		public static IWebHostEnvironment WebHostEnvironment { get { return _webHostEnvironment; } }

		public static IConfigurationRoot Configuration { get { return _configuration; } }

		public static IServiceCollection Services { get { return _services; } }

		public static IServiceProvider ServiceProvider { get { return _serviceProvider; } }

		public static IMemoryCache MemoryCache { get { return _memoryCache; } }

		public static HttpContext Current { get { return HttpContext; } }

		public static HttpRequest Request { get { return HttpContext.Request; } }

		public static HttpResponse Response { get { return HttpContext.Response; } }

		public static HttpContext HttpContext { get { return _httpContextAccessor.HttpContext; } }

		public static void VaryCacheByQuery(string[] keys) {
			var responseCachingFeature = Current.Features.Get<IResponseCachingFeature>();

			if (responseCachingFeature != null && keys != null && keys.Any()) {
				responseCachingFeature.VaryByQueryKeys = keys;
			}
		}

		public static string QueryString(string name) {
			var query = Request.QueryString;

			if (query.HasValue) {
				var dict = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(query.Value);

				if (dict != null) {
					if (dict.ContainsKey(name)) {
						return dict[name];
					}
				}
			}

			return null;
		}

		public static string Session(string name) {
			var query = Current.Session;

			if (query.Keys.Any()) {
				return query.GetString(name);
			}

			return null;
		}

		public static HtmlEncoder HtmlEncoder { get; set; } = HtmlEncoder.Default;
		public static UrlEncoder UrlEncoder { get; set; } = UrlEncoder.Default;

		public static string AssemblyVersion {
			get { return Assembly.GetExecutingAssembly().GetName().Version.ToString(); }
		}

		public static string FileVersion {
			get {
				var assembly = Assembly.GetExecutingAssembly();
				var fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
				return fvi.FileVersion;
			}
		}

		public static string ShortDateFormatPattern {
			get {
				return "{0:" + ShortDatePattern + "}";
			}
		}

		public static string ShortDateTimeFormatPattern {
			get {
				return "{0:" + ShortDatePattern + "} {0:" + ShortTimePattern + "}";
			}
		}

		private static string _shortDatePattern = null;

		public static string ShortDatePattern {
			get {
				if (_shortDatePattern == null) {
					DateTimeFormatInfo _dtf = CultureInfo.CurrentCulture.DateTimeFormat;
					if (_dtf == null) {
						_dtf = CultureInfo.CreateSpecificCulture("en-US").DateTimeFormat;
					}

					_shortDatePattern = _dtf.ShortDatePattern ?? "M/d/yyyy";
					_shortDatePattern = _shortDatePattern.Replace("MM", "M").Replace("dd", "d");
				}

				return _shortDatePattern;
			}
		}

		private static string _shortTimePattern = null;

		public static string ShortTimePattern {
			get {
				if (_shortTimePattern == null) {
					DateTimeFormatInfo _dtf = CultureInfo.CurrentCulture.DateTimeFormat;
					if (_dtf == null) {
						_dtf = CultureInfo.CreateSpecificCulture("en-US").DateTimeFormat;
					}
					_shortTimePattern = _dtf.ShortTimePattern ?? "hh:mm tt";
				}

				return _shortTimePattern;
			}
		}

		public static string EncodeColor(Color color) {
			var colorCode = ColorTranslator.ToHtml(color);

			string sColor = string.Empty;
			if (!string.IsNullOrEmpty(colorCode)) {
				sColor = colorCode.ToLowerInvariant();
				sColor = sColor.Replace("#", string.Empty);
				sColor = sColor.Replace("HEX-", string.Empty);
				sColor = HttpUtility.HtmlEncode(sColor);
			}
			return sColor;
		}

		public static string DecodeColorString(string colorCode) {
			string sColor = string.Empty;
			if (!string.IsNullOrEmpty(colorCode)) {
				sColor = colorCode;
				sColor = HttpUtility.HtmlDecode(sColor);
				sColor = sColor.Replace("HEX-", string.Empty);
				if (!sColor.StartsWith("#")) {
					sColor = string.Format("#{0}", sColor);
				}
			}

			return sColor;
		}

		public static Color DecodeColor(string colorCode) {
			string sColor = DecodeColorString(colorCode);

			if (sColor.ToLowerInvariant().EndsWith("transparent")) {
				return Color.Transparent;
			}
			if (sColor == "#" || string.IsNullOrWhiteSpace(sColor)
					|| sColor.ToLowerInvariant().EndsWith("empty")) {
				return Color.Empty;
			}

			return ColorTranslator.FromHtml(sColor);
		}

		public static string GetAssemblyName(this Assembly assembly) {
			string assemblyName = assembly.ManifestModule.Name;
			return assemblyName.Substring(0, assemblyName.Length - 4);
		}

		public static string HtmlFormat(StringBuilder input) {
			if (input != null) {
				return HtmlFormat(input.ToString());
			}

			return string.Empty;
		}

		public static string HtmlFormat(string input) {
			if (!string.IsNullOrWhiteSpace(input)) {
				bool autoAddTypes = false;
				var subs = new Dictionary<string, int>();
				subs.Add("ndash", 150);
				subs.Add("mdash", 151);
				subs.Add("nbsp", 153);
				subs.Add("trade", 153);
				subs.Add("copy", 169);
				subs.Add("reg", 174);
				subs.Add("laquo", 171);
				subs.Add("raquo", 187);
				subs.Add("lsquo", 145);
				subs.Add("rsquo", 146);
				subs.Add("ldquo", 147);
				subs.Add("rdquo", 148);
				subs.Add("bull", 149);
				subs.Add("amp", 38);
				subs.Add("quot", 34);

				var subs2 = new Dictionary<string, int>();
				subs2.Add("ndash", 150);
				subs2.Add("mdash", 151);
				subs2.Add("nbsp", 153);
				subs2.Add("trade", 153);
				subs2.Add("copy", 169);
				subs2.Add("reg", 174);
				subs2.Add("laquo", 171);
				subs2.Add("raquo", 187);
				subs2.Add("bull", 149);

				string docType = string.Empty;

				if (!input.ToLowerInvariant().StartsWith("<!doctype")) {
					autoAddTypes = true;

					docType = "<!DOCTYPE html [ ";
					foreach (var s in subs) {
						docType += string.Format(" <!ENTITY {0} \"&#{1};\"> ", s.Key, s.Value);
					}
					docType += " ]>".Replace("  ", " ");

					input = docType + Environment.NewLine + input;
				}

				var doc = XDocument.Parse(input);

				if (autoAddTypes) {
					var sb = new StringBuilder();
					sb.Append(doc.ToString().Replace(docType, string.Empty));

					foreach (var s in subs2) {
						sb.Replace(Convert.ToChar(s.Value).ToString(), string.Format("&{0};", s.Key));
					}

					return sb.ToString();
				}

				return doc.ToString();
			}

			return string.Empty;
		}

		public static string RenderToString(this IHtmlContent content) {
			if (content == null) {
				return null;
			}
			string ret = null;

			using (var writer = new StringWriter()) {
				content.WriteTo(writer, HtmlEncoder.Default);
				ret = writer.ToString();
			}

			return ret;
		}

		public static HtmlString RenderToHtmlString(this IHtmlContent content) {
			return content.RenderToString().ToHtmlString();
		}

		public static HtmlString ToHtmlString(this string value) {
			return new HtmlString(value);
		}

		public static string ToKebabCase(this string input) {
			return string.Concat(input.Select((c, i) => (char.IsUpper(c) && i > 0 ? "-" : string.Empty) + char.ToLower(c)));
		}

		public static IDictionary<string, object> ToAttributeDictionary(this object attributes) {
			IDictionary<string, object> attribDict = null;

			if (attributes != null) {
				if ((attributes is IDictionary<string, object>)
							|| (attributes is Dictionary<string, object>)) {
					attribDict = (IDictionary<string, object>)attributes;
				} else {
					attribDict = HtmlHelper.AnonymousObjectToHtmlAttributes(attributes);
				}
			}

			return attribDict;
		}

		//================================
		public static string DateKey() {
			return GenerateTick(DateTime.UtcNow).ToString();
			//return DateKey(15);
		}

		public static string DateKey(int interval) {
			DateTime now = DateTime.UtcNow;
			TimeSpan d = TimeSpan.FromMinutes(interval);
			DateTime dt = new DateTime(((now.Ticks + d.Ticks - 1) / d.Ticks) * d.Ticks);
			byte[] dateStringBytes = Encoding.ASCII.GetBytes(dt.ToString("U"));

			return Convert.ToBase64String(dateStringBytes);
		}

		private static long GenerateTick(DateTime dateIn) {
			int roundTo = 12;
			dateIn = dateIn.AddMinutes(-2);
			int iMin = roundTo * (dateIn.Minute / roundTo);

			DateTime dateOut1 = dateIn.AddMinutes(0 - dateIn.Minute).AddMinutes(iMin);

			var dateOut = new DateTime(dateOut1.Year, dateOut1.Month, dateOut1.Day, dateOut1.Hour, dateOut1.Minute, dateOut1.Minute, (2 * dateOut1.DayOfYear), DateTimeKind.Utc);

			return dateOut.Ticks;
		}

		private static string GetInternalResourceName(string resource) {
			if (resource.ToLowerInvariant().StartsWith("carrotware.web.ui")) {
				return resource;
			}

			return string.Format("Carrotware.Web.UI.Components.{0}", resource);
		}

		internal static string GetWebResourceUrl(string resource) {
			return GetWebResourceUrl(typeof(CarrotWebHelp), GetInternalResourceName(resource));
		}

		public static string GetWebResourceUrl(Type type, string resource) {
			var asmb = type.Assembly;

			return GetWebResourceUrl(asmb, resource);
		}

		public static string GetWebResourceUrl(Assembly assembly, string resource) {
			string sUri = string.Empty;

			var asmb = assembly.ManifestModule.Name;
			var resName = HttpUtility.HtmlEncode(Utils.EncodeBase64(string.Format("{0}:{1}", resource, asmb)));

			try {
				var ver = assembly.GetName().Version.ToString().Replace(".", string.Empty);
				sUri = string.Format("{0}?r={1}&ts={2}-{3}", UrlPaths.ResourcePath, resName, ver, DateKey());
			} catch {
				sUri = string.Format("{0}?r={1}&ts={2}", UrlPaths.ResourcePath, resName, DateKey());
			}

			return sUri;
		}

		internal static Assembly GetAssembly(Type type, string resource) {
			return GetAssembly(type, resource.Split(':'));
		}

		internal static Assembly GetAssembly(Type type, string[] res) {
			if (res.Length > 1) {
				var dir = AppDomain.CurrentDomain.BaseDirectory ?? AppDomain.CurrentDomain.RelativeSearchPath;

				return Assembly.LoadFrom(Path.Combine(dir, res[1]));
			}

			return Assembly.GetAssembly(type);
		}

		internal static Assembly GetAssembly(string[] res) {
			return GetAssembly(typeof(CarrotWebHelp), res);
		}

		internal static Assembly GetAssembly(string resource) {
			return GetAssembly(typeof(CarrotWebHelp), resource);
		}

		internal static string GetManifestResourceText(string resource) {
			return GetManifestResourceText(typeof(CarrotWebHelp), GetInternalResourceName(resource));
		}

		internal static byte[] GetManifestResourceBytes(string resource) {
			return GetManifestResourceBytes(typeof(CarrotWebHelp), GetInternalResourceName(resource));
		}

		internal static string TrimAssemblyName(Assembly assembly) {
			var asmb = assembly.ManifestModule.Name;
			return asmb.Substring(0, asmb.Length - 4);
		}

		internal static string[] FixResourceName(Assembly assembly, string[] res) {
			if (res.Length > 1) {
				var asmbName = TrimAssemblyName(assembly);

				if (!res[0].StartsWith(asmbName)) {
					res[0] = string.Format("{0}.{1}", asmbName, res[0]);
				}
			}

			return res;
		}

		public static string GetManifestResourceText(Type type, string resource) {
			string returnText = null;
			var res = resource.Split(':');

			var assembly = GetAssembly(type, res);
			res = FixResourceName(assembly, res);

			using (var stream = new StreamReader(assembly.GetManifestResourceStream(res[0]))) {
				returnText = stream.ReadToEnd();
			}

			return returnText;
		}

		public static byte[] GetManifestResourceBytes(Type type, string resource) {
			byte[] returnBytes = null;
			var res = resource.Split(':');

			var assembly = GetAssembly(type, res);
			res = FixResourceName(assembly, res);

			using (var stream = assembly.GetManifestResourceStream(res[0])) {
				returnBytes = new byte[stream.Length];
				stream.Read(returnBytes, 0, returnBytes.Length);
			}

			return returnBytes;
		}

		//================================

		public static string GetActionName<T>(this T instance, Expression<Action<T>> expression) {
			var expressionBody = expression.Body;

			if (expressionBody == null) {
				throw new ArgumentException("Cannot be null.");
			}

			if ((expressionBody is MethodCallExpression) != true) {
				throw new ArgumentException("Methods only!");
			}

			if (expressionBody is MethodCallExpression) {
				var methodCallExpression = (MethodCallExpression)expressionBody;
				return methodCallExpression.Method.Name;
			}

			return string.Empty;
		}

		public static string GetPropertyName<T, P>(this T instance, Expression<Func<T, P>> expression) {
			var expressionBody = expression.Body;

			if (expressionBody == null) {
				throw new ArgumentException("Cannot be null.");
			}

			if ((expressionBody is MemberExpression) != true) {
				throw new ArgumentException("Properties only!");
			}

			if (expressionBody is MemberExpression) {
				// Reference type property or field
				var memberExpression = (MemberExpression)expressionBody;
				return memberExpression.Member.Name;
			}

			return string.Empty;
		}

		public static string DisplayNameFor<T>(Expression<Func<T, object>> expression) {
			string propertyName = string.Empty;
			PropertyInfo propInfo = null;
			Type type = null;

			MemberExpression memberExpression = expression.Body as MemberExpression ??
												((UnaryExpression)expression.Body).Operand as MemberExpression;
			if (memberExpression != null) {
				propertyName = memberExpression.Member.Name;
				type = memberExpression.Member.DeclaringType;
				propInfo = type.GetProperty(propertyName);
			}

			if (!string.IsNullOrEmpty(propertyName) && type != null) {
				DisplayAttribute attribute1 = propInfo.GetCustomAttributes(typeof(DisplayAttribute), true).FirstOrDefault() as DisplayAttribute;
				if (attribute1 != null) {
					return attribute1.Name;
				}

				DisplayNameAttribute attribute2 = propInfo.GetCustomAttributes(typeof(DisplayNameAttribute), true).FirstOrDefault() as DisplayNameAttribute;
				if (attribute2 != null) {
					return attribute2.DisplayName;
				}

				MetadataTypeAttribute metadataType = (MetadataTypeAttribute)type.GetCustomAttributes(typeof(MetadataTypeAttribute), true).FirstOrDefault();
				if (metadataType != null) {
					PropertyInfo metaProp = metadataType.MetadataClassType.GetProperty(propInfo.Name);
					if (metaProp != null) {
						DisplayAttribute attribute3 = (DisplayAttribute)metaProp.GetCustomAttributes(typeof(DisplayAttribute), true).SingleOrDefault();
						if (attribute3 != null) {
							return attribute3.Name;
						}

						DisplayNameAttribute attribute4 = (DisplayNameAttribute)metaProp.GetCustomAttributes(typeof(DisplayNameAttribute), true).SingleOrDefault();
						if (attribute4 != null) {
							return attribute4.DisplayName;
						}
					}
				}
			}

			return string.Empty;
		}

		public static IHtmlContent ValidationMultiMessageFor<T>(this IHtmlHelper<T> htmlHelper,
			Expression<Func<T, object>> property, object listAttributes = null, bool messageAsSpan = false) {
			MemberExpression memberExpression = property.Body as MemberExpression ??
									((UnaryExpression)property.Body).Operand as MemberExpression;

			//   prop _html vs IHtmlHelper<T>
			if (memberExpression != null) {
				string propertyName = propertyName = ReflectionUtilities.BuildProp<T>(property);

				ModelStateDictionary stateDictionary = htmlHelper.ViewData.ModelState;

				if (stateDictionary[propertyName] != null) {
					StringBuilder sb = new StringBuilder();
					sb.Append(string.Empty);
					string validationClass = "field-validation-valid";

					foreach (var err in stateDictionary[propertyName].Errors) {
						if (!string.IsNullOrEmpty(err.ErrorMessage.Trim())) {
							if (messageAsSpan) {
								sb.AppendLine(string.Format("<span>{0}</span> ", err.ErrorMessage.Trim()));
							} else {
								sb.AppendLine(string.Format("<li>{0}</li>", err.ErrorMessage.Trim()));
							}
							validationClass = "field-validation-error";
						}
					}

					var msgBuilder = new HtmlTag("ul");
					if (messageAsSpan) {
						msgBuilder = new HtmlTag("span");
					}

					// can be overwritten
					msgBuilder.MergeAttribute("data-valmsg-replace", "true");

					msgBuilder.MergeAttribute("class", validationClass);

					msgBuilder.MergeAttributes(listAttributes);

					// force the data-valmsg-for value to match the property name
					msgBuilder.MergeAttribute("data-valmsg-for", propertyName);

					msgBuilder.InnerHtml = sb.ToString();

					return new HtmlString(msgBuilder.ToString());
				}
			}

			return new HtmlString(string.Empty);
		}
	}

	// ================================

	public class CarrotWebHelp {
		private IHtmlHelper _helper;

		public CarrotWebHelp(IHtmlHelper htmlHelper) {
			_helper = htmlHelper;
		}

		public HtmlString GetRoot() {
			return new HtmlString(CarrotWebHelper.WebHostEnvironment.ContentRootPath);
		}

		public HtmlString GetWebRoot() {
			return new HtmlString(CarrotWebHelper.WebHostEnvironment.WebRootPath);
		}

		public HtmlString GetEnv() {
			return new HtmlString(CarrotWebHelper.WebHostEnvironment.EnvironmentName);
		}

		public HtmlString GetName() {
			return new HtmlString(CarrotWebHelper.WebHostEnvironment.ApplicationName);
		}

		public HtmlString GetPath() {
			return new HtmlString(CarrotWebHelper.Request.Path);
		}

		public HtmlString GetIP() {
			return new HtmlString(CarrotWebHelper.Current.Connection.RemoteIpAddress.ToString());
		}

		public HtmlString GetHost() {
			return new HtmlString(CarrotWebHelper.Request.Host.Host);
		}

		public HtmlString GePort() {
			return new HtmlString(CarrotWebHelper.Request.Host.Port.ToString());
		}

		public HtmlString GetQuery() {
			return new HtmlString(CarrotWebHelper.Request.QueryString.Value);
		}

		public HtmlString GetHttps() {
			return new HtmlString(CarrotWebHelper.Request.IsHttps.ToString());
		}

		public HtmlString GetAssemblyVersion() {
			return new HtmlString(CarrotWebHelper.AssemblyVersion);
		}

		public HtmlString GetFileVersion() {
			return new HtmlString(CarrotWebHelper.FileVersion);
		}

		//================================
		public CarrotWebGrid<T> CarrotWebGrid<T>() where T : class {
			return new CarrotWebGrid<T>(_helper);
		}

		public CarrotWebGrid<T> CarrotWebGrid<T>(PagedData<T> dp) where T : class {
			return new CarrotWebGrid<T>(_helper, dp);
		}

		public CarrotWebDataTable CarrotWebDataTable() {
			return new CarrotWebDataTable(_helper);
		}

		public CarrotWebDataTable CarrotWebDataTable(PagedDataTable dp) {
			return new CarrotWebDataTable(_helper, dp);
		}

		public CarrotWebGrid<T> CarrotWebGrid<T>(List<T> lst) where T : class {
			PagedData<T> dp = new PagedData<T>();
			dp.DataSource = lst;
			dp.PageNumber = 1;
			dp.TotalRecords = lst.Count();

			var grid = new CarrotWebGrid<T>(_helper, dp);
			grid.UseDataPage = false;

			return grid;
		}

		public IHtmlContent MetaTag(string Name, string Content) {
			var metaTag = new HtmlTag("meta");
			metaTag.MergeAttribute("name", Name);
			metaTag.MergeAttribute("content", Content);

			return new HtmlString(metaTag.RenderSelfClosingTag());
		}

		public IUrlHelper GetUrlHelper() {
			var urlHelperFactory = _helper.ViewContext.HttpContext.RequestServices.GetRequiredService<IUrlHelperFactory>();
			return urlHelperFactory.GetUrlHelper(_helper.ViewContext);
		}

		public SimpleAjaxForm BeginSimpleAjaxForm(SimpleAjaxFormOptions options, object route = null, object attributes = null) {
			return new SimpleAjaxForm(_helper, options, route, attributes);
		}

		public SimpleAjaxForm BeginSimpleAjaxForm(SimpleAjaxFormOptions options, object route = null) {
			return new SimpleAjaxForm(_helper, options, route);
		}

		public SimpleAjaxForm BeginSimpleAjaxForm(SimpleAjaxFormOptions options) {
			return new SimpleAjaxForm(_helper, options);
		}

		public SimpleAjaxForm BeginSimpleAjaxForm() {
			var options = new SimpleAjaxFormOptions();
			return new SimpleAjaxForm(_helper, options);
		}

		public IHtmlContent ActionImage(string actionName,
										string controllerName,
										object routeValues,
										string imagePath,
										string imageAltText = "",
										object imageAttributes = null,
										object linkAttributes = null) {
			var url = GetUrlHelper();

			var anchorBuilder = new HtmlTag("a");
			anchorBuilder.Uri = url.Action(actionName, controllerName, routeValues);
			anchorBuilder.MergeAttributes(linkAttributes);

			var imgBuilder = new HtmlTag("img");
			imgBuilder.Uri = url.Content(imagePath);
			imgBuilder.MergeAttribute("alt", imageAltText);
			imgBuilder.MergeAttribute("title", imageAltText);
			imgBuilder.MergeAttributes(imageAttributes);

			string imgHtml = imgBuilder.RenderSelfClosingTag();

			anchorBuilder.InnerHtml = imgHtml;

			return new HtmlString(anchorBuilder.ToString());
		}

		public WrappedItem BeginWrappedItem(string tag,
					string actionName, string controllerName,
					object activeAttributes = null, object inactiveAttributes = null) {
			return new WrappedItem(_helper, tag, actionName, controllerName, activeAttributes, inactiveAttributes);
		}

		public WrappedItem BeginWrappedItem(string tag,
							int currentPage, int selectedPage,
							object activeAttributes = null, object inactiveAttributes = null) {
			return new WrappedItem(_helper, tag, currentPage, selectedPage, activeAttributes, inactiveAttributes);
		}

		public WrappedItem BeginWrappedItem(string tag, object htmlAttributes = null) {
			return new WrappedItem(_helper, tag, htmlAttributes);
		}

		public IHtmlContent ImageSizer(string ImageUrl, string Title, int ThumbSize, bool ScaleImage, object imageAttributes = null) {
			ImageSizer img = new ImageSizer();
			img.ImageUrl = ImageUrl;
			img.Title = Title;
			img.ThumbSize = ThumbSize;
			img.ScaleImage = ScaleImage;
			img.ImageAttributes = imageAttributes;

			return new HtmlString(img.ToHtmlString());
		}

		//================================
		public IHtmlContent RenderControlToHtml(IWebComponent ctrl) {
			return new HtmlString(ctrl.GetHtml());
		}

		public IHtmlContent RenderTwoPartControlBody(ITwoPartWebComponent ctrl) {
			return new HtmlString(ctrl.GetBody());
		}

		public IHtmlContent RenderTwoPartControlBodyCss(ITwoPartWebComponent ctrl) {
			return new HtmlString(ctrl.GetHead());
		}
	}
}