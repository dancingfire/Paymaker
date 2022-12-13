using System.Security.Cryptography.X509Certificates;
using System;
using System.Web;
using System.Web.Security;
using System.Net;
using System.Net.Security;
using ComponentPro.Saml2;
using System.Web.UI;

public partial class SAMLLogin : System.Web.UI.Page {
    private const string ComponentProKey = "64CC87FDD820D900D455349E9F0AA83B8368DBE0D600C259F604B5B3983E954F";

    protected override void OnLoad(System.EventArgs e) {
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3;
        G.User.IsLeave = Request.QueryString["LEAVE"] != null;
        initiateLogin();
    }
    

    protected void initiateLogin() {
        ComponentPro.Licensing.Saml.LicenseManager.SetLicenseKey(ComponentProKey);
        
        // Set the server certificate validation callback.
        ServicePointManager.ServerCertificateValidationCallback = ValidateRemoteServerCertificate;

        // Create the authentication request.
        AuthnRequest a = BuildAuthenticationRequest();

        // Create and cache the relay state so we remember which SP resource the user wishes to access after SSO.
        string spResourceUrl = Util.GetAbsoluteUrl(this.Page, FormsAuthentication.GetRedirectUrl("", false));
        string relayState = Guid.NewGuid().ToString();
       
        // Send the authentication request to the identity provider over the selected binding.
        string idpUrl = string.Format("{0}?binding={1}", G.Settings.SAML.SSOServiceURL, HttpUtility.UrlEncode("urn:oasis:names:tc:SAML:2.0:bindings:HTTP-POST"));
        a.SendHttpPost(Response, idpUrl, relayState);
        Response.End();
    }
    
   
    /// <summary>
    /// Builds an authentication request.
    /// </summary>
    /// <returns>The authentication request.</returns>
    private AuthnRequest BuildAuthenticationRequest() {
        string issuerUrl = Util.GetAbsoluteUrl(this, "~/");
        string AssertionURL = String.Format("https://{0}/acs/ConsumerService.aspx", G.Settings.ServerName);
        string assertionConsumerServiceUrl = string.Format("{0}?binding={1}", AssertionURL, HttpUtility.UrlEncode("urn:oasis:names:tc:SAML:2.0:bindings:HTTP-POST"));

        // Create the authentication request.
        AuthnRequest authnRequest = new AuthnRequest();
        authnRequest.Destination = G.Settings.SAML.SSOServiceURL; ;
        authnRequest.Issuer = new Issuer(G.Settings.SAML.SSOClientID);
        authnRequest.ForceAuthn = false;
        authnRequest.NameIdPolicy = new NameIdPolicy(null, null, true);
        authnRequest.ProtocolBinding = "urn:oasis:names:tc:SAML:2.0:bindings:HTTP-POST";
        authnRequest.AssertionConsumerServiceUrl = AssertionURL;
        return authnRequest;
    }

    public class Util {

        public static string GetAbsoluteUrl(Page page, string relativeUrl) {
            return new Uri(page.Request.Url, page.ResolveUrl(relativeUrl)).ToString();
        }

    }
  

    /// <summary>
    /// Verifies the remote Secure Sockets Layer (SSL) certificate used for authentication.
    /// </summary>
    private static bool ValidateRemoteServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) {
        // NOTE: This is a test application with self-signed certificates, so all certificates are trusted.
        return true;
    }
}
