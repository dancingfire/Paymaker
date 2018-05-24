using System;
using System.Collections.Generic;
using System.Data;
using System.Web.UI;

public partial class email_notification : Page {
    private bool blnMorningEmail = false;

    protected void Page_init(object sender, EventArgs e) {
        if (Valid.getBoolean("AUTORUN", false)) {
            G.CurrentUserID = -1;
            G.User.UserID = -1;
            G.CurrentUserRoleID = 1;
        }

        if (Valid.getBoolean("Check", false)) {
            TimesheetCycle.testAutomatedEmails();
        }

    }

    protected void Page_Load(object sender, EventArgs e) {
        if (Valid.getBoolean("AUTORUN", false) && !IsPostBack) {
           TimesheetCycle.checkAutomatedEmails();
        }
    }

    protected void btnSendMorning_Click(object sender, EventArgs e) {
        TimesheetCycle.checkAutomatedEmails();
    }
    
}