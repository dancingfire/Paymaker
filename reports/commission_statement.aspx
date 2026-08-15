<%@ Page Language="C#" %>
<script runat="server">
    protected void Page_Load(object sender, EventArgs e) {
        //This report was renamed to commission_statement_new.aspx in 2018. Old bookmarks/saved links
        //still hit this path, so redirect (preserving query string) instead of 404ing.
        Response.RedirectPermanent("commission_statement_new.aspx" + Request.Url.Query, false);
    }
</script>
