using System;
using System.Configuration;
using System.Data;
using System.Data.Odbc;

namespace Paymaker {

    /// <summary>
    /// Summary description for about.
    /// </summary>
    public partial class db_test : Root {
        protected System.Web.UI.WebControls.Button Button2;

        protected void Page_Load(object sender, System.EventArgs e) {
            string szCNN = ConfigurationManager.AppSettings["REOFFICE"];
            string szSQL = String.Format(@"
            SELECT BU_CODE,BU_NAME AS ADDRESS, BU_SALEENTERED AS SALEDATE, BU_ENTITLEMENTDATES AS ENTITLEMENTDATE, CASE BU_ENTITLEMENTDATES WHEN '' THEN '1/1/1900' ELSE BU_ENTITLEMENTDATES END AS ENTITLEMENTDATE
            FROM BUSINESS
             WHERE BU_CODE = 'JACK23' and BU_ENTITLEMENTDATES = ''

            ");

            OdbcDataAdapter da = new OdbcDataAdapter(szSQL, szCNN);
            DataSet dsImport = new DataSet();
            da.Fill(dsImport, "emp");

            // BU_LASTACTIONDATE = last updated date
            foreach (DataRow dr in dsImport.Tables[0].Rows) {
                Response.Write("***" + dr["ENTITLEMENTDATE"].ToString() + "***");
            }
        }
    }
}