using AssetCare.Model.Common;
using AssetCare.Model.Models;
using ExpressiveAnnotations.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;

namespace AssetCare.Model.ViewModels
{
    public class JobListViewModel
    {
        public int Id { get; set; }
        public int CallLogFITSId { get; set; }
        public string WorkflowStatus { get; set; }
        public int JobID { get; set; }
        public int CallOutId { get; set; }
        [Required(ErrorMessage = "Please select a school")]
        public int? SchoolId { get; set; }
        public int? PropertyId { get; set; }
        [Required(ErrorMessage = "Please select work category")]
        public int? WorkCategoryId { get; set; }
        public string SchoolName { get; set; }
        public string Buildings { get; set; }
        public IQueryable<IQueryable<string>> BuildingList { get; set; }
        public string WorkCategory { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string FMCRef { get; set; }
        public int? AgencyId { get; set; }
        public string EUR100 { get; set; }
        public string RequestedBy { get; set; }
        public bool CanEditJob { get; set; }
        public bool CanCopy { get; set; }
        public int? DaysSinceCompleted { get; set; }
        public int? Version { get; set; }
        public string SubContractor { get; set; }
        public DateTime? WODate { get; set; }
        public bool CanViewCallOut { get; set; }
        public bool? AMUClaimApproved { get; set; }
        public int? StatusId { get; set; }
        public string IsIncludeBetterment { get; set; }
        public string IsReimbursementComplete { get; set; }
        public string CurrentRoleID { get; set; }
        public bool? ShowWOPaidOnly { get; set; }
        public string Sig { get { return TamperHelper.GenerateSignature(this.Id); } }
        public string CallLogSig { get { return TamperHelper.GenerateSignature(this.CallLogFITSId); } }

    }
    public class JobListForHPViewModel
    {
        [UnSearchable(true)]
        public int Id { get; set; }
        public int? SubContractorId { get; set; }
        public int JobID { get; set; }
        public string JobType { get; set; }
        [Required(ErrorMessage = "Please select a school")]
        [UnSearchable(true)]
        public int PropertyID { get; set; }
        public int? SchoolId { get; set; }
        [Required(ErrorMessage = "Please select a school")]
        public string SchoolName { get; set; }
        [UnSearchable(true)]
        public string Description { get; set; }
        public DateTime? ReceivedDate { get; set; }
        //public bool CanEditJob { get; set; }
        public decimal? Estimate { get; set; }
        public string Status { get; set; }
        public string RequestedBy { get; set; }
        public DateTime? WODate { get; set; }
        public string WODateList { get; set; }
        public string LocalContractorName { get; set; }
        public int? ServiceReportStatusId { get; set; }
        public int? ServiceReportACID { get; set; }
        public string IsIncludeBetterment { get; set; }
        public string IsReimbursementComplete { get; set; }
        public string Sig { get { return TamperHelper.GenerateSignature(this.Id); } }
    }

    public class JobListForSchoolHPViewModel
    {
        [UnSearchable(true)]
        public int Id { get; set; }
        [UnSearchable(true)]
        public int AgencyID { get; set; }
        [UnSearchable(true)]
        public int PropertyID { get; set; }
        [UnSearchable(true)]
        public int WorkCategoryID { get; set; }
        public string WorkCategory { get; set; }
        [UnSearchable(true)]
        public int? ServiceReportId { get; set; }
        [UnSearchable(true)]
        public int? Key { get; set; }
        public int? JobID { get; set; }
        [UnSearchable(true)]
        public int CallOutKey { get; set; }
        public int CallOutID { get; set; }
        public DateTime? CallDate { get; set; }
        public string SchoolName { get; set; }
        public string CallDescription { get; set; }
        public string CallCategory { get; set; }
        [UnSearchable(true)]
        public bool? IsSchoolontribution { get; set; }
        public string IsSchoolontributionText { get; set; }
        public decimal? SchoolAmount { get; set; }
        //public DateTime? ReceivedDate { get; set; }
        //public string Buildings { get; set; }
        //public DateTime? EstCompletionDate { get; set; }
        //public DateTime? CompletionDate { get; set; }
        public string LocalContractorName { get; set; }
        [DateTimeFormatter("dd/MM/yyyy hh:mm")]
        public DateTime? LCExpectedVisitDate { get; set; }
        public string CallStatus { get; set; }
        [UnSearchable(true)]
        public int? RowNo { get; set; }
        public int? WOCount { get; set; }
        public string WOIDs { get; set; }
        public DateTime? CompletionDate { get; set; }
        public string SchoolUrgency { get; set; }
        public string Sig { get { return TamperHelper.GenerateSignature(this.Id); } }
    }

