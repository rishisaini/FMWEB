using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Spatial;

namespace AssetCare.Model.Models
{
    [Table("ServiceReport")]
    public partial class ServiceReport
    {
        [Key]
        public int ServiceReportId { get; set; }
        public int? AgencySequenceId { get; set; }
        [ForeignKey("Agency")]
        public int AgencyId { get; set; }
        [ForeignKey("Property")]
        public int? PropertyId { get; set; }
        public decimal? SchoolLimit { get; set; }
        //this is a radio button in front end, shows whether it is over limit or under limit
        //Over Limit = 2 and Under Limit = 1 
        //It is applicable only for the EUR jobs
        public int? LimitStatus { get; set; }
        [ForeignKey("WorkCategory")]
        public int? WorkCategoryId { get; set; }
        public int? WorkSubCategoryId { get; set; }
        [ForeignKey("MaintenanceTask")]
        public int? MaintExtraTaskId { get; set; }
        [ForeignKey("CostCategory")]
        public short? DETCostCategoryId { get; set; }
        [ForeignKey("ServiceReportStatus")]
        public int? ServiceReportStatusId { get; set; }
        [ForeignKey("CallLogFITS")]
        public int? CallLogFITSId { get; set; }

        public int? InitiatedById { get; set; }
        [ForeignKey("Order")]
        public int? OrderId { get; set; }
        //Not used, use ServiceReportStatusId instead
        public int? JobStatusId { get; set; }
        [ForeignKey("CallLogFinanceUrgency")]
        public int? CallLogFinanceUrgencyId { get; set; }
        public int? CopyToNewId { get; set; }
        [ForeignKey("ScheduledRate")]
        public int? ScheduledRateId { get; set; }
        //Not used
        public int? EquipmentTypeBSEMID { get; set; }
        //Not user
        public int? EquipmentTypeBSEMAssemblyID { get; set; }
        [StringLength(1512)]
        public string ServiceReportDesc { get; set; }
        [StringLength(2512)]
        public string SubContractorComments { get; set; }
        [StringLength(2512)]
        public string Comments { get; set; }
        [StringLength(1024)]
        public string ContractorComments { get; set; }
        [StringLength(100)]
        public string RoomNumber { get; set; }
        public int? InitiatedBy { get; set; }
        [StringLength(256)]
        public string StatusOfRepair { get; set; }
        public DateTime? CompletedDate { get; set; }
        public DateTime? FirstVisitDate { get; set; }
        public DateTime? EstimatedCompletionDate { get; set; }
        [StringLength(128)]
        public string ContractorWONo { get; set; }
        [StringLength(50)]
        public string ServiceReportCode { get; set; }
        public bool? Completed { get; set; }
        public bool? Invoiced { get; set; }
        public DateTime? InvoicedDate { get; set; }
        [StringLength(128)]
        public string EstimatedBy { get; set; }
        [StringLength(1024)]
        public string EstimateComments { get; set; }
        public DateTime? EstimateDate { get; set; }
        public decimal? EstimatedAmount { get; set; }
        public bool? EstimateFinalised { get; set; }
        public bool? ContractorFinalise { get; set; }
        public DateTime? ContractorFinaliseDate { get; set; }
        [StringLength(128)]
        public string CommerceRecommendedBy { get; set; }
        public DateTime? CommerceApprovalDate { get; set; }
        public bool? DETAMCUApproval { get; set; }
        public bool? DETAMUApproval { get; set; }
        [StringLength(512)]
        public string CommerceComments { get; set; }
        public bool? DETAccepted { get; set; }
        [StringLength(64)]
        public string DETApproverName { get; set; }
        [StringLength(64)]
        public string DETApprovalRecorderdBy { get; set; }
        public DateTime? DETApprovalDate { get; set; }
        [StringLength(512)]
        public string DETComments { get; set; }
        public decimal? ApproveAmount { get; set; }
        [ForeignKey("ServiceReportApprovalType")]
        public int? ApprovalType { get; set; }
        public decimal? FeeAmount { get; set; }
        public decimal? FeeOther { get; set; }
        public bool? CommenceWork { get; set; }
        public DateTime? CommenceWorkDate { get; set; }
        public bool? WYD { get; set; }
        public bool? EUR100 { get; set; }
        public bool? CommerceNotInvolved { get; set; }
        public bool? AuditMaterialsComply { get; set; }
        public bool? AuditWorkComply { get; set; }
        public bool? AuditExcessMaterials { get; set; }
        public bool? AuditWorkersSignIn { get; set; }
        public bool? AuditSiteSafe { get; set; }
        public bool? AuditHazardsSafe { get; set; }
        [StringLength(64)]
        public string SMSRReference { get; set; }
        public int? NoOfRepairs { get; set; }
        [StringLength(64)]
        public string AssociatedJobID { get; set; }
        [ForeignKey("ServiceReportJobCategory")]
        public int? JobCategoryID { get; set; }
        public bool? MandatoryWarningOnly { get; set; }
        [StringLength(128)]
        public string RecordedBy { get; set; }
        public DateTime? RecordedDate { get; set; }
        [StringLength(128)]
        public string LastRecordedBy { get; set; }
        public DateTime? LastRecordedDate { get; set; }
        public bool? TEMPNoOrder { get; set; }
        public bool? SchEUR100 { get; set; }
        //Local Contractor Initiative
        public bool? LCIFlag { get; set; }
        [StringLength(16)]
        public string LCIUseYN { get; set; }
        [ForeignKey("LCIReason")]
        public int? ReasonID { get; set; }
        public bool? TreeSafetyIssue { get; set; }
        public bool? HasPWAApproved { get; set; }
        public bool? PWAApproved { get; set; }
        [StringLength(128)]
        public string PWAApprovedBy { get; set; }
        public DateTime? PWAApprovedDate { get; set; }
        [StringLength(1024)]
        public string PWAApprovalComments { get; set; }
        public bool? DocumentsFinalised { get; set; }
        [StringLength(1000)]
        public string AMUComments { get; set; }
        [StringLength(1000)]
        public string PWAComments { get; set; }
        [StringLength(1000)]
        public string APComments { get; set; }

