using FlexCel.Core;
using FlexCel.XlsAdapter;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;

namespace Paymaker {


    public partial class payroll_pdf : Root {
        private string Directory = G.Settings.FileDir + "PayrollTimeSheets\\";

        protected void Page_Load(object sender, System.EventArgs e) {
            /* SECURITY : DO NOT REMOVE
            * This code checks if user is logged in.  
            * This protects WebServices from being accessed by users not logged in. */
            int a = G.User.UserID;

            blnShowMenu = false;

            string file = Valid.getText("file", "", VT.NoValidation);

            // Check for dot dot slash attack (directory traversal)
            if (file.Contains("..\\") || file.Contains("../"))
                throw new Exception("Potential malicious file name");

            // Lists only files conforming to patern "Timesheet_<num>_<YYYYMMDD>.pdf"
            if (!Regex.IsMatch(file, @"timesheets_[0-9]_20[0-9]{2}[0-1][0-9][0-3][0-9].pdf", RegexOptions.IgnoreCase))
                throw new Exception("Invalid file type");

            byte[] bytes = File.ReadAllBytes(Directory + file);
            Response.Buffer = true;
            Response.Clear();
            Response.ContentType = "application/pdf";
            Response.AddHeader("content-disposition", "attachment; filename=" + file);
            Response.BinaryWrite(bytes);
            Response.Flush();
            Response.End();
        }
    }
}
