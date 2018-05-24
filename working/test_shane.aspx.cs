using System;
using System.Collections.Generic;

public static class x {
}

public partial class test : Root {

    protected void Page_Load(object sender, EventArgs e) {
        G.TimeSheetCycleReferences = Payroll.getTimeSheetCycleReferences();

    }
}