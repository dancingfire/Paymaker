using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;


/// <summary>
/// Code to show/hide the modal forms in the system
/// </summary>
public static class ModalForms {

    /// <summary>
    /// The code top open and close the layout update panel
    /// </summary>
    /// <returns></returns>
    public static string createModalUpdate(string Header, string Width = "70%", string Height = "50vh", bool ReturnHTML = false, bool SkipResize = false) {
        string szResize = " onload='parent.resizeFrameToContent(this)' onresize='parent.resizeFrameToContent(this)' ";
        if (SkipResize)
            szResize = "";
        string szHTML = String.Format(@"
            <div id='mModalUpdate' tabindex='-1' class='modal fade'>
                <div class='modal-dialog modal-lg' style='width: {1}; height: {2}'>
                    <div class='modal-content'>
                        <div class='modal-header'>
                            <h4 class='modal-title' id='mModalUpdateTitle'>{0}</h4>
                        </div>
                        <div class='modal-body'>
                            <iframe id='fModalUpdate' src='about:blank' style='width: 100%; height: {2}; border: 0px; overflow: visible'   class='overlay-iframe' scrolling='auto'  {3} ></iframe>
                        </div>
                    </div><!-- /.modal-content -->
                </div><!-- /.modal-dialog -->
            </div><!-- /.modal -->
            <script>
                //Generated framework
                 function closeModalUpdate( data) {{
                    $('#mModalUpdate').modal('hide');
                }}
            </script>", Header, Width, Height, szResize); 
        if (ReturnHTML) {
            return szHTML;
        }
        G.oRoot.Form.Controls.Add(new LiteralControl(szHTML));
        return "";
    }
}

/// <summary>
/// Functions  that are used throughout the application
/// </summary>
///

public static class HTML {

    /// <summary>
    /// Adds all the required js links to the page
    /// </summary>
    /// <param name="oPage"></param>
    /// <param name="IsRoot"></param>
    public static void addJSLinks(Page oPage, bool IsRoot) {
        List<string> arFiles = new List<string>();
        string szDir = "../";
        if (IsRoot)
            szDir = "";
        arFiles.Add(szDir + "include/utility.js?v=13");
        arFiles.Add(szDir + "include/ckeditor/adapters/jquery.js");
        arFiles.Add(szDir + "include/ckeditor/ckeditor.js");
        arFiles.Add("https://cdnjs.cloudflare.com/ajax/libs/select2/4.0.6-rc.0/js/select2.min.js");
            //arFiles.Add("https://cdn.datatables.net/v/dt/dt-1.10.16/b-1.5.0/fh-3.1.3/kt-2.3.2/datatables.min.js");
            arFiles.Add("https://cdn.datatables.net/v/bs/jszip-2.5.0/dt-1.10.18/b-1.5.2/b-html5-1.5.2/fh-3.1.4/datatables.min.js");
        arFiles.Add(szDir + "include/jquery.mods.js?v=13");
        arFiles.Add(szDir + "include/moment-business-days/index.js?t=1");
        arFiles.Add("https://code.jquery.com/ui/1.12.1/jquery-ui.min.js");
        arFiles.Add("https://cdnjs.cloudflare.com/ajax/libs/moment.js/2.24.0/moment.min.js");
        arFiles.Add(szDir + "include/bootstrap/3.3.6/js/bootstrap.min.js");
        arFiles.Add(szDir + "include/jquery.validVal.min.js");
        arFiles.Add("https://code.jquery.com/jquery-2.2.4.min.js");

        HtmlLink oLink = new HtmlLink();

        oLink.Href = szDir + "main.css";
        oLink.Attributes.Add("rel", "stylesheet");
        oLink.Attributes.Add("type", "text/css");
        oLink.Attributes.Add("title", "MainSS");
        oPage.Header.Controls.AddAt(0, oLink);

        List<string> arCssLinks = new List<string>();
        arCssLinks.Add(szDir + "include/JQueryUI1.11.4/smoothness.css?v=1");
         arCssLinks.Add("https://cdn.datatables.net/v/bs/jszip-2.5.0/dt-1.10.18/b-1.5.2/b-html5-1.5.2/fh-3.1.4/datatables.min.css");
        arCssLinks.Add("https://cdnjs.cloudflare.com/ajax/libs/select2/4.0.6-rc.0/css/select2.min.css");

         arCssLinks.Add(szDir + "include/bootstrap/3.3.6/css/bootstrap.min.css");
        arCssLinks.Add(szDir + "include/bootstrap/3.3.6/css/bootstrap-theme.min.css");

        foreach (string File in arCssLinks) {
            if (File == "")
                continue;
            oLink = new HtmlLink();
            oLink.Href = File;
            oLink.Attributes.Add("rel", "stylesheet");
            oLink.Attributes.Add("type", "text/css");
            oPage.Header.Controls.AddAt(0, oLink);
        }

        foreach (string File in arFiles) {
            HtmlGenericControl Include = new HtmlGenericControl("script");
            Include.Attributes.Add("type", "text/javascript");
            Include.Attributes.Add("src", File);
            oPage.Header.Controls.AddAt(0, Include);
        }
    }

