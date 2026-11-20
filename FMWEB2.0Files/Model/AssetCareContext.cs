using System;
using System.Data.Entity;
using System.Data.Entity.Core;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Web;
using AssetCare.Model.Migrations;
using AssetCare.Model.Models.Mapping;
using Microsoft.AspNet.Identity;

namespace AssetCare.Model.Models
{
    public partial class AssetCareContext : DbContext
    {
        public AssetCareContext()
            : base("Name=AssetCareContext")
        {
            base.Database.CommandTimeout = 300; //setting command time out in 5 Mins
            // This line will automatically update the database in LOCAL/TEST/PROD.
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<AssetCareContext, Configuration>());
            //Database.SetInitializer<AssetCareContext>(null);
        }

        public DbSet<AcquisitionMethod> AcquisitionMethods { get; set; }
        public DbSet<Agency> Agencies { get; set; }
        public DbSet<AgencyContractor> AgencyContractors { get; set; }
        public DbSet<AgencyType> AgencyTypes { get; set; }
        public DbSet<AsbestosStatus> AsbestosStatus { get; set; }
        public DbSet<Assembly> Assemblies { get; set; }
        public DbSet<Building> Buildings { get; set; }
        public DbSet<BuildingType> BuildingTypes { get; set; }
        public DbSet<Claim> Claims { get; set; }
        public DbSet<ClaimProactive> ClaimsProactive { get; set; }
        public DbSet<ClaimType> ClaimTypes { get; set; }
        public DbSet<Component> Components { get; set; }
        public DbSet<ConditionRating> ConditionRatings { get; set; }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<ContractStatus> ContractStatus { get; set; }
        public DbSet<CoreBusiness> CoreBusinesses { get; set; }
        public DbSet<CostCategory> CostCategories { get; set; }
        public DbSet<DemountableType> DemountableTypes { get; set; }
        public DbSet<District> Districts { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<EquipmentDocument> EquipmentDocuments { get; set; }
        public DbSet<EducationArea> EducationAreas { get; set; }
        public DbSet<EducationAreaRegion> EducationAreaRegions { get; set; }
        public DbSet<Electorate> Electorates { get; set; }
        public DbSet<Element> Elements { get; set; }
        public DbSet<Equipment> Equipments { get; set; }
        public DbSet<EquipmentType> EquipmentTypes { get; set; }
        public DbSet<Facility> Facilities { get; set; }
        public DbSet<FinancialYear> FinancialYears { get; set; }
        public DbSet<FloorType> FloorTypes { get; set; }
        public DbSet<IntensityUse> IntensityUses { get; set; }
        public DbSet<LocalGovernmentArea> LocalGovernmentAreas { get; set; }
        public DbSet<MakeModel> MakeModels { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderJobType> OrderJobTypes { get; set; }
        public DbSet<PlanType> PlanTypes { get; set; }
        public DbSet<PrimaryUse> PrimaryUses { get; set; }
        public DbSet<Program> Programs { get; set; }
        public DbSet<PlannedMaintenanceProgram> PlannedMaintenancePrograms { get; set; }
        public DbSet<PlannedMaintenanceJob> PlannedMaintenanceJobs { get; set; }
        public DbSet<PlannedMaintenanceJobBuilding> PlannedMaintenanceJobBuildings { get; set; }
        public DbSet<PlannedMaintenanceOtherProgram> PlannedMaintenanceOtherPrograms { get; set; }
        public DbSet<PlannedMaintenanceOtherJob> PlannedMaintenanceOtherJobs { get; set; }
        public DbSet<EUROtherJob> EUROtherJobs { get; set; }
        public DbSet<Property> Properties { get; set; }
        public DbSet<PropertyValuation> PropertyValuations { get; set; }
        public DbSet<Region> Regions { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<RoomFunction> RoomFunctions { get; set; }
        public DbSet<ScheduledRate> ScheduledRates { get; set; }
        public DbSet<CommerceManagementFee> CommerceManagementFees { get; set; }
        public DbSet<GstRate> GstRates { get; set; }
        public DbSet<RoomType> RoomTypes { get; set; }
        public DbSet<AreaType> AreaTypes { get; set; }
        public DbSet<Cause> Causes { get; set; }
        public DbSet<Effect> Effects { get; set; }
        public DbSet<Condition> Conditions { get; set; }
        public DbSet<RecommendedWork> RecommendedWorks { get; set; }
        public DbSet<Priority> Priorities { get; set; }
        public DbSet<StrategicDriver> StrategicDrivers { get; set; }
        public DbSet<SubElement> SubElements { get; set; }
        public DbSet<CostAdjustment> CostAdjustments { get; set; }
        public DbSet<ScheduledRateCategory> ScheduledRateCategories { get; set; }
        public DbSet<ScheduledRateHeading> ScheduledRateHeadings { get; set; }
        public DbSet<SchoolCategory> SchoolCategories { get; set; }
        public DbSet<SmokeDetector> SmokeDetectors { get; set; }
        public DbSet<State> States { get; set; }
        public DbSet<SubRegion> SubRegions { get; set; }
        public DbSet<sysdiagram> sysdiagrams { get; set; }
        public DbSet<Tenure> Tenures { get; set; }
        public DbSet<WorkCategory> WorkCategories { get; set; }
        public DbSet<WorkSubCategory> WorkSubCategories { get; set; }
        public DbSet<Zoning> Zonings { get; set; }

        public DbSet<Report> Reports { get; set; }
        public DbSet<ReportField> ReportFields { get; set; }
        public DbSet<ReportType> ReportTypes { get; set; }
        public DbSet<SavedReport> SavedReports { get; set; }

        public DbSet<TreeInspection> TreeInspections { get; set; }
        public DbSet<TreeInspectionDetail> TreeInspectionDetails { get; set; }
        public DbSet<TreeInspectionItem> TreeInspectionItems { get; set; }
        public DbSet<TreeDefect> TreeDefects { get; set; }
        public DbSet<TreeInspectionReport> TreeInspectionReports { get; set; }
        public DbSet<TreeInspectionReportType> TreeInspectionReportTypes { get; set; }

        public DbSet<DETOrder> DETOrders { get; set; }
        public DbSet<ContractVariation> ContractVariations { get; set; }
        public DbSet<ContractVariationType> ContractVariationTypes { get; set; }
        public DbSet<ContractVariationStatus> ContractVariationStatuses { get; set; }
        public DbSet<ServiceReportLineItem> ServiceReportLineItems { get; set; }
        public DbSet<ContractVariationLineItem> ContractVariationLineItems { get; set; }
        public DbSet<ProgramWorkJob> ProgramWorkJobs { get; set; }
        public DbSet<TMFJobLineItem> TMFJobLineItems { get; set; }
        public DbSet<PlannedMaintenanceOtherJobBuilding> PlannedMaintenanceOtherJobBuildings { get; set; }
        public DbSet<EUROtherJobBuilding> EUROtherJobBuildings { get; set; }
        public DbSet<OrderAdjustment> OrderAdjustments { get; set; }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<RolesRule> RolesRules { get; set; }
        public DbSet<UserAgency> UserAgencies { get; set; }
        public DbSet<UserAfterHours> UserAfterHours { get; set; }
        public DbSet<UserProperty> UserProperties { get; set; }
        public DbSet<VWUserAgencies> UserAgenciesVW { get; set; }
        public DbSet<VWUserProperties> UserPropertiesVW { get; set; }
        public DbSet<SchoolLimit> SchoolLimits { get; set; }
        public DbSet<ContractStatusForPMP> ContractStatusForPMPs { get; set; }

        public DbSet<RateYear> RateYears { get; set; }
        public DbSet<CommerceCVFee> CommerceCVFees { get; set; }
        public DbSet<CostAdjustmentType> CostAdjustmentTypes { get; set; }
        public DbSet<ServiceReportStatus> ServiceReportStatuses { get; set; }

        public DbSet<TMFJob> TMFJob { get; set; }
        public DbSet<SchoolIncident> SchoolIncident { get; set; }
        public DbSet<TMFStatus> TMFStatus { get; set; }
        //public DbSet<SchoolLimit> SchoolLimit { get; set; }
        public DbSet<SchoolIncidentLossStatus> SchoolIncidentLossStatus { get; set; }
        public DbSet<SchoolIncidentLoss> SchoolIncidentLoss { get; set; }
        public DbSet<SecurityEntryPoint> SecurityEntryPoint { get; set; }
        public DbSet<SecurityEntryMode> SecurityEntryMode { get; set; }
        public DbSet<CallLogStatus> CallLogStatus { get; set; }
        public DbSet<CallLogFinanceUrgency> CallLogFinanceUrgency { get; set; }
        public DbSet<ConstructionType> ConstructionType { get; set; }
        public DbSet<SchoolAlarmNotification> SchoolAlarmNotification { get; set; }
        public DbSet<SchoolIncidentCause> SchoolIncidentCause { get; set; }
        public DbSet<SchoolIncidentBuilding> SchoolIncidentBuilding { get; set; }
        public DbSet<SupportDocumentClass> SupportDocumentClass { get; set; }
        public DbSet<SupportDocumentType> SupportDocumentType { get; set; }
        public DbSet<SupportDocument> SupportDocument { get; set; }
        public DbSet<TMFJobBuilding> TMFJobBuilding { get; set; }
        public DbSet<NewEntitySeq> NewEntitySeq { get; set; }
        public DbSet<ContractStatusTMF> ContractStatusTMF { get; set; }
        public DbSet<SchoolIncidentPriority> SchoolIncidentPriorities { get; set; }
        public DbSet<CallerPosition> CallerPositions { get; set; }

        public DbSet<CallLogType> CallLogTypes { get; set; }
        public DbSet<CallLogFITS> CallLogFITS { get; set; }
        public DbSet<ServiceReportBuilding> ServiceReportBuildings { get; set; }
        public DbSet<ContractVariationBuilding> ContractVariationBuildings { get; set; }

        public DbSet<MaintenanceTask> MaintenanceTasks { get; set; }
        public DbSet<MaintenanceTasksCallLog> MaintenanceTasksCallLogs { get; set; }
        public DbSet<CallLogBuildingFITS> CallLogBuildingFITSs { get; set; }


        public DbSet<ServiceReport> ServiceReports { get; set; }

        public DbSet<ServiceReportQRCode> ServiceReportQRCodes { get; set; }
        public DbSet<CrystalReport> CrystalReports { get; set; }
        public DbSet<EquipmentTypeBSEMAssembly> AssemblyEquipmentTypes { get; set; }

        public DbSet<TMFClaim> TMFClaim { get; set; }

        public DbSet<MaintenanceSchedule> MaintenanceSchedules { get; set; }
        public DbSet<NonFMCEURJob> NonFMCEURJobs { get; set; }

        public DbSet<EquipServiceStatus> EquipServiceStatuses { get; set; }
        public DbSet<EquipServiceStatusLineItem> EquipServiceStatusLineItems { get; set; }
        public DbSet<ServiceReportProActive> ServiceReportProActives { get; set; }
        
        public DbSet<EquipService> EquipServices { get; set; }
        public DbSet<LCIReasons> LCIReasons { get; set; }

        public DbSet<ServiceReportAC> ServiceReportAC { get; set; }

        public DbSet<ServicePeriod> ServicePeriods { get; set; }
        public DbSet<ContractVariationReason> ContractVariationReasons { get; set; }
        public DbSet<AdviceAuthority> AdviceAuthorities { get; set; }
        public DbSet<AppSession> AppSessions { get; set; }
        public DbSet<QlikReportURL> QlikReportURLs { get; set; }

        public DbSet<Assessment> Assessments { get; set; }
        public DbSet<AssessmentSignOffGroup> AssessmentSignOffGroups { get; set; }

        public DbSet<VisitorManagementQuestion> VisitorManagementQuestions { get; set; }
        public DbSet<VisitorManagementRole> VisitorManagementRoles { get; set; }
        public DbSet<VisitorManagementUser> VisitorManagementUsers { get; set; }
        public DbSet<VisitorManagementVisitor> VisitorManagementVisitors { get; set; }
        public DbSet<VisitorManagementVisitorQuestion> VisitorManagementVisitorQuestions { get; set; }
        public DbSet<VisitorManagementPerson> VisitorManagementPersons { get; set; }
        public DbSet<Extract> Extract { get; set; }
        public DbSet<ExtractAudit> ExtractAudit { get; set; }
        public DbSet<Cleaner> Cleaner { get; set; }

        public DbSet<AsbestosItem> AsbestosItems { get; set; }
        public DbSet<AsbestosMaterial> AsbestosMaterials { get; set; }
        public DbSet<AsbestosProduct> AsbestosProducts { get; set; }
        public DbSet<AsbestosTestResult> AsbestosTestResults { get; set; }
        public DbSet<AsbestosHygienistUser> AsbestosHygienistUsers { get; set; }
        public DbSet<AsbestosComponent> AsbestosComponents { get; set; }
        public DbSet<EmailKeyword> EmailKeywords { get; set; }
        public DbSet<PSMWorkAssignmentStatus> PSMWorkAssignmentStatus { get; set; }
        public DbSet<PSMDefectCallLog> PSMDefectCallLog { get; set; }
        public DbSet<PSMDefectCallLogBuilding> PSMDefectCallLogBuilding { get; set; }
        public DbSet<SubContractor> SubContractor { get; set; }
        public DbSet<PropertySubContractor> PropertySubContractor { get; set; }
        public DbSet<Tradie> Tradie { get; set; }
        public DbSet<TradieServiceType> TradieServiceType { get; set; }
        public DbSet<TradieActivity> TradieActivities { get; set; }       
        public DbSet<SubContractorTradie> SubContractorTradie { get; set; }

        public DbSet<SubContractorAgency> SubContractorAgency { get; set; }
        public DbSet<ServiceReportTradie> ServiceReportTradie { get; set; }
        public DbSet<ServiceReportProTradie> ServiceReportProTradie { get; set; }
        public DbSet<Proposal> Proposal { get; set; }
        public DbSet<ProposalBuilding> ProposalBuildings { get; set; }
        public DbSet<ProposalEndorsementType> ProposalEndorsementType { get; set; }
        public DbSet<ProposalEndorsement> ProposalEndorsement { get; set; }

        public DbSet<ProposalProjectType> ProposalProjectType { get; set; }

        public DbSet<ProposalWorkNominated> ProposalWorkNominated { get; set; }
        public DbSet<ProposalProgram> ProposalProgram { get; set; }
        public DbSet<ProposalSubProgram> ProposalSubProgram { get; set; }

        public DbSet<ProposalPMMProfile> ProposalPMMProfile { get; set; }
        public DbSet<ProposalEntitlement> ProposalEntitlement { get; set; }
        public DbSet<ProposalStatus> ProposalStatus { get; set; }
        public DbSet<CallLogQuotations> CallLogQuotations { get; set; }

        public DbSet<ProposalSFC> ProposalSFC { get; set; }
        public DbSet<ProposalRoles> ProposalRoles { get; set; }
        public DbSet<BusinessType> BusinessType { get; set; }
        public DbSet<SubContractorBusinessType> SubContractorBusinessType { get; set; }

        public DbSet<Projects> Projects { get; set; }

        public DbSet<ServiceReportProLineItem> ServiceReportProLineItems { get; set; }
        public DbSet<ProServices> ProServices { get; set; }

        public DbSet<ProServiceElementMap> ProServiceElementMaps { get; set; }
        //public DbSet<CalloutServiceReportClaim> CalloutServiceReportClaims { get; set; }
        public DbSet<RegionalContact> RegionalContacts { get; set; }


        public DbSet<NewsBanner> NewsBanner { get; set; }

        public DbSet<NewsBannerContent> NewsBannerContent { get; set; }
        public DbSet<Alert> Alerts { get; set; }
        public DbSet<AssessorStatus> AssessorStatuses { get; set; }
        public DbSet<ReimbursementLineItem> ReimbursementLineItems { get; set; }
        public DbSet<ReimbursementStatus> ReimbursementStatuses { get; set; }
        public DbSet<IRCMClaimStatus> IRCMClaimStatuses { get; set; }
        public DbSet<WebAPIClient> WebAPIClient { get; set; }
        public DbSet<WebAPIClientReport> WebAPIClientReports { get; set; }
        public DbSet<WebAPIClientActivity> WebAPIClientActivity { get; set; }
        public DbSet<WebAPIClientRowVersion> WebAPIClientRowVersions { get; set; }
        public DbSet<WebAPIClientDBSchemaVersion> WebAPIClientDBSchemaVersions { get; set; }
        public DbSet<WebAPIAccess> WebAPIAccesses { get; set; }
        public DbSet<WebAPIClientAccess> WebAPIClientAccesses { get; set; }
        public DbSet<ReleaseNote> ReleaseNotes { get; set; }
        public DbSet<ClaimApprovalStatusFilter> ClaimApprovalStatusFilter { get; set; }
        public DbSet<APMPServiceSchedule> APMPServiceSchedules { get; set; }
        public DbSet<APMPServiceScheduleLineItem> APMPServiceScheduleLineItems { get; set; }
        public DbSet<ServiceSubContrator> ServiceSubContrators { get; set; }

        public DbSet<AgencySequenceIdCounters> AgencySequenceIdCounters { get; set; }

        public DbSet<Log> Logs { get; set; }

        public DbSet<Application> Applications { get; set; }
        public DbSet<UserApplication> UserApplications { get; set; }
        public DbSet<LocalContractorType> LocalContractorTypes { get; set; }

        public DbSet<SystemSetting> SystemSettings { get; set; }
        public DbSet<ReactiveSubCategory> ReactiveSubCategories { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Configurations.Add(new AcquisitionMethodMap());
            modelBuilder.Configurations.Add(new AgencyMap());
            modelBuilder.Configurations.Add(new AgencyContractorMap());
            modelBuilder.Configurations.Add(new AgencyTypeMap());
            modelBuilder.Configurations.Add(new AsbestosStatuMap());
            modelBuilder.Configurations.Add(new AssemblyMap());
            modelBuilder.Configurations.Add(new BuildingMap());
            modelBuilder.Configurations.Add(new BuildingTypeMap());
            modelBuilder.Configurations.Add(new ClaimMap());
            modelBuilder.Configurations.Add(new ClaimTypeMap());
            modelBuilder.Configurations.Add(new ComponentMap());
            modelBuilder.Configurations.Add(new CostAdjustmentMap());
            modelBuilder.Configurations.Add(new ConditionRatingMap());
            modelBuilder.Configurations.Add(new CommerceCVFeeMap());
            modelBuilder.Configurations.Add(new CommerceManagementFeeMap());
            modelBuilder.Configurations.Add(new ContractMap());
            modelBuilder.Configurations.Add(new ContractStatuMap());
            modelBuilder.Configurations.Add(new ContractStatusForPMPMap());
            modelBuilder.Configurations.Add(new CoreBusinessMap());
            modelBuilder.Configurations.Add(new CostCategoryMap());
            modelBuilder.Configurations.Add(new CostAdjustmentTypeMap());
            modelBuilder.Configurations.Add(new DemountableTypeMap());
            modelBuilder.Configurations.Add(new DistrictMap());
            modelBuilder.Configurations.Add(new DocumentMap());
            modelBuilder.Configurations.Add(new EquipmentDocumentMap());
            modelBuilder.Configurations.Add(new EducationAreaMap());
            modelBuilder.Configurations.Add(new EducationAreaRegionMap());
            modelBuilder.Configurations.Add(new ElectorateMap());
            modelBuilder.Configurations.Add(new ElementMap());
            modelBuilder.Configurations.Add(new EquipmentMap());
            modelBuilder.Configurations.Add(new EquipmentTypeMap());
            modelBuilder.Configurations.Add(new FacilityMap());
            modelBuilder.Configurations.Add(new FinancialYearMap());
            modelBuilder.Configurations.Add(new FloorTypeMap());
            modelBuilder.Configurations.Add(new GstRateMap());
            modelBuilder.Configurations.Add(new ServicePeriodMap());
            modelBuilder.Configurations.Add(new IntensityUseMap());
            modelBuilder.Configurations.Add(new LocalGovernmentAreaMap());
            modelBuilder.Configurations.Add(new MakeModelMap());
            modelBuilder.Configurations.Add(new MaintenanceScheduleMap());
            modelBuilder.Configurations.Add(new OrderMap());
            modelBuilder.Configurations.Add(new OrderJobTypeMap());
            modelBuilder.Configurations.Add(new PlanTypeMap());
            modelBuilder.Configurations.Add(new PrimaryUseMap());
            modelBuilder.Configurations.Add(new ProgramMap());
            modelBuilder.Configurations.Add(new ProServicesMap());
            modelBuilder.Configurations.Add(new PlannedMaintenanceProgramMap());
            modelBuilder.Configurations.Add(new PlannedMaintenanceJobMap());
            modelBuilder.Configurations.Add(new PlannedMaintenanceJobBuildingMap());
            modelBuilder.Configurations.Add(new PlannedMaintenanceOtherJobBuildingMap());
            modelBuilder.Configurations.Add(new PlannedMaintenanceOtherProgramMap());
            modelBuilder.Configurations.Add(new PlannedMaintenanceOtherJobMap());
            modelBuilder.Configurations.Add(new EUROtherJobMap());
            modelBuilder.Configurations.Add(new EUROtherJobBuildingMap());
            modelBuilder.Configurations.Add(new PropertyMap());
            modelBuilder.Configurations.Add(new PropertyValuationMap());
            modelBuilder.Configurations.Add(new RateYearMap());
            modelBuilder.Configurations.Add(new RegionMap());
            modelBuilder.Configurations.Add(new RegionalContactMap());
            modelBuilder.Configurations.Add(new RoomMap());
            modelBuilder.Configurations.Add(new RoomFunctionMap());
            modelBuilder.Configurations.Add(new ScheduledRateMap());
            modelBuilder.Configurations.Add(new ScheduledRateCategoryMap());
            modelBuilder.Configurations.Add(new ScheduledRateHeadingMap());
            modelBuilder.Configurations.Add(new SchoolCategoryMap());
            modelBuilder.Configurations.Add(new SmokeDetectorMap());
            modelBuilder.Configurations.Add(new StateMap());
            modelBuilder.Configurations.Add(new SubElementMap());
            modelBuilder.Configurations.Add(new SubRegionMap());
            modelBuilder.Configurations.Add(new sysdiagramMap());
            modelBuilder.Configurations.Add(new TenureMap());
            modelBuilder.Configurations.Add(new UserAfterHoursMap());
            modelBuilder.Configurations.Add(new WorkCategoryMap());
            modelBuilder.Configurations.Add(new WorkSubCategoryMap());
            modelBuilder.Configurations.Add(new ZoningMap());

            modelBuilder.Configurations.Add(new EquipServiceStatusMap());
            modelBuilder.Configurations.Add(new EquipServiceStatusLineItemMap());
            modelBuilder.Configurations.Add(new EquipServiceMap());
            modelBuilder.Configurations.Add(new AssessmentMap());
            modelBuilder.Configurations.Add(new AssessmentSignOffGroupMap());
            modelBuilder.Configurations.Add(new AdviceAuthorityMap());
            modelBuilder.Configurations.Add(new AppSessionMap());

            modelBuilder.Configurations.Add(new RoomTypeMap());
            modelBuilder.Configurations.Add(new AreaTypeMap());
            modelBuilder.Configurations.Add(new CauseMap());
            modelBuilder.Configurations.Add(new EffectMap());
            modelBuilder.Configurations.Add(new ConditionMap());
            modelBuilder.Configurations.Add(new RecommendedWorkMap());
            modelBuilder.Configurations.Add(new PriorityMap());
            modelBuilder.Configurations.Add(new StrategicDriverMap());
            modelBuilder.Configurations.Add(new ReleaseNoteMap());

            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            modelBuilder.Conventions.Add(new FormatFunctionConvention());
            modelBuilder.Conventions.Add(new CalcTMFJobSystemStatusFunctionConvention());
            modelBuilder.Conventions.Add(new GetJobBuildingsFunctionConvention());
            modelBuilder.Conventions.Add(new GetTradieServiceTypesFunctionConvention());
            //modelBuilder.Conventions.Add(new FunctionsConvention("dbo", this.GetType()));

            modelBuilder.Entity<AgencySequenceIdCounters>()
            .HasKey(x => new { x.AgencyId, x.SequenceType });
        }
        public override int SaveChanges()
        {
            try
            {
                var entities = ChangeTracker.Entries();

                foreach (var entity in entities)
                {
                    if (entity is IBaseModel)
                    {
                        if (entity.State == EntityState.Added)
                        {
                            ((BaseModel)entity.Entity).Active = true;
                            ((BaseModel)entity.Entity).CreatedDate = DateTime.Now;
                            //((BaseModel)entity.Entity).CreatedBy = currentUsername;       // TODO: Bring in username via DI and populate. Until then, do it in service area
                        }
                        else if (entity.State == EntityState.Modified)
                        {
                            ((BaseModel)entity.Entity).UpdatedDate = DateTime.Now;
                            //((BaseModel)entity.Entity).UpdatedBy = currentUsername;           // TODO: Bring in username via DI and populate. Until then, do it in service area
                        }
                    }
                }

                var entities2 = ChangeTracker.Entries();

                foreach (var entity in entities2)
                {
                    if (entity.Entity is IHasAuditInfo)
                    {
                        if (entity.State == EntityState.Added)
                        {
                            string userName = HttpContext.Current.User.Identity.IsAuthenticated ?
                                ((System.Security.Claims.ClaimsIdentity)(HttpContext.Current.User.Identity)).FindFirstValue("preferred_username") :
                                string.Empty;
                            ((IHasAuditInfo)entity.Entity).RecordedDate = DateTime.Now;
                            ((IHasAuditInfo)entity.Entity).RecordedBy = (userName == null)? ((System.Security.Claims.ClaimsIdentity)(HttpContext.Current.User.Identity)).FindFirstValue("client_id") : userName;
                            ((IHasAuditInfo)entity.Entity).LastRecordedDate = DateTime.Now;
                            ((IHasAuditInfo)entity.Entity).LastRecordedBy = userName;
                        }
                        else if (entity.State == EntityState.Modified)
                        {
                            string userName = HttpContext.Current.User.Identity.IsAuthenticated ?
                                ((System.Security.Claims.ClaimsIdentity)(HttpContext.Current.User.Identity)).FindFirstValue("preferred_username") :
                                string.Empty;
                            ((IHasAuditInfo)entity.Entity).LastRecordedDate = DateTime.Now;
                            ((IHasAuditInfo)entity.Entity).LastRecordedBy = userName;
                        }
                    }

                }
                return base.SaveChanges();
            }
            catch (DbUpdateException ex )
            {
                throw ex;
            }
        }

        [DbFunction("CodeFirstDatabaseSchema", "CalcTMFJobSystemStatus")]
        public static string CalcTMFJobSystemStatus(int TMFJobID)
        {
            // no need to provide an implementation
            throw new NotSupportedException();
        }

        [DbFunction("CodeFirstDatabaseSchema", "GetJobBuildings")]
        public static string GetJobBuildings(int jobID)
        {
            // no need to provide an implementation
            throw new NotSupportedException();
        }

        [DbFunction("CodeFirstDatabaseSchema", "GetTradieServiceTypes")]
        public static string GetTradieServiceTypes(int TradieID)
        {
            // no need to provide an implementation
            throw new NotSupportedException();
        }
    }
}
