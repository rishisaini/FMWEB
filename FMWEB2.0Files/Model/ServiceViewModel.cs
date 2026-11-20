using AssetCare.Model.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace AssetCare.Model.ViewModels
{
    public class ServiceViewModel
    {
        public int AgencyId { get; set; }
        public int APMPServiceScheduleID { get; set; }
       // public int EquipServiceStatusLineItemId { get; set; }
       // public int LineItemNo { get; set; }
        public int? ElementId { get; set; }
       // public int? ComponentId { get; set; }
     //  public int? ItemId { get; set; }

        public string ProServiceDesc { get; set; }
        public string ElementDesc { get; set; }
       
      //  public string ComponentDesc { get; set; }
      //  public string ItemDesc { get; set; }

       // public string SchoolName { get; set; }
       // public string AgencyName { get; set; }
       // public string FinancialyearName { get; set; }
      //  public string LineNumber { get; set; }
        public string Frequency { get; set; }


     //   public int ScheduledRateId { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime? StartDate { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime? EndDate { get; set; }

        public bool? IsAMUReviewed { get; set; }
        public bool? IsComplete { get; set; }
        /// <summary>
        /// 0 Create WO not required
        /// 1 Create WO
        /// Null - Disable WO already created
        /// </summary>
        public bool? WOExist { get; set; }

      //  public bool IsCompleted { get; set; }
        public int? ProWOID { get; set; }
        public int? AssetCount { get; set; }
        public int? ProAgencySequenceWOID { get; set; }
       // public Dictionary<int?, int> WOStatusId_ProWOID { get; set; }
       public List<WOStatusWOIDWOSeqID> WOStatusWOIDWOSeqIDs { get; set; }


        public int? SchoolID { get; set; }
        public string PropertyDesc { get; set; }

        public string FinancialYearName { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime? CreatedDate { get; set; }

        public string WOStatusWOIDWOSeqIDsText =>
        WOStatusWOIDWOSeqIDs == null
        ? string.Empty
        : string.Join(" ", WOStatusWOIDWOSeqIDs.Select(x => $"{x.ProAgencySequenceWOID}"));

        public ServiceViewModel()
        {
            
        }
       

    }
    public class WOStatusWOIDWOSeqID
    {
       
        public int? ServiceReportStatusId { get; set; }
        public int? ProWOID { get; set; }
        public int? ProAgencySequenceWOID { get; set; }
        public int? LCID { get; set; }

        public string LCName { get; set; }
        public string Sig { get; set; }
    }

}
