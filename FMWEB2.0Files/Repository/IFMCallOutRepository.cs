using System.Collections.Generic;
using AssetCare.Model.Models;
using AssetCare.Model.ViewModels;
using AssetCare.Repository.Interface;

namespace AssetCare.Repository.Interfaces
{
    public interface IFMCallOutRepository : IGenericRepository<CallLogFITS>
    {
        IEnumerable<CallOutViewModel> FilterCallOutData(CallOutFilterModel filterModel, bool includeFullDesc, List<int> userPropertyIds, string userRole, out int filteredResultsCount, out int totalResultsCount);
        IEnumerable<CallOutForHPViewModel> FilterCallOutDataForHP(CallOutFilterModel filterModel, bool includeFullDesc, List<int> userPropertyIds, string userRole, out int filteredResultsCount, out int totalResultsCount);

        bool Save(CallOutAddEditViewModel model, string loggedInUser, out int callLogId);
        CallOutAddEditViewModel GetDetailsById(int? id, int agencyId, double estimationThreshold);
        bool Update(CallOutAddEditViewModel model, UserViewModel currentUser);
        void DeleteDocument(int? supportDocumentID);
        void DeleteCallOut(int? callOutId, int? agencyId);
        int SaveDocument(SupportDocument document, int callOutId, out int documentId);
        bool SaveSchoolIncident(CallOutInsAddModel model);
        void SavePSMDefectCallLog(PSMCalloutAddEditViewModel model, UserViewModel currentUser);
        IEnumerable<CallOutViewModel> FilterDefectCallOutData(CallOutFilterModel filterModel, bool includeFullDesc, List<int> userPropertyIds, out int filteredResultsCount, out int totalResultsCount);
        PSMCalloutAddEditViewModel GetPSMDefectCalloutById(int agencySequenceId, int agencyId);
        List<SupportDocumentViewModel> GetUploadedSupportDocuments(int supportDocumentClassId, int masterRecordId, int agencyId, string schoolName);
        void UpdateDefectEURCallLogIdAndDocuments(int agencyId, int? agencySequenceId, int callLogFITSId, string usercode);
        IEnumerable<CallOutViewModel> FilterDefectCallOutDataHP(CallOutFilterModel filterModel, bool includeFullDesc, List<int> userPropertyIds, out int filteredResultsCount, out int totalResultsCount);
        List<SubContractor> GetSubContractorsByBusinessType(int id, int agencyId);
        TradieResponse GetTradies(CallOutFilterModel callOutFilterModel, out int filteredResultsCount, out int totalResultsCount);
        IEnumerable<TradieViewModel> GetTradieDetailsHP(int woid, int agencyId);
        IEnumerable<TradieViewModel>  GetSchoolHomeTradies(CallOutFilterModel callOutFilterModel, out int filteredResultsCount, out int totalResultsCount);
        TaskListViewModel GetCalloutCounts(int agencyID, int propertyID, List<int> userPropertyIds, string roleID, int defaultContractorId);
        SubContractor GetSubContractorInfo(int userID);
        bool IsTotalEstimationOverLimit(int? id, int agencyId, double estimationThreshold);
        IEnumerable<AssessorStatus> GetAssessorStatuses();
        bool UpdateTotalEstimationOverLimitNotificationDate(CallOutAddEditViewModel calloutModel);


        //Defect 
        IEnumerable<CallOutViewModel> FilterCapitalDefectData(CallOutFilterModel filterModel, bool includeFullDesc, List<int> userPropertyIds, string userRole, out int filteredResultsCount, out int totalResultsCount);
        IEnumerable<CallOutViewModel> FilterCapitalDefectDataHP(CallOutFilterModel filterModel, bool includeFullDesc, List<int> userPropertyIds, string userRole, out int filteredResultsCount, out int totalResultsCount);
        void SaveOrUpdatePSMDefectCallLog(CallOutAddEditViewModel model, UserViewModel currentUser);
        CapitalDefectCalloutViewModel GetPSMDefectCalloutByCallLogFITSId(int callLogFITSID, int agencyId);

    }
}