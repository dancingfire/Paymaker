using System;
using System.Data;
using System.Web.UI.WebControls;

public partial class management_dashboard : Root {
    private DataSet dsCampaign = null;

    protected void Page_Load(object sender, System.EventArgs e) {
        showData();
    }

    private void showData() {
        dsCampaign = loadData();
        DataView dvSpend = dsCampaign.Tables[1].DefaultView;
        DataView dvInvoiced = dsCampaign.Tables[2].DefaultView;
        DataView dvPaid = dsCampaign.Tables[3].DefaultView;
        DataView dvOverDue = dsCampaign.Tables[4].DefaultView;
        DataView dvContributionStatus = dsCampaign.Tables[5].DefaultView;
        double dblTotalValue = 0;
        double dblTotalInvoiced = 0;
        double dblTotalOverdue = 0;
        double dblTotalCurrent = 0;
        int intNumberOverAuthority = 0;
        double dblTotalOverAuthority = 0;
        double dblSpent = 0;
        double dblBudget = 0;
        double dblPaid = 0;
        double dblPrePaid = 0;

        foreach (DataRow drC in dsCampaign.Tables[0].Rows) {
            //Get the campaign product total from the product table
            dvSpend.RowFilter = "ID = " + drC["ID"].ToString();
            dvInvoiced.RowFilter = "ID = " + drC["ID"].ToString();
            dvPaid.RowFilter = "ID = " + drC["ID"].ToString();
            dvOverDue.RowFilter = "ID = " + drC["ID"].ToString();
            dvContributionStatus.RowFilter = "ID = " + drC["ID"].ToString();
            foreach (DataRowView oRow in dvSpend) {
                // If this item has been reconciled, then use the vendor total, otherwise use the cost total
                if (Convert.ToDouble(oRow["VENDORTOTAL"]) == 0)
                    drC["TOTALSPENT"] = Utility.formatMoney(Convert.ToDouble(oRow["ACTUALTOTAL"]));
                else
                    drC["TOTALSPENT"] = Utility.formatMoney(Convert.ToDouble(oRow["VENDORTOTAL"]));
                drC["PRODUCTSINVOICED"] = Convert.ToInt32(oRow["PRODUCTSINVOICED"]);
                drC["PRODUCTCOUNT"] = Convert.ToInt32(oRow["PRODUCTCOUNT"]);
            }

            foreach (DataRowView oRow in dvInvoiced) {
                drC["TOTALINVOICED"] = Utility.formatMoney(Convert.ToDouble(oRow["TOTALINVOICED"]));
            }
            dblSpent = Convert.ToDouble(drC["TOTALSPENT"]);
            dblBudget = Convert.ToDouble(drC["APPROVEDBUDGET"]);

            foreach (DataRowView oRow in dvContributionStatus) {
                if (Convert.ToBoolean(oRow["ISPAYMENT"]) == false)
                    dblPaid += Convert.ToDouble(oRow["AMOUNT"]); //Include the amounts paid by the company or agent unless this is marked as a payment, which would double apply the result
                dblBudget += Convert.ToDouble(oRow["AMOUNT"]);
            }

            dblTotalInvoiced += Convert.ToDouble(drC["TOTALINVOICED"]);
            dblTotalValue += dblSpent;
            double dblAmountLeft = dblBudget - dblSpent;
            drC["AMOUNTLEFT"] = Utility.formatMoney(dblAmountLeft);

            if (Convert.ToInt32(dblAmountLeft) < 0) {
                dblTotalOverAuthority += dblSpent - dblBudget;
                intNumberOverAuthority++;
            }
            dblPaid = 0;

            foreach (DataRowView oRow in dvPaid) {
                dblPaid += Convert.ToDouble(oRow["TOTALPAID"]);
                DateTime dtPrePaidCutoff;
                dtPrePaidCutoff = Convert.ToDateTime(drC["STARTDATE"]).AddDays(G.Settings.PrePaymentNumberOfDays);

                if (Convert.ToDateTime(oRow["CONTRIBUTIONDATE"]) <= dtPrePaidCutoff)
                    dblPrePaid += Convert.ToDouble(oRow["TOTALPAID"]);
            }
            drC["TOTALPAID"] = Utility.formatMoney(dblPaid);

            foreach (DataRowView oRow in dvOverDue) {
                if (Convert.ToInt32(oRow["DAYSOWING"]) > 30)
                    drC["OVERDUE"] = Convert.ToDouble(drC["OVERDUE"]) + Convert.ToDouble(oRow["AMOUNT"]);
                else
                    drC["CURRENTDUE"] = Convert.ToDouble(drC["CURRENTDUE"]) + Convert.ToDouble(oRow["AMOUNT"]);
            }
            dblTotalOverdue += Convert.ToDouble(drC["OVERDUE"]);
            dblTotalCurrent += Convert.ToDouble(drC["CURRENTDUE"]);

            drC["TOTALOWING"] = Utility.formatMoney(Convert.ToDouble(drC["TOTALINVOICED"]) - Convert.ToDouble(drC["TOTALPAID"]));
        }
        lblTotalCampaigns.Text = dsCampaign.Tables[0].Rows.Count.ToString();
        lblTotalValue.Text = "$" + Utility.formatMoney(dblTotalValue);
        lblTotalNotInvoiced.Text = "$" + Utility.formatMoney(dblTotalValue - dblTotalInvoiced);
        lblTotalInvoiced.Text = "$" + Utility.formatMoney(dblTotalInvoiced);
        lblTotalCurrentOwing.Text = "$" + Utility.formatMoney(dblTotalCurrent);
        lblTotalOverDue.Text = "$" + Utility.formatMoney(dblTotalOverdue);
        lblTotalPrePayment.Text = "$" + Utility.formatMoney(dblPrePaid);
        lblNumberExceedingAuthority.Text = intNumberOverAuthority.ToString();
        lblTotalExceedingAuthority.Text = "$" + Utility.formatMoney(dblTotalOverAuthority);

        /*DataView dv = dsCampaign.Tables[0].DefaultView;
        dv.RowFilter = " AMOUNTLEFT < 0 ";
        gvCurrent.DataSource = dv;
        gvCurrent.DataBind();*/
    }

