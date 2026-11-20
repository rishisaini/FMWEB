using AssetCare.Model.Models;
using AssetCare.Model.ViewModels;
using AssetCare.Repository.Interface;
using AssetCare.Repository.Interfaces;
using AssetCare.Service.Interfaces;
using AutoMapper;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using static QRCoder.PayloadGenerator;

namespace AssetCare.Service
{
    public class JobService : IJobService
    {
        IBaseService _baseService;
        IJobRepository _jobsRepository;
        IUnitOfWork _unitOfWork;
        IPropertyRepository _propertyRepository;
        IAgencyService _agencyService;
        IBuildingService _buildingService;
        ICalloutService _callOutService;
        IContractStatusService _contractStatusService;
        IUserService _userService;
        private ICrystalReportService _crystalReportService;
        private IOrderService _orderService;
        private readonly IMapper _mapper;
        public JobService(IOrderService orderService, IUnitOfWork unitOfWork, IUserService userService, IJobRepository jobsRepository, IPropertyRepository propertyRepository,
            IAgencyService agencyService, IBuildingService buildingService, ICalloutService callOutService, IContractStatusService contractStatusService, 
            ICrystalReportService crystalReportService, IBaseService baseService, IMapper mapper)
        {
            _baseService = baseService;
            _jobsRepository = jobsRepository;
            _propertyRepository = propertyRepository;
            _unitOfWork = unitOfWork;
            _agencyService = agencyService;
            _buildingService = buildingService;
            _callOutService = callOutService;
            _contractStatusService = contractStatusService;
            _userService = userService;
            _orderService = orderService;
            _crystalReportService = crystalReportService;
            _mapper = mapper;
        }
        public JobDetails GetCallOutDetails(int agencyId, int? callOutId, int? jobId, UserViewModel user)
        {
            JobDetails details = new JobDetails();
            if (callOutId.HasValue && !jobId.HasValue)
            {
                //int agencySequencId = callOutId.Value;
                CallLogFITS call = _jobsRepository.GetCallLogFitDetails(agencyId, callOutId.Value);
                Property school = _propertyRepository.GetById(call.PropertyID);
                List<int> buildings = _jobsRepository.GetCallLogFitsBuildingIds(agencyId, call.CallLogFITSID);
                SchoolLimit schoolLimit = _jobsRepository.GetSchoolLimit(school.PropertyId);

                details = MapCallOutToJob(call, school, buildings, schoolLimit);
                //details.DetailsTab.MultipleJobsExistForSameCalloutId = _jobsRepository.MutipleJobsBySameCalloutId(agencyId, call.CallLogFITSID);

                ContractStatus contractStatus = _jobsRepository.GetContractStatusNotSignedByFMC(agencyId);
                if (contractStatus != null)
                {
                    details.ClaimMonthYear = String.Format("{0} {1}",
                        CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(contractStatus.ContractStatusMonth),
                        contractStatus.ContractStatusYear);

                }
                //details.Mode = "add";
                details.SchoolCategoryId = school.SchoolCategoryId;
                details.DetailsTab.CallLogDateVal = details.CallLogDate;

                //details.re
                return details;

            }
            else if (jobId.HasValue)
            {
                int cuurentJobId = jobId.Value;
                JobDetails jobDetails = new JobDetails();
                ServiceReport serviceReport = _jobsRepository.GetJobDetails(agencyId, jobId.Value);
                if (serviceReport != null)
                {
                    CallLogFITS call = _jobsRepository.GetCallLogFitDetails(serviceReport.CallLogFITSId);
                    Property school = _propertyRepository.GetById(call.PropertyID);
                    List<int> buildings = _jobsRepository.GetJobBuildingIds(agencyId, serviceReport.ServiceReportId);
                    SchoolLimit schoolLimit = _jobsRepository.GetSchoolLimit(school.PropertyId);
                    jobDetails = MapServiceReportToJob(serviceReport, call, school, buildings, schoolLimit);
                    //jobDetails.HasPWASignedOff = _jobsRepository.HasPWASignedOff(jobDetails.ServiceReportId.Value);
                    jobDetails.SchoolCategoryId = school.SchoolCategoryId;

                }
                //Calling Proposal for LRLV
                jobDetails.ProposalId = _jobsRepository.GetProposalDetails(serviceReport.CallLogFITSId) == null ? 0 : _jobsRepository.GetProposalDetails(serviceReport.CallLogFITSId);

                jobDetails.Estimate.MaxApprovedWorkOrderAmount = jobDetails.Estimate.EstimatedAmount > 5000 ? jobDetails.Estimate.EstimatedAmount : 5000;
                jobDetails.Estimate.ProposalId = jobDetails.ProposalId;
                jobDetails.Estimate.LRLV = jobDetails.Estimate.ProposalId == 0 ? false : true;
                if (serviceReport.WorkCategoryId == 100) //School Funded, then its LRLV Cost Centre
                {
                    jobDetails.CostCentreCode = serviceReport.Property.LrlvCostCentre;
                    
                } else if(serviceReport.WorkCategoryId == 3) //Insurance
                {
                    jobDetails.CostCentreCode = _agencyService.GetById(agencyId).InsCostCentre;
                } else
                {
                    jobDetails.CostCentreCode = _agencyService.GetById(agencyId).CostCentre;
                }

                //Get claims
                //string code = "J";
                List<Claim> claims = _jobsRepository.GetJobClaimsHistory(serviceReport.ServiceReportId, agencyId, string.Empty);
                var reimbursementLineItems = _jobsRepository.GetReimbursementLineItems(serviceReport.ServiceReportId);
                jobDetails.Claims = claims == null ? new List<Claim>() : claims;

                jobDetails.TradieSelectList = _jobsRepository.GetTradies(serviceReport.ServiceReportId);
                List<TradieViewModel> assingnedTradies = _jobsRepository.GetAssignedTradies(serviceReport.ServiceReportId);

                //jobDetails.Claims.First().
                //Total Amount is sum of the approve work items
                if (jobDetails.WorkItems != null)
                {
                    jobDetails.Claim.TotalAmount = jobDetails.WorkItems.Sum(y => y.Amount).Value;
                    jobDetails.Claim.TotalApprovedAmount = claims.Any(c => c.AMUApproved.HasValue && c.AMUApproved.Value) ? claims.Sum(x => x.ApprovedAmount).Value : 0;
                    jobDetails.Claim.TotalReimbursementAmount = claims.Any(c => c.AMUApproved.HasValue && c.AMUApproved.Value) ? reimbursementLineItems.Sum(x => x.ReimbursementAmount).Value : 0;

                    if (jobDetails.Claims.Count() > 0)
                    {
                        jobDetails.Claim.TotalAMUApproved = jobDetails.Claims.Sum(y => y.ApprovedAmount).Value;
                        jobDetails.Claim.AMUApproved = jobDetails.Claims.FirstOrDefault().AMUApproved;
                        jobDetails.Claim.FMCReviewedDate = jobDetails.Claims.FirstOrDefault().FMCReviewedDate;
                        jobDetails.Claim.AMUApprovedDate = jobDetails.Claims.FirstOrDefault().AMUApprovedDate;

                    }

                }

                // jobDetails.Claim.RemainingClaimed = jobDetails.Claim.TotalAmount - (jobDetails.Claims.Where(x => x.Approved == true).Sum(y => y.ApprovedAmount).Value + jobDetails.Claims.Where(x => x.Approved == null).Sum(y => y.ClaimedAmount));
                jobDetails.Claim.ApprovedClaimAmount = _jobsRepository.GetClaimApprovedAmount(serviceReport.ServiceReportId, agencyId);
                jobDetails.Claim.ServiceReportStatusIdForValidation = jobDetails.DetailsTab.ServiceReportStatusId;
                //jobDetails.Claim.ContractorFinaliseForValidation = jobDetails.ContractorFinalise;
                jobDetails.Claim.ClaimFinalized = serviceReport.ClaimFinalized ?? false;
                jobDetails.Claim.ClaimFinalizedDate = serviceReport.ClaimFinalizedDate;

                jobDetails.Claim.ReimbursementComplete = serviceReport.ReimbursementComplete ?? false;
                jobDetails.Claim.ReimbursementCompleteDate = serviceReport.ReimbursementCompleteDate;
                jobDetails.Claim.ReimbursementStatusDesc = GetReimbursementStatusDesc(serviceReport.ServiceReportId);
                jobDetails.Claim.ReimbursementDate = claims.FirstOrDefault()?.ApprovedByMFUDate;
                jobDetails.DetailsTab.HasAtleastOneApprovedClaim = jobDetails.Claims.Where(x => x.Approved == true).Any();
                jobDetails.DetailsTab.ClaimFinalizedForJobStatusValidation = jobDetails.Claim.ClaimFinalized;
                //jobDetails.DetailsTab.ContractorReclaimedDateForJobStatusValidation = jobDetails.ContractorReClaimDate;
                //jobDetails.DetailsTab.MultipleJobsExistForSameCalloutId = _jobsRepository.MutipleJobsBySameCalloutId(agencyId, serviceReport.CallLogFITSId.Value);
                jobDetails.DetailsTab.CallLogDateVal = jobDetails.CallLogDate;

                jobDetails.DetailsTab.SubContractorEmail = serviceReport.SubContractor != null ? serviceReport.SubContractor.Email : "N/A";
                jobDetails.DetailsTab.SubContractorMobile = serviceReport.SubContractor != null ? serviceReport.SubContractor.Mobile : "N/A";
                jobDetails.DetailsTab.SchoolSecuritySubContractor = serviceReport.SubContractor.SchoolSecurity;
                if (claims != null && claims.Count() > 0)
                {
                    //check whether PWA has signed off the month
                  //  jobDetails.HasPWASignedOffLatestClaimMonth = _jobsRepository.HasPWASignedOffLatestClaimMonth(jobDetails.AgencyId, claims.First().ClaimMonth, claims.First().ClaimYear);
                   // jobDetails.HasContractorSignedOffLatestClaimMonth = _jobsRepository.HasContractorSignedOffLatestClaimMonth(jobDetails.AgencyId, claims.First().ClaimMonth, claims.First().ClaimYear);

                    //This should be for the latest claim month
                    Claim latestClaim = claims.FirstOrDefault();
                    if (latestClaim != null && latestClaim.ClaimMonth > 0 && latestClaim.ClaimYear > 0)
                    {
                        // jobDetails.TotalApproved = jobDetails.WorkItems.Where(x => x.PWAApproved == true && x.APApprovedClaimMonth == latestClaim.ClaimMonth && x.APApprovedClaimYear == latestClaim.ClaimYear).Sum(y => y.Amount);
                        jobDetails.TotalApproved = jobDetails.WorkItems.Where(x => x.PWAApproved == true).Sum(y => y.Amount);
                        jobDetails.TotalApprovedInPreviousClaimMonths = jobDetails.WorkItems.Where(x => x.PWAApproved == true && x.APApprovedClaimMonth != latestClaim.ClaimMonth).Sum(y => y.Amount);

                    }

                }

                //calculate the total availaible claim and display only if FMCFinalised is clicked
                if (jobDetails.Claim != null && !jobDetails.Claim.ClaimFinalized)
                {
                    jobDetails.Claim.TotalAvailableToClaimThisMonth = jobDetails.WorkItems != null ?
                                                                            jobDetails.WorkItems.Where(x => x.PWAApproved != true).Sum(x => x.Amount) : 0;

                    //get last claim month's rejected items
                    //if (jobDetails.ContractorReClaim == true)
                    //{
                    //    var totalClaimedSoFar = claims.Sum(x => x.ApprovedAmount);
                    //    var totalWorkItemsSum = jobDetails.WorkItems.Sum(x => x.Amount);

                    //    jobDetails.Claim.TotalAvailableToClaimThisMonth = totalWorkItemsSum - totalClaimedSoFar;
                    //    //if there isn't any newly added work items then claimed amount is zero
                    //    if (!jobDetails.WorkItems.Where(x => (x.PWAApproved == false)).Any())
                    //    {
                    //        jobDetails.Claim.TotalAvailableToClaimThisMonth = 0;
                    //    }
                    //}
                    //else
                    //{
                    //    jobDetails.Claim.TotalAvailableToClaimThisMonth = jobDetails.WorkItems != null ?
                    //                                                        jobDetails.WorkItems.Where(x => x.PWAApproved != true).Sum(x => x.Amount) : 0;
                    //}
                }




                //ContractorSignedOff
                // jobDetails.HasContractorSignedOffCurrentMonth = _jobsRepository.HasContractorSignedOffCurrentMonth(agencyId);
                //Get
                //ContractStatus contractStatus = _jobsRepository.GetContractStatusNotSignedByFMC(agencyId);
                //if (contractStatus != null)
                //{
                //    jobDetails.ClaimMonthYear = String.Format("{0} {1}",
                //        CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(contractStatus.ContractStatusMonth),
                //        contractStatus.ContractStatusYear);

                //}


                //if the Reclaim = true and current month is more than 2, then user cannot claim
                //if (jobDetails.ContractorReClaim == true)
                //{
                //    if (jobDetails.ContractorReClaimDate != null)
                //    {
                //        DateTime claimDate = jobDetails.ContractorReClaimDate.Value;
                //        int months = ((DateTime.Now.Year - claimDate.Year) * 12) + DateTime.Now.Month - claimDate.Month;
                //        if (months > 2)
                //        {
                //            jobDetails.JobNotReclaimedFor40Days = true;
                //        }
                //    }

                //}
                //Go through workitems to check whether they can be editable
                //if (user.RoleID == "S" || user.RoleID == "A" || user.RoleID == "AG")
                //{
                //    if ((jobDetails.HasContractorSignedOffLatestClaimMonth == true && jobDetails.HasPWASignedOffLatestClaimMonth.HasValue && jobDetails.HasPWASignedOffLatestClaimMonth == false) ||
                //        (jobDetails.HasContractorSignedOffLatestClaimMonth == true && jobDetails.HasPWASignedOffLatestClaimMonth == null && user.RoleID == "A"))
                //    {
                //        ContractStatus latestFMCSignedContractStatus = _jobsRepository.GetLatestFMCSingedOffContractStatus(agencyId);
                //        if (latestFMCSignedContractStatus != null)
                //        {
                //            if (jobDetails.WorkItems != null)
                //            {
                //                foreach (JobWorkItem item in jobDetails.WorkItems)
                //                {
                //                    //if PWA has not taken any action, that means he can still approve / reject
                //                    if (item.APApprovedClaimMonth == null)
                //                    {
                //                        item.CanAction = true;
                //                    }

                //                    //if a ApprovedClaimMonth is set and it is equal to the current claim month and month is not signed off by PWA then  user can change it
                //                    if (item.APApprovedClaimMonth != null && item.APApprovedClaimMonth == latestFMCSignedContractStatus.ContractStatusMonth
                //                        && item.APApprovedClaimYear == latestFMCSignedContractStatus.ContractStatusYear
                //                        )
                //                    {
                //                        item.CanAction = true;
                //                    }

                //                    //this combination means, it belongs to a different claim month. User can change it only if it is rejected
                //                    if (item.APApprovedClaimMonth != null
                //                        && (item.APApprovedClaimMonth != latestFMCSignedContractStatus.ContractStatusMonth || item.APApprovedClaimYear != latestFMCSignedContractStatus.ContractStatusYear))
                //                    {
                //                        if (item.PWAApproved == false)
                //                        {
                //                            item.CanAction = true;
                //                        }
                //                    }



                //                }
                //            }
                //        }
                //    }
                //}

                //if (jobDetails.WorkItems != null)
                //{

                //    foreach (JobWorkItem item in jobDetails.WorkItems)
                //    {
                //        //Hack to match FM Web. This is totally not required, but has to do it to support painful old data structure
                //        if (item.HasPWAApproved == false && item.PWAApproved == false)
                //        {
                //            item.WorkItemApprovalStatus = null;
                //        }
                //        else if ((item.HasPWAApproved == true && item.PWAApproved == false))
                //        {
                //            item.WorkItemApprovalStatus = false;
                //        }
                //        if (item.HasPWAApproved == true && item.PWAApproved == true)
                //        {
                //            item.WorkItemApprovalStatus = true;
                //        }
                //    }
                //}
                return jobDetails;


            }


            return details;
        }

