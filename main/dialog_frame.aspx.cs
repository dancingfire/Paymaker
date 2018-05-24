using System;
using System.Collections;

public partial class dialog_frame : Root {
    private Hashtable htPages = new Hashtable();

    protected void Page_Init(object sender, EventArgs e) {
        blnShowMenu = false;
    }

    protected void Page_Load(object sender, EventArgs e) {
        string szPage = Valid.getText("szTargetPage", "undefined", VT.NoValidation);
        setPageTitle();
        addPages();
        //checkPage(szPage);
        fMain.Attributes["src"] = szPage + "?" + Request.QueryString.ToString();
    }

    private void setPageTitle() {
        string szTitle = Valid.getText("szTitle", "Dialog update", VT.NoValidation);
        int i = 0;
        while (i++ < 1000)
            szTitle += "&nbsp;";
        this.Title = szTitle;
    }

    private void checkPage(string szPage) {
        if (szPage.IndexOf('/') > -1)
            szPage = szPage.Substring(szPage.LastIndexOf('/') + 1);

        //Check to see if this is one of the allowed pages
        bool blnFound = htPages.ContainsKey(szPage);

        if (!blnFound) {
            Response.Write("Invalid input." + szPage);
            Response.End();
        }
    }

    private void addPages() {
        // Add  all the allowable pages.
        addPage("permissions_update.aspx'");
        addPage("price_list_billing_group_update.aspx'");
        addPage("price_list_test_update.aspx'");
    }

    private void addPage(string szPage) {
        htPages.Add(szPage, szPage);
    }
}