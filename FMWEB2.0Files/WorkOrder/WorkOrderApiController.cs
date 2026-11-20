using AssetCare.Common;
using AssetCare.Model.Models;
using AssetCare.Model.ViewModels;
using AssetCare.Service.Interfaces;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using AssetCare.Controllers;

namespace AssetCare.Areas.FMWorks.Controllers.ApiControllers
{
    [RoutePrefix("api/WorkOrder")]
    public class WorkOrderApiController : BaseApiController
    {
        // GET: api/JobsApi
        private readonly ICalloutService _fmCallOutService;
        private readonly IJobService _jobService;
        private readonly IUserService _userService;
        private readonly IClaimService _claimService;
        private ICrystalReportService _crystalReportService;
        private IAgencyService _agencyService;
        private readonly IMapper _mapper;
        string sinswEmail = ConfigurationManager.AppSettings["AssetCareSettings:EmailAutoCC"].ToString();

        public WorkOrderApiController(ICalloutService fmCallOutService, IJobService jobService, IUserService userService, IClaimService claimService, 
            ICrystalReportService crystalReportService, IAgencyService agencyService, IMapper mapper, IBaseService baseService) : base(baseService)
        {
            _fmCallOutService = fmCallOutService;
            _jobService = jobService;
            _userService = userService;
            _claimService = claimService;
            _crystalReportService = crystalReportService;
            _agencyService = agencyService;
            _mapper = mapper;
        }

        [HttpPost]
        [Route("filter")]
        [AuthorizeAPIOverride(Roles = "ADMIN,SUPPORT,CONTRACTOR,APIStandard,APIPriemium,PROJECTMNGR,PROJECTDIR,SCHOOL,SCHOOLMGR,DETAMU,GROUPLEADER,SRGRPLEADER,DIRAM,EDAM,AMUOFFICER,DETAMCU,AMCUOFFICE,MFUMGR,MFUOFFICER,SUPERREP,CALLCENTERSTAFF,LOCALCONTRACTOR,DOEUSER,SINSW")]
        public IHttpActionResult Filter(CalloutFilterPostModel filters)
        {
            int filteredResultsCount;
            int totalResultsCount;
            IEnumerable<JobListViewModel> result = GetFilteredData(filters, false, out filteredResultsCount, out totalResultsCount);
            var resultData = new
            {
                draw = "",
                recordsTotal = totalResultsCount,
                recordsFiltered = filteredResultsCount,
                data = result
            };
            return Json(resultData);


        }

        [HttpPost]
        [Route("filteredJobsForHP")]
        [AuthorizeAPIOverride(ReferredPages = "TaskList")]
        public IHttpActionResult FilteredJobsForHP(CalloutFilterPostModel filters)
        {
            int filteredResultsCount;
            int totalResultsCount;
            IEnumerable<JobListForHPViewModel> result = GetFilteredJobDataForHP(filters, false, out filteredResultsCount, out totalResultsCount);
            var resultData = new
            {
                draw = "",
                recordsTotal = totalResultsCount,
                recordsFiltered = filteredResultsCount,
                data = result
            };
            return Json(resultData);
        }


        [HttpPost]
        [Route("filteredJobsForHPClaim")]
        [AuthorizeAPIOverride(ReferredPages = "TaskList")]
        public IHttpActionResult FilteredJobsForHPClaim(CalloutFilterPostModel filters)
        {
            string usercode = RequestContext.Principal.UserCode();
            var user = _userService.GetByUserCode(usercode);
            switch (user.RoleCode)
            {
                case "LOCALCONTRACTOR":
                    filters.ServiceReportACIDClaim = "3,6,9";
                    break;
                case "CONTRACTOR":
                    filters.ServiceReportACIDClaim = "7";
                    break;
                case "DETAMU":
                case "AMUOFFICER":
                case "DETAMCU":
                case "GROUPLEADER":
                case "SRGRPLEADER":
                case "DIRAM":
                case "EDAM":
                    filters.ServiceReportACIDClaim = "8";
                    break;
                case "MFUMGR":
                case "MFUOFFICER":
                    filters.ServiceReportACIDClaim = "8";
                    break;
            }
            int filteredResultsCount;
            int totalResultsCount;
            IEnumerable<JobListForHPViewModel> result = GetFilteredJobDataForHPClaim(filters, false, out filteredResultsCount, out totalResultsCount);
            var resultData = new
            {
                draw = "",
                recordsTotal = totalResultsCount,
                recordsFiltered = filteredResultsCount,
                data = result
            };
            return Json(resultData);
        }


        [HttpPost]
        [Route("filteredJobsForSchoolHP")]
        [AuthorizeAPIOverride(ReferredPages = "SchoolHome")]
        public IHttpActionResult FilteredJobsForSchoolHP(CalloutFilterPostModel filters)
        {
            int filteredResultsCount;
            int totalResultsCount;
            IEnumerable<JobListForSchoolHPViewModel> result = GetFilteredJobDataForSchoolHP(filters, false, out filteredResultsCount, out totalResultsCount);
            var resultData = new
            {
                draw = "",
                recordsTotal = totalResultsCount,
                recordsFiltered = filteredResultsCount,
                data = result
            };
            return Json(resultData);
        }

        [HttpGet]
        [Route("getTradieDetailsHP")]
        [AuthorizeAPIOverride(ReferredPages = "SchoolHome")]
        public IHttpActionResult GetTradieDetailsHP(int woId,  int agencyId)
        {
            IEnumerable<TradieViewModel> result = GetTradieDetailsHP1(woId, agencyId);
            var resultData = new
            {
                draw = "",
                data = result

            };
            return Json(resultData);
        }

