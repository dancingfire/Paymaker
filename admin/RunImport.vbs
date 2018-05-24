
Set oServerXML = CreateObject("Msxml2.ServerXMLHTTP")
oServerXML.setTimeouts 120000, 300000, 300000, 300000
oServerXML.Open "GET","http://commission.fletchers.com.au/campaign/campaign_import.aspx", False
oServerXML.setRequestHeader "Content-Type","application/x-www-form-urlencoded"
oServerXML.send
Set oServerXML = nothing
