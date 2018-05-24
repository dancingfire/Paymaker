using System;
using System.Web.UI;

public partial class permissions_update : Root {
    private RolePermissions oPermissions = null;
    private int intUserID = -1;
    private string szCurrPermissions = "";

    protected void Page_Init(object sender, EventArgs e) {
        blnShowMenu = false;
    }

    protected void Page_Load(object sender, EventArgs e) {
        preparePage();
        if (!Page.IsPostBack) {
            oPermissions = RolePermissions.getRolePermissions();
            setCurrentPermissions();
            drawPermissions();
        }
    }

    protected void setCurrentPermissions() {
        szCurrPermissions = DB.getScalar("SELECT PERMISSIONS FROM DB_USER WHERE ID = " + intUserID, "");
    }

    protected void drawPermissions() {
        Array arGroups = Enum.GetValues(typeof(RolePermissionGroupType));
        foreach (RolePermissionGroupType oGroup in arGroups) {
            string szPermissionHTML = "<div class='ListHeader' style='width: 100%; margin-bottom: 3px;'>" + RolePermissions.getRolePermissionGroupName(oGroup) + "</div>";

            foreach (RolePermission oPermission in oPermissions.PermissionList) {
                if (oPermission.PermissionGroup == oGroup) {
                    // add the role permission to the output for this role permission group
                    string szHelpMouseOver = "";
                    if (oPermission.IsActive)
                        szHelpMouseOver = " onMouseOver=\"doShowHelp(" + oPermission.PermissionTypeAsInt + ", this);\" onMouseOut=\"doHideHelp(this);\" ";
                    string szChecked = "";
                    if (Utility.InCommaSeparatedString(oPermission.PermissionTypeAsInt.ToString(), szCurrPermissions))
                        szChecked = " checked ";

                    szPermissionHTML += String.Format(@"
                        <div class='Normal' style='width: 100%; clear:both' {0}>
				            <div class='Normal' style='float: left;' {1}>{2}</div>
				            <div class='Normal' style='float: right;'><input type='checkbox' name='chkPermission_{3}' value='{3}' {4}></div>
			            </div>
                    ", (!oPermission.IsActive) ? "disabled" : "", szHelpMouseOver, oPermission.Label, oPermission.PermissionTypeAsInt, szChecked);

                    // add the help for this permission to the javascript array
                    sbEndJS.Append(String.Format(@"
                        arHelp[{0}] = ""{1}"";
                    ", oPermission.PermissionTypeAsInt, oPermission.Help));
                }
            }

            LiteralControl oDiv = HTML.createDiv("Group_" + (int)oGroup, "", szPermissionHTML, "float: left; width: 395px; margin-right: 7px; margin-top: 10px; border: 1px solid gray; padding: 3px;");

            pPermissions.Controls.Add(oDiv);
        }
    }

    protected void preparePage() {
        // required variables
        intUserID = Convert.ToInt32(Request.QueryString["intSelUserID"]);

        // instructional text
        string szText = "Select the permissions that you would like to enable for this this person below. ";
        szText += "<br/><br/>Click  update </i> to save changes, or <i>Cancel</i> to exit.<br/><br/><b>Note:</b> You can mouse-over a permission name for help information.";
        oInstructions.InnerHtml = szText;

        // help tooltip div
        oToolTip.Attributes["style"] = String.Format("display: none; position: absolute; top:78px; left:79%; width:160px; height:80%; font-family: verdana; font-size: 8pt; background-color: {0}; filter:progid:DXImageTransform.Microsoft.Gradient(GradientType=0, StartColorStr={0}, EndColorStr=#FFFFFF); padding:10px; z-index: 10; ", "#FFFF80");
    }

    protected void btnUpdate_Click(object sender, EventArgs e) {
        string szUpdatedPermissions = Request.Form["szPermissions"];
        DB.runNonQuery(String.Format(@"
                UPDATE DB_USER
                SET PERMISSIONS = '{0}' WHERE ID = {1}", DB.escape(szUpdatedPermissions), intUserID));

        sbStartJS.Append(String.Format(@"
		   parent.closePermissions();
        ", szUpdatedPermissions));
    }
}