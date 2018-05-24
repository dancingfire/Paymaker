using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class accounting_dashboard : Root {

    protected void Page_Load(object sender, System.EventArgs e) {
        if (!Page.IsPostBack) {
            Utility.loadPayPeriodList(ref lstPayPeriod);
            lstPayPeriod.Items.Insert(0, new ListItem("Current transactions", "CURRENT"));
        }
        showData();
    }

    private void showData() {
        Form.Controls.Add(new LiteralControl(HTML.createModalUpdate("Tx", "User transaction", "630px", 600, "tx_update.aspx")));
        Form.Controls.Add(new LiteralControl(HTML.createModalUpdate("AddTxCategory", "Tx category", "500px", 400, "../admin/list_update.aspx")));
        if (lstPayPeriod.SelectedValue == "CURRENT") {
            gvTXs.DataSource = UserTX.loadCurrentTXs(G.CurrentPayPeriodEnd, DateTime.MaxValue);
        } else if (lstPayPeriod.SelectedValue == "") {
            string szSQL = "SELECT MIN(STARTDATE) AS STARTDATE FROM PAYPERIOD";
            DataSet ds = DB.runDataSet(szSQL);
            foreach (DataRow dr in ds.Tables[0].Rows) {
                gvTXs.DataSource = UserTX.loadCurrentTXs(Convert.ToDateTime(dr["STARTDATE"]), DateTime.MaxValue);
            }
        } else if (Convert.ToInt32(lstPayPeriod.SelectedValue) == G.CurrentPayPeriod)
            gvTXs.DataSource = UserTX.loadCurrentTXs(G.CurrentPayPeriodStart, G.CurrentPayPeriodEnd);
        else {
            string szSQL = "SELECT * FROM PAYPERIOD WHERE ID = " + lstPayPeriod.SelectedValue;
            DataSet ds = DB.runDataSet(szSQL);
            foreach (DataRow dr in ds.Tables[0].Rows) {
                gvTXs.DataSource = UserTX.loadCurrentTXs(Convert.ToDateTime(dr["STARTDATE"]), Convert.ToDateTime(dr["ENDDATE"]));
            }
        }

        gvTXs.DataBind();
        HTML.formatGridView(ref gvTXs, true);
    }

    protected void gvCurrent_RowDataBound(object sender, GridViewRowEventArgs e) {
        if (e.Row.RowType == DataControlRowType.DataRow) {
            string szID = ((DataRowView)e.Row.DataItem)["ID"].ToString();
            e.Row.Attributes["onclick"] = "viewTx(" + szID + ")";
        }
    }

    protected void btnRefresh_Click(object sender, EventArgs e) {
    }
}