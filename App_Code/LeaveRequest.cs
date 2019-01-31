using System;
using System.Data;

public enum LeaveRequestStatus {
    Requested = 0,
    Approved = 1,
    Rejected = 2
}
/// <summary>
/// Summary description for LeaveRequest
/// </summary>
public class LeaveRequest {
    public int intID { get; set;}
    public int UserID { get; set; }
    public int LeaveTypeID { get; set; }
    public LeaveRequestStatus LeaveStatus { get; set; }
    public DateTime EnteredDate { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalDays { get; set; }
    public string Comment { get; set; }

    /// <summary>
    /// Loads an object for the given transaction
    /// </summary>
    /// <param name="TxID"></param>
    public LeaveRequest(int DBID) {
        intID = DBID;
        string szSQL = String.Format("SELECT  * FROM LEAVEREQUEST where ID = {0}", intID);

        DataSet dsSale = DB.runDataSet(szSQL);
        foreach (DataRow dr in dsSale.Tables[0].Rows) {
            LeaveTypeID = Convert.ToInt32(dr["LEAVETYPEID"]);
            UserID = Convert.ToInt32(dr["USERID"]);
            LeaveStatus = (LeaveRequestStatus)Convert.ToInt32(dr["LEAVESTATUSID"]);
            TotalDays = Convert.ToInt32(dr["TotalDays"]);
            Comment = Convert.ToString(dr["COMMENTS"]);
            EnteredDate = DB.readDate(dr["ENTRYDATE"]);
            StartDate = DB.readDate(dr["STARTDATE"]);
            EndDate = DB.readDate(dr["ENDDATE"]);
        }
    }
}