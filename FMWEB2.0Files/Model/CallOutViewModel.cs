using AssetCare.Model.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetCare.Model.ViewModels
{
    public class CallOutViewModel
    {
        public int Id { get; set; }
        public bool CanDelete { get; set; }
        [Required(ErrorMessage = "Please select a school")]
        public int? PropertyId { get; set; }
        [Required(ErrorMessage = "Please select a work category")]
        public int? WorkCategoryId { get; set; }

        public bool? ShowIsReadyForReimbursementOnly { get; set; }
        public int? AgencyId { get; set; }
        public int CallOutID { get; set; }
        public string SchoolName { get; set; }
        public string RequestedBy { get; set; }
        public DateTime? CallDate { get; set; }
        public string CallDateList { get; set; }
        public string Description { get; set; }
        public string WorkCategory { get; set; }
        public List<string> SubContractor { get; set; }
        public string Status { get; set; }
        public string Category { get; set; }
        public int StatusID { get; set; }
        public int? JobId { get; set; }
        public bool CanCreateJob { get; set; }
        public bool CanCreateCalout { get; set; }
        public int? ServiceReportId { get; set; }
        public int? ElementId { get; set; }
        public List<int?> JobIds { get; set; }
        public List<int> ServiceReportIds { get; set; }
        public string CurrentRoleID { get; set; }
        public string CalloutSummary { get; set; }
        public string SchoolUrgency { get; set; }

        public bool? AsbestosHRProperty { get; set; }
        public bool? DefectMgmt { get; set; }
        public bool CapitalDefect { get; set; }
        public bool? DefectMgmtProperty { get; set; }
        public string IsInsuranceEvent { get; set; }
        public string IsReadyForReimbursement { get; set; }
        public string AssessorStatus { get; set; }

        public string IsHazmat { get; set; }
        public string ClaimNumber { get; set; }
        public DateTime? IncidentDate { get; set; }
        public string IncidentDateList{ get; set; }
        public string SchoolIncidentCause { get; set; }
        public string Sig { get { return TamperHelper.GenerateSignature(this.Id); } }
        public List<string> SigWOs
        {
            get
            {
                if (ServiceReportIds == null || ServiceReportIds.Count == 0)
                    return new List<string>();

                return ServiceReportIds
                    .Select(id => TamperHelper.GenerateSignature(id))
                    .ToList();
            }
        }
    }

    public class RCTIDownloadModel
    {
        public int Id { get; set; }
    }

    public class CallOutForHPViewModel
    {
        [UnSearchable(true)]
        public int Id { get; set; }
        [UnSearchable(true)]
        [Required(ErrorMessage = "Please select a school")]
        public int? SchoolId { get; set; }
        [UnSearchable(true)]
        [Required(ErrorMessage = "Please select an agency")]
        public int? AgencyId { get; set; }
        public int CallOutID { get; set; }
        public string SchoolName { get; set; }
        public string ASOName { get; set; }
        public string ASOContactNumber { get; set; }
        public DateTime? CallDate { get; set; }
        public string CallDateList { get; set; }
        public string CallDateString { get; set; }
        public string CallOutTypeDesc { get; set; }
        public string Description { get; set; }
        [UnSearchable(true)]
        public int? CallOutTypeID { get; set; }
        public string CallOutUrgencyDesc { get; set; }
        [UnSearchable(true)]
        public int? CallOutUrgencyID { get; set; }
        public string RequestedBy { get; set; }
        public string Status { get; set; }
        public string Category { get; set; }
        public string SchoolUrgency { get; set; }
        public string IsInsuranceEvent { get; set; }
        public string IsReadyForReimbursement { get; set; }
        public string AssessorStatus { get; set; }
        public string ClaimNumber { get; set; }
        public DateTime? IncidentDate { get; set; }
        public string IncidentDateList { get; set; }
        public string SchoolIncidentCause { get; set; }
        public string Sig { get { return TamperHelper.GenerateSignature(this.Id); } }
    }



    public class CalloutFilterPostModel : DataTableAjaxPostModel
    {
        public int? AgencyId { get; set; }
        public int? Workcategory { get; set; }
        public int? SchoolId { get; set; }
        public int? PropertyId { get; set; }

        public int ExportType { get; set; }

        public int SubContractorID { get; set; }
        public int? StatusId { get; set; }

        public string ServiceReportACID { get; set; }
        public string ServiceReportACIDClaim { get; set; }
        public string RWOType { get; set; }
        public bool? ShowIsReadyForReimbursementOnly { get; set; }
        public bool? ShowWOPaidOnly { get; set; }
    }

    public class CallOutFilterModel : DataTableFilterModel
    {
        public int? AgencyId { get; set; }
        public int? Workcategory { get; set; }
        public int? SchoolId { get; set; }
        public int? PropertyId { get; set; }

        public int? SubContractorID { get; set; }
        public int? StatusId { get; set; }

        public string ServiceReportACID { get; set; }
        public string ServiceReportACIDClaim { get; set; }
        public string RWOType { get; set; }
        public bool RestrictToLocalContractorId { get; set; }
        public bool? ShowIsReadyForReimbursementOnly { get; set; }
        public bool? ShowWOPaidOnly { get; set; }

    }

    public class CustomDropdownItem
    {
        public string Text { get; set; }
        public string Value { get; set; }
        public string Data { get; set; }
    }

    [Serializable()]
    public class CallOutUserCapabilities
    {
        public string MainSection { get; set; }
        public string ResponseTimeSection { get; set; }
        public string CallCenterSection { get; set; }
        public string DocumentSection { get; set; }
        public string IRCMSection { get; set; }
        public string CapitalDefectSection { get; set; }

    }


}
