using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

public partial class campaign_prepayment : Root {

    protected void Page_Load(object sender, System.EventArgs e) {
        blnShowMenu = false;
        showData();
    }

    // for debuging
    public String outputDataSetCSV(DataTable dt) {
        // Only for debugging any user log in other than 'gord' will get no output
        if (G.User.UserID != 0)
            return "";

        List<string> ColumnNames = new List<string>();
        String output = "\n";

        foreach (DataColumn dc in dt.Columns)
            ColumnNames.Add(dc.ColumnName);

        output += String.Join(",", ColumnNames) + "\n";

        foreach (DataRow dr in dt.Rows) {
            foreach (String column in ColumnNames) {
                if (dr[column].ToString().Contains(','))
                    output += String.Format("\"{0}\",", dr[column]);
                else
                    output += String.Format("{0},", dr[column]);
            }
            output += "\n";
        }

        return output;
    }

    private void showData() {
        string szStartDate = Valid.getText("szStartDate", VT.TextNormal);
        string szEndDate = Valid.getText("szEndDate", VT.TextNormal);
        string szCampaignFilterSQL = @" WHERE ADDRESS1 NOT LIKE '%PROMO,%' ";
        hdCompanyID.Value = Valid.getText("szCompanyID", "", VT.TextNormal);

        if (szStartDate != "")
            szCampaignFilterSQL += " AND C.STARTDATE >= '" + szStartDate + "' ";

        if (szEndDate != "")
            szCampaignFilterSQL += " AND C.STARTDATE <= '" + szEndDate + "' ";

        DataTable dt = CampaignDB.loadPrePaymentCampaignData(szCampaignFilterSQL, hdCompanyID.Value, false);

        gvCurrent.DataSource = dt;
        gvCurrent.DataBind();

        HTML.formatGridView(ref gvCurrent);
    }
}