        private string GetReimbursementStatusDesc(int serviceReportId)
        {
            return _jobsRepository.GetReimbursementStatusDesc(serviceReportId);
        }

        public bool IsAMUUser(UserViewModel user)
        {
            if (user.RoleID == "S" || user.RoleID == "D" || user.RoleID == "P" ||
               user.RoleID == "CO" || user.RoleID == "AO")
            {
                return true;
            }

            return false;
        }

        public bool IsMFUUser(UserViewModel user)
        {
            return (user.RoleID == "E" || user.RoleID == "MO");
        }
        public List<Element> GetElementList()
        {
            return _unitOfWork.ElementRepository.GetAll().ToList();
        }

        public List<WorkType> GetWorkTypes()
        {
            return _unitOfWork.WorkTypeRepository.GetAll().ToList();
        }

        //public List<ServiceReportJobCategory> GetServiceReportJobCategories()
        //{
        //    return _unitOfWork.ServiceReportJobCategoryRepository.GetAll().ToList();

        //}

        public List<ServiceReportStatus> GetServiceReportStatus()
        {
            return _unitOfWork.ServiceReportStatusRepository.GetAll().ToList();
        }

        public List<SupportDocumentType> GetSupportDocumentTypes(int workCategoryId)
        {
            return _unitOfWork.SupportDocumentTypeRepository.GetAll().Where(x => x.SupportDocumentClassID == workCategoryId).ToList();
        }