    protected void gvCurrent_RowDataBound(object sender, GridViewRowEventArgs e) {
        if (e.Row.RowType == DataControlRowType.DataRow) {
            string szID = ((DataRowView)e.Row.DataItem)["ID"].ToString();
            e.Row.Attributes["onclick"] = "updateCampaign(" + szID + ")";
        }
    }

    private DataSet loadData() {
        string szCampaignFilterSQL = String.Format(@"
                WHERE C.ISDELETED = 0 AND C.OFFICEID IN ({0})  AND C.STATUSID != {1}", G.Settings.CampaignTrackOffice, (int)CampaignStatus.Completed);
        string szContributionFilterSQL = "";

        string szSQL = String.Format(@"

            SELECT C.ID, 0.00 AS TOTALSPENT, 0.00 AS TOTALINVOICED, 0.00 AS TOTALPAID, 0.00 AS TOTALOWING, SUM(ISNULL(CC.AMOUNT, 0)) AS APPROVEDBUDGET, 0.00 AS AMOUNTLEFT, 0.00 AS CURRENTDUE, 0.00 AS OVERDUE,
            MAX(U.INITIALSCODE) AS AGENT,
                MAX(ADDRESS1) + ' ' + MAX(ADDRESS2) + ' <br/>VIC ' + MAX(CAST(POSTCODE AS VARCHAR)) AS ADDRESS, 0 AS PRODUCTSINVOICED, 0 AS PRODUCTCOUNT, C.ISDELETED,
                MAX(C.ORIGCAMPAIGNNUMBER) AS CAMPAIGNNUMBER, MAX(C.STARTDATE) AS STARTDATE, MAX(L_OFFICE.NAME) AS OFFICE, MAX(C.CAMPAIGNNUMBER) AS CAMPAIGNUNMBER
            FROM CAMPAIGN C
            LEFT JOIN CAMPAIGNCONTRIBUTION CC ON CC.CAMPAIGNID = C.ID AND CC.ISDELETED = 0 AND CC.CONTRIBUTIONTYPEID = {1} AND CC.RECEIPTNUMBER = '' AND CC.ISPAYMENT = 0
            LEFT JOIN DB_USER U ON C.AGENTID = U.ID
            LEFT JOIN LIST L_OFFICE ON U.OFFICEID = L_OFFICE.ID
            {0} {2}
            GROUP BY C.ID, C.ISDELETED
            ORDER BY STARTDATE DESC;

             SELECT C.ID, SUM(ISNULL(CP.VENDORPRICE, 0)) AS VENDORTOTAL,  SUM(ISNULL(CP.COSTPRICE, 0)) AS COSTTOTAL, SUM(ISNULL(CP.ACTUALPRICE, 0)) AS ACTUALTOTAL, 0 AS OVERDUE,
            SUM(CASE WHEN INVOICENO != '' THEN 1 ELSE 0 END) AS PRODUCTSINVOICED, COUNT(CP.ID) AS PRODUCTCOUNT, SUM(CASE WHEN P.EXCLUDEFROMINVOICE = 1 THEN 1 ELSE 0 END) AS PRODUCTSEXCLUDED
            FROM CAMPAIGN C LEFT JOIN CAMPAIGNPRODUCT CP ON CP.CAMPAIGNID = C.ID AND CP.ISDELETED = 0
            LEFT JOIN PRODUCT P ON P.ID = CP.PRODUCTID
            {0} AND CP.VENDORPRICE != 0
            GROUP BY C.ID;

            SELECT C.ID, SUM(ISNULL(CI.AMOUNT, 0)) AS TOTALINVOICED
            FROM CAMPAIGN C LEFT JOIN CAMPAIGNINVOICE CI ON CI.CAMPAIGNID = C.ID
            {0} AND CI.ISDELETED = 0
            GROUP BY C.ID;

            --Payments
            SELECT C.ID, ISNULL(CC.AMOUNT, 0) AS TOTALPAID, CC.CONTRIBUTIONDATE
            FROM CAMPAIGN C LEFT JOIN CAMPAIGNCONTRIBUTION CC ON CC.CAMPAIGNID = C.ID AND CC.ISDELETED = 0 AND CC.ISPAYMENT = 1
            {0} AND CC.CONTRIBUTIONDATE IS NOT NULL;

            SELECT C.ID, ISNULL(CC.AMOUNT, 0) AS AMOUNT, DUEDATE, ISNULL(DATEDIFF(d, DUEDATE, getdate()), 0) AS DAYSOWING
            FROM CAMPAIGN C LEFT JOIN  CAMPAIGNCONTRIBUTION CC ON C.ID = CC.CAMPAIGNID AND CC.CONTRIBUTIONTYPEID = {1} AND CC.ISDELETED = 0 AND CC.ISPAYMENT = 0
                LEFT JOIN CAMPAIGNCONTRIBUTIONSPLIT CCS  ON CCS.CAMPAIGNCONTRIBUTIONID = CC.ID
            {0}
            ORDER BY C.ID;

            SELECT C.ID, ISNULL(CC.FINANCESTATUSID, -1) AS STATUS, ISNULL(CC.AMOUNT, 0) AS AMOUNT, ISNULL(CC.ISPAYMENT, 0) AS ISPAYMENT
            FROM CAMPAIGN C LEFT JOIN CAMPAIGNCONTRIBUTION CC ON CC.CAMPAIGNID = C.ID AND CC.ISDELETED = 0 AND CC.CONTRIBUTIONTYPEID IN (1, 2) AND CC.AMOUNT != 0
            {0} ;
            ", szCampaignFilterSQL, (int)ContributionType.Vendor, szContributionFilterSQL);
        DataSet ds = DB.runDataSet(szSQL);
        return ds;
    }
}