        private IEnumerable<TradieViewModel> GetTradieDetailsHP1(int woId, int agencyId)
        {
            IEnumerable<TradieViewModel> result = _fmCallOutService.GetTradieDetailsHP(woId, agencyId);
            //string filterSearch = filters.search.value;
            result = result.Select(x =>
            {
                return x;
            }).ToList();

            return result;
        }


        [HttpPost]
        [Route("GetSchoolHomeTradies")]
        [AuthorizeAPIOverride(ReferredPages = "SchoolHome")]
        public IHttpActionResult GetSchoolHomeTradies(CalloutFilterPostModel filters)
        {
            int filteredResultsCount;
            int totalResultsCount;
            List<TradieViewModel> result = GetSchoolHomeTradies(filters, out filteredResultsCount, out totalResultsCount).ToList();
            var resultData = new
            {
                draw = filters.draw,
                recordsTotal = totalResultsCount,
                recordsFiltered = filteredResultsCount,
                data = result
            };
            return Json(resultData);
        }

       

        private IEnumerable<TradieViewModel> GetSchoolHomeTradies(CalloutFilterPostModel filters, out int filteredResultsCount, out int totalResultsCount)
        {
            //DataTableFilterModel<CalloutFilterPostModel> filterModel = DataTableHelper.TransformDataTablePostModel<CalloutFilterPostModel>(filters);
            DataTableFilterModel<TradieViewModel> filterModel = DataTableHelper.TransformDataTablePostModel<TradieViewModel>(filters);

            CallOutFilterModel callOutFilterModel = new CallOutFilterModel()
            {

                AgencyId = filters.AgencyId,
                PropertyId = filters.PropertyId,
                Workcategory = filters.Workcategory,
                SubContractorID = filters.SubContractorID,
                SortBy = "ExpectedVisitDate",
            };
            filterModel.SortBy = "ExpectedVisitDate";
            callOutFilterModel = _mapper.Map(filterModel, callOutFilterModel);
            IEnumerable<TradieViewModel> result = _fmCallOutService.GetSchoolHomeTradies(callOutFilterModel, out filteredResultsCount, out totalResultsCount);
            string filterSearch = filters.search.value;
            if (filters.search.value != null)
            {
            }
            result = result.Select(x =>
            {
                return x;
            }).ToList();

            return result;// result.Skip(callOutFilterModel.Skip).Take(callOutFilterModel.Take).ToList();
        }

        [HttpPost]
        [Route("filteredJobApprovalsForHP")]
        [AuthorizeAPIOverride(ReferredPages = "TaskList")]
        public IHttpActionResult FilteredJobApprovalsForHP(CalloutFilterPostModel filters)
        {
            int filteredResultsCount;
            int totalResultsCount;
            IEnumerable<JobListForHPViewModel> result = GetFilteredJobApprovalDataForHP(filters, false, out filteredResultsCount, out totalResultsCount);
            var resultData = new
            {
                draw = "",
                recordsTotal = totalResultsCount,
                recordsFiltered = filteredResultsCount,
                data = result
            };
            return Json(resultData);
        }


        
        [HttpPost]
        [Route("AcceptWorkorder")]       
        public IHttpActionResult AcceptWorkorder(WorkOrderAcceptanceStatus status)
        {
            string loggedInUser = RequestContext.Principal.UserCode();
            status.LoggedInUser = loggedInUser;
            bool updStatus = _jobService.UpdateJobAcceptance(status);
            return Json(updStatus);
        }

       


        [AuthorizeAPIOverride(Roles = "ADMIN,SUPPORT,CONTRACTOR,PROJECTMNGR,PROJECTDIR,SCHOOL,SCHOOLMGR,DETAMU,GROUPLEADER,SRGRPLEADER,DIRAM,EDAM,AMUOFFICER,DETAMCU,AMCUOFFICE,MFUMGR,MFUOFFICER,SUPERREP,CALLCENTERSTAFF,LOCALCONTRACTOR")]
        [Route("download")]
        public HttpResponseMessage Download(CalloutFilterPostModel filters)
        {
            int filteredResultsCount;
            int totalResultsCount;
            List<JobListViewModel> result = GetFilteredData(filters, true, out filteredResultsCount, out totalResultsCount).ToList();
            var excelData = result.Select(x => new
            {
               
                WOID = x.JobID,
                WODate = x.WODate,
                Category = x.WorkCategory,
                LocalContractor = x.SubContractor,               
                SchoolName = x.SchoolName,
                BuildingList = this.GenerateBuidingList(x.BuildingList),
                RequestedBy = x.RequestedBy,
                Description = x.Description,
                Status = x.Status,
                CalloutID = x.CallOutId,                
            }).ToList();


            return (new ExcelHelper()).GenerateExcelDocument(excelData, filters.ExportType, "WorkOrderData");
        }

        [AuthorizeAPIOverride(Roles = "ADMIN,SUPPORT,CONTRACTOR,PROJECTMNGR,PROJECTDIR,SCHOOL,SCHOOLMGR,DETAMU,GROUPLEADER,SRGRPLEADER,DIRAM,EDAM,AMUOFFICER,DETAMCU,AMCUOFFICE,MFUMGR,MFUOFFICER,SUPERREP,CALLCENTERSTAFF,LOCALCONTRACTOR")]
        [HttpGet]
        [Route("GetScheduleRates")]
        public List<ScheduleRateDropdownItem> GetScheduleRates(int agencyId, int? categoryId)
        {
            string usercode = RequestContext.Principal.UserCode();
            var user = _userService.GetByUserCode(usercode);
            //, int? categoryId
            List<ScheduleRateDropdownItem> sc = _jobService.GetScheduledRates(agencyId, categoryId, user.DefaultLocalContractorId );
            return sc;
        }

