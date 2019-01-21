using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

public enum MenuRole {
    Everyone = 0,
    Admin = 1,
    UserOnly = 2,
    Campaign = 3,
    Commission = 4
}

/// <summary>
/// Creates and draw the menu for the client
/// </summary>

public class ClientMenu {
    private Menu oM = new Menu("");

    public ClientMenu() {
        oM.addMenu("Home", "", MenuRole.Admin);

        oM.addMenuItem("View dashboard", "../main/sales_dashboard.aspx");
        oM.addMenuItem("Admin dashboard", "../main/admin_dashboard.aspx", MenuRole.Admin);
        oM.addMenu("Dashboard", "../main/sales_dashboard.aspx", MenuRole.UserOnly);

        oM.addMenu("Finance", "", MenuRole.Admin);

        oM.addMenuItem("View dashboard", "../main/accounting_dashboard.aspx");
        oM.addMenuItem("Search tx", "../main/tx_search.aspx");
        oM.addMenuItem("Finalize Commission", "../reports/commission_statement_finalize.aspx", MenuRole.Admin);
        oM.addMenuItem("MYOB exceptions", "../reports/myob_modifications.aspx", MenuRole.Admin);
        oM.addSpacer();
        oM.addMenuItem("Export to MYOB", "../main/MYOB_export.aspx");
        oM.addSpacer();
        oM.addMenuItem("Change property details", "../main/sale_modification.aspx");
        oM.addSpacer();
        oM.addMenuItem("Setup user budgets", "../main/user_account_update.aspx");
        oM.addMenuItem("Import EOY values", "../admin/import_values.aspx");

        oM.addMenu("Campaign", "../campaign/campaign_dashboard.aspx", MenuRole.Campaign);
        if (Payroll.CanAccess)
            oM.addMenu("Payroll", "../payroll/payroll_dashboard.aspx");
        if (canAccessReports()) {
            oM.addMenu("Reports", "../reports/report_admin.aspx");

            oM.addMenu("Admin", "../reports/report_admin.aspx", MenuRole.Admin);
            oM.addMenuItem("Staff admin", "../admin/user_detail.aspx", MenuRole.Admin);
            oM.addMenuItem("Staff KPI admin", "../admin/user_detail_kpi.aspx", MenuRole.Admin);

            oM.addSpacer();
            oM.addMenuItem("Set current pay period", "../admin/pay_period_detail.aspx");
            oM.addMenuItem("Commission types", "../admin/list_detail.aspx?intListTypeID=3");
            oM.addMenuItem("Expense categories", "../admin/list_detail.aspx?intListTypeID=2");
            oM.addMenuItem("Income categories", "../admin/list_detail.aspx?intListTypeID=4");
            oM.addMenuItem("Off the top items", "../admin/list_detail.aspx?intListTypeID=6");
            oM.addMenuItem("TX Categories", "../admin/list_detail.aspx?intListTypeID=10");
            oM.addSpacer();
            oM.addMenuItem("Campaign GL codes", "../admin/list_detail.aspx?intListTypeID=9");
            oM.addSpacer();
            oM.addMenuItem("Branch locations", "../admin/list_detail.aspx?intListTypeID=1");
            oM.addMenuItem("Companies", "../admin/list_detail.aspx?intListTypeID=7");
            oM.addSpacer();
            oM.addMenuItem("View logins", "../admin/application_audits.aspx", MenuRole.Admin);
            oM.addMenuItem("View change log", "../admin/log_detail.aspx", MenuRole.Admin);
            oM.addMenuItem("Admin functions", "../admin/admin_tasks.aspx", MenuRole.Admin);
            oM.addSpacer();
            oM.addMenuItem("View commission PDFs", "../main/commission_statement_dashboard.aspx", MenuRole.Admin);
            oM.addSpacer();
            oM.addMenuItem("Import Box Dice", "../boxdice/import.aspx");
        }
        oM.addMenu("Help", "", MenuRole.Admin);
        oM.addMenuItem("About", "../main/about.aspx");
 
        oM.addMenu("Logout", "../login.aspx");

        if (G.User.RoleID == 1) {
            oM.lUserRoles.Add(MenuRole.Admin);
            oM.StartPage = "main/admin_dashboard.aspx";
        } else {
            if (G.User.RoleID != 5) {
                oM.lUserRoles.Add(MenuRole.UserOnly);
            }
        }

        if (G.User.hasCampaignAccess) {
            oM.lUserRoles.Add(MenuRole.Campaign);
            if (oM.StartPage == "") {
                oM.StartPage = "campaign/campaign_dashboard.aspx";
            }
        }

        if (G.User.hasPermission(RolePermissionType.ViewCommissionModule)) {
            oM.lUserRoles.Add(MenuRole.Commission);
        }
    }

    private bool canAccessReports() {
        return G.User.RoleID != 5 && G.User.RoleID != 6;
    }

    #region MenuGenerator

