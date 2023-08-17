<%@ Page Language="C#" AutoEventWireup="true" Inherits="logout" CodeFile="logout.aspx.cs" %>
<!DOCTYPE html>
<html>
<head runat="server">
<title>Logout</title>
<script src="https://stackpath.bootstrapcdn.com/bootstrap/3.3.7/js/bootstrap.min.js" type="text/javascript"></script>
<link href="https://stackpath.bootstrapcdn.com/bootstrap/3.3.7/css/bootstrap.min.css" rel="stylesheet" type="text/css" >
<link href="https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/css/bootstrap-theme.min.css" rel="stylesheet" type="text/css" >
<link href="/main.css?v=3" rel="stylesheet" type="text/css" >
</head>
<body>
    <form id="frmMain" runat="server">
		<div class="container-fluid" style='margin-top: 250px'>
			<div class="row">
				<div class="col-12 text-center">
					 You have been logged out. Please close this window or login again.
				<br /><br /><br />
				<button onclick="document.location.href='../redirect.aspx'" class="btn" type="button">Login again</button>
				</div>
			</div>
		</div>
       
    </form>
</body>
</html>
