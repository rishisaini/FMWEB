using AssetCare.Common;
using AssetCare.Controllers;
using AssetCare.Helpers;
using AssetCare.Model.Common;
using AssetCare.Model.Models;
using AssetCare.Model.ViewModels;
using AssetCare.Service;
using AssetCare.Service.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NLog;
using org.apache.zookeeper.data;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Constants = AssetCare.Common.Constants;

namespace AssetCare.Areas.FMWorks.Controllers
{
    [AuthorizeOverride(Roles = "ADMIN,SUPPORT,CONTRACTOR,APIStandard,APIPriemium,PROJECTMNGR,PROJECTDIR,SCHOOL,SCHOOLMGR,DETAMU,MFUMGR,MFUOFFICER,MFUMGR,MFUOFFICER,GROUPLEADER,SRGRPLEADER,DIRAM,EDAM,AMUOFFICER,DETAMCU,AMCUOFFICE,MFUMGR,MFUOFFICER,SUPERREP,CALLCENTERSTAFF,LOCALCONTRACTOR,CALLCENTERSTAFF,GUEST,DOEUSER,SINSW")]
    public class WorkOrderController : BaseController
    {

        private IPropertyService _propertyService;
        private IBuildingService _buildingService;
        private ICalloutService _calloutService;
        private IJobService _jobService;
        private IAgencyService _agencyService;
        private IUserService _userService;
        private ICreateScheduleService _createScheduleService;
        private readonly IClaimService _claimService;
        private readonly ISystemSettingService _systemSettingService;
        private IIRCMService _ircmService;
        private readonly string cachingKey = "CreateScheduleReferenceData";
        private Dictionary<string, string> _titles = new Dictionary<string, string>();
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        public WorkOrderController(ICreateScheduleService createScheduleService, IJobService jobService, IUserService userService, IBuildingService buildingService, IPropertyService propertyService,
            ICalloutService calloutService, IAgencyService agencyService, IClaimService claimService, IBaseService baseService, ISystemSettingService systemSettingService,IIRCMService ircmService) : base(baseService)
        {
            _jobService = jobService;
            _baseService = baseService;
            _propertyService = propertyService;
            _calloutService = calloutService;
            _buildingService = buildingService;
            _agencyService = agencyService;
            _userService = userService;
            _createScheduleService = createScheduleService;
            _claimService = claimService;
            _systemSettingService = systemSettingService;
            _ircmService = ircmService;
        }
        [HttpPost]
        public PartialViewResult QuickHelp()
        {
            return PartialView("QuickHelp/_ReactiveWorkOrderDetail");
        }

        [HttpPost]
        public PartialViewResult QuickHelpIndex()
        {
            switch (CurrentUser.RoleCode)
            {
                case "ADMIN":
                    return PartialView("QuickHelp/_ReactiveMaintenanceProcess");
                case "DIRAM":
                    return PartialView("QuickHelp/_ReactiveMaintenanceProcess");
                case "AMUOFFICER":
                    return PartialView("QuickHelp/_ReactiveMaintenanceProcess");
                case "AMCUOFFICE":
                    return PartialView("QuickHelp/_ReactiveMaintenanceProcess");
                case "DETAMU":
                    return PartialView("QuickHelp/_ReactiveMaintenanceProcess");
                case "EDAM":
                    return PartialView("QuickHelp/_ReactiveMaintenanceProcess");
                case "GROUPLEADER":
                    return PartialView("QuickHelp/_ReactiveMaintenanceProcess");
                case "SRGRPLEADER":
                    return PartialView("QuickHelp/_ReactiveMaintenanceProcess");
                case "NETDETAMU":
                case "NETOFFICER":
                case "DETAMCU":
                    return PartialView("QuickHelp/_ReactiveMaintenanceProcess");
                case "CONTRACTOR":
                case "APIStandard":
                case "APIPriemium":
                    return PartialView("QuickHelp/_ReactiveMaintenanceProcess");
                case "CALLCENTERSTAFF":
                case "LOCALCONTRACTOR":
                    return PartialView("QuickHelp/_ReactiveMaintenanceProcess");
                case "SUPPORT":
                    return PartialView("QuickHelp/_ReactiveMaintenanceProcess");

                default:
                    return PartialView("_FileNotFoundError");
            }
        }

        protected override void Initialize(RequestContext requestContext)
        {
            try
            {
                base.Initialize(requestContext);
                if (CurrentUser == null) return;
                List<int> userPropertyIds = CurrentUser.UserPropertyIds;
                List<SelectListItem> wcNew = _calloutService.GetWorkCategory().Select(x => new SelectListItem()
                {
                    Text = String.Format("{0} - {1}",
                    x.Code, x.Description),
                    Value = x.WorkCategoryId.ToString()
                }).OrderBy(x => x.Text).ToList();
                ViewData["WorkCategoryList"] = wcNew;
                int agencyId = GetAgencyId();
                var currentAgency = _agencyService.GetById(agencyId);
                //Load Equipement Type
                List<SelectListItem> assemblyEquipmentTypes = _calloutService.GetEquipmentTypeAssemblies()
                                      .Select(x => new SelectListItem() { Text = x.EquipmentTypeBSEMAssemblyDesc, Value = x.EquipmentTypeBSEMAssemblyID.ToString() }).OrderBy(a => a.Text).ToList();
                ViewData["AssemblyEquipmentTypes"] = assemblyEquipmentTypes;


                ViewData["JobStatus"] = _jobService.GetServiceReportStatus()
                    .Where(y => y.ShowInSRptList == true)
                    .OrderBy(x => x.SortOrder)
                    .Select(x => new SelectListItem { Text = x.ServiceReportStatusDesc, Value = x.ServiceReportStatusID.ToString() })
                    .ToList();


                //load callLogUrgency
                List<SelectListItem> callLogUrgency = _calloutService.GetFinanceUrgency()
                                        .Select(x => new SelectListItem() { Text = x.CallLogFinanceUrgencyDesc, Value = x.CallLogFinanceUrgencyID.ToString() }).ToList();
                ViewData["CallLogUrgency"] = callLogUrgency;

                var userDetails = _userService.GetByUserCode(User.UserCode(), AgencyId.HasValue ? AgencyId.Value : 0);
                List<SelectListItem> schoolList = _propertyService.GetSchoolLookupWithUserRestrictions(agencyId, userDetails.UserPropertyIds).Select(x => new SelectListItem() { Value = x.Key.ToString(), Text = x.Value }).ToList();

                ViewData["SchoolsList"] = schoolList;

                ViewData["Elements"] = _jobService.GetElementList().Where(e => e.ElementID != 0).Select(x => new SelectListItem() { Value = x.ElementID.ToString(), Text = x.ElementDesc }).ToList();


                ViewData["WorkTypeList"] = _jobService.GetWorkTypes();
                ViewData["WorkTypes"] = _jobService.GetWorkTypes().Select(x => new SelectListItem() { Value = x.WorkTypeID.ToString(), Text = x.Description }).ToList();

                ViewData["SubContractors"] = _calloutService.GetSubContractorsByAgency(GetAgencyId()).Select(x => new SelectListItem() { Text = x.Name, Value = x.ID.ToString() });
                ViewData["SubContractorBusinessType"] = _calloutService.GetSubContractorBusinessTypes().Select(x => new SelectListItem() { Text = String.Format("{0}", x.Description), Value = x.BusinessTypeId.ToString() }).OrderBy(x => x.Text).ToList();

                ViewData["ClaimApprovalStatus"] = _jobService.GetReactiveClaimApprovalStatusFilters(currentAgency.AgencyTypeId).Select(x => new SelectListItem() { Text = x.ApprovalStatusFilterDesc, Value = x.ApprovalStatusFilterID.ToString() });

                ViewData["ClaimeAmount"] = new List<SelectListItem>
            {
                 new SelectListItem() { Text = "Over 5000", Value = "1" },
                 new SelectListItem() { Text ="Less than or equal 5000", Value = "0" }
            };

                ViewData["IRCMClaimStatuses"] = _jobService.GetIRCMClaimStatuses().Select(x => new SelectListItem() { Text = x.IRCMClaimStatusDesc, Value = x.IRCMClaimStatusID.ToString() });

                ViewData["ReimbursementStatuses"] = _jobService.GetReimbursementStatuses().Select(x => new SelectListItem() { Text = x.ReimbursementStatusDesc, Value = x.ReimbursementStatusID.ToString() });

                ViewData["AssessorStatuses"] = _jobService.GetAssessorStatuses().Select(x => new SelectListItem() { Text = x.AssessorStatusDesc, Value = x.AssessorStatusID.ToString() });
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Initializer: {ex.Message} \n {ex.StackTrace}");
                throw;
            }

        }

