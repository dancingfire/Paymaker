//#define ENCRYPTEDSAML

using ComponentPro.Saml2;
using System;
using System.Net.Mail;
using System.Web;
using System.Web.UI;

public partial class ConsumerService : System.Web.UI.Page {

    protected override void OnLoad(EventArgs e) {
        base.OnLoad(e);
        G.User.RoleID = 1;
        G.User.UserID = 1;
        // Get the subject name identifier.
        string emailAddress = String.Empty;

        try {
            // Create a SAML response from the HTTP request.
            ComponentPro.Saml2.Response samlResponse = ComponentPro.Saml2.Response.Create(Request);

            // Success?
            if (!samlResponse.IsSuccess()) {
                throw new ApplicationException("SAML response is not success");
            }

            // Get the asserted identity.
            if (samlResponse.GetAssertions().Length > 0) {
                Assertion[] lAssertions = samlResponse.GetAssertions();
                foreach (Assertion s in samlResponse.GetAssertions()) {
                    foreach (AttributeStatement Statement in s.AttributeStatements) {
                        foreach (ComponentPro.Saml2.Attribute a in Statement.Attributes) {
                            if (a.Name != null && Convert.ToString(a.Name).EndsWith("claims/name")) {
                                emailAddress = ((AttributeValue)a.Values[0]).ToString();
                                break;
                            }
                        }
                    }
                }
            } else {
                throw new ApplicationException("No encrypted assertions found in the SAML response");
            }
        } catch (Exception exception) {
            Response.Write(exception.Message);
        }

        if (String.IsNullOrEmpty(emailAddress)) {
            Response.Write("You do not currently have access to GEARs. Please contact the site administrator to add you to the system. The email address we tried was: " + emailAddress);
            Response.End();
        }
        bool blnLoggedIn = UserLogin.loginUserByEmail(emailAddress);
        if (!blnLoggedIn) {
            Response.Write("You do not currently have access to GEARs. Please contact the site administrator to add you to the system. The email address we tried was: " + emailAddress);
        } else {
            string szStartPage = UserLogin.getStartPage();
            Response.Write(String.Format("<script>window.top.location.href = '../{0}';</script>", szStartPage));
       }
    }
}