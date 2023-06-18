﻿using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Xml;
using Carrotware.Web.UI.Components;
using Microsoft.AspNetCore.Hosting;

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

	public class EMailSettings {

		public EMailSettings() {
			this.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
			this.MailDomainName = string.Empty;
			this.MailUserName = string.Empty;
			this.MailPassword = string.Empty;
			this.ReturnAddress = string.Empty;
		}

		public SmtpDeliveryMethod DeliveryMethod { get; set; }
		public string MailDomainName { get; set; }
		public string MailUserName { get; set; }
		public string MailPassword { get; set; }
		public string ReturnAddress { get; set; }

		public static EMailSettings GetEMailSettings(IWebHostEnvironment environment) {
			var wwwPath = environment.WebRootPath;
			var mailSettings = new EMailSettings();

			var xDoc = new XmlDocument();
			xDoc.Load(Path.Combine(wwwPath, "Web.config"));

			XmlElement xmlMailSettings = xDoc.SelectSingleNode("//system.net/mailSettings/smtp") as XmlElement;

			if (xmlMailSettings != null) {
				if (xmlMailSettings.Attributes["from"] != null) {
					mailSettings.ReturnAddress = xmlMailSettings.Attributes["from"].Value;
				}
				if (xmlMailSettings.Attributes["deliveryMethod"] != null && xmlMailSettings.Attributes["deliveryMethod"].Value.ToLowerInvariant() == "network") {
					mailSettings.DeliveryMethod = SmtpDeliveryMethod.Network;
					if (xmlMailSettings.HasChildNodes) {
						XmlNode xmlNetSettings = xmlMailSettings.SelectSingleNode("//system.net/mailSettings/smtp/network");
						if (xmlNetSettings != null && xmlNetSettings.Attributes["password"] != null) {
							mailSettings.MailUserName = xmlNetSettings.Attributes["userName"].Value;
							mailSettings.MailPassword = xmlNetSettings.Attributes["password"].Value;
							mailSettings.MailDomainName = xmlNetSettings.Attributes["host"].Value;
						}
					}
				}
			}

			if (string.IsNullOrEmpty(mailSettings.MailDomainName)) {
				mailSettings.MailDomainName = CarrotWebHelper.Request.Host.Host;
			}

			if (string.IsNullOrEmpty(mailSettings.ReturnAddress)) {
				mailSettings.ReturnAddress = "no-reply@" + mailSettings.MailDomainName;
			}

			return mailSettings;
		}
	}
}