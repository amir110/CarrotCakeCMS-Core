﻿using System.Drawing;

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

	public class Captcha : BaseWebComponent, IValidateHuman {

		public Captcha() {
			this.NoiseColor = ColorTranslator.FromHtml(CaptchaImage.NColorDef);
			this.ForeColor = ColorTranslator.FromHtml(CaptchaImage.FGColorDef);
			this.BackColor = ColorTranslator.FromHtml(CaptchaImage.BGColorDef);

			this.Instructions = "Please enter the code from the image above in the box below.";
			this.IsValidMessage = "Code correct!";
			this.IsNotValidMessage = "Code incorrect, try again!";
			this.AltValidationFailText = "Failed to validate CAPTCHA.";
		}

		public void SetNoiseColor(string colorCode) {
			this.NoiseColor = CarrotWebHelper.DecodeColor(colorCode);
		}

		public void SetForeColor(string colorCode) {
			this.ForeColor = CarrotWebHelper.DecodeColor(colorCode);
		}

		public void SetBackColor(string colorCode) {
			this.BackColor = CarrotWebHelper.DecodeColor(colorCode);
		}

		public string CaptchaText { get; set; } = string.Empty;

		public override string ToString() {
			return this.CaptchaText;
		}

		public string ValidationGroup { get; set; } = string.Empty;

		public string ValidationMessage { get; set; } = string.Empty;

		private bool IsValid { get; set; }

		public string Instructions { get; set; }

		public string IsValidMessage { get; set; }

		public string IsNotValidMessage { get; set; }

		public Color NoiseColor { get; set; }
		public Color ForeColor { get; set; }
		public Color BackColor { get; set; }

		public bool Validate() {
			this.IsValid = CaptchaImage.Validate(this.CaptchaText);

			if (!this.IsValid) {
				this.ValidationMessage = this.IsNotValidMessage;
			} else {
				this.ValidationMessage = this.IsValidMessage;
			}

			return this.IsValid;
		}

		public object? ImageAttributes { get; set; }

		public override string GetHtml() {
			var key = CaptchaImage.SessionKeyValue;

			var imgBuilder = new HtmlTag("img", this.GetCaptchaImageURI());
			imgBuilder.MergeAttribute("alt", key);
			imgBuilder.MergeAttribute("title", key);
			if (this.ImageAttributes != null) {
				imgBuilder.MergeAttributes(this.ImageAttributes);
			}

			return imgBuilder.RenderSelfClosingTag();
		}

		private string GetCaptchaImageURI() {
			return string.Format("{0}?ts={1}", UrlPaths.CaptchaPath, DateTime.Now.Ticks) +
					"&fgcolor=" + CarrotWebHelper.EncodeColor(this.ForeColor) +
					"&bgcolor=" + CarrotWebHelper.EncodeColor(this.BackColor) +
					"&ncolor=" + CarrotWebHelper.EncodeColor(this.NoiseColor);
		}

		public bool ValidateValue(string testValue) {
			this.CaptchaText = testValue;
			return Validate();
		}

		public string AltValidationFailText { get; set; }
	}
}