        private void initializeSupportDocumentTypes(int workCategoryId)
        {
            ViewData["SupportDocumentType"] = _jobService.GetSupportDocumentTypes(workCategoryId)
                .Select(x => new SelectListItem { Text = x.SupportDocumentTypeDesc, Value = x.SupportDocumentTypeID.ToString() })
               .OrderBy(x => x.Text)
              .ToList(); ;
        }


        public ActionResult Index(bool? showWOPaidOnly)
        {

            //ViewBag.Title = "Callout List";
            JobListViewModel viewModel = new JobListViewModel();
            viewModel.CurrentRoleID = CurrentUser.RoleID;
            viewModel.AgencyId = GetAgencyId();
            viewModel.SchoolId = (User.IsSchoolUser() ? CurrentUser.DefaultPropertyId : null);
            viewModel.CanViewCallOut = (CurrentUser.RoleID == "C" || CurrentUser.RoleID == "CS" || _jobService.IsAMUUser(CurrentUser) || _jobService.IsMFUUser(CurrentUser) ||
                CurrentUser.RoleID == "GL" || CurrentUser.RoleID == "SL" || CurrentUser.RoleID == "AM" || CurrentUser.RoleID == "EM" || CurrentUser.RoleID == "A");
            viewModel.ShowWOPaidOnly = (showWOPaidOnly == true);
            return View(viewModel);
        }

        public ActionResult SchoolJobs(int? propertyId, int? workCategoryId)
        {
            ViewBag.Title = "Callout List";
            if (workCategoryId.HasValue)
            {
                var workCategoryIdText = workCategoryId.Value.ToString();
                string title = null;
                ViewBag.Title = _titles.TryGetValue(workCategoryIdText, out title) ? title : ViewBag.Title;
                initializeSupportDocumentTypes(workCategoryId.Value);
            }
            JobListViewModel viewModel = new JobListViewModel() { AgencyId = GetAgencyId(), SchoolId = propertyId, WorkCategoryId = workCategoryId, CurrentRoleID = CurrentUser.RoleID };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryTokenOnAllPosts]
        public ActionResult Index(Object data)
        {
            return RedirectToAction("Detail");
        }
        public ActionResult Detail(int? callOutId, int? wrkId, string mode, string tab, string sig)
        {
            JobDetails details = new JobDetails();

            try
            {
                int agencyId = GetAgencyId();
                
                details = _jobService.GetCallOutDetails(agencyId, callOutId, wrkId, CurrentUser);
                details.DetailsTab.LoggedInUser = CurrentUser.RoleID;

                if (!TamperHelper.ValidateSignature(wrkId.Value, sig))
                {
                    Toast.ShowError(TempData, Constants.NoAccessToRecordViaQuerystringModification);
                    return RedirectToAction("Index");
                }

                if (!CanAccessWorkOrder(details))
                {
                    Toast.ShowError(TempData, Constants.NoAccessToRecord);
                    return RedirectToAction("Index");
                }
                if (!IsInCurrentRegion(details.AgencyId))
                {
                    Toast.ShowError(TempData, Constants.SwitchRegionFirst);
                    return RedirectToAction("Index");
                }

                if (_agencyService.IsAMURegion(agencyId))
                {
                    details.IsAMURegion = true;
                }

                PopulateWorkCategoryListData(details);
                details.UserCapabilities = GetUserCapabilities(details);
                SetValidationAttributes(details);
                //if all three fields are disabled, then disable validation

                //details.SchoolSitePlanPDFPath = new SharedFunctions().GetSchoolSitePlanPDFPath((int)details.SchoolId);
                //details.AsbestosRegisterPDFPath = new SharedFunctions().GetAsbestosRegisterPDFPath((int)details.SchoolId);

                var callbackUrl = Request.QueryString["callbackUrl"];
                if (!string.IsNullOrEmpty(callbackUrl))
                {
                    details.CallbackUrl = callbackUrl;
                    details.defaultTab = tab;
                }

                initializeSupportDocumentTypes(details.WorkCategoryId.Value);
                SetSupportDocumentDownloadLink(details.SupportDocuments);
                SetSupportDocumentDownloadLink(details.SupportQuoteDocuments);
                SetSupportDocumentDownloadLink(details.SupportDocumentsCallout);
                //if (details.AgencySequenceId.HasValue && details.AgencySequenceId > 0)
                //{
                //    var property = _propertyService.GetById(details.PropertyId);
                //    if ((property != null) && property.SchoolId.HasValue)
                //        GenerateQRCode("reactive", details.AgencySequenceId.Value, property.SchoolId.Value);
                //}
                    
                if (details.DetailsTab.ServiceReportStatusId > 10)
                {
                    ViewData["JobStatus"] = _jobService.GetServiceReportStatus()
                   .OrderBy(x => x.SortOrder)
                   .Select(x => new SelectListItem { Text = x.ServiceReportStatusDesc, Value = x.ServiceReportStatusID.ToString() })
                   .ToList();
                }

                TempData["JOB_USERCAPABILITIES"] = details.UserCapabilities;
                details.CurrentRoleID = CurrentUser.RoleID;

                ViewData["tradieListLockBeforeHours"] =  _systemSettingService.GetSettingByKey("tradieListLockBeforeHours")?.Value;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Detail: {ex.Message} \n {ex.StackTrace}");
                throw;
            }

            return View(details);
        }

