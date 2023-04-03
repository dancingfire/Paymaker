using System;
using System.Data;
using System.Data.SqlClient;

public partial class pay_period_delete : Root {
    private ListType oListType = ListType.Office;

    protected void Page_Load(object sender, System.EventArgs e) {
        oListType = (ListType)Valid.getInteger("intListTypeID");
        sbStartJS.Append("var intListTypeID = " + (int)oListType);
        if (!IsPostBack) {
            loadFilters();
            loadCurrent();
        }
        if (G.User.ID == 0) {
            oSetPayPeriod.Visible = true; 
        }
    }

    private void loadCurrent() {
        string szSQL = "SELECT TOP 1 * FROM PAYPERIOD ORDER BY ID DESC";
        SqlDataReader dr = DB.runReader(szSQL);
        while (dr.Read()) {
            lblCurrentPayPeriod.Text = Utility.formatDate(dr["STARTDATE"].ToString()) + " - " + Utility.formatDate(dr["ENDDATE"].ToString());
        }
        dr.Close();
        dr = null;
    }

    protected void loadFilters() {
        string szSQL = string.Format("select ID, STARTDATE , ENDDATE, '' AS NAME from PAYPERIOD ORDER BY ID DESC");
        DataSet ds = DB.runDataSet(szSQL);
        foreach (DataRow dr in ds.Tables[0].Rows) {
            dr["NAME"] = Utility.formatDate(dr["StartDate"].ToString()) + " - " + Utility.formatDate(dr["ENDDATE"].ToString());
        }
        Utility.BindList(ref lstPayPeriod, ds, "ID", "NAME");
    }

    protected void btnDelete_Click(object sender, EventArgs e) {
        string szSQL = String.Format("DELETE FROM PAYPERIOD WHERE ID = {0}", lstPayPeriod.SelectedValue);
        DB.runNonQuery(szSQL);
        loadCurrent();
        G.PayPeriodInfo.forceReload();
    }
}