        [ForeignKey("ServiceReportAC")]
        public int? ServiceReportACID { get; set; }

        public bool? ContractorReClaim { get; set; }
        public DateTime? ContractorReClaimDate { get; set; }
        public DateTime? FinanlisedAndSubmitEstimateUntickedEmailDT { get; set; }
        public bool? IsClaimOver60Days { get; set; }
        public bool? IsJobReClaimed { get; set; }
        public bool? ClaimFinalized { get; set; }
        public DateTime? ClaimFinalizedDate { get; set; }
        public int? Version { get; set; }

        [ForeignKey("Element")]
        public int? ElementId { get; set; }

        public int? AssemblyGroupId { get; set; }

        public int? ComponentCASId { get; set; }
        [ForeignKey("TimeExtension")]
        public int? TimeExtensionId { get; set; }

        [ForeignKey("WorkType")]
        public int? WorkTypeId { get; set; }
        [ForeignKey("SubContractor")]
        public int? SubContractorId { get; set; }
        [ForeignKey("Tradie")]
        public int? TradieId { get; set; }
        public DateTime? TradiesAssignedDate { get; set; }
        public bool? SubContractorAccepted { get; set; }
        public DateTime? SubContractorRespondedDate { get; set; }
        public DateTime? SubContractorAssignedDate { get; set; }
        [StringLength(50)]
        public string SubContractorRespondedBy { get; set; }
        public bool? FMCApproved { get; set; }
        [StringLength(1000)]
        public string FMCApprovalComments { get; set; }
        [StringLength(128)]
        public string FMCApprovedBy { get; set; }
        public DateTime? FMCApprovedDate { get; set; }
        [StringLength(250)]
        public string FMCRecommendedAction { get; set; }
        [StringLength(128)]
        public string VariationRaisedBy { get; set; }
        public DateTime? VariationRaisedDate { get; set; }

        public bool? AMUApproved { get; set; }
        [StringLength(1000)]
        public string AMUApprovalComments { get; set; }
        [StringLength(128)]
        public string AMUApprovedBy { get; set; }
        public DateTime? AMUApprovedDate { get; set; }
        [StringLength(1000)]
        public string LCComments { get; set; }
        public DateTime? ExpectedVisitDate { get; set; }
        public bool? IsInsuranceEvent { get; set; }
        public bool? IsInsuranceEventByWO { get; set; }
        public bool? IsIncludeBetterment { get; set; }
        public bool? IsSchoolContribution { get; set; }       
        public bool? ClaimApproved { get; set; }
        public bool? ClaimReviewedByFMC { get; set; }
        public decimal? OriginalEstimate { get; set; }
        public bool? ReimbursementComplete { get; set; }
        public DateTime? ReimbursementCompleteDate { get; set; }
        public bool? SCReceiptedYN { get; set; }
        public DateTime? SCReceiptDateTime { get; set; }

        public DateTime? SubContractorCommentDate { get; set; }
        public DateTime? FMCCommentDate { get; set; }
        public DateTime? ASOCommentDate { get; set; }
        //[Index("IX_SVCIKey", IsUnique = true)]
        //public Guid SVCIKey { get; set; }

        //[StringLength(1000)]
        public virtual Property Property { get; set; }
        public virtual Agency Agency { get; set; }
        public virtual WorkCategory WorkCategory { get; set; }
        public virtual MaintenanceTask MaintenanceTask { get; set; }
        public virtual CostCategory CostCategory { get; set; }
        public virtual ServiceReportStatus ServiceReportStatus { get; set; }
        public virtual CallLogFITS CallLogFITS { get; set; }
        public virtual Order Order { get; set; }
        public virtual CallLogFinanceUrgency CallLogFinanceUrgency { get; set; }
        public virtual ScheduledRate ScheduledRate { get; set; }
        public virtual ServiceReportApprovalType ServiceReportApprovalType { get; set; }
        public virtual ServiceReportJobCategory ServiceReportJobCategory { get; set; }
        public virtual LCIReasons LCIReason { get; set; }
        public virtual ServiceReportAC ServiceReportAC { get; set; }
        public virtual Element Element { get; set; }
        public virtual TimeExtension TimeExtension { get; set; }
        public virtual WorkType WorkType { get; set; }
        public virtual SubContractor SubContractor { get; set; }
        public virtual Tradie Tradie { get; set; }
        public virtual ICollection<TradieActivity> TradieActivities { get; set; }
        public virtual ICollection<ServiceReportTradie> ServiceReportTradies { get; set; }
    }
}
