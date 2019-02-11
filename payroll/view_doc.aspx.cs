using System;
using System.IO;

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

            byte[] bytes = File.ReadAllBytes(Path.Combine(Directory, file));
            Response.Buffer = true;
            Response.Clear();
            Response.ContentType = "text/" + Path.GetExtension(file).ToLower();
            Response.ContentType = "text/plain";
            Response.AddHeader("content-disposition", @"attachment; filename=""" + file + @"""");
            Response.BinaryWrite(bytes);
            Response.Flush();
            Response.End();
        }
    }
}