    public class JobDetails
    {
        public string defaultTab;

        public JobDetails()
        {
            DetailsTab = new JobDetailsTab();
            SupportDocument = new SupportDocumentViewModel();
            WorkItem = new JobWorkItem() { WorkDate = DateTime.Today };
            UserCapabilities = new JobUserCapabilities();
            Claim = new JobClaimViewModel();
            Claims = new List<Claim>();

        }

        public JobClaimViewModel Claim { get; set; }
        public JobUserCapabilities UserCapabilities { get; set; }
      //  public string EditableFieldList { get; set; }
        public int? CallLogFITSId { get; set; }
        public string CallbackUrl { get; set; }
        //public string Mode { get; set; }
        public int? ServiceReportId { get; set; }
        public int AgencyId { get; set; }
        public int CallOutId { get; set; }
        public int PropertyId { get; set; }
        public int? SchoolId { get; set; }
        public string SchoolPhone { get; set; }
        public string CurrentRoleID { get; set; }
        public string ASOName { get; set; }
        public string ASOEmail { get; set; }
        public string ASOContactNumber { get; set; }
        public string GLName { get; set; }
        public string GLContactNumber { get; set; }
        public string CallLogType { get; set; }
        public int? AgencySequenceId { get; set; }
        public string SchoolName { get; set; }
        public string SchoolAddress { get; set; }
        public string SchoolSitePlanPDFPath { get; set; }
        public string AsbestosRegisterPDFPath { get; set; }
        public string Phone { get; set; }
        public string SchoolEmail { get; set; }
        public decimal? SchoolLimit { get; set; }
        public string Area { get; set; }
        //public bool TreeSafetyIssue { get; set; }
        public int SupportDocumentClassID { get; set; }
        public bool CommerceNotInvolved { get; set; }
        //coming from CallLogFits Description
        [Required(ErrorMessage = "Work Order description required")]
        public string ServiceReportDesc { get; set; }
       // public bool EUR100 { get; set; }
        //public bool SchEUR100 { get; set; }
        public DateTime CallLogDate { get; set; }
        [Required(ErrorMessage = "Work category required")]
        public int? WorkCategoryId { get; set; }
        public string WorkCategoryDesc { get; set; }

        [Required(ErrorMessage = "Work Type required")]
        public int? WorkTypeId { get; set; }

        public int? ProposalId { get; set; }
        public string SigProposal { get { return TamperHelper.GenerateSignature(ProposalId != null ? ProposalId.Value : 0); } }

        public bool LRLV { get; set; }
        public string CostCentreCode { get; set; }

        public string WorkTypeHelpText { get; set; }
        
        [Required(ErrorMessage = "Please select Element")]
        public int? ElementId { get; set; }

        public string Element { get; set; }
        //[AssertThat("((ValidateElementAssembly) && (ElementId > 0 || AssemblyGroupId > 0 || ComponentCASId > 0)) ? false : false", ErrorMessage = "Please select Assembly")]
        //[RequiredIf("((ValidateElementAssembly) || (ElementId > 0 || AssemblyGroupId > 0 || ComponentCASId > 0))", ErrorMessage = "Please select Assembly")]
       // public int? AssemblyGroupId { get; set; }

        //[AssertThat("((ValidateElementAssembly || (ElementId > 0 || AssemblyGroupId > 0 )) && SubAssemblyCount >0) ? ComponentCASId > 0 : true", ErrorMessage = "Please select Sub Assembly")]
       // public int? ComponentCASId { get; set; }

        public int? WorkCategorySchId { get; set; }
        public string WorkCategorySchDesc { get; set; }
        //[Required(ErrorMessage = "Maintenance task is required")]
       // public int? MaintExtraTaskId { get; set; }
        public string MaintExtraTaskDesc { get; set; }
        //[RequiredIf("WorkCategoryId==13 && MaintExtraTaskId==31", ErrorMessage = "Assembly is required")]
       // public int? EquipmentTypeBSEMAssemblyID { get; set; }
        //public string AssemblyEquipmentDesc { get; set; }

