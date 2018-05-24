using System;
using System.Configuration;
using System.Data;
using System.Data.Odbc;

public partial class reoffice_data : Root {

    protected void Page_Load(object sender, System.EventArgs e) {
        showData();
    }

    private void showData() {
        string szCNN = ConfigurationManager.AppSettings["REOFFICE"];
        string szSQL = String.Format(@"
            SELECT BU_BUSINESSID, BU_CODE,BU_NAME AS ADDRESS, BU_SALEENTERED AS SALEDATE, BU_ENTITLEMENTDATES AS ENTITLEMENTDATE, BU_SALEPRICE AS SALEPRICE,
            BU_GROSSCOMMISSION,BU_CONJUNCTIONCOMMISSION AS CONJUNCTIONALCOMMISSION,BU_ACTUALSETTLEMENT ,BR_CONJAGENT1FULLNAME AS CONJUNCTIONALAGENT, BU_ACTUALSETTLEMENT
            FROM BUSINESS, BUSINESSRELATION
             WHERE BU_TYPEID IN (6,12,65) AND BU_SALEENTERED >='01/01/2011'
              AND BR_BUSINESSID = BU_BUSINESSID
            ");

        OdbcDataAdapter da = new OdbcDataAdapter(szSQL, szCNN);
        DataSet dsImport = new DataSet();
        da.Fill(dsImport, "emp");
        gvCurrent.DataSource = dsImport;
        gvCurrent.DataBind();
    }

    protected void btnShow_Click(object sender, EventArgs e) {
        showData();
    }
}