using System;
using System.Data;
using System.Data.OleDb;
using System.Web;

public partial class import_values : Root {
    protected System.Data.SqlClient.SqlConnection sqlConn;
    protected DataSet dsSettings;

    protected void Page_Load(object sender, System.EventArgs e) {
        if (!Page.IsPostBack)
            loadData();
    }

    void loadData() {
        using (DataSet ds = DB.runDataSet(@"
                SELECT INITIALSCODE as Initials, Category, Amount
                FROM USERVALUE UV JOIN DB_USER U ON UV.USERID = U.ID
                ORDER BY INITIALSCODE, CATEGORY
            ")) {
            gvData.DataSource = ds;
            gvData.DataBind();
        }
    }

    protected void btnUpdate_Click(object sender, System.EventArgs e) {
        DB.runNonQuery("TRUNCATE TABLE USERVALUE");
        if (fuImport.HasFile) {
            foreach (HttpPostedFile uploadedFile in fuImport.PostedFiles) {
                uploadedFile.SaveAs(Server.MapPath("./upload.xlsx"));
                importFile(Server.MapPath("./upload.xlsx"));
            }
        }
        loadData();
    }

    private void importFile(string FilePath) {
        string szCNN = string.Format("Provider=Microsoft.ACE.OLEDB.12.0; Data Source={0}; Extended Properties='Excel 12.0; HDR=NO;IMEX=1'", FilePath);

        OleDbConnection oledbConn = new OleDbConnection(szCNN);
      
        // Open connection
        oledbConn.Open();

        // Create OleDbCommand object and select data from worksheet Sheet1
        OleDbCommand cmd = new OleDbCommand("SELECT * FROM [Sheet1$]", oledbConn);

        // Create new OleDbDataAdapter
        OleDbDataAdapter oleda = new OleDbDataAdapter();

        oleda.SelectCommand = cmd;

        // Create a DataSet which will hold the data extracted from the worksheet.
        DataSet ds = new DataSet();

        // Fill the DataSet from the data extracted from the worksheet.
        oleda.Fill(ds);
        oledbConn.Close();
        int intCount = 0;
        DataRow drHeader = null;
        foreach (DataRow dr in ds.Tables[0].Rows) {
            if(intCount > 0) {
                int UserID = DB.getScalar(String.Format("SELECT ID FROM DB_USER WHERE INITIALSCODE = '{0}' ", DB.escape(Convert.ToString(dr[0]))), -1) ;
                if (UserID == -1)
                    continue;
               
                for (int i = 1; i < 8; i++) {
                    string szValue = Convert.ToString(dr[i]);
                    szValue = szValue.Replace("$", "").Replace(",", "").Replace(" ", "");
                    if (szValue != "-" && szValue != "") {
                        importValue(UserID, Convert.ToString(drHeader[i]), szValue);
                    }
                }
            } else {
                drHeader = dr;
            }
            intCount++;
        }
    

      
        
    }


    void importValue(int UserID, string Header, string Value) {
       
        if (!String.IsNullOrWhiteSpace(Value)) {
            DB.runNonQuery(String.Format(@"
                    INSERT INTO USERVALUE(USERID, CATEGORY, AMOUNT)
                    VALUES({0}, '{1}', {2})", UserID, DB.escape(Header), DB.escape(Value)));
        }
    }

}