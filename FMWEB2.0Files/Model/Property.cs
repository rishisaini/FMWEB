using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssetCare.Model.Models
{
    public partial class Property
    {
        public Property()
        {
            this.Equipments = new List<Equipment>();
            this.PropertyValuations = new List<PropertyValuation>();
            this.PlannedMaintenanceJobs = new List<PlannedMaintenanceJob>();
            this.PlannedMaintenanceOtherJobs = new List<PlannedMaintenanceOtherJob>();
            this.EUROtherJobs = new List<EUROtherJob>();
        }

        // This is an auto-incrementing PK
        public int PropertyId { get; set; }
        // This is what user enters in. ie. Generated from AMS. 
        public int? SchoolId { get; set; }
        public int AgencyId { get; set; }
        public string PropertyShortDesc { get; set; }
        public string PropertyDesc { get; set; }
        public Nullable<bool> RemovedYN { get; set; }
        public Nullable<int> RegionID { get; set; }
        public Nullable<int> FacilityID { get; set; }
        public Nullable<int> EducationAreaId { get; set; }
        public Nullable<int> DistrictId { get; set; }
        public string SchoolCategoryId { get; set; }
        public Nullable<decimal> Limit { get; set; }
        public string Email { get; set; }
        public Nullable<double> BuildingsSqMtr { get; set; }
        public string Street { get; set; }
        public string Suburb { get; set; }
        public Nullable<short> Postcode { get; set; }
        public string State { get; set; }
        public string Phone { get; set; }
        public string Fax { get; set; }
        public string ContactName { get; set; }
        public string ContactTitle { get; set; }
        public string Comments { get; set; }
        public Nullable<bool> Heritage { get; set; }
        public string Address { get; set; }
        public string CAMSYSID { get; set; }
        public string CAMSYSREGIONID { get; set; }
        public string CAMSYSFACILITYID { get; set; }
        public Nullable<bool> InContract { get; set; }
        public Nullable<bool> SealedYN { get; set; }
        public Nullable<int> AgencyIdOld { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<int> UpdatedBy { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }
        public Nullable<bool> Active { get; set; }
        public Nullable<int> TenureID { get; set; }
        public string HeritageStatus { get; set; }
        public string Parish { get; set; }
        public string County { get; set; }
        public string OperatedBy { get; set; }
        public string OperatedByContact { get; set; }
        public Nullable<int> SubRegionID { get; set; }
        public string FileNumber { get; set; }
        public string CoreBusinessID { get; set; }
        public Nullable<short> LocalGovernmentAreaID { get; set; }
        public Nullable<int> AsbestosStatusID { get; set; }
        public Nullable<int> SmokeDetectorID { get; set; }
        public string AssetCondition { get; set; }
        public string ZoneID { get; set; }
        public Nullable<int> ElectorateID { get; set; }
        [ForeignKey("PlanTypeFk")]
        public string PlanTypeID { get; set; }
        public Nullable<decimal> Area { get; set; }
        public string LotNo { get; set; }
        public string SectionNo { get; set; }
        public string PlanType { get; set; }
        public string ProjectManagerEmail { get; set; }
        public string ProjectDirectorEmail { get; set; }
        public Nullable<int> PlanNo { get; set; }
        public Nullable<int> PlaceUniqueNo { get; set; }
        public Nullable<bool> Disposed { get; set; }
        public Nullable<System.DateTime> DisposalDate { get; set; }
        public Nullable<System.DateTime> DisposalExchangeDate { get; set; }
        public Nullable<System.DateTime> DisposalSettlementDate { get; set; }
        public Nullable<System.DateTime> DisposalExecCouncilAppDate { get; set; }
        public string DisposalMethodID { get; set; }
        public Nullable<decimal> DisposalSalePrice { get; set; }
        public string DisposalComments { get; set; }
        public string TRIMLink { get; set; }
        public bool? NominatedYN { get; set; }

        [StringLength(100)]
        public string ASOName { get; set; }
        [StringLength(20)]
        public string ASOContactNumber { get; set; }
        [StringLength(320)]
        public string ASOEmail { get; set; }
        [StringLength(100)]
        public string GLName { get; set; }
        [StringLength(20)]
        public string GLContactNumber { get; set; }
        public bool? AsbestosHR { get; set; }
        [StringLength(100)]
        public string SGLName { get; set; }
        [StringLength(20)]
        public string SGLContactNumber { get; set; }
        [StringLength(100)]
        public string DAMName { get; set; }
        [StringLength(20)]
        public string DAMContactNumber { get; set; }
        [StringLength(100)]
        public string EDAMName { get; set; }
        [StringLength(20)]
        public string EDAMContactNumber { get; set; }
        [StringLength(150)]
        public string GLEmail { get; set; }
        [StringLength(150)]
        public string SGLEmail { get; set; }
        [StringLength(150)]
        public string DAMEmail { get; set; }
        [StringLength(150)]
        public string EDAMEmail { get; set; }
        [StringLength(100)]
        public string DELName { get; set; }
        [StringLength(20)]
        public string DELContactNumber { get; set; }
        [StringLength(150)]
        public string DELEmail { get; set; }
        [StringLength(100)]
        public string EDName { get; set; }
        [StringLength(20)]
        public string EDContactNumber { get; set; }
        [StringLength(150)]
        public string EDEmail { get; set; }
        public string LrlvCostCentre { get; set; }

        public Nullable<System.DateTime> DisposalMinisterialAppDate { get; set; }
        public virtual Agency Agency { get; set; }
        public virtual ICollection<Building> Buildings { get; set; }
        public virtual AsbestosStatus AsbestosStatu { get; set; }
        public virtual District District { get; set; }
        public virtual EducationArea EducationArea { get; set; }
        public virtual Electorate Electorate { get; set; }
        public virtual ICollection<Equipment> Equipments { get; set; }
        public virtual Facility Facility { get; set; }
        public virtual LocalGovernmentArea LocalGovernmentArea { get; set; }
        public virtual SubRegion SubRegion { get; set; }
        public virtual Region Region { get; set; }
        public virtual CoreBusiness CoreBusiness { get; set; }
        public virtual SmokeDetector SmokeDetector { get; set; }
        public virtual Tenure Tenure { get; set; }
        public virtual Zoning Zoning { get; set; }
        public virtual ICollection<PropertyValuation> PropertyValuations { get; set; }
        public virtual SchoolCategory SchoolCategory { get; set; }
        public virtual ICollection<PlannedMaintenanceJob> PlannedMaintenanceJobs { get; set; }
        public virtual ICollection<PlannedMaintenanceOtherJob> PlannedMaintenanceOtherJobs { get; set; }
        public virtual ICollection<EUROtherJob> EUROtherJobs { get; set; }
        public virtual ICollection<UserProperty> UserProperties { get; set; }
        public virtual ICollection<ContractVariation> ContractVariations { get; set; }

        public Nullable<int> DocumentId { get; set; }

        public Nullable<long> AmsDataLoaderBatchId { get; set; }
        public string AmsDataLoaderActionCode { get; set; }
        public virtual Document Document { get; set; }
        public virtual ICollection<CallLogFITS> CallLogFITS { get; set; }
        //public virtual ICollection<Claim> Claims { get; set; }
        public virtual PlanType PlanTypeFk { get; set; }

        public bool? DefectMgmt { get; set; }

        public bool? IsArchieved { get; set; }
        public virtual ICollection<TradieActivity> TradieActivities { get; set; }
    }
}
