Set oServerXML = CreateObject("Msxml2.ServerXMLHTTP")
oServerXML.setTimeouts 60000, 300000, 300000, 300000
oServerXML.Open "GET","http://localhost/paymaker/automation/email_notification.aspx?AUTORUN=true", False
oServerXML.setRequestHeader "Content-Type","application/x-www-form-urlencoded"
oServerXML.send

Set oServerXML = nothing



