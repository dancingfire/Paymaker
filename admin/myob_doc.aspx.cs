using FlexCel.Core;
using FlexCel.XlsAdapter;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;

namespace Paymaker {


    public partial class myob_doc : Root {
        private string Directory = G.Settings.MYOBDir;

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
            if (!szTest.EndsWith(".CSV") && !szTest.EndsWith(".TXT"))
                throw new Exception("Invalid file type");

            byte[] bytes = File.ReadAllBytes(Directory + file);
            Response.Buffer = true;
            Response.Clear();
            Response.ContentType = "text/csv";
            if (szTest.EndsWith(".TXT"))
                Response.ContentType = "text/plain";
            Response.AddHeader("content-disposition", @"attachment; filename=""" + file+ @"""");
            Response.BinaryWrite(bytes);
            Response.Flush();
            Response.End();
        }
    }
}