        //public List<LCIReasons> GetLCIReasons()
        //{
        //    return _jobsRepository.GetLCIReasons();
        //}
        public bool Save(JobDetails model, UserViewModel loggedInUser, double estimationThreshold, string estimationThresholdText, out int jobId)
        {
            model.LastRecordedBy = loggedInUser.UserCode;
            model.LastRecordedDate = DateTime.Now;


            if (this.IsAMUUser(loggedInUser)  || loggedInUser.RoleID == "GL" || loggedInUser.RoleID == "SL" || loggedInUser.RoleID == "AM" || loggedInUser.RoleID == "EM")
            {
                if (model.Estimate.AMUApproved == true)
                {
                    model.Estimate.AMUApprovedDate = DateTime.Now;
                }

                if (!model.DisableField("estimateSection", "AMUApproved"))
                {
                    if(model.Estimate.AMUApproved == false)
                    {
                        model.UserCapabilities.EstimateSection += ",FMCApproved";
                    }
                }
            }


            if (loggedInUser.RoleID == "C" || loggedInUser.RoleID == "LC" )
            {
                if (model.Claim != null && model.Claim.ClaimFinalized)
                {
                    model.Claim.ClaimFinalizedDate = DateTime.Now;
                }

                if (model.Estimate.EstimateFinalised)
                {
                    model.Estimate.EstimatedBy = loggedInUser.UserCode;
                    model.Estimate.EstimateDate = DateTime.Now;
                }

                if (model.DetailsTab.SubContractorAccepted.HasValue)
                {
                    model.DetailsTab.SubContractorRespondedDate = DateTime.Now;
                    model.DetailsTab.SubContractorRespondedBy = loggedInUser.UserCode;
                    model.UserCapabilities.DetailSection += ",ServiceReportStatusId"; //Allow saving WO Status
                    if (model.DetailsTab.ServiceReportStatusId == 1 )
                    {
                        //Set WO status To Local Contractor Accepted /Rejected
                        model.DetailsTab.ServiceReportStatusId = model.DetailsTab.SubContractorAccepted == true ? 2 : 3; 
                    }
                }
            }

            if (loggedInUser.RoleID == "C" || loggedInUser.RoleID == "A" || loggedInUser.RoleID == "AG")
            {
                //If FMC Reject the Estimate, then untick EstimateFinalised check box
                if (!model.DisableField("estimateSection", "FMCApproved"))
                {
                    if (model.Estimate.FMCApproved == false)
                    {
                        model.UserCapabilities.EstimateSection += ",EstimateFinalised" + (model.Estimate.OriginalEstimate > 0 ? "" : ",OriginalEstimate");
                        model.Estimate.OriginalEstimate = model.Estimate.EstimatedAmount;
                    }
                }
            }
            int callLogFitsId = model.CallLogFITSId.HasValue ? model.CallLogFITSId.Value : 0;

            if (!model.DisableField("detailSection","ExpectedFirstVisitTime") && model.DetailsTab.ExpectedFirstVisitTime.HasValue && model.DetailsTab.ExpectedVisitDate.HasValue)
            {
                model.DetailsTab.ExpectedVisitDate =  model.DetailsTab.ExpectedVisitDate.Value.AddHours(model.DetailsTab.ExpectedFirstVisitTime.Value.Hour).AddMinutes(model.DetailsTab.ExpectedFirstVisitTime.Value.Minute);
            }

            //if (loggedInUser.RoleID == "E" || loggedInUser.RoleID == "MO" || loggedInUser.RoleID == "A" || loggedInUser.RoleID == "AG")
            //{
            //    if (model.Claim != null && model.Claim.ReimbursementComplete)
            //    {
            //        model.Claim.ReimbursementCompleteDate = DateTime.Now;
            //    }
            //}

            var tradieChanges = new List<TradieChange>();
            var oldList = (model.DetailsTab.OriginalAssignedTradieIds ?? "").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => int.Parse(x.Trim())).ToList();
            var newList = (model.DetailsTab.AssignedTradieIds ?? "").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => int.Parse(x.Trim())).ToList();

            

            foreach (var id in newList.Except(oldList))
            {
                tradieChanges.Add(new TradieChange
                {
                    TradieId = id,
                    Action = TradieChangeAction.Add
                });
            }

            foreach (var id in oldList.Except(newList))
            {
                tradieChanges.Add(new TradieChange
                {
                    TradieId = id,
                    Action = TradieChangeAction.Remove
                });
            }

            bool calloutCategeoryChanged = false;
            bool status = _jobsRepository.Save(model, loggedInUser.UserCode, tradieChanges, out jobId, out calloutCategeoryChanged, out bool estimateFMCEndorseChanged, out bool estimateAMUEndorseChanged);
            model.AgencySequenceId = jobId;
            //model.QRCode = GenerateQRCode(model.QRCodeId);

