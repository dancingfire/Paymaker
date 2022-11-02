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
        oM.addSpacer();
        oM.addMenuItem("Change property details", "../main/sale_modification.aspx", MenuRole.Admin);

        oM.addMenu("Dashboard", "../main/sales_dashboard.aspx", MenuRole.UserOnly);

        oM.addMenu("Sales Payroll", "", MenuRole.Admin);
        oM.addMenuItem("Dashboard", "../main/accounting_dashboard.aspx");
        oM.addMenuItem("Search transactions", "../main/tx_search.aspx");
        oM.addMenuItem("Commission EOM rollover", "../main/commission_statement_rollover.aspx", MenuRole.Admin);
        oM.addMenuItem("View commission PDFs", "../main/commission_statement_dashboard.aspx", MenuRole.Admin);
        oM.addSpacer();
        oM.addMenuItem("Export to MYOB - Commission", "../main/commission_statement_finalize.aspx", MenuRole.Admin);
        oM.addMenuItem("Export to MYOB - Transactions", "../main/MYOB_export.aspx");
        oM.addMenuItem("MYOB exceptions", "../reports/myob_modifications.aspx", MenuRole.Admin);
        oM.addSpacer();
        oM.addMenuItem("Setup user budgets", "../main/user_account_update.aspx");
        oM.addMenuItem("Import EOY values", "../admin/import_values.aspx");
        oM.addMenuItem("Payroll settings", "../admin/sales_settings.aspx", MenuRole.Admin);
        oM.addSpacer();
        oM.addMenuItem("Set current pay period", "../admin/pay_period_detail.aspx");
        if (Payroll.CanAccess) {
            if (G.User.IsAdmin) { 
                oM.addMenu("IS Payroll", "");
                oM.addMenuItem("My current timesheet", "../payroll/payroll_update.aspx");
                oM.addMenuItem("Admin - Current IS pay cycle", "../payroll/payroll_summary.aspx?type=ALL");
                oM.addMenuItem("View IS Payroll PDFs", "../admin/payroll_pdf_files.aspx");
                oM.addSpacer();
                oM.addMenuItem("Check emails", "../automation/email_notification.aspx?Check=true");
                oM.addMenuItem("View staff list", "../payroll/staff_list.aspx?");
                
            } else {
                oM.addMenu("IS Payroll", "../payroll/payroll_dashboard.aspx");
            }

        }
        oM.addMenu("Campaign", "../campaign/campaign_dashboard.aspx", MenuRole.Campaign);
        oM.addMenu("Leave");
        oM.addMenuItem("Dashboard", "../payroll/leave_dashboard.aspx");
        if (G.User.IsAdmin || G.User.UserID == 497 || G.User.UserID == 178) {
            oM.addSpacer();
            oM.addMenuItem("Export leave requests", "../reports/leave_request.aspx", MenuRole.Admin);
            oM.addSpacer();
            oM.addMenuItem("Leave type", "../admin/list_detail.aspx?intListTypeID=11", MenuRole.Admin);
            oM.addMenuItem("Public holidays", "../admin/holiday_detail.aspx", MenuRole.Admin);
            oM.addMenuItem("Settings", "../admin/leave_settings.aspx", MenuRole.Admin);
            oM.addMenuItem("Staff admin", "../admin/user_detail.aspx", MenuRole.Admin);
        } 

        if (canAccessReports()) {
            oM.addMenu("Reports", "../reports/report_admin.aspx");

            oM.addMenu("Admin", "../reports/report_admin.aspx", MenuRole.Admin);
            oM.addMenuItem("Staff admin", "../admin/user_detail.aspx", MenuRole.Admin);
            oM.addMenuItem("Staff KPI admin", "../admin/user_detail_kpi.aspx", MenuRole.Admin);
            oM.addMenuItem("Glossy publications", "../admin/list_detail.aspx?intListTypeID=" + (int)ListType.GlossyMagazine);

            oM.addSpacer();
            
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
            oM.addMenuItem("View delegations", "../reports/delegation.aspx", MenuRole.Admin);
            oM.addMenuItem("View logins", "../admin/application_audits.aspx", MenuRole.Admin);
            oM.addMenuItem("View change log", "../admin/log_detail.aspx", MenuRole.Admin);
            oM.addMenuItem("View log v2", "../admin/logv2_detail.aspx", MenuRole.Admin);
            oM.addMenuItem("View email log", "../admin/email_log.aspx", MenuRole.Admin);
            oM.addMenuItem("Admin functions", "../admin/admin_tasks.aspx", MenuRole.Admin);
             oM.addSpacer();
            oM.addMenuItem("Import Box Dice", "../boxdice/import.aspx");
        }
     
        oM.addMenu("Logout", "../login.aspx");

        if (G.User.IsAdmin) {
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
        return (G.User.RoleID != 5 && G.User.RoleID != 6) || G.User.hasPermission(RolePermissionType.ReportExpenseSummary);
    }

    #region MenuGenerator

    public string createMenu() {
        string szHTML = String.Format(@"
                  <div id='Logo' style='width: 100%; float: left'>
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
                                    <span class=""nav navbar-nav navbar-right"">
                                        <span style='float: right; font-size: smaller; margin-top: 22px'>
                                            {1}
                                        </span>
                                    </span>    
                                </div><!-- /.navbar-collapse -->
                            </div><!-- /.container-fluid -->
                        </nav>
                       
                    </div>
                
                {2}
            ", oM.createMenu(), getLoginDropDown(), HTML.createModalIFrameHTML("Delegation", "Delegate options", "850", 350));
        return szHTML;
    }


    protected string getLoginDropDown() {
        string szLoggedInAsMsg = "";
        if (G.User.OriginalUserID != G.User.UserID)
            szLoggedInAsMsg = " as " + G.User.Name + " (" + G.User.RoleID + ")";
        string szDelegation = "";

        if (Payroll.IsLeaveSupervisor || Payroll.IsPayrollSupervisor || G.User.IsAdmin) {
            string szAdmin = G.User.IsAdmin ? "Admin" : "";
            szDelegation = String.Format(@"
              <li id='oDelegate' role='presentation'><a role='menuitem' tabindex='-1' href='javascript: show{0}Delegation();'>Manage delegation</a></li>
            <li role='presentation' class='divider'></li>
            ", szAdmin) ;
        }

        return String.Format(@"
            <div class='dropdown' style='margin-top: -15px'>
                <button class='btn btn-default  btn-block dropdown-toggle' type='button' id='mDropDown' data-toggle='dropdown' style='height: 25px; font-size: 11px; padding-left: 20px; padding-right: 20px'>{0}
                <span class='caret'></span></button>
                <ul class='dropdown-menu' role='menu' aria-labelledby='menu1'>
                    {2}
                    <li id='oHelp' role='presentation'><a role='menuitem' tabindex='-1' href='../help/CAPSAgentViewingInfo.pdf'  target='_blank'>Show help <img src='../sys_images/help.gif' align='right' title='Click here to view help'/></a></li>
                    <li role='presentation' class='divider'></li>
                    <li role='presentation'><a role='menuitem' tabindex='-1' href='../login.aspx?Logout=true'>Logout</a></li>
                </ul>
            </div>", G.UserInfo.getName(G.User.OriginalUserID), szLoggedInAsMsg, szDelegation);
        
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