        [Required(ErrorMessage = "Buildings affected is required.")]
        public List<int> JobBuildingIDs { get; set; }
        [Required(ErrorMessage = "Rooms are required.")]
        [MaxLength(15, ErrorMessage = "15 max")]
        public string RoomNumber { get; set; }
        public string RoomLocation { get; set; }
        public int? SelectedDocumentTypeId { get; set; }
        public string ClaimMonthYear { get; set; }
        public string RecordedBy { get; set; }
        public DateTime? RecordedDate { get; set; }
        public string LastRecordedBy { get; set; }
        public DateTime? LastRecordedDate { get; set; }
        public DateTime? WorkOrderCreatedDate { get; set; }
        public string SchoolResponseTimeDesc { get; set; }
        public string FMCResponseTimeDesc { get; set; }
        public string ProcessedBy { get; set; }
        public string AgencyContactNumber { get; set; }
        public bool? IsSchoolContribution { get; set; }
        public bool? IsInsuranceEvent { get; set; }
        public bool? IsIncludeBetterment { get; set; }
        public DateTime? InsuranceIncidentDateTime { get; set; }
        [Required(ErrorMessage = "Valid incident date is required")]
        public DateTime? IncidentDate { get; set; }
        [Required(ErrorMessage = "Valid incident time is required")]
        [RegularExpression(@"^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Time format must be HH:mm.")]
        public string IncidentTime { get; set; }
        public SupportDocumentViewModel SupportDocument { get; set; }
        public List<SupportDocumentViewModel> SupportDocuments { get; set; }
        public List<SupportDocumentViewModel> SupportQuoteDocuments { get; set; }
        public List<SupportDocumentViewModel> SupportDocumentsCallout { get; set; }
        public string SupportDocs { get; set; }
        public string SupportQuoteDocs { get; set; }

        //validate only when ValidateDocumentsFinalised is true. Otherwise previous Job records will not be saved
        //[AssertThat("((WorkCategoryId == 1 || (WorkCategoryId == 13 && Version == 2)) && ContractorFinalise == true && ValidateDocumentsFinalised) ? DocumentsFinalised == true : true", ErrorMessage = "Finalise the documents in 'Records' section")]
        //[AssertThat("(ValidateDocumentsFinalised && DocumentsFinalised == true) ? SupportDocumentsCount > 0 : true", ErrorMessage = "Should have at least one document")]
       // public bool? DocumentsFinalised { get; set; }

       // public bool ValidateDocumentsFinalised { get; set; }

        public JobDetailsTab DetailsTab { get; set; }
        public JobWorkItem WorkItem { get; set; }
        // [AssertThat("DocumentsFinalised == true", ErrorMessage = "Finalise the documents in 'Records' section")]
       // public bool ContractorFinalise { get; set; }
      /////  public DateTime? ContractorFinaliseDate { get; set; }
       // public string PWAApprovalComments { get; set; }
      //  public string PWAApprovedBy { get; set; }
      //  public bool? HasPWAApproved { get; set; }
      //  [AssertThat("ValidatePWAApproval == true ? PWAApproved == true: true", ErrorMessage = "AP Approval required")]
      //  public bool? PWAApproved { get; set; }
      //  public DateTime? PWAApprovedDate { get; set; }
        public decimal TotalClaimed { get; set; }
       //public bool? LCIFlag { get; set; }
        //public string LCIUseYN { get; set; }
        //[RequiredIf("LCIUseYN == 'N'", ErrorMessage = "Reason required")]
        //public int? ReasonID { get; set; }

      //  public bool? NominatedYN { get; set; }

        // [AssertThat("(ValidateContractorReClaim && ContractorReClaim == true) ? PWAApproved == true : true", ErrorMessage = "PWA Approval is required.")]
        public bool? ContractorReClaim { get; set; }
      //  public bool ValidatePWAApproval { get; set; }
     //   public bool ValidateElementAssembly { get; set; }

        public bool? IsClaimOver60Days { get; set; }
        public bool? JobNotReclaimedFor40Days { get; set; }

        public bool? IsJobReClaimed { get; set; }
      //  public DateTime? FinanlisedAndSubmitEstimateUntickedEmailDT { get; set; }
        public DateTime? ContractorReClaimDate { get; set; }

        public string AcceptedFileTypes
        {
            get
            {
                return ConfigurationManager.AppSettings["AcceptedFileTypes"].ToString();
            }
        }

        public bool DisableField(string section, string field)
        {
            return HelperMethods.DisableField(this.UserCapabilities, section, field);
        }


        public JobEstimate Estimate { get; set; }


        public string ServiceReportNewLineItems { get; set; }
        public string DeletedWorkItems { get; set; }
        public string ApprovedWorkItems { get; set; }
        public string RejectedWorkItems { get; set; }
        public List<JobWorkItem> WorkItems { get; set; }
        public List<Claim> Claims { get; set; }
        public bool HasPWASignedOff { get; set; }
        public AgencyContractor AgencyContractor { get; set; }
        public string BuildingsList { get; set; }
        public decimal? TotalApproved { get; set; }
        public decimal? TotalApprovedInPreviousClaimMonths { get; set; }