            //sendQRCodeEmail(model, loggedInUser);
            //sendContractorDocumentsFinalisedEmail(model, loggedInUser);
            if (status && calloutCategeoryChanged && callLogFitsId != 0) //notify IRCM when the callout's category changed by WO
            {
                var calloutModel = _callOutService.GetCallOutDetails(callLogFitsId, model.AgencyId, estimationThreshold);
                _callOutService.SendCalloutCategoryChanged(calloutModel, loggedInUser);
            }
            if (status && callLogFitsId != 0 && 
                IsInsuranceWO(model) &&
                _callOutService.IsCalloutTotalEstimationOverLimit(callLogFitsId, model.AgencyId, estimationThreshold)) //notify IRCM when the total estimation of work orders over limit
            {
                var calloutModel = _callOutService.GetCallOutDetails(callLogFitsId, model.AgencyId, estimationThreshold);
                if (calloutModel.TotalEstimationOverLimitNotificationDate == null) // ensure we has not sent before
                {
                    bool isSent = _callOutService.SendCalloutTotalEstimationOverLimit(calloutModel, loggedInUser, estimationThresholdText);
                    if (isSent)
                        _callOutService.UpdateTotalEstimationOverLimitNotificationDate(calloutModel);
                }
            }
            sendEstimateTabEmails(model, loggedInUser, estimateFMCEndorseChanged, estimateAMUEndorseChanged);
            // sendReclaimEmailToContractors(model, loggedInUser);
            return true;

        }

        private bool IsInsuranceWO(JobDetails model)
        {
            return (model.IsInsuranceEvent == true || model.IsInsuranceEventByWO == true);
        }

        private void sendReclaimEmailToContractors(int agencyId, int claimMonth, int claimYear)
        {
            string jobIds = _jobsRepository.GetAllReclaimJobs(agencyId, claimMonth, claimYear);
            if (!string.IsNullOrEmpty(jobIds))
            {
                Agency agencyDetails = _agencyService.GetById(agencyId);
                AgencyContractor agencyContractor = _agencyService.GetById(agencyId).AgencyContractor;
                EmailService emailHelper = new EmailService();
                Dictionary<string, string> data = new Dictionary<string, string>();
                data.Add("JobIds", jobIds);
                string title = string.Format("Jobs Partially approved for {0}/{1}", claimMonth.ToString(), claimYear.ToString());
                emailHelper.SendMail(string.Format("{0};{1}", agencyContractor.Email, agencyDetails.ContactEmail), data, title, "JobReclaim.html");
            }

        }

        private JobDetails MapServiceReportToJob(ServiceReport serviceReport, CallLogFITS call, Property school, List<int> buildings, SchoolLimit schoolLimit)
        {
            //ServiceReport -> Job
            JobDetails job = _mapper.Map<JobDetails>(serviceReport);
            JobDetailsTab tab = _mapper.Map<JobDetailsTab>(serviceReport);
            JobEstimate estimate = _mapper.Map<JobEstimate>(serviceReport);

            //if (estimate != null)
            //{
            //    ServiceReportApprovalType saprType = GetServiceReportApproavalTypes()
            //          .Where(x => x.ServiceReportApprovalTypeID == estimate.ApprovalType)
            //          .FirstOrDefault();
            //    if (saprType != null)
            //    {
            //        estimate.ApprovalTypeDesc = saprType.ApprovalType;
            //    }
            //}

            job.CallOutId = call.AgencySequenceID.Value;
            job.InsuranceIncidentDateTime = call.IncidentDate;
            job.IncidentDate = job.InsuranceIncidentDateTime;
            job.IncidentTime = job.InsuranceIncidentDateTime.HasValue ? job.InsuranceIncidentDateTime.Value.ToString("HH:mm") : null;
            //if(!job.IsInsuranceEvent.HasValue)
            //{
            //    //if it is not an insurance event at the WO level, then update from CallLog
            //    job.IsInsuranceEvent = call.IsInsuranceEvent;
            //}


            job.DetailsTab = tab;
            job.DetailsTab.RequestedBy = call.RequestedBy;
            job.DetailsTab.RequesterContactNumber = call.RequesterContact;
            job.DetailsTab.SiteContact = call.SiteContact;
            job.DetailsTab.SiteContactPerson = call.SiteContactPerson;
            job.DetailsTab.SubContractorName = serviceReport.SubContractor.Name;
            job.DetailsTab.ExpectedFirstVisitTime = job.DetailsTab.ExpectedVisitDate;// job.DetailsTab.ExpectedVisitDate.HasValue ? job.DetailsTab.ExpectedVisitDate.Value : DateTime.Today.AddHours(9);
          
            job.ProcessedBy = call.ProcessedBy;
            job.AgencyContactNumber = call.Agency.ContactPhone;
            job.WorkTypeDesc = serviceReport.WorkType?.Description;

            if (!string.IsNullOrEmpty(job.AssociatedJobID))
            {
                int associatedJobId = 0;
                if (int.TryParse(job.AssociatedJobID, out associatedJobId))
                {
                    ServiceReport associatedJob = _jobsRepository.GetJobDetails(job.AgencyId, associatedJobId);
                    if (associatedJob != null)
                    {
                        job.AssociatedJobSequenceID = associatedJob.AgencySequenceId;
                    }
                }
            }

            // job.IsInsuranceCallout = call.IsInsuranceEvent;

            job.Estimate = estimate;
            // job.Estimate.FeeAmount = job.Estimate.FeeAmount ?? 0;
            // job.Estimate.FeeOther = job.Estimate.FeeOther ?? 0;
            //if (job.Estimate.OrderId.HasValue)
            //{
            //    job.Estimate.OrderIdDesc = _jobsRepository.GetOrderDetails(job.Estimate.OrderId.Value).Description;
            //}

            if (job.ElementId != null && call.ElementId > 0)
            {
                job.Element = _unitOfWork.ElementRepository.GetAll().Where(x => x.ElementID == job.ElementId).FirstOrDefault().ElementDesc;
            }

            if (call.CallLogTypeID != null && call.CallLogTypeID.Value > 0)
            {
                job.CallLogType = _unitOfWork.CallLogTypeRepository.GetAll().Where(x => x.CallLogTypeID == call.CallLogTypeID.Value).FirstOrDefault().CallLogTypeDesc;
            }


            //ServiceReportQRCode srvQRCode = _jobsRepository.GetQRCode(job);
            //job.QRCodeId = srvQRCode.Code;
            //job.QRCode = GenerateQRCode(srvQRCode.Code);
            job.Area = school.EducationArea != null ? school.EducationArea.EducationAreaDesc : "";
            job.CallLogDate = call.CallDate.Value;

            //job.NominatedYN = school.NominatedYN;
            //job.EquipmentTypeBSEMAssemblyID = call.EquipmentTypeBSEMAssemblyID;

            job.JobBuildingIDs = buildings;

            job.Phone = school.Phone;
            //job.PropertyId = school.SchoolId.Value;

            job.RoomLocation = call.RoomLocation;
            //get this details from callLogFits if it is a new Job
            //if (job.AgencySequenceId == null || job.AgencySequenceId == 0)
            //{
            //    job.MaintExtraTaskId = call.MaintenanceTaskCallLogID;
            //    job.RoomNumber = call.RoomNumber;
            //    job.ServiceReportDesc = call.CallLogDesc;
            //    job.SchEUR100 = call.SchEUR100 ?? false;
            //    job.TreeSafetyIssue = call.TreeSafetyIssue ?? false;
            //    job.EUR100 = call.EUR100 ?? false;
            //    job.DetailsTab.ContractorWONo = call.ContractorWONo;
            //    job.DetailsTab.Comments = call.Comments;
            //    job.DetailsTab.CallLogFinanceUrgencyId = call.CallLogFinanceUrgencyID;
            //    job.DetailsTab.CallLogFinanceUrgencyDesc = call.CallLogFinanceUrgency.CallLogFinanceUrgencyDesc;
            //    job.CommerceNotInvolved = call.CommerceNotInvolved ?? false;
            //}
            //else
            //{
            //    //if it is existing job, then take from job
            //    job.DetailsTab.CallLogFinanceUrgencyDesc = serviceReport.CallLogFinanceUrgency.CallLogFinanceUrgencyDesc;
            //}



            job.SchoolId = school.SchoolId;
            job.SchoolEmail = school.Email;
            job.SchoolLimit = schoolLimit != null ?
                                            decimal.Round(schoolLimit.SchoolLimitAmount.Value, 2, MidpointRounding.AwayFromZero) : 0;
            job.SchoolName = school.PropertyShortDesc;
            job.SchoolAddress = String.Format("{0}, {1}, {2}, {3} {4} ", school.PropertyDesc, school.Street, school.Suburb, school.State, school.Postcode);  
            job.ASOEmail = school.ASOEmail;
            job.ASOName = school.ASOName;
            job.ASOContactNumber = school.ASOContactNumber;
            job.GLName = school.GLName;
            job.GLContactNumber = school.GLContactNumber;

            job.SupportDocuments = new List<SupportDocumentViewModel>();
            job.SupportQuoteDocuments = new List<SupportDocumentViewModel>();
            job.SupportDocumentsCallout = new List<SupportDocumentViewModel>();
            job.SupportDocumentClassID = 5;


            job.WorkCategorySchId = serviceReport.WorkCategoryId;
            job.WorkCategorySchDesc = serviceReport.WorkCategory.Description + " (" + serviceReport.WorkCategory.Code + ")";

            job.WorkCategoryDesc = serviceReport.WorkCategory.Description;
            job.WorkTypeHelpText = serviceReport.WorkType != null ? serviceReport.WorkType.HelpText : "";

            job.SchoolResponseTimeDesc = _unitOfWork.CallLogFinanceUrgencyRepository.GetAll().Where(x => x.CallLogFinanceUrgencyID == call.SchoolUrgencyID).FirstOrDefault().CallLogFinanceUrgencyDesc;
            job.FMCResponseTimeDesc = call.CallLogFinanceUrgency?.CallLogFinanceUrgencyDesc;


            //if (job.MaintExtraTaskId != null && job.MaintExtraTaskId > 0)
            //{
            //    var maintTasks = _unitOfWork.MaintenanceTasksCallLogRepository.GetAll().
            //         Where(x => x.MaintenanceTaskCallLogID == job.MaintExtraTaskId).FirstOrDefault();
            //    if (maintTasks != null)
            //    {
            //        job.MaintExtraTaskDesc = maintTasks.MaintenanceTaskCallLogDesc;
            //    }
            //}

//if (job.EquipmentTypeBSEMAssemblyID != null && job.EquipmentTypeBSEMAssemblyID > 0)
//{
//    var equpiments = _unitOfWork.AssemblyEquipmentTypesRepository.GetAll().
//                       Where(x => x.EquipmentTypeBSEMAssemblyID == job.EquipmentTypeBSEMAssemblyID).FirstOrDefault();
//    if (equpiments != null)
//    {
//        job.AssemblyEquipmentDesc = equpiments.EquipmentTypeBSEMAssemblyDesc;
//    }
//}


//job.Tradies = _jobsRepository.GetTradies(serviceReport.ServiceReportId);
//List<Tradie> tradies = _jobsRepository.GetTradies(serviceReport.ServiceReportId);
//job.Tradies =  AutoMapper.Mapper.Map<List<TradieViewModel>>(tradies);

job.DetailsTab.AssignedTradies = _jobsRepository.GetAssignedTradies(serviceReport.ServiceReportId);
job.DetailsTab.OriginalAssignedTradieIds = string.Join(",", job.DetailsTab.AssignedTradies.Select(x => x.ID));
job.DetailsTab.AssignedTradieIds = job.DetailsTab.OriginalAssignedTradieIds;

                // AssignedTradieIds

                //if (!string.IsNullOrEmpty(job.DetailsTab.AssignedTradieIds))
                //{
                //    string[] assignedTradieIdList = job.DetailsTab.AssignedTradieIds.Split(',');
                //    List<string> assginedTradieNameList = new List<string>();
                //    foreach (string tradieId in assignedTradieIdList)
                //    {
                //        Tradie tradie = _jobsRepository.GetTradieDetails(Convert.ToInt32(tradieId));
                //        if (tradie != null)
                //        {
                //            assginedTradieNameList.Add(string.Format("{0} {1}", tradie.User?.FirstName, tradie.User?.LastName));

                //        }
                //    }

                //    if (assginedTradieNameList.Count() > 0)
                //    {
                //        job.DetailsTab.AssginedTradieNames = string.Join(", ", assginedTradieNameList);
                //    }
                //}

                //Get Callout Documents
                List<SupportDocument> calloutDocs = _jobsRepository.GetSupportDocuments((int)SupportDocumentClassEnum.CALLEUR, serviceReport.CallLogFITSId.Value);
job.SupportDocumentsCallout = calloutDocs.Select(x => new SupportDocumentViewModel()
{
    AgencyID = serviceReport.AgencyId,
    AttachmentLink = x.AttachmentLink,
    AttachmentText = x.AttachmentText,
    PropertyName = job.SchoolName,
    DocumentSequenceID = x.DocumentSequenceID,
    SupportDocumentTypeDesc = _jobsRepository.GetSupportDocumentType(x.SupportDocumentTypeID),
    RecordedDate = x.RecordedDate,
    SupportDocumentClassID = x.SupportDocumentClassID,
    MasterRecordID = x.MasterRecordID,
    SupportDocumentID = x.SupportDocumentID,
    SupportDocumentTypeID = x.SupportDocumentTypeID,
    ClientDocumentName = x.ClientDocumentName

}).ToList(); ;
//Get Work Order documents
List<SupportDocument> docs = _jobsRepository.GetSupportDocuments((int)SupportDocumentClassEnum.WORKORDER, serviceReport.ServiceReportId);



job.SupportDocuments = docs.Select(x => new SupportDocumentViewModel()
{
    AgencyID = serviceReport.AgencyId,
    AttachmentLink = x.AttachmentLink,
    AttachmentText = x.AttachmentText,
    PropertyName = job.SchoolName,
    DocumentSequenceID = x.DocumentSequenceID,
    SupportDocumentTypeDesc = _jobsRepository.GetSupportDocumentType(x.SupportDocumentTypeID),
    RecordedDate = x.RecordedDate,
    SupportDocumentClassID = x.SupportDocumentClassID,
    MasterRecordID = x.MasterRecordID,
    SupportDocumentID = x.SupportDocumentID,
    SupportDocumentTypeID = x.SupportDocumentTypeID,
    ClientDocumentName = x.ClientDocumentName

}).ToList();

job.SupportDocument = new SupportDocumentViewModel();

//Get Quote documents
List<SupportDocument> quoteDocs = _jobsRepository.GetSupportDocuments((int)SupportDocumentClassEnum.WORKORDER_QUOTE, serviceReport.ServiceReportId);
job.SupportQuoteDocuments = quoteDocs.Select(x => new SupportDocumentViewModel()
{
    AgencyID = serviceReport.AgencyId,
    AttachmentLink = x.AttachmentLink,
    AttachmentText = x.AttachmentText,
    PropertyName = job.SchoolName,
    DocumentSequenceID = x.DocumentSequenceID,
    SupportDocumentTypeDesc = _jobsRepository.GetSupportDocumentType(x.SupportDocumentTypeID),
    RecordedDate = x.RecordedDate,
    SupportDocumentClassID = x.SupportDocumentClassID,
    MasterRecordID = x.MasterRecordID,
    SupportDocumentID = x.SupportDocumentID,
    SupportDocumentTypeID = x.SupportDocumentTypeID,
    ClientDocumentName = x.ClientDocumentName

}).ToList();


if (serviceReport.WorkCategoryId == 1 || serviceReport.WorkCategoryId == 3 || serviceReport.WorkCategoryId == 100 || (serviceReport.WorkCategoryId == 13 && call.Version == 2))
{
    //get work items if it is an EUR job
    //List<ServiceReportLineItem> serviceReportItems = _jobsRepository.GetServiceReportLineItems(serviceReport.ServiceReportId);
    //List<JobWorkItem> workItems = new List<JobWorkItem>();
    //foreach (ServiceReportLineItem item in serviceReportItems)
    //{
    //    JobWorkItem jobWorkItem = AutoMapper.Mapper.Map<JobWorkItem>(item);
    //    jobWorkItem.ScheduleItemNo = item.ScheduledRate.ScheduleItemNo;
    //    //ScheduledRate d = item.ScheduledRate;
    //    //if(d != null)
    //    //{
    //    //    string ScheduledRateDesc = "Item " + (d.ScheduleItemNo == null ? "" : d.ScheduleItemNo) + "--" +
    //    //    (d.Description.Length > 60 ? (d.Description.Substring(0, 60) + "...") : d.Description);
    //    //}


    //    workItems.Add(jobWorkItem);
    //}
    var workItems = (
                        from srli in _jobsRepository.GetServiceReportLineItems(serviceReport.ServiceReportId)
                        join rli in _jobsRepository.GetReimbursementLineItems(serviceReport.ServiceReportId) on srli.ServiceReportLineItemID equals rli.ServiceReportLineItemID into srrlis
                        from srrli in srrlis.DefaultIfEmpty()
                        select new JobWorkItem
                        {
                            ServiceReportLineItemID = srli.ServiceReportLineItemID,
                            ScheduleItemNo = srli.ScheduledRate.ScheduleItemNo,
                            WorkDate = srli.WorkDate,
                            ScheduleRateID = srli.ScheduleRateID,
                            //CanAction
                            LineNumber = srli.LineNumber,
                            SRLIDescription = srli.SRLIDescription,
                            WorkCategoryID = srli.WorkCategoryID,
                            TradeName = srli.TradeName,
                            MSRReference = srli.MSRReference,
                            CostAdjPercent = srli.CostAdjPercent,
                            Quantity = srli.Quantity,
                            Rate = srli.Rate,
                            Unit = srli.Unit,
                            Amount = srli.Amount,
                            HoursWorked = srli.HoursWorked,
                            ClaimedAmount = srli.ClaimedAmount,
                            ClaimedGST = srli.ClaimedGST,
                            ApprovedAmount = srli.ApprovedAmount,
                            ApprovedGST = srli.ApprovedGST,
                            HasPWAApproved = srli.HasPWAApproved,
                            Reason = srli.Reason,
                            PWAApproved = srli.PWAApproved,
                            ReportedBy = srli.ReportedBy,
                            ReportedDate = srli.ReportedDate,
                            LCIFlag = srli.LCIFlag,
                            PWAApprovedBy = srli.PWAApprovedBy,
                            PWAApprovedDate = srli.PWAApprovedDate,
                            PropertyId = srrli?.ServiceReport?.PropertyId ?? 0,
                            //CanDelete
                            APApprovedClaimMonth = srli.APApprovedClaimMonth,
                            APApprovedClaimYear = srli.APApprovedClaimYear,
                            Deleted = srli.Deleted,
                            //WorkItemApprovalStatus
                            PWARejected = srli.PWARejected,
                            ReimbursementStatusID = srrli?.ReimbursementStatusID ?? null,
                            IRCMReimbursementAmount = srrli?.ReimbursementAmount ?? null,
                            IRCMComments = srrli?.Comments ?? null,
                            IRCMReimbursementDate = srrli?.ReimbursementDate ?? null
                        }
                    ).ToList();

    job.WorkItems = workItems;
}



return job;
}

