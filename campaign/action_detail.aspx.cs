using System;
using System.Data;
using System.Net.Mail;
using System.Web.UI;
using System.Web.UI.WebControls;

/// <summary>
/// Template admin
/// </summary>
public partial class action_detail : Root {
    private int intSelID = -1;
    private int intCampaignID = 0;
    private int intCampaignNoteID = 0;

    protected void Page_Load(object sender, EventArgs e) {
        blnShowMenu = false;
        txtPageHeader.Text = "Action history";
        intCampaignID = Valid.getInteger("intCampaignID");
        intCampaignNoteID = Valid.getInteger("intCampaignNoteID");
        HDCampaignID.Value = intCampaignID.ToString();
        HDCurrentUserID.Value = G.User.ID.ToString();
        if (!Page.IsPostBack) {
            loadActionList();
            loadCampaignNote();
        }
    }

    private void loadCampaignNote() {
        if (intCampaignNoteID == -1) {
            txtPageHeader.Text = "Create an action";
            return;
        }
        DataSet dsNote = DB.runDataSet(String.Format(@"
            SELECT CN.*,  ISNULL(AC.NAME, '') AS ACTION, U.FIRSTNAME + ' ' + U.LASTNAME AS ADMINUSER, A.FIRSTNAME + ' ' + A.LASTNAME AS AGENTUSER
            FROM CAMPAIGNNOTE CN
            LEFT JOIN DB_USER U ON U.ID = CN.USERID
            LEFT JOIN DB_USER A ON A.ID = CN.AGENTID
            LEFT JOIN ACTION AC ON AC.ID = CN.ACTIONID
            WHERE CN.CAMPAIGNID = {0}
            ORDER BY ACTIONID, NOTEDATE ASC
            ", intCampaignID));
        int intActionID = -1;
        bool blnIsFirst = true;
        foreach (DataRow row in dsNote.Tables[0].Rows) {
            if (intActionID != Convert.ToInt32(row["ACTIONID"])) {
                intActionID = Convert.ToInt32(row["ACTIONID"]);
                dDetails.InnerHtml += "<div class='ActionHeader'>" + row["ACTION"].ToString() + "</div>";
            }
            int intIndent = 0;
            int intEmailWidth = 600;
            /*if (Convert.ToInt32(row["ParentNoteID"]) > -1) {
                intIndent = 50;
                intEmailWidth = 350;
            }*/
            string szReminder = "";
            if (blnIsFirst) {
                blnIsFirst = false;
                //Output the reminder date only for the first email in the history and set this as the parent note ID
                string szReminderDate = "";
                if (row["REMINDER"] != System.DBNull.Value)
                    szReminderDate = Utility.formatDate(Convert.ToDateTime(row["REMINDER"]));
                szReminder = String.Format(@"
                    <br/><br/><b>Reminder</b><br /><input type='text' id='txtOrigReminder' value='{0}' class='Entry' style='width: 90px' onchange='updateReminderDate()'/>
                    <a href='javascript: clearReminderDate()'>Clear reminder</a>
                    <script type='text/javascript'>createCalendar('txtOrigReminder');</script>", szReminderDate);

                //Set the defaults for the email that we are replying to
                HDCampaignNoteID.Value = row["ID"].ToString();
                HDCurrentNoteUserID.Value = row["AGENTID"].ToString();
                Utility.setListBoxItems(ref lstActionID, Convert.ToString(row["ACTIONID"]));
                txtContent.Text = Convert.ToString(row["CONTENT"]);
            }
            dDetails.InnerHtml += String.Format(@"
                <div class='Email'>
                    <table>
                        <tr valign='top'>
                            <td width='{4}'>&nbsp;</td>
                            <td class='EmailDetails' width='120'>
                                <b>{1}</b><br/>
                                <i>{0}</i><br/>
                                {6}
                            </td>
                            <td width='{5}'>
                                <div class='EmailMessage'>
                                {3}
                            </td>
                        </tr>
                    </table>
                </div>
                ", Utility.formatDate(Convert.ToDateTime(row["NOTEDATE"])), row["ADMINUSER"].ToString(), row["AGENTUSER"].ToString(), Convert.ToString(row["CONTENT"]), intIndent,
                 intEmailWidth, szReminder);
        }
    }

    protected void loadActionList() {
        lstActionID.Items.Clear();
        DataSet dsActions = DB.runDataSet("SELECT * FROM ACTION WHERE ISACTIVE = 1 ORDER BY NAME");

        string szHTML = "";
        foreach (DataRow row in dsActions.Tables[0].Rows) {
            int intTemplateID = Convert.ToInt32(row["TEMPLATEID"]);
            int intDefaultDays = Convert.ToInt32(row["DEFAULTREMINDERDAYS"]);

            szHTML += String.Format("addAction({0},{1}, {2}, '{3}');", Convert.ToInt32(row["ID"]), intTemplateID, intDefaultDays, Utility.EscapeJS(row["EMAILSUBJECT"].ToString()));
        }
        //Copy the action details to the page
        ClientScript.RegisterClientScriptBlock(this.GetType(), "Action", szHTML, true);
        Utility.BindList(ref lstActionID, dsActions, "ID", "NAME");
        lstActionID.Items.Insert(0, new ListItem("Select a action...", "-1"));
        lstActionID.Attributes["onchange"] = "changeActionList()";
    }

    protected void createNote(bool SendEmail) {
        Campaign oC = new Campaign(intCampaignID);
        sqlUpdate oSQL = new sqlUpdate("CAMPAIGNNOTE", "ID", -1);
        string szEmailText = Request.Form["txtContent"];
        if (oC.AgentID == G.User.ID) {
            //This is the current agent replying, so we can append information that identifies the property to the email
            szEmailText += "<br><br><b>Property details</b><p>Campaign #: " + oC.PropertyRef + "<br/>Address: " + oC.Address + "</p>";
        }
        oSQL.add("CONTENT", szEmailText);
        oSQL.add("CAMPAIGNID", intCampaignID);
        oSQL.add("NOTEDATE", Utility.formatDateTime(DateTime.Now));
        oSQL.add("AGENTID", oC.AgentID);
        oSQL.add("ACTIONID", Valid.getText("lstActionID", VT.TextNormal));
        oSQL.add("USERID", G.User.ID);
        if (txtReminder.Text != "") {
            oSQL.add("REMINDER", txtReminder.Text);
            //Clear out all other reminder dates - we can only have one active reminder date.
            DB.runNonQuery("UPDATE CAMPAIGNNOTE SET REMINDER = null WHERE CAMPAIGNID = " + intCampaignID);
        }
        oSQL.add("PARENTNOTEID", HDCampaignNoteID.Value);
        if (intSelID == -1) {
            DB.runNonQuery(oSQL.createInsertSQL());
        } else {
            DB.runNonQuery(oSQL.createUpdateSQL());
        }
        if (SendEmail)
            sendEmail(oC, szEmailText);
    }

    protected void btnSend_Click(object sender, EventArgs e) {
        createNote(true);

        ClientScript.RegisterClientScriptBlock(GetType(), "Close", "<script>Close();</script>");
    }

    protected void btnPrint_Click(object sender, EventArgs e) {
        createNote(false);
        dPrintContent.InnerHtml = Request.Form["txtContent"];

        dPrint.Visible = true;
        dHistory.Visible = false;
        dPageHeader.Visible = false;
        dEmailPanel.Visible = false;
    }

    private void sendEmail(Campaign oC, string EmailBody) {
        MailMessage msg = new MailMessage();
        msg.IsBodyHtml = true;
        string szTo = DB.getScalar("SELECT EMAIL FROM DB_USER WHERE ID = " + oC.AgentID, "");
        if (szTo.Contains(";")) {
            string[] szEmails = szTo.Split(';');
            foreach (string szEmailAddress in szEmails) {
                msg.To.Add(szEmailAddress);
            }
        } else {
            msg.To.Add(szTo);
        }

        msg.CC.Add(G.User.Email);

        msg.From = new MailAddress(G.User.Email);
        msg.Subject = txtSubject.Text;
        msg.Body = EmailBody;
        SmtpClient oSMTP = Email.getEmailServer();

        try {
            oSMTP.Send(msg);
        } catch {
            throw;
        } finally {
            oSMTP = null;
        }
    }
}