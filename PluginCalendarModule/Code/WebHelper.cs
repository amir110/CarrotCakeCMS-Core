﻿using Carrotware.CMS.Interface;
using System.Globalization;
using System.Reflection;

namespace CarrotCake.CMS.Plugins.CalendarModule {

	public class WebHelper {

		public static string ReadEmbededScript(string resouceName) {
			string sReturn = null;

			Assembly assembly = Assembly.GetExecutingAssembly();
			using (var stream = new StreamReader(assembly.GetManifestResourceStream(resouceName))) {
				sReturn = stream.ReadToEnd();
			}

			return sReturn;
		}

		private static string _areaName = null;

		public static string AssemblyName {
			get {
				if (_areaName == null) {
					Assembly asmbly = Assembly.GetExecutingAssembly();

					_areaName = asmbly.GetAssemblyName();
				}

				return _areaName;
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

		public static string ShortDateFormatPattern {
			get {
				return "{0:" + ShortDatePattern + "}";
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
	}
}