using AssetCare.Model.Common;
//using AssetCare.Model.Migrations;
using AssetCare.Model.Models;
using AssetCare.Model.ViewModels;
using AssetCare.Repository.Interfaces;
using AutoMapper;
using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.SqlServer;
using System.IO.Compression;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace AssetCare.Repository
{
    public class JobRepository : GenericRepository<CallLogFITS>, IJobRepository
    {
        private readonly IMapper _mapper;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        public JobRepository(AssetCareContext context, IMapper mapper) : base(context)
        {
            _mapper = mapper;
        }

        public IEnumerable<JobListViewModel> FilterJobData(CallOutFilterModel filterModel, bool dataForExport, List<int> userPropertyIds, string userRole, out int filteredResultsCount, out int totalResultsCount)
        {

            var ignoreFields = new string[] { };
            var dynamicQuery = PredicateHelper.BuildDynamicWhereClause<JobListViewModel>(filterModel.ColumnSearches, ignoreFields);
            //  _context.ServiceReportStatuses.Where(x=> (x.ServiceReportStatusDesc == "Cancelled" || x.ServiceReportStatusDesc == "Completed" || x.ServiceReportStatusDesc == "Completed and Claimed")
            if (filterModel.PropertyId.HasValue)
            {
                userPropertyIds.Clear();
                userPropertyIds.Add(filterModel.PropertyId.Value);
            }

            using (var dbContextTransaction = _context.Database.BeginTransaction(IsolationLevel.Snapshot))
            {
                IQueryable<JobListViewModel> searchResults = _context.ServiceReports.Where(x => x.AgencyId == filterModel.AgencyId &&
                          (userPropertyIds.Count > 0 ? userPropertyIds.Contains(x.PropertyId.Value) : true) &&
                          (filterModel.RestrictToLocalContractorId == true ? x.SubContractorId == filterModel.SubContractorID : true) &&
                          (filterModel.StatusId > 0 ? x.ServiceReportStatusId == filterModel.StatusId : (x.JobStatusId == null || x.JobStatusId > 0)) &&
                          (filterModel.Workcategory > 0 ? x.WorkCategoryId == filterModel.Workcategory : (x.WorkCategoryId == null || x.WorkCategoryId > 0)) &&
                          ((filterModel.ShowWOPaidOnly == true) ? x.ServiceReportStatusId == 11 : true) &&
                          ((userRole.Equals("MFUMGR") || userRole.Equals("MFUOFFICER")) ?
                                                (x.CallLogFITS != null) && (x.CallLogFITS.IsInsuranceEvent == true || x.CallLogFITS.IsInsuranceEventByWO == true) : true)

                ).Select(
                d => new JobListViewModel
                {
                    Id = d.ServiceReportId,
                    AgencyId = d.AgencyId,
                    WorkflowStatus = d.ServiceReportAC.Code,
                    Buildings = AssetCareContext.GetJobBuildings(d.ServiceReportId),
                    BuildingList = _context.ServiceReportBuildings
                                          .Where(x => x.ServiceReportID == d.ServiceReportId && x.AgencyID == filterModel.AgencyId && x.Deleted != true)
                                          .Select(y => _context.Buildings.Where(bldng => bldng.BuildingID == y.BuildingID)
                                          .Select(s => s.BuildingNo)),
                    CallLogFITSId = d.CallLogFITSId.Value,
                    CallOutId = d.CallLogFITS.AgencySequenceID.Value,
                    //EUR100 = (d.EUR100 == null || d.EUR100 == false) ? "No" : "Yes",
                    //FMCRef = d.ContractorWONo == null ? string.Empty : d.ContractorWONo,
                    SchoolId = d.Property.SchoolId,
                    PropertyId = d.PropertyId,
                    WorkCategoryId = d.WorkCategoryId,
                    SchoolName = d.Property != null ? d.Property.PropertyDesc : "",
                    Description = d.ServiceReportDesc,
                    //dataForExport ? d.ServiceReportDesc : (String.IsNullOrEmpty(d.ServiceReportDesc) ? String.Empty :
                    //        (d.ServiceReportDesc.Length > 50 ? d.ServiceReportDesc.Substring(0, 50) + "..." : d.ServiceReportDesc)),
                    WorkCategory = (d.CallLogFITSId != null && _context.Proposal.Any(p => p.CallLogFITSID == d.CallLogFITSId))
                        ? "LRLV School Funded"
                        : (d.WorkCategory != null ? d.WorkCategory.Description : ""),
                    StatusId = d.ServiceReportStatusId,
                    Status = d.ServiceReportStatus != null ? d.ServiceReportStatus.ServiceReportStatusDesc : "",
                    AMUClaimApproved = _context.Claims.Where(n => n.ServiceReportId == d.ServiceReportId).FirstOrDefault().AMUApproved,
                    JobID = d.AgencySequenceId.Value,
                    Version = d.Version,
                    WODate = d.RecordedDate,
                    SubContractor = d.SubContractor != null ? d.SubContractor.Name : "",
                    RequestedBy = d.CallLogFITS.RequestedBy,
                    IsIncludeBetterment = (d.IsIncludeBetterment == true) ? "Yes" : "No",
                    IsReimbursementComplete = (d.ReimbursementComplete == true) ? "Yes" : "No"
                    //DaysSinceCompleted = ((d.ServiceReportStatusId == 4 || d.ServiceReportStatusId == 5 ) && d.CompletedDate != null) ? System.Data.Entity.DbFunctions.DiffDays(d.CompletedDate.Value, DateTime.Now) : daysSinceCompleted
                });

                IQueryable<JobListViewModel> filtered = ((userPropertyIds != null) && userPropertyIds.Any()) ? searchResults.Where(m => userPropertyIds.Contains(m.PropertyId.Value)) : searchResults;
                if (!string.IsNullOrEmpty(filterModel.SearchBy))
                {
                    string serchText = filterModel.SearchBy.ToLower();
                    filtered = string.IsNullOrEmpty(filterModel.SearchBy)
                                ? searchResults
                                 : searchResults.Where(f => f.Buildings.ToLower().Contains(serchText) ||
                                  f.JobID.ToString().Contains(serchText) ||
                                  f.Status.ToLower().Contains(serchText) ||
                                  f.SubContractor.ToLower().Contains(serchText) ||
                                  f.SchoolName.ToLower().Contains(serchText) ||
                                  f.RequestedBy != null && f.RequestedBy.ToLower().Contains(serchText) ||
                                  f.Description != null && f.Description.ToLower().Contains(serchText) ||
                                  f.CallOutId.ToString().Contains(serchText) ||
                                  f.WorkflowStatus != null && f.WorkflowStatus.ToString().Contains(serchText) ||
                                  f.WorkCategory != null && f.WorkCategory.ToLower().Contains(serchText) ||
                                  f.IsIncludeBetterment != null && f.IsIncludeBetterment.ToString().Contains(serchText) ||
                                  f.IsReimbursementComplete != null && f.IsReimbursementComplete.ToString().Contains(serchText) ||

                                //(Convert.ToString(f.RequestedBy).ToLower().Contains(filterModel.SearchBy.ToLower())) ||
                                //(Convert.ToString(f.Description).ToLower().Contains(filterModel.SearchBy.ToLower())) ||
                                // (Convert.ToString(f.CallOutId).ToLower().Contains(filterModel.SearchBy.ToLower())) //||
                                //f.BuildingList.Any(x => x.Contains(serchText)) || TODO: commenting until it is fixed properly. Giving error on prod
                                //f.Buildings.ToLower().Contains(serchText) ||
                                //f.Status.ToLower().Contains(serchText) ||
                                //f.SubContractor.ToLower().Contains(serchText) ||
                                //f.SchoolId.ToString().Contains(serchText) ||
                                //f.WorkflowStatus.ToLower().Contains(serchText) ||
                                //f.JobID.ToString().Contains(serchText) ||
                                //f.CallOutId.ToString().Contains(serchText) ||
                                //f.Description.Contains(serchText) ||
                                //f.WorkCategory.Contains(serchText) ||
                                //f.RequestedBy.Contains(serchText) ||
                                f.WODate != null && ((f.WODate.Value.Day < 10 ? "0" + f.WODate.Value.Day.ToString() : f.WODate.Value.Day.ToString()) + "-" +
                                              (f.WODate.Value.Month < 10 ? "0" + f.WODate.Value.Month.ToString() : f.WODate.Value.Month.ToString()) + "-" +
                                              f.WODate.Value.Year.ToString())
                                              .Contains(serchText)  //||
                                                                    ////f.EUR100.Contains(serchText) ||
                                                                    ////f.FMCRef.Contains(serchText) ||
                                                                    //f.Status.ToLower().Contains(filterModel.SearchBy.ToLower()) ||
                                                                    //f.IsIncludeBetterment.Contains(serchText) ||
                                                                    //f.IsReimbursementComplete.Contains(serchText)
                                );
                }



                if (filterModel.ColumnSearches != null && filterModel.ColumnSearches.Count > 0)
                {
                    foreach (var columnSearch in filterModel.ColumnSearches)
                    {
                        if (columnSearch.Key.ToLower() == "schoolid")
                        {
                            filtered = filtered.Where(f => f.SchoolId.ToString().Contains(columnSearch.Value));
                        }
                        else if (columnSearch.Key.ToLower() == "jobid")
                        {
                            filtered = filtered.Where(f => f.JobID.ToString().Contains(columnSearch.Value));
                        }
                        else if (columnSearch.Key.ToLower() == "calloutid")
                        {
                            filtered = filtered.Where(f => f.CallOutId.ToString().Contains(columnSearch.Value));
                        }
                        else if (columnSearch.Key.ToLower() == "buildinglist")
                        {
                            filtered = filtered.Where(x => x.BuildingList.Any(f => f.Contains(columnSearch.Value)));
                        }
                        else if (columnSearch.Key.ToLower() == "wodate")
                        {
                            filtered = filtered.Where(f => ((f.WODate.Value.Day < 10 ? "0" + f.WODate.Value.Day.ToString() : f.WODate.Value.Day.ToString()) + "-" +
                                          (f.WODate.Value.Month < 10 ? "0" + f.WODate.Value.Month.ToString() : f.WODate.Value.Month.ToString()) + "-" +
                                          f.WODate.Value.Year.ToString())
                                          .Contains(columnSearch.Value));
                        }
                        else if (columnSearch.Key.ToLower() == "isincludebetterment")
                        {
                            filtered = filtered.Where(x => x.IsIncludeBetterment.Contains(columnSearch.Value));
                        }
                        else if (columnSearch.Key.ToLower() == "isreimbursementcomplete")
                        {
                            filtered = filtered.Where(x => x.IsReimbursementComplete.Contains(columnSearch.Value));
                        }
                        else
                        {
                            filtered = filtered.Where($"{columnSearch.Key}.ToString().ToLower().Contains(@0)", columnSearch.Value);

                        }
                    }
                }


                if (filterModel.SortBy != null)
                {
                    filtered = filtered.OrderBy($"{filterModel.SortBy} {filterModel.SortDir}");
                }

                filteredResultsCount = filtered.Count();
                totalResultsCount = searchResults.Count();
                List<JobListViewModel> result = null;
                if (filterModel.Take == -1)
                {
                    result = filtered.ToList();
                }
                else
                {
                    result = filtered.Skip(filterModel.Skip).Take(filterModel.Take).ToList();
                    foreach (var job in result)
                    {
                        job.Buildings = job.Buildings.MaxSize(15);
                    }
                }
                dbContextTransaction.Commit();
                return result;
            }

        }

        public IEnumerable<WorkOrderCertifyListItem> GetClaimsToApprove(
                    WorkOrderCertifyFilterModel filterModel, List<int> userPropertyIds,
                    out int filteredResultsCount, out int totalResultsCount)
        {
            int? approvalStatusFilterID = filterModel.ApprovalStatusFilterID;
            int? claimedAmount = filterModel.ClaimedAmountFlag;

            using (var dbContextTransaction = _context.Database.BeginTransaction(IsolationLevel.Snapshot))
            {
                // 1. Base query — entities only, no projection.
                var baseQuery = _context.Claims.Where(x => x.AgencyId == filterModel.AgencyId &&
                    (filterModel.RestrictToLocalContractorId == true ? x.SubContractorId == filterModel.SubContractorId : true) &&
                    (userPropertyIds.Count > 0 ? userPropertyIds.Contains(x.PropertyId.Value) : true)
                    && (!approvalStatusFilterID.HasValue || (approvalStatusFilterID == 1 ? (x.FMCReviewed == null && x.AMUApproved == null) : true)) // Awaiting Endorsement
                    && (!approvalStatusFilterID.HasValue || (approvalStatusFilterID == 2 ? x.AMUApproved == true : true)) // Approved
                    && (!approvalStatusFilterID.HasValue || (approvalStatusFilterID == 3 ? (x.FMCReviewed != null && x.AMUApproved == false) : true)) // FMC Review (Not Approved)
                    && (!approvalStatusFilterID.HasValue || (approvalStatusFilterID == 4 ? (x.AMUApproved == null && x.FMCReviewed == true) : true)) // Endorsed
                    && (!approvalStatusFilterID.HasValue || (approvalStatusFilterID == 5 ? (x.AMUApproved == null && x.FMCReviewed == false) || (x.AMUApproved == false && x.FMCReviewed == null) : true)) // LC Review (Not Endorsed)
                    && (filterModel.ApprovalFromDate.HasValue ? x.AMUApprovedDate >= filterModel.ApprovalFromDate : (filterModel.ApprovalFromDate == null))
                    && (filterModel.ApprovalToDate.HasValue ? x.AMUApprovedDate <= filterModel.ApprovalToDate : filterModel.ApprovalToDate == null)
                    && (filterModel.SubContractorId.HasValue ? x.SubContractorId == filterModel.SubContractorId : (filterModel.SubContractorId == null))
                    && (filterModel.PropertyId.HasValue ? x.PropertyId == filterModel.PropertyId : filterModel.PropertyId == null)
                    && (claimedAmount == 1 ? x.ClaimedAmount > 5000 : (claimedAmount == 0 ? x.ClaimedAmount <= 5000 : true))
                    && (x.ServiceReport.ServiceReportStatusId != 9)   // cancelled WOs excluded
                    && (!x.SubContractor.SchoolSecurity)              // School Security excluded
                    && (x.ClaimTypeCode == "I" || x.ClaimTypeCode == "J" || x.ClaimTypeCode == "H"));

                // 2. Total count - entity columns only.
                totalResultsCount = baseQuery.Count();

                // 3. Apply ALL filters on entity columns — BEFORE any projection.
                var filtered = baseQuery;
                bool anyRowFilterApplied = false;

                if (!string.IsNullOrEmpty(filterModel.SearchBy))
                {
                    anyRowFilterApplied = true;
                    string s = filterModel.SearchBy.ToLower();

                    // Resolve LRLV matches ONCE against Proposal, defined outside the Where
                    var searchMatchingCallLogIds = _context.Proposal
                        .Where(p => p.CallLogFITSID != null &&
                            SqlFunctions.StringConvert((decimal?)p.ProposalID).Trim().Contains(s))
                        .Select(p => p.CallLogFITSID);

                    filtered = filtered.Where(x =>
                        x.Property.PropertyShortDesc.ToLower().Contains(s) ||
                        x.AMUComment.ToLower().Contains(s) ||
                        x.ServiceReport.ServiceReportDesc.ToLower().Contains(s) ||
                        x.FMCComment.ToLower().Contains(s) ||
                        x.SubContractor.Name.ToLower().Contains(s) ||
                        ((x.ServiceReport.CallLogFITSId != null && _context.Proposal.Any(p => p.CallLogFITSID == x.ServiceReport.CallLogFITSId))
                            ? "lrlv school funded"
                            : (x.ClaimTypeCode == "I" ? "insurance" : (x.ServiceReport.WorkCategoryId == 1 ? "reactive" : "school funded"))).Contains(s) ||
                        (x.ServiceReport.AgencySequenceId != null &&
                            SqlFunctions.StringConvert((decimal?)x.ServiceReport.AgencySequenceId).Trim().Contains(s)) ||
                        SqlFunctions.StringConvert((decimal?)x.ClaimedAmount).Trim().Contains(s) ||
                        (x.ApprovedAmount != null &&
                            SqlFunctions.StringConvert(x.ApprovedAmount).Trim().Contains(s)) ||
                        (x.AMUApprovedDate.HasValue && (
                            (SqlFunctions.DatePart("d", x.AMUApprovedDate.Value).ToString().Length == 1 ? "0" + SqlFunctions.DatePart("d", x.AMUApprovedDate.Value).ToString() : SqlFunctions.DatePart("d", x.AMUApprovedDate.Value).ToString()) + "/" +
                            (SqlFunctions.DatePart("m", x.AMUApprovedDate.Value).ToString().Length == 1 ? "0" + SqlFunctions.DatePart("m", x.AMUApprovedDate.Value).ToString() : SqlFunctions.DatePart("m", x.AMUApprovedDate.Value).ToString()) + "/" +
                            SqlFunctions.DatePart("year", x.AMUApprovedDate.Value).ToString()).Contains(s)) ||
                        (x.ServiceReport.ClaimFinalizedDate.HasValue && (
                            (SqlFunctions.DatePart("d", x.ServiceReport.ClaimFinalizedDate.Value).ToString().Length == 1 ? "0" + SqlFunctions.DatePart("d", x.ServiceReport.ClaimFinalizedDate.Value).ToString() : SqlFunctions.DatePart("d", x.ServiceReport.ClaimFinalizedDate.Value).ToString()) + "/" +
                            (SqlFunctions.DatePart("m", x.ServiceReport.ClaimFinalizedDate.Value).ToString().Length == 1 ? "0" + SqlFunctions.DatePart("m", x.ServiceReport.ClaimFinalizedDate.Value).ToString() : SqlFunctions.DatePart("m", x.ServiceReport.ClaimFinalizedDate.Value).ToString()) + "/" +
                            SqlFunctions.DatePart("year", x.ServiceReport.ClaimFinalizedDate.Value).ToString()).Contains(s)) ||
                        (x.ServiceReport.CallLogFITSId != null &&
                            searchMatchingCallLogIds.Contains(x.ServiceReport.CallLogFITSId)));
                }

                if (filterModel.ColumnSearches != null && filterModel.ColumnSearches.Count > 0)
                {
                    foreach (var columnSearch in filterModel.ColumnSearches)
                    {
                        if (string.IsNullOrEmpty(columnSearch.Value)) continue;
                        anyRowFilterApplied = true;
                        string v = columnSearch.Value;

                        switch (columnSearch.Key.ToLower())
                        {
                            case "workorderid":
                                filtered = filtered.Where(x => x.ServiceReport.AgencySequenceId != null &&
                                    SqlFunctions.StringConvert((decimal?)x.ServiceReport.AgencySequenceId).Trim().Contains(v));
                                break;

                            case "lrlv":
                                {
                                    var term = v;
                                    var matchingCallLogIds = _context.Proposal
                                        .Where(p => p.CallLogFITSID != null &&
                                            SqlFunctions.StringConvert((decimal?)p.ProposalID).Trim().Contains(term))
                                        .Select(p => p.CallLogFITSID);

                                    filtered = filtered.Where(x =>
                                        x.ServiceReport.CallLogFITSId != null &&
                                        matchingCallLogIds.Contains(x.ServiceReport.CallLogFITSId));
                                }
                                break;

                            case "category":
                                filtered = filtered.Where(x =>
                                    ((x.ServiceReport.CallLogFITSId != null && _context.Proposal.Any(p => p.CallLogFITSID == x.ServiceReport.CallLogFITSId))
                                        ? "LRLV School Funded"
                                        : (x.ClaimTypeCode == "I" ? "Insurance" : (x.ServiceReport.WorkCategoryId == 1 ? "Reactive" : "School Funded")))
                                    .Contains(v));
                                break;

                            case "localcontractor":
                                filtered = filtered.Where(x => x.SubContractor.Name.Contains(v));
                                break;

                            case "schoolname":
                                filtered = filtered.Where(x => x.Property.PropertyShortDesc.Contains(v));
                                break;

                            case "jobdesc":
                                filtered = filtered.Where(x => x.ServiceReport.ServiceReportDesc.Contains(v));
                                break;

                            case "claimedamount":
                                filtered = filtered.Where(x =>
                                    SqlFunctions.StringConvert((decimal?)x.ClaimedAmount).Trim().Contains(v));
                                break;

                            case "approvedamount":
                                filtered = filtered.Where(x => x.ApprovedAmount != null &&
                                    SqlFunctions.StringConvert(x.ApprovedAmount).Trim().Contains(v));
                                break;

                            case "fmccomments":
                                filtered = filtered.Where(x => x.FMCComment.Contains(v));
                                break;

                            case "asocomment":
                                filtered = filtered.Where(x => x.AMUComment.Contains(v));
                                break;

                            case "approveddate":
                                filtered = filtered.Where(x => x.AMUApprovedDate.HasValue && (
                                    (SqlFunctions.DatePart("d", x.AMUApprovedDate.Value).ToString().Length == 1 ? "0" + SqlFunctions.DatePart("d", x.AMUApprovedDate.Value).ToString() : SqlFunctions.DatePart("d", x.AMUApprovedDate.Value).ToString()) + "/" +
                                    (SqlFunctions.DatePart("m", x.AMUApprovedDate.Value).ToString().Length == 1 ? "0" + SqlFunctions.DatePart("m", x.AMUApprovedDate.Value).ToString() : SqlFunctions.DatePart("m", x.AMUApprovedDate.Value).ToString()) + "/" +
                                    SqlFunctions.DatePart("year", x.AMUApprovedDate.Value).ToString()).Contains(v));
                                break;

                            case "claimfinalizeddate":
                                filtered = filtered.Where(x => x.ServiceReport.ClaimFinalizedDate.HasValue && (
                                    (SqlFunctions.DatePart("d", x.ServiceReport.ClaimFinalizedDate.Value).ToString().Length == 1 ? "0" + SqlFunctions.DatePart("d", x.ServiceReport.ClaimFinalizedDate.Value).ToString() : SqlFunctions.DatePart("d", x.ServiceReport.ClaimFinalizedDate.Value).ToString()) + "/" +
                                    (SqlFunctions.DatePart("m", x.ServiceReport.ClaimFinalizedDate.Value).ToString().Length == 1 ? "0" + SqlFunctions.DatePart("m", x.ServiceReport.ClaimFinalizedDate.Value).ToString() : SqlFunctions.DatePart("m", x.ServiceReport.ClaimFinalizedDate.Value).ToString()) + "/" +
                                    SqlFunctions.DatePart("year", x.ServiceReport.ClaimFinalizedDate.Value).ToString()).Contains(v));
                                break;
                        }
                    }
                }

                // 4. Count the filtered set — still entity columns, no projection.
                filteredResultsCount = anyRowFilterApplied ? filtered.Count() : totalResultsCount;

                // 5. Sort on entity columns. Map view-model names to entity paths.
                var sortMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    { "WorkOrderId",        "ServiceReport.AgencySequenceId" },
                    { "SchoolName",         "Property.PropertyShortDesc" },
                    { "LocalContractor",    "SubContractor.Name" },
                    { "JobDesc",            "ServiceReport.ServiceReportDesc" },
                    { "ClaimedAmount",      "ClaimedAmount" },
                    { "ApprovedAmount",     "ApprovedAmount" },
                    { "ApprovedDate",       "AMUApprovedDate" },
                    { "ClaimFinalizedDate", "ServiceReport.ClaimFinalizedDate" },
                    { "Category",           "ClaimTypeCode" },
                    { "FMCComments",        "FMCComment" },
                    { "ASOComment",         "AMUComment" },
                    { "AMUApproved",        "AMUApproved" },
                    { "FMCReviewed",        "FMCReviewed" },
                    { "ClaimId",            "ClaimId" },
                };

                string sortBy = string.IsNullOrEmpty(filterModel.SortBy) ? "WorkOrderId" : filterModel.SortBy;
                string sortDir = string.IsNullOrEmpty(filterModel.SortDir) ? "desc" : filterModel.SortDir;
                string sortPath;
                if (!sortMap.TryGetValue(sortBy, out sortPath))
                    sortPath = "ServiceReport.AgencySequenceId"; // fallback (covers LRLV)
                var ordered = filtered.OrderBy($"{sortPath} {sortDir}, ClaimId asc"); // ClaimId tiebreaker for stable paging

                // 6. Page on the entity query, THEN project, the join and heavy select
                var pageQuery = (filterModel.Take == -1) ? ordered : ordered.Skip(filterModel.Skip).Take(filterModel.Take);

                var rawPage = pageQuery.Select(m => new
                {
                    m.ClaimId,
                    m.AMUApproved,
                    m.ApprovedAmount,
                    m.AMUApprovedDate,
                    ASOComment = m.AMUComment,
                    m.ClaimedAmount,
                    FMCComments = m.FMCComment,
                    m.FMCReviewed,
                    JobDesc = m.ServiceReport.ServiceReportDesc,
                    m.ClaimTypeCode,
                    LocalContractor = m.SubContractor.Name,
                    SchoolName = m.Property.PropertyShortDesc,
                    WorkOrderId = m.ServiceReport.AgencySequenceId,
                    ServiceReportId = m.ReferringRecordId,
                    m.PropertyId,
                    SAPVendorID = m.SubContractor.SAPVendorID,
                    m.ServiceReport.ClaimFinalized,
                    ClaimFinalizedDate = m.ServiceReport.ClaimFinalizedDate,
                    CallLogFITSId = m.ServiceReport.CallLogFITSId,
                    WorkCategoryId = m.ServiceReport.WorkCategoryId
                }).ToList();

                // One batched lookup for the page's LRLVs.
                var pageCallLogIds = rawPage
                    .Where(r => r.CallLogFITSId.HasValue)
                    .Select(r => r.CallLogFITSId.Value)
                    .Distinct()
                    .ToList();

                var lrlvLookup = pageCallLogIds.Any()
                    ? _context.Proposal
                        .Where(p => p.CallLogFITSID != null && pageCallLogIds.Contains(p.CallLogFITSID.Value))
                        .ToDictionary(p => p.CallLogFITSID.Value, p => (int?)p.ProposalID)
                    : new Dictionary<int, int?>();

                // 7. Date formatting and removes all DatePart SQL from the projection.
                var page = rawPage.Select(r => new WorkOrderCertifyListItem
                {
                    ClaimId = r.ClaimId,
                    AMUApproved = r.AMUApproved,
                    ApprovedAmount = r.ApprovedAmount,
                    ApprovedDate = r.AMUApprovedDate.HasValue ? r.AMUApprovedDate.Value.ToString("dd/MM/yyyy") : "",
                    ASOComment = r.ASOComment,
                    ClaimedAmount = r.ClaimedAmount,
                    FMCComments = r.FMCComments,
                    FMCReviewed = r.FMCReviewed,
                    JobDesc = r.JobDesc,
                    Category = (r.CallLogFITSId.HasValue && lrlvLookup.ContainsKey(r.CallLogFITSId.Value))
                        ? "LRLV School Funded"
                        : (r.ClaimTypeCode == "I" ? "Insurance" : (r.WorkCategoryId == 1 ? "Reactive" : "School Funded")),
                    LocalContractor = r.LocalContractor,
                    SchoolName = r.SchoolName,
                    WorkOrderId = r.WorkOrderId,
                    ServiceReportId = r.ServiceReportId,
                    PropertyId = r.PropertyId,
                    HasSAPVendorId = !string.IsNullOrEmpty(r.SAPVendorID),
                    ClaimFinalized = r.ClaimFinalized,
                    ClaimFinalizedDate = r.ClaimFinalizedDate.HasValue ? r.ClaimFinalizedDate.Value.ToString("dd/MM/yyyy") : "",
                    LRLV = (r.CallLogFITSId.HasValue && lrlvLookup.ContainsKey(r.CallLogFITSId.Value))
                            ? lrlvLookup[r.CallLogFITSId.Value] : null
                }).ToList();

                dbContextTransaction.Commit();
                return page;
            }
        }

        public IEnumerable<InsuranceClaimsReimbursementListItem> GetClaimsToApproveForInsuranceReimbursement(InsuranceClaimsReimbursementFilterModel filterModel, List<int> userPropertyIds, out int filteredResultsCount, out int totalResultsCount)
        {
            int? ircmClaimStatusID = filterModel.IRCMClaimStatusID;
            int? reimbursementStatusID = filterModel.ReimbursementStatusID;
            int? assessorStatusID = filterModel.AssessorStatusID;
            using (var dbContextTransaction = _context.Database.BeginTransaction(IsolationLevel.Snapshot))
            {
                var searchResults = from rli in _context.ReimbursementLineItems
                                    where rli.Claim.AgencyId == filterModel.AgencyId &&
                    (userPropertyIds.Count > 0 ? userPropertyIds.Contains(rli.Claim.PropertyId.Value) : true) &&
                    (ircmClaimStatusID.HasValue ? (rli.IRCMClaimStatusID == ircmClaimStatusID) : true) &&
                    (reimbursementStatusID.HasValue ? (rli.ReimbursementStatusID == reimbursementStatusID) : true) &&
                    (assessorStatusID.HasValue ? (rli.ServiceReport.CallLogFITS.AssessorStatusID == assessorStatusID) : true) &&
                    (filterModel.ApprovalFromDate.HasValue ? rli.Claim.AMUApprovedDate >= filterModel.ApprovalFromDate : (filterModel.ApprovalFromDate == null)) &&
                    (filterModel.ApprovalToDate.HasValue ? rli.Claim.AMUApprovedDate <= filterModel.ApprovalToDate : filterModel.ApprovalToDate == null) &&
                    (filterModel.PropertyId.HasValue ? rli.Claim.PropertyId == filterModel.PropertyId : filterModel.PropertyId == null) &&
                    (rli.Claim.ClaimTypeCode == "I" || rli.Claim.ClaimTypeCode == "J")
                                    select new InsuranceClaimsReimbursementListItem()
                                    {
                                        ClaimId = rli.Claim.ClaimId,
                                        ReimbursementId = rli.ReimbursementLineItemID,
                                        AMUApproved = rli.Claim.AMUApproved,
                                        ApprovedAmount = rli.ServiceReportLineItem.Amount,
                                        ApprovedDate =
                                         rli.Claim.AMUApprovedDate.HasValue ? (
                                         (SqlFunctions.DatePart("d", rli.Claim.AMUApprovedDate.Value).ToString().Length == 1 ? "0" + SqlFunctions.DatePart("d", rli.Claim.AMUApprovedDate.Value).ToString() : SqlFunctions.DatePart("d", rli.Claim.AMUApprovedDate.Value).ToString()) + "/" +
                                         (SqlFunctions.DatePart("m", rli.Claim.AMUApprovedDate.Value).ToString().Length == 1 ? "0" + SqlFunctions.DatePart("m", rli.Claim.AMUApprovedDate.Value).ToString() : SqlFunctions.DatePart("m", rli.Claim.AMUApprovedDate.Value).ToString()) + "/" +
                                         (SqlFunctions.DatePart("year", rli.Claim.AMUApprovedDate.Value).ToString())
                                         ) : "",
                                        ASOComment = rli.Claim.AMUComment,
                                        ClaimedAmount = rli.ServiceReportLineItem.Amount,
                                        FMCComments = rli.Claim.FMCComment,
                                        FMCReviewed = rli.Claim.FMCReviewed,
                                        JobDesc = rli.Claim.ServiceReport.ServiceReportDesc,
                                        Category = rli.Claim.ClaimTypeCode == "I" ? "Insurance" : (rli.Claim.ClaimTypeCode == "J" ? "Reactive" : ""),
                                        LocalContractor = rli.Claim.SubContractor.Name,
                                        SchoolName = rli.Claim.Property.PropertyShortDesc,
                                        WorkOrderId = rli.Claim.ServiceReport.AgencySequenceId,
                                        ServiceReportId = rli.Claim.ReferringRecordId,
                                        PropertyId = rli.Claim.PropertyId,
                                        HasSAPVendorId = (rli.Claim.SubContractor.SAPVendorID == null || rli.Claim.SubContractor.SAPVendorID == "") ? false : true,
                                        ClaimFinalized = rli.ServiceReport.ClaimFinalized,
                                        CalloutId = rli.ServiceReport.CallLogFITSId.Value,
                                        CalloutDisplayId = rli.ServiceReport.CallLogFITS.AgencySequenceID.Value,
                                        ServiceReportLineItemId = rli.ServiceReportLineItemID,
                                        ServiceReportLineItemDisplayId = rli.ServiceReportLineItem.LineNumber.Value,
                                        IRCMClaimStatusID = rli.IRCMClaimStatusID,
                                        ReimbursementStatusID = rli.ReimbursementStatusID,
                                        ReimbursementAmount = rli.ReimbursementAmount,
                                        IRCMComments = rli.Comments,
                                        ReimbursementDate = rli.ReimbursementDate,
                                        ScheduleItemNo = rli.ServiceReportLineItem.ScheduledRate.ScheduleItemNo,
                                        ReimbursementReady = ((rli.ServiceReport.CallLogFITS.IsReadyForReimbursement == true) ? "Yes" : "No"),
                                        AssessorStatus = ((rli.ServiceReport.CallLogFITS.AssessorStatusID == 1) ? "None" :
                                                            ((rli.ServiceReport.CallLogFITS.AssessorStatusID == 2) ? "Assessor Appointed" :
                                                             ((rli.ServiceReport.CallLogFITS.AssessorStatusID == 3) ? "Waive Assessor" :
                                                            "N/A"))),
                                        ClaimNo = rli.ServiceReport.CallLogFITS.ClaimNumber
                                    };

                IQueryable<InsuranceClaimsReimbursementListItem> filtered = ((userPropertyIds != null) && userPropertyIds.Any()) ? searchResults.Where(m => userPropertyIds.Contains(m.PropertyId.Value)) : searchResults;
                if (!string.IsNullOrEmpty(filterModel.SearchBy))
                {
                    string serchText = filterModel.SearchBy.ToLower();
                    filtered = string.IsNullOrEmpty(filterModel.SearchBy)
                                ? searchResults
                                 : searchResults.Where(f =>
                                f.CalloutDisplayId.ToString().Contains(serchText) ||
                                f.WorkOrderId.ToString().Contains(serchText) ||
                                f.ServiceReportLineItemDisplayId.ToString().Contains(serchText) ||
                                f.ScheduleItemNo.ToLower().Contains(serchText) ||
                                f.ReimbursementReady.ToLower().Contains(serchText) ||
                                f.AssessorStatus.ToLower().Contains(serchText) ||
                                f.SchoolName.ToLower().Contains(serchText) ||
                                f.JobDesc.ToLower().Contains(serchText) ||
                                f.ApprovedAmount.ToString().Contains(serchText) ||
                                f.ClaimedAmount.ToString().Contains(serchText) ||
                                ((f.AMUApproved == true) ? "Yes" : "No").ToString().Contains(serchText) ||
                                f.ApprovedDate.ToString().Contains(serchText) ||
                                f.ReimbursementAmount.ToString().Contains(serchText) ||
                                f.IRCMComments.ToString().Contains(serchText)
                                );
                }



                if (filterModel.ColumnSearches != null && filterModel.ColumnSearches.Count > 0)
                {
                    foreach (var columnSearch in filterModel.ColumnSearches)
                    {
                        if (columnSearch.Key.ToLower() == "calloutdisplayid")
                        {
                            filtered = filtered.Where(f => f.CalloutDisplayId.ToString().Contains(columnSearch.Value));
                        }
                        else if (columnSearch.Key.ToLower() == "workorderid")
                        {
                            filtered = filtered.Where(f => f.WorkOrderId.ToString().Contains(columnSearch.Value));
                        }
                        else if (columnSearch.Key.ToLower() == "servicereportlineitemdisplayid")
                        {
                            filtered = filtered.Where(f => f.ServiceReportLineItemDisplayId.ToString().Contains(columnSearch.Value));
                        }
                        else if (columnSearch.Key.ToLower() == "scheduleitemno")
                        {
                            filtered = filtered.Where(f => f.ScheduleItemNo.ToLower().Contains(columnSearch.Value));
                        }
                        else if (columnSearch.Key.ToLower() == "reimbursementready")
                        {
                            filtered = filtered.Where(f => f.ReimbursementReady.ToLower().Contains(columnSearch.Value));
                        }
                        else if (columnSearch.Key.ToLower() == "assessorstatus")
                        {
                            filtered = filtered.Where(f => f.AssessorStatus.ToLower().Contains(columnSearch.Value));
                        }
                        else if (columnSearch.Key.ToLower() == "schoolname")
                        {
                            filtered = filtered.Where(f => f.SchoolName.Contains(columnSearch.Value));
                        }
                        else if (columnSearch.Key.ToLower() == "jobdesc")
                        {
                            filtered = filtered.Where(f => f.JobDesc.Contains(columnSearch.Value));
                        }
                        else if (columnSearch.Key.ToLower() == "claimedamount")
                        {
                            filtered = filtered.Where(f => f.ClaimedAmount.ToString().Contains(columnSearch.Value));
                        }
                        else if (columnSearch.Key.ToLower() == "approvedamount")
                        {
                            filtered = filtered.Where(f => f.ApprovedAmount.ToString().Contains(columnSearch.Value));
                        }
                        else if (columnSearch.Key.ToLower() == "amuapproved")
                        {
                            filtered = filtered.Where(f => ((f.AMUApproved == true) ? "Yes" : "No").ToString().Contains(columnSearch.Value));
                        }
                        else if (columnSearch.Key.ToLower() == "approveddate")
                        {
                            filtered = filtered.Where(f => f.ApprovedDate.Contains(columnSearch.Value));
                        }
                        else if (columnSearch.Key.ToLower() == "reimbursementamount")
                        {
                            filtered = filtered.Where(f => f.ReimbursementAmount.ToString().Contains(columnSearch.Value));
                        }
                        else if (columnSearch.Key.ToLower() == "ircmcomments")
                        {
                            filtered = filtered.Where(f => f.IRCMComments.Contains(columnSearch.Value));
                        }
                    }
                }


                if (filterModel.SortBy != null)
                {
                    filterModel.SortBy = "CalloutId";
                    filterModel.SortDir = "desc";
                    var SortBy2 = "WorkOrderId";
                    var SortDir2 = "desc";
                    var SortBy3 = "ServiceReportLineItemDisplayId";
                    var SortDir3 = "asc";
                    filtered = filtered.OrderBy($"{filterModel.SortBy} {filterModel.SortDir}, {SortBy2} {SortDir2}, {SortBy3} {SortDir3} ");
                }

                filteredResultsCount = filtered.Count();
                totalResultsCount = searchResults.Count();
                dbContextTransaction.Commit();
                if (filterModel.Take == -1)
                    return filtered;
                return filtered.Skip(filterModel.Skip).Take(filterModel.Take);
            }
        }

        public IEnumerable<JobListForHPViewModel> FilterJobDataForHP(CallOutFilterModel filterModel, bool dataForExport, List<int> userPropertyIds, int DefaultContractorID, string userRoleInAssetCare, out int filteredResultsCount, out int totalResultsCount)
        {
            using (var transaction = _context.Database.BeginTransaction(System.Data.IsolationLevel.ReadUncommitted))
            {
                List<int?> serviceReportACID = new List<int?>();
                serviceReportACID = filterModel.ServiceReportACID
                                    .Split(',')
                                    .Where(x => int.TryParse(x, out _))
                                    .Select(int.Parse)
                                    .ToList().ConvertAll<int?>(x => x);

                serviceReportACID = serviceReportACID.Where(i => i != 0).ToList();

                var searchResults = _context.ServiceReports.Include(x => x.Property).Include(x => x.ServiceReportStatus).Include(x => x.SubContractor).Where(x =>
                                            x.AgencyId == filterModel.AgencyId
                                            && (x.ServiceReportStatusId != 9 && x.ServiceReportStatusId != 3)
                                            && (!(x.SubContractor.SchoolSecurity && (x.ServiceReportStatusId == 8 || x.ServiceReportStatusId == 10))) // do not show completed or closed WO from School security LC
                            );

                if (filterModel.PropertyId > 0)
                {
                    searchResults = searchResults.Where(x => x.PropertyId == filterModel.PropertyId);
                }
                else if (userPropertyIds.Count > 0)
                {
                    searchResults = searchResults.Where(x => userPropertyIds.Contains(x.PropertyId.Value));
                }
                if (userRoleInAssetCare.Equals("MFUMGR") || userRoleInAssetCare.Equals("MFUOFFICER"))
                {
                    searchResults = searchResults.Where(x => (x.CallLogFITS != null) && (x.CallLogFITS.IsInsuranceEvent == true || x.CallLogFITS.IsInsuranceEventByWO == true) && (x.ReimbursementComplete != true));

                }
                searchResults = ((userPropertyIds != null) && userPropertyIds.Any()) ? searchResults.Where(m => userPropertyIds.Contains(m.PropertyId.Value)) : searchResults;

                var models = searchResults.Select(d => new JobListForHPViewModel
                {
                    SubContractorId = d.SubContractorId,
                    Id = d.ServiceReportId,
                    SchoolId = d.Property.SchoolId,
                    SchoolName = d.Property != null ? d.Property.PropertyDesc : "",
                    Description = d.ServiceReportDesc,
                    Status = d.ServiceReportStatus != null ? d.ServiceReportStatus.ServiceReportStatusDesc : "",
                    JobID = d.AgencySequenceId.Value,
                    PropertyID = (int)d.PropertyId,
                    WODate = d.RecordedDate,
                    RequestedBy = d.CallLogFITS.RequestedBy,
                    LocalContractorName = d.SubContractor.Name,
                    ServiceReportStatusId = d.ServiceReportStatusId,
                    ServiceReportACID = d.ServiceReportACID,
                    IsIncludeBetterment = (d.IsIncludeBetterment == true) ? "Yes" : "No",
                    IsReimbursementComplete = (d.ReimbursementComplete == true) ? "Yes" : "No"
                });

                if (!string.IsNullOrEmpty(filterModel.SearchBy))
                {
                    string serchText = filterModel.SearchBy.ToLower();
                    models = string.IsNullOrEmpty(filterModel.SearchBy)
                                ? models
                                 : models.Where(f => f.SchoolName.ToLower().Contains(serchText) ||
                                f.SchoolName.ToString().Contains(serchText) ||
                                f.JobID.ToString().Contains(serchText) ||
                                f.Description.ToString().Contains(serchText) ||
                                //f.WODateList.ToString().Contains(serchText) ||
                                f.RequestedBy.ToString().Contains(serchText) ||
                                f.Status.ToString().Contains(serchText) ||
                                f.IsIncludeBetterment.ToString().Contains(serchText) ||
                                f.IsReimbursementComplete.ToString().Contains(serchText)
                                );
                }

                if (userRoleInAssetCare.Equals("LOCALCONTRACTOR"))
                {
                    models = models.Where(x => x.SubContractorId.HasValue ? x.SubContractorId == DefaultContractorID : true);
                    models = models.Where(x => x.ServiceReportStatusId == 1);
                }
                else if (!(userRoleInAssetCare.Equals("MFUMGR") || userRoleInAssetCare.Equals("MFUOFFICER")))
                {
                    models = models.Where(x => (serviceReportACID.Count > 0 ? serviceReportACID.Contains(x.ServiceReportACID.Value) : true));
                }

                if (filterModel.ColumnSearches != null && filterModel.ColumnSearches.Count > 0)
                {
                    foreach (var columnSearch in filterModel.ColumnSearches)
                    {
                        if (columnSearch.Key.ToLower() == "jobid")
                        {
                            models = models.Where(f => f.JobID.ToString().Contains(columnSearch.Value));
                        }
                        else if (columnSearch.Key.ToLower() == "wodate")
                        {
                            models = models.Where(f => ((f.WODate.Value.Day < 10 ? "0" + f.WODate.Value.Day.ToString() : f.WODate.Value.Day.ToString()) + "-" +
                                              (f.WODate.Value.Month < 10 ? "0" + f.WODate.Value.Month.ToString() : f.WODate.Value.Month.ToString()) + "-" +
                                              f.WODate.Value.Year.ToString())
                                              .Contains(columnSearch.Value));
                        }
                        else
                        {
                            models = models.Where($"{columnSearch.Key}.ToString().ToLower().Contains(@0)", columnSearch.Value);

                        }
                    }
                }


                if (filterModel.SortBy != null)
                {
                    models = models.OrderBy($"{filterModel.SortBy} {filterModel.SortDir}");
                }

                filteredResultsCount = models.Count();
                totalResultsCount = searchResults.Count();
                transaction.Commit();

                if (filterModel.Take > 0)
                {
                    return models.Skip(filterModel.Skip).Take(filterModel.Take);
                }
                return models.ToList();
            }
        }

        private static readonly HashSet<string> AmuApprovalRolesFullName = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            // roles whose "awaiting review" is Claim.AMUApproved rather than Claim.FMCReviewed
            "DETAMU", "GROUPLEADER", "SRGRPLEADER", "DETAMCU", "AMUOFFICER",
            "SUPPORT", "EDAM", "DIRAM", "MFUMGR", "MFUOFFICER"
        };

        public IEnumerable<JobListForHPViewModel> FilterJobDataForHPClaim(CallOutFilterModel filterModel, bool dataForExport, List<int> userPropertyIds, int DefaultContractorID, string userRoleInAssetCare, out int filteredResultsCount, out int totalResultsCount)
        {
            List<int?> serviceReportACID = new List<int?>();
            if (filterModel.ServiceReportACIDClaim != null)
            {
                serviceReportACID = filterModel.ServiceReportACIDClaim
                                    .Split(',')
                                    .Where(x => int.TryParse(x, out _))
                                    .Select(int.Parse)
                                    .ToList().ConvertAll<int?>(x => x);

                serviceReportACID = serviceReportACID.Where(i => i != 0).ToList();
            }

            bool claimReviewedByAMU = AmuApprovalRolesFullName.Contains(userRoleInAssetCare);

            using (var transaction = _context.Database.BeginTransaction(System.Data.IsolationLevel.ReadUncommitted))
            {
                IQueryable<JobListForHPViewModel> searchResults = _context.Claims.Where(c =>
                            c.AgencyId == filterModel.AgencyId
                            && (filterModel.PropertyId > 0 ? (c.PropertyId == filterModel.PropertyId) : (userPropertyIds.Count > 0 ? userPropertyIds.Contains(c.PropertyId.Value) : true))
                            && (serviceReportACID.Count > 0 ? serviceReportACID.Contains(c.ServiceReport.ServiceReportACID.Value) : true)
                            && (claimReviewedByAMU
                                    ? (c.AMUApproved == null || c.AMUApproved == false)
                                    : (c.FMCReviewed == null || c.FMCReviewed == false))
                            && (c.ServiceReport.ServiceReportStatus.ServiceReportStatusID != 9 || c.ServiceReport.ServiceReportStatus.ServiceReportStatusID != 3)
                            && (!(c.ServiceReport.SubContractor.SchoolSecurity) && (c.ServiceReport.ServiceReportStatusId == 10)) // do not show completed or closed WO from School security LC
                            && ((userRoleInAssetCare.Equals("MFUMGR") || userRoleInAssetCare.Equals("MFUOFFICER")) ?
                                                (c.ServiceReport.IsInsuranceEvent == true) : true)
                        )
                    .Select(
                c => new JobListForHPViewModel
                {
                    SubContractorId = c.SubContractorId,
                    Id = c.ServiceReport.ServiceReportId,
                    SchoolId = c.ServiceReport.Property.SchoolId,
                    SchoolName = c.ServiceReport.Property != null ? c.ServiceReport.Property.PropertyDesc : "",
                    Description = c.ServiceReport.ServiceReportDesc,
                    //dataForExport ? d.ServiceReportDesc : (String.IsNullOrEmpty(d.ServiceReportDesc) ? String.Empty :
                    //        (d.ServiceReportDesc.Length > 50 ? d.ServiceReportDesc.Substring(0, 50) + "..." : d.ServiceReportDesc)),
                    Status = c.ServiceReport.ServiceReportStatus != null ? c.ServiceReport.ServiceReportStatus.ServiceReportStatusDesc : "",
                    JobID = c.ServiceReport.AgencySequenceId.Value,
                    PropertyID = (int)c.ServiceReport.PropertyId,
                    WODate = c.ServiceReport.RecordedDate,
                    WODateList = c.ServiceReport.RecordedDate.HasValue ? SqlFunctions.Replicate("0", 2 - SqlFunctions.DateName("dd", c.ServiceReport.RecordedDate).ToString().Length) + SqlFunctions.DateName("dd", c.ServiceReport.RecordedDate) + "-" +
                                                                                SqlFunctions.Replicate("0", 2 - SqlFunctions.DatePart("month", c.ServiceReport.RecordedDate).ToString().Length) + SqlFunctions.DatePart("month", c.ServiceReport.RecordedDate) + "-" +
                                                                                SqlFunctions.DateName("yyyy", c.ServiceReport.RecordedDate)
                                                                                : "",
                    RequestedBy = c.ServiceReport.CallLogFITS.RequestedBy,
                    LocalContractorName = _context.SubContractor.Where(r => r.ID == c.ServiceReport.SubContractorId).OrderByDescending(x => x.ID).FirstOrDefault().Name,
                });

                var filtered = ((userPropertyIds != null) && userPropertyIds.Any()) ? searchResults.Where(m => userPropertyIds.Contains(m.PropertyID)) : searchResults;
                if (userRoleInAssetCare.Equals("LOCALCONTRACTOR"))
                {
                    filtered = filtered.Where(x => x.SubContractorId.HasValue ? x.SubContractorId == DefaultContractorID : true);
                    //filtered = filtered.Where(x => x.Status == "Local Contractor Assigned");
                }

                if (!string.IsNullOrEmpty(filterModel.SearchBy))
                {
                    string serchText = filterModel.SearchBy.ToLower();
                    filtered = string.IsNullOrEmpty(filterModel.SearchBy)
                                ? searchResults
                                 : searchResults.Where(f => f.SchoolName.ToLower().Contains(serchText) ||
                                f.SchoolName.ToString().Contains(serchText) ||
                                f.JobID.ToString().Contains(serchText) ||
                                f.Description.ToString().Contains(serchText) ||
                                 f.WODateList.ToString().Contains(serchText) ||
                                f.RequestedBy.ToString().Contains(serchText) ||
                                f.Status.ToString().Contains(serchText)
                                );
                }

                if (filterModel.ColumnSearches != null && filterModel.ColumnSearches.Count > 0)
                {
                    foreach (var columnSearch in filterModel.ColumnSearches)
                    {
                        if (columnSearch.Key.ToLower() == "jobid")
                        {
                            filtered = filtered.Where(f => f.JobID.ToString().Contains(columnSearch.Value));
                        }
                        else if (columnSearch.Key.ToLower() == "wodate")
                        {
                            filtered = filtered.Where(f => ((f.WODate.Value.Day < 10 ? "0" + f.WODate.Value.Day.ToString() : f.WODate.Value.Day.ToString()) + "-" +
                                              (f.WODate.Value.Month < 10 ? "0" + f.WODate.Value.Month.ToString() : f.WODate.Value.Month.ToString()) + "-" +
                                              f.WODate.Value.Year.ToString())
                                              .Contains(columnSearch.Value));
                        }
                        else
                        {
                            filtered = filtered.Where($"{columnSearch.Key}.ToString().ToLower().Contains(@0)", columnSearch.Value);

                        }
                    }
                }


                if (filterModel.SortBy != null)
                {
                    filtered = filtered.OrderBy($"{filterModel.SortBy} {filterModel.SortDir}");
                }

                filteredResultsCount = filtered.Count();
                totalResultsCount = searchResults.Count();
                transaction.Commit();

                if (filterModel.Take == -1)
                    return filtered;
                return filtered.Skip(filterModel.Skip).Take(filterModel.Take);
            }
        }

        public ItemsCounts GetServiceReportCounts(int agencyID, int propertyID, List<int> userPropertyIds, string roleID, int? DefaultContractorID)
        {
            int serviceReportACID = 0;
            string serviceReportACIDClaim = "0";
            bool claimReviewedByAMU = false;   // false = check Claim.FMCReviewed, true = check Claim.AMUApproved
            switch (roleID)
            {
                case "LC":
                    serviceReportACID = 2;
                    serviceReportACIDClaim = "3,6,9";
                    break;
                case "C":
                    serviceReportACID = 4;
                    serviceReportACIDClaim = "7";
                    claimReviewedByAMU = false;
                    break;
                case "D":
                case "GL":
                case "SL":
                case "AM":
                case "EM":
                case "AO":
                case "P":
                case "E":
                case "MO":
                    serviceReportACID = 5;
                    serviceReportACIDClaim = "8";
                    claimReviewedByAMU = true;
                    break;
                default:
                    break;
            }

            List<int?> lstServiceReportACIDClaim = new List<int?>();
            lstServiceReportACIDClaim = serviceReportACIDClaim
                                .Split(',')
                                .Where(x => int.TryParse(x, out _))
                                .Select(int.Parse)
                                .ToList().ConvertAll<int?>(x => x);

            lstServiceReportACIDClaim = lstServiceReportACIDClaim.Where(i => i != 0).ToList();

            using (var dbContextTransaction = _context.Database.BeginTransaction(IsolationLevel.ReadUncommitted))
            {
                var searchReactiveWO = _context.ServiceReports.Where(x => x.AgencyId == agencyID &&
                        (propertyID > 0 ? (x.PropertyId == propertyID) : (userPropertyIds.Count > 0 ? userPropertyIds.Contains(x.PropertyId.Value) : true))
                        // &&  (roleID=="LC"?  x.ServiceReportACID == serviceReportACID:true)
                        && (x.ServiceReportStatus.ServiceReportStatusID != 9 && x.ServiceReportStatus.ServiceReportStatusID != 3)
                        && (!DefaultContractorID.HasValue || DefaultContractorID.HasValue && (x.SubContractorId.HasValue ? x.SubContractorId == DefaultContractorID : true))
                        && (x.CallLogFITSId.HasValue && ((roleID.Equals("E") || roleID.Equals("MO")) ?
                                    x.CallLogFITS.CallLogType.CallLogTypeID == 2 : true) || !x.CallLogFITSId.HasValue)

                        );

                //var searchReactiveWOComplete = (from s in _context.ServiceReports
                //                                where (s.ServiceReportId != 3 && s.ServiceReportId != 9)
                //                                select s).ToList();

                var searchReactiveWOComplete = _context.ServiceReports.Where(x => x.AgencyId == agencyID &&
                            (propertyID > 0 ? (x.PropertyId == propertyID) : (userPropertyIds.Count > 0 ? userPropertyIds.Contains(x.PropertyId.Value) : true))
                            // &&  (roleID=="LC"?  x.ServiceReportACID == serviceReportACID:true)
                            && (x.ServiceReportStatus.ServiceReportStatusID != 9 && x.ServiceReportStatus.ServiceReportStatusID != 3)
                            && (!DefaultContractorID.HasValue || DefaultContractorID.HasValue && (x.SubContractorId.HasValue ? x.SubContractorId == DefaultContractorID : true))
                            && (x.CallLogFITSId.HasValue && ((roleID.Equals("E") || roleID.Equals("MO")) ?
                                        x.CallLogFITS.CallLogType.CallLogTypeID == 2 : true) || !x.CallLogFITSId.HasValue)

                            );

                var searchReactiveWOClaim = _context.Claims.Where(c => c.AgencyId == agencyID &&
                            (propertyID > 0 ? (c.PropertyId == propertyID) : (userPropertyIds.Count > 0 ? userPropertyIds.Contains(c.PropertyId.Value) : true))
                            && (lstServiceReportACIDClaim.Count > 0 ? lstServiceReportACIDClaim.Contains(c.ServiceReport.ServiceReportACID.Value) : true)
                            && (claimReviewedByAMU
                                    ? (c.AMUApproved == null || c.AMUApproved == false)
                                    : (c.FMCReviewed == null || c.FMCReviewed == false))
                            && (c.ServiceReport.ServiceReportStatus.ServiceReportStatusID != 9 && c.ServiceReport.ServiceReportStatus.ServiceReportStatusID != 3)
                            && (c.ServiceReport.ServiceReportStatus.ServiceReportStatusID == 10)
                            && (!DefaultContractorID.HasValue || DefaultContractorID.HasValue && (c.SubContractorId.HasValue ? c.SubContractorId == DefaultContractorID : true))
                            && (c.ServiceReport.CallLogFITSId.HasValue && ((roleID.Equals("E") || roleID.Equals("MO")) ?
                                        c.ServiceReport.CallLogFITS.CallLogType.CallLogTypeID == 2 : true) || !c.ServiceReport.CallLogFITSId.HasValue)
                            );

                //var searchReactiveWONotClosed = _context.ServiceReports
                // .GroupJoin(_context.Claims,
                //            s => s.ServiceReportId,
                //            t => t.ServiceReportId,
                //            (s, t) => new { Staff = s, Training = t.FirstOrDefault() })
                // .Where(st => (st.Staff.ServiceReportStatusId != 9 && st.Staff.ServiceReportStatusId != 3)
                //              && st.Training.AMUApproved != true).ToList();


                var reactiveWONotClosedQuery = _context.ServiceReports.Where(x => x.AgencyId == agencyID &&
                            (propertyID > 0 ? (x.PropertyId == propertyID) : (userPropertyIds.Count > 0 ? userPropertyIds.Contains(x.PropertyId.Value) : true))
                            && (x.ServiceReportStatus.ServiceReportStatusID != 9 && x.ServiceReportStatus.ServiceReportStatusID != 3
                            && x.ServiceReportStatus.ServiceReportStatusID != 10)
                            && (x.CallLogFITSId.HasValue && ((roleID.Equals("E") || roleID.Equals("MO")) ?
                                        x.CallLogFITS.CallLogType.CallLogTypeID == 2 : true) || !x.CallLogFITSId.HasValue));



                //var reactiveWONotClosedQuery = from WO in _context.ServiceReports join C in _context.Claims on WO.ServiceReportId equals C.ServiceReportId into grpJoin                    
                //         from sr in grpJoin.DefaultIfEmpty()
                //         where (WO.ServiceReportStatusId != 9 && WO.ServiceReportStatusId != 3) && (sr.AMUApproved != true) &&
                //                WO.AgencyId == agencyID  &&
                //                (propertyID > 0 ? (WO.PropertyId == propertyID) : (userPropertyIds.Count > 0 ? userPropertyIds.Contains(WO.PropertyId.Value) : true)//)
                //                )
                //         select new { 
                //                  WO = WO,
                //             FMCReviewed = sr.FMCReviewed
                //         };
                //if (roleID.Equals("C"))
                //{
                //    reactiveWONotClosedQuery = reactiveWONotClosedQuery.Where(x => x.FMCReviewed != true);
                //}
                var searchReactiveWONotClosed = reactiveWONotClosedQuery;

                ItemsCounts itemsCounts = new ItemsCounts()
                {
                    //Local Contractor Assigned && Local Contractor Accepted(new/open/inprogress)
                    NewCount = searchReactiveWO.Where(sr => sr.ServiceReportStatus.ServiceReportStatusID == 1).Count(),
                    //Same as Estimate review/approval table
                    EstimateReviewCount = searchReactiveWO.Where(sr => (serviceReportACID > 0 ? sr.ServiceReportACID == serviceReportACID : true)).Count(),
                    //Same as Claim review/approval table
                    ClaimReviewCount = searchReactiveWOClaim.Count(),

                    //Closed
                    CompletedCount = searchReactiveWOComplete.Where(sr => sr.ServiceReportStatus.ServiceReportStatusID == 10).Count(),
                    //Other than Local Contractor Assigned && Local Contractor Accepted,closed
                    InProgressCount = searchReactiveWO.Where(sr => (sr.ServiceReportStatus.ServiceReportStatusID != 1
                                                                  && sr.ServiceReportStatus.ServiceReportStatusID != 10)

                                                                  ).Count()
                };
                itemsCounts.OpenedCount = searchReactiveWONotClosed.Count() - itemsCounts.EstimateReviewCount;
                dbContextTransaction.Commit();

                return itemsCounts;
            }
        }



        public IEnumerable<JobListForHPViewModel> FilterJobApprovalDataForHP(CallOutFilterModel filterModel, bool dataForExport, IEnumerable<int> userPropertyIds, string userRoleInAssetCare, out int filteredResultsCount, out int totalResultsCount)
        {
            using (var dbContextTransaction = _context.Database.BeginTransaction(IsolationLevel.Snapshot))
            {
                IQueryable<JobListForHPViewModel> model = (("CONTRACTOR".Equals(userRoleInAssetCare) || "LOCALCONTRACTOR".Equals(userRoleInAssetCare)) ? _context.ServiceReports
                    .Where(x =>
                                (
                                    x.AgencyId == filterModel.AgencyId &&
                                    (x.WorkCategoryId >= 1 && x.WorkCategoryId <= 7) &&
                                    (x.DETAccepted.HasValue && x.DETAccepted.Value &&
                                    x.CommerceNotInvolved.HasValue && x.CommerceNotInvolved.Value ||
                                    x.CommenceWork.HasValue && x.CommenceWork.Value) &&
                                    (x.ServiceReportStatusId == null || x.ServiceReportStatusId.HasValue && x.ServiceReportStatusId.Value != 9)
                                    && (!(x.SubContractor.SchoolSecurity && (x.ServiceReportStatusId == 8 || x.ServiceReportStatusId == 10))) // do not show completed or closed WO from School security LC
                                    &&
                                    (!_context.ServiceReportLineItems.Where(sli => sli.AgencyID == filterModel.AgencyId && sli.ServiceReportID == x.ServiceReportId && sli.Deleted != true).Any() || _context.ServiceReportLineItems.Where(sli => sli.AgencyID == filterModel.AgencyId && sli.ServiceReportID == x.ServiceReportId && sli.Deleted != true).Any() && _context.ServiceReportLineItems.Where(sli => sli.AgencyID == filterModel.AgencyId && sli.ServiceReportID == x.ServiceReportId && sli.Deleted != true).Sum(sli => sli.Amount) == 0)
                                ) ||
                                (x.ServiceReportAC.Code == "4C" &&
                                x.AgencyId == filterModel.AgencyId)
                            )
                            .Select(
                                d => new JobListForHPViewModel
                                {
                                    Id = d.ServiceReportId,
                                    JobID = d.AgencySequenceId.HasValue ? d.AgencySequenceId.Value : 0,
                                    JobType = "Job",
                                    ReceivedDate = d.DETApprovalDate,
                                    SchoolId = (d.Property == null) ? 0 : d.Property.SchoolId,
                                    PropertyID = (d.Property == null) ? 0 : d.Property.PropertyId,
                                    SchoolName = (d.Property == null) ? "" : d.Property.PropertyShortDesc,
                                    Description = d.ServiceReportDesc,
                                    Estimate = d.EstimatedAmount
                                }) : ("DETAMU".Equals(userRoleInAssetCare) ||
                                        "AMUOFFICER".Equals(userRoleInAssetCare) ||
                                        "DETAMCU".Equals(userRoleInAssetCare) ||
                                        "AMCUOFFICER".Equals(userRoleInAssetCare) ||
                                        "GROUPLEADER".Equals(userRoleInAssetCare) ||
                                        "SRGRPLEADER".Equals(userRoleInAssetCare) ||
                                        "DIRAM".Equals(userRoleInAssetCare) ||
                                        "EDAM".Equals(userRoleInAssetCare)
                                        ) ?
                            _context.ServiceReports.Where(
                            x => x.AgencyId == filterModel.AgencyId &&
                            (x.ServiceReportStatusId == null || x.ServiceReportStatusId.HasValue && x.ServiceReportStatusId.Value != 9) &&
                                                                     x.ServiceReportAC.Code == "3D")
                            .Select(
                                d => new JobListForHPViewModel
                                {
                                    Id = d.ServiceReportId,
                                    JobID = d.AgencySequenceId.Value,
                                    JobType = "Job",
                                    ReceivedDate = d.DETApprovalDate,
                                    SchoolId = (d.Property == null) ? 0 : d.Property.SchoolId,
                                    PropertyID = (d.Property == null) ? 0 : d.Property.PropertyId,
                                    SchoolName = (d.Property == null) ? "" : d.Property.PropertyShortDesc,
                                    Description = d.ServiceReportDesc,
                                    Estimate = d.EstimatedAmount
                                }) : "SUPERREP".Equals(userRoleInAssetCare) ?
                                 _context.ServiceReports.Where(
                                        x => x.AgencyId == filterModel.AgencyId &&
                                             x.ServiceReportAC.Code == "4A"
                             )
                            .Select(
                                d => new JobListForHPViewModel
                                {
                                    Id = d.ServiceReportId,
                                    JobID = d.AgencySequenceId.Value,
                                    JobType = "Job",
                                    ReceivedDate = d.DETApprovalDate,
                                    SchoolId = (d.Property == null) ? 0 : d.Property.SchoolId,
                                    PropertyID = (d.Property == null) ? 0 : d.Property.PropertyId,
                                    SchoolName = (d.Property == null) ? "" : d.Property.PropertyShortDesc,
                                    Description = d.ServiceReportDesc,
                                    Estimate = d.EstimatedAmount
                                }) :
                             _context.ServiceReports.Where(
                            x => x.AgencyId == filterModel.AgencyId &&
                            (x.DETAccepted.HasValue && x.DETAccepted.Value) &&
                            x.CommerceNotInvolved.HasValue && !x.CommerceNotInvolved.Value &&
                            x.CommenceWork.HasValue && !x.CommenceWork.Value &&
                            (x.ServiceReportStatusId == null || x.ServiceReportStatusId.HasValue && x.ServiceReportStatusId.Value != 9))
                            .Select(
                                d => new JobListForHPViewModel
                                {
                                    Id = d.ServiceReportId,
                                    JobID = d.AgencySequenceId.Value,
                                    JobType = "Job",
                                    ReceivedDate = d.DETApprovalDate,
                                    SchoolId = (d.Property == null) ? 0 : d.Property.SchoolId,
                                    PropertyID = (d.Property == null) ? 0 : d.Property.PropertyId,
                                    SchoolName = (d.Property == null) ? "" : d.Property.PropertyShortDesc,
                                    Description = d.ServiceReportDesc,
                                    Estimate = d.EstimatedAmount
                                })).Concat(
                                    "CONTRACTOR".Equals(userRoleInAssetCare) ? _context.ContractVariations.Where(v =>
                                                v.AgencyID == filterModel.AgencyId &&
                                                v.CommenceWork.HasValue && v.CommenceWork.Value &&
                                                (!v.ContractVariationStatusID.HasValue || v.ContractVariationStatusID.HasValue && v.ContractVariationStatusID.Value != 5)
                                                //&&
                                                //(!_context.ContractVariationLineItems.Where(vli => vli.AgencyID == filterModel.AgencyId && vli.ContractVariationID == v.ContractVariationID).Any() || _context.ContractVariationLineItems.Where(vli => vli.AgencyID == filterModel.AgencyId && vli.ContractVariationID == v.ContractVariationID).Any() && _context.ContractVariationLineItems.Where(vli => vli.AgencyID == filterModel.AgencyId && vli.ContractVariationID == v.ContractVariationID).Sum(vli => ((vli == null) ? 0 : vli.Amount)) == 0)
                                                )
                                                .Select(
                                                    v => new JobListForHPViewModel
                                                    {
                                                        Id = v.ContractVariationID,
                                                        JobID = v.AgencySequenceID ?? 0,
                                                        JobType = "Variation",
                                                        ReceivedDate = v.CommenceWorkDate,
                                                        SchoolId = (v.Property == null) ? 0 : v.Property.SchoolId,
                                                        PropertyID = (v.Property == null) ? 0 : v.Property.PropertyId,
                                                        SchoolName = (v.Property == null) ? "" : v.Property.PropertyShortDesc,
                                                        Description = v.CVDesc,
                                                        Estimate = v.EstimateValue
                                                    }) :
                                                    (("DETAMU".Equals(userRoleInAssetCare) ||
                                                        "AMUOFFICER".Equals(userRoleInAssetCare) ||
                                                         "DETAMCU".Equals(userRoleInAssetCare) ||
                                                         "AMCUOFFICER".Equals(userRoleInAssetCare) ||
                                                         "GROUPLEADER".Equals(userRoleInAssetCare) ||
                                                         "SRGRPLEADER".Equals(userRoleInAssetCare) ||
                                                         "DIRAM".Equals(userRoleInAssetCare) ||
                                                         "EDAM".Equals(userRoleInAssetCare)) ?
                                                        _context.ContractVariations.Where(v =>
                                                        v.AgencyID == filterModel.AgencyId &&
                                                        v.ContractorFinalise.HasValue && v.ContractorFinalise.Value &&
                                                        ((v.DETAMCUApproval.HasValue && v.DETAMCUApproval.Value) ||
                                                        (v.DETAMUApproval.HasValue && v.DETAMUApproval.Value)) &&
                                                        (!v.DETAccepted.HasValue || v.DETAccepted.HasValue && !v.DETAccepted.Value) &&
                                                        (((!v.MWPDAccepted.HasValue || v.MWPDAccepted.HasValue && !v.MWPDAccepted.Value) && (!v.MWPDApproval.HasValue || v.MWPDApproval.HasValue && !v.MWPDApproval.Value)) ||
                                                        (!v.MWPDAccepted.HasValue && !v.MWPDApproval.HasValue)) &&
                                                        (!v.ContractVariationStatusID.HasValue || v.ContractVariationStatusID.HasValue && v.ContractVariationStatusID.Value != 5))
                                                        .Select(
                                                            v => new JobListForHPViewModel
                                                            {
                                                                Id = v.ContractVariationID,
                                                                JobID = v.AgencySequenceID ?? 0,
                                                                JobType = "Variation",
                                                                ReceivedDate = v.CommenceWorkDate,
                                                                SchoolId = (v.Property == null) ? 0 : v.Property.SchoolId,
                                                                PropertyID = (v.Property == null) ? 0 : v.Property.PropertyId,
                                                                SchoolName = (v.Property == null) ? "" : v.Property.PropertyShortDesc,
                                                                Description = v.CVDesc,
                                                                Estimate = v.EstimateValue
                                                            })
                                                        :
                                                         ("SUPERREP".Equals(userRoleInAssetCare) ?
                                                        _context.ContractVariations.Where(v =>
                                                        v.AgencyID == filterModel.AgencyId &&
                                                        (v.DETAccepted.HasValue && v.DETAccepted.Value || v.MWPDAccepted.HasValue && v.MWPDAccepted.Value) &&
                                                         (!v.CommenceWork.HasValue || v.CommenceWork.HasValue && !v.CommenceWork.Value) &&
                                                        (!v.ContractVariationStatusID.HasValue || v.ContractVariationStatusID.HasValue && v.ContractVariationStatusID.Value != 5))
                                                        .Select(
                                                            v => new JobListForHPViewModel
                                                            {
                                                                Id = v.ContractVariationID,
                                                                JobID = v.AgencySequenceID ?? 0,
                                                                JobType = "Variation",
                                                                ReceivedDate = v.CommenceWorkDate,
                                                                SchoolId = (v.Property == null) ? 0 : v.Property.SchoolId,
                                                                PropertyID = (v.Property == null) ? 0 : v.Property.PropertyId,
                                                                SchoolName = (v.Property == null) ? "" : v.Property.PropertyShortDesc,
                                                                Description = v.CVDesc,
                                                                Estimate = v.EstimateValue
                                                            })
                                                         : _context.ContractVariations.Where(v => false)
                                                        .Select(
                                                            v => new JobListForHPViewModel
                                                            {
                                                                Id = v.ContractVariationID,
                                                                JobID = v.AgencySequenceID ?? 0,
                                                                JobType = "Variation",
                                                                ReceivedDate = v.CommenceWorkDate,
                                                                SchoolId = (v.Property == null) ? 0 : v.Property.SchoolId,
                                                                PropertyID = (v.Property == null) ? 0 : v.Property.PropertyId,
                                                                SchoolName = (v.Property == null) ? "" : v.Property.PropertyShortDesc,
                                                                Description = v.CVDesc,
                                                                Estimate = v.EstimateValue
                                                            }))
                                                    )
                                );

                var filtered = ((userPropertyIds != null) && userPropertyIds.Any()) ? model.Where(m => userPropertyIds.Contains(m.PropertyID)) : model;
                var dynamicQuery = PredicateHelper.BuildDynamicWhereClause<JobListForHPViewModel>(filterModel.SearchBy.ToLower());
                filtered = string.IsNullOrEmpty(filterModel.SearchBy) ? filtered : filtered.Where(dynamicQuery);

                if (filterModel.SortBy != null)
                {
                    filtered = filtered.OrderBy($"{filterModel.SortBy} {filterModel.SortDir}");
                }

                filteredResultsCount = filtered.Count();
                totalResultsCount = model.Count();
                dbContextTransaction.Commit();
                if (filterModel.Take == -1)
                    return filtered;
                return filtered.Skip(filterModel.Skip).Take(filterModel.Take);
            }
        }

        public IEnumerable<JobListForSchoolHPViewModel> FilterJobDataForSchoolHP(CallOutFilterModel filterModel, UserViewModel currentUser, bool dataForExport, IEnumerable<int> userPropertyIds, string userRoleInAssetCare, out int filteredResultsCount, out int totalResultsCount)
        {
            using (var transaction = _context.Database.BeginTransaction(System.Data.IsolationLevel.ReadUncommitted))
            {
                var calloutIds = (from cl in _context.CallLogFITS
                                  join sr in _context.ServiceReports on cl.CallLogFITSID equals sr.CallLogFITSId into clsr
                                  from clsr2 in clsr.DefaultIfEmpty()
                                  where
                                          !(cl.DeletedYN.HasValue && cl.DeletedYN.Value) && /* exclude deleted rows */
                                          (cl.CallLogStatus.CallLogStatusDesc != "Cancelled") /* exclude cancelled rows */
                                  select new { Id = cl.CallLogFITSID }).Distinct();
                IQueryable<JobListForSchoolHPViewModel> model = from cl in _context.CallLogFITS
                                                                join ci in calloutIds on cl.CallLogFITSID equals ci.Id into clis
                                                                from cli in clis
                                                                let hasSR = _context.ServiceReports.Any(sr => sr.CallLogFITSId == cli.Id)
                                                                let srForExpectedVisitDate = _context.ServiceReports.Where(sr => sr.CallLogFITSId == cli.Id && sr.ExpectedVisitDate != null &&
                                                                        (sr.ServiceReportStatusId != 3 && sr.ServiceReportStatusId != 9 && sr.ServiceReportStatusId != 10 && sr.ServiceReportStatusId != 11)).OrderBy(x => x.ExpectedVisitDate)
                                                                        .FirstOrDefault()
                                                                let srForEstimatedCompletionDate = _context.ServiceReports.Where(sr => sr.CallLogFITSId == cli.Id && sr.EstimatedCompletionDate != null &&
                                                                        sr.ServiceReportStatusId != 9).OrderByDescending(x => x.EstimatedCompletionDate)
                                                                        .FirstOrDefault()
                                                                let firstSR = _context.ServiceReports.FirstOrDefault(sr => sr.CallLogFITSId == cli.Id)
                                                                let LocalContractorName = hasSR ? (firstSR.SubContractorId.HasValue ? _context.SubContractor.FirstOrDefault(lc => lc.ID == firstSR.SubContractorId.Value).Name : "") : ""
                                                                let hasSchoolContributionSR = _context.ServiceReports.Any(sr => sr.CallLogFITSId == cli.Id && sr.IsSchoolContribution.HasValue && sr.IsSchoolContribution.Value)
                                                                let theSchoolContributionSR = _context.ServiceReports.FirstOrDefault(sr => sr.CallLogFITSId == cli.Id && sr.IsSchoolContribution.HasValue && sr.IsSchoolContribution.Value)
                                                                select new JobListForSchoolHPViewModel
                                                                {
                                                                    Id = cl.CallLogFITSID,
                                                                    AgencyID = cl.AgencyID.Value,
                                                                    PropertyID = (cl.PropertyID.HasValue) ? cl.PropertyID.Value : 0,
                                                                    WorkCategoryID = hasSR ? firstSR.WorkCategoryId.Value : 0,
                                                                    WorkCategory = hasSR ? firstSR.WorkCategory.Description : "",
                                                                    ServiceReportId = hasSR ? firstSR.ServiceReportId : 0,
                                                                    Key = hasSR ? firstSR.ServiceReportId : 0,
                                                                    JobID = hasSR ? firstSR.AgencySequenceId.Value : 0,
                                                                    CallOutKey = cl.CallLogFITSID,
                                                                    CallOutID = (cl.AgencySequenceID.HasValue) ? cl.AgencySequenceID.Value : 0,
                                                                    CallDate = cl.CallDate,
                                                                    SchoolName = (cl.Property == null) ? "" : cl.Property.PropertyShortDesc,
                                                                    CallDescription = cl.CallLogDesc,
                                                                    CallCategory = _context.Proposal.Any(p => p.CallLogFITSID == cl.CallLogFITSID)
                                                                        ? "LRLV School Funded"
                                                                        : cl.IsSecurityIncident == true
                                                                        ? "Security"
                                                                        : cl.CallLogType.CallLogTypeDesc,
                                                                    IsSchoolontribution = hasSchoolContributionSR ? theSchoolContributionSR.IsSchoolContribution : false,
                                                                    IsSchoolontributionText = hasSchoolContributionSR ? "Yes" : "No",
                                                                    SchoolAmount = (hasSchoolContributionSR) ?
                                                                                       (_context.Claims.Where(c => c.ServiceReportId == theSchoolContributionSR.ServiceReportId).Any() ?
                                                                                            ((_context.Claims.Where(c => c.ServiceReportId == theSchoolContributionSR.ServiceReportId).Sum(c => c.ClaimedAmount) < theSchoolContributionSR.SchoolLimit) ?
                                                                                              _context.Claims.Where(c => c.ServiceReportId == theSchoolContributionSR.ServiceReportId).Sum(c => c.ClaimedAmount) : theSchoolContributionSR.SchoolLimit) : 0.00M) : 0.00M,
                                                                    LocalContractorName = LocalContractorName,
                                                                    LCExpectedVisitDate = hasSR ? srForExpectedVisitDate.ExpectedVisitDate : null,
                                                                    CallStatus = (cl.CallLogStatus == null) ? "" : cl.CallLogStatus.CallLogStatusDesc,
                                                                    WOCount = _context.ServiceReports.Where(r => r.CallLogFITSId == cl.CallLogFITSID && r.ServiceReportStatusId != 9).OrderByDescending(x => x.ServiceReportId).Count(),
                                                                    CompletionDate = srForEstimatedCompletionDate != null ? srForEstimatedCompletionDate.EstimatedCompletionDate : null,
                                                                    SchoolUrgency = _context.CallLogFinanceUrgency.Where(x => x.CallLogFinanceUrgencyID == cl.SchoolUrgencyID).OrderByDescending(x => x.CallLogFinanceUrgencyID).FirstOrDefault().CallLogFinanceUrgencyDesc,

                                                                };
                var userPropertyIDs = currentUser.UserPropertyIds;
                if (filterModel.PropertyId.HasValue)
                {
                    userPropertyIDs = new List<int>() { filterModel.PropertyId.Value };
                }

                var userPropertyAgencyIDs = currentUser.UserPropertyAgencyIds;
                var userAgencyIDs = currentUser.UserAgencyIds;
                model = model.Where(m => userAgencyIDs.Contains(m.AgencyID) &&
                                         (!userPropertyIDs.Any() ||
                                                        userPropertyIDs.Any() && (userPropertyAgencyIDs.Contains(m.AgencyID) &&
                                                                                    userPropertyIDs.Contains(m.PropertyID))));
                var dynamicQuery = PredicateHelper.BuildDynamicWhereClause<JobListForSchoolHPViewModel>(filterModel.SearchBy.ToLower());
                var filtered = string.IsNullOrEmpty(filterModel.SearchBy) ? model : model.Where(dynamicQuery);
                //var dynamicQuery = PredicateHelper.BuildDynamicWhereClause<JobListForSchoolHPViewModel>(filterModel.ColumnSearches, new string[] { "Id", "AgencyID", "PropertyID", "WorkCategoryID", "ServiceReportId", "Key", "CallOutKey", "IsSchoolontribution", "RowNo" });
                //var filtered = string.IsNullOrEmpty(filterModel.SearchBy) && !filterModel.ColumnSearches.Any() ? model2 : model2.Where(dynamicQuery, filterModel.SearchBy.ToLower());

                if (filterModel.ColumnSearches.Any())
                {
                    var dynamicQuery2 = PredicateHelper.FindByColumnsExact<JobListForSchoolHPViewModel>(filterModel.ColumnSearches);
                    filtered = filtered.Where(dynamicQuery2);
                }

                if (filterModel.SortBy != null)
                {
                    filtered = filtered.OrderBy($"{filterModel.SortBy} {filterModel.SortDir}");
                }

                filteredResultsCount = filtered.Count();
                totalResultsCount = model.Count();
                transaction.Commit();

                if (filterModel.Take == -1)
                    return PatchJobList(filtered.ToList());
                var jobList = filtered.Skip(filterModel.Skip).Take(filterModel.Take);
                return PatchJobList(jobList.ToList());
            }
        }

        private IEnumerable<JobListForSchoolHPViewModel> PatchJobList(IEnumerable<JobListForSchoolHPViewModel> jobList)
        {
            var patchBuilingJobList = jobList.ToList();
            // Batch fetch all WO IDs in one query
            var callLogIds = patchBuilingJobList.Select(j => j.Id).ToList();

            var woIdLookup = _context.ServiceReports
                .Where(r => callLogIds.Contains(r.CallLogFITSId.Value) && r.AgencySequenceId != 9)
                .OrderByDescending(x => x.ServiceReportId)
                .Select(x => new { x.CallLogFITSId, x.AgencySequenceId })
                .ToList()
                .GroupBy(x => x.CallLogFITSId.Value)
                .ToDictionary(g => g.Key, g => string.Join(", ", g.Select(x => x.AgencySequenceId)));

            foreach (var job in patchBuilingJobList)
            {
                //var serviceReportBuildingIds = _context.ServiceReportBuildings
                //                         .Where(x => x.ServiceReportID == job.ServiceReportId)
                //                         .Select(d => d.BuildingID).ToList();
                //job.Buildings = String.Join(", ", _context.Buildings.Where(b => serviceReportBuildingIds.Contains(b.BuildingID)).Select(b => "(" + b.BuildingNo + ") " + b.BuildingDesc).ToArray()).MaxSize(40);
                //job.Buildings = job.Buildings.MaxSize(40);
                job.CallDescription = job.CallDescription.MaxSize(30);
                job.WOIDs = woIdLookup.TryGetValue(job.Id, out var ids) ? ids : "";
            }

            return patchBuilingJobList;
        }

        public CallLogFITS GetCallLogFitDetails(int agencyId, int callLogFITSId)
        {
            CallLogFITS fitDetails = _context.CallLogFITS.Where(x => x.CallLogFITSID == callLogFITSId).FirstOrDefault();

            return fitDetails;
        }

        public bool MutipleJobsBySameCalloutId(int agencyId, int callLogFITSId)
        {
            return _context.ServiceReports.Where(x => x.CallLogFITSId == callLogFITSId && x.ServiceReportStatusId != 9).Count() > 1;
        }

        public CallLogFITS GetCallLogFitDetails(int? callLogFITSId)
        {
            return _context.CallLogFITS.Where(x => x.CallLogFITSID == callLogFITSId).FirstOrDefault();
        }

        public int? GetProposalDetails(int? callLogFITSId)
        {
            return _context.Proposal.Where(x => x.CallLogFITSID == callLogFITSId).FirstOrDefault() == null ? 0 : _context.Proposal.Where(x => x.CallLogFITSID == callLogFITSId).FirstOrDefault().ProposalID;
        }

        public decimal GetCostAdjustment(int agencyId, string costAdjType, string financialYear, DateTime costingDate)
        {

            FinancialYear fy = _context.FinancialYears.Where(x => x.FinancialYearName == financialYear).FirstOrDefault();
            if (fy != null)
            {
                CostAdjustment costAdj = _context.CostAdjustments.Where(x => x.AgencyId == agencyId && x.FinancialYearId == fy.FinancialYearId
                 && (costingDate >= x.DateEffective && costingDate <= x.DateFinished)).FirstOrDefault();
                if (costAdj != null)
                {
                    if (costAdjType.ToLower() == "o")
                    {
                        return costAdj.OIndice.HasValue ? costAdj.OIndice.Value : (decimal)(1.000000);
                    }
                    else if (costAdjType.ToLower() == "l")
                    {
                        return costAdj.LIndice.HasValue ? costAdj.LIndice.Value : (decimal)(1.000000);

                    }
                }
            }

            return (decimal)(1.000000);
        }

        public List<int> GetCallLogFitsBuildingIds(int agencyId, int callLogFitsId)
        {
            return _context.CallLogBuildingFITSs
                                         .Where(x => x.CallLogFITSID == callLogFitsId && x.Deleted != true)
                                         .Select(d => d.BuildingID).ToList();
        }

        public List<ServiceReportLineItem> GetServiceReportLineItems(int serviceReportId)
        {
            return _context.ServiceReportLineItems.Where(x => x.ServiceReportID == serviceReportId && x.Deleted != true)
                .ToList();
        }

        public ServiceReport GetJobDetails(int agencyId, int jobId)
        {
            return _context.ServiceReports.
                 Where(x => x.ServiceReportId == jobId).FirstOrDefault();
        }

        public ServiceReport GetJobDetailsByJobNo(int agencyId, int jobNo)
        {
            return _context.ServiceReports.
                 Where(x => x.AgencyId == agencyId  && x.AgencySequenceId == jobNo).FirstOrDefault();
        }
        public bool IsMonthMayOrJuneWitAprilSubmitted(int orderId)
        {
            //Model.Models.Order order = _context.Orders.Where(x => x.OrderId == orderId).FirstOrDefault();
            //if (order != null)
            //{
            //    int agencyId = order.AgencyId;
            //    int? financialYearId = order.FinancialYearId;
            //    short year = Convert.ToInt16(order.FinancialYear.FinancialYearName.Split('/').FirstOrDefault());
            //    _context.ContractStatus.Where(x => x.AgencyId == agencyId && x.ContractStatusYear == year && x.)

            //}

            return false;
        }

        public bool HasPWASignedOff(int serviceReportId)
        {

            var commerceConfirmed = from CS in _context.ContractStatus
                                    from C in _context.Claims
                                    from WC in _context.WorkCategories
                                    from SR in _context.ServiceReports
                                    where CS.ContractStatusYear == C.ClaimYear && CS.ContractStatusMonth == C.ClaimMonth && CS.AgencyId == C.AgencyId &&
                                            C.WorkCategoryId == WC.WorkCategoryId && (C.ClaimTypeCode == "J" || C.ClaimTypeCode == "B") &&
                                            C.ReferringRecordId == SR.ServiceReportId &&
                                            C.AgencyId == SR.AgencyId && (SR.ServiceReportId == serviceReportId)
                                    orderby C.ClaimYear descending, C.ClaimMonth descending
                                    select CS.CommerceConfirmed;
            bool pwaSignedOff = commerceConfirmed.FirstOrDefault().HasValue ? commerceConfirmed.FirstOrDefault().Value : false;

            return pwaSignedOff;
        }

        public List<LCIReasons> GetLCIReasons()
        {
            return _context.LCIReasons.ToList();
        }

        public decimal GetClaimApprovedAmount(int jobId, int agencyId)
        {
            ClaimType jobClaimType = _context.ClaimTypes.Where(x => x.Code == "J").FirstOrDefault();
            ClaimType reaJobClaimType = _context.ClaimTypes.Where(x => x.Code == "B").FirstOrDefault();

            if (jobClaimType != null)
            {
                List<Claim> claims = _context.Claims.Where(x => x.ReferringRecordId == jobId && x.Approved == true
                && x.AgencyId == agencyId
                && (x.ClaimTypeId == jobClaimType.ClaimTypeId || x.ClaimTypeId == reaJobClaimType.ClaimTypeId)).ToList();

                var total = claims.Sum(x => x.ApprovedAmount);
                if (total.HasValue)
                {
                    return total.Value;
                }
            }

            return 0;
        }

        public List<ScheduleRateDropdownItem> GetScheduledRates(int agencyId, int? schduleRateCategoryId, int? subContractorId)
        {
            using (var dbContextTransaction = _context.Database.BeginTransaction(IsolationLevel.Snapshot))
            {
                List<ScheduleRateDropdownItem> scheduleNumberData = _context.ScheduledRates
                .Where(x => x.AgencyId == agencyId && x.Deleted.HasValue == false &&
                (subContractorId.HasValue ? x.SubContractorID == subContractorId.Value : true) &&
                (schduleRateCategoryId.HasValue ? x.ScheduledRateCategoryId == schduleRateCategoryId.Value : schduleRateCategoryId.HasValue == false))
                .OrderBy(y => y.ScheduleItemNo)
                .Select(d =>
                       new ScheduleRateDropdownItem()
                       {
                           ScheduledRateCategoryId = d.ScheduledRateCategoryId,
                           ScheduledRateID = (d.ScheduledRateId == 0 ? "" : d.ScheduledRateId.ToString()) + "|" +
                                 (d.ScheduleItemNo == null ? "" : d.ScheduleItemNo) + "|" +
                                 (d.Quantity) + "|" +
                                 (d.Unit == null ? "" : d.Unit.ToString()) + "|" +
                                 (d.Rate) + "|" + (d.CostAdjType == null ? "0" : d.CostAdjType.ToString()) + "|" +
                                 (d.Description == null ? "" : d.Description) + "|0|1",
                           ScheduledRateDesc = "Item " + (d.ScheduleItemNo == null ? "" : d.ScheduleItemNo) + "--" + //d.Description
                                   (d.Description.Length > 60 ? (d.Description.Substring(0, 60) + "...") : d.Description)
                       }).ToList();
                dbContextTransaction.Commit();
                return scheduleNumberData;
            }

        }

        public SchoolLimit GetSchoolLimit(int schoolId)
        {
            return _context.SchoolLimits.Where(x => x.PropertyID == schoolId && (x.EndDate == null || x.EndDate >= DateTime.Now)).OrderByDescending(x => x.StartDate).FirstOrDefault();
        }

        public List<SupportDocument> GetSupportDocuments(int documentClassId, int masterRecordID)
        {

            return _context.SupportDocument
                .Where(x => x.SupportDocumentClassID == documentClassId
                        && x.MasterRecordID == masterRecordID && (x.MarkAsDeleted == null || x.MarkAsDeleted == false))
                .ToList();


        }
        public bool HasContractorSignedOffCurrentMonth(int agencyId)
        {
            var currentMonthStatus = _context.ContractStatus
                    .Where(x => x.AgencyId == agencyId && x.ContractStatusYear == DateTime.Now.Year && x.ContractStatusMonth == DateTime.Now.Month).FirstOrDefault();
            if (currentMonthStatus != null)
            {
                return currentMonthStatus.Completed.HasValue ? (currentMonthStatus.Completed.Value == 1) : false;
            }
            return false;
        }

        public ContractStatus GetContractStatusNotSignedByFMC(int agencyId)
        {
            return _context.ContractStatus
                  .Where(x => x.AgencyId == agencyId && x.Completed == 0)
                  .OrderBy(y => y.ContractStatusYear).ThenBy(y => y.ContractStatusMonth)
                  .FirstOrDefault();
        }

        public ContractStatus GetPreviousContractStatusSignedByFMC(int agencyId)
        {
            return _context.ContractStatus
                  .Where(x => x.AgencyId == agencyId && x.Completed == 1)
                  .OrderByDescending(y => y.ContractStatusYear).ThenByDescending(y => y.ContractStatusMonth)
                  .FirstOrDefault();
        }
        public string GetSupportDocumentType(int supportDocumentTypeID)
        {
            return _context.SupportDocumentType
                        .Where(d => d.SupportDocumentTypeID == supportDocumentTypeID)
                        .FirstOrDefault()
                        .SupportDocumentTypeDesc;
        }

        public decimal GetGSTRate()
        {
            GstRate gstRate = _context.GstRates.Where(x => DateTime.Now >= x.StartDate && DateTime.Now <= x.EndDate).FirstOrDefault();
            if (gstRate != null)
            {
                return gstRate.GstPercent.HasValue ? gstRate.GstPercent.Value : (decimal).1;
            }
            else
            {
                return (decimal).1;
            }

        }

        public ServiceReportQRCode SaveQRCode(int serviceReportId, string loggedInUser)
        {
            ServiceReportQRCode qrCode = _context.ServiceReportQRCodes.Where(x => x.ServiceReportId == serviceReportId && x.Active == true).FirstOrDefault();
            if (qrCode == null)
            {
                qrCode = new ServiceReportQRCode()
                {
                    Active = true,
                    Code = Guid.NewGuid(),
                    CreatedBy = loggedInUser,
                    CreatedDate = DateTime.Now,
                    ServiceReportId = serviceReportId
                };
                _context.ServiceReportQRCodes.Add(qrCode);
                _context.SaveChanges();
            }

            return qrCode;

        }

        public ServiceReportQRCode GetQRCode(JobDetails job)
        {
            ServiceReportQRCode qrCode = _context.ServiceReportQRCodes.Where(x => x.ServiceReportId == job.ServiceReportId.Value && x.Active == true).FirstOrDefault();

            return qrCode;

        }
        public int GetNextSequenceId(int agencyId)
        {
            var connection = _context.Database.Connection; // EF6 way
            bool shouldClose = false;

            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
                shouldClose = true;
            }

            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = @"
            DECLARE @NextId INT;
            UPDATE AgencySequenceIdCounters
            SET @NextId = NextSequenceVal = NextSequenceVal + 1
            WHERE AgencyId = @AgencyId and SequenceType = 'NextWOId';;
            SELECT @NextId;";

                var param = cmd.CreateParameter();
                param.ParameterName = "@AgencyId";
                param.Value = agencyId;
                cmd.Parameters.Add(param);

                // attach current EF transaction
                if (_context.Database.CurrentTransaction != null)
                    cmd.Transaction = _context.Database.CurrentTransaction.UnderlyingTransaction;

                int nextId = (int)cmd.ExecuteScalar();

                if (shouldClose)
                    connection.Close();

                return nextId;
            }
        }
        public int? ConvertCallOutToWorkOrder(CallOutAddEditViewModel model, int subContractorId, string loggedInUser, string comments)
        {
            int seqId = 0;
            int numberOfWO = _context.ServiceReports.Where(x => x.CallLogFITSId == model.CallLogFITSID && x.ServiceReportStatusId != 9).Count();
            SchoolLimit schoolLimit = this.GetSchoolLimit(model.PropertyID);
            decimal? schoolLimitValue = schoolLimit != null ? schoolLimit.SchoolLimitAmount : null;
            using (var dbContextTransaction = _context.Database.BeginTransaction())
            {
                try
                {
                    CallLogFITS savedCallLog = _context.CallLogFITS.Where(x => x.CallLogFITSID == model.CallLogFITSID).FirstOrDefault();


                    //If the call log is registered status and no other WO present, then Change it to Work In Progress
                    if (numberOfWO == 0 && (savedCallLog.CallLogStatusID == 1 || savedCallLog.CallLogStatusID == 12))
                    {
                        savedCallLog.CallLogStatusID = 6; //Update Call log to Work in Progress
                    }
                    ServiceReport tobeSaved = new ServiceReport()
                    {
                        AgencyId = model.AgencyID,
                        CallLogFinanceUrgencyId = model.CallLogFinanceUrgencyID,
                        PropertyId = model.PropertyID,
                        CallLogFITSId = model.CallLogFITSID,
                        WorkCategoryId = (model.ProposalId > 0 || model.CallLogTypeID == 4) ? 100 : //Default to Proposal Category when it is coming from proposal or Callout set as School Funded
                        //When new Callout set to IsInsuranceEvent, all subsequent WO WorkCategory will be 3 = Insurance
                        (savedCallLog.IsInsuranceEvent == true && (savedCallLog.IsInsuranceEventByWO == null || savedCallLog.IsInsuranceEventByWO == false) ? 3 : 1),
                        ServiceReportStatusId = 1,
                        ServiceReportDesc = model.CallLogDesc,
                        Comments = comments,
                        FMCCommentDate = !string.IsNullOrEmpty(comments) ? DateTime.Now : (DateTime?)null,
                        RoomNumber = model.RoomNumber,
                        ElementId = model.ElementId,
                        WorkTypeId = model.WorkTypeId,
                        SubContractorId = subContractorId,
                        SubContractorAssignedDate = DateTime.Now,
                        RecordedBy = loggedInUser,
                        RecordedDate = DateTime.Now,
                        LastRecordedDate = DateTime.Now,
                        LastRecordedBy = loggedInUser,
                        IsInsuranceEvent = savedCallLog.IsInsuranceEvent == true && (savedCallLog.IsInsuranceEventByWO == null || savedCallLog.IsInsuranceEventByWO == false),
                        IsSchoolContribution = false,
                        ServiceReportACID = 2, //1C when WO is created
                        SchoolLimit = schoolLimitValue
                    };

                    if (savedCallLog.WorkTypeId == null || savedCallLog.ElementId == null || savedCallLog.CallLogFinanceUrgencyID == null) // Make sure CallLog WorkTypeId, ElementId, CallLogFinanceUrgencyId are not null
                    {
                        savedCallLog.WorkTypeId = model.WorkTypeId;
                        savedCallLog.ElementId = model.ElementId;
                        savedCallLog.CallLogFinanceUrgencyID = model.CallLogFinanceUrgencyID;
                    }
                    tobeSaved.AgencySequenceId = seqId = GetNextSequenceId(model.AgencyID);
                    _context.ServiceReports.Add(tobeSaved);
                    _context.SaveChanges();

                    //Save building
                    List<CallLogBuildingFITS> callLogBuildings = _context.CallLogBuildingFITSs.Where(x => x.CallLogFITSID == model.CallLogFITSID && (x.Deleted != true)).ToList();
                    foreach (CallLogBuildingFITS c in callLogBuildings)
                    {
                        _context.ServiceReportBuildings.Add(new ServiceReportBuilding()
                        {
                            AgencyID = model.AgencyID,
                            BuildingID = c.BuildingID,
                            PropertyID = c.PropertyID.Value,
                            ServiceReportID = tobeSaved.ServiceReportId,
                            Deleted = false,
                            LastUpdated = DateTime.Now
                        });

                    }
                    _context.SaveChanges();
                    dbContextTransaction.Commit();
                }
                catch
                {
                    dbContextTransaction.Rollback();
                    throw;
                }
            }


            return seqId;



        }

        private DateTime? GetUpdatedCommentDate(string newComment, string originalComment, DateTime? existingDate)
        {
            return newComment != originalComment ? DateTime.Now : existingDate;
        }

        public bool Save(JobDetails job, string loggedInUser, List<TradieChange> tradieChanges, out int jobId, out bool calloutCategeoryChanged, out bool estimateFMCEndorseChanged, out bool estimateAMUEndorseChanged)
        {
            ServiceReport serviceReportToBeSaved = null;
            CallLogFITS existingCallLog = null;
            int? seqId = null;
            bool jobCancelled = false;
            bool jobDeferred = false;
            decimal gstRate = GetGSTRate();
            //Schedule RateID is hard coded to 1 when inserting a claim (based on FMWeb SP)
            ScheduledRate shr = _context.ScheduledRates.Where(x => x.ScheduledRateId == 1).FirstOrDefault();
            //Get EUR job type - Used in claim section
            string code = "J";
            OrderJobType jobType = _context.OrderJobTypes.Where(x => x.Code == code).FirstOrDefault();
            //Get the claim type id for "Jobs"

            ClaimType jobClaimType = _context.ClaimTypes.Where(x => x.Code == code).FirstOrDefault();
            HelperMethods helper = new HelperMethods();
            ContractStatus contractStatus = GetContractStatusNotSignedByFMC(job.AgencyId);
            ContractStatus prevContractStatus = GetPreviousContractStatusSignedByFMC(job.AgencyId);

            //1. Check whether Job is cancelled
            ServiceReportStatus cancelledSvcRptStatus = _context.ServiceReportStatuses.Where(x => x.ServiceReportStatusDesc == "Cancelled").FirstOrDefault();
            ServiceReportStatus deferredSvcRptStatus = _context.ServiceReportStatuses.Where(x => x.ServiceReportStatusDesc == "Delete").FirstOrDefault();// Previously status is Deffered
            if (cancelledSvcRptStatus != null)
            {
                if (job.DetailsTab.ServiceReportStatusId == cancelledSvcRptStatus.ServiceReportStatusID)
                {
                    jobCancelled = true;

                }
            }

            jobDeferred = ((deferredSvcRptStatus != null) &&
                            (job.DetailsTab.ServiceReportStatusId == deferredSvcRptStatus.ServiceReportStatusID));

            calloutCategeoryChanged = false;
            estimateFMCEndorseChanged = false;
            estimateAMUEndorseChanged = false;
            var reimbursementCompleteChanged = false;
            using (var dbContextTransaction = _context.Database.BeginTransaction())
            {
                try
                {
                    //first get the existing record
                    serviceReportToBeSaved = _context.ServiceReports.Where(x => x.ServiceReportId == job.ServiceReportId).FirstOrDefault();
                    var wasServiceReportStatusId = serviceReportToBeSaved.ServiceReportStatusId;
                    existingCallLog = _context.CallLogFITS.Where(x => x.CallLogFITSID == serviceReportToBeSaved.CallLogFITSId).FirstOrDefault();
                    //handle change in ReimbursementComplete
                    var wasReimbursementComplete = serviceReportToBeSaved.ReimbursementComplete;
                    if (!HelperMethods.DisableField(job.UserCapabilities, "claimSection", "ReimbursementComplete") &&
                        job.Claim != null &&
                        job.Claim.ReimbursementComplete != wasReimbursementComplete)
                    {
                        job.Claim.ReimbursementCompleteDate = DateTime.Now;
                        reimbursementCompleteChanged = true;
                        //UpdateReimbursementLineItemsStatues(job.ServiceReportId, job.Claim.ReimbursementComplete);
                    }

                    if (job.Estimate.FMCApproved != null && (job.Estimate.FMCApproved != serviceReportToBeSaved.FMCApproved || job.Estimate.FMCApprovedDate != serviceReportToBeSaved.FMCApprovedDate))
                    {
                        estimateFMCEndorseChanged = true;
                    }
                    if (job.Estimate.AMUApproved != null && job.Estimate.AMUApproved != serviceReportToBeSaved.AMUApproved)
                    {
                        estimateAMUEndorseChanged = true;
                    }

                    job.DetailsTab.SubContractorCommentDate = GetUpdatedCommentDate(
                        job.DetailsTab.SubContractorComments,
                        serviceReportToBeSaved.SubContractorComments,
                        serviceReportToBeSaved.SubContractorCommentDate);

                    job.DetailsTab.FMCCommentDate = GetUpdatedCommentDate(
                        job.DetailsTab.Comments,
                        serviceReportToBeSaved.Comments,
                        serviceReportToBeSaved.FMCCommentDate);

                    job.DetailsTab.ASOCommentDate = GetUpdatedCommentDate(
                        job.DetailsTab.AMUComments,
                        serviceReportToBeSaved.AMUComments,
                        serviceReportToBeSaved.ASOCommentDate);
                    //if (CanEditInsuranceStatusInWorkOrder(job.CallLogFITSId.Value, job.ServiceReportId.Value))
                    //{
                    //    serviceReportToBeSaved.IsInsuranceEventByWO = job.IsInsuranceEvent;
                    //}
                    //if there is any
                    if (!HelperMethods.DisableField(job.UserCapabilities, "mainSection", "IsInsuranceEvent"))
                    {
                        if (job.IsInsuranceEvent != null && (existingCallLog.IsInsuranceEvent == null || existingCallLog.IsInsuranceEvent == false || existingCallLog.IsInsuranceEventByWO == true))
                        {
                            serviceReportToBeSaved.IsInsuranceEventByWO = job.IsInsuranceEvent;
                        }
                        //check whether IsInsuranceEvent checked
                        if (job.IsInsuranceEvent == true)
                        {
                            job.WorkCategoryId = 3; //insurance
                            if (jobCancelled)
                            {
                                bool anyWOWithINS = _context.ServiceReports.Where(x => x.CallLogFITSId == serviceReportToBeSaved.CallLogFITSId && x.ServiceReportStatusId != 9 && x.IsInsuranceEvent == true).Any();
                                if (!anyWOWithINS)
                                {
                                    //if the callout was set as INS event, then AMU cannot change it at the WO level. 
                                    //if AMU Set INS event and Cancelled the job, then it should been set to true bu AMU before.
                                    if (existingCallLog.IsInsuranceEvent == true && existingCallLog.IsInsuranceEventByWO == true)
                                    {
                                        existingCallLog.IsInsuranceEventByWO = false;
                                        existingCallLog.IsInsuranceEvent = false;
                                        existingCallLog.IsInsuranceEventByWOComments += String.Format("Updated by:{0} on {1} based on WO {2}. ", loggedInUser, DateTime.Now.ToString(), serviceReportToBeSaved.ServiceReportId);

                                    }
                                }
                                else
                                {
                                    //there are aleady WOs with INS=Yes, that means call log is already insurance
                                    existingCallLog.IsInsuranceEventByWOComments += String.Format("Updated by:{0} on {1} based on WO {2}. ", loggedInUser, DateTime.Now.ToString(), serviceReportToBeSaved.ServiceReportId);

                                }
                            }
                            else
                            {
                                if (existingCallLog.CallLogTypeID != 2)
                                {
                                    var wasCallLogTypeID = existingCallLog.CallLogTypeID;
                                    //WO set as INS event and WO is not cancelled
                                    existingCallLog.IsInsuranceEvent = true;
                                    existingCallLog.IsInsuranceEventByWO = true;
                                    existingCallLog.IsInsuranceEventByWOComments += String.Format("Set by:{0} on {1} based on WO {2}. ", loggedInUser, DateTime.Now.ToString(), serviceReportToBeSaved.ServiceReportId);
                                    //set flag to notification to IRCM team
                                    existingCallLog.CallLogTypeID = 2; //change the callout to be insurance category
                                    calloutCategeoryChanged = (wasCallLogTypeID != 2);
                                }

                            }

                            if (!HelperMethods.DisableField(job.UserCapabilities, "mainSection", "InsuranceIncidentDateTime") &&
                                    job.IncidentDate.HasValue && job.IncidentTime != null)
                            {
                                var timeparts = job.IncidentTime.Split(new char[] { ':' });
                                var hh = int.Parse(timeparts[0]);
                                var mm = int.Parse(timeparts[1]);
                                job.InsuranceIncidentDateTime = job.IncidentDate.Value.AddHours(hh).AddMinutes(mm);
                                existingCallLog.IncidentDate = job.InsuranceIncidentDateTime;
                            }
                        }
                        else if (job.IsInsuranceEvent == false)
                        {
                            //job.WorkCategoryId = 1; //reactive
                            int numberOfExistingInsWOs = _context.ServiceReports.Where(x => x.CallLogFITSId == existingCallLog.CallLogFITSID && x.ServiceReportStatusId != 9 && x.IsInsuranceEvent == true && x.ServiceReportId != job.ServiceReportId).Count();
                            if (numberOfExistingInsWOs == 0)
                            {
                                if (existingCallLog.CallLogTypeID != 1)
                                {
                                    //all WOs become non insurance WOs then change callout category to reactive
                                    if (existingCallLog.IsInsuranceEvent == true || existingCallLog.IsInsuranceEventByWO == true)
                                    {
                                        var wasCallLogTypeID = existingCallLog.CallLogTypeID;
                                        existingCallLog.IsInsuranceEventByWO = false;
                                        existingCallLog.IsInsuranceEvent = false;
                                        existingCallLog.IsInsuranceEventByWOComments += String.Format("Updated by:{0} on {1} based on WO {2}. ", loggedInUser, DateTime.Now.ToString(), serviceReportToBeSaved.ServiceReportId);
                                        //set flag to notification to IRCM team
                                        existingCallLog.CallLogTypeID = 1; //change the callout to be reactive category
                                        existingCallLog.IncidentDate = null; // reset incident date
                                        calloutCategeoryChanged = (wasCallLogTypeID != 1);
                                    }
                                }
                            }
                        }
                    }

                    //if (!HelperMethods.DisableField(job.UserCapabilities, "mainSection", "IsSchoolContribution"))
                    //{
                    //If the School contribution is editable
                    if (!HelperMethods.DisableField(job.UserCapabilities, "mainSection", "IsSchoolContribution"))
                    {
                        int numberOfWOSchoolContribution = NumberOfWOWithSchoolContribution(job.CallLogFITSId.Value, job.ServiceReportId.Value);

                        //if (job.IsSchoolContribution == true) {
                        //there is already a WO with School Contribution and it is not this WO, then set it to false
                        if (numberOfWOSchoolContribution == 1 && (serviceReportToBeSaved.IsSchoolContribution == null || serviceReportToBeSaved.IsSchoolContribution == false))
                        {
                            job.IsSchoolContribution = false;
                        }

                        //there is only one WO with School Contribution and it is this WO and User set it to false
                        if (numberOfWOSchoolContribution == 1 && serviceReportToBeSaved.IsSchoolContribution == true && (job.IsSchoolContribution == false || job.IsSchoolContribution == null))
                        {
                            existingCallLog.HasSchoolContributionWorkOrderComments += String.Format("Updated by:{0} on {1} CHANGED on WO {2}. ", loggedInUser, DateTime.Now.ToString(), serviceReportToBeSaved.ServiceReportId);
                            existingCallLog.HasSchoolContributionWorkOrder = job.IsSchoolContribution;
                        }

                        //there is No WO with school contribuition and User set it to Tru
                        if (numberOfWOSchoolContribution == 0 && job.IsSchoolContribution == true)
                        {
                            existingCallLog.HasSchoolContributionWorkOrderComments += String.Format("Updated by:{0} on {1} based on WO {2}. ", loggedInUser, DateTime.Now.ToString(), serviceReportToBeSaved.ServiceReportId);
                            existingCallLog.HasSchoolContributionWorkOrder = job.IsSchoolContribution;
                        }


                        if (numberOfWOSchoolContribution == 1 && serviceReportToBeSaved.IsSchoolContribution == true && jobCancelled)
                        {
                            existingCallLog.HasSchoolContributionWorkOrderComments += String.Format("Updated by:{0} on {1} based on CANCELLED WO {2}. ", loggedInUser, DateTime.Now.ToString(), serviceReportToBeSaved.ServiceReportId);
                            existingCallLog.HasSchoolContributionWorkOrder = false;
                        }

                    }


                    //update from main section
                    helper.updateChangedFields(job.UserCapabilities.MainSection, job, serviceReportToBeSaved);
                    //update from details section
                    helper.updateChangedFields(job.UserCapabilities.DetailSection, job.DetailsTab, serviceReportToBeSaved);
                    if (job.Estimate != null)
                    {
                        //update from estimate section
                        //this section is not available for MET/REA
                        helper.updateChangedFields(job.UserCapabilities.EstimateSection, job.Estimate, serviceReportToBeSaved);
                    }

                    //update from records section
                    helper.updateChangedFields(job.UserCapabilities.RecordSection, job, serviceReportToBeSaved);
                    //update from costing section
                    helper.updateChangedFields(job.UserCapabilities.CostingSection, job, serviceReportToBeSaved);

                    helper.updateChangedFields(job.UserCapabilities.ClaimSection, job.Claim, serviceReportToBeSaved);

                    string mode = getServiceReportMode(serviceReportToBeSaved);

                    serviceReportToBeSaved.ServiceReportAC = _context.ServiceReportAC.Where(x => x.Code == mode).FirstOrDefault();
                    serviceReportToBeSaved.LastRecordedDate = DateTime.Now;
                    serviceReportToBeSaved.LastRecordedBy = loggedInUser;

                    jobId = serviceReportToBeSaved.AgencySequenceId.Value;

                    //update ServiceReport Insurance Status
                    if (!HelperMethods.DisableField(job.UserCapabilities, "mainSection", "IsInsuranceEvent"))
                    {
                        //at the Job level insurance status is set, while it is not set at the CallLog level.
                        if (job.IsInsuranceEvent != null && (existingCallLog.IsInsuranceEvent == null || existingCallLog.IsInsuranceEvent == false))
                        {
                            serviceReportToBeSaved.IsInsuranceEventByWO = job.IsInsuranceEvent;
                        }
                    }

                    // _context.SaveChanges();

                    decimal? sumOfDeletedWorkItems = 0;
                    //6. Save new workitems for EUR jobs
                    if (!HelperMethods.DisableField(job.UserCapabilities, "costingSection", "addWorkItem") || job.WorkItems.Sum(item=>item.Amount)==0)
                    {
                        foreach (JobWorkItem wi in job.WorkItems)
                        {
                            int? lineNumber = _context.ServiceReportLineItems.Where(x => x.AgencyID == job.AgencyId && x.ServiceReportID == job.ServiceReportId && x.Deleted != true).Max(x => (int?)x.LineNumber);
                            lineNumber = lineNumber.HasValue ? lineNumber + 1 : 1;
                            ServiceReportLineItem srl = _mapper.Map<JobWorkItem, ServiceReportLineItem>(wi);
                            srl.ServiceReportID = job.ServiceReportId;
                            srl.LineNumber = (short?)lineNumber;
                            srl.AgencyID = job.AgencyId;
                            srl.WorkCategoryID = job.WorkCategoryId;
                            srl.ReportedBy = loggedInUser;
                            srl.ReportedDate = DateTime.Now;
                            srl.HasPWAApproved = false;
                            srl.PWARejected = false;
                            _context.ServiceReportLineItems.Add(srl);

                        }

                        if (!string.IsNullOrEmpty(job.DeletedWorkItems))
                        {
                            string[] ids = job.DeletedWorkItems.Split(',');
                            foreach (var id in ids)
                            {
                                int lineItemId = int.Parse(id);
                                ServiceReportLineItem lineItem = _context.ServiceReportLineItems.Where(x => x.ServiceReportLineItemID == lineItemId).FirstOrDefault();

                                if (lineItem != null)
                                {
                                    lineItem.Deleted = true;
                                    lineItem.ReportedDate = DateTime.Now;
                                    sumOfDeletedWorkItems = sumOfDeletedWorkItems + lineItem.Amount;

                                    //_context.SaveChanges();
                                }
                            }

                        }
                        _context.SaveChanges();

                    }


                    //When the FMC makes the claim, Month is still not closed.
                    //So use the ContractStatus, which is current month. After FMC closed the month
                    //use the Prev contract status.
                    if (!HelperMethods.DisableField(job.UserCapabilities, "claimSection", "ClaimFinalized"))
                    {
                        //FMC can tick on delete check box and save. So if anything is deleted then we need to take that amount out.
                        //This is used when sending claim email
                        job.Claim.TotalAvailableToClaimThisMonth = _context.ServiceReportLineItems.Where(x => x.AgencyID == job.AgencyId && x.ServiceReportID == job.ServiceReportId && x.Deleted != true).Sum(y => y.Amount); //job.Claim.TotalAvailableToClaimThisMonth - sumOfDeletedWorkItems;

                        if (job.IsInsuranceEvent.HasValue && job.IsInsuranceEvent.Value == true)
                        {
                            code = "I";
                            jobClaimType = _context.ClaimTypes.Where(x => x.Code == code).FirstOrDefault();
                        }
                        if (job.WorkCategoryId == 100)
                        {
                            code = "H";
                            jobClaimType = _context.ClaimTypes.Where(x => x.Code == code).FirstOrDefault();
                        }

                        if (job.Claim.TotalAvailableToClaimThisMonth > 0 && job.Claim.ClaimFinalized)
                        {

                            int? orderId = job.Estimate.OrderId ?? null;
                            int claimMonth = DateTime.Now.Month; //contractStatus != null ? contractStatus.ContractStatusMonth : 0;
                            int claimYear = DateTime.Now.Year; //contractStatus != null ? contractStatus.ContractStatusYear : 0;                          

                            var existingClaim = _context.Claims.Where(x => x.ReferringRecordId == job.ServiceReportId.Value && x.AgencyId == job.AgencyId).FirstOrDefault();
                            if (existingClaim == null)
                            {
                                //10. Add claims for both over-limit / under-limit jobs
                                _context.Claims.Add(new Claim()
                                {
                                    AgencyId = job.AgencyId,
                                    ClaimTypeId = jobClaimType != null ? jobClaimType.ClaimTypeId : 0,
                                    PropertyId = job.PropertyId,
                                    ClaimMonth = (short)claimMonth,
                                    ClaimYear = (short)claimYear,
                                    ReferringRecordId = job.ServiceReportId.HasValue ? job.ServiceReportId.Value : 0,
                                    WorkCategoryId = job.WorkCategoryId,
                                    WorkSubCategoryId = shr != null ? shr.WorkSubCategoryId : null,
                                    OrderID = orderId,
                                    ScheduledRateId = 1,
                                    Quantity = 1,
                                    ClaimedAmount = job.Claim.TotalAvailableToClaimThisMonth.Value,
                                    ClaimedGst = job.Claim.TotalAvailableToClaimThisMonth * gstRate,
                                    CostAdjPercent = null,
                                    ClaimTypeCode = jobClaimType.Code,
                                    RecordedBy = loggedInUser,
                                    RecordedDate = DateTime.Now,
                                    DemountableTypeId = 0,
                                    SubContractorId = serviceReportToBeSaved.SubContractorId,
                                    ServiceReportId = job.ServiceReportId

                                });
                                // _context.SaveChanges();
                            }
                            else
                            {
                                //if updating the existing claim
                                existingClaim.ClaimTypeCode = code;
                                existingClaim.ClaimTypeId = jobClaimType != null ? jobClaimType.ClaimTypeId : 6;
                                existingClaim.ClaimedAmount = job.Claim.TotalAvailableToClaimThisMonth.Value;
                                existingClaim.ClaimedGst = job.Claim.TotalAvailableToClaimThisMonth * gstRate;
                                existingClaim.LastRecordedBy = loggedInUser;
                                existingClaim.LastRecordedDate = DateTime.Now;
                                // _context.SaveChanges();
                            }

                        }
                    }

                    // Add Remove Tradies
                    foreach (var tradie in tradieChanges)
                    {
                        if (tradie.Action == TradieChangeAction.Add)
                        {
                            var newTradie = new ServiceReportTradie() {
                                ServiceReportId = job.ServiceReportId,
                                TradieId = tradie.TradieId,
                                CreatedBy = loggedInUser,
                                CreatedDate = DateTime.Now,
                                LastUpdatedBy = loggedInUser,
                                LastUpdatedDate = DateTime.Now,
                                PropertyId = job.PropertyId
                            };
                            _context.ServiceReportTradie.Add(newTradie);
                        }
                        if (tradie.Action == TradieChangeAction.Remove)
                        {
                            var deleteTradie = _context.ServiceReportTradie.FirstOrDefault(t => t.TradieId == tradie.TradieId && t.ServiceReportId == job.ServiceReportId);
                            _context.ServiceReportTradie.Remove(deleteTradie);
                        }    
                    }

                    _context.SaveChanges();

                    if ((wasServiceReportStatusId != cancelledSvcRptStatus.ServiceReportStatusID) &&
                        (jobCancelled || jobDeferred))
                    {
                        CheckIfCalloutReadyForReimburstment(serviceReportToBeSaved);
                    }

                    if (reimbursementCompleteChanged)
                    {
                        UpdateReimbursementLineItemsStatues(job.ServiceReportId, job.Claim.ReimbursementComplete);
                        UpdateReimbursementCalloutStatues(serviceReportToBeSaved, job.Claim.ReimbursementComplete, loggedInUser);
                        _context.SaveChanges();
                    }
                    dbContextTransaction.Commit();
                    return true;
                }
                catch
                {
                    dbContextTransaction.Rollback();
                    throw;
                }
            }
        }

        private void CheckIfCalloutReadyForReimburstment(ServiceReport serviceReportToBeSaved)
        {
            var callout = serviceReportToBeSaved.CallLogFITS;

            if (callout.IsReimbursementComplete.HasValue && callout.IsReimbursementComplete.Value) return;

            var wasReadyForReimbursement = callout.IsReadyForReimbursement;
            if (wasReadyForReimbursement.HasValue && wasReadyForReimbursement.Value) return;

            var isReadyForReimbursement = IsReadyForReimbursement(callout.CallLogFITSID, callout.AgencyID.Value);
            if (isReadyForReimbursement.HasValue &&
                isReadyForReimbursement.Value)
            {
                callout.IsReadyForReimbursement = isReadyForReimbursement;
                //Update all reimbursement lines for the callout to be ready ror reimbursement
                var reimbursementLineItems = _context.ReimbursementLineItems.Where(x => x.ServiceReport.CallLogFITSId == callout.CallLogFITSID && x.Deleted != true).ToList();
                foreach (var reimbursementLineItem in reimbursementLineItems)
                {
                    reimbursementLineItem.IRCMClaimStatusID = 1; //Ready for imbursement
                }
                //Sent notification to MFU for callout ready for reimburstment
                _context.SaveChanges();
            }
        }

        private void UpdateReimbursementCalloutStatues(ServiceReport serviceReportToBeSaved, bool reimbursementComplete, string loggedInUser)
        {
            var callout = serviceReportToBeSaved.CallLogFITS;
            var wos = _context.ServiceReports.Where(sr => sr.CallLogFITSId == callout.CallLogFITSID);
            callout.IsReimbursementComplete = (wos.Count() > 0) && !wos.Any(sr => sr.ReimbursementComplete != true);
            if (reimbursementComplete)
            {
                if (callout.IsReimbursementComplete == true)
                {
                    callout.IsReadyForReimbursement = null;
                    callout.CallLogStatusID = 4; //Complete
                    callout.Complete = true; //Complete
                    //callout.ProcessedBy = loggedInUser;
                    //callout.ProcessedDate = DateTime.Now;
                }
            }
            else
            {
                callout.IsReadyForReimbursement = IsReadyForReimbursement(callout.CallLogFITSID, callout.AgencyID.Value);
                callout.CallLogStatusID = 6; //wrok in progress
                callout.Complete = null; //Complete
                //callout.ProcessedBy = loggedInUser;
                //callout.ProcessedDate = DateTime.Now;
            }
        }

        private void UpdateReimbursementLineItemsStatues(int? serviceReportId, bool reimbursementComplete)
        {
            var reimbursementLineItems = GetReimbursementLineItems(serviceReportId.Value);
            foreach (var reimbursementLineItem in reimbursementLineItems)
            {
                reimbursementLineItem.IRCMClaimStatusID = (reimbursementComplete) ? 3 : 1;
            }
        }

        public Dictionary<string, string> GetWorkOrderDetailsForEmail(int workOrderId)
        {
            ServiceReport serviceReport = _context.ServiceReports.Where(x => x.ServiceReportId == workOrderId).FirstOrDefault();
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("WODesc", serviceReport.ServiceReportDesc);
            data.Add("FMCComments", serviceReport.Comments);
            data.Add("ContractorComments", serviceReport.SubContractorComments);
            data.Add("CallTypeDesc", serviceReport.WorkCategory != null ? serviceReport.WorkCategory.Description : "");
            return data;
        }
        public int NumberOfWOWithSchoolContribution(int callLogFitsId, int ServiceReportId)
        {
            //Can edit only 
            var woWithSchoolContribution = _context.ServiceReports.Where(x => x.CallLogFITSId == callLogFitsId && x.IsSchoolContribution == true && x.ServiceReportStatusId != 9).ToList();
            if (woWithSchoolContribution != null)
            {
                return woWithSchoolContribution.Count();

            }

            return 0;
        }

        public bool HasOnlyOneWOWithSchoolContribution(int callLogFitsId, int ServiceReportId)
        {
            //Can edit only 
            var woWithSchoolContribution = _context.ServiceReports.Where(x => x.CallLogFITSId == callLogFitsId && x.IsSchoolContribution == true && x.ServiceReportStatusId != 9).ToList();
            if (woWithSchoolContribution != null && woWithSchoolContribution.Count() > 0)
            {
                if (woWithSchoolContribution.Count() == 1 && woWithSchoolContribution.Where(x => x.ServiceReportId == ServiceReportId).Count() == 1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }


        public bool CanEditInsuranceStatusInWorkOrder(int callLogFitsId, int ServiceReportId)
        {
            //Can edit only 
            var woWithInsuranceEvent = _context.ServiceReports.Where(x => x.CallLogFITSId == callLogFitsId && x.IsInsuranceEvent == true && x.ServiceReportStatusId != 9).ToList();
            if (woWithInsuranceEvent != null && woWithInsuranceEvent.Count() > 0)
            {
                if (woWithInsuranceEvent.Count() == 1 && woWithInsuranceEvent.Where(x => x.ServiceReportId == ServiceReportId).Count() == 1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }
        private string GetFinancialYear(DateTime dateTime)
        {
            if (dateTime.Month >= 7)
            {
                return (dateTime.Year) + "/" + (dateTime.Year + 1);
            }
            else
            {
                return (dateTime.Year - 1) + "/" + (dateTime.Year);

            }

        }
        private void insertUpdateCommerceCVFees(JobDetails job, string loggedInUser,
            decimal? feeAmount, decimal? feeOther, int? orderID, short claimYear, short claimMonth)
        {
            //insert into CommerceCVFee table
            if (job.Estimate != null && job.Estimate.LimitStatus == 2 && !HelperMethods.DisableField(job.UserCapabilities, "estimateSection", "DETAccepted"))
            {
                //if exists upate the record
                decimal gstRate = GetGSTRate();
                string feeTypeCode = job.WorkCategoryId == 1 ? "J" : (job.WorkCategoryId == 1 ? "I" : (job.WorkCategoryId == 13 ? "B" : null)); // No Fee type code for MET
                CommerceCVFee commFee = _context.CommerceCVFees.Where(x => x.AgencyId == job.AgencyId && x.ReferringRecordId == job.ServiceReportId && x.FeeTypeCode == feeTypeCode).FirstOrDefault();
                if (commFee != null)
                {
                    commFee.FeeAmount = feeAmount;
                    commFee.FeeOther = feeOther;
                    commFee.FeeGST = feeAmount * gstRate;
                    commFee.FeeOtherGST = feeOther * gstRate;
                    commFee.OrderId = orderID;
                    commFee.LastRecordedBy = loggedInUser;

                    commFee.LastRecordedDate = DateTime.Now;
                    _context.SaveChanges();

                }
                else
                {
                    _context.CommerceCVFees.Add(new AssetCare.Model.Models.CommerceCVFee()
                    {
                        ReferringRecordId = job.ServiceReportId,
                        FeeAmount = feeAmount,
                        FeeOther = feeOther,
                        FeeGST = feeAmount * gstRate,
                        FeeOtherGST = feeOther * gstRate,
                        OrderId = orderID,
                        FeeTypeCode = feeTypeCode,
                        DateEntered = DateTime.Now,
                        DemountableTypeCode = "0",
                        RecordedBy = loggedInUser,
                        RecordedDate = DateTime.Now,
                        AgencyId = job.AgencyId,
                        ClaimYear = claimYear,
                        ClaimMonth = claimMonth

                    });
                    _context.SaveChanges();
                }
            }
        }
        public List<Claim> GetJobClaimsHistory(int serviceReportId, int agencyId, string code)
        {

            return _context.Claims.Where(x => x.ServiceReportId == serviceReportId && x.AgencyId == agencyId)
                .OrderByDescending(y => y.ClaimYear).ThenByDescending(y => y.ClaimMonth).ToList();
        }

        public List<ReimbursementLineItem> GetReimbursementLineItems(int serviceReportId)
        {

            return _context.ReimbursementLineItems.Where(x => x.ServiceReportId == serviceReportId && x.Deleted != true).ToList();
        }
        public IEnumerable<CustomDropdownItem> GetPurchaseOrder(int agencyId, int workCategoryID)
        {
            string code = workCategoryID == 1 ? "J" : "B";
            OrderJobType jobType = _context.OrderJobTypes.Where(x => x.Code == code).FirstOrDefault();
            if (jobType != null)
            {
                int jobTypeId = jobType.OrderJobTypeId;
                return _context.Orders.Where(x => x.OrderJobTypeId == jobTypeId && x.FinancialYear.ShowDETOrders == true &&
                        x.AgencyId == agencyId && (x.Completed == false || x.Completed == null)).Select(
                     y => new CustomDropdownItem()
                     {
                         Text = y.Description,
                         Value = y.OrderId.ToString()
                     });
            }

            return new List<CustomDropdownItem>();

        }
        public Model.Models.Order GetOrderDetails(int orderId)
        {
            return _context.Orders.Where(x => x.OrderId == orderId).FirstOrDefault();
        }

        public Claim GetCurrentClaim(int serviceReporId)
        {
            return new Claim();
        }

        public List<SubContractor> GetSubContractorsByAgency(int agencyId)
        {
            List<SubContractor> contractors = (
                                               from s in _context.SubContractor
                                               join sa in _context.SubContractorAgency on s.ID equals sa.SubContractorId
                                               where sa.AgencyId == agencyId && s.Active
                                               select s).ToList();
            return contractors;
        }

        public List<SubContractor> GetLocalContractorsByAgency(int agencyId)
        {
            List<SubContractor> contractors = (from sc in _context.SubContractor
                                               join sa in _context.SubContractorAgency on sc.ID equals sa.SubContractorId
                                               where sa.AgencyId == agencyId && sc.Active
                                               select sc).ToList();
            return contractors;
        }

        //public Claim GetCurrentClaim(int serviceReporId)
        //{
        //    return new Claim();
        //}
        private void updatePWAApprovalStatus(string idString, bool status, ContractStatus contractStatus, string loggedInUser)
        {
            if (!string.IsNullOrEmpty(idString))
            {
                string[] ids = idString.Split(',');
                foreach (var id in ids)
                {
                    int lineItemId = int.Parse(id);
                    ServiceReportLineItem lineItem = _context.ServiceReportLineItems.Where(x => x.ServiceReportLineItemID == lineItemId).FirstOrDefault();
                    if (lineItem != null)
                    {

                        lineItem.APApprovedClaimMonth = contractStatus.ContractStatusMonth;
                        lineItem.APApprovedClaimYear = contractStatus.ContractStatusYear;
                        lineItem.HasPWAApproved = true;
                        if (status)
                        {
                            lineItem.PWAApprovedBy = loggedInUser;
                            lineItem.PWAApprovedDate = DateTime.Now;
                            lineItem.PWAApproved = status;
                            lineItem.PWARejected = false;
                        }
                        else
                        {
                            //lineItem.PWAApprovedBy = null;
                            //lineItem.PWAApprovedDate = null;
                            lineItem.PWARejectedBy = loggedInUser;
                            lineItem.PWARejected = true;
                            lineItem.PWARejectedDate = DateTime.Now;
                            lineItem.PWAApproved = status;
                        }

                        _context.SaveChanges();
                    }
                }

                //if there is non approved claim, set it to 0.00 and set status to reject
            }
        }

        private string getServiceReportMode(ServiceReport d)
        {
            string mode = "X";
            //If WO is cancelled or FMC Reject or AMU Reject
            //If Cancelled then status is X   
            if (d.ServiceReportStatusId == 9 || d.ClaimApproved == true)
            {
                mode = "X";
                return mode;
            }

            if (d.ClaimReviewedByFMC == true)
            {
                mode = "7C";
            }
            else if (d.ClaimFinalized == true)
            {
                mode = "6C";
            }
            else if ((d.ClaimFinalized != true) && (d.AMUApproved == true))
            {
                //if contractor finalised (in the costing tab) and has not claimed yet OR it is marked as Reclaimed
                mode = "5C";
            }
            else if ((d.AMUApproved == null && d.FMCApproved == true) || (d.FMCApproved == true && d.AMUApproved == false && d.EstimateFinalised == true && d.EstimatedAmount > 5000))
            {
                //FMC approved, AMU to complete the workflow
                mode = "4A";
            }
            else if (d.FMCApproved != true && d.EstimateFinalised == true && d.EstimatedAmount > 5000)
            {
                //when FMC rejected, FMC Approved = false, at that time LC can approve
                //Over 5000 and FMC to approve
                mode = "3D";
            }
            else if (d.FMCApproved != true && d.EstimateFinalised == true && d.EstimatedAmount <= 5000)
            {
                //Less than 5000, No need to approve 
                mode = "3A";
            }
            else if ((d.EstimateFinalised != true && d.SubContractorAccepted == true))
            {
                //If Estimate is not yet finalised, then LC has to work on it
                //If Estimate finalised and FMC rejected, then LC has to work on it
                mode = "2A";
            }



            return mode;

        }

        public List<int> GetJobBuildingIds(int agencyId, int serviceReportId)
        {
            return _context.ServiceReportBuildings
                                         .Where(x => x.ServiceReportID == serviceReportId && x.Deleted != true)
                                         .Select(d => d.BuildingID).ToList();
        }

        public ContractStatus GetLatestFMCSingedOffContractStatus(int agencyId)
        {

            return _context.ContractStatus
                           .Where(x => x.AgencyId == agencyId && x.ContractorConfirmed == true && (x.CommerceConfirmed == null || x.CommerceConfirmed == false))
                           .OrderByDescending(y => y.ContractStatusYear).ThenByDescending(y => y.ContractStatusMonth)
                           .FirstOrDefault();


        }

        public Claim GetLatestClaim(int agencyId, int? serviceReportId, short contractStatusYear, short contractStatusMonth)
        {
            return _context.Claims.Where(x => x.AgencyId == agencyId && x.ReferringRecordId == serviceReportId &&
            x.ClaimMonth == contractStatusMonth && x.ClaimYear == contractStatusYear).FirstOrDefault();
        }

        public void GetTotalApprovedForThisClaimMonth(int? serviceReportId, ContractStatus contStatus)
        {
            throw new NotImplementedException();
        }
        public string GetAllReclaimJobs(int agencyId, int claimMonth, int claimYear)
        {
            var thisMonthReclaims = from sr in _context.ServiceReports
                                    join c in _context.Claims
                                    on sr.ServiceReportId equals c.ReferringRecordId
                                    where sr.AgencyId == agencyId && sr.ContractorReClaim == true &&
                                           c.ClaimYear == claimYear && c.ClaimMonth == claimMonth &&
                                           (c.ClaimTypeCode == "J" || (c.ClaimTypeCode == "B" && sr.Version == 2))
                                    select sr.AgencySequenceId;
            var data = thisMonthReclaims.ToList();
            if (data != null && data.Count > 0)
            {
                return string.Join(",", data);
            }

            return null;
        }
        public void ResetOverallApprovalForFMCReclaim(int agencyId, int claimMonth, int claimYear)
        {
            var thisMonthReclaims = from sr in _context.ServiceReports
                                    join c in _context.Claims
                                    on sr.ServiceReportId equals c.ReferringRecordId
                                    where sr.AgencyId == agencyId && sr.ContractorReClaim == true &&
                                           c.ClaimYear == claimYear && c.ClaimMonth == claimMonth &&
                                           (c.ClaimTypeCode == "J" || (c.ClaimTypeCode == "B" && sr.Version == 2))
                                    select new
                                    {
                                        ServiceReportId = sr.ServiceReportId,

                                    };
            bool changed = false;
            foreach (var report in thisMonthReclaims)
            {
                var ss = _context.ServiceReports.Where(x => x.ServiceReportId == report.ServiceReportId).FirstOrDefault();
                ss.PWAApproved = false;
                ss.HasPWAApproved = false;
                ss.ContractorReClaimDate = DateTime.Now;
                ss.ContractorFinalise = false;

                ss.DocumentsFinalised = false;
                ss.ClaimFinalized = false;
                ss.ClaimFinalizedDate = null;

                changed = true;
            }



            if (changed)
            {
                _context.SaveChanges();
            }

        }

        public bool? HasPWASignedOffLatestClaimMonth(int agencyId, short claimMonth, short claimYear)
        {
            bool commerceSignedOff = false;
            var latestContractStatus = _context.ContractStatus
                               .Where(x => x.AgencyId == agencyId && x.ContractStatusMonth == claimMonth && x.ContractStatusYear == claimYear)
                               .OrderByDescending(y => y.ContractStatusYear).ThenByDescending(y => y.ContractStatusMonth)
                               .FirstOrDefault();
            if (latestContractStatus != null)
            {
                commerceSignedOff = latestContractStatus.CommerceConfirmed ?? false;
            }

            return commerceSignedOff;
        }

        public bool? HasContractorSignedOffLatestClaimMonth(int agencyId, short claimMonth, short claimYear)
        {
            bool contractorSignedOff = false;
            var latestContractStatus = _context.ContractStatus
                               .Where(x => x.AgencyId == agencyId && x.ContractStatusMonth == claimMonth && x.ContractStatusYear == claimYear)
                               .OrderByDescending(y => y.ContractStatusYear).ThenByDescending(y => y.ContractStatusMonth)
                               .FirstOrDefault();
            if (latestContractStatus != null)
            {
                contractorSignedOff = latestContractStatus.ContractorConfirmed ?? false;
            }

            return contractorSignedOff;
        }

        public void ResetFMCReclaimForSubmittedClaims(int agencyId, int contractStatusMonth, int contractStatusYear)
        {
            //get all the Jobs with ContractorReclaim = true for the current claim month
            var thisMonthReclaims = from sr in _context.ServiceReports
                                    join c in _context.Claims
                                    on sr.ServiceReportId equals c.ReferringRecordId
                                    where sr.AgencyId == agencyId && sr.ContractorReClaim == true &&
                                           c.ClaimYear == contractStatusYear && c.ClaimMonth == contractStatusMonth &&
                                            (c.ClaimTypeCode == "J" || (c.ClaimTypeCode == "B" && sr.Version == 2))
                                    select new
                                    {
                                        ServiceReportId = sr.ServiceReportId,

                                    };

            bool changed = false;
            foreach (var report in thisMonthReclaims)
            {
                var ss = _context.ServiceReports.Where(x => x.ServiceReportId == report.ServiceReportId).FirstOrDefault();
                ss.ContractorReClaim = false;
                changed = true;
            }

            if (changed)
            {
                _context.SaveChanges();
            }
        }

        public void SetJobApprovedDate(int agencyId, int contractStatusMonth, int contractStatusYear)
        {
            var thisMonthReclaims = from sr in _context.ServiceReports
                                    join c in _context.Claims
                                    on sr.ServiceReportId equals c.ReferringRecordId
                                    where sr.AgencyId == agencyId &&
                                           c.ClaimYear == contractStatusYear && c.ClaimMonth == contractStatusMonth && c.ClaimTypeCode == "J"
                                           && sr.PWAApproved == true && c.Approved == true
                                    select new
                                    {
                                        ServiceReportId = sr.ServiceReportId,

                                    };

            bool changed = false;
            foreach (var report in thisMonthReclaims)
            {
                var ss = _context.ServiceReports.Where(x => x.ServiceReportId == report.ServiceReportId).FirstOrDefault();
                ss.PWAApprovedDate = DateTime.Now;
                changed = true;
            }

            if (changed)
            {
                _context.SaveChanges();
            }
        }

        public ContractStatus GetLatestContractStatusFromClaim(int agencyId, int serviceReportId, int v)
        {
            var data = (from x in _context.Claims
                        where x.ReferringRecordId == serviceReportId && x.ClaimTypeCode == "J" && x.AgencyId == agencyId
                        select new { Year = x.ClaimYear, Month = x.ClaimMonth }).Distinct()
                     .OrderByDescending(y => y.Year).ThenByDescending(y => y.Month)
                     .ToList();
            if (data != null && v < data.Count())
            {
                return new ContractStatus() { ContractStatusMonth = data[v].Month, ContractStatusYear = data[v].Year };
            }

            return new ContractStatus();


        }


        public bool CanAOApproveInMayJune(int orderId)
        {
            Model.Models.Order orderDetail = _context.Orders.Where(x => x.OrderId == orderId).FirstOrDefault();
            if (orderDetail != null)
            {
                string financialYearName = orderDetail.FinancialYear.FinancialYearName;
                bool parsed = false;
                int financialYear = 0;
                parsed = int.TryParse(financialYearName.Split('/').Last(), out financialYear);
                if (parsed)
                {
                    //chec whether April is submitted
                    ContractStatus contractStatusApril = _context.ContractStatus.Where(x => x.AgencyId == orderDetail.AgencyId && x.ContractStatusYear == financialYear && x.ContractStatusMonth == 4 && x.CommerceConfirmed == true).FirstOrDefault();
                    if (contractStatusApril != null)
                    {
                        ContractStatus contractStatusMay = _context.ContractStatus.Where(x => x.AgencyId == orderDetail.AgencyId && x.ContractStatusYear == financialYear && x.ContractStatusMonth == 5).FirstOrDefault();
                        ContractStatus contractStatusJune = _context.ContractStatus.Where(x => x.AgencyId == orderDetail.AgencyId && x.ContractStatusYear == financialYear && x.ContractStatusMonth == 6).FirstOrDefault();
                        //if both May and
                        if (contractStatusMay.CommerceConfirmed == true && contractStatusJune.CommerceConfirmed == true)
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }

                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return false;

        }

        public bool UnfinalizeJob(JobDetails model, UserViewModel currentUser)
        {
            ClaimType jobClaimType = _context.ClaimTypes.Where(x => x.Code == "J").FirstOrDefault();
            ClaimType reaJobClaimType = _context.ClaimTypes.Where(x => x.Code == "B").FirstOrDefault();

            ContractStatus contractStatus = GetContractStatusNotSignedByFMC(model.AgencyId);

            using (var dbContextTransaction = _context.Database.BeginTransaction())
            {
                int claimTypeId = (jobClaimType != null ? jobClaimType.ClaimTypeId : 0);
                //if there is an unapproved claim, delete that
                Claim existingNonApprovedClaim = _context.Claims.Where(x => x.AgencyId == model.AgencyId && x.ReferringRecordId == model.ServiceReportId.Value &&
                                (x.ClaimTypeId == jobClaimType.ClaimTypeId || x.ClaimTypeId == reaJobClaimType.ClaimTypeId) && x.Approved != true && x.ClaimMonth == contractStatus.ContractStatusMonth &&
                                x.ClaimYear == contractStatus.ContractStatusYear).FirstOrDefault();
                if (existingNonApprovedClaim != null)
                {
                    _context.Claims.Remove(existingNonApprovedClaim);
                    _context.SaveChanges();
                }

                //set Claim finalised to false
                ServiceReport serviceReportToBeSaved = _context.ServiceReports.Where(x => x.ServiceReportId == model.ServiceReportId).FirstOrDefault();
                serviceReportToBeSaved.ClaimFinalized = false;
                serviceReportToBeSaved.ServiceReportACID = 8;
                serviceReportToBeSaved.ClaimFinalizedDate = null;
                serviceReportToBeSaved.LastRecordedDate = DateTime.Now;
                serviceReportToBeSaved.LastRecordedBy = currentUser.UserCode;
                _context.SaveChanges();
                dbContextTransaction.Commit();
                return true;
            }
        }

        public bool SetJobToBeReclaimed(JobDetails model, UserViewModel currentUser)
        {
            using (var dbContextTransaction = _context.Database.BeginTransaction())
            {
                ServiceReport serviceReportToBeSaved = _context.ServiceReports.Where(x => x.ServiceReportId == model.ServiceReportId).FirstOrDefault();
                serviceReportToBeSaved.ContractorReClaim = true;
                serviceReportToBeSaved.ClaimFinalized = false;
                serviceReportToBeSaved.ContractorFinalise = false;
                serviceReportToBeSaved.ServiceReportACID = 8;
                serviceReportToBeSaved.HasPWAApproved = false;
                serviceReportToBeSaved.PWAApproved = false;
                serviceReportToBeSaved.LastRecordedDate = DateTime.Now;
                serviceReportToBeSaved.LastRecordedBy = currentUser.UserCode;
                _context.SaveChanges();
                dbContextTransaction.Commit();
                return true;
            }

        }

        public bool ResetJob60Day(JobDetails model, UserViewModel currentUser)
        {
            using (var dbContextTransaction = _context.Database.BeginTransaction())
            {
                ServiceReport serviceReportToBeSaved = _context.ServiceReports.Where(x => x.ServiceReportId == model.ServiceReportId).FirstOrDefault();
                serviceReportToBeSaved.IsClaimOver60Days = false;
                serviceReportToBeSaved.LastRecordedDate = DateTime.Now;
                serviceReportToBeSaved.LastRecordedBy = currentUser.UserCode;
                _context.SaveChanges();
                dbContextTransaction.Commit();
                return true;
            }
        }

        public bool ResetJobNotReclaimedIn40Day(JobDetails model, UserViewModel currentUser)
        {
            using (var dbContextTransaction = _context.Database.BeginTransaction())
            {
                ServiceReport serviceReportToBeSaved = _context.ServiceReports.Where(x => x.ServiceReportId == model.ServiceReportId).FirstOrDefault();
                serviceReportToBeSaved.ContractorReClaimDate = DateTime.Now;
                serviceReportToBeSaved.LastRecordedDate = DateTime.Now;
                serviceReportToBeSaved.LastRecordedBy = currentUser.UserCode;
                _context.SaveChanges();
                dbContextTransaction.Commit();
                return true;
            }
        }

        // Get all Tradies under Contractor expect already assigned
        public List<TradieViewModel> GetTradies(int serviceReportId)
        {
            List<TradieViewModel> tradies = new List<TradieViewModel>();
            int? subContractorID = _context.ServiceReports.Where(x => x.ServiceReportId == serviceReportId).FirstOrDefault().SubContractorId;
            if (subContractorID.HasValue)
            {
                var assignedTradieIds = _context.ServiceReportTradie
                                    .Where(x => x.ServiceReportId == serviceReportId)
                                    .Select(x => x.TradieId)
                                    .ToList();

                var rawData = (from t in _context.Tradie 
                               join u in _context.Users on t.UserID equals u.UserID
                               where t.LocalContractorID == subContractorID.Value && !assignedTradieIds.Contains(t.ID) && u.Active == true  && t.Compliant == true
                               select new
                               {
                                   Tradie = t,
                                   User = u,
                                   ServiceTypes = t.TradieServiceTypes.Select(ts => ts.BusinessType.Description),
                                   DocumentUrl = t.Document != null ? t.Document.Url : null
                               }).ToList();   

                tradies = rawData?.Select(x => new TradieViewModel
                {
                    ID = x.Tradie.ID,
                    UserCode = x.User.UserCode,
                    FirstName = x.User.FirstName,
                    LastName = x.User.LastName,
                    Mobile = x.User.Mobile,
                    LicenseNumber = x.Tradie.LicenseNumber,
                    LicenseExpiryDate = x.Tradie.LicenseExpiryDate,
                    LicenseStartDate = x.Tradie.LicenseStartDate,
                    PoliceCheckExpiryDate = x.Tradie.PoliceCheckExpiryDate,
                    NTBELCheck = x.Tradie.NTBELCheck,
                    PoliceCheck = x.Tradie.PoliceCheck,
                    WWCCCheck = x.Tradie.WWCCCheck,
                    Compliant = x.Tradie.Compliant,
                    Active = x.Tradie.User.Active ?? false,
                    //CreatedDate = x.ST.CreatedDate,
                    //AssignedBy = x.ST.CreatedBy,
                    PictureUrl = x.DocumentUrl,
                    LocalContractorName = x.Tradie.SubContractor?.Name,
                    TradieServiceTypeOptionNames = string.Join(", ", x.ServiceTypes)
                    
                }).ToList();
            }
            return tradies;
        }
        public List<TradieViewModel> GetAssignedTradies(int serviceReportId)
        {
            var rawData = (from st in _context.ServiceReportTradie
                           join t in _context.Tradie on st.TradieId equals t.ID
                           join u in _context.Users on t.UserID equals u.UserID
                           where st.ServiceReportId == serviceReportId
                           select new
                           {
                               Tradie = t,
                               User = u,
                               ServiceTypes = t.TradieServiceTypes.Select(ts => ts.BusinessType.Description),
                               ST = st,
                               DocumentUrl = t.Document.Url
                           })
                   .ToList();   

            var tradies = rawData.Select(x => new TradieViewModel
            {
                ID = x.Tradie.ID,
                UserCode = x.User.UserCode,
                FirstName = x.User.FirstName,
                LastName = x.User.LastName,
                Mobile = x.User.Mobile,
                LicenseNumber = x.Tradie.LicenseNumber,
                LicenseExpiryDate = x.Tradie.LicenseExpiryDate,
                LicenseStartDate = x.Tradie.LicenseStartDate,
                PoliceCheckExpiryDate = x.Tradie.PoliceCheckExpiryDate,
                NTBELCheck = x.Tradie.NTBELCheck,
                WWCCCheck = x.Tradie.WWCCCheck,
                PoliceCheck = x.Tradie.PoliceCheck,
                Compliant = x.Tradie.Compliant,
                Active = x.Tradie.User.Active ?? false,
                CheckIn = x.ST.CheckIn,
                CheckOut = x.ST.CheckOut,
                CreatedDate = x.ST.CreatedDate,
                AssignedBy = x.ST.CreatedBy,
                PictureUrl = x.DocumentUrl,
                TradieServiceTypeOptionNames = string.Join(", ", x.ServiceTypes)
            })
            .ToList();

            return tradies;
        }

        public List<Tradie> GetTradiesByCallout(int callLogFITSID)
        {
            List<Tradie> tradies = new List<Tradie>();
            //int? subContractorID = _context.CallLogFITS.Where(x => x.CallLogFITSID == callLogFITSID).FirstOrDefault().SubContractorId;
            //if (subContractorID.HasValue)
            //{
            //    tradies = (from st in _context.SubContractorTradie
            //               join t in _context.Tradie on st.TradieId equals t.ID
            //               where st.SubContractorId == subContractorID.Value
            //               select t).ToList();

            //}

            return tradies;
        }

        public Tradie GetTradieDetails(int tradieId)
        {
            var tradie = _context.Tradie.Where(x => x.ID == tradieId).FirstOrDefault();
            return tradie;
        }

        public SubContractor GetSubContractorDetails(int subContractorId)
        {
            var subContractor = _context.SubContractor.Where(x => x.ID == subContractorId).FirstOrDefault();
            return subContractor;
        }

        public LCWOViewModel GetLCWOCount(int subContractorId)
        {
            // All status except Cancelled and Paid
            int ActiveWO = _context.ServiceReports.Where(x => (x.ServiceReportStatusId < 9 || x.ServiceReportStatusId == 10) && x.SubContractorId == subContractorId).Count();
            // All status except Cancelled
            int TotalWO = _context.ServiceReports.Where(x => x.ServiceReportStatusId != 9 && x.SubContractorId == subContractorId).Count();
            LCWOViewModel lcWOVM = new LCWOViewModel
            {
                ActiveWOCount = ActiveWO,
                TotalWOCount = TotalWO
            };
            return lcWOVM;
        }

        public string GetServiceReportAssignedTradies(int serviceReportId)
        {
            List<int> tradies = _context.ServiceReportTradie.Where(x => x.ServiceReportId == serviceReportId ).Select(y => y.TradieId.Value).ToList();
            string tradieIdList = string.Empty;
            if (tradies != null && tradies.Count() > 0)
            {
                tradieIdList = string.Join(",", tradies);
            }

            return tradieIdList;
        }

        public bool UpdateJobAcceptance(WorkOrderAcceptanceStatus status)
        {
            ServiceReport tobeSaved = _context.ServiceReports.Where(x => x.ServiceReportId == status.JobId && x.AgencyId == status.AgencyId).FirstOrDefault();
            if (tobeSaved != null)
            {
                tobeSaved.SubContractorAccepted = status.Status;
                tobeSaved.SubContractorRespondedDate = DateTime.Now;
                tobeSaved.LastRecordedDate = DateTime.Now;
                tobeSaved.LastRecordedBy = status.LoggedInUser;

                _context.SaveChanges();
            }

            return true;

        }

        public int CreateWOAsVariation(CreateWorkOrder model, out int serviceReportId)
        {
            ServiceReport existingServiceReport = _context.ServiceReports.Where(x => x.ServiceReportId == model.WOServiceReportId).FirstOrDefault();
            int? callLogId = existingServiceReport.CallLogFITSId;
            int agencyId = existingServiceReport.AgencyId;
            using (var dbContextTransaction = _context.Database.BeginTransaction())
            {
            retry:
                CallLogFITS savedCallLog = _context.CallLogFITS.Where(x => x.CallLogFITSID == callLogId.Value).FirstOrDefault();
                int? seqId = _context.ServiceReports.Where(x => x.AgencyId == agencyId).Max(x => (int?)x.AgencySequenceId);
                seqId = (seqId == null ? 0 : seqId.Value) + 1;

                ServiceReport tobeSaved = new ServiceReport()
                {
                    AgencyId = agencyId,
                    AgencySequenceId = seqId,
                    CallLogFinanceUrgencyId = existingServiceReport.CallLogFinanceUrgencyId,
                    PropertyId = existingServiceReport.PropertyId,
                    CallLogFITSId = callLogId,
                    WorkCategoryId = 1,
                    ServiceReportStatusId = 1,
                    ServiceReportDesc = existingServiceReport.ServiceReportDesc,
                    Comments = model.WOComments,
                    RoomNumber = existingServiceReport.RoomNumber,
                    ElementId = existingServiceReport.ElementId,
                    WorkTypeId = existingServiceReport.WorkTypeId,
                    SubContractorId = model.WOSubContractorId,
                    SubContractorAssignedDate = DateTime.Now,
                    RecordedBy = model.LoggedInUser,
                    RecordedDate = DateTime.Now,
                    LastRecordedDate = DateTime.Now,
                    LastRecordedBy = model.LoggedInUser,
                    IsInsuranceEvent = savedCallLog.IsInsuranceEvent == true && (savedCallLog.IsInsuranceEventByWO == null || savedCallLog.IsInsuranceEventByWO == false),
                    AssociatedJobID = model.WOServiceReportId.ToString()
                };

                try
                {
                    _context.ServiceReports.Add(tobeSaved);
                    _context.SaveChanges();
                }
                catch (DbUpdateException ex)
                {
                    if (ex.InnerException.InnerException.ToString().Contains("'UC_Agency_AgencySequenceId_WO'"))
                    {
                        _context.Entry(tobeSaved).State = EntityState.Detached;
                        goto retry;
                    }
                }

                //Save building
                List<ServiceReportBuilding> callLogBuildings = _context.ServiceReportBuildings.Where(x => x.ServiceReportID == model.WOServiceReportId && (x.Deleted != true)).ToList();
                foreach (ServiceReportBuilding c in callLogBuildings)
                {
                    _context.ServiceReportBuildings.Add(new ServiceReportBuilding()
                    {
                        AgencyID = agencyId,
                        BuildingID = c.BuildingID,
                        PropertyID = c.PropertyID,
                        ServiceReportID = tobeSaved.ServiceReportId,
                        Deleted = false,
                        LastUpdated = DateTime.Now
                    });
                    _context.SaveChanges();
                }

                dbContextTransaction.Commit();
                serviceReportId = tobeSaved.ServiceReportId;
                return seqId.Value;
            }

        }

        public bool ReviewClaims(WorkOrderApprovalListItem item, string loggedInUser)
        {
            var claim = _context.Claims.Where(x => x.ClaimId == item.Id).FirstOrDefault();
            claim.FMCComment = item.Comment;
            claim.FMCReviewed = item.Approved;
            claim.FMCReviewedBy = loggedInUser;
            claim.FMCReviewedDate = DateTime.Now;
            if (item.Approved == true)
            {
                claim.ServiceReport.ServiceReportACID = 8; //update ServiceReport status
                claim.ServiceReport.ClaimReviewedByFMC = item.Approved;
                claim.ServiceReport.ClaimFinalized = true;

            }
            else if (item.Approved == false)
            {
                claim.ServiceReport.ServiceReportACID = 6; //5C
                claim.ServiceReport.ClaimFinalized = false;
                claim.ServiceReport.ClaimReviewedByFMC = null;
                claim.ServiceReport.ServiceReportStatusId = 8; //Set Status to Complete
                claim.ApprovedAmount = null;
            }


            _context.SaveChanges();
            return true;
        }

        public bool ApproveClaims(WorkOrderApprovalListItem item, string loggedInUser, bool amuRegion)
        {
            var claim = _context.Claims.Where(x => x.ClaimId == item.Id).FirstOrDefault();
            decimal gstRate = GetGSTRate();
            claim.AMUComment = item.Comment;
            claim.AMUApproved = item.Approved;
            if (item.Amount > claim.ClaimedAmount)
            {
                claim.ApprovedAmount = claim.ClaimedAmount;
                claim.ApprovedGst = claim.ClaimedAmount * gstRate;
            }
            else
            {
                claim.ApprovedAmount = item.Amount;
                claim.ApprovedGst = item.Amount * gstRate;
            }

            if (item.Approved == true)
            {
                claim.AMUApprovedBy = loggedInUser;
                claim.AMUApprovedDate = DateTime.Now;
                claim.ServiceReport.ServiceReportACID = 1;
                claim.ServiceReport.ServiceReportStatusId = 11; //Paid

                claim.ServiceReport.ClaimApproved = item.Approved;

            }
            else if (item.Approved == false)
            {
                claim.ServiceReport.ServiceReportACID = 7; //6C
                claim.FMCReviewed = null;
                claim.ServiceReport.ClaimApproved = null;
                claim.ApprovedAmount = null;
                // claim.ApprovedAmount
                if (amuRegion)
                {
                    claim.ServiceReport.ServiceReportACID = 6; //5C
                    claim.ServiceReport.ClaimFinalized = false;
                    claim.ServiceReport.ServiceReportStatusId = 8; //Set Status to Complete
                    claim.ApprovedAmount = null;
                }
            }

            //update reimbursement line items
            UpdateReimbursementLineItems(_context, claim, loggedInUser, item.Approved);

            _context.SaveChanges();

            return true;
        }

        private void UpdateReimbursementLineItems(AssetCareContext context, Claim claim, string loggedInUser, bool? isApproved)
        {
            var callout = claim.ServiceReport.CallLogFITS;
            if ((callout != null) &&
                    (callout.IsInsuranceEvent.HasValue && callout.IsInsuranceEvent.Value ||
                    callout.IsInsuranceEventByWO.HasValue && callout.IsInsuranceEventByWO.Value))
            {
                if (isApproved.HasValue && isApproved.Value)
                {

                    if ((claim != null) && (claim.ServiceReport != null) &&
                        (claim.ServiceReport.IsInsuranceEvent.HasValue && claim.ServiceReport.IsInsuranceEvent.Value ||
                        claim.ServiceReport.IsInsuranceEventByWO.HasValue && claim.ServiceReport.IsInsuranceEventByWO.Value))
                        AddReimbursementLineItems(context, claim, loggedInUser);
                    var wasReadyForReimbursement = claim.ServiceReport?.CallLogFITS?.IsReadyForReimbursement;
                    var isReadyForReimbursement = IsReadyForReimbursement(claim.ServiceReport?.CallLogFITS?.CallLogFITSID, (int)claim.ServiceReport?.CallLogFITS?.AgencyID.Value);
                    if ((!wasReadyForReimbursement.HasValue ||
                          wasReadyForReimbursement.HasValue && !wasReadyForReimbursement.Value) &&
                        isReadyForReimbursement.HasValue &&
                        isReadyForReimbursement.Value)
                    {
                        claim.ServiceReport.CallLogFITS.IsReadyForReimbursement = isReadyForReimbursement;
                        //Update all reimbursement lines for the callout to be ready ror reimbursement
                        var reimbursementLineItems = context.ReimbursementLineItems.Where(x => x.ServiceReport.CallLogFITSId == claim.ServiceReport.CallLogFITS.CallLogFITSID && x.Deleted != true).ToList();
                        foreach (var reimbursementLineItem in reimbursementLineItems)
                        {
                            reimbursementLineItem.IRCMClaimStatusID = 1; //Ready for imbursement
                        }
                        //Sent notification to MFU for callout ready for reimburstment
                        context.SaveChanges();
                    }
                }
                else
                    RemoveReimbursementLineItems(context, claim, loggedInUser);
            }
        }

        private bool? IsReadyForReimbursement(int? id, int agencyId)
        {
            bool isReadyForReimbursement = false;

            CallLogFITS callout = _context.CallLogFITS.Where(x => x.CallLogFITSID == id.Value).FirstOrDefault();
            if ((callout != null) &&
                    (callout.IsInsuranceEvent.HasValue && callout.IsInsuranceEvent.Value ||
                    callout.IsInsuranceEventByWO.HasValue && callout.IsInsuranceEventByWO.Value))
            {
                var orderIds = _context.ServiceReports.Where(x => x.CallLogFITSId == id.Value && x.ServiceReportStatusId != 9 /* Cancelled */ && x.ServiceReportStatusId != 7 /* Delete previously Deferred */).Select(x => x.ServiceReportId);
                var approvedclaimsOrderIdsByCallout = _context.Claims.Where(x => x.AgencyId == agencyId &&
                                                                    x.AMUApproved.HasValue &&
                                                                    x.AMUApproved.Value &&
                                                                    orderIds.Contains(x.ServiceReportId.Value)).Select(x => x.ServiceReportId.Value);
                isReadyForReimbursement = (orderIds.Count() > 0) && !orderIds.Except(approvedclaimsOrderIdsByCallout).Any();
            }


            return isReadyForReimbursement;
        }

        private void AddReimbursementLineItems(AssetCareContext context, Claim claim, string loggedInUser)
        {
            //add reimbursement line items
            claim.ReadyForInsurancePaymentBy = loggedInUser;
            claim.ReadyForInsurancePaymentDate = DateTime.Now;
            //var orderlines = context.ServiceReportLineItems.Where(l => l.ServiceReportID == claim.ServiceReportId);
            var orderlines = GetServiceReportLineItems(claim.ServiceReportId.Value);
            var reimbursementLineItems = GetReimbursementLineItems(claim.ServiceReportId.Value);
            foreach (var orderline in orderlines)
            {
                if (reimbursementLineItems.Any(rli => rli.ServiceReportLineItemID == orderline.ServiceReportLineItemID)) continue;
                var reimbursementLineItem = new ReimbursementLineItem
                {
                    AgencyID = claim.AgencyId,
                    ClaimId = claim.ClaimId,
                    ServiceReportId = claim.ServiceReportId.Value,
                    ServiceReportLineItemID = orderline.ServiceReportLineItemID,
                    IRCMClaimStatusID = 2, // not ready for reimbursement
                };
                context.ReimbursementLineItems.Add(reimbursementLineItem);
            }
            context.SaveChanges();
        }

        private void RemoveReimbursementLineItems(AssetCareContext context, Claim claim, string loggedInUser)
        {
            var reimbursementLineItems = GetReimbursementLineItems(claim.ServiceReportId.Value).ToList();
            foreach (var reimbursementLineItem in reimbursementLineItems)
            {
                reimbursementLineItem.Deleted = true;
            }
        }
        public bool ApproveInsuranceReimbursement(InsuranceReimbursementListItem item, string loggedInUser)
        {
            var reimbursementLineItem = _context.ReimbursementLineItems.Where(x => x.ReimbursementLineItemID == item.Id).FirstOrDefault();

            if (reimbursementLineItem.IRCMClaimStatusID == null ||
                reimbursementLineItem.IRCMClaimStatusID == 2 ||
                reimbursementLineItem.IRCMClaimStatusID == 3)
                return true; //return if not ready for reimbursement

            reimbursementLineItem.ReimbursementStatusID = item.ReimbursementStatusID;
            reimbursementLineItem.ReimbursementAmount = item.Amount;
            reimbursementLineItem.Comments = item.Comment.SizeLimit(512);
            reimbursementLineItem.ReimbursementDate = DateTime.Now;
            //reimbursementLineItem.IRCMClaimStatusID = (item.ReimbursementStatusID > 0) ? 3 : reimbursementLineItem.IRCMClaimStatusID;
            //UpdateReimbursementCompleteIfAllItemsComplete(reimbursementLineItem);
            _context.SaveChanges();

            return true;
        }

        private void UpdateReimbursementCompleteIfAllItemsComplete(ReimbursementLineItem reimbursementLineItem)
        {
            if (reimbursementLineItem.IRCMClaimStatusID != 3) return;
            var reimbursementLineItems = GetReimbursementLineItems(reimbursementLineItem.ServiceReportId);
            var allItemsCompleted = !reimbursementLineItems.Any(rli => rli.IRCMClaimStatusID != 3);
            if (allItemsCompleted)
            {
                reimbursementLineItem.ServiceReport.ReimbursementComplete = allItemsCompleted;
                reimbursementLineItem.ServiceReport.ReimbursementCompleteDate = DateTime.Now;
                reimbursementLineItem.Claim.ApprovedByMFUDate = DateTime.Now;
                reimbursementLineItem.Claim.ApprovedAmountByMFU = reimbursementLineItems.Sum(rli => rli.ReimbursementAmount);
                reimbursementLineItem.Claim.ApprovedGstByMFU = reimbursementLineItem.Claim.ApprovedAmountByMFU.Value * 0.1M;
            }
        }

        public WorkOrderClaimApprovalViewModdel GetClaimApprovalData(int claimId)
        {
            WorkOrderClaimApprovalViewModdel vm = new WorkOrderClaimApprovalViewModdel();
            var claim = _context.Claims.Where(x => x.ClaimId == claimId).FirstOrDefault();
            if (claim != null)
            {
                vm.WorkOrderId = claim.ServiceReport.AgencySequenceId.HasValue ? claim.ServiceReport.AgencySequenceId.Value : 0;
                vm.AgencyId = claim.AgencyId.ToString();
                vm.ClaimedAmount = claim.ClaimedAmount;
                vm.ApprovedAmount = claim.ApprovedAmount;
                vm.FMCReviewComments = claim.FMCComment;
                vm.AMUApprovalComment = claim.AMUComment;
                vm.FMCReviewed = claim.FMCReviewed;
                vm.AMUApproved = claim.AMUApproved;
                vm.FMCReviewedDate = claim.FMCReviewedDate;
                vm.SchoolName = claim.Property.PropertyShortDesc;
                vm.AMUApprovedDate = claim.AMUApprovedDate;
                vm.WorkCategory = claim.ClaimTypeCode == "J" ? "Reactive" : (claim.ClaimTypeCode == "I" ? "Insurance" : "");
                vm.LCEmail = claim.SubContractor.Email;
                vm.ClaimedDate = claim.RecordedDate;
                vm.ASOEmail = claim.Property.ASOEmail;
                var firstLC = claim.SubContractor.SubContractorAgencies.Where(x => x.AgencyId == claim.AgencyId).FirstOrDefault();
                if (firstLC.Agency != null && firstLC.Agency.AgencyContractor != null)
                {
                    vm.FMCEmail = firstLC.Agency.AgencyContractor.Email;
                }
            }

            return vm;
        }

        public List<int> GetBulkInvoiceEmailData(int agencyId, out string data, out decimal total)
        {
            total = 0;
            data = string.Empty;
            List<int> claimIds = new List<int>();
            var HeaderList = (from sr in _context.ServiceReports
                              join c in _context.Claims on sr.ServiceReportId equals c.ServiceReportId
                              join pr in _context.Properties on sr.PropertyId equals pr.PropertyId
                              join ag in _context.Agencies on c.AgencyId equals ag.AgencyId
                              where sr.AgencyId == agencyId && (c.ClaimTypeId == 6 || c.ClaimTypeId == 14) && c.AMUApproved == true && c.FMCReviewed == true && (c.SAPProcessed == null || c.SAPProcessed == false) && c.ClaimedAmount > 0
                              select
                              new BulkInvoiceHeaderModel
                              {
                                  AgencyId = c.AgencyId,
                                  ServiceReportId = sr.ServiceReportId,
                                  AgencySequenceId = sr.AgencySequenceId,
                                  ClaimId = c.ClaimId,
                                  HeaderIdentifier = "H",
                                  DocumentDate = sr.RecordedDate,
                                  PostingDate = DateTime.Today,
                                  DocType = "KR",
                                  CompanyCode = "1010",
                                  Currency = "AUD",
                                  Reference = "R" + sr.ServiceReportId.ToString(),
                                  DocHeadrText = string.Empty,
                                  CrossCCNo = string.Empty,
                                  AgencyCode = ag.AgencyCode,
                                  SchoolId = pr.SchoolId

                              }).ToList();

            foreach (BulkInvoiceHeaderModel header in HeaderList)
            {

                var lineItem = (from sr in _context.ServiceReports
                                join c in _context.Claims on sr.ServiceReportId equals c.ServiceReportId
                                join ct in _context.ClaimTypes on c.ClaimTypeId equals ct.ClaimTypeId
                                join ag in _context.Agencies on c.AgencyId equals ag.AgencyId
                                join sc in _context.SubContractor on sr.SubContractorId equals sc.ID
                                where c.ClaimId == header.ClaimId && (sc.SAPVendorID != null)
                                select
                                new BulkInvoiceHeaderLineItemModel
                                {
                                    LineItemIdentifier = "I",
                                    PostingKey = "31",
                                    Account = sc.SAPVendorID,
                                    GLAccountCode = ct.GL,
                                    WBS = ct.WBS,
                                    FundCode = ct.FundCode,
                                    SplGL = null,
                                    TransactionType = null,
                                    Amount = (c.ClaimedGst.HasValue ? c.ClaimedGst.Value : 0) + (c.ClaimedAmount),
                                    TaxCode = null,
                                    WithholdingTaxcode = null,
                                    CostCentre = ag.CostCentre,
                                    Order = null,
                                    ProfitCentre = null,
                                    Assignment = null,
                                    LineItemText = "All GST inclusive",
                                    NewCompanyCode = null,
                                    PaymentTerms = "Z001",
                                    BaselineDate = c.AMUApprovedDate,
                                    PaymentBlock = null,
                                    TradingPartner = null,
                                }).FirstOrDefault();

                if (lineItem != null)
                {
                    data = data + string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8}\n" +
                                "{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25},{26},{27},{28},{29}\n" +
                                "{30},{31},{32},{33},{34},{35},{36},{37},{38},{39},{40},{41},{42},{43},{44}\n",
                        header.HeaderIdentifier, (header.DocumentDate.HasValue ? header.DocumentDate.Value.ToString("yyyyMMdd") : ""), (header.PostingDate.HasValue ? header.PostingDate.Value.ToString("yyyyMMdd") : ""),
                        header.DocType, header.CompanyCode, header.Currency,
                        header.Reference,
                        string.Format("{0} {1} WO:{2}", header.AgencyCode, (header.SchoolId.HasValue ? header.SchoolId.Value.ToString() : "0"), (header.AgencySequenceId.HasValue ? header.AgencySequenceId.Value.ToString() : "")),
                        header.CrossCCNo,
                        //First line item
                        lineItem.LineItemIdentifier, lineItem.PostingKey, lineItem.Account, lineItem.SplGL,
                        lineItem.TransactionType, lineItem.Amount.ToString("0.00"), lineItem.TaxCode, lineItem.WithholdingTaxcode,
                        string.Empty, string.Empty, string.Empty, lineItem.Order,
                        lineItem.ProfitCentre, lineItem.Assignment, lineItem.LineItemText, lineItem.NewCompanyCode,
                        lineItem.PaymentTerms, (lineItem.BaselineDate.HasValue ? lineItem.BaselineDate.Value.ToString("yyyyMMdd") : ""), lineItem.PaymentBlock, lineItem.TradingPartner, lineItem.TransactionType,
                        //Second Line item
                        lineItem.LineItemIdentifier, "40", lineItem.GLAccountCode, string.Empty, string.Empty, lineItem.Amount.ToString("0.00"), "P1", string.Empty, lineItem.CostCentre, lineItem.WBS, lineItem.FundCode,
                        string.Empty, string.Empty, string.Empty, lineItem.LineItemText
                        );
                    claimIds.Add(header.ClaimId);
                    total = total + lineItem.Amount;
                }

            }


            return claimIds;
        }

        public void UpdateClaimsForBulkInvoice(List<int> claimIds)
        {
            foreach (int claimId in claimIds)
            {
                var claim = _context.Claims.Where(x => x.ClaimId == claimId).FirstOrDefault();
                claim.SAPProcessed = true;
                claim.SAPProcessedDate = DateTime.Now;
                _context.SaveChanges();
            }
        }

        public WorkOrderEmailData GetWorkOrderEmailData(int serviceReportId)
        {
            ServiceReport sr = _context.ServiceReports.Where(x => x.ServiceReportId == serviceReportId).FirstOrDefault();
            if (sr == null)
            {
                return new WorkOrderEmailData();
            }

            return new WorkOrderEmailData()
            {
                AMUApproved = sr.AMUApproved,
                AMUApprovedDate = sr.AMUApprovedDate,
                EstimateDate = sr.EstimateDate,
                FMCRecommend = sr.FMCApproved,
                FMCRecommendDate = sr.FMCApprovedDate
            };
        }

        public string GetReimbursementStatusDesc(int serviceReportId)
        {
            var reimbursementLineItems = GetReimbursementLineItems(serviceReportId);
            var fullyReimbursement = reimbursementLineItems.All(rli => rli.ReimbursementStatusID == 1);
            var declinedReimbursement = reimbursementLineItems.All(rli => rli.ReimbursementStatusID == 2);
            var partialReimbursement = reimbursementLineItems.Any(rli => rli.ReimbursementStatusID == 3);
            return (fullyReimbursement ? "Fully Reimbursed" :
                    (partialReimbursement) ? "Partial Reimbursed" :
                    (declinedReimbursement) ? "Fully Declined" : "N/A");
        }

        public IList<IRCMClaimStatus> GetIRCMClaimStatuses()
        {
            return _context.IRCMClaimStatuses.ToList();
        }

        public IList<ClaimApprovalStatusFilter> GetReactiveClaimApprovalStatusFilters()
        {
            return _context.ClaimApprovalStatusFilter.Where(x => x.ReactiveYN).ToList();
        }

        public IList<ReimbursementStatus> GetReimbursementStatuses()
        {
            return _context.ReimbursementStatuses.ToList();
        }

        public IList<AssessorStatus> GetAssessorStatuses()
        {
            return _context.AssessorStatuses.ToList();
        }

        public void GenerateReimbursementItems(IEnumerable<WorkOrderCertifyListItem> certifylistItems, string loggedInUser)
        {
            foreach (var certifylistItem in certifylistItems)
            {
                var claim = _context.Claims.FirstOrDefault(cli => cli.ClaimId == certifylistItem.ClaimId);
                if (claim != null &&
                    claim.AMUApproved == true)
                {
                    //update reimbursement line items
                    UpdateReimbursementLineItems(_context, claim, loggedInUser, claim.AMUApproved);
                }
            }
            _context.SaveChanges();
        }

        public IEnumerable<WorkOrderCertifyListItem> GetWorkOrderCertifyListItems(WorkOrderCertifyFilterModel filterModel, List<int> userPropertyIds, out int filteredResultsCount, out int totalResultsCount)
        {
            bool? amuApprovalStatus = filterModel.ApprovalStatus;
            int? claimedAmount = filterModel.ClaimedAmountFlag;
            using (var dbContextTransaction = _context.Database.BeginTransaction(IsolationLevel.Snapshot))
            {
                var searchResults = _context.Claims.Where(x => /*x.AgencyId == filterModel.AgencyId &&*/
                //(filterModel.RestrictToLocalContractorId == true ? x.SubContractorId == filterModel.SubContractorId : true) &&
                //(userPropertyIds.Count > 0 ? userPropertyIds.Contains(x.PropertyId.Value) : true) &&
                (amuApprovalStatus.HasValue ? (amuApprovalStatus == true ? x.AMUApproved == true : (x.AMUApproved == amuApprovalStatus || x.AMUApproved == null)) : true) &&
                (filterModel.ApprovalFromDate.HasValue ? x.AMUApprovedDate >= filterModel.ApprovalFromDate : (filterModel.ApprovalFromDate == null)) &&
                (filterModel.ApprovalToDate.HasValue ? x.AMUApprovedDate <= filterModel.ApprovalToDate : filterModel.ApprovalToDate == null) &&
                //(filterModel.SubContractorId.HasValue ? x.SubContractorId == filterModel.SubContractorId : (filterModel.SubContractorId == null)) &&
                //(filterModel.PropertyId.HasValue ? x.PropertyId == filterModel.PropertyId : filterModel.PropertyId == null) &&
                //(claimedAmount == 1 ? x.ClaimedAmount > 5000 : (claimedAmount == 0 ? x.ClaimedAmount <= 5000 : true)) &&
                (x.ClaimTypeCode == "I" || x.ClaimTypeCode == "J")).Select(
                m => new WorkOrderCertifyListItem()
                {
                    ClaimId = m.ClaimId,
                    AMUApproved = m.AMUApproved,
                    ApprovedAmount = m.ApprovedAmount,
                    ApprovedDate =
                     m.AMUApprovedDate.HasValue ? (
                     (SqlFunctions.DatePart("d", m.AMUApprovedDate.Value).ToString().Length == 1 ? "0" + SqlFunctions.DatePart("d", m.AMUApprovedDate.Value).ToString() : SqlFunctions.DatePart("d", m.AMUApprovedDate.Value).ToString()) + "/" +
                     (SqlFunctions.DatePart("m", m.AMUApprovedDate.Value).ToString().Length == 1 ? "0" + SqlFunctions.DatePart("m", m.AMUApprovedDate.Value).ToString() : SqlFunctions.DatePart("m", m.AMUApprovedDate.Value).ToString()) + "/" +
                     (SqlFunctions.DatePart("year", m.AMUApprovedDate.Value).ToString())
                     ) : "",
                    ASOComment = m.AMUComment,
                    ClaimedAmount = m.ClaimedAmount,
                    FMCComments = m.FMCComment,
                    FMCReviewed = m.FMCReviewed,
                    JobDesc = m.ServiceReport.ServiceReportDesc,
                    Category = m.ClaimTypeCode == "I" ? "Insurance" : (m.ClaimTypeCode == "J" ? "Reactive" : ""),
                    LocalContractor = m.SubContractor.Name,
                    SchoolName = m.Property.PropertyShortDesc,
                    WorkOrderId = m.ServiceReport.AgencySequenceId,
                    ServiceReportId = m.ReferringRecordId,
                    PropertyId = m.PropertyId,
                    HasSAPVendorId = (m.SubContractor.SAPVendorID == null || m.SubContractor.SAPVendorID == "") ? false : true,
                    ClaimFinalized = m.ServiceReport.ClaimFinalized
                });
                filteredResultsCount = searchResults.Count();
                totalResultsCount = searchResults.Count();
                dbContextTransaction.Commit();
                return searchResults.ToList();
            }
        }

        //public ServiceReport GetJobDetailsBySVCIKey(Guid svciKey)
        //{
        //    return _context.ServiceReports.
        //         Where(x => x.SVCIKey == svciKey).FirstOrDefault();
        //}
    }
}