using System;
using System.Data;
using System.Text;
using System.Web.Script.Services;
using System.Web.Services;

[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[ScriptService]
public class ws_BoxDice : System.Web.Services.WebService {

    public ws_BoxDice() : base() {
    }

    [WebMethod(EnableSession = false)]
    public void runFullBDImport() {
        BlimpsHelper.runFullImport();
    }

}