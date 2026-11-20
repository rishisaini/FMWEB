using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AssetCare.Model.Models;
using AssetCare.Repository;


namespace AssetCare.Repository.Interface
{
    public interface IUnitOfWork : IDisposable
    {
        GenericRepository<Agency> AgencyRepository { get; }
        GenericRepository<Property> PropertyRepository { get; }
        GenericRepository<PropertyValuation> PropertyValuationRepository { get; }
        GenericRepository<Program> ProgramRepository { get; }
        GenericRepository<FinancialYear> FinancialYearRepository { get; }
        GenericRepository<Electorate> ElectorateRepository { get; }
        GenericRepository<LocalGovernmentArea> LocalGovernmentAreaRepository { get; }
        GenericRepository<Zoning> ZoningRepository { get; }
        GenericRepository<SmokeDetector> SmokeDetectorRepository { get; }
        GenericRepository<AsbestosStatus> AsbestosStatusRepository { get; }
        GenericRepository<State> StateRepository { get; }
        GenericRepository<Facility> FacilityRepository { get; }
        GenericRepository<Region> RegionRepository { get; }
        GenericRepository<SubRegion> SubRegionRepository { get; }
        GenericRepository<CoreBusiness> CoreBusinessRepository { get; }
        GenericRepository<Tenure> TenureRepository { get; }
        GenericRepository<AcquisitionMethod> AcquisitionMethodRepository { get; }
        GenericRepository<PlanType> PlanTypeRepository { get; }
        GenericRepository<ConditionRating> ConditionRatingRepository { get; }
        GenericRepository<PrimaryUse> PrimaryUseRepository { get; }
        GenericRepository<BuildingType> BuildingTypeRepository { get; }
        GenericRepository<RoomFunction> RoomFunctionRepository { get; }
        GenericRepository<Building> BuildingRepository { get; }
        GenericRepository<Room> RoomRepository { get; }