        // public decimal? TotalAvailableToClaim { get; set; }
        public bool HasContractorSignedOffCurrentMonth { get; set; }
        public bool? HasPWASignedOffLatestClaimMonth { get; set; }
        public bool? HasContractorSignedOffLatestClaimMonth { get; set; }

        public string SaveType { get; set; }

        //This flag is used to count whether there are any support docuemtns uploaded
        //used for ExpressiveDataAnnotations
        public int SupportDocumentsCount
        {
            get
            {
                return (this.SupportDocuments != null ? this.SupportDocuments.ToList().Count : 0);
            }
        }

        public int SupportQuoteDocumentsCount
        {
            get
            {
                return (this.SupportQuoteDocuments != null ? this.SupportQuoteDocuments.Count : 0);
            }
        }

        public bool UserCannotPerformAnyAction()
        {
            if (this.UserCapabilities == null ||
              ((string.IsNullOrEmpty(this.UserCapabilities.MainSection) || this.UserCapabilities.MainSection == "calloutLink") &&
              string.IsNullOrEmpty(this.UserCapabilities.EstimateSection) &&
              (string.IsNullOrEmpty(this.UserCapabilities.DetailSection)) &&
              string.IsNullOrEmpty(this.UserCapabilities.CostingSection) &&
               string.IsNullOrEmpty(this.UserCapabilities.RecordSection) &&
               string.IsNullOrEmpty(this.UserCapabilities.ClaimSection)))
            {
                return true;
            }

            return false;
        }

      //  public int SubAssemblyCount { get { return 1; } }

        public string SchoolCategoryId { get; set; }
        public int Version { get; set; }
        public List<TradieViewModel> TradieSelectList { get; set; }
        public Guid QRCodeId { get; set; }
        public string QRCode { get; set; }
        public bool? IsInsuranceEventByWO { get; set; }
        //public bool? IsInsuranceCallout { get; set; }
        [Required(ErrorMessage = "Local Contractor required")]
        public int? WOSubContractorId { get; set; }
        public int? SubContractorBusinessTypeId { get; set; }
        public string WOComments { get; set; }
        
        public string AssociatedJobID { get; set; }
        public int? AssociatedJobSequenceID { get; set; }

        public string WorkTypeDesc { get; set; }

        public bool IsAMURegion { get; set; }
        public bool? IsTotalEstimationOverLimit { get; set; }
        public string SchoolIncidentCauseDesc { get; set; }
    }


    public class JobDetailsTab
    {

       // public string CallLogFinanceUrgencyDesc { get; set; }
        [Required(ErrorMessage = "Urgency is required")]
        public int? CallLogFinanceUrgencyId { get; set; }
        //public int? JobStatusId { get; set; }
        [Required(ErrorMessage = "Work Order status is required")]
        [Remote("IsValidResportStatus", "WorkOrder", HttpMethod = "POST", AdditionalFields = "ClaimFinalizedForJobStatusValidation,HasAtleastOneApprovedClaim,SubContractorAccepted", ErrorMessage = "You cannot Cancel this work order. Please contact admin.")]
        public int? ServiceReportStatusId { get; set; }
        //sewage
        public int? JobCategoryID { get; set; }
        public int? NoOfRepairs { get; set; }

        [Remote("IsValidExpectedVisitDate", "WorkOrder", HttpMethod = "POST", AdditionalFields = "CompletedDate,FirstVisitDate,ValidateCompFirstDates,CallLogDateVal,ExpectedVisitDate", ErrorMessage = "Please provide a valid Expected Visit date.")]
        [RequiredIf("SubContractorAccepted == true", ErrorMessage = "Expected Visit Date is required")]
        public DateTime? ExpectedVisitDate { get; set; }
        [RequiredIf("SubContractorAccepted == true", ErrorMessage = "Expected Visit Time is required")]
        [DisplayFormat(DataFormatString = "{0:HH:mm}", ApplyFormatInEditMode = true)]
        [DataType(DataType.Time)]
        public DateTime? ExpectedFirstVisitTime { get; set; }

        [RequiredIf("ValidateDatesOnEstimateFinalised == true", ErrorMessage = "Estimated Completion date is required")]
        [Remote("IsValidEstimatedDate", "WorkOrder", HttpMethod = "POST", AdditionalFields = "ExpectedVisitDate,CompletedDate,FirstVisitDate,ValidateCompFirstDates", ErrorMessage = "Please provide a valid EstimatedCompletionDate.")]
        public DateTime? EstimatedCompletionDate { get; set; }

