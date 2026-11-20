using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace AssetCare.Common
{
    public class ReactiveRCTIReportModel
    {
        public string WorkOrderNo { get; set; }
        public string SubContractorName { get; set; }
        public string Contract { get; set; }
        public string ContractNo { get; set; }
        public string SubContractorPostalAddress { get; set; }
        public string SubContractorSuburb { get; set; }
        public string SubContractorState { get; set; }
        public string SubContractorPostCode { get; set; }
        public string ContractorABN { get; set; }
        public decimal ClaimValue { get; set; }
        public decimal ClaimGST { get; set; }
        public decimal ClaimTotal { get; set; }
        public decimal ClaimValueGrandTotal { get; set; }
        public decimal ClaimGSTGrandTotal { get; set; }
        public decimal ClaimGrandTotal { get; set; }
        public decimal ApprovedValue { get; set; }
        public decimal ApprovedGST { get; set; }
        public decimal ApprovedTotal { get; set; }
        public decimal ApprovedValueGrandTotal { get; set; }
        public decimal ApprovedGSTGrandTotal { get; set; }
        public decimal ApprovedGrandTotal { get; set; }
        public DateTime AMUApprovedDate { get; set; }
        public string SignatureUrl { get; set; }
        public string SignedName { get; set; }
        public string WorkCategory { get; set; }
        public DataTable Table
        {
            get
            {
                DataTable dt = new DataTable("RCTIDatax");
                dt.Columns.Add(new DataColumn("ID", typeof(int)));
                return dt;
            }


        }
        public string School { get; set; }
    }
    public class FMRCTIViewModel
    {

        // data entry
        public int FinancialMonth { get; set; }
        public int FinancialYear { get; set; }

        public string FinancialDateDesc { get; set; }
        public string ProgressNo { get; set; }
        public string Contractor { get; set; }
        public string Contract { get; set; }
        public string ContractorPostalAddress { get; set; }
        public string ContractorSuburb { get; set; }
        public string ContractorState { get; set; }
        public string ContractorPostCode { get; set; }
        public string ContractNo { get; set; }
        public DateTime CurrentDate { get; set; }
        public string ContractorABN { get; set; }
        public decimal ClaimValue { get; set; }
        public decimal ClaimGST { get; set; }
        public decimal ClaimTotal { get; set; }
        public decimal ClaimValueGrandTotal { get; set; }
        public decimal ClaimGSTGrandTotal { get; set; }
        public decimal ClaimGrandTotal { get; set; }
        public decimal ApprovedValue { get; set; }
        public decimal ApprovedGST { get; set; }
        public decimal ApprovedTotal { get; set; }
        public decimal ApprovedValueGrandTotal { get; set; }
        public decimal ApprovedGSTGrandTotal { get; set; }
        public decimal ApprovedGrandTotal { get; set; }
        public DateTime AMUConfirmedDate { get; set; }
        public string APSignedName { get; set; }
        public string APName { get; set; }

        public string SignatureUrl { get; set; }
        public DataTable Table
        {
            get
            {
                DataTable dt = new DataTable("RCTIData");
                dt.Columns.Add(new DataColumn("ID", typeof(int)));
                return dt;
            }


        }
        public string D { get; set; }
        public DataSet MyProperty { get; set; }
        public List<RMCTIPaymentBreakdown> PaymentBreakdown { get; set; }

    }

    public class RMCTIPaymentBreakdown
    {
        public string WorkCategoryDesc { get; set; }
        public decimal? ApprovedValue { get; set; }
        public decimal? ApprovedGST { get; set; }
    }

    public class ProactiveRCTIReportModel
    {
        public string WorkOrderNo { get; set; }
        public string SubContractorName { get; set; }
        public string Contract { get; set; }
        public string ContractNo { get; set; }
        public string SubContractorPostalAddress { get; set; }
        public string SubContractorSuburb { get; set; }
        public string SubContractorState { get; set; }
        public string SubContractorPostCode { get; set; }
        public string ContractorABN { get; set; }
        public decimal ClaimValue { get; set; }
        public decimal ClaimGST { get; set; }
        public decimal ClaimTotal { get; set; }
        public decimal ClaimValueGrandTotal { get; set; }
        public decimal ClaimGSTGrandTotal { get; set; }
        public decimal ClaimGrandTotal { get; set; }
        public decimal ApprovedValue { get; set; }
        public decimal ApprovedGST { get; set; }
        public decimal ApprovedTotal { get; set; }
        public decimal ApprovedValueGrandTotal { get; set; }
        public decimal ApprovedGSTGrandTotal { get; set; }
        public decimal ApprovedGrandTotal { get; set; }
        public DateTime AMUApprovedDate { get; set; }
        public string SignatureUrl { get; set; }
        public string SignedName { get; set; }
        public DataTable Table
        {
            get
            {
                DataTable dt = new DataTable("RCTIDatax");
                dt.Columns.Add(new DataColumn("ID", typeof(int)));
                return dt;
            }


        }
    }
}