        private void GenerateQRCode(string orderType, int orderNo, int schoolNo)
        {
            var contractType = "FM"; //FM for maintenance contract, CS for cleaning service contract
            var baseUrl = ConfigurationManager.AppSettings["AssetCareSettings:APIBaseUri"];
            //The URL for your HTML page that you want the QR code to link to
            string url = $"{baseUrl}api/visitorsbyorder/{contractType}/{orderType}/{orderNo}/{schoolNo}"; // Replace with your actual URL
            ViewBag.QRCodeUrl = QRCodeManager.Generate(url);
        }

        private bool CanAccessWorkOrder(JobDetails details)
        {
            return CanAccessProperty(new PropertyKey() { AgencyID = details.AgencyId, PropertyID = details.PropertyId });
        }

        private void SetValidationAttributes(JobDetails details)
        {
            if (details.UserCapabilities.DetailSection != null &&
               !details.UserCapabilities.DetailSection.Contains("EstimatedCompletionDate") &&
               !details.UserCapabilities.DetailSection.Contains("FirstVisitDate") &&
               !details.UserCapabilities.DetailSection.Contains("CompletedDate"))
            {
                details.DetailsTab.ValidateCompFirstDates = false;
            }
            else
            {
                details.DetailsTab.ValidateCompFirstDates = true;
            }


            //validate dates only if Estimate is finalised and Date fields are visible
            if (!details.DisableField("detailsection", "EstimatedCompletionDate") &&
               !details.DisableField("detailsection", "FirstVisitDate") && details.Estimate.EstimateFinalised)
            {
                details.DetailsTab.ValidateDatesOnEstimateFinalised = true;
            }
            else
            {
                details.DetailsTab.ValidateDatesOnEstimateFinalised = false;
            }

            //Remove WO Status
            if (details.DetailsTab.ServiceReportStatusId == 2)
            {
                //If work order is accepted then remove Rejected
                List<SelectListItem> statuses = (List<SelectListItem>)ViewData["JobStatus"];
                SelectListItem rejectedWOStatus = statuses.Where(x => x.Value == "3").FirstOrDefault();
                SelectListItem assignedWOStatus = statuses.Where(x => x.Value == "1").FirstOrDefault();
                if (rejectedWOStatus != null)
                {
                    statuses.Remove(rejectedWOStatus);

                }

                if (assignedWOStatus != null)
                {
                    statuses.Remove(assignedWOStatus);
                }

                ViewData["JobStatus"] = statuses;
            }



        }


        [HttpPost]
        [ValidateAntiForgeryTokenOnAllPosts]
        public ActionResult CreateWorkOrder(CreateWorkOrder model)
        {
            int serviceReportId = 0;
            model.LoggedInUser = CurrentUser.UserCode;
            int jobId = _jobService.CreateWOAsVariation(model, out serviceReportId);
            var estimationThreshold = CalloutSharedFunctions.GetTotalEstimationThresholdValue();
            CalloutSharedFunctions calloutSharedFunctions = new CalloutSharedFunctions(_calloutService, _agencyService, _buildingService, _propertyService);
            CallOutAddEditViewModel callOutAddEdit = _calloutService.GetCallOutDetails(model.WOCalloutId, model.WOAgencyId.Value, estimationThreshold);

            callOutAddEdit.JobId = jobId;
            callOutAddEdit.WorkerId = jobId;
            callOutAddEdit.WorkOrderComments = model.WOComments;
            calloutSharedFunctions.SendEmailToContractor(callOutAddEdit, CurrentUser, model.WOSubContractorId.Value);

            return RedirectToAction("Detail", new { wrkId = serviceReportId });

        }

        [HttpPost]
        [ValidateAntiForgeryTokenOnAllPosts]
        public ActionResult Detail(JobDetails model)
        {
            model.WorkItems = GetWorkItemsFromJob(model);

            model.UserCapabilities = TempData["JOB_USERCAPABILITIES"] != null ? (JobUserCapabilities)TempData["JOB_USERCAPABILITIES"] : new JobUserCapabilities();

            //if the Local contractor creat
            int jobId = 0;
            string saveType = !string.IsNullOrEmpty(model.SaveType) ? model.SaveType.ToLower() : "";
            //check whether it is an Admin function
            if (saveType == "unf" || saveType == "recl" || saveType == "60day" || saveType == "40day")
            {
                if (CurrentUser.RoleID == "A")
                {

                    _jobService.ResetJob(GetAgencyId(), model, CurrentUser);
                }
            }
            else
            {
                var estimationThreshold = CalloutSharedFunctions.GetTotalEstimationThresholdValue();
                var estimationThresholdText = CalloutSharedFunctions.GetTotalEstimationThresholdText();
                bool saved = _jobService.Save(model, CurrentUser, estimationThreshold, estimationThresholdText, out jobId);
            }



            //if it is just save, then stay in the same page
            if (saveType == "save" || saveType == "unf" || saveType == "recl" || saveType == "60day" || saveType == "40day")
            {
                return RedirectToAction("Detail", new { wrkId = model.ServiceReportId, callbackUrl = model.CallbackUrl, sig = TamperHelper.GenerateSignature(model.ServiceReportId.Value) });
            }
            else if (saveType == "saveandexit")
            {
                if (string.IsNullOrEmpty(model.CallbackUrl))
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    if (!Url.IsLocalUrl(model.CallbackUrl))
                        return RedirectToAction("Index");
                    return Redirect(model.CallbackUrl);
                }

            }
            else
            {
                return RedirectToAction("Index");
            }

        }

