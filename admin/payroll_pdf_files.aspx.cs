using iTextSharp.tool.xml.html;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;

public partial class payroll_pdf_files : Root {
    protected string directories = "";
    protected string cDIRECTORYPATH = Path.Combine(G.Settings.DataDir, "PayrollTimeSheets\\");
    protected string cWEBPATH = "payroll_pdf.aspx?file=";

    protected void Page_Load(object sender, System.EventArgs e) {
        // Only Admin staff can access this page
        if (!G.User.IsAdmin) // Admin
            throw new Exception("Page restricted to only Admin staff");
        
        getLocalDirectoriesAndFiles(cDIRECTORYPATH, "timesheets_1", tvDirectoryList.Nodes[0]);
        getLocalDirectoriesAndFiles(cDIRECTORYPATH, "timesheets_2", tvTimesheet2.Nodes[0]);
    }

    private void getLocalDirectoriesAndFiles(string szPath, string FileFilter, TreeNode node) {
        int FileCount = 0;
        DirectoryInfo dirInfo = new DirectoryInfo(szPath);
        foreach (DirectoryInfo dir in dirInfo.GetDirectories()) {
            
            TreeNode newNode = new TreeNode("&nbsp;" + dir.Name);
            newNode.Value = dir.FullName;
            getLocalDirectoriesAndFiles(dir.FullName, FileFilter, newNode);
            node.ChildNodes.Add(newNode);
        }


        foreach (FileInfo file in dirInfo.GetFiles().OrderByDescending(f=>f.CreationTime)) {
            // Lists only files conforming to patern "Timesheet_<num>_<YYYYMMDD>.pdf"
            if (!Regex.IsMatch(file.Name, FileFilter + @"_20[0-9]{2}[0-1][0-9][0-3][0-9].pdf", RegexOptions.IgnoreCase))
                continue;
            FileCount++;
            TreeNode newNode = new TreeNode("&nbsp;" + file.Name);
            newNode.Value = file.FullName;
            if (Path.GetExtension(file.Name) == ".url") {
                newNode.NavigateUrl = getURLFromURI(file);
                newNode.Target = "_blank";
            } else {
                newNode.NavigateUrl = file.FullName.Replace(cDIRECTORYPATH, cWEBPATH);
            }
            node.ChildNodes.Add(newNode);
            if (!chkShowAll.Checked && FileCount > 20)
                break;
        }
    }

    /// <summary>
    /// Get the HREF of the file
    /// </summary>
    /// <param name="oFile"></param>
    /// <returns></returns>
    private string getURLFromURI(FileInfo oFile) {
        string text = System.IO.File.ReadAllText(oFile.FullName);

        if (text.Contains("URL=")) {
            text = text.Substring(text.IndexOf("URL=") + 4);
            text = text.Substring(0, text.IndexOf("\n"));
            return text.Trim();
        }

        return oFile.Name;
    }
}