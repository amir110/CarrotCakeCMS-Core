﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Reflection;
using Carrotware.Web.UI.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

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

	public static class EmailHelper {

		private static Version CurrentDLLVersion {
			get { return Assembly.GetExecutingAssembly().GetName().Version; }
		}

		public static bool SendMail(IWebHostEnvironment environment, string fromEmail, string emailTo, string subjectLine, string bodyText, bool isHTML) {
			List<string> lst = new List<string>();
			if (string.IsNullOrEmpty(emailTo)) {
				emailTo = string.Empty;
			}
			//emailTo = emailTo.Replace(",", ";");

			if (emailTo.Contains(";")) {
				lst = emailTo.Split(';').Where(x => x.Length > 2).Select(x => x.Trim()).ToList();
			} else {
				lst.Add(emailTo);
			}

			return SendMail(environment, fromEmail, lst, null, subjectLine, bodyText, isHTML, null);
		}

		public static bool SendMail(IWebHostEnvironment environment, string fromEmail, List<string> emailTo, List<string> emailCC,
				string subjectLine, string bodyText, bool isHTML, List<string> attachments) {
			HttpContext context = HttpContextHelper.Current;
			EMailSettings mailSettings = EMailSettings.GetEMailSettings(environment);

			if (string.IsNullOrEmpty(fromEmail)) {
				fromEmail = mailSettings.ReturnAddress;
			}

			if (emailTo != null && emailTo.Any()) {
				MailMessage message = new MailMessage {
					From = new MailAddress(fromEmail),
					Subject = subjectLine,
					Body = bodyText,
					IsBodyHtml = isHTML
				};

				message.Headers.Add("X-Computer", Environment.MachineName);
				message.Headers.Add("X-Originating-IP", CarrotWebHelper.Current.Connection.RemoteIpAddress.ToString());
				message.Headers.Add("X-Application", "Carrotware Web " + CurrentDLLVersion);
				message.Headers.Add("User-Agent", "Carrotware Web " + CurrentDLLVersion);
				message.Headers.Add("Message-ID", "<" + Guid.NewGuid().ToString().ToLowerInvariant() + "@" + mailSettings.MailDomainName + ">");

				foreach (var t in emailTo) {
					message.To.Add(new MailAddress(t));
				}

				if (emailCC != null) {
					foreach (var t in emailCC) {
						message.CC.Add(new MailAddress(t));
					}
				}

				if (attachments != null) {
					foreach (var f in attachments) {
						Attachment a = new Attachment(f, MediaTypeNames.Application.Octet);
						ContentDisposition disp = a.ContentDisposition;
						disp.CreationDate = System.IO.File.GetCreationTime(f);
						disp.ModificationDate = System.IO.File.GetLastWriteTime(f);
						disp.ReadDate = System.IO.File.GetLastAccessTime(f);
						message.Attachments.Add(a);
					}
				}

				using (SmtpClient client = new SmtpClient()) {
					if (mailSettings.DeliveryMethod == SmtpDeliveryMethod.Network
							&& !string.IsNullOrEmpty(mailSettings.MailUserName)
							&& !string.IsNullOrEmpty(mailSettings.MailPassword)) {
						client.Host = mailSettings.MailDomainName;
						client.Credentials = new NetworkCredential(mailSettings.MailUserName, mailSettings.MailPassword);
					} else {
						client.Credentials = new NetworkCredential();
					}

					client.Send(message);
				}
			}

			return true;
		}
	}
}