        GenericRepository<ScheduledRate> ScheduledRateRepository { get; }
        GenericRepository<CommerceManagementFee> CommerceManagementFeeRepository { get; }
        GenericRepository<Claim> ClaimRepository { get; }
        GenericRepository<ClaimType> ClaimTypeRepository { get; }
        GenericRepository<IntensityUse> IntensityUseRepository { get; }
        GenericRepository<Document> DocumentRepository { get; }
        GenericRepository<EquipmentDocument> EquipmentDocumentRepository { get; }
        GenericRepository<Equipment> EquipmentRepository { get; }
        GenericRepository<EquipmentType> EquipmentTypeRepository { get; }
        GenericRepository<Element> ElementRepository { get; }
        GenericRepository<SubElement> SubElementRepository { get; }
        GenericRepository<Component> ComponentRepository { get; }
        GenericRepository<MakeModel> MakeModelRepository { get; }
        GenericRepository<FloorType> FloorTypeRepository { get; }
        GenericRepository<Report> ReportRepository { get; }
        GenericRepository<ReportField> ReportFieldRepository { get; }
        GenericRepository<ReportType> ReportTypeRepository { get; }
        GenericRepository<SavedReport> SavedReportRepository { get; }
        GenericRepository<User> UserRepository { get; }
        GenericRepository<UserProperty> UserPropertyRepository { get; }
        GenericRepository<Role> RoleRepository { get; }
        GenericRepository<RolesRule> RolesRuleRepository { get; }
        GenericRepository<PlannedMaintenanceProgram> PlannedMaintenanceProgramRepository { get; }
        GenericRepository<PlannedMaintenanceJob> PlannedMaintenanceJobRepository { get; }
        GenericRepository<PlannedMaintenanceOtherProgram> PlannedMaintenanceOtherProgramRepository { get; }
        GenericRepository<PlannedMaintenanceOtherJob> PlannedMaintenanceOtherJobRepository { get; }
        GenericRepository<PlannedMaintenanceJobStatus> PlannedMaintenanceJobStatusRepository { get; }
        GenericRepository<PlannedMaintenanceOtherJobStatus> PlannedMaintenanceOtherJobStatusRepository { get; }
        GenericRepository<EUROtherJob> EUROtherJobRepository { get; }
        GenericRepository<Model.Models.Order> OrderRepository { get; }
        GenericRepository<OrderJobType> OrderJobTypeRepository { get; }
        GenericRepository<ContractStatus> ContractStatusRepository { get; }
        GenericRepository<PlannedMaintenanceJobBuilding> PlannedMaintenanceJobBuildingRepository { get; }
        GenericRepository<CommerceCVFee> CommerceCVFeeRepository { get; }
        GenericRepository<PlannedMaintenanceOtherJobBuilding> PlannedMaintenanceOtherJobBuildingRepository { get; }
        GenericRepository<EUROtherJobBuilding> EUROtherJobBuildingRepository { get; }
        GenericRepository<EUROtherJobStatus> EUROtherJobStatusRepository { get; }
        GenericRepository<EUROtherJobUrgency> EUROtherJobUrgencyRepository { get; }
        GenericRepository<ScheduledRateCategory> ScheduledRateCategoryRepository { get; }
        GenericRepository<ScheduledRateHeading> ScheduledRateHeadingRepository { get; }
        GenericRepository<WorkCategory> WorkCategoryRepository { get; }
        GenericRepository<CostCategory> CostCategoryRepository { get; }
        GenericRepository<WorkSubCategory> WorkSubCategoryRepository { get; }
        GenericRepository<ContractStatusForPMP> ContractStatusForPMPRepository { get; }
        GenericRepository<OrderAdjustment> OrderAdjustmentRepository { get; }
        GenericRepository<RateYear> RateYearRepository { get; }
        GenericRepository<CostAdjustmentType> CostAdjustmentTypeRepository { get; }
        GenericRepository<GstRate> GstRateRepository { get; }
        GenericRepository<ServicePeriod> ServicePeriodRepository { get; }
        GenericRepository<AdviceAuthority> AdviceAuthorityRepository { get; }
        GenericRepository<CostAdjustment> CostAdjustmentRepository { get; }
        GenericRepository<NewEntitySeq> NewEntitySeqRepository { get; }
        GenericRepository<CrystalReport> CrystalReportRepository { get; }

        GenericRepository<CallLogFinanceUrgency> CallLogFinanceUrgencyRepository { get; }
        GenericRepository<CallLogType> CallLogTypeRepository { get; }
        GenericRepository<CallLogStatus> CallLogStatusRepository { get; }
        GenericRepository<ProposalStatus> ProposalStatusRepository { get; }
        GenericRepository<SupportDocumentType> SupportDocumentTypeRepository { get; }
        GenericRepository<SupportDocumentClass> SupportDocumentClassRepository { get; }
        GenericRepository<EquipmentTypeBSEMAssembly> AssemblyEquipmentTypesRepository { get; }
        GenericRepository<MaintenanceTasksCallLog> MaintenanceTasksCallLogRepository { get; }
        GenericRepository<MaintenanceTask> MaintenanceTasksRepository { get; }
        GenericRepository<SchoolLimit> SchoolLimitRepository { get; }


        GenericRepository<MaintenanceSchedule> MaintenanceScheduleRepository { get; }
        GenericRepository<ServiceReportStatus> ServiceReportStatusRepository { get; }
        GenericRepository<ServiceReportProStatus> ServiceReportProStatusRepository { get; }
        GenericRepository<ServiceReportJobCategory> ServiceReportJobCategoryRepository { get; }
        GenericRepository<ServiceReportApprovalType> ServiceReportApprovalTypesRepository { get; }


        GenericRepository<EquipServiceStatus> EquipServiceStatusRepository { get; }
        GenericRepository<EquipServiceStatusLineItem> EquipServiceStatusLineItemRepository { get; }
        GenericRepository<EquipService> EquipServiceRepository { get; }
        GenericRepository<Assessment> AssessmentRepository { get; }
        GenericRepository<AssessmentSignOffGroup> AssessmentSignOffGroupRepository { get; }


