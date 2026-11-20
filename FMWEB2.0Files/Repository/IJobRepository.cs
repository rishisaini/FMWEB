using AssetCare.Model.Models;
using AssetCare.Model.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetCare.Repository.Interfaces
{
    public interface IJobRepository
    {
        CallLogFITS GetCallLogFitDetails(int agencyId, int callOutId);
        List<int> GetCallLogFitsBuildingIds(int agencyId, int callLogFitsId);
        List<int> GetJobBuildingIds(int agencyId, int serviceReportId);

        SchoolLimit GetSchoolLimit(int value);
        List<SupportDocument> GetSupportDocuments(int documentClassId, int masterRecordId);
        bool Save(JobDetails model, string loggedInUser, List<TradieChange> tradieChanges, out int jobId, out bool calloutCategeoryChanged, out bool estimateFMCEndorseChanged, out bool estimateAMUEndorseChanged);
        ServiceReport GetJobDetails(int agencyId, int jobId);
        ServiceReport GetJobDetailsByJobNo(int agencyId, int jobNo);
        //ServiceReport GetJobDetailsBySVCIKey(Guid svciKey);
        CallLogFITS GetCallLogFitDetails(int? callLogFITSId);

        int? GetProposalDetails(int? callLogFITSId);
        string GetSupportDocumentType(int supportDocumentTypeID);
        IEnumerable<JobListViewModel> FilterJobData(CallOutFilterModel callOutFilterModel, bool dataForExport, List<int> userPropertyIds, string userRole, out int filteredResultsCount, out int totalResultsCount);
        IEnumerable<JobListForHPViewModel> FilterJobDataForHP(CallOutFilterModel callOutFilterModel, bool dataForExport, List<int> userPropertyIds, int DefaultContractorID, string userRoleInAssetCare, out int filteredResultsCount, out int totalResultsCount);
        IEnumerable<JobListForHPViewModel> FilterJobDataForHPClaim(CallOutFilterModel callOutFilterModel, bool dataForExport, List<int> userPropertyIds,  int DefaultContractorID, string userRoleInAssetCare,out int filteredResultsCount, out int totalResultsCount);

        IEnumerable<JobListForHPViewModel> FilterJobApprovalDataForHP(CallOutFilterModel callOutFilterModel, bool dataForExport, IEnumerable<int> userPropertyIds, string userRoleInAssetCare, out int filteredResultsCount, out int totalResultsCount);

        IEnumerable<JobListForSchoolHPViewModel> FilterJobDataForSchoolHP(CallOutFilterModel callOutFilterModel, UserViewModel currentUser, bool dataForExport, IEnumerable<int> userPropertyIds, string userRoleInAssetCare, out int filteredResultsCount, out int totalResultsCount);

        List<ScheduleRateDropdownItem> GetScheduledRates(int agencyId, int? schduleRateCategoryId, int? subContractorId);
        decimal GetCostAdjustment(int agencyId, string costAdjType, string financialYear, DateTime costingDate);
        List<ServiceReportLineItem> GetServiceReportLineItems(int serviceReportId);
        List<Claim> GetJobClaimsHistory(int serviceReportId, int agencyId, string code);
        List<ReimbursementLineItem> GetReimbursementLineItems(int serviceReportId);
        IEnumerable<CustomDropdownItem> GetPurchaseOrder(int agencyId, int workCategoryID);
        ContractStatus GetContractStatusNotSignedByFMC(int agencyId);
        decimal GetClaimApprovedAmount(int jobId, int agencyId);
        List<LCIReasons> GetLCIReasons();
        bool HasPWASignedOff(int serviceReportId);
        bool HasContractorSignedOffCurrentMonth(int agencyId);
        ContractStatus GetLatestFMCSingedOffContractStatus(int agencyId);
        Claim GetLatestClaim(int agencyId, int? serviceReportId, short contractStatusYear, short contractStatusMonth);
        void GetTotalApprovedForThisClaimMonth(int? serviceReportId, ContractStatus contStatus);
        void ResetOverallApprovalForFMCReclaim(int agencyId, int claimMonth, int claimYear);
        bool? HasPWASignedOffLatestClaimMonth(int agencyId, short claimMonth, short claimYear);
        ContractStatus GetPreviousContractStatusSignedByFMC(int agencyId);
        void ResetFMCReclaimForSubmittedClaims(int agencyId, int contractStatusMonth, int contractStatusYear);
        void SetJobApprovedDate(int agencyId, int contractStatusMonth, int contractStatusYear);
        ContractStatus GetLatestContractStatusFromClaim(int agencyId, int serviceReportId, int v);
        string GetAllReclaimJobs(int agencyId, int claimMonth, int claimYear);

        bool CanAOApproveInMayJune(int orderId);
        bool? HasContractorSignedOffLatestClaimMonth(int agencyId, short claimMonth, short claimYear);
        bool UnfinalizeJob(JobDetails model, UserViewModel currentUser);
        bool SetJobToBeReclaimed(JobDetails model, UserViewModel currentUser);
        Model.Models.Order GetOrderDetails(int orderId);
        bool MutipleJobsBySameCalloutId(int agencyId, int callLogFITSId);
        bool ResetJob60Day(JobDetails model, UserViewModel currentUser);
        bool ResetJobNotReclaimedIn40Day(JobDetails model, UserViewModel currentUser);
        List<SubContractor> GetSubContractorsByAgency(int agencyId);
        List<SubContractor> GetLocalContractorsByAgency(int agencyId);
        List<TradieViewModel> GetTradies(int serviceReportId);
        List<TradieViewModel> GetAssignedTradies(int serviceReportId);
        List<Tradie> GetTradiesByCallout(int callLogFITSID);
        Tradie GetTradieDetails(int tradieId);
        SubContractor GetSubContractorDetails(int subContractorId);
        
        ServiceReportQRCode SaveQRCode(int serviceReportId, string loggedInUser);
        ServiceReportQRCode GetQRCode(JobDetails job);
       
        int? ConvertCallOutToWorkOrder(CallOutAddEditViewModel model, int subContractorId, string loggedInUser,string comments);
        bool UpdateJobAcceptance(WorkOrderAcceptanceStatus status);
        Dictionary<string, string> GetWorkOrderDetailsForEmail(int workOrderId);
        int CreateWOAsVariation(CreateWorkOrder model, out int serviceReportId);
        bool CanEditInsuranceStatusInWorkOrder(int callLogFitsId, int ServiceReportId);
        bool HasOnlyOneWOWithSchoolContribution(int callLogFitsId, int ServiceReportId);
        IEnumerable<WorkOrderCertifyListItem> GetClaimsToApprove(WorkOrderCertifyFilterModel search, List<int> userPropertyIds, out int filteredResultsCount, out int totalResultsCount);
        IEnumerable<InsuranceClaimsReimbursementListItem> GetClaimsToApproveForInsuranceReimbursement(InsuranceClaimsReimbursementFilterModel search, List<int> userPropertyIds, out int filteredResultsCount, out int totalResultsCount);

        bool ApproveClaims(WorkOrderApprovalListItem data, string loggedInUser, bool AMURegion);
        WorkOrderClaimApprovalViewModdel GetClaimApprovalData(int claimId);
        bool ReviewClaims(WorkOrderApprovalListItem item, string loggedInUser);
        bool ApproveInsuranceReimbursement(InsuranceReimbursementListItem data, string loggedInUser);
        //bool ReviewInsuranceClaims(InsuranceReimbursementListItem item, string loggedInUser);
        ItemsCounts GetServiceReportCounts(int agencyID, int propertyID, List<int> userPropertyIds, string roleID, int? localContractorID);
        List<int> GetBulkInvoiceEmailData(int agencyId,out string data, out decimal total);
        void UpdateClaimsForBulkInvoice(List<int> claimIds);
        WorkOrderEmailData GetWorkOrderEmailData(int serviceReportId);
        string GetReimbursementStatusDesc(int serviceReportId);
        IList<IRCMClaimStatus> GetIRCMClaimStatuses();
        IList<ReimbursementStatus> GetReimbursementStatuses();
        IList<AssessorStatus> GetAssessorStatuses();
        void GenerateReimbursementItems(IEnumerable<WorkOrderCertifyListItem> claims, string loggedInUser);
        IEnumerable<WorkOrderCertifyListItem> GetWorkOrderCertifyListItems(WorkOrderCertifyFilterModel workOderCertifyFilterModel, List<int> userPropertyIds, out int filteredResultsCount, out int totalResultsCount);
        LCWOViewModel GetLCWOCount(int subContractorId);
        IList<ClaimApprovalStatusFilter> GetReactiveClaimApprovalStatusFilters();

        string GetServiceReportAssignedTradies(int serviceReportId);
    }
}
