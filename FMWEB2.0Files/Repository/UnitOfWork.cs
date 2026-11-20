using System;
using System.Data.Entity;
using AssetCare.Model.Models;
using AssetCare.Repository.Interface;


namespace AssetCare.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private AssetCareContext context = new AssetCareContext();
        private GenericRepository<Agency> agencyRepository;
        private GenericRepository<Property> propertyRepository;
        private GenericRepository<PropertyValuation> propertyValuationRepository;
        private GenericRepository<Program> programRepository;
        private GenericRepository<FinancialYear> financialYearRepository;
        private GenericRepository<Electorate> electorateRepository;
        private GenericRepository<LocalGovernmentArea> localGovernmentAreaRepository;
        private GenericRepository<Zoning> zoningRepository;
        private GenericRepository<SmokeDetector> smokeDetectorRepository;
        private GenericRepository<AsbestosStatus> asbestosStatusRepository;
        private GenericRepository<State> stateRepository;
        private GenericRepository<Facility> facilityRepository;
        private GenericRepository<Region> regionRepository;
        private GenericRepository<SubRegion> subRegionRepository;
        private GenericRepository<CoreBusiness> coreBusinessRepository;
        private GenericRepository<Tenure> tenureRepository;
        private GenericRepository<AcquisitionMethod> acquisitionMethodRepository;
        private GenericRepository<PlanType> planTypeRepository;
        private GenericRepository<ConditionRating> conditionRatingRepository;
        private GenericRepository<PrimaryUse> primaryUseRepository;
        private GenericRepository<BuildingType> buildingTypeRepository;
        private GenericRepository<RoomFunction> roomFunctionRepository;
        private GenericRepository<Building> buildingRepository;
        private GenericRepository<Room> roomRepository;
        private GenericRepository<IntensityUse> intensityUseRepository;
        private GenericRepository<ScheduledRate> scheduledRateRepository;
        private GenericRepository<CommerceManagementFee> commerceManagementFeeRepository;
        private GenericRepository<ScheduledRateCategory> scheduledRateCategoryRepository;
        private GenericRepository<ScheduledRateHeading> scheduledRateHeadingRepository;
        private GenericRepository<Claim> claimRepository;
        private GenericRepository<Equipment> equipmentRepository;
        private GenericRepository<EquipmentType> equipmentTypeRepository;
        private GenericRepository<Element> elementRepository;
        private GenericRepository<SubElement> subElementRepository;
        private GenericRepository<Component> componentRepository;
        private GenericRepository<MakeModel> makeModelRepository;
        private GenericRepository<FloorType> floorTypeRepository;
        private GenericRepository<Report> reportRepository;
        private GenericRepository<ReportField> reportFieldRepository;
        private GenericRepository<ReportType> reportTypeRepository;
        private GenericRepository<SavedReport> savedReportRepository;
        private GenericRepository<User> userRepository;
        private GenericRepository<Role> roleRepository;
        private GenericRepository<RolesRule> rolesRuleRepository;
        private GenericRepository<PlannedMaintenanceProgram> plannedMaintenanceProgramRepository;
        private GenericRepository<PlannedMaintenanceJob> plannedMaintenanceJobRepository;
        private GenericRepository<PlannedMaintenanceOtherProgram> plannedMaintenanceOtherProgramRepository;
        private GenericRepository<PlannedMaintenanceOtherJob> plannedMaintenanceOtherJobRepository;
        private GenericRepository<PlannedMaintenanceJobStatus> plannedMaintenanceJobStatusRepository;
        private GenericRepository<PlannedMaintenanceOtherJobStatus> plannedMaintenanceOtherJobStatusRepository;
        private GenericRepository<Model.Models.Order> orderRepository;
        private GenericRepository<OrderJobType> orderJobTypeRepository;
        private GenericRepository<EUROtherJob> eurOtherJobRepository;
        private GenericRepository<ClaimType> claimTypeRepository;
        private GenericRepository<ContractStatus> contractStatusRepository;
        private GenericRepository<PlannedMaintenanceJobBuilding> plannedMaintenanceJobBuildingRepository;
        private GenericRepository<CommerceCVFee> commerceCVFeeRepository;
        private GenericRepository<PlannedMaintenanceOtherJobBuilding> plannedMaintenanceOtherJobBuildingRepository;
        private GenericRepository<EUROtherJobBuilding> eurOtherJobBuildingRepository;
        private GenericRepository<EUROtherJobStatus> eurOtherJobStatusRepository;
        private GenericRepository<EUROtherJobUrgency> eurOtherJobUrgencyRepository;
        private GenericRepository<WorkCategory> workCategoryRepository;
        private GenericRepository<OrderAdjustment> orderAdjustmentRepository;
        private GenericRepository<ContractStatusForPMP> contractStatusForPMPRepository;
        private GenericRepository<CostCategory> costCategoryRepository;
        private GenericRepository<WorkSubCategory> workSubCategoryRepository;
        private GenericRepository<RateYear> rateYearRepository;
        private GenericRepository<CostAdjustmentType> costAdjustmentTypeRepository;
        private GenericRepository<GstRate> gstRateRepository;
        private GenericRepository<ServicePeriod> servicePeriodRepository;
        private GenericRepository<AdviceAuthority> adviceAuthorityRepository;
        private GenericRepository<CostAdjustment> costAdjustmentRepository;
        private GenericRepository<UserProperty> userPropertyRepository;
        private GenericRepository<NewEntitySeq> newEntitySeqRepository;
        private GenericRepository<CrystalReport> crystalReportRepository;
        private GenericRepository<MaintenanceSchedule> maintenanceScheduleRepository;

        private GenericRepository<CallLogType> callLogTypeRepository;
        private GenericRepository<CallLogStatus> callLogStatusRepository;
        private GenericRepository<ProposalStatus> proposalStatusRepository;
        private GenericRepository<SupportDocumentType> supportDocumentTypeRepository;
        private GenericRepository<SupportDocumentClass> supportDocumentClassRepository;
        private GenericRepository<EquipmentTypeBSEMAssembly> assemblyEquipmentTypes;
        private GenericRepository<MaintenanceTasksCallLog> maintenanceTaskCallLog;
        private GenericRepository<MaintenanceTask> maintenanceTasks;
        private GenericRepository<SchoolLimit> schoolLimitRepository;
        private GenericRepository<CallLogFinanceUrgency> callLogFinanceUrgencyRepository;
        private GenericRepository<ServiceReportStatus> serviceReportStatusRepository;
        private GenericRepository<ServiceReportProStatus> serviceReportProStatusRepository;
        private GenericRepository<ServiceReportJobCategory> serviceReportJobCategoryRepository;
        private GenericRepository<UserAgency> userAgencyRepository;
        private GenericRepository<ServiceReportApprovalType> serviceReportApprovalTypesRepository;

        private GenericRepository<EquipServiceStatus> equipServiceStatusRepository;
        private GenericRepository<Assessment> assessmentRepository;
        private GenericRepository<AssessmentSignOffGroup> assessmentSignOffGroupRepository;

        private GenericRepository<EquipServiceStatusLineItem> equipServiceStatusLineItemRepository;
        private GenericRepository<EquipService> equipServiceRepository;
        private GenericRepository<ContractVariationStatus> contractVariationStatusRepository;
        //private GenericRepository<QlikReportURL> qlikReportURLRepository;

        private GenericRepository<RoomType> roomTypeRepository;
        private GenericRepository<AreaType> areaTypeRepository;
        private GenericRepository<Cause> causeRepository;
        private GenericRepository<Effect> effectRepository;
        private GenericRepository<Condition> conditionRepository;
        private GenericRepository<RecommendedWork> recommendedWorkRepository;
        private GenericRepository<Priority> priorityRepository;
        private GenericRepository<StrategicDriver> strategicDriverRepository;
        private GenericRepository<CallLogFITS> callLogFitsRepository;
        private GenericRepository<TimeExtension> timeExtensionRepository;
        private GenericRepository<WorkType> workTypeRepository;
        private GenericRepository<EquipmentDocument> equipmentDocumentRepository;
        private GenericRepository<Cleaner> cleanerRepository;
        private GenericRepository<EmailKeyword> emailKeywordRepository;
        private GenericRepository<PSMWorkAssignmentStatus> psmWorkAssignmentRepository;
        private GenericRepository<SubContractor> subContractorRepository;
        private GenericRepository<SubContractorAgency> subContractorAgencyRepository;
        private GenericRepository<ProposalProjectType> projectTypeRepository;
        private GenericRepository<ProposalWorkNominated> workNominatedRepository;
        private GenericRepository<RegionalContact> regionalContactRepository;
        private GenericRepository<ProposalProgram> proposalProgramRepository;
        private GenericRepository<ProjectStatus> projectStatusRepository;
        private GenericRepository<ProposalSubProgram> proposalSubProgramRepository;
        private GenericRepository<ProposalPMMProfile> pmmProfileRepository;
        private GenericRepository<ProposalSFC> proposalSFCRepository;
        private GenericRepository<ProposalProcurement> proposalProcurementRepository;

        private GenericRepository<ProposalEndorsementType> endorsementTypeRepository;
        private GenericRepository<BusinessType> businessTypeRepository;
        private GenericRepository<Tradie> tradieRepository;
        private GenericRepository<TradieServiceType> tradieServiceTypeRepository;
        private GenericRepository<SubContractorBusinessType> subContractorBusinessTypeRepository;
        private GenericRepository<PropertySubContractor> propertySubContractorRepository;
        private GenericRepository<ProServices> proServicesRepository;
        private GenericRepository<ProServiceElementMap> proServiceElementMapRepository;
        private GenericRepository<WebAPIClient> webAPIClientRepository;
        private GenericRepository<WebAPIClientReport> webAPIClientReportRepository;
        private GenericRepository<WebAPIClientActivity> webAPIClientActivityRepository;
        private GenericRepository<WebAPIClientRowVersion> webAPIClientRowVersionRepository;
        private GenericRepository<WebAPIAccess> webAPIAccessRepository;
        private GenericRepository<WebAPIClientAccess> webAPIClientAccessRepository;
        private GenericRepository<APMPServiceSchedule> apmpServiceScheduleRepository;
        private GenericRepository<UserAfterHours> userAfterHoursRepository;
        private GenericRepository<Application> applicationRepository;
        private GenericRepository<UserApplication> userApplicationRepository;
        private GenericRepository<LocalContractorType> localContractorTypeRepository;
        public GenericRepository<ProServiceElementMap> ProServiceElementMapRepository
        {
            get
            {
                if (this.proServiceElementMapRepository == null)
                {
                    this.proServiceElementMapRepository = new GenericRepository<ProServiceElementMap>(context);
                }
                return proServiceElementMapRepository;
            }
        }

        public GenericRepository<CallLogFITS> CallLogFitsRepository
        {
            get
            {
                if (this.callLogFitsRepository == null)
                {
                    this.callLogFitsRepository = new GenericRepository<CallLogFITS>(context);
                }
                return callLogFitsRepository;
            }
        }


        public GenericRepository<UserAgency> UserAgencyRepository
        {
            get
            {
                if (this.userAgencyRepository == null)
                {
                    this.userAgencyRepository = new GenericRepository<UserAgency>(context);
                }
                return userAgencyRepository;
            }
        }

        public GenericRepository<CrystalReport> CrystalReportRepository
        {
            get
            {
                if (this.crystalReportRepository == null)
                {
                    this.crystalReportRepository = new GenericRepository<CrystalReport>(context);
                }
                return crystalReportRepository;
            }
        }

        public GenericRepository<UserProperty> UserPropertyRepository
        {
            get
            {
                if (this.userPropertyRepository == null)
                {
                    this.userPropertyRepository = new GenericRepository<UserProperty>(context);
                }
                return userPropertyRepository;
            }
        }

        public GenericRepository<User> UserRepository
        {
            get
            {
                if (this.userRepository == null)
                {
                    this.userRepository = new GenericRepository<User>(context);
                }
                return userRepository;
            }
        }
        public GenericRepository<Role> RoleRepository
        {
            get
            {
                if (this.roleRepository == null)
                {
                    this.roleRepository = new GenericRepository<Role>(context);
                }
                return roleRepository;
            }
        }
        public GenericRepository<RolesRule> RolesRuleRepository
        {
            get
            {
                if (this.rolesRuleRepository == null)
                {
                    this.rolesRuleRepository = new GenericRepository<RolesRule>(context);
                }
                return rolesRuleRepository;
            }
        }

        public GenericRepository<SavedReport> SavedReportRepository
        {
            get
            {
                if (this.savedReportRepository == null)
                {
                    this.savedReportRepository = new GenericRepository<SavedReport>(context);
                }
                return savedReportRepository;
            }
        }
        public GenericRepository<ReportType> ReportTypeRepository
        {
            get
            {
                if (this.reportTypeRepository == null)
                {
                    this.reportTypeRepository = new GenericRepository<ReportType>(context);
                }
                return reportTypeRepository;
            }
        }
        public GenericRepository<ReportField> ReportFieldRepository
        {
            get
            {
                if (this.reportFieldRepository == null)
                {
                    this.reportFieldRepository = new GenericRepository<ReportField>(context);
                }
                return reportFieldRepository;
            }
        }
        public GenericRepository<Report> ReportRepository
        {
            get
            {
                if (this.reportRepository == null)
                {
                    this.reportRepository = new GenericRepository<Report>(context);
                }
                return reportRepository;
            }
        }
        public GenericRepository<FloorType> FloorTypeRepository
        {
            get
            {
                if (this.floorTypeRepository == null)
                {
                    this.floorTypeRepository = new GenericRepository<FloorType>(context);
                }
                return floorTypeRepository;
            }
        }
        public GenericRepository<Equipment> EquipmentRepository
        {
            get
            {
                if (this.equipmentRepository == null)
                {
                    this.equipmentRepository = new GenericRepository<Equipment>(context);
                }
                return equipmentRepository;
            }
        }

        public GenericRepository<ScheduledRate> ScheduledRateRepository
        {
            get
            {
                if (this.scheduledRateRepository == null)
                {
                    this.scheduledRateRepository = new GenericRepository<ScheduledRate>(context);
                }
                return scheduledRateRepository;
            }
        }

        public GenericRepository<CommerceManagementFee> CommerceManagementFeeRepository
        {
            get
            {
                if (this.commerceManagementFeeRepository == null)
                {
                    this.commerceManagementFeeRepository = new GenericRepository<CommerceManagementFee>(context);
                }
                return commerceManagementFeeRepository;
            }
        }


        public GenericRepository<ScheduledRateCategory> ScheduledRateCategoryRepository
        {
            get
            {
                if (this.scheduledRateCategoryRepository == null)
                {
                    this.scheduledRateCategoryRepository = new GenericRepository<ScheduledRateCategory>(context);
                }
                return scheduledRateCategoryRepository;
            }
        }


        public GenericRepository<ScheduledRateHeading> ScheduledRateHeadingRepository
        {
            get
            {
                if (this.scheduledRateHeadingRepository == null)
                {
                    this.scheduledRateHeadingRepository = new GenericRepository<ScheduledRateHeading>(context);
                }
                return scheduledRateHeadingRepository;
            }
        }

        private GenericRepository<Document> documentRepository;

        public GenericRepository<Agency> AgencyRepository
        {
            get
            {
                if (this.agencyRepository == null)
                {
                    this.agencyRepository = new GenericRepository<Agency>(context);
                }
                return agencyRepository;
            }
        }

        public GenericRepository<Property> PropertyRepository
        {
            get
            {
                if (this.propertyRepository == null)
                {
                    this.propertyRepository = new GenericRepository<Property>(context);
                }
                return propertyRepository;
            }
        }

        public GenericRepository<PropertyValuation> PropertyValuationRepository
        {
            get
            {
                if (this.propertyValuationRepository == null)
                {
                    this.propertyValuationRepository = new GenericRepository<PropertyValuation>(context);
                }
                return propertyValuationRepository;
            }
        }

        public GenericRepository<Program> ProgramRepository
        {
            get
            {
                if (this.programRepository == null)
                {
                    this.programRepository = new GenericRepository<Program>(context);
                }
                return programRepository;
            }
        }

        public GenericRepository<FinancialYear> FinancialYearRepository
        {
            get
            {
                if (this.financialYearRepository == null)
                {
                    this.financialYearRepository = new GenericRepository<FinancialYear>(context);
                }
                return financialYearRepository;
            }
        }

        public GenericRepository<Electorate> ElectorateRepository
        {
            get
            {
                if (this.electorateRepository == null)
                {
                    this.electorateRepository = new GenericRepository<Electorate>(context);
                }
                return electorateRepository;
            }
        }

        public GenericRepository<LocalGovernmentArea> LocalGovernmentAreaRepository
        {
            get
            {
                if (this.localGovernmentAreaRepository == null)
                {
                    this.localGovernmentAreaRepository = new GenericRepository<LocalGovernmentArea>(context);
                }
                return localGovernmentAreaRepository;
            }
        }
        public GenericRepository<Zoning> ZoningRepository
        {
            get
            {
                if (this.zoningRepository == null)
                {
                    this.zoningRepository = new GenericRepository<Zoning>(context);
                }
                return zoningRepository;
            }
        }
        public GenericRepository<SmokeDetector> SmokeDetectorRepository
        {
            get
            {
                if (this.smokeDetectorRepository == null)
                {
                    this.smokeDetectorRepository = new GenericRepository<SmokeDetector>(context);
                }
                return smokeDetectorRepository;
            }
        }
        public GenericRepository<AsbestosStatus> AsbestosStatusRepository
        {
            get
            {
                if (this.asbestosStatusRepository == null)
                {
                    this.asbestosStatusRepository = new GenericRepository<AsbestosStatus>(context);
                }
                return asbestosStatusRepository;
            }
        }
        public GenericRepository<State> StateRepository
        {
            get
            {
                if (this.stateRepository == null)
                {
                    this.stateRepository = new GenericRepository<State>(context);
                }
                return stateRepository;
            }
        }
        public GenericRepository<Facility> FacilityRepository
        {
            get
            {
                if (this.facilityRepository == null)
                {
                    this.facilityRepository = new GenericRepository<Facility>(context);
                }
                return facilityRepository;
            }
        }
        public GenericRepository<Region> RegionRepository
        {
            get
            {
                if (this.regionRepository == null)
                {
                    this.regionRepository = new GenericRepository<Region>(context);
                }
                return regionRepository;
            }
        }
        public GenericRepository<SubRegion> SubRegionRepository
        {
            get
            {
                if (this.subRegionRepository == null)
                {
                    this.subRegionRepository = new GenericRepository<SubRegion>(context);
                }
                return subRegionRepository;
            }
        }
        public GenericRepository<CoreBusiness> CoreBusinessRepository
        {
            get
            {
                if (this.coreBusinessRepository == null)
                {
                    this.coreBusinessRepository = new GenericRepository<CoreBusiness>(context);
                }
                return coreBusinessRepository;
            }
        }
        public GenericRepository<Tenure> TenureRepository
        {
            get
            {
                if (this.tenureRepository == null)
                {
                    this.tenureRepository = new GenericRepository<Tenure>(context);
                }
                return tenureRepository;
            }
        }

        public GenericRepository<AcquisitionMethod> AcquisitionMethodRepository
        {
            get
            {
                if (this.acquisitionMethodRepository == null)
                {
                    this.acquisitionMethodRepository = new GenericRepository<AcquisitionMethod>(context);
                }
                return acquisitionMethodRepository;
            }
        }
        public GenericRepository<PlanType> PlanTypeRepository
        {
            get
            {
                if (this.planTypeRepository == null)
                {
                    this.planTypeRepository = new GenericRepository<PlanType>(context);
                }
                return planTypeRepository;
            }
        }
        public GenericRepository<ConditionRating> ConditionRatingRepository
        {
            get
            {
                if (this.conditionRatingRepository == null)
                {
                    this.conditionRatingRepository = new GenericRepository<ConditionRating>(context);
                }
                return conditionRatingRepository;
            }
        }
        public GenericRepository<PrimaryUse> PrimaryUseRepository
        {
            get
            {
                if (this.primaryUseRepository == null)
                {
                    this.primaryUseRepository = new GenericRepository<PrimaryUse>(context);
                }
                return primaryUseRepository;
            }
        }
        public GenericRepository<BuildingType> BuildingTypeRepository
        {
            get
            {
                if (this.buildingTypeRepository == null)
                {
                    this.buildingTypeRepository = new GenericRepository<BuildingType>(context);
                }
                return buildingTypeRepository;
            }
        }
        public GenericRepository<RoomFunction> RoomFunctionRepository
        {
            get
            {
                if (this.roomFunctionRepository == null)
                {
                    this.roomFunctionRepository = new GenericRepository<RoomFunction>(context);
                }
                return roomFunctionRepository;
            }
        }
        public GenericRepository<Building> BuildingRepository
        {
            get
            {
                if (this.buildingRepository == null)
                {
                    this.buildingRepository = new GenericRepository<Building>(context);
                }
                return buildingRepository;
            }
        }
        public GenericRepository<Room> RoomRepository
        {
            get
            {
                if (this.roomRepository == null)
                {
                    this.roomRepository = new GenericRepository<Room>(context);
                }
                return roomRepository;
            }
        }

        public GenericRepository<Claim> ClaimRepository
        {
            get
            {
                if (this.claimRepository == null)
                {
                    this.claimRepository = new GenericRepository<Claim>(context);
                }
                return claimRepository;
            }
        }

        public GenericRepository<IntensityUse> IntensityUseRepository
        {
            get
            {
                if (this.intensityUseRepository == null)
                {
                    this.intensityUseRepository = new GenericRepository<IntensityUse>(context);
                }
                return intensityUseRepository;
            }
        }
        public GenericRepository<Document> DocumentRepository
        {
            get
            {
                if (this.documentRepository == null)
                {
                    this.documentRepository = new GenericRepository<Document>(context);
                }
                return documentRepository;
            }
        }

        public GenericRepository<EquipmentDocument> EquipmentDocumentRepository
        {
            get
            {
                if (this.equipmentDocumentRepository == null)
                {
                    this.equipmentDocumentRepository = new GenericRepository<EquipmentDocument>(context);
                }
                return equipmentDocumentRepository;
            }
        }

        public GenericRepository<EquipmentType> EquipmentTypeRepository
        {
            get
            {
                if (this.equipmentTypeRepository == null)
                {
                    this.equipmentTypeRepository = new GenericRepository<EquipmentType>(context);
                }
                return equipmentTypeRepository;
            }
        }

        public GenericRepository<Element> ElementRepository
        {
            get
            {
                if (this.elementRepository == null)
                {
                    this.elementRepository = new GenericRepository<Element>(context);
                }
                return elementRepository;
            }
        }

        public GenericRepository<SubElement> SubElementRepository
        {
            get
            {
                if (this.subElementRepository == null)
                {
                    this.subElementRepository = new GenericRepository<SubElement>(context);
                }
                return subElementRepository;
            }
        }

        public GenericRepository<Component> ComponentRepository
        {
            get
            {
                if (this.componentRepository == null)
                {
                    this.componentRepository = new GenericRepository<Component>(context);
                }
                return componentRepository;
            }
        }

        public GenericRepository<MakeModel> MakeModelRepository
        {
            get
            {
                if (this.makeModelRepository == null)
                {
                    this.makeModelRepository = new GenericRepository<MakeModel>(context);
                }
                return makeModelRepository;
            }
        }

        public GenericRepository<PlannedMaintenanceProgram> PlannedMaintenanceProgramRepository
        {
            get
            {
                if (this.plannedMaintenanceProgramRepository == null)
                {
                    this.plannedMaintenanceProgramRepository = new GenericRepository<PlannedMaintenanceProgram>(context);
                }
                return plannedMaintenanceProgramRepository;
            }
        }

        public GenericRepository<PlannedMaintenanceJob> PlannedMaintenanceJobRepository
        {
            get
            {
                if (this.plannedMaintenanceJobRepository == null)
                {
                    this.plannedMaintenanceJobRepository = new GenericRepository<PlannedMaintenanceJob>(context);
                }
                return plannedMaintenanceJobRepository;
            }
        }

        public GenericRepository<PlannedMaintenanceOtherProgram> PlannedMaintenanceOtherProgramRepository
        {
            get
            {
                if (this.plannedMaintenanceOtherProgramRepository == null)
                {
                    this.plannedMaintenanceOtherProgramRepository = new GenericRepository<PlannedMaintenanceOtherProgram>(context);
                }
                return plannedMaintenanceOtherProgramRepository;
            }
        }

        public GenericRepository<PlannedMaintenanceOtherJob> PlannedMaintenanceOtherJobRepository
        {
            get
            {
                if (this.plannedMaintenanceOtherJobRepository == null)
                {
                    this.plannedMaintenanceOtherJobRepository = new GenericRepository<PlannedMaintenanceOtherJob>(context);
                }
                return plannedMaintenanceOtherJobRepository;
            }
        }

        public GenericRepository<PlannedMaintenanceJobStatus> PlannedMaintenanceJobStatusRepository
        {
            get
            {
                if (this.plannedMaintenanceJobStatusRepository == null)
                {
                    this.plannedMaintenanceJobStatusRepository = new GenericRepository<PlannedMaintenanceJobStatus>(context);
                }
                return plannedMaintenanceJobStatusRepository;
            }
        }

        public GenericRepository<PlannedMaintenanceOtherJobStatus> PlannedMaintenanceOtherJobStatusRepository
        {
            get
            {
                if (this.plannedMaintenanceOtherJobStatusRepository == null)
                {
                    this.plannedMaintenanceOtherJobStatusRepository = new GenericRepository<PlannedMaintenanceOtherJobStatus>(context);
                }
                return plannedMaintenanceOtherJobStatusRepository;
            }
        }

        public GenericRepository<Model.Models.Order> OrderRepository
        {
            get
            {
                if (this.orderRepository == null)
                {
                    this.orderRepository = new GenericRepository<Model.Models.Order>(context);
                }
                return orderRepository;
            }
        }

        public GenericRepository<OrderJobType> OrderJobTypeRepository
        {
            get
            {
                if (this.orderJobTypeRepository == null)
                {
                    this.orderJobTypeRepository = new GenericRepository<OrderJobType>(context);
                }
                return orderJobTypeRepository;
            }
        }

        public GenericRepository<EUROtherJob> EUROtherJobRepository
        {
            get
            {
                if (this.eurOtherJobRepository == null)
                {
                    this.eurOtherJobRepository = new GenericRepository<EUROtherJob>(context);
                }
                return eurOtherJobRepository;
            }
        }

        public GenericRepository<ClaimType> ClaimTypeRepository
        {
            get
            {
                if (this.claimTypeRepository == null)
                {
                    this.claimTypeRepository = new GenericRepository<ClaimType>(context);
                }
                return claimTypeRepository;
            }
        }

        public GenericRepository<ContractStatus> ContractStatusRepository
        {
            get
            {
                if (this.contractStatusRepository == null)
                {
                    this.contractStatusRepository = new GenericRepository<ContractStatus>(context);
                }
                return contractStatusRepository;
            }
        }

        public GenericRepository<PlannedMaintenanceJobBuilding> PlannedMaintenanceJobBuildingRepository
        {
            get
            {
                if (this.plannedMaintenanceJobBuildingRepository == null)
                {
                    this.plannedMaintenanceJobBuildingRepository = new GenericRepository<PlannedMaintenanceJobBuilding>(context);
                }
                return plannedMaintenanceJobBuildingRepository;
            }
        }

        public GenericRepository<CommerceCVFee> CommerceCVFeeRepository
        {
            get
            {
                if (this.commerceCVFeeRepository == null)
                {
                    this.commerceCVFeeRepository = new GenericRepository<CommerceCVFee>(context);
                }
                return commerceCVFeeRepository;
            }
        }

        public GenericRepository<PlannedMaintenanceOtherJobBuilding> PlannedMaintenanceOtherJobBuildingRepository
        {
            get
            {
                if (this.plannedMaintenanceOtherJobBuildingRepository == null)
                {
                    this.plannedMaintenanceOtherJobBuildingRepository = new GenericRepository<PlannedMaintenanceOtherJobBuilding>(context);
                }
                return plannedMaintenanceOtherJobBuildingRepository;
            }
        }

        public GenericRepository<EUROtherJobBuilding> EUROtherJobBuildingRepository
        {
            get
            {
                if (this.eurOtherJobBuildingRepository == null)
                {
                    this.eurOtherJobBuildingRepository = new GenericRepository<EUROtherJobBuilding>(context);
                }
                return eurOtherJobBuildingRepository;
            }
        }

        public GenericRepository<EUROtherJobStatus> EUROtherJobStatusRepository
        {
            get
            {
                if (this.eurOtherJobStatusRepository == null)
                {
                    this.eurOtherJobStatusRepository = new GenericRepository<EUROtherJobStatus>(context);
                }
                return eurOtherJobStatusRepository;
            }
        }

        public GenericRepository<EUROtherJobUrgency> EUROtherJobUrgencyRepository
        {
            get
            {
                if (this.eurOtherJobUrgencyRepository == null)
                {
                    this.eurOtherJobUrgencyRepository = new GenericRepository<EUROtherJobUrgency>(context);
                }
                return eurOtherJobUrgencyRepository;
            }
        }

        public GenericRepository<WorkCategory> WorkCategoryRepository
        {
            get
            {
                if (this.workCategoryRepository == null)
                {
                    this.workCategoryRepository = new GenericRepository<WorkCategory>(context);
                }
                return workCategoryRepository;
            }
        }

        public GenericRepository<WorkSubCategory> WorkSubCategoryRepository
        {
            get
            {
                if (this.workSubCategoryRepository == null)
                {
                    this.workSubCategoryRepository = new GenericRepository<WorkSubCategory>(context);
                }
                return workSubCategoryRepository;
            }
        }

        public GenericRepository<CostCategory> CostCategoryRepository
        {
            get
            {
                if (this.costCategoryRepository == null)
                {
                    this.costCategoryRepository = new GenericRepository<CostCategory>(context);
                }
                return costCategoryRepository;
            }
        }


        public GenericRepository<OrderAdjustment> OrderAdjustmentRepository
        {
            get
            {
                if (this.orderAdjustmentRepository == null)
                {
                    this.orderAdjustmentRepository = new GenericRepository<OrderAdjustment>(context);
                }
                return orderAdjustmentRepository;
            }
        }

        public GenericRepository<ContractStatusForPMP> ContractStatusForPMPRepository
        {
            get
            {
                if (this.contractStatusForPMPRepository == null)
                {
                    this.contractStatusForPMPRepository = new GenericRepository<ContractStatusForPMP>(context);
                }
                return contractStatusForPMPRepository;
            }
        }

        public GenericRepository<RateYear> RateYearRepository
        {
            get
            {
                if (this.rateYearRepository == null)
                {
                    this.rateYearRepository = new GenericRepository<RateYear>(context);
                }
                return rateYearRepository;
            }
        }

        public GenericRepository<CostAdjustmentType> CostAdjustmentTypeRepository
        {
            get
            {
                if (this.costAdjustmentTypeRepository == null)
                {
                    this.costAdjustmentTypeRepository = new GenericRepository<CostAdjustmentType>(context);
                }
                return costAdjustmentTypeRepository;
            }
        }

        public GenericRepository<GstRate> GstRateRepository
        {
            get
            {
                if (this.gstRateRepository == null)
                {
                    this.gstRateRepository = new GenericRepository<GstRate>(context);
                }
                return gstRateRepository;
            }
        }

        public GenericRepository<ServicePeriod> ServicePeriodRepository
        {
            get
            {
                if (this.servicePeriodRepository == null)
                {
                    this.servicePeriodRepository = new GenericRepository<ServicePeriod>(context);
                }
                return servicePeriodRepository;
            }
        }

        public GenericRepository<AdviceAuthority> AdviceAuthorityRepository
        {
            get
            {
                if (this.adviceAuthorityRepository == null)
                {
                    this.adviceAuthorityRepository = new GenericRepository<AdviceAuthority>(context);
                }
                return adviceAuthorityRepository;
            }
        }


        public GenericRepository<CostAdjustment> CostAdjustmentRepository
        {
            get
            {
                if (this.costAdjustmentRepository == null)
                {
                    this.costAdjustmentRepository = new GenericRepository<CostAdjustment>(context);
                }
                return costAdjustmentRepository;
            }
        }

        public GenericRepository<NewEntitySeq> NewEntitySeqRepository
        {
            get
            {
                if (this.newEntitySeqRepository == null)
                {
                    this.newEntitySeqRepository = new GenericRepository<NewEntitySeq>(context);
                }
                return this.newEntitySeqRepository;
            }
        }

        public GenericRepository<CallLogFinanceUrgency> CallLogFinanceUrgencyRepository
        {
            get
            {
                if (this.callLogFinanceUrgencyRepository == null)
                {
                    this.callLogFinanceUrgencyRepository = new GenericRepository<CallLogFinanceUrgency>(context);
                }
                return this.callLogFinanceUrgencyRepository;
            }
        }

        public GenericRepository<CallLogType> CallLogTypeRepository
        {
            get
            {
                if (this.callLogTypeRepository == null)
                {
                    this.callLogTypeRepository = new GenericRepository<CallLogType>(context);
                }
                return this.callLogTypeRepository;
            }
        }

        public GenericRepository<CallLogStatus> CallLogStatusRepository
        {
            get
            {
                if (this.callLogStatusRepository == null)
                {
                    this.callLogStatusRepository = new GenericRepository<CallLogStatus>(context);
                }
                return this.callLogStatusRepository;
            }
        }

        public GenericRepository<ProposalStatus> ProposalStatusRepository
        {
            get
            {
                if (this.proposalStatusRepository == null)
                {
                    this.proposalStatusRepository = new GenericRepository<ProposalStatus>(context);
                }
                return this.proposalStatusRepository;
            }
        }

        public GenericRepository<SupportDocumentType> SupportDocumentTypeRepository
        {
            get
            {
                if (this.supportDocumentTypeRepository == null)
                {
                    this.supportDocumentTypeRepository = new GenericRepository<SupportDocumentType>(context);
                }
                return this.supportDocumentTypeRepository;
            }
        }

        public GenericRepository<SupportDocumentClass> SupportDocumentClassRepository
        {
            get
            {
                if (this.supportDocumentClassRepository == null)
                {
                    this.supportDocumentClassRepository = new GenericRepository<SupportDocumentClass>(context);
                }
                return this.supportDocumentClassRepository;
            }
        }

        public GenericRepository<EquipmentTypeBSEMAssembly> AssemblyEquipmentTypesRepository
        {
            get
            {
                if (this.assemblyEquipmentTypes == null)
                {
                    this.assemblyEquipmentTypes = new GenericRepository<EquipmentTypeBSEMAssembly>(context);
                }
                return this.assemblyEquipmentTypes;
            }
        }

        public GenericRepository<MaintenanceTasksCallLog> MaintenanceTasksCallLogRepository
        {
            get
            {
                if (this.maintenanceTaskCallLog == null)
                {
                    this.maintenanceTaskCallLog = new GenericRepository<MaintenanceTasksCallLog>(context);
                }
                return this.maintenanceTaskCallLog;
            }
        }


        public GenericRepository<MaintenanceTask> MaintenanceTasksRepository
        {
            get
            {
                if (this.maintenanceTasks == null)
                {
                    this.maintenanceTasks = new GenericRepository<MaintenanceTask>(context);
                }
                return this.maintenanceTasks;
            }
        }

        public GenericRepository<SchoolLimit> SchoolLimitRepository
        {
            get
            {
                if (this.schoolLimitRepository == null)
                {
                    this.schoolLimitRepository = new GenericRepository<SchoolLimit>(context);
                }
                return this.schoolLimitRepository;
            }
        }

        public GenericRepository<MaintenanceSchedule> MaintenanceScheduleRepository
        {
            get
            {
                if (this.maintenanceScheduleRepository == null)
                {
                    this.maintenanceScheduleRepository = new GenericRepository<MaintenanceSchedule>(context);
                }
                return maintenanceScheduleRepository;
            }
        }

        public GenericRepository<ServiceReportStatus> ServiceReportStatusRepository
        {
            get
            {
                if (this.serviceReportStatusRepository == null)
                {
                    this.serviceReportStatusRepository = new GenericRepository<ServiceReportStatus>(context);
                }
                return serviceReportStatusRepository;
            }
        }

        public GenericRepository<ServiceReportProStatus> ServiceReportProStatusRepository
        {
            get
            {
                if (this.serviceReportProStatusRepository == null)
                {
                    this.serviceReportProStatusRepository = new GenericRepository<ServiceReportProStatus>(context);
                }
                return serviceReportProStatusRepository;
            }
        }

        public GenericRepository<ServiceReportJobCategory> ServiceReportJobCategoryRepository
        {
            get
            {
                if (this.serviceReportJobCategoryRepository == null)
                {
                    this.serviceReportJobCategoryRepository = new GenericRepository<ServiceReportJobCategory>(context);
                }
                return serviceReportJobCategoryRepository;
            }
        }

        public GenericRepository<Assessment> AssessmentRepository
        {
            get
            {
                if (this.assessmentRepository == null)
                {
                    this.assessmentRepository = new GenericRepository<Assessment>(context);
                }
                return assessmentRepository;
            }
        }

        public GenericRepository<AssessmentSignOffGroup> AssessmentSignOffGroupRepository
        {
            get
            {
                if (this.assessmentSignOffGroupRepository == null)
                {
                    this.assessmentSignOffGroupRepository = new GenericRepository<AssessmentSignOffGroup>(context);
                }
                return assessmentSignOffGroupRepository;
            }
        }


        public GenericRepository<EquipServiceStatus> EquipServiceStatusRepository
        {
            get
            {
                if (this.equipServiceStatusRepository == null)
                {
                    this.equipServiceStatusRepository = new GenericRepository<EquipServiceStatus>(context);
                }
                return equipServiceStatusRepository;
            }
        }
        public GenericRepository<EquipServiceStatusLineItem> EquipServiceStatusLineItemRepository
        {
            get
            {
                if (this.equipServiceStatusLineItemRepository == null)
                {
                    this.equipServiceStatusLineItemRepository = new GenericRepository<EquipServiceStatusLineItem>(context);
                }
                return equipServiceStatusLineItemRepository;
            }
        }
        public GenericRepository<EquipService> EquipServiceRepository
        {
            get
            {
                if (this.equipServiceRepository == null)
                {
                    this.equipServiceRepository = new GenericRepository<EquipService>(context);
                }
                return equipServiceRepository;
            }
        }

        public GenericRepository<ServiceReportApprovalType> ServiceReportApprovalTypesRepository
        {
            get
            {
                if (this.serviceReportApprovalTypesRepository == null)
                {
                    this.serviceReportApprovalTypesRepository = new GenericRepository<ServiceReportApprovalType>(context);
                }
                return serviceReportApprovalTypesRepository;
            }
        }

        public GenericRepository<ContractVariationStatus> ContractVariationStatusRepository
        {
            get
            {
                if (this.contractVariationStatusRepository == null)
                {
                    this.contractVariationStatusRepository = new GenericRepository<ContractVariationStatus>(context);
                }
                return contractVariationStatusRepository;
            }
        }

        //public GenericRepository<QlikReportURL> QlikReportURLRepository
        //{
        //    get
        //    {
        //        if (this.qlikReportURLRepository == null)
        //        {
        //            this.qlikReportURLRepository = new GenericRepository<QlikReportURL>(context);
        //        }
        //        return qlikReportURLRepository;
        //    }
        //}

        public GenericRepository<StrategicDriver> StrategicDriverRepository
        {
            get
            {
                if (this.strategicDriverRepository == null)
                {
                    this.strategicDriverRepository = new GenericRepository<StrategicDriver>(context);
                }
                return strategicDriverRepository;
            }
        }
        public GenericRepository<Priority> PriorityRepository
        {
            get
            {
                if (this.priorityRepository == null)
                {
                    this.priorityRepository = new GenericRepository<Priority>(context);
                }
                return priorityRepository;
            }
        }
        public GenericRepository<RecommendedWork> RecommendedWorkRepository
        {
            get
            {
                if (this.recommendedWorkRepository == null)
                {
                    this.recommendedWorkRepository = new GenericRepository<RecommendedWork>(context);
                }
                return recommendedWorkRepository;
            }
        }
        public GenericRepository<Condition> ConditionRepository
        {
            get
            {
                if (this.conditionRepository == null)
                {
                    this.conditionRepository = new GenericRepository<Condition>(context);
                }
                return conditionRepository;
            }
        }
        public GenericRepository<RoomType> RoomTypeRepository
        {
            get
            {
                if (this.roomTypeRepository == null)
                {
                    this.roomTypeRepository = new GenericRepository<RoomType>(context);
                }
                return roomTypeRepository;
            }
        }
        public GenericRepository<AreaType> AreaTypeRepository
        {
            get
            {
                if (this.areaTypeRepository == null)
                {
                    this.areaTypeRepository = new GenericRepository<AreaType>(context);
                }
                return areaTypeRepository;
            }
        }

        public GenericRepository<Cause> CauseRepository
        {
            get
            {
                if (this.causeRepository == null)
                {
                    this.causeRepository = new GenericRepository<Cause>(context);
                }
                return causeRepository;
            }
        }


        public GenericRepository<Effect> EffectRepository
        {
            get
            {
                if (this.effectRepository == null)
                {
                    this.effectRepository = new GenericRepository<Effect>(context);
                }
                return effectRepository;
            }
        }

        public DbContextTransaction BeginSnapshotTransaction()
        {
            var transaction = context.Database.BeginTransaction(System.Data.IsolationLevel.Snapshot);
            return transaction;
        }

        public GenericRepository<TimeExtension> TimeExtensionRepository
        {
            get
            {
                if (this.timeExtensionRepository == null)
                {
                    this.timeExtensionRepository = new GenericRepository<TimeExtension>(context);
                }
                return timeExtensionRepository;
            }
        }

        public GenericRepository<WorkType> WorkTypeRepository
        {
            get
            {
                if (this.workTypeRepository == null)
                {
                    this.workTypeRepository = new GenericRepository<WorkType>(context);
                }
                return workTypeRepository;
            }
        }

        public GenericRepository<Cleaner> CleanerRepository
        {
            get
            {
                if (this.cleanerRepository == null)
                {
                    this.cleanerRepository = new GenericRepository<Cleaner>(context);
                }
                return cleanerRepository;
            }
        }

        public GenericRepository<EmailKeyword> EmailKeywordRepository
        {
            get
            {
                if (this.emailKeywordRepository == null)
                {
                    this.emailKeywordRepository = new GenericRepository<EmailKeyword>(context);
                }
                return emailKeywordRepository;
            }
        }

        public GenericRepository<PSMWorkAssignmentStatus> PSMWorkAssignmentRepository
        {
            get
            {
                if (this.psmWorkAssignmentRepository == null)
                {
                    this.psmWorkAssignmentRepository = new GenericRepository<PSMWorkAssignmentStatus>(context);
                }
                return psmWorkAssignmentRepository;
            }
        }


        public GenericRepository<SubContractor> SubContractorRepository
        {
            get
            {
                if (this.subContractorRepository == null)
                {
                    this.subContractorRepository = new GenericRepository<SubContractor>(context);
                }
                return subContractorRepository;
            }
        }

        public GenericRepository<SubContractorAgency> SubContractorAgencyRepository
        {
            get
            {
                if (this.subContractorAgencyRepository == null)
                {
                    this.subContractorAgencyRepository = new GenericRepository<SubContractorAgency>(context);
                }
                return subContractorAgencyRepository;
            }
        }
        //public void EndReadUncommittedTransaction(DbContextTransaction transaction)
        //{

        //    transaction.Dispose();
        //}

        public GenericRepository<ProposalProjectType> ProjectTypeRepository
        {
            get
            {
                if (this.projectTypeRepository == null)
                {
                    this.projectTypeRepository = new GenericRepository<ProposalProjectType>(context);
                }
                return this.projectTypeRepository;
            }
        }

        public GenericRepository<ProposalWorkNominated> WorkNominatedRepository
        {
            get
            {
                if (this.workNominatedRepository == null)
                {
                    this.workNominatedRepository = new GenericRepository<ProposalWorkNominated>(context);
                }
                return this.workNominatedRepository;
            }
        }
        public GenericRepository<RegionalContact> RegionalContactRepository
        {
            get
            {
                if (this.regionalContactRepository == null)
                {
                    this.regionalContactRepository = new GenericRepository<RegionalContact>(context);
                }
                return this.regionalContactRepository;
            }
        }

        public GenericRepository<ProposalProgram> ProposalProgramRepository
        {
            get
            {
                if (this.proposalProgramRepository == null)
                {
                    this.proposalProgramRepository = new GenericRepository<ProposalProgram>(context);
                }
                return this.proposalProgramRepository;
            }
        }
        public GenericRepository<ProjectStatus> ProjectStatusRepository
        {
            get
            {
                if (this.projectStatusRepository == null)
                {
                    this.projectStatusRepository = new GenericRepository<ProjectStatus>(context);
                }
                return this.projectStatusRepository;
            }
        }
      


        public GenericRepository<ProposalSubProgram> ProposalSubProgramRepository
        {
            get
            {
                if (this.proposalSubProgramRepository == null)
                {
                    this.proposalSubProgramRepository = new GenericRepository<ProposalSubProgram>(context);
                }
                return this.proposalSubProgramRepository;
            }
        }

        public GenericRepository<ProposalPMMProfile> PMMProfileRepository
        {
            get
            {
                if (this.pmmProfileRepository == null)
                {
                    this.pmmProfileRepository = new GenericRepository<ProposalPMMProfile>(context);
                }
                return this.pmmProfileRepository;
            }
        }

        public GenericRepository<ProposalSFC> ProposalSFCRepository
        {
            get
            {
                if (this.proposalSFCRepository == null)
                {
                    this.proposalSFCRepository = new GenericRepository<ProposalSFC>(context);
                }
                return this.proposalSFCRepository;
            }
        }

        public GenericRepository<ProposalProcurement> ProposalProcurementRepository
        {
            get
            {
                if (this.proposalProcurementRepository == null)
                {
                    this.proposalProcurementRepository = new GenericRepository<ProposalProcurement>(context);
                }
                return this.proposalProcurementRepository;
            }
        }

        public GenericRepository<ProposalEndorsementType> EndorsementTypeRepository
        {
            get
            {
                if (this.endorsementTypeRepository == null)
                {
                    this.endorsementTypeRepository = new GenericRepository<ProposalEndorsementType>(context);
                }
                return this.endorsementTypeRepository;
            }
        }

        public GenericRepository<BusinessType> BusinessTypeRepository
        {
            get
            {
                if (this.businessTypeRepository == null)
                {
                    this.businessTypeRepository = new GenericRepository<BusinessType>(context);
                }
                return this.businessTypeRepository;
            }
        }

        public GenericRepository<Tradie> TradieRepository
        {
            get
            {
                if (this.tradieRepository == null)
                {
                    this.tradieRepository = new GenericRepository<Tradie>(context);
                }
                return tradieRepository;
            }
        }

        public GenericRepository<TradieServiceType> TradieServiceTypeRepository
        {
            get
            {
                if (this.tradieServiceTypeRepository == null)
                {
                    this.tradieServiceTypeRepository = new GenericRepository<TradieServiceType>(context);
                }
                return tradieServiceTypeRepository;
            }
        }

        public GenericRepository<SubContractorBusinessType> SubContractorBusinessTypeRepository
        {
            get
            {
                if (this.subContractorBusinessTypeRepository == null)
                {
                    this.subContractorBusinessTypeRepository = new GenericRepository<SubContractorBusinessType>(context);
                }
                return subContractorBusinessTypeRepository;
            }
        }

        public GenericRepository<PropertySubContractor> PropertySubContractorRepository
        {
            get
            {
                if (this.propertySubContractorRepository == null)
                {
                    this.propertySubContractorRepository = new GenericRepository<PropertySubContractor>(context);
                }
                return propertySubContractorRepository;
            }
        }

        public GenericRepository<ProServices> ProServicesRepository
        {
            get
            {
                if (this.proServicesRepository == null)
                {
                    this.proServicesRepository = new GenericRepository<ProServices>(context);
                }
                return proServicesRepository;
            }
        }

        public GenericRepository<WebAPIClient> WebAPIClientRepository
        {
            get
            {
                if (this.webAPIClientRepository == null)
                {
                    this.webAPIClientRepository = new GenericRepository<WebAPIClient>(context);
                }
                return webAPIClientRepository;
            }
        }

        public GenericRepository<WebAPIClientReport> WebAPIClientReportRepository
        {
            get
            {
                if (this.webAPIClientReportRepository == null)
                {
                    this.webAPIClientReportRepository = new GenericRepository<WebAPIClientReport>(context);
                }
                return webAPIClientReportRepository;
            }
        }

        public GenericRepository<WebAPIClientActivity> WebAPIClientActivityRepository
        {
            get
            {
                if (this.webAPIClientActivityRepository == null)
                {
                    this.webAPIClientActivityRepository = new GenericRepository<WebAPIClientActivity>(context);
                }
                return webAPIClientActivityRepository;
            }
        }

        public GenericRepository<WebAPIClientRowVersion> WebAPIClientRowVersionRepository
        {
            get
            {
                if (this.webAPIClientRowVersionRepository == null)
                {
                    this.webAPIClientRowVersionRepository = new GenericRepository<WebAPIClientRowVersion>(context);
                }
                return webAPIClientRowVersionRepository;
            }
        }

        public GenericRepository<WebAPIAccess> WebAPIAccessRepository
        {
            get
            {
                if (this.webAPIAccessRepository == null)
                {
                    this.webAPIAccessRepository = new GenericRepository<WebAPIAccess>(context);
                }
                return webAPIAccessRepository;
            }
        }
        public GenericRepository<WebAPIClientAccess> WebAPIClientAccessRepository
        {
            get
            {
                if (this.webAPIClientAccessRepository == null)
                {
                    this.webAPIClientAccessRepository = new GenericRepository<WebAPIClientAccess>(context);
                }
                return webAPIClientAccessRepository;
            }
        }
        public GenericRepository<APMPServiceSchedule> APMPServiceScheduleRepository
        {
            get
            {
                if (this.apmpServiceScheduleRepository == null)
                {
                    this.apmpServiceScheduleRepository = new GenericRepository<APMPServiceSchedule>(context);
                }
                return apmpServiceScheduleRepository;
            }
        }

        public GenericRepository<UserAfterHours> UserAfterHoursRepository
        {
            get
            {
                if (this.userAfterHoursRepository == null)
                {
                    this.userAfterHoursRepository = new GenericRepository<UserAfterHours>(context);
                }
                return userAfterHoursRepository;
            }
        }



        public AssetCareContext CurrentContext {
            get
            {
                return context;
            }
        }
        public int Save()
        {
            return context.SaveChanges();
        }

        private bool disposed = false;
        

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    context.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public GenericRepository<Application> ApplicationRepository
        {
            get
            {
                if (this.applicationRepository == null)
                {
                    this.applicationRepository = new GenericRepository<Application>(context);
                }
                return applicationRepository;
            }
        }
        public GenericRepository<UserApplication> UserApplicationRepository
        {
            get
            {
                if (this.userApplicationRepository == null)
                {
                    this.userApplicationRepository = new GenericRepository<UserApplication>(context);
                }
                return userApplicationRepository;
            }
        }

        public GenericRepository<LocalContractorType> LocalContractorTypeRepository
        {
            get
            {
                if (this.localContractorTypeRepository == null)
                {
                    this.localContractorTypeRepository = new GenericRepository<LocalContractorType>(context);
                }
                return localContractorTypeRepository;
            }
        }
    }
}
