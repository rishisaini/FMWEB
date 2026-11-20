using AssetCare.Model.Common;
using AssetCare.Model.Models;
using ExpressiveAnnotations.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace AssetCare.Model.ViewModels
{
    public class CallOutAddEditViewModel
    {
        public string mode;

        public CallOutAddEditViewModel()
        {
            this.SupportDocument = new SupportDocumentViewModel();
            this.SupportDocuments = new List<SupportDocumentViewModel>();
        }

        public int? AgencySequenceID { get; set; }
        public int CallLogFITSID { get; set; }
        public int AgencyID { get; set; }
        public int SchoolId { get; set; }
        public int PropertyID { get; set; }
        public int? CallOutId { get; set; }
        public int ContractorID { get; set; }
        public string WOComments { get; set; }
        public string SchoolName { get; set; }
        public string SchoolSitePlanPDFPath { get; set; }
        public string AsbestosRegisterPDFPath { get; set; }
        public string CallbackUrl { get; set; }
        public string Phone { get; set; }
        public string SchoolEmail { get; set; }
        public decimal? SchoolLimit { get; set; }
        public string Area { get; set; }
        public int? WorkCategoryID { get; set; }
        [RequiredIf("(CallLogStatusID != 2)", ErrorMessage = "Element is required")]
        public int? ElementId { get; set; }
        public int? AssemblyGroupId { get; set; }

        public int? ComponentCASId { get; set; }


        //[AssertThat("WorkTypeId == 8 && CapitalDefect == false ? false : true ", ErrorMessage = "Tick 'Capital Defect' or change the Work Type.")]
        [RequiredIf("(CallLogStatusID != 2)", ErrorMessage = "Work Type is required")]
        //[Required(ErrorMessage = "Work Type is required")]
        public int? WorkTypeId { get; set; }

        public string WorkCategoryDesc { get; set; }
        public int SelectedSchWorkCategoryId { get; set; }
        public string SchWorkCategory { get; set; }
        [Required(ErrorMessage = "Callout Date is required")]
        public DateTime? CallDate { get; set; }

        public DateTime? IncidentDate { get; set; }
        public bool WHSIssue { get; set; }
        public bool TreeSafetyIssue { get; set; }
        public bool PWANotInvolved { get; set; }
        public bool AMUEUR { get; set; }
        public bool SchEUR { get; set; }

        [Required(ErrorMessage = "'Callout Time' is required")]
        [Display(Name = "Time of Incident To")]
        public DateTime? CallLogTime { get; set; }

        [Required(ErrorMessage = "Valid incident time is required")]
        [Display(Name = "Time of Incident To")]
        public DateTime? IncidentLogTime { get; set; }

        [Required(ErrorMessage = "Valid incident time is required")]
        [Display(Name = "Time of Incident To")]
        public DateTime? SecIncidentLogTime { get; set; }
        [Required(ErrorMessage = "Callout description is required.")]
        public string CallLogDesc { get; set; }
        //TODO Buildings
        [Required(ErrorMessage = "Buildings affected is required.")]
        public List<int> JobBuildingIDs { get; set; }
        // [Required(ErrorMessage = "Rooms are required.")]
        [MaxLength(100, ErrorMessage = "100 max")]
        public string RoomNumber { get; set; }
        public string RoomLocation { get; set; }
        public bool EmailToSchoolAndEmc { get; set; }

        public string SaveType { get; set; }
        public DateTime? LastEmailDate { get; set; }
        public DateTime? EmailDate { get; set; }
        [Required(ErrorMessage = "Requested By is required")]
        public string RequestedBy { get; set; }
        [RequiredIf("(CallLogStatusID != 2)", ErrorMessage = "Response Time is required")]
        //[Required(ErrorMessage = "Response Time is required")]
        public int? CallLogFinanceUrgencyID { get; set; }

        [Required(ErrorMessage = "School response time is required")]
        public int? SchoolUrgencyID { get; set; }
        public DateTime? JobCreatedDate { get; set; }
        public bool CapitalDefect { get; set; }
        public bool? CapitalDefectCompleteForValidation { get; set; }
        public int? CapitalDefectWorkAssignmentForValidation { get; set; }

        [Required(ErrorMessage = "Call log status is required")]
        [AssertThat("CallLogStatusID == 2 ? JobId == null : true ", ErrorMessage = "Work Order already exists. You cannot Cancel this Call out.")]
        //[AssertThat("(CallLogStatusID == 4 || CallLogStatusID == 5)  ? Complete == true  : true ", ErrorMessage = "Tick 'Completed' in FMC Section")]
        [AssertThat("(CapitalDefect == false && CallLogStatusID == 4) ? Complete == true  : true ", ErrorMessage = "Tick 'Completed' in FMC Section")]
        [AssertThat("(CapitalDefect == true && CallLogStatusID == 4) ? CapitalDefectCompleteForValidation == true  : true ", ErrorMessage = "Tick 'Completed' in Defect PM/PD Section")]
        [AssertThat("(CapitalDefectWorkAssignmentForValidation == 3) ? CallLogStatusID == 1  : true ", ErrorMessage = "Change 'Work Assignment' in Defect PM/PD Section")]
        public int? CallLogStatusID { get; set; }

        public int? SubContractorId { get; set; }
        public int? SubContractorBusinessTypeId { get; set; }

        public SubContractor SubContractor { get; set; }

        [Required(ErrorMessage = "Requester Title is required")]
        public string RequesterPosition { get; set; }

        [Required(ErrorMessage = "Requester Contact is required")]
        public string RequesterContact { get; set; }

        public int? JobId { get; set; }
        public int? DefectId { get; set; }
        public string Comments { get; set; }
        public string TradesDescription { get; set; }
        [AssertThat("CallLogStatusID == 2", ErrorMessage = "Please add comments for cancelled call status.")]
        public string CallStatusComments { get; set; }
        public bool JobCancelled { get; set; }
        public int? SelectedCallLogTypeId { get; set; }
        public string CallTakenBy { get; set; }
        public string FMCRefNo { get; set; }

        public string EmailAlertTypes { get; set; }
        public string HAZMAT { get; set; }

        public bool? AsbestosHR { get; set; }
        public bool? DefectMgmt { get; set; }
        public decimal? ConceptEstimate { get; set; }

        public int? ProposalId { get; set; }
        public string SigProposal { get { return TamperHelper.GenerateSignature(ProposalId != null ? ProposalId.Value : 0); } }

        public int? SelectedMaintenanceCallLogId { get; set; }
        public int? SelectedEquipmentTypeId { get; set; }
        public int? SelectedDocumentTypeId { get; set; }
        public SupportDocumentViewModel SupportDocument { get; set; }
        public List<SupportDocumentViewModel> SupportDocuments { get; set; }
        public List<SubContractoWorkOrder> LocalContractorWorkOderDetails { get; set; }
        public List<TradieViewModel> Tradies { get; set; }

        public string SupportDocs { get; set; }
        [AssertThat("Complete == true ? (CallLogStatusID == 4 || CallLogStatusID == 5 ): true ", ErrorMessage = "Change the Callout Status to 'Complete'")]

        public bool Complete { get; set; }
        public bool FmcReadOnly { get; set; }
        public bool FormReadOnly { get; set; }
        public bool CanDeleteDocument { get; set; }
        public string AcceptedFileTypes
        {
            get
            {
                return ConfigurationManager.AppSettings["AcceptedFileTypes"].ToString();
            }
        }

        public bool EnableAMU100 { get; set; }
        public bool EnablePWANotInvolved { get; set; }
        public string NewContractors { get; set; }
        public string NewQuoteRequests { get; set; }
        public AgencyContractor Contractor { get; set; }
        public string SchoolAddress { get; set; }
        public Property SchoolDetails { get; set; }
        public bool CanEditFMCOnly { get; set; }
        public int? SchoolLimitId { get; set; }
        public CallOutUserCapabilities UserCapabilities { get; set; }
        public bool ShowSave { get; set; }
        public bool? HasAtleastOneJob { get; set; }
        //public bool? HasAtleaseOneClaim { get; set; }
        public bool DisableField(string section, string field)
        {
            return HelperMethods.DisableField(this.UserCapabilities, section, field);
        }

        [RequiredIf("CallLogTypeID == 2 && (IsInsuranceEventByWO == null || IsInsuranceEventByWO == false)", ErrorMessage = "Type of Incident is required")]
        public int? SchoolIncidentCauseId { get; set; }

        public string EventNo { get; set; }
        public int? CallLogTypeID { get; set; }
        public bool SchoolFunded { get; set; }

        [RequiredIf("CallLogTypeID == 2 && (IsInsuranceEventByWO == null || IsInsuranceEventByWO == false)", ErrorMessage = "Yes/No Required")]
        public bool? LocalCotractorToRepairDamage { get; set; }

        [RequiredIf("CallLogTypeID == 2 && (IsInsuranceEventByWO == null || IsInsuranceEventByWO == false)", ErrorMessage = "Yes/No Required")]
        public bool? LocalCotractorToSecurityIncident { get; set; }
        public string ASOName { get; set; }
        public string ASOEmail { get; set; }
        public string ASOContactNumber { get; set; }
        public string GLName { get; set; }
        public string GLContactNumber { get; set; }
        public string ProcessedBy { get; set; }

        public string RoleID { get; set; }
        public string CurrentRoleID { get; set; }

        public int? WorkerId { get; set; }
        public string WorkOrderComments { get; set; }

        public DateTime? ProcessedDate { get; set; }
        [Required(ErrorMessage = "Yes/No Required")]
        public bool? IsInsuranceEvent { get; set; }

        [Required(ErrorMessage = "Yes/No Required")]
        public bool? IsSecurityIncident { get; set; }
        public bool? IsTotalEstimationOverLimit { get; set; }
        public bool? IsReadyForReimbursement { get; set; }
        public bool? IsReimbursementComplete { get; set; }
        [RequiredIf("CallLogTypeID == 2 && (IsInsuranceEventByWO == null || IsInsuranceEventByWO == false)", ErrorMessage = "Is third party involved Required")]
        public bool? IsThirdpartyInvolved { get; set; }
        public bool? IsInsuranceEventByWO { get; set; }
        [StringLength(500)]
        public string CauseOfTheIssue { get; set; }
        [RequiredIf("SiteContactPerson != null && SiteContactPerson != ''", ErrorMessage = "validation is required when site contact person value is not null")]
        [RegularExpression(@"^[\s'0-9a-zA-Z]*", ErrorMessage = "Invalid characters are in the site contact person.")]
        public string SiteContactPerson { get; set; }
        [RequiredIf("SiteContact != null && SiteContact != ''", ErrorMessage = "validation is required when site contact value is not null")]
        [RegularExpression(@"^[\s\.'\-0-9a-zA-Z]*", ErrorMessage = "Invalid characters are in the site contact.")]
        public string SiteContact { get; set; }
        [RequiredIf("ClaimNumber != null && ClaimNumber != ''", ErrorMessage = "validation is required when claim number value is not null")]
        [RegularExpression(@"^[\s\/\.\-0-9a-zA-Z]*", ErrorMessage = "Invalid characters are in the claim number.")]
        public string ClaimNumber { get; set; }
        [RequiredIf("CORPFileNo != null && CORPFileNo != ''", ErrorMessage = "validation is required when CORP file number value is not null")]
        [RegularExpression(@"^[\s\/\.\-0-9a-zA-Z]*", ErrorMessage = "Invalid characters are in the CORP file number.")]
        public string CORPFileNo { get; set; }
        public int? AssessorStatusID { get; set; }
        [RequiredIf("CommentsByIRCM != null && CommentsByIRCM != ''", ErrorMessage = "validation is required when comments by IRCM value is not null")]
        [StringLength(1000, ErrorMessage = "The max size of comments by IRCM is 1000 characters")]
        public string CommentsByIRCM { get; set; }
        public string ProcessedByIRCM { get; set; }
        public DateTime? ProcessedDateByIRCM { get; set; }
        public DateTime? TotalEstimationOverLimitNotificationDate { get; set; }
        public CapitalDefectCalloutViewModel CapitalDefectCalloutViewModel { get; set; }

        public bool CanAddEditCapitalDefect { get; set; }

        public bool UsersCanAddEditCapitalDefect { get; set; }

        public bool IsAMURegion { get; set; }

        //Security details
        [RequiredIf("CallLogTypeID == 2 && (IsInsuranceEventByWO == null || IsInsuranceEventByWO == false)", ErrorMessage = "Yes/No Required")]
        public bool? IsSecInsuranceEvent { get; set; }

        [StringLength(100)]
        public string SecEventNo { get; set; }

        [StringLength(500)]
        public string SecCauseOfTheIssue { get; set; }

        [RequiredIf("IsSecurityIncident == true", ErrorMessage = "Yes/No Required")]
        public bool? SecIsThirdPartyInvolved { get; set; }

        [RequiredIf("IsSecurityIncident == true", ErrorMessage = "Yes/No Required")]
        public bool? SecLocalCotractorToRepairDamage { get; set; }
        [RequiredIf("IsSecurityIncident == true", ErrorMessage = "Type of Incident is required")]
        public int? SecSchoolIncidentCauseId { get; set; }

        public DateTime? SecIncidentDate { get; set; }

    }

    public class SubContractoWorkOrder
    {
        public int? SubContractorId { get; set; }
        public int WorkOrderId { get; set; }
        public bool? Accepted { get; set; }
        public string SubContractorName { get; set; }
        public DateTime? SubContractorRespondedDate { get; set; }
        public DateTime? SubContractorAssignedDate { get; set; }
        public string WorkOrderStatus { get; set; }
        public DateTime? WorkOrderCreatedDate { get; set; }
        public int ServiceReportId { get; set; }
        public int? WorkOrderCategoryId { get; set; }
        public string WorkOrderCategory { get; set; }
        public string Sig { get { return TamperHelper.GenerateSignature(this.ServiceReportId); } }
    }
}
