using System;
using System.Data;

public partial class log_detail : Root {

    protected void Page_Load(object sender, System.EventArgs e) {
        if (!IsPostBack) {
        }
    }

    protected void loadData() {
        string szSQL = "";
        if (lstType.SelectedIndex == 0) {
            szSQL = String.Format(@"
                SELECT *, U.FIRSTNAME + ' ' + U.LASTNAME AS PERSON, S.CODE + ' ' + S.ADDRESS AS INFO
                FROM LOG L JOIN DB_USER U ON L.USERID = U.ID
                JOIN SALE S ON S.ID = L.PRIMARYKEYID
                WHERE L.TYPEID IN ({0}) AND (S.ADDRESS LIKE '%{0}%' OR S.CODE LIKE '%{0}%')
                ORDER BY CHANGEDATE DESC
                ", DB.escape(txtSearch.Text), lstType.SelectedValue);
        } else {
            szSQL = string.Format(@"
                SELECT *, U.FIRSTNAME + ' ' + U.LASTNAME AS PERSON, C.ORIGCAMPAIGNNUMBER + ' ' + C.ADDRESS1  AS INFO
                FROM LOG L JOIN DB_USER U ON L.USERID = U.ID
                JOIN CAMPAIGN C ON C.ID = L.PRIMARYKEYID
                 WHERE (C.ADDRESS1 LIKE '%{0}%' OR C.ADDRESS2 LIKE '%{0}%' OR C.ORIGCAMPAIGNNUMBER LIKE '%{0}%')
                AND TYPEID IN ({1})
                ORDER BY CHANGEDATE DESC", DB.escape(txtSearch.Text), lstType.SelectedValue);
        }

        DataSet dsList = DB.runDataSet(szSQL);
        gvLog.DataSource = dsList;
        gvLog.DataBind();
        HTML.formatGridView(ref gvLog, true);
    }

    protected void btnSearch_Click(object sender, EventArgs e) {
        loadData();
    }
}