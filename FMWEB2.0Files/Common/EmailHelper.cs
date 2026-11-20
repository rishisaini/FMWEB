using AssetCare.Model.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Hosting;
using System.Xml;
using System.Xml.Xsl;

namespace AssetCare.Common
{
    public class EmailHelper
    {


        public EmailHelper() { }



        public bool SendMail(string emailReceiver, IDictionary<string, string> contentData, string subject, string templateName, string fromEmail = null, string cc = "")
        {
            try
            {
                if (string.IsNullOrEmpty(fromEmail))
                {
                    fromEmail = ConfigurationManager.AppSettings["AssetCareSettings:AssetCareAlertEMail"];
                }
                var transformedContents = TransformContents(GetEmailContent(templateName), contentData);
                SendEMail(fromEmail, emailReceiver, transformedContents, subject, templateName: templateName, CC: cc);
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }



        public bool SendMailWithAttachments( string emailReceiver, IDictionary<string, string> contentData, string subject, string templateName, byte[] attachmentData, string attachmentName, string cc = "", string fromEmail = null)
        {
            if (string.IsNullOrEmpty(fromEmail))
            {
                fromEmail = ConfigurationManager.AppSettings["AssetCareSettings:AssetCareAlertEMail"];
            }

            var transformedContents = TransformContents(GetEmailContent(templateName), contentData);
            try
            {
                SendEMail(fromEmail, emailReceiver, transformedContents, subject, templateName, attachmentData, attachmentName, cc, true);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public bool SendMail(string fromEmail, string emailReceiver, IDictionary<string, string> contentData, string subject, string templateName, List<UploadItem> uploadItems, string cc = "")
        {
            var transformedContents = TransformContents(GetEmailContent(templateName), contentData);
            try
            {
                SendWithMultiAttachment(fromEmail, emailReceiver, transformedContents, subject, uploadItems, cc);
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        private int SendWithMultiAttachment(string fromEmail, string emailReceiver, string contents, string subject, List<UploadItem> uploadItems = null, string CC = "")
        {
            int newEmailId = 0;
            using (var email = new MailMessage())
            {
                email.From = new MailAddress(fromEmail);
                var env = ConfigurationManager.AppSettings["AssetCareSettings:Env"];
                string targetEmail = string.Empty;
                if ((env != null) && ("PROD".Equals(env.ToUpper())))
                {
                    ExtractEmails(emailReceiver).ForEach(mailAddress => email.To.Add(mailAddress));
                }
                else
                {
                    targetEmail = ConfigurationManager.AppSettings["AssetCareSettings:TestPropertyEMail"];
                    if ((string.IsNullOrEmpty(targetEmail)))
                        targetEmail = ConfigurationManager.AppSettings["AssetCareSettings:AssetCareEMail"];

                    email.To.Add(targetEmail);
                }

                if (!string.IsNullOrWhiteSpace(CC))
                {
                    ExtractEmails(CC).ForEach(mailAddress => email.CC.Add(mailAddress));
                }

                email.Subject = subject;
                email.IsBodyHtml = true;
                email.Body = contents;

                foreach (var uploadItem in uploadItems)
                {
                    if (uploadItem != null && uploadItem.FileToUpload.ContentLength > 0)
                    {
                        var fileNameSplit = uploadItem.FileToUpload.FileName.Split('.');
                        var fileName = fileNameSplit[0] + $"({uploadItem.DocType})" + "." + fileNameSplit[1];
                        var attachment = new Attachment
                                (
                                    uploadItem.FileToUpload.InputStream,
                                    fileName,
                                    uploadItem.FileToUpload.ContentType
                                );
                        email.Attachments.Add(attachment);
                    }
                }

                using (var smtp = new SmtpClient())
                {
                    try
                    {
                        smtp.Send(email);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }
            }
            return newEmailId;
        }

        private List<MailAddress> ExtractEmails(string mailAddresses)
        {
            List<MailAddress> addresses = new List<MailAddress>();
            try
            {
                string[] emails = mailAddresses.ToLower().Split(';');
                foreach (string email in emails)
                {
                    if (!String.IsNullOrEmpty(email))
                    {
                        addresses.Add(new MailAddress(email.Trim()));
                    }
                }


            }
            catch (Exception)
            {
                //sink
            }

            return addresses;
        }
        private readonly static Regex regex = new Regex("%(?<name>.+?)%");

        private string TransformContents(string input, IDictionary<string, string> replacements)
        {
            string result = regex.Replace(input, m =>
            {

                string name = m.Groups["name"].Value;
                string value;
                if (replacements.TryGetValue(name, out value))
                {
                    return value;
                }
                else
                {
                    return m.Captures[0].Value;
                }
            });

            return result;
        }
        private string GetEmailContent(string templateName, bool embedLogo = true)
        {
            string emailContent = File.ReadAllText(string.Concat(AppDomain.CurrentDomain.BaseDirectory + @"/App_Data/EmailTemplates/" + templateName));
            return emailContent;
        }

        private int SendEMail(string fromEmail, string emailReceiver, string contents, string subject, string templateName, byte[] attachmentData = null, string attachmentName = "", string CC = "", bool embedLogo = true)
        {

            int newEmailId = 0;
            using (var email = new MailMessage())
            {
                email.From = new MailAddress(fromEmail);
                var env = ConfigurationManager.AppSettings["AssetCareSettings:Env"];
                string targetEmail = string.Empty;
                if ((env != null) && ("PROD".Equals(env.ToUpper())))
                {
                    ExtractEmails(emailReceiver).ForEach(mailAddress => email.To.Add(mailAddress));
                }
                else
                {
                    targetEmail = ConfigurationManager.AppSettings["AssetCareSettings:TestPropertyEMail"];
                    if ((string.IsNullOrEmpty(targetEmail)))
                        targetEmail = ConfigurationManager.AppSettings["AssetCareSettings:AssetCareEMail"];

                    email.To.Add(targetEmail);
                }



                if (!string.IsNullOrWhiteSpace(CC))
                    email.CC.Add(CC);

                email.Subject = subject;
                email.IsBodyHtml = true;
                email.Body = contents;


                if (attachmentData != null)
                {
                    var file = new Attachment(new MemoryStream(attachmentData), attachmentName);
                    var disposition = file.ContentDisposition;
                    disposition.FileName = attachmentName;
                    disposition.DispositionType = DispositionTypeNames.Attachment;
                    email.Attachments.Add(file);
                }


                using (var smtp = new SmtpClient())
                {
                    try
                    {
                        //SendMailAsync is not working in production environment for some reason
                        //smtp.SendMailAsync(message); 
                        smtp.Send(email);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                        //ErrorSignal.FromCurrentContext().Raise(ex);
                    }
                }



            }
            return newEmailId;
        }

    }
}