private JobDetails MapCallOutToJob(CallLogFITS call, Property school, List<int> buildings, SchoolLimit schoolLimit)
{
JobDetails job = new JobDetails();
job.Estimate = new JobEstimate();
job.WorkItems = new List<JobWorkItem>();
//For EUR +5
if (call.WorkCategoryID == 1)
{
    job.DetailsTab.EstimatedCompletionDate = call.CallDate.HasValue ? call.CallDate.Value.AddDays(7) : DateTime.Now.AddDays(5);
}
else
{
    //FOR MET/REA +3
    job.DetailsTab.EstimatedCompletionDate = call.CallDate.HasValue ? call.CallDate.Value.AddDays(5) : DateTime.Now.AddDays(5);

}




job.Version = call.Version;
job.CallOutId = call.AgencySequenceID.Value;
job.AgencyId = call.AgencyID.Value;
job.CallLogFITSId = call.CallLogFITSID;
job.Area = school.EducationArea != null ? school.EducationArea.EducationAreaDesc : "";
job.CallLogDate = call.CallDate.Value;
if (call.ElementId != null && call.ElementId > 0)
{
    job.Element = _unitOfWork.ElementRepository.GetAll().Where(x => x.ElementID == call.ElementId).FirstOrDefault().ElementDesc;
}

int? callLogTypeId = call.CallLogTypeID;
if(callLogTypeId != null && callLogTypeId > 0)
{
    job.CallLogType = _unitOfWork.CallLogTypeRepository.GetAll().Where(x => x.CallLogTypeID == callLogTypeId.Value).FirstOrDefault().CallLogTypeDesc;
}

job.JobBuildingIDs = buildings;

job.Phone = school.Phone;
job.PropertyId = school.PropertyId;
job.SchoolId = school.SchoolId;
job.RoomLocation = call.RoomLocation;
job.RoomNumber = call.RoomNumber;
job.SchoolEmail = school.Email;
job.SchoolLimit = schoolLimit != null ?
                                decimal.Round(schoolLimit.SchoolLimitAmount.Value, 2, MidpointRounding.AwayFromZero) : 0;
job.SchoolName = school.PropertyShortDesc;
job.SchoolAddress = school.PropertyShortDesc + ", " + school.Street + ", " + school.Suburb + ", " + school.State + ", " + school.Postcode;//school.Address;
job.ServiceReportDesc = call.CallLogDesc;

job.SupportDocuments = new List<SupportDocumentViewModel>();
job.SupportQuoteDocuments = new List<SupportDocumentViewModel>();
job.SupportDocumentClassID = 5;

job.WorkCategorySchId = call.SchoolWorkCategory.WorkCategoryId;
job.WorkCategorySchDesc = call.SchoolWorkCategory.Description + " (" + call.SchoolWorkCategory.Code + ")";

job.WorkCategoryId = call.WorkCategory.WorkCategoryId;
job.WorkCategoryDesc = call.WorkCategory.Description;

job.DetailsTab.ContractorWONo = call.ContractorWONo;
job.DetailsTab.Comments = call.Comments;
job.DetailsTab.AMUComments = string.Empty;
job.DetailsTab.CallLogFinanceUrgencyId = call.CallLogFinanceUrgencyID;            
job.WorkTypeId = call.WorkTypeId;

job.SupportDocument = new SupportDocumentViewModel();
job.TradieSelectList = _mapper.Map<List<TradieViewModel>>(_jobsRepository.GetTradiesByCallout(call.CallLogFITSID));

return job;

}

