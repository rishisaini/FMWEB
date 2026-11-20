using AssetCare.Model.Models;
using AssetCare.Model.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetCare.Service.Interfaces
{
    public interface ICalloutService
    {
        IEnumerable<CallOutViewModel> FilterCallOutData(CallOutFilterModel filterModel, bool includeFullDesc, out int filteredResultsCount, out int totalResultsCount);
        IEnumerable<CallOutForHPViewModel> FilterCallOutDataForHP(CallOutFilterModel filterModel, bool includeFullDesc, out int filteredResultsCount, out int totalResultsCount);

        CallOutAddEditViewModel GetAddCalloutViewModel(int agencyId, int propertyId, int workCategoryId, int? calloutId);
        List<WorkCategory> GetWorkCategory();
        IEnumerable<CallLogFinanceUrgency> GetFinanceUrgency();
        IEnumerable<CallLogStatus> GetCallLogStatuses();
        IEnumerable<CallLogType> GetCallLogTypes();
        IEnumerable<EquipmentTypeBSEMAssembly> GetEquipmentTypeAssemblies();
        IEnumerable<MaintenanceTasksCallLog> GetMaintenanceTasksCallLogs();
        IEnumerable<MaintenanceTask> GetMaintenanceTasks();
        IEnumerable<SupportDocumentType> GetSupportDocumentTypes();
        bool Save(CallOutAddEditViewModel model, string loggedInUser, out int callLogId);
        CallOutAddEditViewModel GetCallOutDetails(int? id, int agencyId, double estimationThreshold);
        bool Update(CallOutAddEditViewModel model, UserViewModel currentUser);
        void DeleteDocument(int? supportDocumentID);
        AgencyContractor GetAgencyContractor(int agencyId);
        void DeleteCallOut(int? callOutId, int? agencyId);
        int SaveDocument(SupportDocument document, int callOutId, out int documentId);
        Property GetPropertyDetails(int propertyId);
        List<Element> GetElementList();
        IEnumerable<WorkType> GetWorkTypes();
        int GetSupportDocumentClassId(int? workCategoryId);
        bool SaveInsIncident(CallOutInsAddModel model, UserViewModel currentUser);
        bool IsCalloutTotalEstimationOverLimit(int? id, int agencyId, double estimationThreshold);
        List<EmailKeyword> GetEmailKeywords();
        List<WorkCategory> GetAllWorkCategories();
        List<string> DetectAlertKeywords(CallOutAddEditViewModel model);
        SubContractor GetSubContractortails(int subContractorId);
        TradieResponse GetTradesPeopleList(CallOutFilterModel callOutFilterModel, out int filteredResultsCount, out int totalResultsCount);

        IEnumerable<TradieViewModel> GetTradieDetailsHP(int woid, int agencyid);
        IEnumerable<TradieViewModel> GetSchoolHomeTradies(CallOutFilterModel callOutFilterModel, out int filteredResultsCount, out int totalResultsCount);
        int? ConvertCallOutToWorkOrder(CallOutAddEditViewModel model, int subContractoid, string loggedInUser,string comments);
        List<BusinessType> GetSubContractorBusinessTypes();
        List<SubContractor> GetSubContractorsByBusinessType(int id, int agencyId);
        List<SubContractor> GetSubContractorsByAgency(int agencyId);
        List<SubContractor> GetLocalContractorsByAgency(int agencyId);
        SubContractor GetSubContractorDetails(int subContractorId);
        TaskListViewModel GetCalloutCounts(int agencyID, int propertyID,string roleID);
        SubContractor GetSubContractorInfo();
        IEnumerable<AssessorStatus> GetAssessorStatuses();
        bool SendCalloutCategoryChanged(CallOutAddEditViewModel model, UserViewModel currentUser);
        bool SendCalloutTotalEstimationOverLimit(CallOutAddEditViewModel model, UserViewModel currentUser, string estimationThreshold);
        bool UpdateTotalEstimationOverLimitNotificationDate(CallOutAddEditViewModel calloutModel);
        LCWOViewModel GetLCWOCount(int subContractorId);
    }

}
