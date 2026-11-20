using AssetCare.Model.Models;
using AssetCare.Model.ViewModels;
using AssetCare.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace AssetCare.Common
{
    public class CalloutSharedFunctions
    {
        private IBuildingService _buildingService;
        private ICalloutService _calloutService;
        private IAgencyService _agencyService;
        private IPropertyService _propertyService;
        public CalloutSharedFunctions(ICalloutService calloutService, IAgencyService agencyService,
            IBuildingService buildingService, IPropertyService propertyService)
        {
            _buildingService = buildingService;
            _agencyService = agencyService;
            _calloutService = calloutService;
            _propertyService = propertyService;
        }
        public void SendCalloutDiscriptionAlertEmail(CallOutAddEditViewModel model, UserViewModel currentUser, int agencyId)
        {
            string toEmail = model.SchoolEmail;
            string fromEmail = ConfigurationManager.AppSettings["AssetCareSettings:EmailAutoCC"];
            string ccEmail = ConfigurationManager.AppSettings["AssetCareSettings:AsbestosCopyEmail"];
            string ircmEmail = ConfigurationManager.AppSettings["AssetCareSettings:DETMFUEmail"];
            string detectedKeywords = model.EmailAlertTypes;
            bool isMould = detectedKeywords.ToLower().Contains("mould");

            if (!String.IsNullOrEmpty(detectedKeywords))
            {                
                var callLogUrgency = _calloutService.GetFinanceUrgency().Where(x => x.CallLogFinanceUrgencyID == model.CallLogFinanceUrgencyID).FirstOrDefault();
                string callOutId = model.AgencySequenceID.HasValue ? model.AgencySequenceID.ToString() : "NA";
                
                //get agency contract
                Agency agencyDetails = _agencyService.GetById(model.AgencyID);

                if (agencyDetails != null)
                {
                    toEmail = toEmail + ";" + agencyDetails.ContactEmail + ";" + agencyDetails.RegionalAMUDirectorsEmails + ";" + agencyDetails.AssetServicesManagersEmails;
                    ccEmail = string.IsNullOrEmpty(ccEmail) ? agencyDetails.AlertCCEmails : ccEmail + "," + agencyDetails.AlertCCEmails;
                }
                //if(isMould) Ticket: 5905 temporary stop sending email to ManagedFund
                //{
                //    toEmail = toEmail + ";" + ircmEmail;
                //}

                Dictionary<string, string> data = GetEmailData(model, currentUser);
                EmailHelper emailHelper = new EmailHelper();
                string subject = string.Format("ALERT {0} {1} {2} Contract {3} Call ID: {4}", (model.CallLogStatusID == 2 ? "CANCELLED" : ""), detectedKeywords.ToUpper(), callLogUrgency?.CallLogFinanceUrgencyDesc, model.AgencyID, callOutId);
                data.Add("KeyWord", detectedKeywords.ToUpper());
                 
                emailHelper.SendMail(toEmail, data, subject, "CallLogAlertEmailContent.html", cc: string.Format("{0},{1}", fromEmail, ccEmail.TrimEnd(',')));

            }
        }
        public void SendEmail(CallOutAddEditViewModel model, UserViewModel currentUser)
        {
            string toEmail = string.Empty;
            string sinswEmail = ConfigurationManager.AppSettings["AssetCareSettings:EmailAutoCC"].ToString();
            //if (model.EmailToSchoolAndEmc)// it was a chkbox, if ticked. Need to know at what point we need to send this email
            //{
            toEmail = model.SchoolEmail + ";" + currentUser.Email;
            //get agency contract
            Agency agencyDetails = _agencyService.GetById(model.AgencyID);
             
            if (agencyDetails != null)
            {
                toEmail = toEmail + ";" + agencyDetails.ContactEmail;
            }


            var callLogUrgency = _calloutService.GetFinanceUrgency().Where(x => x.CallLogFinanceUrgencyID == model.CallLogFinanceUrgencyID).FirstOrDefault();
            string callOutId = model.AgencySequenceID.HasValue ? model.AgencySequenceID.ToString() : "NA";

            Dictionary<string, string> data = GetEmailData(model, currentUser);

            EmailHelper emailHelper = new EmailHelper();

            string subject = "New Callout ID "+ model.CallOutId + " is raised for "+ model.SchoolName;
            
            emailHelper.SendMail(toEmail, data, subject, "CallLogEmailContent.html", cc: sinswEmail);
            //}


        }

        public void SendEmailToContractor(CallOutAddEditViewModel model, UserViewModel currentUser, int contractId)
        {
            string toEmail = string.Empty;
            string sinswEmail = ConfigurationManager.AppSettings["AssetCareSettings:EmailAutoCC"].ToString();
            //if (model.EmailToSchoolAndEmc)// it was a chkbox, if ticked. Need to know at what point we need to send this email
            //{
            toEmail = model.SchoolEmail + ";" + currentUser.Email;
            //get agency contract           
            Agency agencyDetails = _agencyService.GetById(model.AgencyID);
            if (agencyDetails != null)
            {
                toEmail = toEmail + ";" + agencyDetails.ContactEmail;
            }
                      

            var callLogUrgency = _calloutService.GetFinanceUrgency().Where(x => x.CallLogFinanceUrgencyID == model.CallLogFinanceUrgencyID).FirstOrDefault();
            string callOutId = model.AgencySequenceID.HasValue ? model.AgencySequenceID.ToString() : "NA";

            Dictionary<string, string> data = GetEmailData(model, currentUser);
            SubContractor sub = _calloutService.GetSubContractortails(contractId);
            if (sub != null)
            {
                toEmail = toEmail + ";" + sub.Email;
                data.Add("LocalContractor", sub.Name);
            }

            
            EmailHelper emailHelper = new EmailHelper();


            string subject = "Local Contractor has been assigned for Work Order ID: " +model.WorkerId;
            emailHelper.SendMail(toEmail, data, subject, "CallLogWOAssigned.html", cc: sinswEmail);
          


        }

        /// <summary>
        /// If Mould is detected 100 EMU EUR needs to be ticked
        /// To avoid hard coding, use the database table field KeywordType
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public void SetAMUEURBasedOnCallLogKeywords(CallOutAddEditViewModel model)
        {
            List<EmailKeyword> keywords = _calloutService.GetEmailKeywords().Where(x => x.KeywordTypeID == 1).ToList();
            foreach (EmailKeyword keyword in keywords)
            {
                if (model.CallLogDesc.Contains(keyword.Keyword))
                {
                    model.AMUEUR = true;
                    return;
                }
            }


        }

        public CallOutAddEditViewModel ConvertToFMCallOut(PSMCalloutAddEditViewModel psmDefect)
        {
            CallOutAddEditViewModel callout = new CallOutAddEditViewModel()
            {
                AgencyID = psmDefect.AgencyId,
                CallDate = psmDefect.DefectCallDate,
                CallLogTime = psmDefect.DefectCallTime,
                CallLogDesc = psmDefect.CallLogDesc,
                CallTakenBy = psmDefect.RequestedBy,
                PropertyID = psmDefect.PropertyId,
                RequestedBy = psmDefect.RequestedBy,
                ElementId = psmDefect.ElementId,
                JobBuildingIDs = psmDefect.JobBuildingIDs,
                Comments = psmDefect.CallComments,
                RequesterPosition = psmDefect.RequestorPosition,
                RoomLocation = psmDefect.RoomLocation,
                RoomNumber = psmDefect.RoomNumber,
                CallLogFinanceUrgencyID = psmDefect.CallLogFinanceUrgencyId,
                CallLogStatusID = psmDefect.CallLogStatusId,
                WorkCategoryID = psmDefect.WorkCategoryId,
                WorkTypeId = psmDefect.WorkTypeId,
                SelectedSchWorkCategoryId = psmDefect.WorkCategoryId,
                SchoolEmail = psmDefect.SchoolEmail,
                EmailToSchoolAndEmc = true, //Email should always go when callout created first time
                Area = psmDefect.Area

            };

            Property school = _propertyService.GetById(psmDefect.PropertyId);
            if(school != null )
            {
                if (!string.IsNullOrEmpty(school.ProjectDirectorEmail))
                {
                    callout.SchoolEmail = string.Format("{0};{1}", callout.SchoolEmail, school.ProjectDirectorEmail);
                }

                if (!string.IsNullOrEmpty(school.ProjectManagerEmail))
                {
                    callout.SchoolEmail = string.Format("{0};{1}", callout.SchoolEmail, school.ProjectManagerEmail);
                }
            }            

            return callout;
        }

        public void SendDefectCalloutEmail(PSMCalloutAddEditViewModel model, UserViewModel currentUser)
        {
            string toEmail = string.Empty;
            string sinswEmail = ConfigurationManager.AppSettings["AssetCareSettings:EmailAutoCC"].ToString();
            if (model.EmailToSchoolAndEmc)
            {
                toEmail = model.SchoolEmail + ";" + currentUser.Email;
                Property school = _propertyService.GetById(model.PropertyId);
                if (school != null)
                {
                    if (!string.IsNullOrEmpty(school.ProjectDirectorEmail))
                    {
                        toEmail = string.Format("{0};{1}", toEmail, school.ProjectDirectorEmail);
                    }

                    if (!string.IsNullOrEmpty(school.ProjectManagerEmail))
                    {
                        toEmail = string.Format("{0};{1}", toEmail, school.ProjectManagerEmail);
                    }
                }


                var callLogUrgency = _calloutService.GetFinanceUrgency().Where(x => x.CallLogFinanceUrgencyID == model.CallLogFinanceUrgencyId).FirstOrDefault();
                string callOutId = model.AgencySequenceId.HasValue ? model.AgencySequenceId.ToString() : "NA";

                Dictionary<string, string> data = GetEmailData(model, currentUser);

                EmailHelper emailHelper = new EmailHelper();

                string subject = callLogUrgency.CallLogFinanceUrgencyDesc + "Job - Contract; " + model.AgencyId + "; FMWEB; - Defect Call ID;" + callOutId;
                emailHelper.SendMail(toEmail, data, subject, "DefectCallLogEmailContent.html", cc: sinswEmail);
            }
        }

        private Dictionary<string, string> GetEmailData(PSMCalloutAddEditViewModel model, UserViewModel currentUser)
        {
            var callLogUrgency = _calloutService.GetFinanceUrgency().Where(x => x.CallLogFinanceUrgencyID == model.CallLogFinanceUrgencyId).FirstOrDefault();
            //var schoolLogUrgency = _calloutService.GetFinanceUrgency().Where(x => x.CallLogFinanceUrgencyID == model).FirstOrDefault();
            Dictionary<string, string> data = new Dictionary<string, string>();
            string callOutId = model.AgencySequenceId.HasValue ? model.AgencySequenceId.ToString() : "NA";
            data.Add("CallOutId", callOutId);
            data.Add("AgencyId", model.AgencyId.ToString());
            data.Add("RequestedBy", currentUser.FullName);
            data.Add("SchoolName", model.SchoolName);
            data.Add("Phone", model.Phone);

            //get agency contract           
            Agency agencyDetails = _agencyService.GetById(model.AgencyId);
            if (agencyDetails != null)
            {
                data.Add("AgencyEmail", agencyDetails.ContactEmail);
            }

            if ((System.Web.HttpContext.Current.Session["AgencyDetails"]) != null)
            {
                data.Add("Region", ((KeyValuePair<int, string>)HttpContext.Current.Session["AgencyDetails"]).Value);
            }
            //get buildings list
            List<string> buildingList = new List<string>();
            foreach (int buildingId in model.JobBuildingIDs)
            {

                var b = _buildingService.GetByProperty(model.PropertyId)
                    .Where(x => x.BuildingID == buildingId)
                    .FirstOrDefault();
                if (b != null)
                {
                    buildingList.Add("(" + b.BuildingNo + ") " + b.BuildingDesc);
                }

            }


            data.Add("BuildingList", String.Join(",", buildingList));
            data.Add("Rooms", model.RoomNumber);
            data.Add("RoomLocation", model.RoomLocation);
            data.Add("Area", model.Area);

            data.Add("SchoolEmail", model.SchoolEmail);

            string workCategory = _calloutService.GetAllWorkCategories()
                .Where(x => x.WorkCategoryId == model.WorkCategoryId)
                .FirstOrDefault().Description;

            data.Add("WorkCategoryDesc", workCategory);

            data.Add("CallDate", model.DefectCallDate.Value.ToString("dd-MM-yyyy"));
            data.Add("CallTime", model.DefectCallDate.Value.ToString("HH:mm tt"));
            data.Add("ScopeOfWork", model.CallComments);
            data.Add("JobComments", model.CallLogDesc);
            if (callLogUrgency != null)
            {
                data.Add("CallResponseTime", callLogUrgency.CallLogFinanceUrgencyDesc);
            }

            var request = HttpContext.Current.Request;

            data.Add("Link", string.Format("{0}://{1}{2}?id={3}", request.Url.Scheme, request.Url.Authority, request.ApplicationPath + "/Defect/DefectDetail", model.AgencySequenceId));
            return data;
        }

        private Dictionary<string, string> GetEmailData(CallOutAddEditViewModel model, UserViewModel currentUser)
        {
            var callLogUrgency = _calloutService.GetFinanceUrgency().Where(x => x.CallLogFinanceUrgencyID == model.CallLogFinanceUrgencyID).FirstOrDefault();
            var schoolLogUrgency = _calloutService.GetFinanceUrgency().Where(x => x.CallLogFinanceUrgencyID == model.SchoolUrgencyID).FirstOrDefault();
            Dictionary<string, string> data = new Dictionary<string, string>();
            string callOutId = model.AgencySequenceID.HasValue ? model.AgencySequenceID.ToString() : "NA";
            data.Add("CallOutId", callOutId);
            data.Add("AgencyId", model.AgencyID.ToString());
            data.Add("RequestedBy", currentUser.FullName);
            data.Add("SchoolName", model.SchoolName);
            data.Add("Phone", model.Phone);

            //get agency contract
            AgencyContractor agencyContractor = _calloutService.GetAgencyContractor(model.AgencyID);
            Agency agencyDetails = _agencyService.GetById(model.AgencyID);
            if (agencyContractor != null)
            {               
                data.Add("Agency", agencyContractor.ContractorName);
            }

            if(agencyDetails != null)
            {
                data.Add("AgencyEmail", agencyDetails.ContactEmail);
            }

            if ((System.Web.HttpContext.Current.Session["AgencyDetails"]) != null)
            {
                data.Add("Region", ((KeyValuePair<int, string>)HttpContext.Current.Session["AgencyDetails"]).Value);
            }
            //get buildings list
            List<string> buildingList = new List<string>();
            foreach (int buildingId in model.JobBuildingIDs)
            {

                var b = _buildingService.GetByProperty(model.PropertyID)
                    .Where(x => x.BuildingID == buildingId)
                    .FirstOrDefault();
                if (b != null)
                {
                    buildingList.Add("(" + b.BuildingNo + ") " + b.BuildingDesc);
                }

            }


            data.Add("BuildingList", String.Join(",", buildingList));
            data.Add("Rooms", model.RoomNumber);
            data.Add("RoomLocation", model.RoomLocation);
            data.Add("Area", model.Area);

            data.Add("SchoolEmail", model.SchoolEmail);
            string sCategory = string.Empty;
            switch (model.CallLogTypeID)
            {
                case 1:
                    sCategory = "Reactive";
                    break;
                case 2:
                    sCategory = "Insurance";
                    break;
                case 3:
                    sCategory = "Defect";
                    break;
                default:
                    sCategory = "Reactive";
                    break;
            }

            string workCategory = _calloutService.GetAllWorkCategories()
                .Where(x => x.WorkCategoryId == model.WorkCategoryID)
                .FirstOrDefault().Description;

            data.Add("WorkCategoryDesc", sCategory);

            data.Add("CallDate", model.CallDate.Value.ToString("dd-MM-yyyy"));
            data.Add("CallTime", model.CallDate.Value.ToString("HH:mm tt"));
            data.Add("ScopeOfWork", model.Comments);
            data.Add("JobComments", model.CallLogDesc);
            data.Add("WorkOrderComments", model.WorkOrderComments);
            data.Add("WorkOrderId", model.WorkerId.HasValue ? model.WorkerId.ToString() : "" );

            if (callLogUrgency != null)
            {
                data.Add("CallResponseTime", callLogUrgency.CallLogFinanceUrgencyDesc);
            }else if (schoolLogUrgency != null)
            {
                data.Add("CallResponseTime", schoolLogUrgency.CallLogFinanceUrgencyDesc);
            }

            var request = HttpContext.Current.Request; //ControllerContext.RequestContext.HttpContext.Request;

            data.Add("Link", string.Format("{0}://{1}{2}?id={3}", request.Url.Scheme, request.Url.Authority, request.ApplicationPath + "/CallOut/Detail", model.CallLogFITSID));
            return data;
        }

        public static double GetTotalEstimationThresholdValue()
        {
            var totalEstimationThresholdText = System.Configuration.ConfigurationManager.AppSettings["AssetCareSettings:TotalEstimationThreshold"];
            totalEstimationThresholdText = totalEstimationThresholdText ?? "20K";
            bool hasK = (totalEstimationThresholdText.IndexOf("K") >= 0);
            var totalEstimationThreshold = Convert.ToDouble(totalEstimationThresholdText.ToUpper().Replace("K", ""));
            var K = 1000;
            return hasK ? totalEstimationThreshold * K : totalEstimationThreshold;
        }
        public static string GetTotalEstimationThresholdText()
        {
            var totalEstimationThresholdText = System.Configuration.ConfigurationManager.AppSettings["AssetCareSettings:TotalEstimationThreshold"];
            return totalEstimationThresholdText?? "20K";
        }

        public void SendDefectCreatedEmailToPM(CallOutAddEditViewModel model, UserViewModel currentUser)
        {
            string sinswEmail = ConfigurationManager.AppSettings["AssetCareSettings:EmailAutoCC"].ToString();

            string toEmail = model.SchoolEmail + ";" + currentUser.Email;
            Property school = _propertyService.GetById(model.PropertyID);
            if (school != null)
            {
                if (!string.IsNullOrEmpty(school.ProjectDirectorEmail))
                {
                    toEmail = string.Format("{0};{1}", toEmail, school.ProjectDirectorEmail);
                }

                if (!string.IsNullOrEmpty(school.ProjectManagerEmail))
                {
                    toEmail = string.Format("{0};{1}", toEmail, school.ProjectManagerEmail);
                }
            }

            Dictionary<string, string> data = GetEmailData(model, currentUser);
            string subject = "Defect ID " + model.AgencySequenceID + " is raised for " + model.SchoolName;
            new EmailHelper().SendMail(toEmail, data, subject, "DefectCalloutEmailContent.html", cc: sinswEmail);
        }
    }
}