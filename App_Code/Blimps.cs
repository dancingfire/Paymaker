using ExceptionHandler;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

public enum BlimpObject {
    Office = 0,
    SalesListing = 1,
    Property = 2,
    Consultant = 3,
    SalesVoucher = 4,
    SalesVoucherCommission = 5,
    Contact = 6,
    ContactActivity = 7,
    SalesVoucherDeduction = 8,
    SalesListingExpense = 9,
    OfficeCommission = 10,
    ContactCategory = 11,
    Task = 12
}

/// <summary>
/// Helper functions for Blimps access
/// </summary>
public static class BlimpsHelper {
    private static string szUserName = "gordfunk";
    private static string szPassword = "gordfunk";
    private static string szBlimpsURL = "https://fletchers.boxdice.com.au/blimps/load";

    /// <summary>
    /// Get the lastest update ID for the specified type. Will return -1 if nothing is set
    /// </summary>
    /// <param name="Type"></param>
    /// <returns></returns>
    public static int getUpdateID(BlimpObject Type) {
        string szConfigName = Type.ToString() + "LATESTID";
        AppConfigAdmin oConfigAdmin = new AppConfigAdmin();
        oConfigAdmin.addConfig(szConfigName, "-1");
        oConfigAdmin.loadValuesFromDB();
        return Convert.ToInt32(oConfigAdmin.getValue(szConfigName));
    }

    /// <summary>
    /// Set the update ID for the specififed type
    /// </summary>
    /// <param name="Type"></param>
    /// <param name="UpdateValue"></param>
    public static void setUpdateID(BlimpObject Type, int UpdateValue) {
        if (UpdateValue == -1)
            return;
        string szConfigName = Type.ToString() + "LATESTID";
        AppConfigAdmin oConfigAdmin = new AppConfigAdmin();
        oConfigAdmin.addConfig(szConfigName, "-1");
        oConfigAdmin.loadValuesFromDB();
        oConfigAdmin.setValue(szConfigName, UpdateValue.ToString());
        oConfigAdmin.updateConfigValueSQL(szConfigName, UpdateValue.ToString());
    }

    /// <summary>
    /// Returns a ready to go request object
    /// </summary>
    /// <returns></returns>
    public static RestRequest getRequest(int LastRangeID) {
        var request = new RestRequest(Method.POST);
        request.AddHeader("Content-Type", "application/json");
        if (LastRangeID > -1) {
            request.AddHeader("Range", "ts=" + LastRangeID);
            DB.runNonQuery("-- " + LastRangeID);
        }
        request.AddHeader("Test2", "test");
        request.RequestFormat = DataFormat.Json;

        request.OnBeforeDeserialization = resp => {
            resp.ContentType = "application/json";
        };
        return request;
    }

    private static string szPingClient = "http://123.2.135.112:8888";

    public static RestClient getPingClient() {
        var client = new RestClient();
        var c = new Http();

        client.BaseUrl = szPingClient;
        client.Authenticator = new HttpBasicAuthenticator(szUserName, szPassword);

        return client;
    }

    public static RestClient getClient() {
        var client = new RestClient();
        var c = new Http();

        client.BaseUrl = szBlimpsURL;
        client.Authenticator = new HttpBasicAuthenticator(szUserName, szPassword);

        return client;
    }

    /// <summary>
    /// Returns the final value of the content range from the header of the response object
    /// </summary>
    /// <param name="oR"></param>
    /// <returns></returns>
    public static int getContentRange(IList<Parameter> oR, ref int TotalRecords) {
        foreach (Parameter oP in oR) {
            if (oP.Name == "Content-Range") {
                string szValue = Convert.ToString(oP.Value); //Value is in the format of: ts 1-1014/1372835384
                szValue = szValue.Replace("ts", "").Trim();
                string[] arValue = szValue.Split('/');

                if (arValue.Length > 1) {
                    szValue = arValue[0]; //Reduced the expression to 1-1024
                    TotalRecords = Convert.ToInt32(arValue[1]);
                } else
                    return -1;
                arValue = szValue.Split('-');
                if (arValue.Length > 1)
                    return Convert.ToInt32(arValue[1]) + 1;
                else
                    return -1;
            }
        }
        return -1;
    }

    public static string loadIDs(BlimpObject Type) {
        var request = BlimpsHelper.getRequest(-1);
        var client = BlimpsHelper.getClient();
        client.BaseUrl = "https://fletchers.boxdice.com.au/blimps/ids";
        object o = null;
        switch (Type) {
            case BlimpObject.Property:
                o = new {
                    properties = new {
                        ids = "true"
                    }
                };
                break;

            case BlimpObject.SalesVoucherCommission:
                o = new {
                    SalesVoucherCommissions = new {
                        ids = "true"
                    }
                };
                break;

            case BlimpObject.SalesListing:
                o = new {
                    SalesListings = new {
                        ids = "true"
                    }
                };
                break;

            case BlimpObject.SalesVoucherDeduction:
                o = new {
                    SalesVoucherDeductions = new {
                        ids = "true"
                    }
                };
                break;

            case BlimpObject.SalesListingExpense:
                o = new {
                    SalesListingExpenses = new {
                        ids = "true"
                    }
                };
                break;

            case BlimpObject.SalesVoucher:
                o = new {
                    SalesVouchers = new {
                        ids = "true"
                    }
                };
                break;

            case BlimpObject.ContactActivity:
                o = new {
                    ContactActivities = new {
                        ids = "true"
                    }
                };
                break;
        }
        request.AddBody(o);
        RestResponse oR = (RestResponse)client.Execute(request);
        return oR.Content;
    }

    private static Random r = null;

    private static Random rand {
        get {
            if (r == null) {
                r = new Random();
            }
            return r;
        }
    }

    public static string createIDTable(BlimpObject Type) {
        //Process any deletes that have happened
        string szResult = loadIDs(Type);

        if (string.IsNullOrWhiteSpace(szResult) || szResult.IndexOf("[") < 0 || szResult.IndexOf("]") < 0)
            return "";

        szResult = szResult.Substring(szResult.IndexOf("[") + 1);
        szResult = szResult.Substring(0, szResult.IndexOf("]"));

        if (string.IsNullOrWhiteSpace(szResult))
            return "";

        var intResult = szResult.Split(',').Select(n => Convert.ToInt32(n));

        string szTableName = String.Format("__IDCHECK_{0}_{1}", Type, rand.Next());

        DB.runNonQuery(string.Format(@"CREATE TABLE {0}([ID] [int] NOT NULL)", szTableName));

        SQLInsertQueue oSQL = new SQLInsertQueue(string.Format("INSERT INTO {0}(ID) VALUES", szTableName));
        foreach (int id in intResult)
            oSQL.addSQL(String.Format(@"({0})", id));

        oSQL.flush();

        return szTableName;
    }

    public static void removeIDTable(string IDTable) {
        DB.runNonQuery(string.Format("DROP TABLE {0}", IDTable));
    }

    public static void runFullImport(bool UserUpdate = false) {
        APILog.addLog(APISource.BoxDice, "Starting import");
        iPropertyType.importLatest();
        iPropertyCategory.importLatest();
        iConsultant.importLatest();
        iOffice.importLatest();
        iListingSource.importLatest();
        iProperty.importLatest();
        SalesListing.importLatest();
        iSalesVouchersCommissions.importLatest();
        iContactActivityType.importLatest();
        iContactCategoryType.importLatest();
        iContact.importLatest();
        iContactActivity.importLatest();
        iContactCategory.importLatest();
        iSalesVouchers.importLatest();
        iSalesListingExpenses.importLatest();
        iSalesVoucherDeductions.importLatest();
        iTask.importLatest();

        string szConfigName = "LASTUPDATE";
        AppConfigAdmin oConfigAdmin = new AppConfigAdmin();
        oConfigAdmin.addConfig(szConfigName, "-1");
        oConfigAdmin.loadValuesFromDB();
        oConfigAdmin.updateConfigValueSQL(szConfigName, Utility.formatDateTime(DateTime.Now));
        APILog.addLog(APISource.BoxDice, "Import completed");
        Sale.processBDImports(UserUpdate: UserUpdate);
        APILog.addLog(APISource.BoxDice, "Imported records processed");
    }
}

#region Office

public class OfficeRoot {

    public List<iOffice> offices {
        get;
        set;
    }
}

public class iOffice {

    public int id {
        get;
        set;
    }

    public string name {
        get;
        set;
    }

    public string fax {
        get;
        set;
    }

    public string email {
        get;
        set;
    }

    public static void importLatest() {
        var request = BlimpsHelper.getRequest(-1);
        var client = BlimpsHelper.getClient();
        object o = new {
            Offices = new {
                fields = new string[] { "id", "name" }
            }
        };

        request.AddBody(o);
        request.RootElement = "offices";
        RestResponse oR = (RestResponse)client.Execute(request);

        OfficeRoot oList = JsonConvert.DeserializeObject<OfficeRoot>(oR.Content);

        if (oList == null)
            return;

        foreach (iOffice Office in oList.offices) {
            Office.writeToDB();
        }
    }

    public void writeToDB() {
        string szSQL = String.Format(@"SELECT ID FROM bdOFFICE WHERE ID = {0}", id);
        int intDBID = DB.getScalar(szSQL, -1);
        sqlUpdate oSQL = new sqlUpdate("bdOFFICE", "ID", intDBID);
        oSQL.add("NAME", name);
        oSQL.add("ID", id);

        if (intDBID == -1) {
            oSQL.add("CODE", "");
            oSQL.add("ISACTIVE", true);
            DB.runNonQuery(oSQL.createInsertSQL());
        } else
            DB.runNonQuery(oSQL.createUpdateSQL());
    }
}

#endregion Office

#region OfficeCommissions

public class OfficeCommissionsRoot {

    public List<iOfficeCommissions> officeCommissions {
        get;
        set;
    }
}

public class officeCommissionsIDRoot {