public IEnumerable<JobListViewModel> FilteredJobData(CallOutFilterModel callOutFilterModel, bool dataForExport, out int filteredResultsCount, out int totalResultsCount)
{

var userPropertyIds = _userService.GetByUserCode(_baseService.GetCurrentUser().UserCode, callOutFilterModel.AgencyId).UserPropertyIds;
var userRole = _baseService.CurrentUserRole;
return _jobsRepository.FilterJobData(callOutFilterModel, dataForExport, userPropertyIds, userRole, out filteredResultsCount, out totalResultsCount);

}

public IEnumerable<JobListForHPViewModel> FilteredJobDataForHP(CallOutFilterModel callOutFilterModel, bool dataForExport, out int filteredResultsCount, out int totalResultsCount)
{
using (var transaction = _unitOfWork.BeginSnapshotTransaction())
{
    var userPropertyIds = _userService.GetByUserCode(_baseService.GetCurrentUser().UserCode).UserPropertyIds;
    var userRole = _baseService.CurrentUserRole;
    transaction.Commit();

    return _jobsRepository.FilterJobDataForHP(callOutFilterModel, dataForExport, userPropertyIds, Convert.ToInt32(_baseService.GetCurrentUser().DefaultLocalContractorId), userRole, out filteredResultsCount, out totalResultsCount);
}

}

public IEnumerable<JobListForHPViewModel> FilteredJobDataForHPClaim(CallOutFilterModel callOutFilterModel, bool dataForExport, out int filteredResultsCount, out int totalResultsCount)
{
using (var transaction = _unitOfWork.BeginSnapshotTransaction())
{
    var userPropertyIds = _userService.GetByUserCode(_baseService.GetCurrentUser().UserCode).UserPropertyIds;
    var userRole = _baseService.CurrentUserRole;
    transaction.Commit();

    return _jobsRepository.FilterJobDataForHPClaim(callOutFilterModel, dataForExport, userPropertyIds, Convert.ToInt32(_baseService.GetCurrentUser().DefaultLocalContractorId), userRole, out filteredResultsCount, out totalResultsCount);
}

}

public IEnumerable<JobListForHPViewModel> FilteredJobApprovalDataForHP(CallOutFilterModel callOutFilterModel, bool dataForExport, out int filteredResultsCount, out int totalResultsCount)
{
using (var transaction = _unitOfWork.BeginSnapshotTransaction())
{
    //Filter result with user properties
    var userPropertyIds = _userService.GetByUserCode(_baseService.GetCurrentUser().UserCode, callOutFilterModel.AgencyId).UserPropertyIds;
    var userRole = _baseService.CurrentUserRole;
    transaction.Commit();

    return _jobsRepository.FilterJobApprovalDataForHP(callOutFilterModel, dataForExport, userPropertyIds, userRole, out filteredResultsCount, out totalResultsCount);
}
}

public IEnumerable<JobListForSchoolHPViewModel> FilteredJobDataForSchoolHP(CallOutFilterModel callOutFilterModel, bool dataForExport, out int filteredResultsCount, out int totalResultsCount)
{
var userPropertyIds = _userService.GetByUserCode(_baseService.GetCurrentUser().UserCode, callOutFilterModel.AgencyId).UserPropertyIds;
var userRole = _baseService.CurrentUserRole;
return _jobsRepository.FilterJobDataForSchoolHP(callOutFilterModel, _baseService.GetCurrentUser(), dataForExport, userPropertyIds, userRole, out filteredResultsCount, out totalResultsCount);

}

public void GetJobDetails(JobDetails details)
{
details.Estimate = new JobEstimate();
details.Estimate.LimitStatus = 2;
}

public List<ServiceReportApprovalType> GetServiceReportApproavalTypes()
{
return _unitOfWork.ServiceReportApprovalTypesRepository.GetAll().ToList();

}

public List<ScheduledRateCategory> GetScheduleRateCatgories()
{
return _unitOfWork.ScheduledRateCategoryRepository.GetAll().ToList();
}

public List<ScheduleRateDropdownItem> GetScheduledRates(int agencyId, int? schduleRateCategoryId, int? subContractorId)
{
return _jobsRepository.GetScheduledRates(agencyId, schduleRateCategoryId, subContractorId);
}

public string GetBuildingList(JobDetails model, bool NoOnly = false)
{
List<string> buildingList = new List<string>();
foreach (int buildingId in model.JobBuildingIDs)
{

    var b = _buildingService.GetByProperty(model.PropertyId)
        .Where(x => x.BuildingID == buildingId)
        .FirstOrDefault();
    if (b != null)
    {

        buildingList.Add(NoOnly ? b.BuildingNo : "(" + b.BuildingNo + ") " + b.BuildingDesc + ((b.RemovedYN.HasValue && b.RemovedYN.Value) ? " (REMOVED)" : ""));
    }

}

return String.Join(",", buildingList);
}

private void sendContractorDocumentsFinalisedEmail(JobDetails model, UserViewModel currentUser)
{
if (currentUser.RoleID == "C" )//&& model.DocumentsFinalised.HasValue && model.DocumentsFinalised.Value && model.Claim.ClaimFinalized)
{
    Agency agency = _agencyService.GetById(model.AgencyId);
    string toEmail = string.Empty;
    EmailService emailHelper = new EmailService();
    Dictionary<string, string> data = new Dictionary<string, string>();
    string jobId = model.AgencySequenceId.HasValue ? model.AgencySequenceId.ToString() : "Auto Generated Once Saved";
    data.Add("JobId", jobId);
    data.Add("AgencyId", model.AgencyId.ToString());
    data.Add("RequestedBy", currentUser.FullName);
    data.Add("SchoolName", model.SchoolName);
    data.Add("Phone", model.Phone);
    data.Add("Region", agency.AgencyName);
    //get agency contract
    AgencyContractor agencyContractor = _agencyService.GetById(model.AgencyId).AgencyContractor;
    if (agencyContractor != null)
    {
        data.Add("AgencyEmail", agencyContractor.Email);
        toEmail = agencyContractor.Email;
    }


    data.Add("BuildingList", GetBuildingList(model));
    data.Add("Rooms", model.RoomNumber);
    data.Add("RoomLocation", model.RoomLocation);
    data.Add("Area", model.Area);

    data.Add("SchoolEmail", model.SchoolEmail);
    toEmail = toEmail + ";" + model.SchoolEmail + ";" + currentUser.Email;

    string workCategory = _callOutService.GetWorkCategory()
        .Where(x => x.WorkCategoryId == model.WorkCategoryId)
        .FirstOrDefault().Description;

    data.Add("WorkCategoryDesc", workCategory);

    var request = HttpContext.Current.Request;
    data.Add("Link", string.Format("{0}://{1}{2}?jobId={3}", request.Url.Scheme, request.Url.Authority, request.Url.AbsolutePath, model.ServiceReportId));

    string urgency = _callOutService.GetFinanceUrgency()
        .Where(x => x.CallLogFinanceUrgencyID == model.DetailsTab.CallLogFinanceUrgencyId)
        .FirstOrDefault()
        .CallLogFinanceUrgencyDesc;
    string subject = "FMWEB Documents Finalised for Job Id: " + jobId;

    emailHelper.SendMail(toEmail, data, subject, "JobFinalised.html");
}
}

private void sendQRCodeEmail(JobDetails model, UserViewModel currentUser)
{
Agency agency = _agencyService.GetById(model.AgencyId);
Dictionary<string, string> additionalData = new Dictionary<string, string>();

if (model.DetailsTab.TradieId.HasValue)
{
    Tradie tradie = _jobsRepository.GetTradieDetails(model.DetailsTab.TradieId.Value);
    additionalData.Add("TradieName", string.Format("{0} {1}", tradie.User?.FirstName, tradie.User?.LastName));
}

if (model.DetailsTab.SubContractorId.HasValue)
{
    SubContractor subbie = _jobsRepository.GetSubContractorDetails(model.DetailsTab.SubContractorId.Value);
    additionalData.Add("LocalContractor", subbie.Name);

}

if (!string.IsNullOrEmpty(model.QRCode))
{
    additionalData.Add("QRCode", "data:image/png;base64," + model.QRCode);
    string subject = "School checkin QR Code for Work Order: " + model.AgencySequenceId;
    sendEmail(model, currentUser, agency.AMUEmail, "JobStatusEmail.html", subject, "", overwriteData: additionalData);
}

}

