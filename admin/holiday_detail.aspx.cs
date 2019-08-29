using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Web.UI.WebControls;

public partial class holiday_detail : Root {

    protected void Page_Load(object sender, System.EventArgs e) {
        if (!IsPostBack) {
            loadData();
        }
    }

    protected void loadData() {
        string szSQL = string.Format(@"
            SELECT *
            FROM PUBLICHOLIDAY

            ORDER BY HOLIDAYDATE DESC");
        DataSet dsList = DB.runDataSet(szSQL);
        gvList.DataSource = dsList;
        gvList.DataBind();
        HTML.formatGridView(ref gvList, true);
    }

    protected void gvList_RowDataBound(object sender, GridViewRowEventArgs e) {
        if (e.Row.RowType == DataControlRowType.DataRow) {
            string szID = ((DataRowView)e.Row.DataItem)["ID"].ToString();

            e.Row.Attributes["onclick"] = "viewItem(" + szID + ")";
        }
    }

    protected void btnUploadFile_Click(object sender, System.EventArgs e) {
        if (FileUpload1.HasFile) {
            using (StreamReader sr = new StreamReader(FileUpload1.FileContent)) {
                string ical = sr.ReadToEnd();
                char[] delim = { '\n' };
                string[] lines = ical.Split(delim);
                string Name = "";
                DateTime dt = DateTime.MinValue;
                delim[0] = ':';
                for (int i = 0; i < lines.Length; i++) {
                    if (lines[i].Contains("SUMMARY:")) {
                        Name = lines[i].Replace("SUMMARY:", "");
                    }
                    if (lines[i].Contains("DTSTART;VALUE=DATE:")) {
                        string stString = lines[i].Replace("DTSTART;VALUE=DATE:", "");
                        stString = stString.Replace("\r", "");
                        dt = DateTime.ParseExact(stString, "yyyyMd", CultureInfo.InvariantCulture);
                        int DBID = DB.getScalar(String.Format(@"
                            SELECT ID
                            FROM PUBLICHOLIDAY WHERE NAME ='{0}' AND HOLIDAYDATE = '{1}'
                            ", DB.escape(Name), Utility.formatDate(dt)), -1);
                        if (DBID == -1) {
                            DB.runNonQuery(String.Format(@"
                                INSERT INTO PUBLICHOLIDAY(NAME, HOLIDAYDATE) 
                                VALUES('{0}', '{1}');          
                            ", DB.escape(Name), Utility.formatDate(dt)));
                        }
                    }
                }
            }
        }
        loadData();
    }
}