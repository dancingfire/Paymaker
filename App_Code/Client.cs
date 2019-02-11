using System;
using System.Collections.Generic;
using System.Configuration;

public enum ClientID {
    HeadOffice = 0,
    Eltham = 1
}

public static class Client {

    //Gets the DBName based on the client
    public static string DBName {
        get {
            switch (G.Settings.ClientID) {
                case ClientID.Eltham: return "Eltham";
                case ClientID.HeadOffice: return "Paymaker";
            }
            return "PAYMAKER";
        }
    }

    /// <summary>
    /// How many months do we hold back the EOFY payment
    /// </summary>
    public static int EOFYBonusMonthDelay {
        get {
            return Convert.ToInt32(ConfigurationManager.AppSettings["EOFYBonusMonthDelay"].ToString());
        }
    }

    /// <summary>
    /// Returns the commission tiers for different clients using the application
    /// </summary>
    /// <param name="WhichClient"></param>
    /// <param name="dYTDBonusAmountAlreadyPaid"></param>
    /// <returns></returns>
    public static List<CommissionTier> getCommissionTiers(double dYTDBonusAmountAlreadyPaid, int UserID) {
        List<CommissionTier> lTiers = new List<CommissionTier>();
        return G.CTInfo.getCommissionTiersForUser(dYTDBonusAmountAlreadyPaid, UserID);
        /* switch(G.Settings.ClientID){
                 return G.CTInfo.getCommissionTiersForUser(dYTDBonusAmountAlreadyPaid);

             case ClientID.HeadOffice:
                 lTiers.Add(new CommissionTier(0, 374999, 0, 374999 - dYTDBonusAmountAlreadyPaid));
                 lTiers.Add(new CommissionTier(375000, 474999, 0.02, 474999 - dYTDBonusAmountAlreadyPaid));
                 lTiers.Add(new CommissionTier(475000, 649999, 0.05, 649999 - dYTDBonusAmountAlreadyPaid));
                 lTiers.Add(new CommissionTier(650000, 799999, 0.08, 799999 - dYTDBonusAmountAlreadyPaid));
                 lTiers.Add(new CommissionTier(800000, 899999, 0.15, 899999 - dYTDBonusAmountAlreadyPaid));
                 lTiers.Add(new CommissionTier(900000, 99999999, 0.20, 99999999 - dYTDBonusAmountAlreadyPaid));
                 break;

             case ClientID.Eltham:
                 lTiers.Add(new CommissionTier(0, 249999, 0, 249999 - dYTDBonusAmountAlreadyPaid));
                 lTiers.Add(new CommissionTier(250000, 499999, 0.10, 499999 - dYTDBonusAmountAlreadyPaid));
                 lTiers.Add(new CommissionTier(500000, 99999999, 0.20, 99999999 - dYTDBonusAmountAlreadyPaid));
                 break;*/
    }

    /// <summary>
    /// Returns the graph total percentages for the sale
    /// </summary>
    /// <param name="dFarm"></param>
    /// <param name="dLead"></param>
    /// <param name="dList"></param>
    /// <param name="dManage"></param>
    /// <param name="dSell"></param>
    /// <param name="dMentor"></param>
    /// <param name="blnHasFarm"></param>
    /// <param name="blnHasMentor"></param>
    public static void getGraphCommisionPercentage(ref double dFarm, ref double dLead, ref double dList, ref double dManage, ref double dSell, ref double dMentor, bool blnHasFarm, bool blnHasMentor) {
        switch (G.Settings.ClientID) {
            case ClientID.HeadOffice:
                dFarm = 0;
                dLead = 0.1111;
                dList = 0.4445;
                dManage = 0.2222;
                dSell = 0.2222;
                dMentor = 0;

                /*if (blnHasFarm && blnHasMentor) {
                    dFarm = 0.0833;
                    dLead = 0.0833;
                    dList = 0.3333;
                    dManage = 0.1667;
                    dSell = 0.1667;
                    dMentor = 0.1667;
                } else if (blnHasFarm) {
                    dFarm = 0.10;
                    dLead = 0.10;
                    dList = 0.40;
                    dManage = 0.20;
                    dSell = 0.20;
                    dMentor = 0;
                } else if (blnHasMentor) {
                    dFarm = 0;
                    dLead = 0.0909;
                    dList = 0.3637;
                    dManage = 0.1818;
                    dSell = 0.1818;
                    dMentor = 0.1818;
                }*/

                break;

            case ClientID.Eltham:
                dLead = 0.3023;
                dList = 0.4651;
                dSell = 0.2326;
                break;
        }
    }
}