        [RequiredIf("ServiceReportStatusId == 5", ErrorMessage = "First visit date is required")]        
        [RequiredIf("ValidateDatesOnEstimateFinalised == true", ErrorMessage = "First visit date is required")]
        [Remote("IsValidFirstVisitDate", "WorkOrder", HttpMethod = "POST", AdditionalFields = "CompletedDate,EstimatedCompletionDate,ValidateCompFirstDates,CallLogDateVal", ErrorMessage = "Please provide a valid FirstVisitDate.")]
        public DateTime? FirstVisitDate { get; set; }

        


        public bool ValidateCompFirstDates { get; set; }
        //Completed
        [Remote("IsValidWorkCompletedDate", "WorkOrder", HttpMethod = "POST", AdditionalFields = "EstimatedCompletionDate,FirstVisitDate,ValidateCompFirstDates", ErrorMessage = "Please provide a valid date.")]
        [RequiredIf("ServiceReportStatusId==8 || ServiceReportStatusId==10 ", ErrorMessage = "Date work all completed is required")]
        public DateTime? CompletedDate { get; set; }
        public bool ValidateDatesOnEstimateFinalised { get; set; }
        //fmc refe number
        public string ContractorWONo { get; set; }
        public string RequesterContactNumber { get; set; }
        public string SiteContactPerson { get; set; }
        public string SiteContact { get; set; }
        public string LoggedInUser { get; set; }
        public string StatusOfRepair { get; set; }
        public string FMCContactNumber { get; set; }
        [RequiredIf("SubContractorAccepted == false && LoggedInUser =='LC'", ErrorMessage ="Local Contractor Comments required" )]
        public string SubContractorComments { get; set; }
        public string AMUComments { get; set; }
        public string PWAComments { get; set; }

        //FMC comments
        [RequiredIf("SubContractorAccepted == false && ServiceReportStatusId!=9  && LoggedInUser =='C'", ErrorMessage = "FMC Comments required")]
        //[RequiredIf("ServiceReportStatusId==9 && LoggedInUser =='C'",  ErrorMessage = "FMC Comments required")]
        public string Comments { get; set; }
        public bool AuditMaterialsComply { get; set; }
        public bool AuditWorkComply { get; set; }
        public bool AuditExcessMaterials { get; set; }
        public bool AuditWorkersSignIn { get; set; }
        public bool AuditSiteSafe { get; set; }
        public bool AuditHazardsSafe { get; set; }
        public bool CanJobCancelled { get; set; }
        public bool HasAtleastOneApprovedClaim { get; set; }
        public bool ClaimFinalizedForJobStatusValidation { get; set; }
        public DateTime? ContractorReclaimedDateForJobStatusValidation { get; set; }
        public bool MultipleJobsExistForSameCalloutId { get; set; }
        public DateTime CallLogDateVal { get; set; }
        public int? SubContractorId { get; set; }
        public int? TradieId { get; set; }
        [Required(ErrorMessage = "Tradie needs to be assigned")]
        //public string AssginedTradieNames { get; set; }
        public string OriginalAssignedTradieIds { get; set; }
        public string AssignedTradieIds { get; set; }
        public List<TradieViewModel> AssignedTradies { get; set; }

        [RequiredIf("ServiceReportStatusId!=9 && LoggedInUser =='C'", ErrorMessage ="Local Contractor Acceptance required")]        
        public bool? SubContractorAccepted { get; set; }
        public DateTime? SubContractorRespondedDate { get; set; }
        public string SubContractorRespondedBy { get; set; }
        public string CallerName { get; set; }

        public string RequestedBy { get; set; }
        public string SubContractorName { get; set; }
        public string SubContractorContactPersonName { get; set; }
        public string SubContractorMobile { get; set; }
        public string SubContractorEmail { get; set; }
        public bool SchoolSecuritySubContractor { get; set; }

        public DateTime? FMCCommentDate { get; set; }

        public DateTime? ASOCommentDate { get; set; }

        public DateTime? SubContractorCommentDate { get; set; }
    }

    public class CombinedMinLengthAttribute : ValidationAttribute
    {
        public CombinedMinLengthAttribute(int estimatedAmount, int proposalId)
        {
            this.ProposalId = proposalId;
            this.EstimatedAmount = estimatedAmount;
        }

        public int ProposalId { get; private set; }
        public int EstimatedAmount { get; private set; }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if(ProposalId>0 && EstimatedAmount>10000)
                return new ValidationResult(this.FormatErrorMessage("Amount cant be greater than 10k"));
            //var properties = this.PropertyNames.Select(validationContext.ObjectType.GetProperty);
            //var values = properties.Select(p => p.GetValue(validationContext.ObjectInstance, null)).OfType<string>();
            //var totalLength = values.Sum(x => x.Length) + Convert.ToString(value).Length;
            //if (totalLength < this.MinLength)
            //{
            //    return new ValidationResult(this.FormatErrorMessage(validationContext.DisplayName));
            //}
            return null;
        }
    }

