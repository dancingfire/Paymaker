using System;
using System.Collections;
using System.Configuration;

/// <summary>
/// Summary description for sql.
/// </summary>
public class sqlUpdate {
    private string szTable;
    private string szPrimaryKey;
    private int intPrimaryKeyID;
    private string oCnn = ConfigurationSettings.AppSettings["DB"];
    private ArrayList oCol = new ArrayList();
    private ArrayList oValue = new ArrayList();
    private ArrayList oType = new ArrayList();

    public sqlUpdate() {
    }

    /// <summary>
    /// Are there updates ready to run for this object
    /// </summary>
    public bool HasUpdates {
        get {
            return oCol.Count > 0;
        }
    }

    public sqlUpdate(String szTableName, String szPK, int intID) {
        szTable = szTableName;
        szPrimaryKey = szPK;
        intPrimaryKeyID = intID;
    }

    public void add(String szCol, String szValue) {
        oCol.Add(szCol);
        if (String.IsNullOrWhiteSpace(szValue)) {
            oValue.Add("");
        } else {
            oValue.Add(szValue.Replace("'", "''").Trim());
        }
        oType.Add("string");
    }

    public void add(String szCol, int szValue) {
        oCol.Add(szCol);
        oValue.Add(szValue);
        oType.Add("int");
    }

    public void add(String szCol, bool Value) {
        oCol.Add(szCol);
        if (Value)
            oValue.Add("1");
        else
            oValue.Add("0");
        oType.Add("int");
    }

    public void add(String szCol, double Value) {
        if (Value == Double.MinValue) {
            addNull(szCol);
            return;
        }

        oCol.Add(szCol);
        oValue.Add(Value);
        oType.Add("int");
    }

    public void add(String szCol, System.Web.UI.WebControls.CheckBox oCheck) {
        oCol.Add(szCol);
        if (oCheck.Checked)
            oValue.Add("1");
        else
            oValue.Add("0");
        oType.Add("int");
    }

    public void add(String szCol, System.Web.UI.WebControls.RadioButton oCheck) {
        oCol.Add(szCol);
        if (oCheck.Checked)
            oValue.Add("1");
        else
            oValue.Add("0");
        oType.Add("int");
    }

    public void addNull(String szCol) {
        oCol.Add(szCol);
        oValue.Add(null);
        oType.Add("int");
    }

    public string createUpdateSQL() {
        string szSQL;
        szSQL = "UPDATE [" + szTable + "] SET ";
        System.Collections.IEnumerator eCol = oCol.GetEnumerator();
        System.Collections.IEnumerator eValue = oValue.GetEnumerator();
        System.Collections.IEnumerator eType = oType.GetEnumerator();
        while (eCol.MoveNext()) {
            eValue.MoveNext();
            eType.MoveNext();
            if (eValue.Current == null)
                szSQL += String.Format("[{0}] = null", eCol.Current);
            else if (eType.Current == "int")
                szSQL += String.Format("[{0}] = {1}", eCol.Current, eValue.Current);
            else
                szSQL += String.Format("[{0}] = '{1}'", eCol.Current, eValue.Current);
            szSQL += ", ";
        }

        szSQL = szSQL.Remove(szSQL.LastIndexOf(", "), 2);
        szSQL += " WHERE " + szPrimaryKey + " = " + intPrimaryKeyID.ToString();
        return szSQL;
    }

    /// <summary>
    /// Creates the update or insert statement based on whether the primary key is set
    /// </summary>
    /// <returns></returns>
    public string createSQL() {
        if (intPrimaryKeyID == -1)
            return createInsertSQL();
        else
            return createUpdateSQL();
    }

    public string createInsertSQL() {
        string szSQL;
        szSQL = "INSERT INTO  [" + szTable + "] ( ";

        // Where table name references Table 'path' square brackets around Table name invalidates call
        if (szTable.Contains("."))
            szSQL = "INSERT INTO  " + szTable + " ( ";

        System.Collections.IEnumerator eCol = oCol.GetEnumerator();
        System.Collections.IEnumerator eValue = oValue.GetEnumerator();
        System.Collections.IEnumerator eType = oType.GetEnumerator();
        while (eCol.MoveNext()) {
            szSQL += String.Format("[{0}],  ", eCol.Current);
        }

        szSQL = szSQL.Remove(szSQL.LastIndexOf(", "), 2);
        szSQL += ") VALUES (";
        while (eValue.MoveNext()) {
            eType.MoveNext();
            if (eValue.Current == null)
                szSQL += String.Format("null, ");
            else if (eType.Current == "int")
                szSQL += String.Format("{0}, ", eValue.Current);
            else
                szSQL += String.Format("'{0}', ", eValue.Current);
        }
        szSQL = szSQL.Remove(szSQL.LastIndexOf(", "), 2);
        szSQL += "); SELECT SCOPE_IDENTITY(); ";
        return szSQL;
    }
}