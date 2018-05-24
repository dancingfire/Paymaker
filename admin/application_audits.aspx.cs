using System;
using System.Data;

namespace Paymaker {

    /// <summary>
    /// Summary description for set_detail.
    /// </summary>
    public partial class application_audits : Root {
        //Declare all protected variables here

        protected string szLoginTrackerStartDate = "";
        protected string szLoginTrackerEndDate = "";

        protected void Page_Load(object sender, System.EventArgs e) {
            szLoginTrackerStartDate = txtLoginSearchStartDate.Text;
            szLoginTrackerEndDate = txtLoginSearchEndDate.Text;

            loadPage();
        }

        private void loadPage() {
            string szWhereClauseForLoginAttempts = "";
            if (txtLoginSearchEndDate.Text != "" && txtLoginSearchStartDate.Text != "") {
                if (txtLoginSearchEndDate.Text == txtLoginSearchStartDate.Text) {
                    DateTime DateStorer = Convert.ToDateTime(szLoginTrackerEndDate);
                    string dateOnly = DateStorer.ToString("dd/M/yyyy");
                    szWhereClauseForLoginAttempts = string.Format(@"
                      WHERE  CONVERT(VARCHAR(10),LL.LOGINDATE,103) = '{0}'", dateOnly);
                } else {
                    szWhereClauseForLoginAttempts = string.Format(@"
                     WHERE  LOGINDATE BETWEEN '{0}' AND '{1}'", szLoginTrackerStartDate, szLoginTrackerEndDate);
                }

                string szOrderByClauseForLoginAttempts = string.Format(@"ORDER BY LL.ID DESC");
                string szSQL = string.Format(@"
                   SELECT LL.ID,CONVERT(VARCHAR(10),LL.LOGINDATE,103) AS [DATE], LL.IPADDRESS,
				   CONVERT(VARCHAR(8),LL.LOGINDATE,108) AS [TIME],LL.USERNAME AS USERNAME,
				   CASE
				   WHEN LL.ISSUCCESS = 0
				   THEN 'Failed'
				   ELSE 'Success' END AS RESULT
				   FROM LOGINLOG LL
                    {0} {1} ", szWhereClauseForLoginAttempts, szOrderByClauseForLoginAttempts);

                gvList.DataSource = DB.runDataSet(szSQL); ;
                gvList.DataBind();
                HTML.formatGridView(ref gvList, true);
            }
        }
    }
}