public string GenerateQRCode(Guid qrCodeServiceReport)
{
var request = HttpContext.Current.Request;
Url generator = new Url(string.Format("{0}://{1}{2}/ServiceReportQRCodeScan?Code={3}", request.Url.Scheme, request.Url.Authority, request.ApplicationPath, qrCodeServiceReport));

string payload = generator.ToString();
QRCodeGenerator qrGenerator = new QRCodeGenerator();
QRCodeData qrCodeData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
QRCode qrCode = new QRCode(qrCodeData);
System.Drawing.Bitmap qrCodeAsBitmap = qrCode.GetGraphic(5, System.Drawing.Color.DarkBlue, System.Drawing.Color.White, true);
byte[] qrCodeByteArray;
using (MemoryStream stream = new MemoryStream())
{
    qrCodeAsBitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
    qrCodeByteArray = stream.ToArray();
}
string qrCodeBase64 = Convert.ToBase64String(qrCodeByteArray);
return qrCodeBase64;
}
public bool ReviewClaims(WorkOrderApprovalListItem item, string usercode)
{
return _jobsRepository.ReviewClaims(item, usercode);

}

public bool ApproveClaims(WorkOrderApprovalListItem data, string usercode, bool amuRegion)
{            
return _jobsRepository.ApproveClaims(data, usercode, amuRegion);          
}

//public bool ReviewInsuranceClaims(InsuranceReimbursementListItem item, string usercode)
//{
//    return _jobsRepository.ReviewInsuranceClaims(item, usercode);

//}

public bool ApproveInsuranceReimbursement(InsuranceReimbursementListItem data, string usercode)
{
return _jobsRepository.ApproveInsuranceReimbursement(data, usercode);
}

private void sendEstimateTabEmails(JobDetails model, UserViewModel currentUser,bool estimateFMCEndorseChanged, bool estimateAMUEndorseChanged)
{
string subject = string.Empty;
string title = string.Empty;
var job = _jobsRepository.GetJobDetails(model.AgencyId, model.ServiceReportId.Value);
string jobCode = job.WorkCategory != null ? job.WorkCategory.Description : ""; //model.WorkCategoryId == 1 ? "EUR" : (model.WorkCategoryId == 13 ? "REA" : "MET");
Agency agency = null;
if (currentUser.RoleID == "LC" && model.DetailsTab.SubContractorAccepted != null && model.UserCapabilities.DetailSection.Contains("SubContractorAccepted"))
{
    //Contractor finalised the estimate
    subject = string.Format("FMWEB 2.0 {0} Work Order {1} for {2} is {3}", jobCode, model.AgencySequenceId, model.SchoolName, (model.DetailsTab.SubContractorAccepted == true ? "ACCEPTED" : "REJECTED"));
    title = "The Following FMWEB 2.0 Work Order is " + (model.DetailsTab.SubContractorAccepted == true ? "ACCEPTED" : "REJECTED");
    agency = _agencyService.GetById(model.AgencyId);
    //+ ";" + _agencyService.GetById(model.AgencyId).AMUEmail
    sendEmail(model, currentUser, agency.ContactEmail , "JobStatusEmail.html", subject, title);
}
else if (currentUser.RoleID == "LC" && model.Estimate != null && model.Estimate.EstimateFinalised &&
   (model.UserCapabilities.EstimateSection != null && model.UserCapabilities.EstimateSection.Contains("EstimateFinalised")))
{
    //Contractor finalised the estimate
    subject = string.Format("FMWEB 2.0 Reactive Work Order {0} ESTIMATE SUBMITTED BY LC", model.AgencySequenceId);
    title = "FMWEB 2.0 WORK ORDER – ESTIMATE SUBMITTED BY LOCAL CONTRACTOR";
    string note = model.Estimate?.EstimatedAmount > 5000 ? "The following work order estimate is submitted by Local Contractor and awaiting FMC review" : "The following work order estimate is submitted by Local Contractor";
    agency = _agencyService.GetById(model.AgencyId);
    Dictionary<string, string> data = new Dictionary<string, string>();

    if (model.Estimate != null && model.Estimate.EstimateDate.HasValue)
    {
        data.Add("EstimatedDate", model.Estimate.EstimateDate.Value.ToString("dd/MM/yyyy"));
    }

    sendEmail(model, currentUser, agency.ContactEmail, "WOEstimateSubmitted.html", subject, title,note, overwriteData: data);
}
else if (currentUser.RoleID == "C" && estimateFMCEndorseChanged &&
   model.Estimate != null && model.Estimate.FMCApproved != null && (!model.DisableField("estimateSection", "FMCApproved")))
{
    //PWA Approved and not a reversal save
    //agency = _agencyService.GetById(model.AgencyId);
    string toEmail = string.Empty;
    subject = string.Format("FMWEB 2.0 Reactive WO {0} ESTIMATE IS {1} BY FMC",  model.AgencySequenceId, (model.Estimate.FMCApproved == true ? "ENDORSED" : "REJECTED"));                
    title = string.Format("FMWEB 2.0 WORK ORDER – ESTIMATE IS {0} BY FMC", model.Estimate.FMCApproved == true ? "ENDORSED" : "REJECTED");
    string note = model.Estimate.FMCApproved == true ? "The following work order estimate is endorsed by FMC and awaiting AMU approval" : "";
    Dictionary<string, string> data = new Dictionary<string, string>();
    data.Add("FMCRecommended", (model.Estimate.FMCApproved.HasValue && model.Estimate.FMCApproved.Value) ? "Yes" : "No");
    toEmail = model.ASOEmail;
    if (model.DetailsTab.SubContractorId != null)
    {
        string subContractorEmail = _jobsRepository.GetSubContractorDetails(model.DetailsTab.SubContractorId.Value)?.Email;
        toEmail = toEmail + ";" + subContractorEmail;
    }

    sendEmail(model, currentUser,toEmail , "WOEstimateFMCApproval.html", subject, title,note, data);
}
else if ((IsAMUUser(currentUser) || currentUser.RoleID == "GL" || currentUser.RoleID == "SL" || currentUser.RoleID == "AM" || currentUser.RoleID == "EM") &&
    estimateAMUEndorseChanged && model.Estimate != null && !model.DisableField("estimateSection", "AMUApproved") && (model.Estimate.AMUApproved != null))
{

    subject = string.Format("FMWEB 2.0 {0} WO {1} ESTIMATE IS {2} BY AMU", jobCode, model.AgencySequenceId, (model.Estimate.AMUApproved == true ? "APPROVED" : "REJECTED"));
    title =string.Format("FMWEB 2.0 WORK ORDER – ESTIMATE IS {0} BY AMU " , (model.Estimate.AMUApproved == true ? "APPROVED" : "REJECTED"));
    string note =  model.Estimate.AMUApproved == true ? "The following work order estimate is approved by AMU and available for local contractor’s claim submission." : "";
    agency = _agencyService.GetById(model.AgencyId);
    string toEmail = "";
    WorkOrderEmailData emailExtraData = _jobsRepository.GetWorkOrderEmailData(model.ServiceReportId.Value);
    Dictionary<string, string> overwriteData = new Dictionary<string, string>();
    overwriteData.Add("FMCRecommended", (emailExtraData.FMCRecommend.HasValue && emailExtraData.FMCRecommend.Value) ? "Yes" : "No");
    overwriteData.Add("FMCRecommendDate", emailExtraData.FMCRecommendDate.HasValue ? emailExtraData.FMCRecommendDate.Value.ToString("dd/MM/yyyy") : "");
    overwriteData.Add("EstimateDate", emailExtraData.EstimateDate.HasValue ? emailExtraData.EstimateDate.Value.ToString("dd/MM/yyyy") : "");
    overwriteData.Add("AMUApprovedDate", emailExtraData.AMUApprovedDate.HasValue ? emailExtraData.AMUApprovedDate.Value.ToString("dd/MM/yyyy") : "");
    overwriteData.Add("AMUApproved", (emailExtraData.AMUApproved.HasValue && emailExtraData.AMUApproved.Value) ? "Yes" : "No");
    //Add FMC
    string subContractorEmail = string.Empty;
    toEmail = agency.ContactEmail;
    if (model.DetailsTab.SubContractorId != null)
    {
        subContractorEmail = _jobsRepository.GetSubContractorDetails(model.DetailsTab.SubContractorId.Value)?.Email;
        toEmail = toEmail + ";" + subContractorEmail;
    }

    sendEmail(model, currentUser, toEmail, "WOEstimateAMUApproval.html", subject, title, note, overwriteData: overwriteData);
}
else if (currentUser.RoleID == "LC" && model.Claim != null && model.Claim.ClaimFinalized == true && !model.DisableField("claimSection", "ClaimFinalized"))
{
    //AMU Approved
    agency = _agencyService.GetById(model.AgencyId);
    title = "The Following FMWEB 2.0  Work Order Claim is awaiting your review.";
    subject = string.Format("FMWEB 2.0 {0} Work Order {1} for {2} claim review", jobCode, model.AgencySequenceId, model.SchoolName);               
    agency = _agencyService.GetById(model.AgencyId);
    decimal? claimedAmount = model.Claim.TotalAvailableToClaimThisMonth;

    Dictionary<string, string> overwriteData = new Dictionary<string, string>();
    overwriteData.Add("ClaimedAmount", claimedAmount != null ? claimedAmount.Value.ToString("0.00") : "0.00");

    sendEmail(model, currentUser, agency.ContactEmail, "JobClaimed.html", subject, title, overwriteData: overwriteData);

}

}

