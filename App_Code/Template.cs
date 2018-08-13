using System;
using System.Data.SqlClient;

/// <summary>
/// Summary description for Template
/// </summary>
public class Template {
    private int intID = -1;
    private string szTemplateHTML = "";
    private string szName = "";
    private string szDescription = "";
    private string szFilledTemplate = "";

    public Template(int ID) {
        intID = ID;
        loadTemplate();
    }

    public void fillTemplate(int CampaignID, int CurrentUserID) {
        Campaign oC = new Campaign(CampaignID);
        szFilledTemplate = szTemplateHTML;
        string szSQL = "SELECT LASTNAME + '*****' + FIRSTNAME FROM DB_USER WHERE ID = " + CurrentUserID;
        string szUserName = DB.getScalar(szSQL, "*****");
        string[] arUserName = Utility.SplitByString(szUserName, "*****");
        string szCurrUserFirstName = arUserName[1];
        string szCurrUserLastName = arUserName[0];
        string szAgentFirstName = arUserName[1];
        string szAgentLastName = arUserName[0];
        if (CampaignID > -1) {
            szSQL = "SELECT LASTNAME + '*****' + FIRSTNAME FROM DB_USER U JOIN CAMPAIGN C ON C.AGENTID = U.ID WHERE C.ID = " + CampaignID;
            szUserName = DB.getScalar(szSQL, "*****");
            arUserName = Utility.SplitByString(szUserName, "*****");
            szAgentFirstName = arUserName[1];
            szAgentLastName = arUserName[0];
        }
        szFilledTemplate = szFilledTemplate.Replace("[AGENTFIRSTNAME]", szAgentFirstName);
        szFilledTemplate = szFilledTemplate.Replace("[AGENTLASTNAME]", szAgentLastName);
        szFilledTemplate = szFilledTemplate.Replace("[SENDERFIRSTNAME]", szCurrUserFirstName);
        szFilledTemplate = szFilledTemplate.Replace("[SENDERLASTNAME]", szCurrUserLastName);
        szFilledTemplate = szFilledTemplate.Replace("[PROPERTYDETAILS]", oC.Address);
        szFilledTemplate = szFilledTemplate.Replace("[OUSTANDINGBALANCE]", Utility.formatMoney(oC.AmountOwing));
        szFilledTemplate = szFilledTemplate.Replace("[AMOUNTEXCEEDINGAUTHORITY]", Utility.formatMoney(-1 * oC.BudgetRemaining));
        szFilledTemplate = szFilledTemplate.Replace("[TOTALPAID]", Utility.formatMoney(oC.TotalPaid));
        szFilledTemplate = szFilledTemplate.Replace("[TOTALSPEND]", Utility.formatMoney(oC.ActualSpent));
        szFilledTemplate = szFilledTemplate.Replace("[TOTALINVOICED]", Utility.formatMoney(oC.TotalInvoices));
        string szInvoiceDetails = "";
        foreach (CampaignInvoice oI in oC.InvoiceList) {
            szInvoiceDetails += String.Format(@"
                <tr>
                    <td>{0}</td>
                    <td>{1}</td>
                    <td align='right'>{2}</td>
                </tr>", oI.InvoiceNumber, Utility.formatDate(oI.Date), Utility.formatMoney(oI.Amount));
        }
        if (!String.IsNullOrWhiteSpace(szInvoiceDetails)) {
            szInvoiceDetails = @"
                <table width='100%'>
                    <thead>
                        <th width='40%'>Invoice number</th>
                        <th width='30%'>Date</th>
                        <th width='30%'>Amount</th>
                </thead>" + szInvoiceDetails + "</table>";
        }
        szFilledTemplate = szFilledTemplate.Replace("[INVOICEDETAILS]", szInvoiceDetails);
    }

    private void loadTemplate() {
        string szSQL = "SELECT * FROM TEMPLATE WHERE ID = " + intID;
        using (SqlDataReader dr = DB.runReader(szSQL)) {
            while (dr.Read()) {
                szTemplateHTML = dr["CONTENT"].ToString();
                szDescription = dr["DESCRIPTION"].ToString();
                szName = dr["NAME"].ToString();
            }
        }
    }

    public int ID {
        get { return intID; }
    }

    public string FilledTemplate {
        get { return szFilledTemplate; }
    }

    public string TemplateHTML {
        get { return szTemplateHTML; }
    }

    public string Name {
        get { return szName; }
    }

    public string Description {
        get { return szDescription; }
    }
}