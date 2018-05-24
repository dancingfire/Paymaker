using System;
using System.IO;
using System.Web.UI.WebControls;
using System.Web;


public partial class myob_files : Root {
    protected string directories = "";
    protected string cDIRECTORYPATH = G.Settings.MYOBDir;
    protected string cWEBPATH = "myob_doc.aspx?file=";

    protected void Page_Load(object sender, System.EventArgs e) {
        // Check access privilege -- user should not be able to access this page if they do not have correct privilege
        if (!G.User.hasCampaignAccess) 
            throw new Exception("User does not have access to this page");

        if (!IsPostBack) {
            getLocalDirectoriesAndFiles(cDIRECTORYPATH, tvDirectoryList.Nodes[0]);
        }
    }

    private void getLocalDirectoriesAndFiles(string szPath, TreeNode node) {
        DirectoryInfo dirInfo = new DirectoryInfo(szPath);
        foreach (DirectoryInfo dir in dirInfo.GetDirectories()) {
            TreeNode newNode = new TreeNode("&nbsp;" + dir.Name);
            newNode.Value = dir.FullName;
            getLocalDirectoriesAndFiles(dir.FullName, newNode);
            node.ChildNodes.Add(newNode);
        }

        foreach (FileInfo file in dirInfo.GetFiles()) {
            string szTest = file.Name.ToUpper();

            // Lists only files that are *.txt or *.csv
            if (!szTest.EndsWith(".CSV") && !szTest.EndsWith(".TXT"))
                continue;

            TreeNode newNode = new TreeNode("&nbsp;" + file.Name);
            newNode.Value = file.FullName;

            // File name needs to be UrlEncoded to handle characters such as '&'
            String szFileName = file.FullName.Replace(cDIRECTORYPATH, "");
            newNode.NavigateUrl = cWEBPATH + Server.UrlEncode(szFileName);

            node.ChildNodes.Add(newNode);
        }
    }
}