        public ActionResult Claims()
        {
            if (User.IsInRoles("GUEST"))
                return RedirectToAction("Index", "Home", new { area = "" });

            WorkOrderCertifyViewModel claimModel = new WorkOrderCertifyViewModel() { WorkOrderList = new List<WorkOrderCertifyListItem>(), AgencyId = GetAgencyId() };
            if (CurrentUser.RoleID == "LC")
            {
                if (CurrentUser.DefaultLocalContractorId.HasValue)
                {
                    ViewData["SubContractors"] = _calloutService.GetSubContractorsByAgency(GetAgencyId()).Where(y => y.ID == CurrentUser.DefaultLocalContractorId.Value).Select(d => new SelectListItem() { Text = d.Name, Value = d.ID.ToString() });
                }
                else
                {
                    ViewData["SubContractors"] = new SelectList(new List<SelectListItem>() { });
                }
            }
            claimModel.UserCapabilities = GetUserCapabilitiesForClaims(claimModel, false);
            var x = CurrentUser.DefaultLocalContractorId;

            if (_agencyService.IsAMURegion(claimModel.AgencyId))
            {
                claimModel.IsAMURegion = true;
            }
            claimModel.CurrentRoleID = CurrentUser.RoleID;
            return View(claimModel);
        }
        [HttpPost]
        [ValidateAntiForgeryTokenOnAllPosts]
        public ActionResult Claims(WorkOrderCertifyViewModel claimModel)
        {
            claimModel.AgencyId = GetAgencyId();
            claimModel.UserCapabilities = GetUserCapabilitiesForClaims(claimModel, false);

            return View(claimModel);
        }

        [AuthorizeAPIOverride(Roles = "ADMIN,SUPPORT,CONTRACTOR,DETAMU,GROUPLEADER,SRGRPLEADER,DIRAM,EDAM,AMUOFFICER,DETAMCU,AMCUOFFICE,MFUMGR,MFUOFFICER,CALLCENTERSTAFF,LOCALCONTRACTOR,DOEUSER,SINSW")]
        public ActionResult ClaimWODetails(int wrkId)
        {
            JobDetails details = new JobDetails();

            int agencyId = GetAgencyId();

            details = _jobService.GetCallOutDetails(agencyId, 0, wrkId, CurrentUser);
            var estimationThreshold = CalloutSharedFunctions.GetTotalEstimationThresholdValue();

            CallOutAddEditViewModel callOutAddEdit;
            if (details.CallLogFITSId > 0) {
                callOutAddEdit = _calloutService.GetCallOutDetails(details.CallLogFITSId, agencyId, estimationThreshold);
                details.IsTotalEstimationOverLimit = callOutAddEdit.IsTotalEstimationOverLimit;
                details.SchoolIncidentCauseDesc  = _ircmService.CurrentContext.SchoolIncidentCauseRepository.FirstOrDefault( x=> x.SchoolIncidentCauseID == callOutAddEdit.SchoolIncidentCauseId )?.SchoolIncidentCauseDesc;
            }

            initializeSupportDocumentTypes(details.WorkCategoryId.Value);
            SetSupportDocumentDownloadLink(details.SupportDocuments);
            SetSupportDocumentDownloadLink(details.SupportQuoteDocuments);
            SetSupportDocumentDownloadLink(details.SupportDocumentsCallout);

            return PartialView("_ClaimWODetails", details);

        }

        public ActionResult ClaimRCTI(int claimId)
        {
            //int claimId = 0;
            ReactiveRCTIReportViewModel reportViewModel = _claimService.GetReactiveRCTIData(claimId);
            List<ReactiveRCTILineItems> lineItems = new List<ReactiveRCTILineItems>();
            if (CurrentUser.RoleID == "LC" || CurrentUser.RoleID == "A" || _jobService.IsAMUUser(CurrentUser))
            {
                if (reportViewModel.SubContractorId == CurrentUser.DefaultLocalContractorId ||
                    _jobService.IsAMUUser(CurrentUser) ||
                     CurrentUser.RoleID == "A")
                {
                    ViewData["LineItems"] = _claimService.GetReactiveRCTILineItems(claimId);
                    return View(reportViewModel);
                }
                else
                {
                    reportViewModel = new ReactiveRCTIReportViewModel();

                }
            }

            return View(reportViewModel);

        }

        [AuthorizeOverride(Roles = "ADMIN,DETAMU,MFUMGR,MFUOFFICER,MFUMGR,MFUOFFICER,AMUOFFICER,DETAMCU,AMCUOFFICE,MFUMGR,MFUOFFICER,SUPPORT")]
        public ActionResult InsuranceClaims()
        {
            if (User.IsInRoles("GUEST,DOEUSER,SINSW"))
                return RedirectToAction("Index", "Home", new { area = "" });

            WorkOrderCertifyViewModel claimModel = new WorkOrderCertifyViewModel() { WorkOrderList = new List<WorkOrderCertifyListItem>(), AgencyId = GetAgencyId() };
            claimModel.UserCapabilities = GetUserCapabilitiesForClaims(claimModel, true);
            var x = CurrentUser.DefaultLocalContractorId;
            return View(claimModel);
        }

        [HttpPost]
        [ValidateAntiForgeryTokenOnAllPosts]
        public ActionResult InsuranceClaims(WorkOrderCertifyViewModel claimModel)
        {
            claimModel.AgencyId = GetAgencyId();
            claimModel.UserCapabilities = GetUserCapabilitiesForClaims(claimModel, true);

            return View(claimModel);
        }

        [HttpPost]
        [ValidateAntiForgeryTokenOnAllPosts]
        public JsonResult IsValidExpectedVisitDate(JobDetails details)
        {

            if (details.DetailsTab.ExpectedVisitDate != null && details.DetailsTab.CallLogDateVal != null && details.DetailsTab.ValidateCompFirstDates)
            {
                if (details.DetailsTab.ExpectedVisitDate.Value.Date < details.DetailsTab.CallLogDateVal.Date)
                {
                    return Json("Expected visit date should be greater than call out date");
                }
            }

            return Json(true);

        }


        [HttpPost]
        [ValidateAntiForgeryTokenOnAllPosts]
        public JsonResult IsValidWorkCompletedDate(JobDetails details)
        {

            if (details.DetailsTab.CompletedDate != null && details.DetailsTab.FirstVisitDate != null && details.DetailsTab.ValidateCompFirstDates)
            {
                if (details.DetailsTab.CompletedDate < details.DetailsTab.FirstVisitDate)
                {
                    return Json("Completed date should be greater than First Visit date");
                }
            }

            return Json(true);

        }

