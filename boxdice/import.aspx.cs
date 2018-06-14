using System;
using System.Collections.Generic;
using System.Data;

namespace Paymaker {

    /// <summary>
    /// Summary description for about.
    /// </summary>
    public partial class import : Root {
        protected System.Web.UI.WebControls.Button Button2;

        protected void Page_Load(object sender, System.EventArgs e) {
            bool blnRun = Valid.getBoolean("blnRunAuto", false);
            if (blnRun) {
                importAll();
            }
        }

        protected void btnOffice_Click(object sender, EventArgs e) {
            iOffice.importLatest();
        }

        protected void btnTestPing_Click(object sender, EventArgs e) {
            TestPing.importLatest();
        }

        protected void btnConsultants_Click(object sender, EventArgs e) {
            Team.importLatest();
            iConsultant.importLatest();
        }

        protected void btnPropertyType_Click(object sender, EventArgs e) {
            iPropertyType.importLatest();
        }

        protected void btnExpense_Click(object sender, EventArgs e) {
            iSalesListingExpenses.importLatest();
        }

        protected void btnContactActivityType_Click(object sender, EventArgs e) {
            iContactActivityType.importLatest();
        }

        protected void btnContactActivity_Click(object sender, EventArgs e) {
            iContactActivity.importLatest();
        }

        protected void btnPropertyCategory_Click(object sender, EventArgs e) {
            iPropertyCategory.importLatest();
        }

        protected void btnProperty_Click(object sender, EventArgs e) {
            iProperty.importLatest();
        }

        protected void btnContactCategory_Click(object sender, EventArgs e) {
            iContactCategory.importLatest();
        }

        protected void btnContactCategoryType_Click(object sender, EventArgs e) {
            iContactCategoryType.importLatest();
        }

        protected void btnSalesListing_Click(object sender, EventArgs e) {
            SalesListing.importLatest();
        }

        protected void btnSalesVouchers_Click(object sender, EventArgs e) {
            iSalesVouchers.importLatest();
        }

        protected void btnDeductions_Click(object sender, EventArgs e) {
            iSalesVoucherDeductions.importLatest();
        }

        protected void btnCommission_Click(object sender, EventArgs e) {
            iSalesVouchersCommissions.importLatest();
        }

        protected void btnContacts_Click(object sender, EventArgs e) {
            iContact.importLatest();
        }

        protected void btnTasks_Click(object sender, EventArgs e) {
            iTask.importLatest();
        }

        protected void btnListingSource_Click(object sender, EventArgs e) {
            iListingSource.importLatest();
        }

        protected void btnTest_Click(object sender, EventArgs e) {
            importAll();
        }

        private void importAll() {
            BlimpsHelper.runFullImport();
        }



        protected void btnProcess_Click(object sender, EventArgs e) {
            Sale.processBDImports();
        }
    }

}