        [HttpGet]
        [AuthorizeAPIOverride(Roles = "ADMIN,SUPPORT,CONTRACTOR,PROJECTMNGR,PROJECTDIR,SCHOOL,SCHOOLMGR,DETAMU,GROUPLEADER,SRGRPLEADERAMUOFFICER,DETAMCU,AMCUOFFICE,MFUMGR,MFUOFFICER,SUPERREP,CALLCENTERSTAFF,LOCALCONTRACTOR")]
        [Route("GetCostAdjustmemt")]
        public string GetCostAdjustmemt(int agencyId, string costAdjType, string costDate)
        {
            //, int? categoryId
            decimal costAdj = _jobService.GetCostAdjustment(agencyId, costAdjType, costDate);
            return costAdj.ToString();
        }

        [Route("downloadJobsForHP")]
        [AuthorizeAPIOverride(ReferredPages = "TaskList")]
        public HttpResponseMessage DownloadJobsForHP(CalloutFilterPostModel filters)
        {
            int filteredResultsCount;
            int totalResultsCount;
            List<JobListForHPViewModel> result = GetFilteredJobDataForHP(filters, true, out filteredResultsCount, out totalResultsCount).ToList();
            var excelData = result.Select(x => new
            {
                WOID = x.JobID,
                WODate = x.WODate,
                SchoolName = x.SchoolName,
                RequestedBy = x.RequestedBy,
                Description = x.Description,
                Status = x.Status,

            }).ToList();
            return (new ExcelHelper()).GenerateExcelDocument(excelData, filters.ExportType, "WorkOrderData");
        }

        [Route("downloadJobsForHPClaim")]
        [AuthorizeAPIOverride(ReferredPages = "TaskList")]
        public HttpResponseMessage DownloadJobsForHPClaim(CalloutFilterPostModel filters)
        {
            int filteredResultsCount;
            int totalResultsCount;
            List<JobListForHPViewModel> result = GetFilteredJobDataForHPClaim(filters, true, out filteredResultsCount, out totalResultsCount).ToList();
            var excelData = result.Select(x => new
            {
                WOID = x.JobID,
                WODate = x.WODate,
                SchoolName = x.SchoolName,
                RequestedBy = x.RequestedBy,
                Description = x.Description,
                Status = x.Status,

            }).ToList();
            return (new ExcelHelper()).GenerateExcelDocument(excelData, filters.ExportType, "WorkOrderData");
        }

        [Route("downloadJobsForSchoolHP")]
        [AuthorizeAPIOverride(ReferredPages = "SchoolHome")]
        public HttpResponseMessage DownloadJobsForSchoolHP(CalloutFilterPostModel filters)
        {
            int filteredResultsCount;
            int totalResultsCount;
            List<JobListForSchoolHPViewModel> result = GetFilteredJobDataForSchoolHP(filters, true, out filteredResultsCount, out totalResultsCount).ToList();
            var excelData = result.Select(x => new
            {
                CallID = x.CallOutID,
                //Buildings = x.Buildings,
                //CallDate = x.CallDate,
                //CompletionDate = x.CompletionDate,
                //EstimatedCompletionDate = x.EstCompletionDate,
                JobID = x.JobID,
                WorkCategory = x.WorkCategory,
                PropertyID = x.PropertyID,
                Descriptoion = x.CallDescription
            }).ToList();
            return (new ExcelHelper()).GenerateExcelDocument(excelData, filters.ExportType, "WorkOrderData");
        }

        [Route("downloadJobApprovalsForHP")]
        [AuthorizeAPIOverride(ReferredPages = "TaskList")]
        public HttpResponseMessage DownloadJobApprovalsForHP(CalloutFilterPostModel filters)
        {
            int filteredResultsCount;
            int totalResultsCount;
            List<JobListForHPViewModel> result = GetFilteredJobApprovalDataForHP(filters, true, out filteredResultsCount, out totalResultsCount).ToList();
            var excelData = result.Select(x => new
            {
                JobID = x.JobID,
                JobType = x.JobType,
                ReceivedDate = x.ReceivedDate,
                SchoolName = x.SchoolName,
                Description = x.Description
            }).ToList();

            return (new ExcelHelper()).GenerateExcelDocument(excelData, filters.ExportType);
        }


        [AuthorizeAPIOverride(Roles = "ADMIN,SUPPORT,CONTRACTOR,PROJECTMNGR,PROJECTDIR,SCHOOL,SCHOOLMGR,DETAMU,GROUPLEADER,SRGRPLEADER,DIRAM,EDAM,AMUOFFICER,DETAMCU,AMCUOFFICE,MFUMGR,MFUOFFICER,SUPERREP,CALLCENTERSTAFF,LOCALCONTRACTOR")]
        [HttpGet]
        [Route("GetReimbursementStatuses")]
        public IList<ReimbursementStatusVM> GetReimbursementStatuses()
        {
            IList<ReimbursementStatus> rs = _jobService.GetReimbursementStatuses();
            return rs.Select(x => new ReimbursementStatusVM { ReimbursementStatusID = x.ReimbursementStatusID, ReimbursementStatusDesc = x.ReimbursementStatusDesc }).ToList();
        }