    public enum TradieChangeAction
    {
        Add,
        Remove
    }
    public class TradieChange
    {
        public int TradieId { get; set; }
        public TradieChangeAction Action { get; set; } // "Add" or "Remove"
    }

    public class JobEstimate
    {
        //this is a radio button in front end, shows whether it is over limit or under limit
        //Over Limit = 2 and Under Limit = 1 
        //It is applicable only for the EUR jobs       
        //[RequiredIf("EstimateFinalised== true", ErrorMessage = "Limit status is required to submit estimate.")]
        public int? LimitStatus { get; set; }
        /// <summary>
        /// submit estimate
        /// </summary>
        public bool EstimateFinalised { get; set; }
        //[RequiredIf("EstimateFinalised== true", ErrorMessage = "Estimated By required")]
        public string EstimatedBy { get; set; }
        [RequiredIf("EstimateFinalised== true", ErrorMessage = "Estimate Comments required")]
        public string EstimateComments { get; set; }
     //   public bool? ContractorFinalise { get; set; }

        [RequiredIf("EstimateFinalised== true", ErrorMessage = "Estimate Date required")]
        //[AssertThat("EstimateDate <= Today()", ErrorMessage = "Estimate date cannot be in future")]
        public DateTime? EstimateDate { get; set; }

        
        [RequiredIf("EstimateFinalised==true", ErrorMessage = "Estimated Amount required")]
        [RegularExpression(@"^[0-9]\d*(\.\d+)?$", ErrorMessage = "Estimated Amount must be a valid number.")]
        public decimal? EstimatedAmount { get; set; }

        [RequiredIf("FMCApproved==false", ErrorMessage = "Recommended Action required")]
        [MaxLength(250)]
        public string FMCRecommendedAction { get; set; }
        public bool? FMCApproved { get; set; }
        [MaxLength(1000)]
        [RequiredIf("FMCApproved==true", ErrorMessage = "Comments required")]
        public string FMCApprovalComments { get; set; }
        [MaxLength(50)]
        [RequiredIf("FMCApproved==true ", ErrorMessage ="Recommended by required")]
        public string FMCApprovedBy { get; set; }
        [RequiredIf("FMCApproved==true", ErrorMessage = "Recommended Date required")]
        public DateTime? FMCApprovedDate { get; set; }
        public bool? AMUApproved { get; set; }
        [MaxLength(1000)]
        [RequiredIf("AMUApproved==true || AMUApproved ==false", ErrorMessage = "Comments by required")]
        public string AMUApprovalComments { get; set; }
        [MaxLength(50)]
        [RequiredIf("AMUApproved==true ", ErrorMessage = "Approved by required")]
        public string AMUApprovedBy { get; set; }
        public DateTime? AMUApprovedDate { get; set; }
        /// <summary>
        /// Deperecated
        /// </summary>
        //public bool? DETAccepted { get; set; }
        public int? ProposalId { get; set; }
        public bool LRLV { get; set; }
        //[RequiredIf("DETAccepted== true", ErrorMessage = "Approved amount required")]
        public decimal? ApproveAmount { get; set; }
        public decimal? MaxApprovedWorkOrderAmount { get; set; }
        //[RequiredIf("DETAccepted== true", ErrorMessage = "Approval Type required")]
        //public int? ApprovalType { get; set; }

        //public int? TimeExtensionId { get; set; }

        //public string ApprovalTypeDesc { get; set; }
        //public DateTime? CommenceWorkDate { get; set; }
        //AMU comments
        //[RequiredIf("DETAMCUApproval== true || DETAMUApproval== true", ErrorMessage = "Comments required")]
        //public string CommerceComments { get; set; }

        //2nd Column
        //[RequiredIf("DETAMCUApproval== true || DETAMUApproval== true", ErrorMessage = "PWA Standard Fee required")]
        //[RegularExpression(@"\d+(\.\d{1,2})?", ErrorMessage = "AP Standard Fee must be a valid number.")]
        //public decimal? FeeAmount { get; set; } = 0;

        // [RequiredIf("DETAMCUApproval== true || DETAMUApproval== true", ErrorMessage = "PWA Other Fee required")]
        //[RegularExpression(@"\d+(\.\d{1,2})?", ErrorMessage = "Other Fee must be a valid number.")]
        //public decimal? FeeOther { get; set; } = 0;

