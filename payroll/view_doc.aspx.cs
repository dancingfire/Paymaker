using System;
using System.Configuration;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace Paymaker {

    public partial class view_doc : Root {
        private string Directory = G.Settings.DataDir;

        protected void Page_Load(object sender, System.EventArgs e) {
            /* SECURITY : DO NOT REMOVE
            * This code checks if user is logged in.
            * This protects WebServices from being accessed by users not logged in. */
            int a = G.User.UserID;
           
            string file = Valid.getText("file", "", VT.NoValidation);

            // Check for dot dot slash attack (directory traversal)
            if (file.Contains("..\\") || file.Contains("../"))
                throw new Exception("Potential malicious file name");

            string szTest = file.ToUpper();
           
            // Lists only files that are *.txt or *.csv
            if (!szTest.EndsWith(".JPG") && !szTest.EndsWith(".PDF"))
                throw new Exception("Invalid file type");
            string connectionString = ConfigurationManager.AppSettings["StorageConnectionString"];

            byte[] bytes = File.ReadAllBytes(Directory + "/"+ file);
          
            Response.Buffer = true;
            Response.Clear();
            if (szTest.EndsWith(".PDF")) {
                Response.ContentType = "application/pdf";
            } else {
                Response.ContentType = "text/" + Path.GetExtension(file).ToLower();
            }

            Response.AddHeader("content-disposition", @"attachment; filename=""" + file + @"""");
            Response.BinaryWrite(bytes);
            Response.Flush();
            Response.End();
        }
    }
}