        [HttpPost]
        [Route("SaveClaimApproval")]
        public IHttpActionResult SaveClaimApproval(List<WorkOrderApprovalListItem> data)
        {
            string usercode = RequestContext.Principal.UserCode();
            var user = _userService.GetByUserCode(usercode);
            
            foreach (WorkOrderApprovalListItem item in data)
            {
                
                if (item.ApprovalType == "fmc")
                {
                    if (user.RoleID == "A" || user.RoleID == "C")
                    {
                        string title = string.Format("FMWEB 2.0 WORK ORDER – PAYMENT CLAIM IS {0} BY FMC", (item.Approved.HasValue && item.Approved.Value) ? "ENDORSED" : "RETURNED");
                        bool status = _jobService.ReviewClaims(item, usercode);
                        if (item.Approved == true)
                        {
                            
                            SendFMCClaimReviewwedEmail(item.Id, user, "FMWEB 2.0 Reactive WO {0} PAYMENT CLAIM IS AWAITING YOUR APPROVAL", "WOFMCReviewed.html", title : title, header: "The following work order payment claim is reviewed by FMC and awaiting AMU's approval.");
                        }
                        else
                        {
                            SendFMCClaimReviewwedEmail(item.Id, user, "FMWEB 2.0 Reactive WO {0} PAYMENT CLAIM IS RETURNED TO LOCAL CONTRACTOR", "WOFMCReviewed.html", title: title, header: "The following work order payment claim is returned to Local Contractor for review.");
                        }
                    }

                }
                else if (item.ApprovalType == "amu")
                {
                    if (_jobService.IsAMUUser(user) || user.RoleID == "GL" || user.RoleID == "SL" || user.RoleID == "AM" || user.RoleID == "EM" || user.RoleID == "A")
                    {
                        var agencyId = Convert.ToInt32( _jobService.GetClaimApprovalData(item.Id).AgencyId);
                        bool status = _jobService.ApproveClaims(item, usercode, _agencyService.IsAMURegion(agencyId));
                        string title = string.Format("FMWEB 2.0 WORK ORDER – PAYMENT CLAIM IS {0} BY AMU", (item.Approved.HasValue && item.Approved.Value) ? "APPROVED" : "REJECTED");
                        if (item.Approved == true)
                        {
                            string reportName = "";

                            var reportData = GenerateReactiveRCTIReport(item.Id, out reportName);
                            SendFMCClaimReviewwedEmail(item.Id, user, "FMWEB 2.0 Reactive WO {0} PAYMENT CLAIM IS APPROVED", "WOClaimASOApproved.html", reportName, attachmentData: reportData, title: title, header: "The following work order payment claim is approved by AMU and submitted to the Finance team for further processing.");
                        }
                        else
                        {
                            SendFMCClaimReviewwedEmail(item.Id, user, "FMWEB 2.0 Reactive WO {0} PAYMENT CLAIM IS RETURNED", "WOClaimASOApproved.html", title: title, header: "The following work order payment claim is returned by AMU for FMC's review");
                        }
                    }

                }

            }

            return  Json(true); 
        }

        [HttpPost]
        [Route("SaveInsuranceReimbursement")]
        public IHttpActionResult SaveInsuranceReimbursement(List<InsuranceReimbursementListItem> data)
        {
            string usercode = RequestContext.Principal.UserCode();
            var user = _userService.GetByUserCode(usercode);

            foreach (var item in data)
            {

                 if (item.ApprovalType == "mfu" || user.RoleID == "A")
                 {
                    if (_jobService.IsMFUUser(user) || user.RoleID == "A")
                    {
                        bool status = _jobService.ApproveInsuranceReimbursement(item, usercode);
                        //string title = string.Format("FMWEB 2.0 WORK ORDER – PAYMENT CLAIM IS {0} BY AMU", (item.Approved.HasValue && item.Approved.Value) ? "APPROVED" : "REJECTED");
                        //if (item.Approved == true)
                        //{
                        //    string reportName = "";

                        //    var reportData = GenerateReactiveRCTIReport(item.Id, out reportName);
                        //    SendFMCClaimReviewwedEmail(item.Id, user, "FMWEB 2.0 Reactive WO {0} PAYMENT CLAIM IS APPROVED", "WOClaimASOApproved.html", reportName, attachmentData: reportData, title: title, header: "The following work order payment claim is approved by AMU and submitted to the Finance team for further processing.");
                        //}
                        //else
                        //{
                        //    SendFMCClaimReviewwedEmail(item.Id, user, "FMWEB 2.0 Reactive WO {0} PAYMENT CLAIM IS RETURNED", "WOClaimASOApproved.html", title: title, header: "The following work order payment claim is returned by AMU for FMC's review");
                        //}
                    }

                }

            }

            return Json(true);
        }

        [HttpPost]
        [Route("DownloadRCTI")]
        [AuthorizeAPIOverride(Roles = "ADMIN,SUPPORT,CONTRACTOR,DETAMU,GROUPLEADER,SRGRPLEADER,DIRAM,EDAM,AMUOFFICER,DETAMCU,AMCUOFFICE,MFUMGR,MFUOFFICER,CALLCENTERSTAFF,LOCALCONTRACTOR")]
        public HttpResponseMessage DownloadRCTI(RCTIDownloadModel model)
        {
            string reportName = "";
            if(model != null && model.Id > 0)
            {
                var result = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(GenerateReactiveRCTIReport(model.Id, out reportName))
                };

                result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = reportName
                };
                result.Content.Headers.ContentType =  new MediaTypeHeaderValue("aapplication/pdf") ;

