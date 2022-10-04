using System;
using System.Collections.Generic;
using System.Data;
using System.Web.UI;
using Paymaker;

public partial class email_notification : Page {
    private bool blnMorningEmail = false;

    protected void Page_init(object sender, EventArgs e) {
        if (Valid.getBoolean("AUTORUN", false)) {
            G.User.ID = -1;
            G.User.UserID = -1;
            G.User.RoleID = 1;
        }

        if (Valid.getBoolean("Check", false)) {
            HTML.addMinJS(this);
            ClientMenu oM = new ClientMenu();
            Form.Controls.AddAt(0, new LiteralControl(oM.createMenu()));
            dOutput.InnerHtml = TimesheetCycle.testAutomatedEmails(); 
        }
    }

    protected void Page_Load(object sender, EventArgs e) {
        if (Valid.getBoolean("AUTORUN", false) && !IsPostBack) {
            TimesheetCycle.checkAutomatedEmails();
            LeaveReminders.checkReminders();
        }
    }

    protected void btnSendMorning_Click(object sender, EventArgs e) {
        TimesheetCycle.checkAutomatedEmails();
        LeaveReminders.checkReminders();

    }
    
}