        [HttpPost]
        [ValidateAntiForgeryTokenOnAllPosts]
        public JsonResult IsValidResportStatus(JobDetails details)
        {

            if (details.DetailsTab.ServiceReportStatusId != null)
            {
                //ServiceReportStatus == 9 is Cancelled
                if (details.DetailsTab.ServiceReportStatusId == 9)
                {
                    if (CurrentUser.RoleID == "C" || CurrentUser.RoleID == "LC")
                    {
                        //if Contractor, if there is at least one approved claim OR Job is finalised then they can NOT cancel the job
                        if (details.DetailsTab.HasAtleastOneApprovedClaim || details.DetailsTab.ClaimFinalizedForJobStatusValidation)
                        {
                            return Json("You cannot change the Job Status to Cancel.");
                        }

                        return Json(true);

                    }

                    if (CurrentUser.RoleID == "A" || CurrentUser.RoleID == "S")
                    {
                        //For Admin if there is at least one approved claim then Job cannot be cancelled
                        if (details.DetailsTab.HasAtleastOneApprovedClaim)
                        {
                            return Json("You cannot change the Job Status to Cancel.");
                        }

                        return Json(true);
                    }

                    return Json("You cannot change the Job Status to Cancel.");

                }

                if (CurrentUser.RoleID == "C" || CurrentUser.RoleID == "LC" || CurrentUser.RoleID == "A")
                {
                    if (details.DetailsTab.ContractorReclaimedDateForJobStatusValidation != null)
                    {
                        if (details.DetailsTab.ClaimFinalizedForJobStatusValidation)
                        {
                            //if the Job is reclaimed, and ClaimIsFinalized, then Job status should be Completed and Reclaimed
                            if (details.DetailsTab.ServiceReportStatusId != 13)
                            {
                                return Json("Change the Job Status to 'Closed'");
                            }
                        }
                    }
                }

                //ServiceReportStatus == 9 is Cancelled
                if (details.DetailsTab.ServiceReportStatusId == 8)
                {
                    if (CurrentUser.RoleID == "C"
                        || CurrentUser.RoleID == "AO"
                        || CurrentUser.RoleID == "D"
                        || CurrentUser.RoleID == "GL"
                        || CurrentUser.RoleID == "SL"
                        || CurrentUser.RoleID == "AM"
                        || CurrentUser.RoleID == "EM"
                        || CurrentUser.RoleID == "P")
                    {
                        if (details.DetailsTab.SubContractorAccepted == null)
                        {
                            return Json("You cannot change the status to 'Complete'");
                        }
                    }
                    //CurrentUser.RoleID == "C" )
                }

                return Json(true);
            }

            return Json(true);

        }


        [ValidateAntiForgeryTokenOnAllPosts]
        [HttpPost]
        public JsonResult IsValidFirstVisitDate(JobDetails details)
        {
            //Removing First Visit Date Validation
            //if (details.DetailsTab.FirstVisitDate != null && details.DetailsTab.EstimatedCompletionDate != null && details.DetailsTab.ValidateCompFirstDates)
            //{
            //    if (details.DetailsTab.EstimatedCompletionDate < details.DetailsTab.FirstVisitDate)
            //    {
            //        return Json("First visit date should be less than Estimated Completion date");
            //    }
            //}

            if (details.DetailsTab.CallLogDateVal != null)
            {
                var calloutDateShort = Convert.ToDateTime(details.DetailsTab.CallLogDateVal.ToShortDateString());
                if (calloutDateShort > details.DetailsTab.FirstVisitDate)
                {
                    return Json("First visit date cannot be less than Callout date");
                }
            }

            if (details.DetailsTab.CompletedDate != null && details.DetailsTab.FirstVisitDate != null && details.DetailsTab.ValidateCompFirstDates)
            {
                if (details.DetailsTab.CompletedDate < details.DetailsTab.FirstVisitDate)
                {
                    return Json("First visit date should be less than Completion date");
                }
            }


            return Json(true);

        }

        [ValidateAntiForgeryTokenOnAllPosts]
        [HttpPost]
        public JsonResult IsValidEstimatedDate(JobDetails details)
        {
            //Removing First Visit Date Validation

            //if (details.DetailsTab.FirstVisitDate != null && details.DetailsTab.EstimatedCompletionDate != null && details.DetailsTab.ValidateCompFirstDates)
            //{
            //    if (details.DetailsTab.EstimatedCompletionDate < details.DetailsTab.FirstVisitDate)
            //    {
            //        return Json("Estimated Completion date should be greater than First Visit Date");
            //    }
            //}
            bool validationStatus1 = true;
            bool validationStatus2 = true;
            if (details.DetailsTab.FirstVisitDate.HasValue && details.DetailsTab.EstimatedCompletionDate.HasValue)
            {
                if (details.DetailsTab.EstimatedCompletionDate < details.DetailsTab.FirstVisitDate)
                {
                    validationStatus1 = false;
                }

                //validationStatus1 = false;

            }

            if (details.DetailsTab.ExpectedVisitDate.HasValue && details.DetailsTab.EstimatedCompletionDate.HasValue)
            {
                if (details.DetailsTab.EstimatedCompletionDate < details.DetailsTab.ExpectedVisitDate)
                {
                    validationStatus2 = false;
                }

                //validationStatus2 = false;


            }

            if (validationStatus1 == false)
            {
                return Json("Estimated Completion date should be greater than First Visit Date");
            }
            else if (validationStatus2 == false)
            {
                return Json("Estimated Completion date should be greater than Expected Visit Date");
            }
            else
            {
                return Json(true);
            }



        }


        private void SetSupportDocumentDownloadLink(List<SupportDocumentViewModel> SupportDocuments)
        {
            //reset download urls
            if (SupportDocuments != null)
            {
                foreach (var doc in SupportDocuments)
                {
                    if (!String.IsNullOrEmpty(doc.AttachmentLink))
                    {
                        doc.AttachmentLink = Url.Content("~/") + doc.AttachmentLink.Remove(0, 1);
                    }
                }
            }
        }

