using AssetCare.Model.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetCare.Model.Models
{
    [Table("ServiceReportProActive")]
    public partial class ServiceReportProActive
    {

        [Key]
        public int ServiceReportProActiveId { get; set; }

        public int AgencySequenceWOID { get; set; }

        // Service ID
        //[ForeignKey("EquipServiceStatusLineItem")]
        //public int EquipServiceStatusLineItemID { get; set; }
        //public virtual EquipServiceStatusLineItem EquipServiceStatusLineItem { get; set; }

        public int APMPServiceScheduleID { get; set; }

        public int RecordedBy { get; set; }
        public DateTime? RecordedDate { get; set; }

        public bool InvoicedYesOrNo { get; set; }

        [ForeignKey("SubContractor")]
        public int? SubContractorId { get; set; }
        public virtual SubContractor SubContractor { get; set; }

        public DateTime? ActualServiceDate { get; set; }
        
        [StringLength(2512)]
        public string LocalContractorComments { get; set; }

        [StringLength(2512)]
        public string FMCComments { get; set; }

        [StringLength(2512)]
        public string ASOComments { get; set; }

        public DateTime? AssignDate { get; set; }
        public DateTime? LocalContractorRespondedDate { get; set; }

        public bool? ClaimFinalized { get; set; }

        public DateTime? ClaimFinalizedDate { get; set; }

        // comment later
        [ForeignKey("ServiceReportProStatus")]
        public int? ServiceReportStatusId { get; set; }
        public virtual ServiceReportProStatus ServiceReportProStatus { get; set; }

        [StringLength(2512)]
        public string ServiceNotes { get; set; }

        // UserId from user table
        public string PerformedByName { get; set; }

        public bool CloseOff { get; set; }

        [ForeignKey("Agency")]
        public int? AgencyID { get; set; }
        public virtual Agency Agency { get; set; }
        public int FinancialYearID { get; set; }
        public int PropertyID { get; set; }
        public int ElementID { get; set; }
        public int ComponentId { get; set; }
        public int ItemId { get; set; }
        public int ServiceTypeID { get; set; }

        public DateTime? ExpectedVisitDate { get; set; }
        public int? ServiceAssessmentID { get; set; }

        //public bool? AMUApproved { get; set; }
        // [StringLength(1000)]
        //public string AMUApprovalComments { get; set; }
        // [StringLength(128)]
        // public string AMUApprovedBy { get; set; }
        //public DateTime? AMUApprovedDate { get; set; }
        //public bool? ClaimReviewedByFMC { get; set; }
        public DateTime? SubContractorCommentDate { get; set; }
        public DateTime? FMCCommentDate { get; set; }
        public DateTime? ASOCommentDate { get; set; }
        //[Index("IX_SVCIKey", IsUnique = true)]
        //public Guid SVCIKey { get; set; }

        public virtual ICollection<ServiceReportProTradie> ServiceReportProTradies { get; set; }
    }

    [Table("ServiceReportProLineItem")]
    public partial class ServiceReportProLineItem
    {
        public int ServiceReportProLineItemID { get; set; }

        public int? AgencyID { get; set; }

        public int? ServiceReportProActiveID { get; set; }

        public int? WorkCategoryID { get; set; }
        [ForeignKey("ScheduledRate")]
        public int? ScheduleRateID { get; set; }

        public short? LineNumber { get; set; }

        public double CostAdjPercent { get; set; }

        public DateTime? WorkDate { get; set; }

        [StringLength(15)]
        public string MSRReference { get; set; }

        [StringLength(255)]
        public string SRLIDescription { get; set; }

        public double? Quantity { get; set; }

        [Column(TypeName = "money")]
        public decimal? Rate { get; set; }

        [StringLength(30)]
        public string Unit { get; set; }

        [Column(TypeName = "money")]
        public decimal? Amount { get; set; }

        [StringLength(50)]
        public string TradeName { get; set; }

        public decimal? HoursWorked { get; set; }

        [Column(TypeName = "money")]
        public decimal? ClaimedAmount { get; set; }

        [Column(TypeName = "money")]
        public decimal? ClaimedGST { get; set; }

        [Column(TypeName = "money")]
        public decimal? ApprovedAmount { get; set; }

        [Column(TypeName = "money")]
        public decimal? ApprovedGST { get; set; }

        public bool? HasPWAApproved { get; set; }

        [StringLength(250)]
        public string Reason { get; set; }

        public bool PWAApproved { get; set; }

        [StringLength(100)]
        public string ReportedBy { get; set; }

        public DateTime? ReportedDate { get; set; }

        public bool? LCIFlag { get; set; }

        [StringLength(15)]
        public string PWAApprovedBy { get; set; }

        public DateTime? PWAApprovedDate { get; set; }


        public Int16? APApprovedClaimMonth { get; set; }
        public Int16? APApprovedClaimYear { get; set; }
        public bool? Deleted { get; set; }
        public bool? PWAReClaimed { get; set; }
        public bool? PWARejected { get; set; }

        [StringLength(15)]
        public string PWARejectedBy { get; set; }
        public DateTime? PWARejectedDate { get; set; }


        public virtual Agency Agency { get; set; }

        public virtual WorkCategory WorkCategory { get; set; }
        public virtual ScheduledRate ScheduledRate { get; set; }
    }

    

    public partial class ServiceReportProStatus
    {
        [Key]
        public int ServiceReportStatusID { get; set; }

        [StringLength(50)]
        public string ServiceReportStatusDesc { get; set; }

        public int? SortOrder { get; set; }

        public bool? ShowInSRptList { get; set; }

        public bool? ShowInPWJList { get; set; }
        public virtual ICollection<ServiceReportProActive> ServiceReports { get; set; }

    }


}
