using System;
using System.Data;
using System.Data.SqlClient;

public partial class pay_period_detail : Root {
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

    protected void btnCreate_Click(object sender, EventArgs e) {
        string szSQL = String.Format("INSERT INTO PAYPERIOD(STARTDATE, ENDDATE) VALUES('{0}', '{1}') ", Utility.formatDate(txtStartDate.Text), Utility.formatDate(txtEndDate.Text));
        DB.runNonQuery(szSQL);
        G.CurrentPayPeriod = DB.getScalar("SELECT MAX(ID) FROM PAYPERIOD", 0);
        G.CurrentPayPeriodStart = Convert.ToDateTime(txtStartDate.Text);
        G.CurrentPayPeriodEnd = Convert.ToDateTime(txtEndDate.Text);
        loadCurrent();
        G.PayPeriodInfo.forceReload();
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

    protected void btnUpdateWorkingPayPeriod_Click(object sender, EventArgs e) {
        string szSQL = "SELECT TOP 1 ID, STARTDATE, ENDDATE FROM PAYPERIOD WHERE ID = " + lstPayPeriod.SelectedValue;
        SqlDataReader dr = DB.runReader(szSQL);
        while (dr.Read()) {
            G.CurrentPayPeriod = Convert.ToInt32(dr["ID"]);
            G.CurrentPayPeriodStart = Convert.ToDateTime(dr["STARTDATE"].ToString());
            G.CurrentPayPeriodEnd = Convert.ToDateTime(dr["ENDDATE"].ToString());
        }
        dr.Close();
        dr = null;
    }
}