    /// <summary>
    /// Creates a modal dialog window that can be called via bootstrap, including the calling code for the update and reloading of the frame
    /// The Name parameter will be used to name the modal dialog: m<Name> and the iframe f<Name>
    /// </summary>
    /// <param name="Name"></param>
    /// <param name="Width"></param>
    /// <param name="Height"></param>
    public static string createModalUpdate(string Name, string Title, string Width, int Height, string UpdatePageName, bool exitX = false) {
        string szStyle = "";
        int intIframeHeight = 500;
        string szAdjustableHeight = "";

        if (!String.IsNullOrWhiteSpace(Width))
            szStyle += "width: " + Width;
        if (Height > 0) {
            Utility.Append(ref szStyle, "height: " + Height + "px", ";");
            intIframeHeight = Height - 80;
        } else {
            szAdjustableHeight = string.Format(@"
                function adjustHeight{0}() {{
                    var height = $(window).height();
                    $(""#mDialog{0}"").css(""height"", height - 50);
                    $(""#f{0}"").css(""height"", height - 130);
                }}

                $(document).ready(function() {{
                    adjustHeight{0}();
                }});

                $(window).resize(function() {{
                    adjustHeight{0}();
                }});", Name);
        }

        if (!String.IsNullOrWhiteSpace(szStyle))
            szStyle = "style='" + szStyle + "' ";

        string szExitX = "";

        if (exitX) {
            szExitX = string.Format("<a href='javascript: close{0}()' style='float: right; text-decoration: none'>x</a>", Name);
        }

        return String.Format(@"
            <div id=""m{0}"" class=""modal"">
                <div id=""mDialog{0}"" class=""modal-dialog"" {1}>
                    <div class=""modal-content"">
                        <div class=""modal-header"">
                            <h4 class=""modal-title"">{2}{7}</h4>
                        </div>
                        <div class=""modal-body"">
                            <iframe id='f{0}' src=""about:blank""  width=""99%"" height=""{4}"" style='border: 0px' frameBorder='0'></iframe>
                        </div>
                        <div class=""modal-footer"">
                            <button type=""button"" class=""btn btn-default"" data-dismiss=""modal"">Close</button>
                        </div>
                    </div><!-- /.modal-content -->
                </div><!-- /.modal-dialog -->
            </div><!-- /.modal -->
            <script type='text/javascript'>
                 function view{0}(intID) {{
                    $('#m{0}').on('show.bs.modal', function () {{
                        $('#f{0}').attr(""src"", ""{3}?IsPopup=true&blnReadOnly="" + blnReadOnly + ""&intItemID="" + intID + ""&ts={6}"");
                    }});
                    $('#m{0}').on('hidden.bs.modal', function () {{
                        $('#f{0}')[0].contentWindow.removeValidation();
                    }});

                    $('#m{0}').modal('show');
                    return false;
                 }}

                 function close{0}(blnUpdate) {{
                    $('#f{0}').attr(""src"", ""about:blank"" );
                    $('#m{0}').modal('hide');
                 }}

                {5}
            </script>
        ", Name, szStyle, Title, UpdatePageName, intIframeHeight,
        szAdjustableHeight, DateTime.Now.Ticks, szExitX);
    }

    /// <summary>
    /// Creates a modal dialog window that can be called via bootstrap,
    /// The Name parameter will be used to name the modal dialog: m<Name> and the iframe f<Name>
    /// </summary>
    /// <param name="Name">The name of the modal dialog - the iframe has to be set when opening</param>
    /// <param name="Width"></param>
    /// <param name="Height"></param>
    public static string createModalIFrameHTML(string Name, string Title, string Width, int Height) {
        string szStyle = "";
        if (!String.IsNullOrWhiteSpace(Width))
            szStyle += "width: " + Width;

        Utility.Append(ref szStyle, "height: " + Height + "px", "; ");
        if (!String.IsNullOrWhiteSpace(szStyle))
            szStyle = "style='" + szStyle + "' ";

        return String.Format(@"
            <div id=""m{0}"" class=""modal"" style='float: left'>
                <div class=""modal-dialog"" {1}>
                    <div class=""modal-content"">
                        <div class=""modal-header"">
                            <h4 class=""modal-title"">{2}</h4>
                        </div>
                        <div class=""modal-body"">
                            <iframe id='f{0}' src=""about:blank"" style='width: 100%; height: {3}px; border: 0px' frameBorder='0'></iframe>
                        </div>
                    </div><!-- /.modal-content -->
                </div><!-- /.modal-dialog -->
            </div><!-- /.modal -->
        ", Name, szStyle, Title, Height);
    }

    /// <summary>
    /// Creates a modal dialog window that can be called via bootstrap,
    /// The Name parameter will be used to name the modal dialog: m<Name> and the iframe f<Name>
    /// </summary>
    /// <param name="Name">The name of the modal dialog - the iframe has to be set when opening</param>
    /// <param name="Width"></param>
    /// <param name="Height"></param>
    public static string createModalPanel(string Name, string Title, string Content, string Width, int Height) {
        string szStyle = "";
        if (!String.IsNullOrWhiteSpace(Width))
            szStyle += "width: " + Width;

        Utility.Append(ref szStyle, "height: " + Height, "; ");
        if (!String.IsNullOrWhiteSpace(szStyle))
            szStyle = "style='" + szStyle + "' ";

        return String.Format(@"
            <div id=""m{0}"" class=""modal"" >
                <div class=""modal-dialog"" {1} tabindex='-1' data-keyboard='true' data-backdrop='static'>
                    <div class=""modal-content"">
                        <div class=""modal-header"">
                            <button type=""button"" class=""close"" data-dismiss=""modal"" aria-hidden=""true"">&times;</button>
                            <h4 class=""modal-title"">{2}</h4>
                        </div>
                        <div class=""modal-body"">
                            {3}
                        </div>
                    </div><!-- /.modal-content -->
                </div><!-- /.modal-dialog -->
            </div><!-- /.modal -->
        ", Name, szStyle, Title, Content);
    }

    /// <summary>
    /// Returns the HTML for the CommissionType list
    /// </summary>
    /// <param name="szName"></param>
    /// <param name="CurrentValue"></param>
    /// <returns></returns>
    public static string getCommissionTypeSelectHTML(string szName, int CurrentValue) {
        string szSQL = String.Format(@"
            SELECT ID, NAME
            FROM LIST WHERE LISTTYPEID = {0}
            ORDER BY SEQUENCENO, NAME ", (int)ListType.Commission);
        SqlDataReader dr = DB.runReader(szSQL);
        string szItems = "";
        while (dr.Read()) {
            int intID = int.Parse(dr["ID"].ToString());

            szItems += "<option value='" + dr["ID"].ToString() + "' " + (intID == CurrentValue ? " SELECTED " : "") + ">" + dr["NAME"].ToString() + "</option>";
        }
        dr.Close();
        dr = null;
        string szHTML = String.Format(@"
            <select id='{0}' name='{0}' class='Entry'>
            {1}
            </select>", szName, szItems);
        return szHTML;
    }

    public static TextBox createTextBox(string Name, string CssClass, string Value) {
        return createTextBox(Name, CssClass, Value, false);
    }

    public static TextBox createTextBox(string Name, string CssClass, string Value, bool ReadOnly, string Style) {
        TextBox oText = new TextBox();
        oText.CssClass = CssClass;
        if (ReadOnly) {
            oText.Style.Value = "border: 0px solid white; overflow: auto";
        }
        oText.ReadOnly = ReadOnly;
        oText.ID = Name;
        oText.Text = Value;
        if (Style != "")
            oText.Style.Value = Style;
        return oText;
    }

    public static TextBox createTextBox(string Name, string CssClass, string Value, bool ReadOnly) {
        TextBox oText = new TextBox();
        oText.CssClass = CssClass;
        if (ReadOnly) {
            oText.Style.Value = "border: 0px solid white; overflow: auto";
        }
        oText.ReadOnly = ReadOnly;
        oText.ID = Name;
        oText.Text = Value;

        return oText;
    }

    public static Label createLabel(string Name, string CssClass, string Value) {
        return createLabel(Name, CssClass, "", Value);
    }

    public static Label createLabel(string Name, string CssClass, string Style, string Value) {
        Label oObj = new Label();
        oObj.CssClass = CssClass;
        oObj.ID = Name;
        oObj.Text = Value;
        if (Style != "")
            oObj.Style.Value = Style;
        return oObj;
    }

    public static Image createImage(string Name, string CssClass, string Style, string Url, string AltText, string onMouseOver, string onMouseOut, string onClick) {
        Image oObj = new Image();
        if (CssClass != "")
            oObj.CssClass = CssClass;
        oObj.ID = Name;
        oObj.AlternateText = AltText;
        oObj.ImageUrl = Url;
        if (Style != "")
            oObj.Style.Value = Style;
        if (onMouseOver != "")
            oObj.Attributes["onmouseover"] = onMouseOver;
        if (onMouseOut != "")
            oObj.Attributes["onmouseout"] = onMouseOut;
        if (onClick != "")
            oObj.Attributes["onclick"] = onClick;
        oObj.BorderWidth = 0;
        return oObj;
    }

    public static Panel createPanel(string Name, string CssClass, string Style) {
        Panel oObj = new Panel();
        oObj.CssClass = CssClass;
        oObj.ID = Name;
        if (Style != "")
            oObj.Style.Value = Style;
        return oObj;
    }

    public static CheckBox createCheckBox(string Name, string CssClass, string Style, bool Checked) {
        CheckBox oObj = new CheckBox();
        oObj.CssClass = CssClass;
        oObj.ID = Name;
        oObj.Text = "";
        if (Style != "")
            oObj.Style.Value = Style;
        oObj.Checked = Checked;
        return oObj;
    }

    public static CheckBox createCheckBox(string Name, string CssClass, string Style, bool Checked, string Event, string EventValue) {
        CheckBox oObj = new CheckBox();
        oObj.CssClass = CssClass;
        oObj.ID = Name;
        oObj.Text = "";
        if (Style != "")
            oObj.Style.Value = Style;
        oObj.Checked = Checked;
        oObj.Attributes[Event] = EventValue;
        return oObj;
    }

    public static Label createLabel(string Value) {
        Label oObj = new Label();
        oObj.CssClass = "Label";
        oObj.Text = Value;
        return oObj;
    }

    public static Literal createBRAlign() {
        Literal oObj = new Literal();
        oObj.Text = "<br class='Align'/>";
        return oObj;
    }

    public static HiddenField createHidden(string Name, string Value) {
        HiddenField oObj = new HiddenField();
        oObj.ID = Name;
        oObj.Value = Value;
        return oObj;
    }

    public static LiteralControl createDiv(string Name, string CssClass, string Contents) {
        string szHTML = String.Format("<div id='{0}' class='{1}'>{2}</div>", Name, CssClass, Contents);
        return new LiteralControl(szHTML);
    }

    public static LiteralControl createDiv(string Name, string CssClass, string Contents, string Style) {
        string szHTML = String.Format("<div id='{0}' class='{1}' style='{3}'>{2}</div>", Name, CssClass, Contents, Style);
        return new LiteralControl(szHTML);
    }

    public static LiteralControl createDiv(string Name, string CssClass, string Contents, string Style, string Event) {
        string szHTML = String.Format("<div id='{0}' class='{1}' style='{3}' {4}> {2} </div>", Name, CssClass, Contents, Style, Event);
        return new LiteralControl(szHTML);
    }

    /// <summary>
    /// Formats the CKEditor based on the settings in the application
    /// </summary>
    /// <param name="oCK">The editor</param>
    /// <param name="oParams"> Controls some aspects of the control</param>
    public static void formatCKEditor(ref Syrinx.Gui.AspNet.CkEditor oCK) {
        oCK.Toolbar = "'BASIC'";
        oCK.BaseContentUrl = "../ckeditor/";
        oCK.Resizable = false;
    }

    public static LiteralControl createSpan(string Name, string CssClass, string Contents, string Style, string Event) {
        string szHTML = String.Format("<span id='{0}' class='{1}' style='{3}' {4}  >{2}</span>", Name, CssClass, Contents, Style, Event);
        return new LiteralControl(szHTML);
    }

    public static ListBox createListBox(string szID, string szClass, bool AllowMultiple) {
        return createListBox(szID, szClass, "", AllowMultiple, false);
    }

    public static ListBox createListBox(string szID, string szClass, string szStyle, bool AllowMultiple) {
        return createListBox(szID, szClass, szStyle, AllowMultiple, false);
    }

    public static ListBox createListBox(string szID, string szClass, string szStyle, bool AllowMultiple, bool ReadOnly) {
        ListBox oList = new ListBox();
        oList.ID = szID;
        if (AllowMultiple) {
            oList.SelectionMode = ListSelectionMode.Multiple;
            oList.Rows = 4;
        } else {
            oList.Rows = 1;
        }
        oList.Enabled = !ReadOnly;
        oList.Style.Value = szStyle;
        oList.CssClass = szClass;
        return oList;
    }

    public static DropDownList createDropDownListBox(string szID, string szClass, string szStyle) {
        DropDownList oList = new DropDownList();
        oList.ID = szID;
        oList.Style.Value = szStyle;
        oList.CssClass = szClass;

        return oList;
    }

    public static DropDownList createDropDownListBox(string szID, string szClass, string szStyle, string szJavaScript) {
        DropDownList oList = new DropDownList();
        oList.ID = szID;
        oList.Style.Value = szStyle;
        oList.CssClass = szClass;
        oList.Attributes["onChange"] = szJavaScript;
        return oList;
    }

    public static void formatGridView(ref GridView oGV, bool FormatForFiltering = false, bool TableHover = false) {
        if (oGV.Rows.Count > 0) {
            if (FormatForFiltering) {
                oGV.HeaderRow.TableSection = TableRowSection.TableHeader;
                oGV.BorderStyle = BorderStyle.None;
                oGV.BorderWidth = 0;
               
            }
            oGV.HeaderRow.CssClass = "ListHeader";
            oGV.CssClass += " table table-condensed";
            if (TableHover)
                oGV.CssClass += " table-hover";

            oGV.EmptyDataRowStyle.CssClass = "EmptyData";
        }
        oGV.GridLines = GridLines.None;
    }

    public static DropDownList createAmountTypeListBox(string szID, string szClass, string szStyle) {
        DropDownList oList = new DropDownList();
        oList.ID = szID;
        oList.Style.Value = szStyle;
        oList.CssClass = szClass;
        oList.Items.Add(new ListItem("$", "0"));
        oList.Items.Add(new ListItem("%", "1"));
        return oList;
    }

    public static string userSplitHTML(int CommissionTypeID, int SaleSplitID, bool blnAddButton, UserSalesSplit oUSS = null, int UserSaleSplitCount = -1000) {
        StringBuilder sbHTML = new StringBuilder();
        string szIDTag = "";
        if (oUSS != null && oUSS.ID > -1) {
            szIDTag = "CT_" + CommissionTypeID + "_SS_" + SaleSplitID + "_USS_" + oUSS.ID;
        } else {
            szIDTag = "CT_" + CommissionTypeID + "_SS_" + SaleSplitID + "_USS_" + UserSaleSplitCount;
        }
        DropDownList ddlAmountType = HTML.createAmountTypeListBox("lstAmountType_" + szIDTag, "Entry JQUserSaleSplitAmountTypeList", "width: 100%");
        ddlAmountType.Attributes.Add("onchange", "amountTypeChange(this);");
        DropDownList ddlUserSaleSplit = HTML.createDropDownListBox("lstUserSaleSplit_" + szIDTag, "Entry JQUserSaleSplitList", "width: 100%");
        int intDefaultUserID = -1;
        if (oUSS != null)
            intDefaultUserID = oUSS.UserID;
        Utility.BindList(ref ddlUserSaleSplit, DB.Paymaker_User.loadList("", true, intDefaultUserID, true), "ID", "NAME");

        string szValidationType = "percent";
        string szAmount = "100";
        string szCalcAmount = "";
        string szIncludeInKPI = "";
        Utility.setListBoxItems(ref ddlAmountType, ((int)AmountType.Percent).ToString());

        if (oUSS != null && oUSS.ID > -1) {
            Utility.setListBoxItems(ref ddlAmountType, ((int)oUSS.oAmountType).ToString());
            Utility.setListBoxItems(ref ddlUserSaleSplit, ((int)oUSS.UserID).ToString());
            if (oUSS.oAmountType == AmountType.Percent)
                szValidationType = "percent";
            szAmount = oUSS.Amount.ToString();
            szCalcAmount = Utility.formatMoney(oUSS.CalculatedAmount);
            if (oUSS.IncludeInKPI)
                szIncludeInKPI = "checked";
        }
        string szHTMLAmount = string.Format(@"
                <input id='{0}' name='{0}' value='{1}' class='Entry JQUserSaleSplitAmount {2}' style='width:100%;'  onfocus='highlightTextOnFocus(this)' onblur='saleSplitAmountChange(this)'/>"
            , "txtAmount_" + szIDTag, szAmount, szValidationType);
        string szHTMLCalcAmount = string.Format(@"
                <div id='{0}' class='CalcTotal AlignRight JQUserSaleSplit', style='width:100%'>{1}</div>"
            , "txtCalculatedAmount_" + szIDTag, szCalcAmount);
        string szHTMLButton = "";

        if (blnAddButton) {
            szHTMLButton += string.Format(@"
                <input id='btnSplit_{0}' type='button' class='Button btn style='width:100%' onclick='addSplitRow(this)' value='Add' />"
                , "CT_" + CommissionTypeID + "_SS_" + SaleSplitID);
        }
        string szKPIRadioButton = String.Format(@"<input type='radio' id='{0}' name='KPI'  value='On' {1} title='Include this agent in the KPI split?' class='SelectKPI'/>", "chkIncludeInKPI_" + szIDTag, szIncludeInKPI);
        if (CommissionTypeID != 10) //Lister
            szKPIRadioButton = "";
        string szHTML = string.Format(@"
             <tr>
                <td style='width:5%'>{5}</td>
                <td style='width:20%'>{0}</td>
                <td style='width:15%'>{1}</td>
                <td style='width:10%'>{2}</td>
                <td style='width:5%'>&nbsp;</td>
                <td style='width:15%'>{3}</td>
                <td style='width:7%'>&nbsp;</td>
                <td style='width:13%'>{4}</td>
                <td style='width:10%'>&nbsp;</td>
            </tr>", Utility.ControlToString(ddlUserSaleSplit), szHTMLAmount, Utility.ControlToString(ddlAmountType), szHTMLCalcAmount, szHTMLButton,
                  szKPIRadioButton);
        return szHTML;
    }

    /// <summary>
    /// This creates an HTML for the Fletchers User Sales split for the summing purpose of Users sales splits
    /// </summary>
    /// <param name="CommissionTypeID"></param>
    /// <param name="SaleSplitID"></param>
    /// <returns></returns>
    public static string userSplitHTML(int CommissionTypeID, int SaleSplitID) {
        string szIDTag = "";
        szIDTag = "CT_" + CommissionTypeID + "_SS_" + SaleSplitID + "_USS_-1000";
        string szHTMLCalcAmount = string.Format(@"
           <div id='{0}' class='CalcTotal AlignRight JQUserSaleSplit JQFletchersUserSaleSplit', style='width:100%'>{1}</div>"
            , "txtCalculatedAmount_" + szIDTag, "");

        string szHTML = string.Format(@"
            <tr>
                <td style='width:30%'>&nbsp;</td>
                <td style='width:15%'>&nbsp;</td>
                <td style='width:10%'>&nbsp;</td>
                <td style='width:5%'>&nbsp;</td>
                <td style='width:15%'>{0}</td>
                <td style='width:7%'>&nbsp;</td>
                <td style='width:13%'>&nbsp;</td>
                <td style='width:10%'>&nbsp;</td>
            </tr>", szHTMLCalcAmount);
        return szHTML;
    }
}