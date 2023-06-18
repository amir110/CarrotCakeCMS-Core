﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.IIS;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

/*
* CarrotCake CMS (MVC Core)
* http://www.carrotware.com/
*
* Copyright 2015, 2023, Samantha Copeland
* Dual licensed under the MIT or GPL Version 3 licenses.
*
* Date: June 2023
*/

namespace Carrotware.Web.UI.Components.Controllers {

	public class HomeController : BaseController {
		protected MemoryStream _stream = new MemoryStream();

		public HomeController(IWebHostEnvironment environment) : base(environment) {

		}

		protected override void Dispose(bool disposing) {
			base.Dispose(disposing);

			_stream.Dispose();
		}

		public ActionResult GetImageThumb(string thumb, bool? scale, int? square) {
			CarrotWebHelper.VaryCacheByQuery(new string[] { "thumb", "scale", "square" });

			DoCacheMagic(3);

			string sImageUri = thumb;
			string sImgPath = thumb;

			bool scaleImage = scale.HasValue ? scale.Value : false;
			int iThumb = square.HasValue ? square.Value : 150;

			if (sImageUri.Contains("../") || sImageUri.Contains(@"..\")) {
				throw new Exception("Cannot use relative paths.");
			}
			if (sImageUri.Contains(":")) {
				throw new Exception("Cannot specify drive letters.");
			}
			if (sImageUri.Contains("//") || sImageUri.Contains(@"\\")) {
				throw new Exception("Cannot use UNC paths.");
			}

			if (iThumb < 5 || iThumb > 500) {
				iThumb = 100;
			}

			Bitmap bmpThumb = new Bitmap(iThumb, iThumb);

			int iHeight = iThumb;
			int iWidth = iThumb;

			if (!string.IsNullOrEmpty(sImageUri)) {
				sImgPath = Path.Combine(_environment.WebRootPath, sImageUri);
				if (System.IO.File.Exists(sImgPath)) {
					using (Bitmap bmpOriginal = new Bitmap(sImgPath)) {
						if (scaleImage) {
							int h = bmpOriginal.Height;
							int w = bmpOriginal.Width;

							if (iHeight > 0) {
								iWidth = (int)(((float)w / (float)h) * (float)iHeight);
							} else {
								iHeight = h;
								iWidth = w;
							}
						}

						bmpThumb = ResizeBitmap(bmpOriginal, iWidth, iHeight);
					}
				} else {
					using (Graphics graphics = Graphics.FromImage(bmpThumb)) {
						Rectangle rect = new Rectangle(0, 0, bmpThumb.Width, bmpThumb.Height);
						using (HatchBrush hatch = new HatchBrush(HatchStyle.Weave, Color.BurlyWood, Color.AntiqueWhite)) {
							graphics.FillRectangle(hatch, rect);
							int topPadding = 2; // top and bottom padding in pixels
							int sidePadding = 2; // side padding in pixels
							Font font = new Font(FontFamily.GenericSerif, 10, FontStyle.Bold);
							SolidBrush textBrush = new SolidBrush(Color.Black);

							if (sImageUri.Contains("/")) {
								sImageUri = sImageUri.Substring(sImageUri.LastIndexOf("/") + 1);
							}

							sImageUri = "404 \r\n" + sImageUri;

							graphics.DrawString(sImageUri, font, textBrush, sidePadding, topPadding);

							bmpThumb.Save(_stream, ImageFormat.Png);

							textBrush.Dispose();
							font.Dispose();
						}
					}
				}
			}

			if (bmpThumb == null) {
				return null;
			}

			bmpThumb.Save(_stream, ImageFormat.Png);
			bmpThumb.Dispose();

			return File(_stream.ToArray(), "image/png");
		}

		private Bitmap ResizeBitmap(Bitmap bmpIn, int w, int h) {
			if (w < 1) {
				w = 1;
			}
			if (h < 1) {
				h = 1;
			}

			Bitmap bmpNew = new Bitmap(w, h);
			using (Graphics g = Graphics.FromImage(bmpNew)) {
				g.DrawImage(bmpIn, 0, 0, w, h);
			}

			return bmpNew;
		}

		public ActionResult GetWebResource(string r, string ts) {
			CarrotWebHelper.VaryCacheByQuery(new string[] { "r", "ts" });

			DoCacheMagic(5);

			var mime = "text/x-plain";
			string resource = Utils.DecodeBase64(r);

			var res = resource.Split(':');
			var ext = Path.GetExtension(res[0]);

			if (FileDataHelper.MimeTypes.ContainsKey(ext)) {
				mime = FileDataHelper.MimeTypes[ext];
			}

			if (mime == "text/x-plain") {
				// sometimes the lookup fails, fix these common types so they render binary/byte[]
				var exts = new string[] { ".exe", ".zip", ".png", ".gif", ".jpg", ".jpeg", ".webp", ".mp3", ".mp4" };
				if (exts.Contains(ext.ToLowerInvariant())) {
					mime = "application/octet-stream";
				}
			}

			if (mime.ToLowerInvariant().StartsWith("text")) {
				var assembly = CarrotWebHelper.GetAssembly(res);

				var txt = CarrotWebHelper.GetManifestResourceText(this.GetType(), resource);
				var sb = new StringBuilder(txt);

				//because things like css might have images that are in the dll with the css
				try {
					Regex webResourceRegEx = new Regex(@"<%\s*=\s*(?<rt>WebResource)\(""(?<rn>[^""]*)""\)\s*%>", RegexOptions.Singleline);
					MatchCollection matches = webResourceRegEx.Matches(txt);

					if (matches.Count > 0) {
						List<string> resNames = assembly.GetManifestResourceNames().ToList();

						foreach (Match m in matches) {
							var orig = m.Value;
							Group g = m.Groups["rn"];

							string resourceName = g.Value;

							var shortestNamespace = assembly.GetTypes()
													.Where(x => x != null && !string.IsNullOrEmpty(x.Namespace))
													.OrderBy(n => n.Namespace.Length)
													.Select(n => n.Namespace)
													.FirstOrDefault();

							//var altResourceName1 = string.Format("{0}.{1}", CarrotWebHelper.TrimAssemblyName(assembly), resourceName);
							var altResourceName2 = string.Format("{0}.{1}", shortestNamespace, resourceName);

							//validate that the resource is even there, and match the case that it is stored with
							var embeddedName = resNames.Where(x => x.ToLowerInvariant() == resourceName.ToLowerInvariant()
											//|| x.ToLowerInvariant() == altResourceName1.ToLowerInvariant()
											|| x.ToLowerInvariant() == altResourceName2.ToLowerInvariant()).FirstOrDefault();

							//if plugging in the namespace didn't do it, just get trailing matches
							if (string.IsNullOrWhiteSpace(embeddedName)) {
								embeddedName = resNames.OrderByDescending(x => x.Length)
											.Where(x => x.ToLowerInvariant().EndsWith(resourceName.ToLowerInvariant()))
											.FirstOrDefault();
							}

							// only do the sub if the name is found
							if (!string.IsNullOrWhiteSpace(embeddedName)) {
								// search the same assembly as had the initial file that was loaded
								sb.Replace(orig, CarrotWebHelper.GetWebResourceUrl(assembly, embeddedName));
							}
						}
					}
				} catch (Exception ex) { }

				txt = sb.ToString();

				var byteArray = Encoding.UTF8.GetBytes(txt);
				_stream = new MemoryStream(byteArray);
			} else {
				var bytes = CarrotWebHelper.GetManifestResourceBytes(this.GetType(), resource);
				_stream = new MemoryStream(bytes);
			}

			return File(_stream, mime);
		}

		public ActionResult GetCaptchaImage(string fgcolor, string bgcolor, string ncolor) {
			CarrotWebHelper.VaryCacheByQuery(new string[] { "fgcolor", "bgcolor", "ncolor", "ts" });

			DoCacheMagic(3);

			Color f = CarrotWebHelper.DecodeColor(fgcolor);
			Color b = CarrotWebHelper.DecodeColor(bgcolor);
			Color n = CarrotWebHelper.DecodeColor(ncolor);

			Bitmap bmpCaptcha = CaptchaImage.GetCaptchaImage(f, b, n);

			if (bmpCaptcha == null) {
				Response.StatusCode = 404;
				//Response.StatusDescription = "Not Found";
				byte[] bb = new byte[0];

				return File(bb, "image/png");
			}

			bmpCaptcha.Save(_stream, ImageFormat.Png);
			bmpCaptcha.Dispose();

			return File(_stream.ToArray(), "image/png");
		}

		public ActionResult GetCarrotCalendarCss(string el, string wc, string wb, string cc, string cb,
												string tc, string tb, string tsb, string tl,
												string nc, string nb, string nsb, string nl) {
			CarrotWebHelper.VaryCacheByQuery(new string[] { "el", "ts" });

			DoCacheMagic(7);

			var cal = new Calendar(wc, wb, cc, cb, tc, tb, tsb,
									tl, nc, nb, nsb, nl);

			cal.ElementId = Utils.DecodeBase64(el).Replace("{", "").Replace(">", "").Replace("<", "").Replace(">", "")
									.Replace("'", "").Replace("\\", "").Replace("//", "").Replace(":", "");

			var txt = cal.GenerateCSS();
			var byteArray = Encoding.UTF8.GetBytes(txt);

			_stream = new MemoryStream(byteArray);

			return File(_stream, "text/css");
		}

		public ActionResult GetCarrotHelp(string id) {
			var context = CarrotWebHelper.Current;
			// context.Response.Cache.VaryByParams["ts"] = true;

			DoCacheMagic(10);

			DateTime timeAM = DateTime.Now.Date.AddHours(7);  // 7AM
			DateTime timePM = DateTime.Now.Date.AddHours(17);  // 5PM

			var sb = new StringBuilder();
			sb.Append(CarrotWebHelper.GetManifestResourceText("carrotHelp.js"));

			sb.Replace("[[TIMESTAMP]]", DateTime.UtcNow.ToString("u"));

			sb.Replace("[[SHORTDATEPATTERN]]", CarrotWebHelper.ShortDatePattern);
			sb.Replace("[[SHORTTIMEPATTERN]]", CarrotWebHelper.ShortTimePattern);

			sb.Replace("[[SHORTDATEFORMATPATTERN]]", CarrotWebHelper.ShortDateFormatPattern);
			sb.Replace("[[SHORTDATETIMEFORMATPATTERN]]", CarrotWebHelper.ShortDateTimeFormatPattern);

			sb.Replace("[[AM_TIMEPATTERN]]", timeAM.ToString("tt"));
			sb.Replace("[[PM_TIMEPATTERN]]", timePM.ToString("tt"));

			string sBody = sb.ToString();

			var byteArray = Encoding.UTF8.GetBytes(sBody);
			_stream = new MemoryStream(byteArray);

			return File(_stream, "text/javascript");
		}
	}
}