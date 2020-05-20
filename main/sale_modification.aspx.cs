using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class sale_modification : Root {

    protected void btnSearch_Click(object sender, EventArgs e) {
        pDetails.Visible = false;
        loadSalesData();
        pDetails.Visible = true;
    }

    private void loadSalesData() {
        string szSQL = String.Format(@"
            SELECT DATENAME(m, P.STARTDATE) AS PAYPERIOD, S.*, ISNULL(S.PAYPERIODID, -1) AS SAFEPAYPERIODID
            FROM SALE S LEFT JOIN PAYPERIOD P ON S.PAYPERIODID = P.ID
            WHERE CODE LIKE '%{0}%' or address like '%{0}%'", DB.escape(txtPropertyFilter.Text));
        DataSet ds = DB.runDataSet(szSQL);
        gvSales.DataSource = ds;
        gvSales.DataBind();
        if (ds.Tables[0].Rows.Count == 0) {
            G.Notifications.addPageNotification(PageNotificationType.Error, "Property not found", "A property containing this code was not found", false);
            G.Notifications.showPageNotification();
        }
        HTML.formatGridView(ref gvSales);
        Form.Controls.Add(new LiteralControl(HTML.createModalUpdate("Sale", "Sale", "65%", 650, "sale_update_special.aspx")));
    }

    public string getStatus(int StatusID) {
        string szReturn = "Incomplete";
        if (StatusID == 1)
            szReturn = "Complete";
        else if (StatusID == 2)
            szReturn = "Finalized";
        else if (StatusID == 3)
            szReturn = "Hidden";
        return szReturn;
    }

    public string getLocked(int IsLocked) {
        if (IsLocked == 1)
            return "Yes";
        return "No";
    }

    protected void gvSales_RowDataBound(object sender, GridViewRowEventArgs e) {
        if (e.Row.RowType == DataControlRowType.DataRow) {
            string szID = ((DataRowView)e.Row.DataItem)["ID"].ToString();
            e.Row.Attributes["onclick"] = "viewSale(" + szID + ")";
        }
    }
}