        private JobUserCapabilities GetUserCapabilitiesForClaims(WorkOrderCertifyViewModel model, bool forInsuranceClaims)
        {
            string mainSection = string.Empty;
            JobUserCapabilities userCapability = new JobUserCapabilities();
            if (CurrentUser != null)
            {
                if (CurrentUser.RoleID == "C")
                {
                    model.ApprovalType = "fmc";
                    mainSection = "SubContractorId,PropertyId,ApprovalStatus,fmcComment,fmcApprove,ApprovalToDate,ApprovalFromDate,ClaimedAmountFlag,btnSave";
                }
                else if (_jobService.IsAMUUser(CurrentUser) || CurrentUser.RoleID == "GL" || CurrentUser.RoleID == "SL" || CurrentUser.RoleID == "AM" || CurrentUser.RoleID == "EM" || CurrentUser.RoleID == "SP")
                {
                    model.ApprovalType = "amu";
                    mainSection = "SubContractorId,PropertyId,ApprovalStatus,asoComment,asoApprove,approveAmount,ApprovalToDate,ApprovalFromDate,ClaimedAmountFlag,btnSave";
                    if (CurrentUser.RoleID == "P")
                    {
                        mainSection = mainSection + ",btnSave";
                    }
                    // AMU agencies do not have FMC role, all FMC role are now assigned to AMU
                    if (_agencyService.IsAMURegion(GetAgencyId()) && _jobService.IsAMUUser(CurrentUser))
                    {
                        mainSection = mainSection + ",fmcComment,fmcApprove";
                    }
                }
                else if (_jobService.IsMFUUser(CurrentUser))
                {
                    if (forInsuranceClaims)
                    {
                        model.ApprovalType = "mfu";
                        mainSection = "SubContractorId,IRCMClaimStatusID,PropertyId,ReimbursementStatusID,AssessorStatusID,ApprovalToDate,ApprovalFromDate,btnSave";
                    }
                    else
                    {
                        model.ApprovalType = "amu";
                        mainSection = "SubContractorId,PropertyId,ApprovalStatus,asoComment,asoApprove,approveAmount,ApprovalToDate,ApprovalFromDate,ClaimedAmountFlag";
                    }
                }
                else if (CurrentUser.RoleID == "LC")
                {
                    mainSection = "SubContractorId,PropertyId,ApprovalStatus,ApprovalToDate,ApprovalFromDate,ClaimedAmountFlag";
                }
                else if (CurrentUser.RoleID == "A")
                {
                    model.ApprovalType = "fa";
                    mainSection = "IRCMClaimStatusID,PropertyId,ReimbursementStatusID,AssessorStatusID,SubContractorId,PropertyId,ApprovalStatus,asoComment,asoApprove,fmcComment,fmcApprove,approveAmount,ApprovalToDate,ApprovalFromDate,ClaimedAmountFlag,btnSave";

                }
                else if (CurrentUser.RoleID == "DE" || CurrentUser.RoleID == "SI")
                {
                    model.ApprovalType = "amu";
                    mainSection = "PropertyId,ReimbursementStatusID,AssessorStatusID,SubContractorId,PropertyId,ApprovalStatus,asoComment,asoApprove,fmcComment,fmcApprove,approveAmount,ApprovalToDate,ApprovalFromDate,ClaimedAmountFlag";
                }
            }

            userCapability.MainSection = mainSection;
            return userCapability;
        }
        private JobUserCapabilities GetUserCapabilities(JobDetails details)
        {
            //SerializableDictionary<string, string> editableFields = new SerializableDictionary<string, string>();
            string mainSection = string.Empty;
            string detailSection = string.Empty;
            string estimateSection = string.Empty;
            string costingSection = string.Empty;
            string recordSection = string.Empty;
            string claimSection = string.Empty;
            JobUserCapabilities userCapability = new JobUserCapabilities();
            bool isWorkItemsAmtZero = details.WorkItems.Any() && details.WorkItems.Sum(item => item.Amount) == 0;
            if (CurrentUser != null)
            {
                //if the claim is over 60 days then users cannot change anything
                //if (CurrentUser.RoleID != "A" && ((details.IsClaimOver60Days.HasValue && details.IsClaimOver60Days.Value && !_jobService.IsPWAOrAMUUser(CurrentUser)) || (details.JobNotReclaimedFor40Days.HasValue && details.JobNotReclaimedFor40Days.Value)))
                //{
                //    return userCapability;
                //}
                //else 
                if (CurrentUser.RoleID == "C")
                {

                    mainSection = "calloutLink"; //(details.Estimate.EstimateFinalised) ? "" : "IsInsuranceEvent,SchoolContribution";
                    detailSection = "ExpectedVisitDate,ExpectedFirstVisitTime,ServiceReportStatusId,FirstVisitDate,CompletedDate,Comments,FMCCommentDate,SubContractorId,TradieSelectList";

                    estimateSection = "";

                    costingSection = "workItemDelete";
                    recordSection = (details.AgencySequenceId.HasValue && details.AgencySequenceId.Value > 0) ?
                        "IssueDate,SelectedDocumentTypeId," : "";

                    if (!details.Estimate.EstimateFinalised)
                    {
                        if (details.DetailsTab.ServiceReportStatusId == null ||
                            (details.DetailsTab.ServiceReportStatusId.Value != 8 &&
                                  details.DetailsTab.ServiceReportStatusId.Value != 10))
                        {
                            detailSection = detailSection + ",EstimatedCompletionDate";
                            if (details.DetailsTab.SubContractorAccepted == null)
                            {
                                detailSection = detailSection + ",SubContractorAccepted";
                            }
                         }

                        //if (details.Estimate.LimitStatus == 2)
                        //{
                        //    estimateSection = "EstimateFinalised,EstimatedBy,EstimateDate,EstimateComments,EstimatedAmount";
                        //}

                        costingSection = "workItemDelete";
                    }
                    else
                    {
                        if (details.Estimate.EstimateFinalised && details.Estimate.EstimatedAmount > 5000)
                        {
                            detailSection = detailSection + ",SubContractorBusinessTypeId,WOSubContractorId,WOComments,raiseVariation";
                            //(details.Estimate.FMCApproved == null || details.Estimate.FMCApproved == false) && details.Estimate.AMUApproved == null) ||
                            //    (details.Estimate.AMUApproved == false && details.Estimate.FMCApproved != null)
                            // Estimate is finalised, but AMU has not approved yet 

                            if (details.Estimate.AMUApproved != true)
                            {
                                estimateSection = "FMCApproved,FMCApprovalComments,FMCApprovedBy,FMCApprovedDate,FMCRecommendedAction";
                            }
                        }

                        if (details.Estimate.AMUApproved == true)
                        {
                            //Estimate finalised, and FMC and AMU approved and a by the LC
                            estimateSection = "";
                            costingSection = "";
                            recordSection = "";
                            detailSection = "Comments,FMCCommentDate";
                        }
                    }

                    if (details.Claim.ClaimFinalized)
                    {
                        costingSection = "";
                        recordSection = "";
                        claimSection = isWorkItemsAmtZero ? "ClaimFinalized,ClaimFinalizedDate,ClaimedAmount" : ""; //if workItems amount is 0 enable edit
                        detailSection = isWorkItemsAmtZero ? "ServiceReportStatusId" : ""; //if workItems amount is 0 allow change of status
                    }

                    //if contractor reclaim them FMC cannot 
                    //WorkItem delete not allowed if it is existing work item
                    if ((details.Claim.ClaimFinalized || details.ContractorReClaim == true) && details.HasPWASignedOffLatestClaimMonth.HasValue
                        && !details.HasPWASignedOffLatestClaimMonth.Value)
                    {
                        costingSection = ""; //addWorkItem,workItemDelete not allowed as ClaimFinalized is ticked
                        recordSection = "IssueDate,SelectedDocumentTypeId,selectFile,btnUpload,DocumentsFinalised";
                        claimSection = "";
                    }
                }
                else if (CurrentUser.RoleID == "LC")
                {
                    if (details.Claim.ClaimFinalized)
                    {
                        //if claim is finalised, then they cant change anything
                        costingSection = "";
                        recordSection = "";
                        claimSection = isWorkItemsAmtZero ? "ClaimFinalized,ClaimFinalizedDate,ClaimedAmount" : ""; //if workItems amount is 0 enable edit
                        detailSection = isWorkItemsAmtZero ? "ServiceReportStatusId" : ""; //if workItems amount is 0 allow change of status
                    }
                    else
                    {
                        recordSection = "IssueDate,SelectedDocumentTypeId,selectFile,btnUpload,deleteDocument";
                        costingSection = "addWorkItem,workItemDelete";
                        //if claim is not finalised then rest of the logics needs to be tested
                        if (details.Estimate.EstimateFinalised)
                        {
                            detailSection = "SubContractorComments,SubContractorCommentDate,";
                            estimateSection = "";
                            if (details.Estimate.EstimatedAmount <= 5000)
                            {
                                claimSection = "ClaimFinalized,ClaimFinalizedDate,";
                                detailSection = detailSection + ",ServiceReportStatusId,CompletedDate";
                            }
                            else
                            {
                                //Estimate is more than 5000. Now approval process needs to be followed
                                if (details.Estimate.AMUApproved == true)
                                {
                                    claimSection = "ClaimFinalized,ClaimFinalizedDate,";
                                    detailSection = detailSection + ",ServiceReportStatusId,CompletedDate";
                                }
                            }
                        }
                        else
                        {
                            if (details.DetailsTab.SubContractorAccepted == null)
                            {
                                detailSection = "ExpectedVisitDate,FirstVisitDate,ExpectedFirstVisitTime,CompletedDate,EstimatedCompletionDate,SubContractorComments,SubContractorCommentDate,SubContractorId,SubContractorAccepted,SubContractorRespondedDate,SubContractorRespondedBy,TradieSelectList";
                                costingSection = "";
                                recordSection = "";
                            }
                            else if (details.DetailsTab.SubContractorAccepted == true)
                            {
                                detailSection = "ServiceReportStatusId,FirstVisitDate,CompletedDate,EstimatedCompletionDate,SubContractorComments,SubContractorCommentDate,SubContractorId,TradieSelectList,TradieSelectList";
                                estimateSection = "EstimateFinalised,EstimatedBy,EstimateDate,EstimateComments,EstimatedAmount,deleteDocument";
                                costingSection = "";
                            }
                            else
                            {
                                detailSection = "SubContractorComments,SubContractorCommentDate";
                                estimateSection = "";
                                recordSection = "";
                                costingSection = "";
                            }
                        }
                    }
                }
                else if (_jobService.IsAMUUser(CurrentUser) || CurrentUser.RoleID == "GL" || CurrentUser.RoleID == "SL" || CurrentUser.RoleID == "AM" || CurrentUser.RoleID == "EM")
                {
                    //AMU can update comments anytime
                    detailSection = "AMUComments,ASOCommentDate";
                    //FMC approved but not yet claimed
                    if (details.Estimate.FMCApproved == true && !details.Claim.ClaimFinalized)
                    {
                        estimateSection = "AMUApproved,AMUApprovalComments,AMUApprovedBy,AMUApprovedDate";
                    }

                    if ((!details.Estimate.EstimateFinalised && details.DetailsTab.SubContractorAccepted == null) || _agencyService.IsAMURegion(GetAgencyId())) //change
                    {
                        //AMU can change the INS status untill the Estimate is finalised and sub contractor has not accpeted
                        mainSection = "calloutLink,";

                    }
                    //Ticket 4116: enable school contribution for AMU untill WO status is paid.
                    if (_jobService.IsAMUUser(CurrentUser) && (details.DetailsTab.ServiceReportStatusId != null && details.DetailsTab.ServiceReportStatusId != 11)) //Paid
                    {
                        mainSection = mainSection + "IsSchoolContribution,";
                    }
                    //
                    if (!details.Claim.ClaimFinalized)
                    {
                        if (_jobService.HasOnlyOneWOWithSchoolContribution(details.CallLogFITSId.Value, details.ServiceReportId.Value))
                        {
                            mainSection = mainSection + "IsSchoolContribution,";
                        }

                        //Enable only if it is set by WO screen or CallOut INS status is false
                        //bool canEditInsStatus = _jobService.CanEditInsuranceStatusInWorkOrder(details.CallLogFITSId.Value, details.ServiceReportId.Value);
                        //if (canEditInsStatus)
                        //{
                        //    mainSection = mainSection + "IsInsuranceEvent,IsIncludeBetterment,";
                        //}
                        mainSection = mainSection + "IsInsuranceEvent,IsIncludeBetterment,InsuranceIncidentDateTime,WorkCategoryId";
                    }

                    if (details.Claim.ClaimFinalized && isWorkItemsAmtZero) //Even after claim Finalized but WorkItems Amt is 0, can change status to Cancelled
                    {
                        claimSection = "ClaimFinalized,ClaimFinalizedDate,ClaimedAmount"; //enable edit work items
                        detailSection = "ServiceReportStatusId"; //allow change of status
                    }

                    //AMU gets FMC access
                    if (_agencyService.IsAMURegion(GetAgencyId()))
                    {
                        detailSection = detailSection + ",ExpectedVisitDate,ExpectedFirstVisitTime,ServiceReportStatusId,FirstVisitDate,CompletedDate,Comments,FMCCommentDate,SubContractorId";
                        costingSection = "workItemDelete";
                        recordSection = (details.AgencySequenceId.HasValue && details.AgencySequenceId.Value > 0) ? "IssueDate,SelectedDocumentTypeId," : "";

                        // LC estimate finalized and not claimed enable AMU section
                        if (!details.Claim.ClaimFinalized)
                        {
                            if (details.Estimate.EstimateFinalised)
                            {
                                estimateSection = estimateSection + ",EstimateFinalised,EstimatedBy,EstimateDate";
                                detailSection = detailSection + ",SubContractorBusinessTypeId,WOSubContractorId,WOComments,raiseVariation";
                                if (details.Estimate.EstimatedAmount > 5000)
                                    estimateSection = estimateSection + ",AMUApproved,AMUApprovalComments,AMUApprovedBy,AMUApprovedDate,";
                            }
                            else
                            {
                                if (details.DetailsTab.ServiceReportStatusId == null || (details.DetailsTab.ServiceReportStatusId.Value != 8 && details.DetailsTab.ServiceReportStatusId.Value != 10))
                                {
                                    detailSection = detailSection + ",EstimatedCompletionDate";
                                }
                                costingSection = "workItemDelete";
                            }
                        }
                        else
                        {
                            estimateSection = "";
                            costingSection = "";
                            recordSection = "";
                            detailSection = "AMUComments,ASOCommentDate";
                        }

                        //if contractor reclaim them AMU cannot 
                        //WorkItem delete not allowed if it is existing work item
                        if ((details.Claim.ClaimFinalized || details.ContractorReClaim == true) && details.HasPWASignedOffLatestClaimMonth.HasValue && !details.HasPWASignedOffLatestClaimMonth.Value)
                        {
                            costingSection = ""; //addWorkItem, workItemDelete not allowed as ClaimFinalized is ticked
                            recordSection = "IssueDate,SelectedDocumentTypeId,selectFile,btnUpload,DocumentsFinalised";
                            claimSection = "";
                        }
                    }// END

                    if (details.DetailsTab.ServiceReportStatusId == 11)
                    {
                        mainSection = string.Empty;
                        detailSection = string.Empty;
                        estimateSection = string.Empty;
                        costingSection = string.Empty;
                        recordSection = string.Empty;
                        claimSection = string.Empty;
                    }
                }
                else if (_jobService.IsMFUUser(CurrentUser))
                {
                    claimSection += (details.Claim != null) ? ",ReimbursementComplete" : "";
                }

                //if the Job is Cancelled then, it cannot be changed by any of the roles other than Admin
                if (details.DetailsTab.ServiceReportStatusId == 9)
                {
                    mainSection = string.Empty;
                    detailSection = string.Empty;
                    estimateSection = string.Empty;
                    costingSection = string.Empty;
                    recordSection = string.Empty;
                    claimSection = string.Empty;
                }

                if (CurrentUser.RoleID == "A")
                {
                    mainSection = "calloutLink,IsInsuranceEvent,IsIncludeBetterment,IsSchoolContribution,InsuranceIncidentDateTime,WorkCategoryId";

                    detailSection = "ServiceReportStatusId,ExpectedVisitDate,ExpectedFirstVisitTime,FirstVisitDate,EstimatedCompletionDate,CompletedDate,FMCContactNumber," +
                        "RequestedBy,RequesterContactNumber,SubContractorComments,SubContractorCommentDate,SubContractorAccepted,Comments,FMCCommentDate,ExtentOfWork,AMUComments,ASOCommentDate,SubContractorBusinessTypeId,WOSubContractorId,WOComments,raiseVariation,TradieSelectList";

                    estimateSection = "deleteDocument,EstimateComments,EstimatedAmount,FMCRecommendedAction," +
                                                    "FMCApproved,FMCApprovalComments,FMCApprovedBy,FMCApprovedDate," +
                                                    "AMUApproved,AMUApprovalComments,AMUApprovedBy,AMUApprovedDate,";

                    //userCapability.Hide2A = true;
                    //userCapability.Hide3D = true;
                    //userCapability.Hide4A = true;

                    recordSection = "IssueDate,SelectedDocumentTypeId,selectFile,btnUpload,DocumentsFinalised,deleteDocument";
                    claimSection = (details.Claim != null && details.Claim.TotalAmount == 0) ? "ClaimFinalized,ClaimFinalizedDate,ClaimedAmount" : "";
                    claimSection += (details.Claim != null && details.Claim.ReimbursementComplete != true) ? ",ReimbursementComplete" : "";
                    claimSection += (details.Claim != null) ? ",ReimbursementComplete" : "";
                    costingSection = "workItemDelete,addWorkItem,ContractorFinalise,ContractorFinaliseDate,";

                    //if (details.Claim != null && details.Claim.ClaimFinalized)
                    //{
                    //    if (details.HasContractorSignedOffLatestClaimMonth != true)
                    //    {
                    mainSection = mainSection + ",unf";
                    //    }
                    //}

                    //if (details.DetailsTab.ServiceReportStatusId == 9)
                    //{
                    //    //if the Job is cancelled and another Job is not yet created, then Admin can change. 
                    //    //if another job is created then they cannot change
                    //    if (details.DetailsTab.MultipleJobsExistForSameCalloutId)
                    //    {
                    //        userCapability = new JobUserCapabilities();
                    //    }

                    //}

                    //return userCapability;
                }
            }
            userCapability.MainSection = mainSection;
            userCapability.DetailSection = detailSection;
            userCapability.EstimateSection = estimateSection;
            userCapability.CostingSection = costingSection;
            userCapability.RecordSection = recordSection;
            userCapability.ClaimSection = claimSection;

            return userCapability;
        }