    public string createMenu() {
        string szHTML = String.Format(@"
              <div id='Logo' style='margin-top: 4px; width: 100%; float: left'>
                    <span style='width: 30%; float: left'>&nbsp;</span>
                    <img src='../sys_images/logo.png' alt='Fletchers logo' style='float: left; '/>

                    <span class='LogoWord' style='float: right; width:200px; text-align: left; '>
                        <span class='LogoLeadingLetter'>C</span>OLLECTIONS<br />
                        <span class='LogoLeadingLetter'>A</span>ND<br />
                        <span class='LogoLeadingLetter'>P</span>AYROLL<br />
                        <span class='LogoLeadingLetter'>S</span>YSTEM<br />
                    </span>
                </div>
                <div class='AppMenu'>
                    <div style='float: left; width: 70%' >
                        <nav id='custom-bootstrap-menu' class='navbar navbar-default'>
                            <div class='container-fluid'>
                                <div class='navbar-header'>
                                    <button type='button' class='navbar-toggle collapsed' data-toggle='collapse' data-target='#bs-example-navbar-collapse-1'>
                                    <span class='sr-only'>Toggle navigation</span>
                                    <span class='icon-bar'></span>
                                    <span class='icon-bar'></span>
                                    <span class='icon-bar'></span>
                                    </button>
                                </div>

                                <!-- Collect the nav links, forms, and other content for toggling -->
                                <div class='collapse navbar-collapse' id='bs-example-navbar-collapse-1'>
                                    <ul class='nav navbar-nav'>
                                        {0}
                                    </ul>
                                </div><!-- /.navbar-collapse -->
                            </div><!-- /.container-fluid -->
                        </nav>
                    </div>
                    <div style='float: right; width: 30%; text-align: right; '>
                       <span style='color: white; width: 350px; font-size: 12px'>{1}</span>
                        <a href='../help/CAPSAgentViewingInfo.pdf'  target='_blank'>
                           <img src='../sys_images/help.gif' align='right' title='Click here to view help'/>
                        </a>
                    </div>
                </div>
            ", oM.createMenu(), G.User.UserName + " - " + System.DateTime.Now.ToString("D"));
        return szHTML;
    }
}

/// <summary>
/// Contains the logic to generate a menu for the client
/// </summary>
public class Menu {
    private List<MenuRole> lRoles = new List<MenuRole>();
    private List<MenuItem> lMenu = new List<MenuItem>();
    private MenuItem currMenu = null;
    private string DefaultTarget = "";
    public List<MenuRole> lUserRoles = new List<MenuRole>();
    public string StartPage = "";

    /// <summary>
    /// A list of menu roles used by the user - should be loaded by the Menu object only
    /// </summary>
    public static string MenuRoleList {
        get {
            if (HttpContext.Current.Session["USERMENUROLELIST"] == null)
                return "";
            else
                return Convert.ToString(HttpContext.Current.Session["USERMENUROLELIST"]);
        }
        set {
            HttpContext.Current.Session["USERMENUROLELIST"] = value;
        }
    }

    public Menu(string DefaultTarget = "") {
        this.DefaultTarget = DefaultTarget;
    }

    /// <summary>
    /// Creates a bootstrap menu and returns the basic HTML - this must be wrapped in the bootstrap controls for a proper responsive framework
    /// </summary>
    /// <returns></returns>
    public string createMenu() {
        string szHTML = "";
        foreach (MenuItem oM in lMenu) {
            if (!oM.visibleToUser(lUserRoles))
                continue;
            //Menu with a single item in it
            if (oM.lSubMenu.Count == 0) {
                szHTML += String.Format(@"
                            <li><a href='{1}' target='{2}'>{0}</a></li>", oM.Name, oM.URL, "");
                continue;
            }
            szHTML += String.Format(@"
                    <li class='dropdown'>
                      <a href='#' class='dropdown-toggle' data-toggle='dropdown' role='button' aria-expanded='false'>{0} <span class='caret'></span></a>
                      <ul class='dropdown-menu' role='menu'>", oM.Name);

            //Menu with
            foreach (MenuItem oI in oM.lSubMenu) {
                if (!oI.visibleToUser(lUserRoles))
                    continue;

                if (oI.Name == "SPACER")
                    szHTML += "<li class='divider'></li>";
                else
                    szHTML += String.Format(@"
                        <li><a href='{1}' target='{2}'>{0}</a></li>
                       ", oI.Name, oI.URL, oI.Target);
            }
            szHTML += @"
                      </ul>
                    </li>";
        }
        return szHTML;
    }

    private bool hasRole(MenuRole CheckRole) {
        return lUserRoles.Contains(CheckRole);
    }

    public MenuItem addMenu(string Name, string URL = "", MenuRole Role = MenuRole.Everyone, string Target = "") {
        if (Target == "")
            Target = DefaultTarget;

        currMenu = new MenuItem(Name, URL, Role, Target);
        lMenu.Add(currMenu);
        return currMenu;
    }

    public void addMenuItem(string Name, string URL = "", MenuRole Role = MenuRole.Everyone, string Target = "") {
        if (Target == "")
            Target = DefaultTarget;
        currMenu.addItem(new MenuItem(Name, URL, Role, Target));
    }

    public void addSpacer() {
        currMenu.addItem(new MenuItem("SPACER"));
    }
}

/// <summary>
/// A menu item
/// </summary>
public class MenuItem {
    public string Name { get; set; }
    public string Target { get; set; }
    public string URL { get; set; }
    public List<MenuItem> lSubMenu = new List<MenuItem>();
    private List<MenuRole> lRoles = new List<MenuRole>();

    public MenuItem(string Name, string URL = "", MenuRole Role = MenuRole.Everyone, string Target = "") {
        this.Name = Name;
        addAnotherRole(Role);
        this.URL = URL;
        this.Target = Target;
    }

    public void addItem(MenuItem oM) {
        lSubMenu.Add(oM);
    }

    public bool visibleToUser(List<MenuRole> lUserRoles) {
        if (this.lRoles.Count == 0)
            return true;
        return lUserRoles.Intersect(this.lRoles).Count() > 0;
    }

    //Add additional roles to the
    public void addAnotherRole(MenuRole oR) {
        if (oR == MenuRole.Everyone)
            return;

        if (!lRoles.Contains(oR))
            lRoles.Add(oR);
    }
}

#endregion MenuGenerator