                return result;
            }
            else
            {
                return new HttpResponseMessage();
            }
        }
        private void SendFMCClaimReviewwedEmail(int claimId, UserViewModel currentUser, string subject, string template, string attachmentName = null, byte[] attachmentData = null, string header = "Below claim is approved.", string title = "")
        {
            WorkOrderClaimApprovalViewModdel claimData = _jobService.GetClaimApprovalData(claimId);
            string toEmail = string.Format("{0};{1};{2};{3}",claimData.ASOEmail , currentUser.Email, claimData.LCEmail, claimData.FMCEmail);
            subject = string.Format(subject, claimData.WorkOrderId.ToString());
            EmailHelper emailHelper = new EmailHelper();
            Dictionary<string, string> data = new Dictionary<string, string>();            
            data.Add("JobId", claimData.WorkOrderId.ToString());
            data.Add("Header", header);
            data.Add("Title", title);

            data.Add("FMCEndorsedYN", (claimData.FMCReviewed.HasValue && claimData.FMCReviewed.Value ? "Yes" : "No"));
            data.Add("FMCEndorseDate", claimData.FMCReviewedDate.HasValue ? claimData.FMCReviewedDate.Value.ToString("dd/MM/yyyy") : "" );
            data.Add("AMUApprovedYN", (claimData.AMUApproved.HasValue && claimData.AMUApproved.Value) ? "Yes" : "No");
            data.Add("ClaimDate", claimData.ClaimedDate.HasValue ? claimData.ClaimedDate.Value.ToString("dd/MM/yyyy") : "");

            data.Add("AgencyId", claimData.AgencyId.ToString());
            data.Add("ReviewedBy", currentUser.FullName);
            data.Add("SchoolName", claimData.SchoolName);
            data.Add("WorkCategoryDesc", claimData.WorkCategory);
            data.Add("FMCReviewedDate", claimData.FMCReviewedDate.HasValue ? claimData.FMCReviewedDate.Value.ToString("dd/MM/yyyy") : "");
            data.Add("FMCComment", claimData.FMCReviewComments);

            data.Add("ApprovedDate", claimData.AMUApprovedDate.HasValue ? claimData.AMUApprovedDate.Value.ToString("dd/MM/yyyy") : "");
            data.Add("ClaimedAmount", claimData.ClaimedAmount.HasValue ? claimData.ClaimedAmount.Value.ToString("0.00") : "");
            data.Add("ApprovedAmount", claimData.ApprovedAmount.HasValue ? claimData.ApprovedAmount.Value.ToString("0.00") : "");
            data.Add("ASOComment", claimData.AMUApprovalComment);
            if (attachmentData == null)
            {
                emailHelper.SendMail(toEmail, data, subject, template, cc: sinswEmail);
            }
            else
            {
                emailHelper.SendMailWithAttachments(toEmail, data, subject, template, attachmentData, attachmentName, cc: sinswEmail);
            }
        }

        private byte[] GenerateReactiveRCTIReport(int claimId, out string reportName)
        {
            ReactiveRCTIReportViewModel reportViewModel = _claimService.GetReactiveRCTIData(claimId);
            int reactiveRCTIReportId = 16;
            ReactiveRCTIReportModel reportModel = new ReactiveRCTIReportModel();

            reportModel = _mapper.Map(reportViewModel, reportModel);
            var report = _crystalReportService.Get(reactiveRCTIReportId);
            report.Parameters = new Dictionary<string, object>
            {
                { "ClaimId", claimId },            
            };
                                  
            report.ContractName = reportModel.ContractNo;
            report.EducationArea = reportModel.Contract;
            report.Category = reportModel.WorkCategory;
            report.ProgressNo = reportModel.WorkOrderNo;
            report.APName = reportModel.SignedName;
            report.PostalAddress = reportModel.SubContractorPostalAddress;
            report.Suburb = reportModel.SubContractorSuburb;
            report.State = reportModel.SubContractorState;
            report.PostCode = reportModel.SubContractorPostCode;
            report.ABN = reportModel.ContractorABN;
            report.APName = reportModel.SignedName;
            report.ContractFull = reportModel.SubContractorName;
            report.ApprovedAmount = reportModel.ApprovedValue.ToString("C");
            report.ApprovedGST = reportModel.ApprovedGST.ToString("C");
            report.ApprovedTotal = reportModel.ApprovedValueGrandTotal.ToString("C");
            report.ClaimedAmount = reportModel.ClaimValue.ToString("C");
            report.ClaimedGST = reportModel.ClaimGST.ToString("C");
            report.ClaimedGrandTotal = reportModel.ClaimValueGrandTotal.ToString("C");
            report.School = reportModel.School;

            ExportFormatType reportType = ExportFormatType.PortableDocFormat;           
            
            reportName = $"RCTI WO-{reportModel.WorkOrderNo} {DateTime.Now.ToString("yyyyMMdd")}_{Guid.NewGuid().ToString().Substring(0, 8)}.pdf";
            var rptFile = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/Reports"), report.CrystalRptFilename);
            var stream = _crystalReportService.Generate(rptFile, reportType, report);
         
            byte[] reportBytes = null;         
            MemoryStream ms = new MemoryStream();
            using (stream)
            {
                stream.CopyTo(ms);
            }
            reportBytes = ms.ToArray();

            return reportBytes;
        }

        [HttpPost]
        [Route("FilterWorkOrderClaimList")]
        [AuthorizeAPIOverride(Roles = "ADMIN,SUPPORT,CONTRACTOR,DETAMU,GROUPLEADER,SRGRPLEADER,DIRAM,EDAM,AMUOFFICER,DETAMCU,AMCUOFFICE,MFUMGR,MFUOFFICER,CALLCENTERSTAFF,LOCALCONTRACTOR,DOEUSER,SINSW")]
        public IHttpActionResult FilterWorkOrderClaimList(WorkOrderCertifyPostModel claimModel)
        {
            int filteredResultsCount;
            int totalResultsCount;
            if (claimModel.ApprovalFromDate.HasValue || claimModel.ApprovalToDate.HasValue )
            {
                claimModel.ApprovalStatus = true;
            }
            IEnumerable<WorkOrderCertifyListItem> result = GetFilteredData(claimModel, false, out filteredResultsCount, out totalResultsCount);
            var resultData = new
            {
                draw = "",
                recordsTotal = totalResultsCount,
                recordsFiltered = filteredResultsCount,
                data = result
            };
            return Json(resultData);
          
        }
        [HttpPost]
        [Route("DownloadClaimList")]
        [AuthorizeAPIOverride(Roles = "ADMIN,SUPPORT,CONTRACTOR,DETAMU,GROUPLEADER,SRGRPLEADER,DIRAM,EDAM,AMUOFFICER,DETAMCU,AMCUOFFICE,MFUMGR,MFUOFFICER,CALLCENTERSTAFF,LOCALCONTRACTOR,DOEUSER,SINSW")]
        public HttpResponseMessage DownloadClaimList(WorkOrderCertifyPostModel claimModel)
        {
            int filteredResultsCount;
            int totalResultsCount;
            List<WorkOrderCertifyListItem> result = GetFilteredData(claimModel, true, out filteredResultsCount, out totalResultsCount).ToList();
            var excelData = result.Select(x => new
            {

                WOID = x.WorkOrderId,  
                LRLV = x.LRLV,
                Category = x.Category,
                LocalContractor = x.LocalContractor,
                SchoolName = x.SchoolName,
                JobDesc = x.JobDesc,
                ClaimDate = x.ClaimFinalizedDate,
                ClaimedAmount = x.ClaimedAmount,
                ApprovedAmount = x.ApprovedAmount,
                FMCComment = x.FMCComments,
                FMCReviewed = x.FMCReviewed == true ? "Yes" : "No",
                ASOComment = x.ASOComment,
                ASOApproved = x.AMUApproved == true ? "Yes": "No",
                ApprovedDate = x.ApprovedDate
                
                
            }).ToList();


            return (new ExcelHelper()).GenerateExcelDocument(excelData, claimModel.ExportType, "WorkOrderClaimsData");
        }

        [HttpPost]
        [Route("FilterInsuranceClaimsReimbursement")]
        [AuthorizeAPIOverride(Roles = "ADMIN,SUPPORT,CONTRACTOR,DETAMU,GROUPLEADER,SRGRPLEADER,DIRAM,EDAM,AMUOFFICER,DETAMCU,AMCUOFFICE,MFUMGR,MFUOFFICER,CALLCENTERSTAFF,LOCALCONTRACTOR")]
        public IHttpActionResult FilterInsuranceClaimsReimbursement(InsuranceClaimsReimbursementFilterPostModel claimModel)
        {
            int filteredResultsCount;
            int totalResultsCount;
            if (claimModel.ApprovalFromDate.HasValue || claimModel.ApprovalToDate.HasValue)
            {
                claimModel.ApprovalStatus = true;
            }
            IEnumerable<InsuranceClaimsReimbursementListItem> result = GetInsuranceClaimsReimbursementFilteredData(claimModel, false, out filteredResultsCount, out totalResultsCount);
            var resultData = new
            {
                draw = "",
                recordsTotal = totalResultsCount,
                recordsFiltered = filteredResultsCount,
                data = result
            };
            return Json(resultData);

        }
        [HttpPost]
        [Route("DownloadInsuranceClaimList")]
        [AuthorizeAPIOverride(Roles = "ADMIN,SUPPORT,CONTRACTOR,DETAMU,GROUPLEADER,SRGRPLEADER,DIRAM,EDAM,AMUOFFICER,DETAMCU,AMCUOFFICE,MFUMGR,MFUOFFICER,CALLCENTERSTAFF,LOCALCONTRACTOR")]
        public HttpResponseMessage DownloadInsuranceClaimList(InsuranceClaimsReimbursementFilterPostModel reimbursementModel)
        {
            int filteredResultsCount;
            int totalResultsCount;
            List<InsuranceClaimsReimbursementListItem> result = GetInsuranceClaimsReimbursementFilteredData(reimbursementModel, true, out filteredResultsCount, out totalResultsCount).ToList();
            var excelData = result.Select(x => new
            {
                WOID = x.WorkOrderId,
                Category = x.Category,
                LocalContractor = x.LocalContractor,
                SchoolName = x.SchoolName,
                JobDesc = x.JobDesc,
                ClaimedAmount = x.ClaimedAmount,
                ApprovedAmount = x.ApprovedAmount,
                FMCComment = x.FMCComments,
                FMCReviewed = x.FMCReviewed == true ? "Yes" : "No",
                ASOComment = x.ASOComment,
                ASOApproved = x.AMUApproved == true ? "Yes" : "No",
                ApprovedDate = x.ApprovedDate,
                CalloutDisplayId = x.CalloutDisplayId,
                ServiceReportLineItemDisplayId = x.ServiceReportLineItemDisplayId,
                ClaimStatusID = x.IRCMClaimStatusID,
                ReimbursementStatusID = x.ReimbursementStatusID,
                ReimbursementAmount = x.ReimbursementAmount,
                IRCMComments = x.IRCMComments,
                ReimbursementDate = x.ReimbursementDate
            }).ToList();


            return (new ExcelHelper()).GenerateExcelDocument(excelData, reimbursementModel.ExportType, "WorkOrderClaimsData");
        }

        [HttpPost]
        [Route("GenerateInsuranceReimbursementItems")]
        [AuthorizeAPIOverride(Roles = "ADMIN,SUPPORT,CONTRACTOR,DETAMU,GROUPLEADER,SRGRPLEADER,DIRAM,EDAM,AMUOFFICER,DETAMCU,AMCUOFFICE,MFUMGR,MFUOFFICER,CALLCENTERSTAFF,LOCALCONTRACTOR")]
        public IHttpActionResult GenerateInsuranceReimbursementItems(InsuranceClaimsReimbursementFilterPostModel claimModel)
        {
            int filteredResultsCount;
            int totalResultsCount;
            if (claimModel.ApprovalFromDate.HasValue || claimModel.ApprovalToDate.HasValue)
            {
                claimModel.ApprovalStatus = true;
            }
            IEnumerable<WorkOrderCertifyListItem> result = GetWorkOrderCertifyListItems(claimModel, false, out filteredResultsCount, out totalResultsCount);
            string loggedInUser = RequestContext.Principal.UserCode();
            _jobService.GenerateReimbursementItems(result, loggedInUser);
            var resultData = new
            {
                draw = "",
                recordsTotal = totalResultsCount,
                recordsFiltered = filteredResultsCount,
                data = result
            };
            return Json(resultData);
         }

        private IEnumerable<WorkOrderCertifyListItem> GetFilteredData(WorkOrderCertifyPostModel filters, bool dataForExport, out int filteredResultsCount, out int totalResultsCount)
        {
            string usercode = RequestContext.Principal.UserCode();
            var user = _userService.GetByUserCode(usercode);
            //DataTableFilterModel<WorkOrderCertifyPostModel> filterModel = DataTableHelper.TransformDataTablePostModel<WorkOrderCertifyPostModel>(filters);
            DataTableFilterModel<WorkOrderCertifyListItem> filterModel = DataTableHelper.TransformDataTablePostModel<WorkOrderCertifyListItem>(filters);
            WorkOrderCertifyFilterModel workOderCertifyFilterModel = new WorkOrderCertifyFilterModel();
            workOderCertifyFilterModel = _mapper.Map(filters, workOderCertifyFilterModel);
            workOderCertifyFilterModel = _mapper.Map(filterModel, workOderCertifyFilterModel);

            workOderCertifyFilterModel.RestrictToLocalContractorId = user.RoleID == "LC";
            if(user.RoleID == "LC")
            {
                workOderCertifyFilterModel.SubContractorId = user.DefaultLocalContractorId;
            }
            

            

            if (dataForExport) workOderCertifyFilterModel.Take = -1; //if it is export, we need all the data
            IEnumerable<WorkOrderCertifyListItem> result = _jobService.GetClaimsToApprove(workOderCertifyFilterModel, out filteredResultsCount, out totalResultsCount);
            return result;
        }

        private IEnumerable<WorkOrderCertifyListItem> GetWorkOrderCertifyListItems(WorkOrderCertifyPostModel filters, bool dataForExport, out int filteredResultsCount, out int totalResultsCount)
        {
            string usercode = RequestContext.Principal.UserCode();
            var user = _userService.GetByUserCode(usercode);

            WorkOrderCertifyFilterModel workOderCertifyFilterModel = new WorkOrderCertifyFilterModel();
            workOderCertifyFilterModel = _mapper.Map(filters, workOderCertifyFilterModel);

            if (dataForExport) workOderCertifyFilterModel.Take = -1; //if it is export, we need all the data
            IEnumerable<WorkOrderCertifyListItem> result = _jobService.GetWorkOrderCertifyListItems(workOderCertifyFilterModel, out filteredResultsCount, out totalResultsCount);
            return result;
        }

        private IEnumerable<InsuranceClaimsReimbursementListItem> GetInsuranceClaimsReimbursementFilteredData(InsuranceClaimsReimbursementFilterPostModel filters, bool dataForExport, out int filteredResultsCount, out int totalResultsCount)
        {
            string usercode = RequestContext.Principal.UserCode();
            var user = _userService.GetByUserCode(usercode);
            //DataTableFilterModel<InsuranceClaimsReimbursementFilterPostModel> filterModel = DataTableHelper.TransformDataTablePostModel<InsuranceClaimsReimbursementFilterPostModel>(filters);
            DataTableFilterModel<InsuranceClaimsReimbursementListItem> filterModel = DataTableHelper.TransformDataTablePostModel<InsuranceClaimsReimbursementListItem>(filters);
            InsuranceClaimsReimbursementFilterModel workOderCertifyFilterModel = new InsuranceClaimsReimbursementFilterModel();
            workOderCertifyFilterModel = _mapper.Map(filters, workOderCertifyFilterModel);
            workOderCertifyFilterModel = _mapper.Map(filterModel, workOderCertifyFilterModel);
            if (dataForExport) workOderCertifyFilterModel.Take = -1; //if it is export, we need all the data
            IEnumerable<InsuranceClaimsReimbursementListItem> result = _jobService.GetClaimsToApproveForInsuranceReimbursement(workOderCertifyFilterModel, out filteredResultsCount, out totalResultsCount);
            return result;
        }
        private IEnumerable<JobListViewModel> GetFilteredData(CalloutFilterPostModel filters, bool dataForExport, out int filteredResultsCount, out int totalResultsCount)
        {
            string usercode = RequestContext.Principal.UserCode();
            var user = _userService.GetByUserCode(usercode);

            //DataTableFilterModel<CalloutFilterPostModel> filterModel = DataTableHelper.TransformDataTablePostModel<CalloutFilterPostModel>(filters);
            DataTableFilterModel<JobListViewModel> filterModel = DataTableHelper.TransformDataTablePostModel<JobListViewModel>(filters);
            CallOutFilterModel callOutFilterModel = new CallOutFilterModel()
            {
                AgencyId = filters.AgencyId,
                SchoolId = filters.SchoolId,
                PropertyId = filters.PropertyId,
                StatusId = filters.StatusId,
                Workcategory = filters.Workcategory,
                ShowWOPaidOnly = filters.ShowWOPaidOnly
            };
            callOutFilterModel = _mapper.Map(filterModel, callOutFilterModel);
            if (dataForExport) callOutFilterModel.Take = -1; //if it is export, we need all the data
            callOutFilterModel.RestrictToLocalContractorId = user.RoleID == "LC";
            callOutFilterModel.SubContractorID = user.DefaultLocalContractorId;
            IEnumerable<JobListViewModel> result = _jobService.FilteredJobData(callOutFilterModel, dataForExport, out filteredResultsCount, out totalResultsCount);
            return result;
        }

        private IEnumerable<JobListForHPViewModel> GetFilteredJobDataForHP(CalloutFilterPostModel filters, bool dataForExport, out int filteredResultsCount, out int totalResultsCount)
        {
            //DataTableFilterModel<CalloutFilterPostModel> filterModel = DataTableHelper.TransformDataTablePostModel<CalloutFilterPostModel>(filters);
            DataTableFilterModel<JobListForHPViewModel> filterModel = DataTableHelper.TransformDataTablePostModel<JobListForHPViewModel>(filters);
            CallOutFilterModel callOutFilterModel = new CallOutFilterModel()
            {
                AgencyId = filters.AgencyId,
                SchoolId = filters.SchoolId,
                Workcategory = filters.Workcategory,
                PropertyId = filters.PropertyId,
                ServiceReportACID = filters.ServiceReportACID,
                ServiceReportACIDClaim = filters.ServiceReportACIDClaim,
                RWOType = filters.RWOType,
            };
            callOutFilterModel = _mapper.Map(filterModel, callOutFilterModel);
            if (dataForExport) callOutFilterModel.Take = -1; //if it is export, we need all the data
            List<JobListForHPViewModel> result = _jobService.FilteredJobDataForHP(callOutFilterModel, dataForExport, out filteredResultsCount, out totalResultsCount).ToList();
            return result;
        }

        private IEnumerable<JobListForHPViewModel> GetFilteredJobDataForHPClaim(CalloutFilterPostModel filters, bool dataForExport, out int filteredResultsCount, out int totalResultsCount)
        {
            //DataTableFilterModel<CalloutFilterPostModel> filterModel = DataTableHelper.TransformDataTablePostModel<CalloutFilterPostModel>(filters);
            DataTableFilterModel<JobListForHPViewModel> filterModel = DataTableHelper.TransformDataTablePostModel<JobListForHPViewModel>(filters);
            CallOutFilterModel callOutFilterModel = new CallOutFilterModel()
            {
                AgencyId = filters.AgencyId,
                SchoolId = filters.SchoolId,
                Workcategory = filters.Workcategory,
                PropertyId = filters.PropertyId,
                ServiceReportACID = filters.ServiceReportACID,
                ServiceReportACIDClaim = filters.ServiceReportACIDClaim,
                RWOType = filters.RWOType,
            };
            callOutFilterModel = _mapper.Map(filterModel, callOutFilterModel);
            if (dataForExport) callOutFilterModel.Take = -1; //if it is export, we need all the data
            List<JobListForHPViewModel> result = _jobService.FilteredJobDataForHPClaim(callOutFilterModel, dataForExport, out filteredResultsCount, out totalResultsCount).ToList();
            return result;
        }

        private IEnumerable<JobListForSchoolHPViewModel> GetFilteredJobDataForSchoolHP(CalloutFilterPostModel filters, bool dataForExport, out int filteredResultsCount, out int totalResultsCount)
        {
            //DataTableFilterModel<CalloutFilterPostModel> filterModel = DataTableHelper.TransformDataTablePostModel<CalloutFilterPostModel>(filters);
            DataTableFilterModel<JobListForSchoolHPViewModel> filterModel = DataTableHelper.TransformDataTablePostModel<JobListForSchoolHPViewModel>(filters);
            CallOutFilterModel callOutFilterModel = new CallOutFilterModel()
            {
                AgencyId = filters.AgencyId,
                SchoolId = filters.SchoolId,
                PropertyId = filters.PropertyId,
                Workcategory = filters.Workcategory
            };
            callOutFilterModel = _mapper.Map(filterModel, callOutFilterModel);
            if (dataForExport) callOutFilterModel.Take = -1; //if it is export, we need all the data
            IEnumerable<JobListForSchoolHPViewModel> result = _jobService.FilteredJobDataForSchoolHP(callOutFilterModel, dataForExport, out filteredResultsCount, out totalResultsCount);
            return result;
        }

        private IEnumerable<JobListForHPViewModel> GetFilteredJobApprovalDataForHP(CalloutFilterPostModel filters, bool dataForExport, out int filteredResultsCount, out int totalResultsCount)
        {
            //DataTableFilterModel<CalloutFilterPostModel> filterModel = DataTableHelper.TransformDataTablePostModel<CalloutFilterPostModel>(filters);
            DataTableFilterModel<JobListForHPViewModel> filterModel = DataTableHelper.TransformDataTablePostModel<JobListForHPViewModel>(filters);
            CallOutFilterModel callOutFilterModel = new CallOutFilterModel()
            {
                AgencyId = filters.AgencyId,
                SchoolId = filters.SchoolId,
                Workcategory = filters.Workcategory
            };
            callOutFilterModel = _mapper.Map(filterModel, callOutFilterModel);
            if (dataForExport) callOutFilterModel.Take = -1; //if it is export, we need all the data
            IEnumerable<JobListForHPViewModel> result = _jobService.FilteredJobApprovalDataForHP(callOutFilterModel, dataForExport, out filteredResultsCount, out totalResultsCount);
            return result;
        }

        public string GenerateBuidingList(IQueryable<IQueryable<string>> data)
        {
            string buidings = string.Empty;
            if (data != null)
            {
                foreach (var d in data)
                {
                    var x = d.ToList();
                    if (x != null)
                    {
                        foreach (var dx in x)
                        {
                            var xc = dx.ToList();
                            buidings = buidings + dx + ",";
                        }
                    }
                }
            }
            if (buidings != string.Empty)
            {
                return buidings.TrimEnd(',');
            }
            return buidings;
        }


    }
}