        //[RequiredIf("DETAMCUApproval== true || DETAMUApproval== true", ErrorMessage = "Recommended By required")]
        //public string CommerceRecommendedBy { get; set; }
        /// <summary>
        ///1. Is total fee a calculated field? 
        ///1. What is Comments field
        ///What is Recommended By
        ///What is recommended date?
        /// 
        /// 
        /// 3D AMU
        ///  FMC estimate is Estimated Amount from 1C
        ///  PWA Fees  is TotalFees from 2A
        ///  Total Price, is it a calculated?
        ///  Approval Type - is it same as ApprovalType in 1C
        ///  Comments?
        ///  purchase Order
        ///  Approved By
        ///  ApprovalDate
        /// </summary>

        public string RecordedBy { get; set; }
        public DateTime? CommerceApprovalDate { get; set; }

        //[RequiredIf("DETAccepted== true", ErrorMessage = "Approved By required")]
        public string DETApproverName { get; set; }

        public string DETApprovalRecorderdBy { get; set; }
        public DateTime? DETApprovalDate { get; set; }

        //[RequiredIf("DETAccepted== true", ErrorMessage = "Comments required")]
        public string DETComments { get; set; }
        public bool DETAMCUApproval { get; set; }
        public bool DETAMUApproval { get; set; }
        public bool CommenceWork { get; set; }

        //[RequiredIf("DETAccepted== true", ErrorMessage = "Purchase Order required")]
        //[Remote("IsValidPurchaseOrder", "WorkOrder", HttpMethod = "POST", AdditionalFields = "ApproveAmount", ErrorMessage = "Invalid purchase order")]
        public int? OrderId { get; set; }

        public string OrderIdDesc { get; set; }
        public decimal? OriginalEstimate { get; set; }

    }

    public class JobViewModel
    {
        public int ServiceReportId { get; set; }


        public int? LimitStatus { get; set; }


        public int? WorkSubCategoryId { get; set; }

        public short? DETCostCategoryId { get; set; }

        public int? CallLogFITSId { get; set; }

        public int? InitiatedById { get; set; }
        public int? OrderId { get; set; }
        //Not used, use ServiceReportStatusId instead

        public int? CallLogFinanceUrgencyId { get; set; }
        public int? CopyToNewId { get; set; }
        public int? ScheduledRateId { get; set; }
        //Not used

        public string RoomNumber { get; set; }
        public int? InitiatedBy { get; set; }

        public string ServiceReportCode { get; set; }
        public bool? Completed { get; set; }
        public bool? Invoiced { get; set; }
        public DateTime? InvoicedDate { get; set; }

       // public DateTime? ContractorFinaliseDate { get; set; }

        public bool? WYD { get; set; }

        public bool? MandatoryWarningOnly { get; set; }

        public DateTime? RecordedDate { get; set; }
        public string LastRecordedBy { get; set; }
        public DateTime? LastRecordedDate { get; set; }
        public bool? TEMPNoOrder { get; set; }
        public bool? SchEUR100 { get; set; }
        //Local Contractor Initiative


        public bool? DocumentsFinalised { get; set; }
    }

    public class JobWorkItem
    {
        public int ServiceReportLineItemID { get; set; }
        public string ScheduleItemNo { get; set; }
        public DateTime? WorkDate { get; set; }
        public int? ScheduleRateID { get; set; }

        public bool CanAction { get; set; }
        public short? LineNumber { get; set; }
        [MaxLength(255)]
        public string SRLIDescription { get; set; }

        public int? WorkCategoryID { get; set; }
        [MaxLength(50)]
        public string TradeName { get; set; }

        [MaxLength(15)]
        public string MSRReference { get; set; }

        public double? CostAdjPercent { get; set; }

        public double? Quantity { get; set; }

        public decimal? Rate { get; set; }

        [MaxLength(30)]
        public string Unit { get; set; }

        public decimal? Amount { get; set; }

        public decimal? HoursWorked { get; set; }

        public decimal? ClaimedAmount { get; set; }

        public decimal? ClaimedGST { get; set; }

        public decimal? ApprovedAmount { get; set; }

        public decimal? ApprovedGST { get; set; }

        public bool? HasPWAApproved { get; set; }

        [MaxLength(250)]
        public string Reason { get; set; }

        public bool PWAApproved { get; set; }

        [MaxLength(15)]
        public string ReportedBy { get; set; }

        public DateTime? ReportedDate { get; set; }

        public bool? LCIFlag { get; set; }

        [MaxLength(15)]
        public string PWAApprovedBy { get; set; }

