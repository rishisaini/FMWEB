using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Data;
using System.ComponentModel;
using System.Net.Http;
using System.Net;
using System.Net.Http.Headers;
using OfficeOpenXml;
using System.Text;
using ServiceStack;

namespace AssetCare.Common
{
    public class ExcelHelper
    {
        public HttpResponseMessage GenerateExcelDocument<T>(IList<T> data, int exportType, string fileName = "data")
        {
            DataTable dtData = this.ToDataTable(data);
            ExcelPackage.License.SetNonCommercialOrganization("NSW Gov DoE");
            var result = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(exportType == 1 ? GenerateExcelData(dtData) : GenerateCSVData(dtData))
            };
           

            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = (exportType == 1 ? String.Format("{0}.xlsx", fileName) : String.Format("{0}.csv", fileName))
            };
            result.Content.Headers.ContentType = exportType == 1 ? new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet") : new MediaTypeHeaderValue("text/csv");
            
            return result;
        }
        

        private byte[] GenerateExcelData(DataTable dt)
        {
            using (var stream = new MemoryStream())
            {


                using (var package = new OfficeOpenXml.ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Data");
                    worksheet.Cells["A1"].LoadFromDataTable(dt, true);

                    int colNumber = 1;

                    foreach (DataColumn col in dt.Columns)
                    {
                        if (col.DataType == typeof(DateTime))
                        {
                            worksheet.Column(colNumber).Style.Numberformat.Format = "dd/MM/yyyy";
                        }
                        colNumber++;
                    }

                    // auto size columns
                    worksheet.Cells.AutoFitColumns();

                    package.SaveAs(stream);
                }

                return stream.ToArray();
            }
        }

        private byte[] GenerateCSVData(DataTable dt)
        {
            var columns = dt.Columns;
            string[] columnNames = columns.Cast<DataColumn>().
                                              Select(column => column.ColumnName).
                                              ToArray();
            using (MemoryStream ms = new MemoryStream())
            {
                using (StreamWriter sw = new StreamWriter(ms, Encoding.UTF8))
                {
                    sw.WriteLine(string.Join(",", columnNames));

                    foreach (DataRow row in dt.Rows)
                    {
                        string[] fields = row.ItemArray.Select(field => AssetCareFormatString(field, false)).
                                                        ToArray();
                        sw.Write(fields.ToCsv());
                    }

                }
                return ms.ToArray();
            }
        }

        private string AssetCareFormatString(object colValue, bool oldFormat)
        {
            if ((colValue != null) && colValue.GetType() != typeof(DBNull))
            {
                var dataType = colValue.GetType();
                if (typeof(DateTime) == dataType || typeof(DateTime?) == dataType)
                    return oldFormat ? ((DateTime)colValue).ToString("dd/MM/yyyy") : ((DateTime)colValue).ToString("dd/MM/yyyy HH:mm");
                if (typeof(decimal) == dataType || typeof(decimal?) == dataType)
                {
                    return string.Format("{0:n}", (decimal)colValue);
                }
                if (typeof(int) == dataType || typeof(int?) == dataType || typeof(long) == dataType || typeof(long?) == dataType)
                {
                    return colValue.ToString();
                }
                return colValue.ToString();
            }

            return oldFormat ? "N/A" : string.Empty;
        }
    private DataTable ToDataTable<T>(IList<T> data)
        {
            PropertyDescriptorCollection properties =
                TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            foreach (PropertyDescriptor prop in properties)
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            foreach (T item in data)
            {
                DataRow row = table.NewRow();
                foreach (PropertyDescriptor prop in properties)
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                table.Rows.Add(row);
            }
            return table;
        }
    }
}