    public List<string> officeCommissions {
        get;
        set;
    }
}

public class iOfficeCommissions {

    public int id {
        get;
        set;
    }

    public int officeId {
        get;
        set;
    }

    public float percentage {
        get;
        set;
    }

    public float amount {
        get;
        set;
    }

    public static void importLatest() {
        int intUpdateID = BlimpsHelper.getUpdateID(BlimpObject.OfficeCommission);
        int intMaxRecords = Int32.MaxValue;

        while (intUpdateID < intMaxRecords) {
            var request = BlimpsHelper.getRequest(intUpdateID);
            var client = BlimpsHelper.getClient();
            object o = new {
                OfficeCommissions = new {
                    fields = new string[] { "id", "voucherId", "amount", "percentage", "officeId" }
                }
            };

            request.AddBody(o);

            IRestResponse oR = client.Execute(request);
            if (oR.ErrorMessage != null)
                break;
            intUpdateID = BlimpsHelper.getContentRange(oR.Headers, ref intMaxRecords);
            OfficeCommissionsRoot oList = JsonConvert.DeserializeObject<OfficeCommissionsRoot>(oR.Content);
            if (oList == null)
                break;
            foreach (iOfficeCommissions o1 in oList.officeCommissions) {
                o1.writeToDB();
            }
            BlimpsHelper.setUpdateID(BlimpObject.OfficeCommission, intUpdateID);
        }
    }

    public void writeToDB() {
        string szSQL = String.Format(@"SELECT ID FROM bdOFFICECOMMISSION WHERE ID = {0}", id);
        int intDBID = DB.getScalar(szSQL, -1);
        sqlUpdate oSQL = new sqlUpdate("bdOFFICECOMMISSION", "ID", intDBID);
        oSQL.add("OFFICEID", officeId);
        oSQL.add("AMOUNT", amount);
        oSQL.add("PERCENTAGE", percentage);
        //oSQL.add("DESCRIPTION", description);
        //oSQL.add("percentage", grossCommission);

        try {
            if (intDBID == -1) {
                oSQL.add("ID", id);
                DB.runNonQuery(oSQL.createInsertSQL());
            } else
                DB.runNonQuery(oSQL.createUpdateSQL());
        } catch (Exception e) {
            if (!e.Message.Contains("INSERT statement conflicted")) {
                throw;
            }
        }
    }
}

#endregion OfficeCommissions

#region Team

public class TeamRoot {

    public List<Team> teams {
        get;
        set;
    }
}

public class Team {

    public int id {
        get;
        set;
    }

    public string name {
        get;
        set;
    }

    public static void importLatest() {
        var request = BlimpsHelper.getRequest(-1);
        var client = BlimpsHelper.getClient();
        object o = new {
            Teams = new {
                fields = new string[] { "id", "name" }
            }
        };

        request.AddBody(o);
        request.RootElement = "teams";

        RestResponse oR = (RestResponse)client.Execute(request);
        if (oR.StatusCode == System.Net.HttpStatusCode.InternalServerError) {
            HttpContext.Current.Response.Write("Box & Dice have reported an error loading the teams.");
            HttpContext.Current.Response.End();
        }
        TeamRoot oList = JsonConvert.DeserializeObject<TeamRoot>(oR.Content);

        foreach (Team oTeam in oList.teams) {
            oTeam.writeToDB();
        }
    }

    public void writeToDB() {
        string szSQL = String.Format(@"SELECT ID FROM bdTEAM WHERE ID = {0}", id);
        int intDBID = DB.getScalar(szSQL, -1);
        sqlUpdate oSQL = new sqlUpdate("bdTEAM", "ID", intDBID);
        oSQL.add("NAME", name);
        oSQL.add("ID", id);

        if (intDBID == -1)
            DB.runNonQuery(oSQL.createInsertSQL());
        else
            DB.runNonQuery(oSQL.createUpdateSQL());
    }
}

#endregion Team

#region Consultant

public class ConsultantList {

    public List<iConsultant> consultants {
        get;
        set;
    }
}

public class iConsultant {

    public int id {
        get;
        set;
    }

    public bool archived {
        get;
        set;
    }

    public string email {
        get;
        set;
    }

    public string mobile {
        get;
        set;
    }

    public string firstName {
        get;
        set;
    }

    public string initials {
        get;
        set;
    }

    public string lastName {
        get;
        set;
    }

    public int officeId {
        get;
        set;
    }

    public int[] teamIds {
        get;
        set;
    }

    public static void importLatest() {
        int intUpdateID = -1;
        int intMaxRecords = Int32.MaxValue;

        while (intUpdateID < intMaxRecords) {
            var request = BlimpsHelper.getRequest(intUpdateID);
            var client = BlimpsHelper.getClient();
            object o = new {
                Consultants = new {
                    fields = new string[] { "id", "email", "archived", "firstName", "lastName", "mobile", "officeId", "initials", "teamIds" }
                }
            };

            request.AddBody(o);
            request.RootElement = "consultants";
            IRestResponse oR = client.Execute(request);
            if (oR.StatusCode == System.Net.HttpStatusCode.InternalServerError) {
                HttpContext.Current.Response.Write("Box & Dice have reported an error loading the consultants. The error is: " + oR.ErrorMessage);
                HttpContext.Current.Response.End();
            }
            if (oR.ErrorMessage != null)
                break;

            intUpdateID = BlimpsHelper.getContentRange(oR.Headers, ref intMaxRecords);
            ConsultantList oAgentList = JsonConvert.DeserializeObject<ConsultantList>(oR.Content);
            foreach (iConsultant Agent in oAgentList.consultants) {
                if (!String.IsNullOrWhiteSpace(Agent.lastName)) {
                    Agent.writeToDB();
                }
            }
        }
    }