        public DateTime? PWAApprovedDate { get; set; }
        public int? PropertyId { get; set; }
        public bool? CanDelete { get; set; }
        public Int16? APApprovedClaimMonth { get; set; }
        public Int16? APApprovedClaimYear { get; set; }
        public bool? Deleted { get; set; }
        public bool? WorkItemApprovalStatus { get; set; }
        public bool? PWARejected { get; set; }
        public int? ReimbursementStatusID { get; set; }
        public decimal? IRCMReimbursementAmount { get; set; }
        [StringLength(512)]
        public string IRCMComments { get; set; }
        public DateTime? IRCMReimbursementDate { get; set; }
        
    }

    public class ScheduleRateDropdownItem
    {
        public int? ScheduledRateCategoryId { get; set; }
        public string ScheduledRateID { get; set; }
        public string ScheduledRateDesc { get; set; }
    }

    [Serializable()]
    public class JobUserCapabilities
    {
        public bool Hide2A { get; set; }
        public bool Hide3D { get; set; }
        public bool Hide4A { get; set; }


        public string MainSection { get; set; }
        public string DetailSection { get; set; }
        public string EstimateSection { get; set; }
        public string CostingSection { get; set; }
        public string RecordSection { get; set; }
        public string ClaimSection { get; set; }


        // public Dictionary<string, string> EditableFieldsList { get; set; }

    }

    public class JobClaimViewModel
    {
        public int ClaimMonth { get; set; }
        public int ClaimYear { get; set; }
       // public decimal RemainingClaimed { get; set; }

        public int? ServiceReportStatusIdForValidation { get; set; }
       // public bool ContractorFinaliseForValidation { get; set; }
        public decimal ApprovedClaimAmount { get; set; }


        public decimal? TotalAvailableToClaimThisMonth { get; set; }

        [DisplayFormat(DataFormatString = "{0:n2}", ApplyFormatInEditMode = true)]
        public decimal TotalAmount { get; set; }
        //Status should be either Completed or Completed and Claimed
        //[AssertThat("(ClaimFinalized == true && RemainingClaimed>0) ? (TotalAvailableToClaimThisMonth <= RemainingClaimed ): true", ErrorMessage = "Claimed amount should be less than or equal to Remaining Claim")]
        //[AssertThat("ClaimFinalized == true ? ContractorFinaliseForValidation == true : true", ErrorMessage = "FMC Finalise should be ticked in Costing tab")]
        [AssertThat("ClaimFinalized == true ? (ServiceReportStatusIdForValidation == 10 || ServiceReportStatusIdForValidation == 11): true ", ErrorMessage = "Change the Work Order status to 'Closed'")]
        [AssertThat("ClaimFinalized == false ? (ServiceReportStatusIdForValidation != 10): true ", ErrorMessage = "Work Order status cannot be 'Closed'")]
        public bool ClaimFinalized { get; set; }
        public DateTime? ClaimFinalizedDate { get; set; }
        public DateTime? FMCReviewedDate { get; set; }
        public DateTime? AMUApprovedDate { get; set; }
        public bool? AMUApproved { get; set; }

        public decimal TotalAMUApproved { get; set; }

        [DisplayFormat(DataFormatString = "{0:n2}", ApplyFormatInEditMode = true)]
        public decimal TotalApprovedAmount { get; set; }

        [DisplayFormat(DataFormatString = "{0:n2}", ApplyFormatInEditMode = true)]
        public decimal TotalReimbursementAmount { get; set; }
        public string ReimbursementStatusDesc { get; set; }
        public DateTime? ReimbursementDate { get; set; }
        public bool ReimbursementComplete { get; set; }
        public DateTime? ReimbursementCompleteDate { get; set; }
    }

    [Serializable]
    public class CreateWorkOrder
    {
        public int? SubContractorBusinessTypeId { get; set; }
        public int? WOSubContractorId { get; set; }
        public int? WOCalloutId { get; set; }
        public int? WOAgencyId { get; set; }

        public int WOServiceReportId { get; set; }
        [StringLength(1000)]
        public string WOComments { get; set; }
        public string LoggedInUser { get; set; }
    }

    public class WorkOrderAcceptanceStatus
    {
        public int JobId { get; set; }
        public int AgencyId { get; set; }
        public bool Status { get; set; }
        public string LoggedInUser { get; set; }
    }

    public class WorkOrderEmailData
    {
        public bool? FMCRecommend { get; set; }
        public DateTime? FMCRecommendDate { get; set; }
        public bool? AMUApproved { get; set; }
        public DateTime? AMUApprovedDate { get; set; }
        public DateTime? EstimateDate { get; set; }

    }

}