private void sendEmail(JobDetails model, UserViewModel currentUser, string toEmail, string template, string subject, string title, string note = "", Dictionary<string, string> overwriteData = null)
{
Agency agency = _agencyService.GetById(model.AgencyId);
//AMU
string userFN = currentUser.FullName;

toEmail = toEmail + ";" + currentUser.Email ;
EmailService emailHelper = new EmailService();
Dictionary<string, string> data = new Dictionary<string, string>();
data.Add("Title", title);
data.Add("JobNo", model.AgencySequenceId.ToString());
data.Add("AgencyId", model.AgencyId.ToString());
data.Add("AgencyDesc", agency.AgencyName);
data.Add("RequestedBy", model.DetailsTab.RequestedBy);
data.Add("LocalContractor", model.DetailsTab.SubContractorName);            
data.Add("SchoolName", model.SchoolName);
data.Add("Phone", model.Phone);
data.Add("BuildingList", GetBuildingList(model));
data.Add("SchoolEmail", model.SchoolEmail);
//if (model.Estimate != null && model.Estimate.EstimateDate.HasValue) {
//    data.Add("EstimatedDate", model.Estimate.EstimateDate.Value.ToString("dd/MM/yyyy"));
//}
//if (model.Estimate != null)
//{

//}

//data.Add("WorkCategoryDesc", model.WorkCategoryDesc);

//data.Add("EstimatedAmount", model.Estimate.EstimatedAmount.HasValue ? model.Estimate.EstimatedAmount.ToString() : "0.00");
//data.Add("ApprovedAmount", model.Estimate.ApproveAmount.HasValue ? model.Estimate.ApproveAmount.ToString() : "0.00");
data.Add("EstimatedAmount", model.Estimate.EstimatedAmount != null ? model.Estimate.EstimatedAmount.ToString() : "0.00");
data.Add("Note", note);

//double gst = model.Estimate.ApproveAmount.HasValue ? ((double)model.Estimate.ApproveAmount.Value * .1) : 0.0;
//data.Add("GST", gst.ToString());
//data.Add("TotalWithGST", model.Estimate.ApproveAmount.HasValue ? ((double)model.Estimate.ApproveAmount.Value + gst).ToString() : "0.00");

//var request = HttpContext.Current.Request;
//data.Add("Link", string.Format("{0}://{1}{2}?jobId={3}", request.Url.Scheme, request.Url.Authority, request.Url.AbsolutePath, model.ServiceReportId));

//append overwrite data
if (overwriteData != null)
{
    foreach (var x in overwriteData)
    {
        if (data.ContainsKey(x.Key))
        {
            data[x.Key] = x.Value;
        }
        else
        {
            data.Add(x.Key, x.Value);
        }
    }
}

if (model.ServiceReportId.HasValue)
{
    Dictionary<string, string> additionalData = _jobsRepository.GetWorkOrderDetailsForEmail(model.ServiceReportId.Value);
    data = data.Concat(additionalData).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
}

emailHelper.SendMail(toEmail, data, subject, template);


}

public decimal GetCostAdjustment(int agencyId, string costAdjType, string costDate)
{
DateTime costingDate = DateTime.ParseExact(costDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);

string FinancialYear = GetFinancialYear(costingDate);
decimal costAdj = _jobsRepository.GetCostAdjustment(agencyId, costAdjType, FinancialYear, costingDate);
return costAdj;
}

private string GetFinancialYear(DateTime dateTime)
{
if (dateTime.Month >= 7)
{
    return (dateTime.Year) + "/" + (dateTime.Year + 1);
}
else
{
    return (dateTime.Year - 1) + "/" + dateTime.Year;

}

}

public IEnumerable<CustomDropdownItem> GetPurchaseOrder(int agencyId, int workCategoryID)
{
return _jobsRepository.GetPurchaseOrder(agencyId, workCategoryID);
}

public void ResetFlagsAndNotifyFMC(int contractStatusId)
{
FMSignOffDetailViewModel contractStatus = _contractStatusService.GetById(contractStatusId);
//if PWA has confirmed this month
if (contractStatus.AMUComplete)
{
    // _jobsRepository.GetAllFMCReclaimJobs();
}

}


public bool CanAOApproveInMayJune(int orderId)
{
bool canApprove = _jobsRepository.CanAOApproveInMayJune(orderId);
return canApprove;
}

public bool ResetJob(int agencyId, JobDetails model, UserViewModel currentUser)
{
if (model.SaveType.ToLower() == "unf")
{
    _jobsRepository.UnfinalizeJob(model, currentUser);
}
else if (model.SaveType.ToLower() == "recl")
{
    _jobsRepository.SetJobToBeReclaimed(model, currentUser);
}
else if (model.SaveType.ToLower() == "60day")
{
    _jobsRepository.ResetJob60Day(model, currentUser);
}
else if (model.SaveType.ToLower() == "40day")
{
    _jobsRepository.ResetJobNotReclaimedIn40Day(model, currentUser);
}

return true;

}

public bool UpdateJobAcceptance(WorkOrderAcceptanceStatus status)
{
return  _jobsRepository.UpdateJobAcceptance(status);
}

public int CreateWOAsVariation(CreateWorkOrder model, out int serviceReportId)
{
return _jobsRepository.CreateWOAsVariation(model, out serviceReportId);
}

public bool HasOnlyOneWOWithSchoolContribution(int callLogFitsId, int ServiceReportId)
{
return _jobsRepository.HasOnlyOneWOWithSchoolContribution(callLogFitsId, ServiceReportId);
}

public bool CanEditInsuranceStatusInWorkOrder(int callLogFitsId, int ServiceReportId)
{
return _jobsRepository.CanEditInsuranceStatusInWorkOrder(callLogFitsId, ServiceReportId);
}

public IEnumerable<WorkOrderCertifyListItem> GetClaimsToApprove(WorkOrderCertifyFilterModel claimModel, out int filteredResultsCount, out int totalResultsCount)
{
var userPropertyIds = _userService.GetByUserCode(_baseService.GetCurrentUser().UserCode, claimModel.AgencyId).UserPropertyIds;
return _jobsRepository.GetClaimsToApprove(claimModel, userPropertyIds, out filteredResultsCount, out totalResultsCount);
}

public IEnumerable<InsuranceClaimsReimbursementListItem> GetClaimsToApproveForInsuranceReimbursement(InsuranceClaimsReimbursementFilterModel reimbursementModel, out int filteredResultsCount, out int totalResultsCount)
{
var userPropertyIds = _userService.GetByUserCode(_baseService.GetCurrentUser().UserCode, reimbursementModel.AgencyId).UserPropertyIds;
return _jobsRepository.GetClaimsToApproveForInsuranceReimbursement(reimbursementModel, userPropertyIds, out filteredResultsCount, out totalResultsCount);
}

public WorkOrderClaimApprovalViewModdel GetClaimApprovalData(int claimId)
{
return _jobsRepository.GetClaimApprovalData(claimId);
}

public ItemsCounts GetServiceReportCounts(int agencyID, int propertyID, List<int> userPropertyIds, string roleID, int? localContractorID)
{
return _jobsRepository.GetServiceReportCounts(agencyID, propertyID, userPropertyIds, roleID, localContractorID);
}

public IList<IRCMClaimStatus> GetIRCMClaimStatuses()
{
return _jobsRepository.GetIRCMClaimStatuses();
}

public IList<ClaimApprovalStatusFilter> GetReactiveClaimApprovalStatusFilters(int? currentAgencyTypeId)
{            
var claimApprovalStatusFilter = _jobsRepository.GetReactiveClaimApprovalStatusFilters().Where(a=> a.ShowInAgencyTypeIds.Contains(currentAgencyTypeId.ToString())).ToList();
return claimApprovalStatusFilter;
}

public IList<ReimbursementStatus> GetReimbursementStatuses()
{
return _jobsRepository.GetReimbursementStatuses();
}

public IList<AssessorStatus> GetAssessorStatuses()
{
return _jobsRepository.GetAssessorStatuses();
}

public void GenerateReimbursementItems(IEnumerable<WorkOrderCertifyListItem> claims, string loggedInUser)
{
_jobsRepository.GenerateReimbursementItems(claims, loggedInUser);
}

public IEnumerable<WorkOrderCertifyListItem> GetWorkOrderCertifyListItems(WorkOrderCertifyFilterModel workOderCertifyFilterModel, out int filteredResultsCount, out int totalResultsCount)
{
var userPropertyIds = _userService.GetByUserCode(_baseService.GetCurrentUser().UserCode, workOderCertifyFilterModel.AgencyId).UserPropertyIds;
return _jobsRepository.GetWorkOrderCertifyListItems(workOderCertifyFilterModel, userPropertyIds, out filteredResultsCount, out totalResultsCount);

}
}
}