        GenericRepository<UserAgency> UserAgencyRepository { get; }
        GenericRepository<ContractVariationStatus> ContractVariationStatusRepository { get; }
        //GenericRepository<QlikReportURL> QlikReportURLRepository { get; }


        GenericRepository<RoomType> RoomTypeRepository { get; }
        GenericRepository<AreaType> AreaTypeRepository { get; }

        GenericRepository<Cause> CauseRepository { get; }
        GenericRepository<Effect> EffectRepository { get; }

        GenericRepository<Condition> ConditionRepository { get; }
        GenericRepository<RecommendedWork> RecommendedWorkRepository { get; }
        GenericRepository<Priority> PriorityRepository { get; }
        GenericRepository<StrategicDriver> StrategicDriverRepository { get; }

        GenericRepository<CallLogFITS> CallLogFitsRepository { get; }
        GenericRepository<TimeExtension> TimeExtensionRepository { get; }
        GenericRepository<WorkType> WorkTypeRepository { get; }
        GenericRepository<EmailKeyword> EmailKeywordRepository { get; }
        GenericRepository<PSMWorkAssignmentStatus> PSMWorkAssignmentRepository { get; }
        GenericRepository<SubContractor> SubContractorRepository { get; }
        GenericRepository<SubContractorAgency> SubContractorAgencyRepository { get; }
        GenericRepository<ProposalProjectType> ProjectTypeRepository { get; }

        GenericRepository<ProposalWorkNominated> WorkNominatedRepository { get; }
        GenericRepository<RegionalContact> RegionalContactRepository { get; }

        GenericRepository<ProposalProgram> ProposalProgramRepository { get; }
        GenericRepository<ProjectStatus> ProjectStatusRepository { get; }
       // GenericRepository<ProposalProgram> ProjectProgramRepository { get; }


        GenericRepository<ProposalSubProgram> ProposalSubProgramRepository { get; }

        GenericRepository<ProposalPMMProfile> PMMProfileRepository { get; }
        GenericRepository<ProposalSFC> ProposalSFCRepository { get; }
        GenericRepository<ProposalProcurement> ProposalProcurementRepository { get; }
        GenericRepository<ProposalEndorsementType> EndorsementTypeRepository { get; }

        GenericRepository<BusinessType> BusinessTypeRepository { get; }
        GenericRepository<Tradie> TradieRepository { get; }
        GenericRepository<TradieServiceType> TradieServiceTypeRepository { get; }
        GenericRepository<SubContractorBusinessType> SubContractorBusinessTypeRepository { get; }
        GenericRepository<ProServices> ProServicesRepository { get; }

        GenericRepository<PropertySubContractor> PropertySubContractorRepository { get; }

        GenericRepository<ProServiceElementMap> ProServiceElementMapRepository { get; }
        GenericRepository<WebAPIClient> WebAPIClientRepository { get; }
        GenericRepository<WebAPIClientReport> WebAPIClientReportRepository { get; }
        GenericRepository<WebAPIClientActivity> WebAPIClientActivityRepository { get; }
        GenericRepository<WebAPIClientRowVersion> WebAPIClientRowVersionRepository { get; }
        GenericRepository<APMPServiceSchedule> APMPServiceScheduleRepository { get; }
        GenericRepository<UserAfterHours> UserAfterHoursRepository { get; }
        GenericRepository<WebAPIAccess> WebAPIAccessRepository { get; }
        GenericRepository<WebAPIClientAccess> WebAPIClientAccessRepository { get; }
        AssetCareContext CurrentContext { get; }
        int Save();
        new void Dispose();

        DbContextTransaction BeginSnapshotTransaction();

        GenericRepository<Application> ApplicationRepository { get; }
        GenericRepository<UserApplication> UserApplicationRepository { get; }

        GenericRepository<LocalContractorType> LocalContractorTypeRepository { get; }
    }
}
