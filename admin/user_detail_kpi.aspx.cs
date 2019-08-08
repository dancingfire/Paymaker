using BootstrapWrapper;
using System;
using System.Data;

public partial class user_detail_kpi : Root {
    protected System.Data.SqlClient.SqlConnection sqlConn;
    protected DataSet dsTest;

    protected void Page_Load(object sender, System.EventArgs e) {
        if (!IsPostBack) {
            G.UserInfo.loadList(ref lstUser, true, false);
        }
    }

    protected void loadUser() {
        pDetails.Visible = lstUser.SelectedValue != "-1";
        string szSQL = string.Format(@"
            SELECT U.ID, U.INITIALSCODE + ' ' + U.FIRSTNAME + ' ' + U.LASTNAME AS NAME, R.NAME AS ROLE,
                PROFILEVIDEODATE, SHOWONKPIREPORT, SALESTARGET
            FROM DB_USER U
            JOIN ROLE R ON U.ROLEID = R.ID
            WHERE U.ID = {0}
            ORDER BY U.INITIALSCODE;

            SELECT * from USERKPIBUDGET WHERE USERID = {0};
            ", lstUser.SelectedValue);
        using (DataSet ds = DB.runDataSet(szSQL)) {
            foreach (DataRow dr in ds.Tables[0].Rows) {
                txtVideo.Text = DB.readDateString(dr["PROFILEVIDEODATE"]);
                txtSalesTarget.Text = DB.readString(dr["SALESTARGET"]);
                Utility.setListBoxItems(lstShowOnReport, DB.readBool(dr["SHOWONKPIREPORT"])?"1":"0");
            }
            foreach (DataRow dr in ds.Tables[1].Rows) {
                string Category = DB.readString(dr["CATEGORY"]);
                ((bwTextBox)Form.FindControl("txt" + Category)).Text = DB.readString(dr["VALUE"]);
            }
        }
        hdPrevUserID.Value = lstUser.SelectedValue;
    }

    protected void btnUpdate_Click(object sender, EventArgs e) {
        saveData(Convert.ToInt32(lstUser.SelectedValue));
    }

    private void saveData(int UserID) {
        if (UserID == -1)
            return;

        DateTime dtVideo = Valid.getDate("txtVIDEO", DateTime.MinValue);
        bool blnShowKPI = Valid.getCheck("chkSHOWONREPORT");
        string SalesTarget = Valid.getText("txtSalesTarget", "0");
        string szSQLUpdate = String.Format(@"
                UPDATE DB_USER
                    SET PROFILEVIDEODATE = {0},
                    SHOWONKPIREPORT = {1}, SALESTARGET= {3} WHERE ID = {2};

                DELETE FROM USERKPIBUDGET WHERE USERID = {2};
                ", dtVideo == DateTime.MinValue ? "null" : "'" + Utility.formatDate(dtVideo) + "'", lstShowOnReport.SelectedValue, UserID, SalesTarget);

        szSQLUpdate += getUpdateSQL(UserID, "APPRAISALSTOLISTINGS", txtAPPRAISALSTOLISTINGS.Text);
        szSQLUpdate += getUpdateSQL(UserID, "APPRAISALSPvsC", txtAPPRAISALSPvsC.Text);
        szSQLUpdate += getUpdateSQL(UserID, "APPRAISALS_ABC", txtAPPRAISALS_ABC.Text);
        szSQLUpdate += getUpdateSQL(UserID, "APPRAISALSCATEGORY_A", txtAPPRAISALSCATEGORY_A.Text);
        szSQLUpdate += getUpdateSQL(UserID, "APPRAISALS_FOLLOWUP", txtAPPRAISALS_FOLLOWUP.Text);
        szSQLUpdate += getUpdateSQL(UserID, "NUMBEROFCONTACTS", txtNUMBEROFCONTACTS.Text);
        szSQLUpdate += getUpdateSQL(UserID, "TOTALLISTINGS", txtTOTALLISTINGS.Text);
        szSQLUpdate += getUpdateSQL(UserID, "WITHDRAWNLISTINGS", txtWITHDRAWNLISTINGS.Text);
        szSQLUpdate += getUpdateSQL(UserID, "ADVERTISINGPERPROPERTY", txtADVERTISINGPERPROPERTY.Text);
        szSQLUpdate += getUpdateSQL(UserID, "PLACES", txtPLACES.Text);
        szSQLUpdate += getUpdateSQL(UserID, "GLOSSIESPERLISTING", txtGLOSSIESPERLISTING.Text);
        szSQLUpdate += getUpdateSQL(UserID, "GLOSSIESAVG", txtGLOSSIESAVG.Text);
        szSQLUpdate += getUpdateSQL(UserID, "PROPERTYVIDEO", txtPROPERTYVIDEO.Text);
        szSQLUpdate += getUpdateSQL(UserID, "AVGCOMMISION", txtAVGCOMMISION.Text);
        szSQLUpdate += getUpdateSQL(UserID, "AVGCOMMISIONPERCENT", txtAVGCOMMISIONPERCENT.Text);
        szSQLUpdate += getUpdateSQL(UserID, "AVGSALEPRICE", txtAVGSALEPRICE.Text);
        szSQLUpdate += getUpdateSQL(UserID, "AUCTIONCLEARANCE", txtAUCTIONCLEARANCE.Text);
        szSQLUpdate += getUpdateSQL(UserID, "NUMBEROFDAYSAUCTION", txtNUMBEROFDAYSAUCTION.Text);
        szSQLUpdate += getUpdateSQL(UserID, "NUMBEROFDAYSPRIVATE", txtNUMBEROFDAYSPRIVATE.Text);
        
        DB.runNonQuery(szSQLUpdate);
    }

    private string getUpdateSQL(int UserID, string Category, string Value) {
        string szReturn = String.Format(@"
           INSERT INTO USERKPIBUDGET(CATEGORY, USERID, VALUE) VALUES('{0}', {1}, '{2}');
            ", DB.escape(Category), UserID, DB.escape(Value));
        if (UserID != Convert.ToInt32(lstUser.SelectedValue)) {
            ((bwTextBox)Form.FindControl("txt" + Category)).Text = "";
        }
        return szReturn;
    }

    protected void lstUser_SelectedIndexChanged(object sender, EventArgs e) {
        saveData(Convert.ToInt32(hdPrevUserID.Value));
        loadUser();
    }
}