    public void writeToDB() {
        string szSQL = String.Format(@"
            SELECT * FROM bdDB_USER WHERE ID = {0};
            SELECT * FROM bdUSERTEAM WHERE USERID = {0};", id);
        using (DataSet ds = DB.runDataSet(szSQL)) {
            int intDBID = -1;
            DataRow dr = null;
            if (ds.Tables[0].Rows.Count > 0) {
                dr = ds.Tables[0].Rows[0];
                intDBID = Convert.ToInt32(dr["ID"]);
            }

            sqlUpdate oSQL = new sqlUpdate("bdDB_USER", "ID", intDBID);

            if (Utility.valueHasChanged(dr, "FIRSTNAME", firstName))
                oSQL.add("FIRSTNAME", firstName);
            if (Utility.valueHasChanged(dr, "ISACTIVE", !archived))
                oSQL.add("ISACTIVE", !archived);

            if (Utility.valueHasChanged(dr, "LASTNAME", lastName)) {
                oSQL.add("LASTNAME", lastName);
            }

            if (Utility.valueHasChanged(dr, "EMAIL", email))
                oSQL.add("EMAIL", email);
            if (Utility.valueHasChanged(dr, "TELEPHONE", mobile))
                oSQL.add("TELEPHONE", mobile);

            if (Utility.valueHasChanged(dr, "INITIALS", initials))
                oSQL.add("INITIALS", initials);
            if (Utility.valueHasChanged(dr, "OFFICEID", Convert.ToString(officeId)))
                oSQL.add("OFFICEID", officeId);
            if (intDBID == -1) {
                oSQL.add("ID", id);
                oSQL.add("ISDELETED", false);
                DB.runNonQuery(oSQL.createInsertSQL());
                intDBID = id;
            } else if (oSQL.HasUpdates) {
                oSQL.add("ID", id);
                oSQL.add("ISMODIFIED", 1);
                DB.runNonQuery(oSQL.createUpdateSQL());
            }
            if (teamIds == null) {
                DB.runNonQuery("DELETE FROM bdUSERTEAM WHERE TEAMID != 11 AND USERID = " + intDBID);
            } else {
                string szTeamIDs = String.Join(",", teamIds.Select(x => x.ToString()).ToArray());

                if (szTeamIDs == "") {
                    DB.runNonQuery("DELETE FROM bdUSERTEAM WHERE TEAMID != 11 AND USERID = " + intDBID);
                } else {
                    szSQL = String.Format(@"
                        DELETE FROM bdUSERTEAM WHERE USERID = {0} AND TEAMID NOT IN ({1}, 11);
                        INSERT INTO bdUSERTEAM(USERID, TEAMID)
                        SELECT {0}, ID
                        FROM bdTEAM T WHERE T.ID IN ({1}) AND ID NOT IN (SELECT TEAMID FROM bdUSERTEAM WHERE USERID = {0});", intDBID, szTeamIDs);
                    DB.runNonQuery(szSQL);
                }
            }
        }
    }
}

#endregion Consultant

#region Contact

public class ContactRoot {

    public List<iContact> contacts {
        get;
        set;
    }
}

public class iContact {

    public int id {
        get;
        set;
    }

    public string firstName {
        get;
        set;
    }

    public string lastName {
        get;
        set;
    }

    public string jobTitle {
        get;
        set;
    }

    public int residentialPropertyId {
        get;
        set;
    }

    public int consultantId {
        get;
        set;
    }

    public void writeToDB() {
        string szSQL = String.Format(@"
                SELECT ID FROM bdCONTACT WHERE ID = {0}
                SELECT ID FROM bdPROPERTY WHERE ID = {1}", id, residentialPropertyId);
        int intDBID = -1;
        using (DataSet ds = DB.runDataSet(szSQL)) {
            foreach (DataRow dr in ds.Tables[0].Rows)
                intDBID = DB.readInt(dr["ID"]);

            // Checks if Property ID still exists
            residentialPropertyId = Int32.MinValue;
            foreach (DataRow dr in ds.Tables[1].Rows)
                residentialPropertyId = DB.readInt(dr["ID"]);
        }

        sqlUpdate oSQL = new sqlUpdate("bdCONTACT", "ID", intDBID);
        oSQL.add("FIRSTNAME", firstName);
        oSQL.add("LASTNAME", lastName);
        oSQL.add("JOBTITLE", jobTitle);
        oSQL.add("CONSULTANTID", consultantId);
        if (residentialPropertyId > 0)
            oSQL.add("RESIDENTIALPROPERTYID", residentialPropertyId);
        oSQL.add("ID", id);
        if (intDBID == -1)
            DB.runNonQuery(oSQL.createInsertSQL());
        else
            DB.runNonQuery(oSQL.createUpdateSQL());
    }

    /// <summary>
    /// Imports latest data for given record ID
    /// </summary>
    /// <param name="ID"></param>
    /// <param name="forceReload"></param>
    public static void importRecord(int ID, Boolean forceReload = false) {
        int intUpdateID = forceReload ? -1 : BlimpsHelper.getUpdateID(BlimpObject.Contact);

        RestRequest request = BlimpsHelper.getRequest(intUpdateID);
        var client = BlimpsHelper.getClient();
        object o = new {
            Contacts = new {
                Id = ID,
                fields = new string[] { "id", "firstName", "lastName", "jobTitle", "residentialPropertyId", "consultantId" }
            }
        };

        request.AddBody(o);

        IRestResponse oR = client.Execute(request);
        ContactRoot oList = JsonConvert.DeserializeObject<ContactRoot>(oR.Content);

        if (oList == null)
            return;

        foreach (iContact oC in oList.contacts) {
            oC.writeToDB();
        }
    }

    /// <summary>
    /// Gets the data
    /// </summary>
    public static void importLatest() {
        int intUpdateID = BlimpsHelper.getUpdateID(BlimpObject.Contact);
        int intMaxRecords = Int32.MaxValue;

        while (intUpdateID < intMaxRecords) {
            var request = BlimpsHelper.getRequest(intUpdateID);
            var client = BlimpsHelper.getClient();
            object o = new {
                Contacts = new {
                    fields = new string[] { "id", "firstName", "lastName", "jobTitle", "residentialPropertyId" }
                }
            };

            request.AddBody(o);
            request.RootElement = "Contacts";
            IRestResponse oR = client.Execute(request);
            intUpdateID = BlimpsHelper.getContentRange(oR.Headers, ref intMaxRecords);
            ContactRoot oContactaList = JsonConvert.DeserializeObject<ContactRoot>(oR.Content);
            if (oContactaList == null)
                return;

            foreach (iContact oC in oContactaList.contacts) {
                oC.writeToDB();
            }
            BlimpsHelper.setUpdateID(BlimpObject.Contact, intUpdateID);
        }
    }
}

#endregion Contact

#region Contact

#region ContactActivityTypes

public class iContactActivityType {

    public int id {
        get;
        set;
    }

    public string name {
        get;
        set;
    }

    public void writeToDB() {
        string szSQL = String.Format(@"SELECT ID FROM bdCONTACTACTIVITYTYPE WHERE ID = {0}", id);
        int intDBID = DB.getScalar(szSQL, -1);
        sqlUpdate oSQL = new sqlUpdate("bdCONTACTACTIVITYTYPE", "ID", intDBID);
        oSQL.add("NAME", name);
        oSQL.add("ID", id);
        if (intDBID == -1)
            DB.runNonQuery(oSQL.createInsertSQL());
        else
            DB.runNonQuery(oSQL.createUpdateSQL());
    }

    /// <summary>
    /// Gets the data
    /// </summary>
    public static void importLatest() {
        var request = BlimpsHelper.getRequest(-1);
        var client = BlimpsHelper.getClient();
        object o = new {
            ContactActivityTypes = new {
                fields = new string[] { "id", "name" }
            }
        };

        request.AddBody(o);
        request.RootElement = "contactActivityTypes";
        var lData = client.Execute<List<iContactActivityType>>(request);
        if (lData == null || lData.Data == null)
            return;

        foreach (iContactActivityType oPT in lData.Data) {
            oPT.writeToDB();
        }
    }
}

#endregion ContactActivityTypes

public class ContactActivityRoot {

    public List<iContactActivity> contactActivities {
        get;
        set;
    }
}

public class iContactActivity {

    public int id {
        get;
        set;
    }

    public int propertyId {
        get;
        set;
    }

    public int contactId {
        get;
        set;
    }

    public int salesListingId {
        get;
        set;
    }

    public int typeId {
        get;
        set;
    }

    public string startedOn {
        get;
        set;
    }

    public string endedOn {
        get;
        set;
    }

    /// <summary>
    /// True if this is a record we wish to store on DB
    /// </summary>
    public bool storeRecord {
        get {
            return typeId == 7 || typeId == 9 || typeId == 10 || typeId == 11 || typeId == 12 || typeId == 13 || typeId == 15 || typeId == 17 || typeId == 18;
        }
    }

    public void writeToDB() {
        string szSQL = String.Format(@"SELECT ID FROM bdCONTACTACTIVITY WHERE ID = {0}", id);
        int intDBID = DB.getScalar(szSQL, -1);
        sqlUpdate oSQL = new sqlUpdate("bdCONTACTACTIVITY", "ID", intDBID);
        if (salesListingId == 0)
            return; //If theres not matching sales listing ID we don't care about this client contact type
        oSQL.add("PROPERTYID", propertyId);
        oSQL.add("CONTACTID", contactId);

        if (!storeRecord)
            return; // record not to be stored

        oSQL.add("CONTACTACTIVITYTYPEID", typeId);
        oSQL.add("SALESLISTINGID", salesListingId);
        if (Utility.isDateTime(startedOn))
            oSQL.add("STARTDATE", Utility.formatDate(startedOn));
        else
            oSQL.addNull("STARTDATE");

        if (Utility.isDateTime(endedOn))
            oSQL.add("ENDDATE", Utility.formatDate(endedOn));
        else
            oSQL.addNull("ENDDATE");
        try {
            if (intDBID == -1) {
                oSQL.add("ID", id);

                DB.runNonQuery(oSQL.createInsertSQL());
            } else
                DB.runNonQuery(oSQL.createUpdateSQL());
        } catch (Exception e) {
            if (!e.Message.Contains("FK_CONTACTACTIVITY_SALESLISTING"))
                throw;
        }
    }

    /// <summary>
    /// Imports latest data for given record ID including iContact record
    /// </summary>
    /// <param name="ListingID">Sale Listing ID</param>
    /// <param name="forceReload"></param>
    public static void importRecord(int ListingID, Boolean forceReload = false) {
        int intUpdateID = forceReload ? -1 : BlimpsHelper.getUpdateID(BlimpObject.ContactActivity);

        RestRequest request = BlimpsHelper.getRequest(intUpdateID);
        var client = BlimpsHelper.getClient();
        object o = new {
            contactActivities = new {
                salesListingId = ListingID,
                fields = new string[] { "id", "contactId", "propertyId", "salesListingId", "typeId", "startedOn", "endedOn" }
            }
        };

        request.AddBody(o);

        IRestResponse oR = client.Execute(request);
        ContactActivityRoot oList = JsonConvert.DeserializeObject<ContactActivityRoot>(oR.Content);

        if (oList == null)
            return;

        foreach (iContactActivity oCA in oList.contactActivities) {
            if (!oCA.storeRecord)
                continue; // this is a record that is not stored

            iContact.importRecord(oCA.contactId, forceReload);
            oCA.writeToDB();
        }
    }

    /// <summary>
    /// Gets the data
    /// </summary>
    public static void importLatest() {
        int intUpdateID = BlimpsHelper.getUpdateID(BlimpObject.ContactActivity);
        int intMaxRecords = Int32.MaxValue;

        while (intUpdateID < intMaxRecords) {
            var request = BlimpsHelper.getRequest(intUpdateID);
            var client = BlimpsHelper.getClient();
            object o = new {
                contactActivities = new {
                    fields = new string[] { "id", "contactId", "propertyId", "salesListingId", "typeId", "startedOn", "endedOn" }
                }
            };

            request.AddBody(o);
            request.RootElement = "contactActivities";
            IRestResponse oR = client.Execute(request);
            intUpdateID = BlimpsHelper.getContentRange(oR.Headers, ref intMaxRecords);
            ContactActivityRoot oContactActivityList = JsonConvert.DeserializeObject<ContactActivityRoot>(oR.Content);
            if (oContactActivityList == null)
                return;

            foreach (iContactActivity oC in oContactActivityList.contactActivities) {
                oC.writeToDB();
            }
            BlimpsHelper.setUpdateID(BlimpObject.ContactActivity, intUpdateID);
        }
    }
}

#region ContactCategoryTypes

public class iContactCategoryType {

    public int id {
        get;
        set;
    }

    public string name {
        get;
        set;
    }

    public void writeToDB() {
        string szSQL = String.Format(@"SELECT ID FROM bdCONTACTCATEGORYTYPE WHERE ID = {0}", id);
        int intDBID = DB.getScalar(szSQL, -1);
        sqlUpdate oSQL = new sqlUpdate("bdCONTACTCATEGORYTYPE", "ID", intDBID);
        oSQL.add("NAME", name);
        oSQL.add("ID", id);
        if (intDBID == -1)
            DB.runNonQuery(oSQL.createInsertSQL());
        else
            DB.runNonQuery(oSQL.createUpdateSQL());
    }

    /// <summary>
    /// Gets the data
    /// </summary>
    public static void importLatest() {
        var request = BlimpsHelper.getRequest(-1);
        var client = BlimpsHelper.getClient();
        object o = new {
            ContactCategoryTypes = new {
                fields = new string[] { "id", "name" }
            }
        };

        request.AddBody(o);
        request.RootElement = "contactCategoryTypes";
        var lData = client.Execute<List<iContactCategoryType>>(request);
        if (lData == null || lData.Data == null)
            return;

        foreach (iContactCategoryType oPT in lData.Data) {
            oPT.writeToDB();
        }
    }
}

#endregion ContactCategoryTypes

public class ContactCategoryRoot {

    public List<iContactCategory> contactCategories {
        get;
        set;
    }
}

public class iContactCategory {

    public int id {
        get;
        set;
    }

    public int consultantId {
        get;
        set;
    }

    public int contactId {
        get;
        set;
    }

    public int typeId {
        get;
        set;
    }

    public void writeToDB() {
        string szSQL = String.Format(@"SELECT ID FROM bdCONTACTCATEGORY WHERE ID = {0}", id);
        int intDBID = DB.getScalar(szSQL, -1);
        sqlUpdate oSQL = new sqlUpdate("bdCONTACTCATEGORY", "ID", intDBID);
        oSQL.add("CONSULTANTID", consultantId);
        oSQL.add("CONTACTID", contactId);
        oSQL.add("CONTACTCATEGORYTYPEID", typeId);
        try {
            if (intDBID == -1) {
                oSQL.add("ID", id);

                DB.runNonQuery(oSQL.createInsertSQL());
            } else
                DB.runNonQuery(oSQL.createUpdateSQL());
        } catch (Exception e) {
            ;
        }
    }

    /// <summary>
    /// Gets the data
    /// </summary>
    public static void importLatest() {
        int intUpdateID = BlimpsHelper.getUpdateID(BlimpObject.ContactCategory);
        int intMaxRecords = Int32.MaxValue;

        while (intUpdateID < intMaxRecords) {
            var request = BlimpsHelper.getRequest(intUpdateID);
            var client = BlimpsHelper.getClient();
            object o = new {
                contactCategories = new {
                    fields = new string[] { "id", "contactId", "consultantId", "typeId" }
                }
            };

            request.AddBody(o);
            request.RootElement = "contactCategories";
            IRestResponse oR = client.Execute(request);
            intUpdateID = BlimpsHelper.getContentRange(oR.Headers, ref intMaxRecords);
            ContactCategoryRoot oContactCategoryList = JsonConvert.DeserializeObject<ContactCategoryRoot>(oR.Content);
            if (oContactCategoryList == null)
                return;

            foreach (iContactCategory oC in oContactCategoryList.contactCategories) {
                oC.writeToDB();
            }
            BlimpsHelper.setUpdateID(BlimpObject.ContactCategory, intUpdateID);
        }
        updateContactCounts();
    }

    private static void updateContactCounts() {
        //Insert the current month value
        DB.runNonQuery(@"
            INSERT INTO bdAGENTCONTACTCOUNT(USERID, MONTHDATE)
            SELECT ID, DATEADD(m, DATEDIFF(m, 0, GETDATE()), 0)-- First day of month
            FROM bdDB_USER U
            WHERE U.ID NOT IN(SELECT USERID FROM bdAGENTCONTACTCOUNT WHERE MONTHDATE = DATEADD(m, DATEDIFF(m, 0, GETDATE()), 0))
            ");

        //Update the counts (now that we know they exist)
        using (DataSet ds = DB.runDataSet(@"
           SELECT U.ID, SUM(CASE WHEN CC.CONTACTCATEGORYTYPEID = 1 THEN 1 ELSE 0 END) AS CONTACTCOUNT,
            SUM(CASE WHEN CCT.NAME LIKE '%buyer bulletin%' THEN 1 ELSE 0 END) AS BUYERCOUNT
            FROM bdCONTACTCATEGORY CC
            JOIN bdCONTACTCATEGORYTYPE CCT ON CC.CONTACTCATEGORYTYPEID = CCT.ID
            JOIN bdDB_USER C ON CC.CONSULTANTID = C.ID
            JOIN DB_USER U on U.INITIALSCODE = C.INITIALS COLLATE Latin1_General_CI_AS
            GROUP BY U.ID
            ")) {
            foreach (DataRow dr in ds.Tables[0].Rows) {
                DB.runNonQuery(String.Format(@"
                    UPDATE bdAGENTCONTACTCOUNT SET CONTACTCOUNT = {0}, BUYERCOUNT = {1} WHERE USERID = {2} AND MONTHDATE = DATEADD(m, DATEDIFF(m, 0, GETDATE()), 0)
                ", DB.readInt(dr["CONTACTCOUNT"]), DB.readInt(dr["BUYERCOUNT"]), DB.readInt(dr["ID"])));
            }
        }
    }
}

#endregion Contact

#region Property

public class PropertyRoot {

    public List<iProperty> properties {
        get;
        set;
    }
}

public class iProperty {

    public int id {
        get;
        set;
    }

    public int ts {
        get;
        set;
    }

    public int categoryId {
        get;
        set;
    }

    public string address {
        get;
        set;
    }

    public string postcode {
        get;
        set;
    }

    public string state {
        get;
        set;
    }

    public string unit {
        get;
        set;
    }

    public string suburb {
        get;
        set;
    }

    public string landMeasure {
        get;
        set;
    }

    public string landSize {
        get;
        set;
    }

    public int typeId {
        get;
        set;
    }

    public string yearBuilt {
        get;
        set;
    }

    public int rooms {
        get;
        set;
    }

    public int beds {
        get;
        set;
    }

    public string landDescription {
        get;
        set;
    }

    public string houseSize {
        get;
        set;
    }

    public string houseMeasure {
        get;
        set;
    }

    public string houseDescription {
        get;
        set;
    }

    public iProperty() {
    }

    public static void importRecord(int ID, Boolean forceReload = false) {
        int intUpdateID = forceReload ? -1 : BlimpsHelper.getUpdateID(BlimpObject.Property);

        RestRequest request = BlimpsHelper.getRequest(intUpdateID);
        var client = BlimpsHelper.getClient();
        object o = new {
            Properties = new {
                id = ID,
                fields = new string[] { "id", "ts", "address", "postcode", "state", "suburb", "unit", "typeId", "categoryId", "houseDescription", "houseMeasure", "houseSize", "landDescription", "landMeasure", "landSize", "rooms", "beds", "yearBuilt" }
            }
        };

        request.AddBody(o);
        request.RootElement = "properties";
        IRestResponse oR = client.Execute(request);
        PropertyRoot oPropertylist = JsonConvert.DeserializeObject<PropertyRoot>(oR.Content);
        if (oPropertylist == null)
            return;

        foreach (iProperty oP in oPropertylist.properties) {
            oP.writeToDB();
        }
    }

    public static void importLatest() {
        //ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
        int intUpdateID = BlimpsHelper.getUpdateID(BlimpObject.Property);
        int intMaxRecords = Int32.MaxValue;

        while (intUpdateID < intMaxRecords) {
            RestRequest request = BlimpsHelper.getRequest(intUpdateID);
            var client = BlimpsHelper.getClient();
            object o = new {
                Properties = new {
                    fields = new string[] { "id", "ts", "address", "postcode", "state", "suburb", "unit", "typeId", "categoryId", "houseDescription", "houseMeasure", "houseSize", "landDescription", "landMeasure", "landSize", "rooms", "beds", "yearBuilt" }
                }
            };

            request.AddBody(o);
            request.RootElement = "properties";
            //Utility.WriteLine(ObjectDumper.Dump(request, 3));

            IRestResponse oR = client.Execute(request);
            intUpdateID = BlimpsHelper.getContentRange(oR.Headers, ref intMaxRecords);
            //Utility.WriteLine("--UPDATE ID: " + intUpdateID);
            PropertyRoot oPropertylist = JsonConvert.DeserializeObject<PropertyRoot>(oR.Content);
            if (oPropertylist == null)
                return;

            foreach (iProperty oP in oPropertylist.properties) {
                oP.writeToDB();
            }

            BlimpsHelper.setUpdateID(BlimpObject.Property, intUpdateID);
        }
    }

    public void writeToDB() {
        string szSQL = String.Format(@"SELECT ID FROM bdPROPERTY WHERE ID = {0}", id);
        int intDBID = DB.getScalar(szSQL, -1);
        sqlUpdate oSQL = new sqlUpdate("bdPROPERTY", "ID", intDBID);
        oSQL.add("ID", id);
        oSQL.add("TIMESTAMP", ts);
        oSQL.add("ADDRESS", address);
        oSQL.add("POSTCODE", postcode);
        oSQL.add("SUBURB", suburb);
        oSQL.add("UNIT", unit);
        oSQL.add("PROPERTYCATEGORYID", categoryId);
        oSQL.add("PROPERTYTYPEID", typeId);
        oSQL.add("LANDMEASURE", landMeasure);
        oSQL.add("LANDSIZE", landSize);
        oSQL.add("YEARBUILT", yearBuilt);
        oSQL.add("ROOMS", rooms);
        oSQL.add("BEDS", beds);
        oSQL.add("LANDDESCRIPTION", landDescription);
        oSQL.add("HOUSESIZE", houseSize);
        oSQL.add("HOUSEMEASURE", houseMeasure);
        oSQL.add("DESCRIPTION", houseDescription);
        if (intDBID == -1) {
            oSQL.add("ISACTIVE", 1);
            DB.runNonQuery(oSQL.createInsertSQL());
        } else
            DB.runNonQuery(oSQL.createUpdateSQL());
    }
}

#endregion Property

#region TestPing

public class TestPing {

    public static void importLatest() {
        int intUpdateID = 1432088795; // set as test value
        int intMaxRecords = Int32.MaxValue;

        //while (intUpdateID < intMaxRecords) {
        var request = BlimpsHelper.getRequest(intUpdateID);
        var client = BlimpsHelper.getPingClient();
        object o = new {
            SalesListings = new {
                fields = new string[] { "id", "ts", "propertyId", "officeID", "description", "auctioneerId", "passedInAuction", "passedInAuctionAt", "privateSaleEndsAt", "priceUndisclosed", "consultantIds", "listedOn", "status", "displayPrice", "vendorPrice", "internetHits", "situationVerySensitive", "auctionAt", "listingType", "sourceId", "reportingOfficeId", "withdrawnOn" }
            }
        };

        request.AddBody(o);
        request.Timeout = 20 * 1000; //10 mins
        IRestResponse oR = client.Execute(request);
        if (oR.ErrorMessage != null)
            return;

        intUpdateID = BlimpsHelper.getContentRange(oR.Headers, ref intMaxRecords);

        SalesListingRoot oList = JsonConvert.DeserializeObject<SalesListingRoot>(oR.Content);

        if (oList == null)
            return;

        /*foreach (SalesListing oSL in oList.salesListings) {
            if (!String.IsNullOrWhiteSpace(oSL.propertyId.ToString()) && oSL.propertyId.ToString() != "0") {
                //oSL.writeToDB();
            }
        }*/

        //BlimpsHelper.setUpdateID(BlimpObject.SalesListing, intUpdateID);
        //}
    }
}

#endregion TestPing

#region SalesListing

public class SalesListingRoot {

    public List<SalesListing> salesListings {
        get;
        set;
    }
}

public class SalesListing {

    public int id {
        get;
        set;
    }

    public long ts {
        get;
        set;
    }

    public int propertyId {
        get;
        set;
    }

    public int auctioneerId {
        get;
        set;
    }

    public string description {
        get;
        set;
    }

    public string internetHits {
        get;
        set;
    }

    public string displayPrice {
        get;
        set;
    }

    public string listedOn {
        get;
        set;
    }

    public string auctionAt {
        get;
        set;
    }

    public string privateSaleEndsAt {
        get;
        set;
    }

    public string status {
        get;
        set;
    }

    public string listingType {
        get;
        set;
    }

    public int[] consultantIds {
        get;
        set;
    }

    public int sourceId {
        get;
        set;
    }

    public int officeId {
        get;
        set;
    }

    public int reportingOfficeId {
        get;
        set;
    }

    public bool priceUndisclosed {
        get;
        set;
    }

    public bool situationVerySensitive {
        get;
        set;
    }

    public bool passedInAuction {
        get;
        set;
    }

    public string passedInAuctionAt {
        get;
        set;
    }

    public long vendorPrice {
        get;
        set;
    }

    public string withdrawnOn {
        get;
        set;
    }

    public SalesListing() {
    }

    /// <summary>
    /// Imports record and all related record (property, saleslisting, salesvoucher, contactactivity, contact)
    /// </summary>
    /// <param name="ID"></param>
    /// <param name="forceReload"></param>
    public static void importCompleteRecord(int ID, Boolean forceReload = false) {
        int intPropertyID = Int32.MinValue;

        // get propertyID
        RestRequest request = BlimpsHelper.getRequest(-1);
        var client = BlimpsHelper.getClient();
        object o = new {
            SalesListings = new {
                id = ID,
                fields = new string[] { "id", "ts", "propertyId", "officeID", "description", "auctioneerId", "passedInAuction", "passedInAuctionAt", "privateSaleEndsAt", "priceUndisclosed", "consultantIds", "listedOn", "status", "displayPrice", "vendorPrice", "internetHits", "situationVerySensitive", "auctionAt", "listingType", "sourceId", "reportingOfficeId", "withdrawnOn" }
            }
        };

        request.AddBody(o);

        IRestResponse oR = client.Execute(request);
        SalesListingRoot oList = JsonConvert.DeserializeObject<SalesListingRoot>(oR.Content);

        if (oList == null)
            return;

        foreach (SalesListing oSL in oList.salesListings) {
            if (!String.IsNullOrWhiteSpace(oSL.propertyId.ToString()) && oSL.propertyId.ToString() != "0") {
                intPropertyID = oSL.propertyId;
            }
        }

        if (intPropertyID == Int32.MinValue)
            return;

        iProperty.importRecord(intPropertyID, forceReload);
        importRecord(ID, forceReload);
        iSalesVouchers.importRecord(ID, forceReload);
        iContactActivity.importRecord(ID, forceReload);
        Sale.processBDImports(BnDSalesID: ID);
    }

    /// <summary>
    /// Imports latest data for given record ID
    ///
    /// TODO: Do we need to get the update ID in this scenario?
    /// </summary>
    /// <param name="ID"></param>
    /// <param name="forceReload"></param>
    public static void importRecord(int ID, Boolean forceReload = false) {
        int intUpdateID = forceReload ? -1 : BlimpsHelper.getUpdateID(BlimpObject.SalesListing);

        RestRequest request = BlimpsHelper.getRequest(intUpdateID);
        var client = BlimpsHelper.getClient();
        object o = new {
            SalesListings = new {
                id = ID,
                fields = new string[] { "id", "ts", "propertyId", "officeID", "description", "auctioneerId", "passedInAuction", "passedInAuctionAt", "privateSaleEndsAt", "priceUndisclosed", "consultantIds", "listedOn", "status", "displayPrice", "vendorPrice", "internetHits", "situationVerySensitive", "auctionAt", "listingType", "sourceId", "reportingOfficeId", "withdrawnOn" }
            }
        };

        request.AddBody(o);

        IRestResponse oR = client.Execute(request);
        SalesListingRoot oList = JsonConvert.DeserializeObject<SalesListingRoot>(oR.Content);

        if (oList == null)
            return;

        foreach (SalesListing oSL in oList.salesListings) {
            if (!String.IsNullOrWhiteSpace(oSL.propertyId.ToString()) && oSL.propertyId.ToString() != "0") {
                oSL.writeToDB();
            }
        }
    }

    public static void importLatest(int PropertyID = -1) {
        int intUpdateID = BlimpsHelper.getUpdateID(BlimpObject.SalesListing);
        int intMaxRecords = Int32.MaxValue;

        while (intUpdateID < intMaxRecords) {
            var request = BlimpsHelper.getRequest(intUpdateID);
            var client = BlimpsHelper.getClient();
            object o = new {
                SalesListings = new {
                    fields = new string[] { "id", "ts", "propertyId", "officeID", "description", "auctioneerId", "passedInAuction", "passedInAuctionAt", "privateSaleEndsAt", "priceUndisclosed", "consultantIds", "listedOn", "status", "displayPrice", "vendorPrice", "internetHits", "situationVerySensitive", "auctionAt", "listingType", "sourceId", "reportingOfficeId", "withdrawnOn" }
                }
            };

            request.AddBody(o);
            request.Timeout = 60 * 1000 * 10; //10 mins
            IRestResponse oR = client.Execute(request);
            if (oR.ErrorMessage != null)
                break;

            intUpdateID = BlimpsHelper.getContentRange(oR.Headers, ref intMaxRecords);
            DB.runNonQuery("-- Update ID from query:" + intUpdateID);
            SalesListingRoot oList = JsonConvert.DeserializeObject<SalesListingRoot>(oR.Content);

            if (oList == null)
                break;

            foreach (SalesListing oSL in oList.salesListings) {
                if (!String.IsNullOrWhiteSpace(oSL.propertyId.ToString()) && oSL.propertyId.ToString() != "0") {
                    oSL.writeToDB();
                }
            }

            BlimpsHelper.setUpdateID(BlimpObject.SalesListing, intUpdateID);
        }
        deleteListingRecords();
    }

    private static void deleteListingRecords() {
        string szIDTable = BlimpsHelper.createIDTable(BlimpObject.SalesListing);

        // unable to get results
        if (szIDTable == "")
            return;

        try {
            string szSQL = string.Format(@"
            DELETE FROM bdSALESLISTINGEXPENSE WHERE SALESLISTINGID IN (SELECT SL.ID FROM bdSALESLISTING SL left join {0} I ON I.ID = SL.ID where I.ID IS NULL);
            DELETE FROM bdCOMMISSION WHERE SALESVOUCHERID IN (
                SELECT ID FROM bdSALESVOUCHER  WHERE SALESLISTINGID IN (
                    SELECT SL.ID FROM bdSALESLISTING SL left join {0} I ON I.ID = SL.ID where I.ID IS NULL)
                );

            DELETE FROM bdSALESVOUCHER WHERE SALESLISTINGID IN (SELECT SL.ID FROM bdSALESLISTING SL left join {0} I ON I.ID = SL.ID where I.ID IS NULL);
            DELETE FROM bdCONTACTACTIVITY WHERE SALESLISTINGID IN (SELECT SL.ID FROM bdSALESLISTING SL left join {0} I ON I.ID = SL.ID where I.ID IS NULL);

            DELETE FROM bdSALESLISTING
            WHERE ID IN (
            SELECT SL.ID FROM bdSALESLISTING SL left join {0} I ON I.ID = SL.ID where I.ID IS NULL)", szIDTable);
            DB.runNonQuery(szSQL);
        } catch (Exception e) {; }//Ignore the error
        BlimpsHelper.removeIDTable(szIDTable);
    }

    public void writeToDB() {
        // Check if sale listing already exists.  Check if property data exists
        string szSQL = String.Format(@"
                -- 0.
                SELECT ID FROM bdSALESLISTING WHERE ID = {0}
                -- 1.
                SELECT ID FROM bdPROPERTY WHERE ID = {1}
                -- 2.
                SELECT * FROM bdLISTINGSOURCE WHERE ID = {2}", id, propertyId, sourceId);
        int intDBID = -1;
        bool blnPropertyExists;
        bool blnListingSourceExists;
        using (DataSet ds = DB.runDataSet(szSQL)) {
            foreach (DataRow dr in ds.Tables[0].Rows)
                intDBID = DB.readInt(dr["ID"]);

            blnPropertyExists = ds.Tables[1].Rows.Count > 0;

            blnListingSourceExists = ds.Tables[2].Rows.Count > 0;
        }

        if (!blnPropertyExists)
            return;

        sqlUpdate oSQL = new sqlUpdate("bdSALESLISTING", "ID", intDBID);
        oSQL.add("ID", id);
        oSQL.add("TIMESTAMP", ts);
        oSQL.add("PROPERTYID", propertyId);
        oSQL.add("OFFICEID", officeId);
        oSQL.add("REPORTINGOFFICEID", reportingOfficeId);
        oSQL.add("DESCRIPTION", description);
        oSQL.add("INTERNETHITS", internetHits);
        oSQL.add("DISPLAYPRICE", displayPrice);
        oSQL.add("VENDORPRICE", vendorPrice);
        if (consultantIds != null) {
            if (consultantIds.Length > 0)
                oSQL.add("CONSULTANT1ID", consultantIds[0]);
            if (consultantIds.Length > 1)
                oSQL.add("CONSULTANT2ID", consultantIds[1]);
            if (consultantIds.Length > 2)
                oSQL.add("CONSULTANT3ID", consultantIds[2]);
        }
        if (auctioneerId == 0)
            oSQL.addNull("AUCTIONEERID");
        else
            oSQL.add("AUCTIONEERID", auctioneerId);

        if (Utility.isDateTime(withdrawnOn))
            oSQL.add("WITHDRAWNON", Utility.formatDate(withdrawnOn));
        else
            oSQL.addNull("WITHDRAWNON");

        if (Utility.isDateTime(listedOn))
            oSQL.add("LISTEDDATE", Utility.formatDate(listedOn));
        else
            oSQL.addNull("LISTEDDATE");

        if (Utility.isDateTime(privateSaleEndsAt))
            oSQL.add("PRIVATESALEENDSAT", Utility.formatDateTime(Convert.ToDateTime(privateSaleEndsAt)));
        else
            oSQL.addNull("PRIVATESALEENDSAT");

        if (Utility.isDateTime(auctionAt))
            oSQL.add("AUCTIONAT", Utility.formatDateTime(Convert.ToDateTime(auctionAt)));
        else
            oSQL.addNull("AUCTIONAT");

        if (Utility.isDateTime(passedInAuctionAt))
            oSQL.add("PASSEDINAUCTIONAT", Utility.formatDateTime(Convert.ToDateTime(passedInAuctionAt)));
        else
            oSQL.addNull("PASSEDINAUCTIONAT");

        oSQL.add("STATUS", status);
        oSQL.add("PRICEUNDISCLOSED", priceUndisclosed);
        oSQL.add("SITUATIONVERYSENTITIVE", situationVerySensitive);
        oSQL.add("PASSEDINAUCTION", passedInAuction);

        oSQL.add("LISTINGTYPE", listingType);
        if (blnListingSourceExists)
            oSQL.add("LISTINGSOURCEID", sourceId);

        if (intDBID == -1)
            DB.runNonQuery(oSQL.createInsertSQL());
        else
            DB.runNonQuery(oSQL.createUpdateSQL());
    }
}

public class salesListingIDRoot {

    public List<string> salesListings {
        get;
        set;
    }
}

#endregion SalesListing

#region SalesVoucher

public class SalesVoucherRoot {

    public List<iSalesVouchers> salesVouchers {
        get;
        set;
    }
}

public class iSalesVouchers {

    public int id {
        get;
        set;
    }

    public int ts {
        get;
        set;
    }

    public int listingId {
        get;
        set;
    }

    public string description {
        get;
        set;
    }

    public string salePrice {
        get;
        set;
    }

    public string soldOn {
        get;
        set;
    }

    public string expectedSettlementOn {
        get;
        set;
    }

    public string expectedUnconditionalDate {
        get;
        set;
    }

    public string commissionDrawnDate {
        get;
        set;
    }

    public string saleStatus {
        get;
        set;
    }

    public string actualSettlementOn {
        get;
        set;
    }

    public string actualUnconditionalOn {
        get;
        set;
    }

    public float grossCommission {
        get;
        set;
    }

    public string grossCommissionCriteria {
        get;
        set;
    }

    /// <summary>
    /// Imports latest data for given record Sale Listing ID
    /// </summary>
    /// <param name="ListingID">Sale Listing ID</param>
    /// <param name="forceReload"></param>
    public static void importRecord(int ListingID, Boolean forceReload = false) {
        int intUpdateID = forceReload ? -1 : BlimpsHelper.getUpdateID(BlimpObject.SalesVoucher);

        RestRequest request = BlimpsHelper.getRequest(intUpdateID);
        var client = BlimpsHelper.getClient();
        object o = new {
            SalesVouchers = new {
                listingId = ListingID,
                fields = new string[] { "id", "ts", "listingId", "soldOn", "saleStatus", "actualSettlementOn", "expectedSettlementOn", "salePrice", "actualUnconditionalOn", "expectedUnconditionalDate", "commissionDrawnDate", "grossCommission", "grossCommissionCriteria" }
            }
        };

        request.AddBody(o);

        IRestResponse oR = client.Execute(request);
        SalesVoucherRoot oList = JsonConvert.DeserializeObject<SalesVoucherRoot>(oR.Content);

        if (oList == null)
            return;

        foreach (iSalesVouchers oSV in oList.salesVouchers) {
            oSV.writeToDB();
        }
    }

    public static void importLatest() {
        int intUpdateID = BlimpsHelper.getUpdateID(BlimpObject.SalesVoucher);
        int intMaxRecords = Int32.MaxValue;

        while (intUpdateID < intMaxRecords) {
            var request = BlimpsHelper.getRequest(intUpdateID);
            var client = BlimpsHelper.getClient();
            object o = new {
                SalesVouchers = new {
                    fields = new string[] { "id", "ts", "listingId", "soldOn", "saleStatus", "actualSettlementOn", "expectedSettlementOn", "salePrice", "actualUnconditionalOn", "expectedUnconditionalDate", "commissionDrawnDate", "grossCommission", "grossCommissionCriteria" }
                }
            };

            request.AddBody(o);

            IRestResponse oR = client.Execute(request);
            if (oR.ErrorMessage != null)
                break;
            intUpdateID = BlimpsHelper.getContentRange(oR.Headers, ref intMaxRecords);
            SalesVoucherRoot oList = JsonConvert.DeserializeObject<SalesVoucherRoot>(oR.Content);
            if (oList == null)
                return;
            foreach (iSalesVouchers oSL in oList.salesVouchers) {
                oSL.writeToDB();
            }
            BlimpsHelper.setUpdateID(BlimpObject.SalesVoucher, intUpdateID);
        }
    }

    public void writeToDB() {
        string szSQL = String.Format(@"SELECT ID FROM bdSALESVOUCHER WHERE ID = {0}", id);
        int intDBID = DB.getScalar(szSQL, -1);
        sqlUpdate oSQL = new sqlUpdate("bdSALESVOUCHER", "ID", intDBID);
        oSQL.add("SALESLISTINGID", listingId);
        oSQL.add("SALEPRICE", salePrice);
        if (grossCommissionCriteria == "INC_GST")
            grossCommission = (float)(grossCommission / 1.1);

        oSQL.add("GROSSCOMMISSION", grossCommission);
        oSQL.add("SALESTATUS", saleStatus);

        if (Utility.isDateTime(soldOn))
            oSQL.add("SOLDDATE", Utility.formatDate(soldOn));
        else
            oSQL.addNull("SOLDDATE");
        if (Utility.isDateTime(commissionDrawnDate))
            oSQL.add("COMMISSIONDRAWNDATE", Utility.formatDate(commissionDrawnDate));
        else
            oSQL.addNull("COMMISSIONDRAWNDATE");

        if (Utility.isDateTime(expectedSettlementOn))
            oSQL.add("EXPECTEDSETTLEMENT", Utility.formatDate(expectedSettlementOn));
        else
            oSQL.addNull("EXPECTEDSETTLEMENT");

        if (Utility.isDateTime(expectedUnconditionalDate))
            oSQL.add("EXPECTEDUNCONDITIONAL", Utility.formatDate(expectedUnconditionalDate));
        else
            oSQL.addNull("EXPECTEDUNCONDITIONAL");

        if (Utility.isDateTime(actualSettlementOn))
            oSQL.add("ACTUALSETTLEMENT", Utility.formatDate(actualSettlementOn));
        else
            oSQL.addNull("ACTUALSETTLEMENT");

        if (Utility.isDateTime(actualUnconditionalOn))
            oSQL.add("ACTUALUNCONDITIONAL", Utility.formatDate(actualUnconditionalOn));
        else
            oSQL.addNull("ACTUALUNCONDITIONAL");

        try {
            if (intDBID == -1) {
                oSQL.add("ID", id);
                DB.runNonQuery(oSQL.createInsertSQL());
            } else
                DB.runNonQuery(oSQL.createUpdateSQL());
        } catch (Exception e) {
            if (e.Message.Contains("FK_SALESVOUCHER_SALESLISTING")) {
                LogException oL = new LogException();
                oL.HandleException(e);
                oL = null;
            } else {
                throw e;
            }
        }
    }
}

#endregion SalesVoucher

#region SalesVoucherDeductions

public class SalesVoucherDeductionsRoot {

    public List<iSalesVoucherDeductions> salesVoucherDeductions {
        get;
        set;
    }
}

public class salesVoucherDeductionsIDRoot {

    public List<string> salesVoucherDeductions {
        get;
        set;
    }
}

public class iSalesVoucherDeductions {

    public int id {
        get;
        set;
    }

    public int voucherId {
        get;
        set;
    }

    public int svoucherCommissionId {
        get;
        set;
    }

    public string reason {
        get;
        set;
    }

    public string description {
        get;
        set;
    }

    public float percentage {
        get;
        set;
    }

    public float amount {
        get;
        set;
    }

    public string criteria {
        get;
        set;
    }

    public static void importLatest() {
        /*        // temp work around
                using (DataSet ds = DB.runDataSet(string.Format("SELECT ID FROM SALESVOUCHER WHERE SOLDDATE >= '{0}'", Utility.formatDate(G.TransitionToBnDDate)))) {
                    foreach (DataRow dr in ds.Tables[0].Rows) {
                        importRecord(DB.readInt(dr["ID"]), true);
                    }
                }
                deleteRecords();
                return;*/

        int intUpdateID = BlimpsHelper.getUpdateID(BlimpObject.SalesVoucherDeduction);
        int intMaxRecords = Int32.MaxValue;

        while (intUpdateID < intMaxRecords) {
            var request = BlimpsHelper.getRequest(intUpdateID);
            var client = BlimpsHelper.getClient();
            object o = new {
                SalesVoucherDeductions = new {
                    fields = new string[] { "id", "voucherId", "criteria", "amount", "reason", "description", "svoucherCommissionId" }
                }
            };

            request.AddBody(o);

            IRestResponse oR = client.Execute(request);
            if (oR.ErrorMessage != null) {
                APILog.addLog(APISource.BoxDice, "SalesVoucherDeduction : " + oR.ErrorMessage);
                break;
            }
            intUpdateID = BlimpsHelper.getContentRange(oR.Headers, ref intMaxRecords);
            SalesVoucherDeductionsRoot oList = JsonConvert.DeserializeObject<SalesVoucherDeductionsRoot>(oR.Content);
            if (oList == null)
                break;
            foreach (iSalesVoucherDeductions oSL in oList.salesVoucherDeductions) {
                oSL.writeToDB();
            }
            BlimpsHelper.setUpdateID(BlimpObject.SalesVoucherDeduction, intUpdateID);
        }
        deleteRecords();
    }

    /// <summary>
    /// Imports latest data for given record Sale Listing ID
    /// </summary>
    /// <param name="ListingID">Sale Listing ID</param>
    /// <param name="forceReload"></param>
    public static void importRecord(int VoucherID, Boolean forceReload = false) {
        int intUpdateID = forceReload ? -1 : BlimpsHelper.getUpdateID(BlimpObject.SalesVoucherDeduction);

        RestRequest request = BlimpsHelper.getRequest(intUpdateID);
        var client = BlimpsHelper.getClient();
        object o = new {
            SalesVoucherDeductions = new {
                voucherId = VoucherID,
                fields = new string[] { "id", "voucherId", "criteria", "amount", "reason", "description", "svoucherCommissionId" }
            }
        };

        request.AddBody(o);

        IRestResponse oR = client.Execute(request);
        SalesVoucherDeductionsRoot oList = JsonConvert.DeserializeObject<SalesVoucherDeductionsRoot>(oR.Content);

        if (oList == null)
            return;

        // this shouldn't happen but there is a request with B&D to fix at their end
        if (oList.salesVoucherDeductions == null)
            return;

        foreach (iSalesVoucherDeductions oSV in oList.salesVoucherDeductions) {
            oSV.writeToDB();
        }
    }

    public void writeToDB() {
        string szSQL = String.Format(@"SELECT ID FROM bdSALESVOUCHERDEDUCTION WHERE ID = {0}", id);
        int intDBID = DB.getScalar(szSQL, -1);
        sqlUpdate oSQL = new sqlUpdate("bdSALESVOUCHERDEDUCTION", "ID", intDBID);
        oSQL.add("SALESVOUCHERID", voucherId);
        oSQL.add("AMOUNT", amount);
        oSQL.add("REASON", reason);
        oSQL.add("DESCRIPTION", description);
        oSQL.add("COMMISSIONID", svoucherCommissionId);
        oSQL.add("CRITERIA", criteria);
        //oSQL.add("percentage", grossCommission);

        try {
            if (intDBID == -1) {
                oSQL.add("ID", id);
                DB.runNonQuery(oSQL.createInsertSQL());
            } else
                DB.runNonQuery(oSQL.createUpdateSQL());
        } catch (Exception e) {
            if (!e.Message.Contains("INSERT statement conflicted")) {
                throw;
            }
        }
    }

    private static void deleteRecords() {
        string szIDTable = BlimpsHelper.createIDTable(BlimpObject.SalesVoucherDeduction);

        // unable to get results
        if (szIDTable == "")
            return;

        string szSQL = string.Format(@"
            DELETE FROM bdSALESVOUCHERDEDUCTION
            WHERE ID > -1 AND ID IN (
            SELECT C.ID FROM bdSALESVOUCHERDEDUCTION C left join {0} I ON I.ID = C.ID where I.ID IS NULL)", szIDTable);
        DB.runNonQuery(szSQL);

        BlimpsHelper.removeIDTable(szIDTable);
    }
}

#endregion SalesVoucherDeductions

#region SalesListingExpenses

public class SalesListingExpensesRoot {

    public List<iSalesListingExpenses> salesListingExpenses {
        get;
        set;
    }
}

public class iSalesListingExpenses {

    public int id {
        get;
        set;
    }

    public int listingId {
        get;
        set;
    }

    public int ts {
        get;
        set;
    }

    public bool incGST {
        get;
        set;
    }

    public string description {
        get;
        set;
    }

    public float cost {
        get;
        set;
    }

    public float receivedAmount {
        get;
        set;
    }

    public static void importLatest() {
        int intUpdateID = BlimpsHelper.getUpdateID(BlimpObject.SalesListingExpense);
        int intMaxRecords = Int32.MaxValue;

        while (intUpdateID < intMaxRecords) {
            var request = BlimpsHelper.getRequest(intUpdateID);
            var client = BlimpsHelper.getClient();
            object o = new {
                SalesListingExpenses = new {
                    fields = new string[] { "id", "listingId", "cost", "incGST", "description", "receivedAmount", "ts" }
                }
            };

            request.AddBody(o);

            IRestResponse oR = client.Execute(request);
            if (oR.ErrorMessage != null)
                break;
            intUpdateID = BlimpsHelper.getContentRange(oR.Headers, ref intMaxRecords);
            SalesListingExpensesRoot oList = JsonConvert.DeserializeObject<SalesListingExpensesRoot>(oR.Content);
            if (oList == null)
                return;
            foreach (iSalesListingExpenses oSL in oList.salesListingExpenses) {
                oSL.writeToDB();
            }
            BlimpsHelper.setUpdateID(BlimpObject.SalesListingExpense, intUpdateID);
        }
    }

    public void writeToDB() {
        string szSQL = String.Format(@"SELECT ID FROM bdSALESLISTINGEXPENSE WHERE ID = {0}", id);
        int intDBID = DB.getScalar(szSQL, -1);
        sqlUpdate oSQL = new sqlUpdate("bdSALESLISTINGEXPENSE", "ID", intDBID);
        oSQL.add("SALESLISTINGID", listingId);
        oSQL.add("RECEIVEDAMOUNT", receivedAmount);
        oSQL.add("COST", cost);
        oSQL.add("INCGST", incGST);
        oSQL.add("DESCRIPTION", description);
        oSQL.add("TIMESTAMP", ts);

        try {
            if (intDBID == -1) {
                oSQL.add("ID", id);
                DB.runNonQuery(oSQL.createInsertSQL());
            } else
                DB.runNonQuery(oSQL.createUpdateSQL());
        } catch (Exception e) {
            ;// throw;
        }
    }
}

#endregion SalesListingExpenses

#region SalesVoucherCommissions

public class SalesVoucherCommissionsRoot {

    public List<iSalesVouchersCommissions> salesVoucherCommissions {
        get;
        set;
    }
}

public class salesVoucherCommissionsIDRoot {

    public List<string> salesVoucherCommissions {
        get;
        set;
    }
}

public class iSalesVouchersCommissions {

    public int id {
        get;
        set;
    }

    public int consultantId {
        get;
        set;
    }

    public int voucherId {
        get;
        set;
    }

    public double introducedAmount {
        get;
        set;
    }

    public double introducedPercentage {
        get;
        set;
    }

    public double propertyValueAmount {
        get;
        set;
    }

    public double propertyValuePercentage {
        get;
        set;
    }

    public double splitAmount {
        get;
        set;
    }

    public double splitPercentage {
        get;
        set;
    }

    public string role {
        get;
        set;
    }

    private static void deleteRecords() {
        string szIDTable = BlimpsHelper.createIDTable(BlimpObject.SalesVoucherCommission);

        // unable to get results
        if (szIDTable == "")
            return;

        string szSQL = string.Format(@"
            DELETE FROM bdCOMMISSION
            WHERE ID IN (
            SELECT C.ID FROM bdCOMMISSION C left join {0} I ON I.ID = C.ID where I.ID IS NULL)", szIDTable);
        DB.runNonQuery(szSQL);
        BlimpsHelper.removeIDTable(szIDTable);
    }

    public static void importLatest() {
        int intUpdateID = BlimpsHelper.getUpdateID(BlimpObject.SalesVoucherCommission);
        int intMaxRecords = Int32.MaxValue;

        while (intUpdateID < intMaxRecords) {
            var request = BlimpsHelper.getRequest(intUpdateID);
            var client = BlimpsHelper.getClient();
            object o = new {
                SalesVoucherCommissions = new {
                    fields = new string[] { "id", "consultantId", "voucherId", "introducedAmount", "introducedPercentage", "propertyValueAmount", "propertyValuePercentage", "splitAmount", "splitPercentage", "role" }
                }
            };

            request.AddBody(o);

            IRestResponse oR = client.Execute(request);
            if (oR.ErrorMessage != null) {
                APILog.addLog(APISource.BoxDice, "SalesVoucherCommission : " + oR.ErrorMessage);
                break;
            }
            intUpdateID = BlimpsHelper.getContentRange(oR.Headers, ref intMaxRecords);
            SalesVoucherCommissionsRoot oList = JsonConvert.DeserializeObject<SalesVoucherCommissionsRoot>(oR.Content);
            if (oList == null)
                return;

            foreach (iSalesVouchersCommissions oSL in oList.salesVoucherCommissions) {
                if (oSL.voucherId > 0)
                    oSL.writeToDB();
            }
            BlimpsHelper.setUpdateID(BlimpObject.SalesVoucherCommission, intUpdateID);
        }
        deleteRecords();
    }

    public void writeToDB() {
        string szSQL = String.Format(@"SELECT ID FROM bdCOMMISSION WHERE ID = {0}", id);
        int intDBID = DB.getScalar(szSQL, -1);
        sqlUpdate oSQL = new sqlUpdate("bdCOMMISSION", "ID", intDBID);
        oSQL.add("ID", id);
        oSQL.add("SALESVOUCHERID", voucherId);
        oSQL.add("CONSULTANTID", consultantId);
        int RoleID = 1; //List
        if (role == "MANAGE")
            RoleID = 2;
        else if (role == "SELL")
            RoleID = 3;

        oSQL.add("ROLEID", RoleID);
        oSQL.add("INTRODUCEDAMOUNT", introducedAmount);
        oSQL.add("INTRODUCEDPERCENT", introducedPercentage);
        oSQL.add("PROPERTYAMOUNT", propertyValueAmount);
        oSQL.add("PROPERTYPERCENT", propertyValuePercentage);
        oSQL.add("SPLITAMOUNT", splitAmount);
        oSQL.add("SPLITPERCENT", splitPercentage);

        try {
            if (intDBID == -1) {
                DB.runNonQuery(oSQL.createInsertSQL());
            } else
                DB.runNonQuery(oSQL.createUpdateSQL());
        } catch (Exception e) {
            ;// throw;
        }
    }
}

#endregion SalesVoucherCommissions

#region PropertyCategory

public class iPropertyCategory {

    public int id {
        get;
        set;
    }

    public string name {
        get;
        set;
    }

    public int typeID {
        get;
        set;
    }

    public void writeToDB() {
        string szSQL = String.Format(@"SELECT ID FROM bdPROPERTYCATEGORY WHERE ID = {0}", id);
        int intDBID = DB.getScalar(szSQL, -1);
        sqlUpdate oSQL = new sqlUpdate("bdPROPERTYCATEGORY", "ID", intDBID);
        oSQL.add("NAME", name);
        oSQL.add("PROPERTYTYPEID", typeID);
        oSQL.add("ID", id);
        if (intDBID == -1)
            DB.runNonQuery(oSQL.createInsertSQL());
        else
            DB.runNonQuery(oSQL.createUpdateSQL());
    }

    /// <summary>
    /// Gets the data
    /// </summary>
    public static void importLatest() {
        var request = BlimpsHelper.getRequest(-1);
        var client = BlimpsHelper.getClient();
        object o = new {
            PropertyCategories = new {
                fields = new string[] { "id", "name", "typeID" }
            }
        };

        request.AddBody(o);
        request.RootElement = "propertyCategories";
        var lData = client.Execute<List<iPropertyCategory>>(request);
        if (lData == null || lData.Data == null)
            return;

        foreach (iPropertyCategory oPT in lData.Data) {
            oPT.writeToDB();
        }
    }
}

#endregion PropertyCategory

#region PropertyType

public class iPropertyType {

    public int id {
        get;
        set;
    }

    public string name {
        get;
        set;
    }

    public string key {
        get;
        set;
    }

    public void writeToDB() {
        string szSQL = String.Format(@"SELECT ID FROM bdPROPERTYTYPE WHERE ID = {0}", id);
        int intDBID = DB.getScalar(szSQL, -1);
        sqlUpdate oSQL = new sqlUpdate("bdPROPERTYTYPE", "ID", intDBID);
        oSQL.add("NAME", name);
        //oSQL.add("CODE", key);
        oSQL.add("ID", id);
        if (intDBID == -1)
            DB.runNonQuery(oSQL.createInsertSQL());
        else
            DB.runNonQuery(oSQL.createUpdateSQL());
    }

    /// <summary>
    /// Gets the data
    /// </summary>
    public static void importLatest() {
        var request = BlimpsHelper.getRequest(-1);
        var client = BlimpsHelper.getClient();
        object o = new {
            PropertyTypes = new {
                fields = new string[] { "id", "name", "key" }
            }
        };

        request.AddBody(o);
        request.RootElement = "propertyTypes";
        var lData = client.Execute<List<iPropertyType>>(request);
        if (lData == null || lData.Data == null)
            return;
        foreach (iPropertyType oPT in lData.Data) {
            oPT.writeToDB();
        }
    }
}

#endregion PropertyType

#region ListingSource

public class iListingSource {

    public int id {
        get;
        set;
    }

    public string name {
        get;
        set;
    }

    /// <summary>
    /// Loads the lastest listing sources
    /// </summary>
    public static void importLatest() {
        var request = BlimpsHelper.getRequest(-1);
        var client = BlimpsHelper.getClient();
        object o = new {
            Sources = new {
                fields = new string[] { "id", "name" }
            }
        };

        request.AddBody(o);
        request.RootElement = "sources";
        var lData = client.Execute<List<iListingSource>>(request);
        string szIDList = "";

        if (lData.Data == null)
            return;

        foreach (iListingSource oLS in lData.Data) {
            if (!String.IsNullOrWhiteSpace(oLS.name)) {
                oLS.writeToDB();
                Utility.Append(ref szIDList, oLS.id.ToString(), ",");
            }
        }
        if (szIDList != "") {
            DB.runDataSet("DELETE FROM bdLISTINGSOURCE WHERE ID NOT IN (0," + szIDList + ") ");
        }
    }

    public void writeToDB() {
        string szSQL = String.Format(@"SELECT ID FROM bdLISTINGSOURCE WHERE ID = {0}", id);
        int intDBID = DB.getScalar(szSQL, -1);
        sqlUpdate oSQL = new sqlUpdate("bdLISTINGSOURCE", "ID", intDBID);
        oSQL.add("NAME", name);
        oSQL.add("ID", id);
        oSQL.add("ISCOMPANY", 0);
        if (intDBID == -1)
            DB.runNonQuery(oSQL.createInsertSQL());
        else
            DB.runNonQuery(oSQL.createUpdateSQL());
    }
}

#endregion ListingSource

#region Task

public class TaskRoot {

    public List<iTask> tasks {
        get;
        set;
    }
}

public class iTask {

    public int id {
        get;
        set;
    }

    public int consultantId {
        get;
        set;
    }

    public int propertyId {
        get;
        set;
    }

    public int[] contactIds {
        get;
        set;
    }

    public string actionOn {
        get;
        set;
    }

    public string completedOn {
        get;
        set;
    }

    public string subject {
        get;
        set;
    }

    public string content {
        get;
        set;
    }

    public string kind {
        get;
        set;
    }

    public int ts {
        get;
        set;
    }

    public void writeToDB() {
        string szSQL = String.Format(@"
                SELECT ID FROM bdTASK WHERE ID = {0}
                SELECT ID FROM bdPROPERTY WHERE ID = {1}", id, propertyId);
        int intDBID = -1;
        using (DataSet ds = DB.runDataSet(szSQL)) {
            foreach (DataRow dr in ds.Tables[0].Rows)
                intDBID = DB.readInt(dr["ID"]);
            if (ds.Tables[1].Rows.Count == 0 && propertyId > 0) {
                iProperty.importRecord(propertyId, forceReload: true);
                using (DataSet ds1 = DB.runDataSet(szSQL)) {
                    if (ds.Tables[1].Rows.Count == 0) {
                        propertyId = 0;
                    }
                }
            }
        }

        sqlUpdate oSQL = new sqlUpdate("bdTASK", "ID", intDBID);
        oSQL.add("ID", id);
        oSQL.add("CONSULTANTID", consultantId);
        if (propertyId > 0)
            oSQL.add("PROPERTYID", propertyId);
        if (!string.IsNullOrWhiteSpace(actionOn))
            oSQL.add("ACTIONON", Utility.formatDate(actionOn));
        if (!string.IsNullOrWhiteSpace(completedOn))
            oSQL.add("COMPLETEDON", Utility.formatDate(completedOn));
        oSQL.add("SUBJECTLINE", subject);
        oSQL.add("CONTENT", content);
        oSQL.add("KIND", kind);
        if (intDBID == -1)
            szSQL = oSQL.createInsertSQL();
        else
            szSQL = oSQL.createUpdateSQL();

        szSQL += string.Format(" DELETE FROM bdCONTACTTASK WHERE TASKID = {0}; ", id);

        if (contactIds != null && contactIds.Length > 0) {
            // Ensures link to contact is only written where contact exists
            string szContactSQL = string.Format("SELECT ID FROM bdCONTACT WHERE ID IN ({0})", string.Join(",", contactIds));
            using (DataSet ds = DB.runDataSet(szContactSQL)) {
                foreach (DataRow dr in ds.Tables[0].Rows) {
                    sqlUpdate oSQL1 = new sqlUpdate("bdCONTACTTASK", "TASKID", intDBID);
                    oSQL1.add("CONTACTID", DB.readInt(dr["ID"]));
                    oSQL1.add("TASKID", id);
                    szSQL += oSQL1.createInsertSQL();
                }
            }
        }

        DB.runNonQuery(szSQL);
    }

    /// <summary>
    /// Gets the data
    /// </summary>
    public static void importLatest() {
        int intUpdateID = BlimpsHelper.getUpdateID(BlimpObject.Task);
        int intMaxRecords = Int32.MaxValue;

        while (intUpdateID < intMaxRecords) {
            var request = BlimpsHelper.getRequest(intUpdateID);
            var client = BlimpsHelper.getClient();
            object o = new {
                Tasks = new {
                    fields = new string[] { "actionOn", "completedOn", "consultantId", "contactIds", "content", "id", "kind", "propertyId", "subject", "ts" }
                }
            };

            request.AddBody(o);
            request.RootElement = "Tasks";
            IRestResponse oR = client.Execute(request);
            intUpdateID = BlimpsHelper.getContentRange(oR.Headers, ref intMaxRecords);
            TaskRoot oTaskList = JsonConvert.DeserializeObject<TaskRoot>(oR.Content);
            if (oTaskList == null)
                return;

            foreach (iTask oC in oTaskList.tasks) {
                oC.writeToDB();
            }
            BlimpsHelper.setUpdateID(BlimpObject.Task, intUpdateID);
        }
    }
}

#endregion Task

public class PostCode {

    public string name {
        get;
        set;
    }

    public string postcode {
        get;
        set;
    }
}