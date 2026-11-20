using AssetCare.Model.Models;
using AssetCare.Model.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetCare.Service.Interfaces
{
    public interface IProposalService
    {
        IEnumerable<ProposalViewModel> FilterProposalData(ProposalFilterModel filterModel, bool includeFullDesc, out int filteredResultsCount, out int totalResultsCount);

        IEnumerable<ProposalForHPViewModel> FilterProposalDataForHP(ProposalFilterModel filterModel, bool includeFullDesc, out int filteredResultsCount, out int totalResultsCount);
        IEnumerable<ProposalForHPViewModel> FilterProposalDataForEndorsement(ProposalFilterModel filterModel, bool includeFullDesc, out int filteredResultsCount, out int totalResultsCount,string location);

        ProposalAddEditViewModel GetAddProposalViewModel(int agencyId, int propertyId, int? calloutId);
        List<WorkCategory> GetWorkCategory();
        IEnumerable<CallLogFinanceUrgency> GetFinanceUrgency();
        IEnumerable<ProposalStatus> GetProposalStatuses();
        IEnumerable<CallLogType> GetCallLogTypes();
        IEnumerable<EquipmentTypeBSEMAssembly> GetEquipmentTypeAssemblies();
        IEnumerable<MaintenanceTasksCallLog> GetMaintenanceTasksCallLogs();
        IEnumerable<SupportDocumentType> GetSupportDocumentTypes();
        bool Save(ProposalAddEditViewModel model, string loggedInUser, out int callLogId);
        ProposalAddEditViewModel GetProposalDetails(int? id, int agencyId);
        bool Update(ProposalAddEditViewModel model, string loggedInUser);
        AgencyContractor GetAgencyContractor(int agencyId);
        void DeleteProposal(int? callOutId, int? agencyId);
        Property GetPropertyDetails(int propertyId);
        List<Element> GetElementList();
        IEnumerable<WorkType> GetWorkTypes();
        List<EmailKeyword> GetEmailKeywords();
        int GetSupportDocumentClassId(int? workCategoryId);
        int SaveDocument(SupportDocument document, int callOutId, out int documentId);
        IEnumerable<ProposalProjectType> GetProjectTypes();
        IEnumerable<ProposalWorkNominated> GetWorkNominated();

        IEnumerable<ProposalProgram> GetPrograms();
        IEnumerable<ProposalSubProgram> GetSubPrograms(int? programId = 0);
        IEnumerable<ProposalPMMProfile> GetPMMProfiles();
        IEnumerable<ProposalSFC> GetProposalSFC();
        IEnumerable<ProposalProcurement> GetProposalProcurement();

        IEnumerable<ProposalProcurement> GetAllProposalProcurement();
        IEnumerable<RegionalContact> GetPMASO();
        IEnumerable<RegionalContact> GetRegionalContact();
        List<ProposalEndorsementType> GetEndorsementTypes();

        ProposalRoles GetProposalRoles(int agencyID, int propertyID);

        ItemsCounts GetProposalCounts(int agencyID, int propertyID, string RoleID);
        IEnumerable<ProposalSubProgram> GetSubProgramList(int programId);
    }
}
