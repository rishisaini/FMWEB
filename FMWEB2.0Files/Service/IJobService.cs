using AssetCare.Model.Models;
using AssetCare.Model.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetCare.Service.Interfaces
{
    public interface IJobService
    {
        JobDetails GetCallOutDetails(int agencyId, int? callOutId, int? jobId, UserViewModel user);
        List<ServiceReportStatus> GetServiceReportStatus();
        //List<ServiceReportJobCategory> GetServiceReportJobCategories();
        List<SupportDocumentType> GetSupportDocumentTypes(int workCategoryId);
        bool Save(JobDetails model, UserViewModel loggedInUser, double estimationThreshold, string estimationThresholdText, out int jobId);
        IEnumerable<JobListViewModel> FilteredJobData(CallOutFilterModel callOutFilterModel, bool dataForExport, out int filteredResultsCount, out int totalResultsCount);
        ItemsCounts GetServiceReportCounts(int agencyID, int propertyID, List<int> userPropertyIds, string RoleID, int? localContractorID);
        IEnumerable<JobListForHPViewModel> FilteredJobDataForHP(CallOutFilterModel callOutFilterModel, bool dataForExport, out int filteredResultsCount, out int totalResultsCount);
        IEnumerable<JobListForHPViewModel> FilteredJobDataForHPClaim(CallOutFilterModel callOutFilterModel, bool dataForExport, out int filteredResultsCount, out int totalResultsCount);

        IEnumerable<JobListForHPViewModel> FilteredJobApprovalDataForHP(CallOutFilterModel callOutFilterModel, bool dataForExport, out int filteredResultsCount, out int totalResultsCount);
        IEnumerable<JobListForSchoolHPViewModel> FilteredJobDataForSchoolHP(CallOutFilterModel callOutFilterModel, bool dataForExport, out int filteredResultsCount, out int totalResultsCount);

        void GetJobDetails(JobDetails details);
        List<ServiceReportApprovalType> GetServiceReportApproavalTypes();
        List<ScheduledRateCategory> GetScheduleRateCatgories();
        List<ScheduleRateDropdownItem> GetScheduledRates(int agencyId, int? schduleRateCategoryId, int? subContractorId);
        decimal GetCostAdjustment(int agencyId, string costAdjType, string costDate);
        IEnumerable<CustomDropdownItem> GetPurchaseOrder(int agencyId, int workCategoryID);
       // List<LCIReasons> GetLCIReasons();
        string GetBuildingList(JobDetails model, bool NoOnly = false);
        void ResetFlagsAndNotifyFMC(int contractStatusId);
        //void ResetOverallApprovalForFMCReclaim(int agencyId, int claimMonth, int claimYear);
       // void ResetFMCReclaimForSubmittedClaims(int value, int contractStatusMonth, int contractStatusYear);
       // void SetJobApprovedDate(int value, int contractStatusMonth, int contractStatusYear);
       // List<TimeExtension> GetTimeExtensions();
        List<Element> GetElementList();
        List<WorkType> GetWorkTypes();
      //  PMPurchaseOrderDetailViewModel GetPurchaseOrderDetails(int? orderId);
        bool CanAOApproveInMayJune(int orderId);
        bool IsAMUUser(UserViewModel user);
        bool IsMFUUser(UserViewModel user);
        bool ResetJob(int agencyId, JobDetails model, UserViewModel currentUser);
        string GenerateQRCode(Guid qrCodeServiceReport);
        bool UpdateJobAcceptance(WorkOrderAcceptanceStatus status);
        int CreateWOAsVariation(CreateWorkOrder model,out int serviceReportId);
        bool HasOnlyOneWOWithSchoolContribution(int callLogFitsId, int ServiceReportId);
        bool CanEditInsuranceStatusInWorkOrder(int callLogFitsId, int ServiceReportId);
        IEnumerable<WorkOrderCertifyListItem> GetClaimsToApprove(WorkOrderCertifyFilterModel claimModel,out int filteredResultsCount, out int totalResultsCount);
        IEnumerable<InsuranceClaimsReimbursementListItem> GetClaimsToApproveForInsuranceReimbursement(InsuranceClaimsReimbursementFilterModel reimbursementModel, out int filteredResultsCount, out int totalResultsCount);
        bool ApproveClaims(WorkOrderApprovalListItem data, string usercode, bool amuRegion);
        bool ReviewClaims(WorkOrderApprovalListItem item, string usercode);
        bool ApproveInsuranceReimbursement(InsuranceReimbursementListItem data, string usercode);
        //bool ReviewInsuranceClaims(InsuranceReimbursementListItem item, string usercode);
        WorkOrderClaimApprovalViewModdel GetClaimApprovalData(int claimId);
        IList<IRCMClaimStatus> GetIRCMClaimStatuses();
        IList<ReimbursementStatus> GetReimbursementStatuses();
        IList<AssessorStatus> GetAssessorStatuses();
        void GenerateReimbursementItems(IEnumerable<WorkOrderCertifyListItem> result, string loggedInUser);
        IEnumerable<WorkOrderCertifyListItem> GetWorkOrderCertifyListItems(WorkOrderCertifyFilterModel workOderCertifyFilterModel, out int filteredResultsCount, out int totalResultsCount);
        IList<ClaimApprovalStatusFilter> GetReactiveClaimApprovalStatusFilters(int? currentAgencyTypeId);
    }
}