        private void PopulateWorkCategoryListData(JobDetails details)
        {
            int? workCategory = details.WorkCategoryId;
            int propertyId = details.PropertyId;



            ViewData["BuildingsList"] = _buildingService.GetByProperty(propertyId).Select(x => new BuildingViewModel
            {
                BuildingID = x.BuildingID,
                BuildingDesc = "(" + x.BuildingNo + ") " + x.BuildingDesc + ((x.RemovedYN.HasValue && x.RemovedYN.Value) ? " (REMOVED)" : "")
            }).ToList();
            ViewData["ScheduleRateCategories"] = _jobService.GetScheduleRateCatgories()
                           .Select(x => new SelectListItem
                           {
                               Text = x.Description,
                               Value = x.ScheduledRateCategoryId.ToString()
                           }).OrderBy(d => d.Text).ToList();





        }

        private List<JobWorkItem> GetWorkItemsFromJob(JobDetails model)
        {
            List<JobWorkItem> data = new List<JobWorkItem>();
            if (model.WorkCategoryId == 1 || model.WorkCategoryId == 3 || model.WorkCategoryId == 100 || (model.WorkCategoryId == 13 && model.Version == 2))
            {
                //add only new documents. 
                if (model.ServiceReportNewLineItems != null)
                {
                    var format = "dd/MM/yyyy";
                    var dateTimeConverter = new IsoDateTimeConverter { DateTimeFormat = format };
                    data = JsonConvert.DeserializeObject<List<JobWorkItem>>(model.ServiceReportNewLineItems, dateTimeConverter);

                }
            }

            return data;
        }
        private int GetAgencyId()
        {
            var agencyId = 0;
            if (Session["AgencyDetails"] != null)
            {
                agencyId = ((KeyValuePair<int, string>)Session["AgencyDetails"]).Key;
            }
            if (agencyId == 0)
                Toast.ShowError(TempData, Constants.ToastErrorNoAgencySelected);
            return agencyId;
        }

        [HttpPost]
        public PartialViewResult FileNotFoundError()
        {
            return PartialView("_FileNotFoundError");
        }
    }
}