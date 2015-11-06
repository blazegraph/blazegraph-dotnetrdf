
# dotNetRDF-blazegraph
Blazegraph connector using dotNetRDF
# Features support matrix

<table cellpadding="0" cellspacing="0">
	<colgroup>
		<col width="229">
		<col width="95">
	</colgroup>
	<colgroup>
		<col width="391">
	</colgroup>
	<colgroup>
		<col width="192">
	</colgroup>
	<tr>
		<td bgcolor="#ffffff" style="border-top: 1px solid #000000; border-bottom: 1px solid #000000; border-left: 1px solid #000000; border-right: none; padding-top: 0.02in; padding-bottom: 0.02in; padding-left: 0.03in; padding-right: 0in">
			<p align="center"><font color="#000000"><font face="Arial"><b>REST
			Endpoint</b></font></font></p>
		</td>
		<td colspan="2" bgcolor="#ffffff" style="border-top: 1px solid #000000; border-bottom: 1px solid #000000; border-left: none; border-right: 1px solid #000000">
			<p align="center"><font color="#000000"><font face="Arial"><b>Call
			/ parameters</b></font></font></p>
		</td>
		<td style="border-top: 1px solid #000000; border-bottom: 1px solid #000000; border-left: none; border-right: 1px solid #000000">
			<p align="center"><b>dotNetRDF</b></p>
		</td>
	</tr>
	<tr>
		<td colspan="4" bgcolor="#d9d2e9" style="border-top: none; border-bottom: 1px solid #000000; border-left: 1px solid #000000; border-right: none"></td>
	</tr>
	<tr>
		<td rowspan="13" bgcolor="#ffffff" style="border-top: none; border-bottom: 1px solid #000000; border-left: 1px solid #000000; border-right: 1px solid #000000">
			<p align="left"><font color="#000000"><font face="Arial"><b>QUERY</b></font></font></p>
		</td>
		<td colspan="2" style="border-top: none; border-bottom: 1px solid #000000; border-left: none; border-right: 1px solid #000000">
			<p><font color="#000000"><font face="Arial">GET Request-URI
			?query=...</font></font></p>
		</td>
		<td style="border-top: none; border-bottom: 1px solid #000000; border-left: none; border-right: 1px solid #000000">
			<p align="center">+</p>
		</td>
	</tr>
	<tr>
		<td colspan="2" style="border-top: none; border-bottom: 1px solid #000000; border-left: none; border-right: 1px solid #000000">
			<p><font color="#000000"><font face="Arial">POST Request-URI
			?query=...</font></font></p>
		</td>
		<td style="border-top: none; border-bottom: 1px solid #000000; border-left: none; border-right: 1px solid #000000">
			<p align="center">+</p>
		</td>
	</tr>
	<tr>
		<td rowspan="10" bgcolor="#ffffff" style="border-top: none; border-bottom: 1px solid #000000; border-left: none; border-right: 1px solid #000000">
			<p align="left"><font color="#000000"><font face="Arial"><b>parameters</b></font></font></p>
		</td>
		<td style="border-top: none; border-bottom: 1px solid #000000; border-left: none; border-right: 1px solid #000000">
			<p>timestamp</p>
		</td>
		<td style="border-top: none; border-bottom: 1px solid #000000; border-left: none; border-right: 1px solid #000000">
			<p align="center">+</p>
		</td>
	</tr>
	<tr>
		<td style="border-top: none; border-bottom: 1px solid #000000; border-left: none; border-right: 1px solid #000000">
			<p>explain</p>
		</td>
		<td style="border-top: none; border-bottom: 1px solid #000000; border-left: none; border-right: 1px solid #000000">
			<p align="center">-</p>
		</td>
	</tr>
	<tr>
		<td style="border-top: none; border-bottom: 1px solid #000000; border-left: none; border-right: 1px solid #000000">
			<p>analytic</p>
		</td>
		<td style="border-top: none; border-bottom: 1px solid #000000; border-left: none; border-right: 1px solid #000000">
			<p align="center">-</p>
		</td>
	</tr>
	<tr>
		<td style="border-top: none; border-bottom: 1px solid #000000; border-left: none; border-right: 1px solid #000000">
			<p>default-graph-uri</p>
		</td>
		<td style="border-top: none; border-bottom: 1px solid #000000; border-left: none; border-right: 1px solid #000000">
			<p align="center">-</p>
		</td>
	</tr>
	<tr>
		<td style="border-top: none; border-bottom: 1px solid #000000; border-left: none; border-right: 1px solid #000000">
			<p>named-graph-uri</p>
		</td>
		<td style="border-top: none; border-bottom: 1px solid #000000; border-left: none; border-right: 1px solid #000000">
			<p align="center">-</p>
		</td>
	</tr>
	<tr>
		<td style="border-top: none; border-bottom: 1px solid #000000; border-left: none; border-right: 1px solid #000000">
			<p>format</p>
		</td>
		<td style="border-top: none; border-bottom: 1px solid #000000; border-left: none; border-right: 1px solid #000000">
			<p align="center">-</p>
		</td>
	</tr>
	<tr>
		<td style="border-top: none; border-bottom: 1px solid #000000; border-left: none; border-right: 1px solid #000000">
			<p>baseURI</p>
		</td>
		<td style="border-top: none; border-bottom: 1px solid #000000; border-left: none; border-right: 1px solid #000000">
			<p align="center">-</p>
		</td>
	</tr>
	<tr>
		<td style="border-top: none; border-bottom: 1px solid #000000; border-left: none; border-right: 1px solid #000000">
			<p>includeInferred</p>
		</td>
		<td style="border-top: none; border-bottom: 1px solid #000000; border-left: none; border-right: 1px solid #000000">
			<p align="center">-</p>
		</td>
	</tr>
	<tr>
		<td style="border-top: none; border-bottom: 1px solid #000000; border-left: none; border-right: 1px solid #000000">
			<p>timeout</p>
		</td>
		<td style="border-top: none; border-bottom: 1px solid #000000; border-left: none; border-right: 1px solid #000000">
			<p align="center">-</p>
		</td>
	</tr>
	<tr>
		<td style="border-top: none; border-bottom: 1px solid #000000; border-left: none; border-right: 1px solid #000000">
			<p>${var}=Value</p>
		</td>
		<td style="border-top: none; border-bottom: 1px solid #000000; border-left: none; border-right: 1px solid #000000">
			<p align="center">-</p>
		</td>
	</tr>
	<tr>
		<td style="border-top: none; border-bottom: 1px solid #000000; border-left: none; border-right: 1px solid #000000">
			<p><b>headers</b></p>
		</td>
		<td style="border-top: none; border-bottom: 1px solid #000000; border-left: none; border-right: 1px solid #000000">
			<p>X-BIGDATA-MAX-QUERY-MILLIS</p>
		</td>
		<td style="border-top: none; border-bottom: 1px solid #000000; border-left: none; border-right: 1px solid #000000">
			<p align="center">+</p>
		</td>
	</tr>
	<tr>
		<td colspan="4" bgcolor="#d9d2e9" style="border-top: none; border-bottom: 1px solid #000000; border-left: 1px solid #000000; border-right: none">
			<p><b>INSERT</b></p>
		</td>
	</tr>
	<tr>
		<td style="border-top: none; border-bottom: 1px solid #000000; border-left: 1px solid #000000; border-right: 1px solid #000000">
			<p><font color="#000000"><font face="Arial"><b>INSERT RDF (POST
			with Body)</b></font></font></p>
		</td>
		<td colspan="2" style="border-top: none; border-bottom: 1px solid #000000; border-left: none; border-right: 1px solid #000000">
			<p>POST Request-URI<br>...<br>Content-Type:<br>...<br>BODY</p>
		</td>
		<td style="border-top: none; border-bottom: 1px solid #000000; border-left: none; border-right: 1px solid #000000">
			<p align="center">-</p>
		</td>
	</tr>
	<tr>
		<td style="border-top: none; border-bottom: 1px solid #000000; border-left: 1px solid #000000; border-right: 1px solid #000000">
			<p><font color="#000000"><font face="Arial"><b>INSERT RDF (POST
			with URLs)</b></font></font></p>
		</td>
		<td colspan="2" style="border-top: none; border-bottom: 1px solid #000000; border-left: none; border-right: 1px solid #000000">
			<p><font color="#000000"><font face="Arial">POST Request-URI
			?uri=URI</font></font></p>
		</td>
		<td style="border-top: none; border-bottom: 1px solid #000000; border-left: none; border-right: 1px solid #000000">
			<p align="center">-</p>
		</td>
	</tr>
	<tr>
		<td colspan="4" bgcolor="#d9d2e9" style="border-top: none; border-bottom: 1px solid #000000; border-left: 1px solid #000000; border-right: none">
			<p><b>DELETE</b></p>
		</td>
	</tr>
	<tr>
		<td style="border-top: none; border-bottom: 1px solid #000000; border-left: 1px solid #000000; border-right: 1px solid #000000">
			<p><font color="#000000"><font face="Arial"><b>DELETE with Query</b></font></font></p>
		</td>
		<td colspan="2" style="border-top: none; border-bottom: 1px solid #000000; border-left: none; border-right: 1px solid #000000">
			<p><font color="#000000"><font face="Arial">DELETE Request-URI
			?query=...</font></font></p>
		</td>
		<td style="border-top: none; border-bottom: 1px solid #000000; border-left: none; border-right: 1px solid #000000">
			<p align="center">-</p>
		</td>
	</tr>
	<tr>
		<td style="border-top: none; border-bottom: 1px solid #000000; border-left: 1px solid #000000; border-right: 1px solid #000000">
			<p><font color="#000000"><font face="Arial"><b>DELETE with Body
			(using POST)</b></font></font></p>
		</td>
		<td colspan="2" style="border-top: none; border-bottom: 1px solid #000000; border-left: none; border-right: 1px solid #000000">
			<p>POST Request-URI ?delete<br>...<br>Content-Type<br>...<br>BODY</p>
		</td>
		<td style="border-top: none; border-bottom: 1px solid #000000; border-left: none; border-right: 1px solid #000000">
			<p align="center">-</p>
		</td>
	</tr>
	<tr>
		<td colspan="4" bgcolor="#d9d2e9" style="border-top: none; border-bottom: 1px solid #000000; border-left: 1px solid #000000; border-right: none">
			<p><b>UPDATE</b></p>
		</td>
	</tr>
	<tr>
		<td rowspan="3" style="border-top: none; border-bottom: 1px solid #000000; border-left: 1px solid #000000; border-right: 1px solid #000000">
			<p><font color="#000000"><font face="Arial"><b>UPDATE <br>(SPARQL
			1.1 UPDATE)</b></font></font></p>
		</td>
		<td colspan="2" style="border-top: none; border-bottom: 1px solid #000000; border-left: none; border-right: 1px solid #000000">
			<p>POST Request-URI ?update=...</p>
		</td>
		<td style="border-top: none; border-bottom: 1px solid #000000; border-left: none; border-right: 1px solid #000000">
			<p align="center">-</p>
		</td>
	</tr>
	<tr>
		<td rowspan="2" bgcolor="#ffffff" style="border-top: none; border-bottom: 1px solid #000000; border-left: none; border-right: 1px solid #000000">
			<p align="left"><font color="#000000"><font face="Arial"><b>parameters</b></font></font></p>
		</td>
		<td style="border-top: none; border-bottom: 1px solid #000000; border-left: none; border-right: 1px solid #000000">
			<p>using-graph-uri</p>
		</td>
		<td style="border-top: none; border-bottom: 1px solid #000000; border-left: none; border-right: 1px solid #000000">
			<p align="center">-</p>
		</td>
	</tr>
	<tr>
		<td style="border-top: none; border-bottom: 1px solid #000000; border-left: none; border-right: 1px solid #000000">
			<p><font color="#000000"><font face="Arial">using-named-graph-uri </font></font>
			</p>
		</td>
		<td style="border-top: none; border-bottom: 1px solid #000000; border-left: none; border-right: 1px solid #000000">
			<p align="center">-</p>
		</td>
	</tr>
	<tr>
		<td style="border-top: none; border-bottom: 1px solid #000000; border-left: 1px solid #000000; border-right: 1px solid #000000">
			<p><b>UPDATE (DELETE + INSERT)<br>(DELETE statements <br>selected
			by a QUERY plus <br>INSERT statements from <br>Request Body using
			PUT)</b></p>
		</td>
		<td colspan="2" style="border-top: none; border-bottom: 1px solid #000000; border-left: none; border-right: 1px solid #000000">
			<p>PUT Request-URI ?query=...<br>...<br>Content-Type<br>...<br>BODY</p>
		</td>
		<td style="border-top: none; border-bottom: 1px solid #000000; border-left: none; border-right: 1px solid #000000">
			<p align="center">+</p>
		</td>
	</tr>
	<tr>
		<td style="border-top: none; border-bottom: 1px solid #000000; border-left: 1px solid #000000; border-right: 1px solid #000000">
			<p><font color="#000000"><font face="Arial"><b>UPDATE <br>(POST
			with Multi-Part <br>Request Body)</b></font></font></p>
		</td>
		<td colspan="2" style="border-top: none; border-bottom: 1px solid #000000; border-left: none; border-right: 1px solid #000000">
			<p>POST Request-URI ?updatePost<br>...<br>Content-Type:
			multipart/form-data; boundary=...<br>...<br>form-data;
			name=&quot;remove&quot;<br>Content-Type:
			...<br>Content-Body<br>...<br>form-data; name=&quot;add&quot;<br>Content-Type:
			...<br>Content-Body<br>...<br>BODY</p>
		</td>
		<td style="border-top: none; border-bottom: 1px solid #000000; border-left: none; border-right: 1px solid #000000">
			<p align="center">+</p>
		</td>
	</tr>
	<tr>
		<td colspan="4" bgcolor="#d9d2e9" style="border-top: none; border-bottom: 1px solid #000000; border-left: 1px solid #000000; border-right: none">
			<p><b>Multi-Tenancy API</b></p>
		</td>
	</tr>
	<tr>
		<td style="border-top: none; border-bottom: 1px solid #000000; border-left: 1px solid #000000; border-right: 1px solid #000000">
			<p><b>DESCRIBE DATA SETS</b></p>
		</td>
		<td colspan="2" style="border-top: none; border-bottom: 1px solid #000000; border-left: none; border-right: 1px solid #000000">
			<p>GET /bigdata/namespace</p>
		</td>
		<td style="border-top: none; border-bottom: 1px solid #000000; border-left: none; border-right: 1px solid #000000">
			<p align="center">-</p>
		</td>
	</tr>
	<tr>
		<td style="border-top: none; border-bottom: 1px solid #000000; border-left: 1px solid #000000; border-right: 1px solid #000000">
			<p><b>CREATE DATA SET</b></p>
		</td>
		<td colspan="2" style="border-top: none; border-bottom: 1px solid #000000; border-left: none; border-right: 1px solid #000000">
			<p>POST /bigdata/namespace<br>...<br>Content-Type<br>...<br>BODY</p>
		</td>
		<td style="border-top: none; border-bottom: 1px solid #000000; border-left: none; border-right: 1px solid #000000">
			<p align="center">-</p>
		</td>
	</tr>
	<tr>
		<td style="border-top: none; border-bottom: 1px solid #000000; border-left: 1px solid #000000; border-right: 1px solid #000000">
			<p><b>DESTROY DATA SET</b></p>
		</td>
		<td colspan="2" style="border-top: none; border-bottom: 1px solid #000000; border-left: none; border-right: 1px solid #000000">
			<p>DELETE /bigdata/namespace/NAMESPACE</p>
		</td>
		<td style="border-top: none; border-bottom: 1px solid #000000; border-left: none; border-right: 1px solid #000000">
			<p align="center">-</p>
		</td>
	</tr>
	<tr>
		<td colspan="4" bgcolor="#d9d2e9" style="border-top: none; border-bottom: 1px solid #000000; border-left: 1px solid #000000; border-right: 1px solid #000000">
			<p><b>Transaction Management API</b></p>
		</td>
	</tr>
	<tr>
		<td style="border-top: none; border-bottom: 1px solid #000000; border-left: 1px solid #000000; border-right: 1px solid #000000"></td>
		<td colspan="2" style="border-top: none; border-bottom: 1px solid #000000; border-left: none; border-right: 1px solid #000000">
			<p>POST /bigdata/tx =&gt; txId</p>
		</td>
		<td style="border-top: none; border-bottom: 1px solid #000000; border-left: none; border-right: 1px solid #000000">
			<p align="center">+</p>
		</td>
	</tr>
	<tr>
		<td style="border-top: none; border-bottom: 1px solid #000000; border-left: 1px solid #000000; border-right: 1px solid #000000">
			<p><b>COMMIT-TX</b></p>
		</td>
		<td colspan="2" style="border-top: none; border-bottom: 1px solid #000000; border-left: none; border-right: 1px solid #000000">
			<p>POST /bigdata/tx/txid?COMMIT</p>
		</td>
		<td style="border-top: none; border-bottom: 1px solid #000000; border-left: none; border-right: 1px solid #000000">
			<p align="center">+</p>
		</td>
	</tr>
	<tr>
		<td style="border-top: none; border-bottom: 1px solid #000000; border-left: 1px solid #000000; border-right: 1px solid #000000">
			<p><b>LIST-TX</b></p>
		</td>
		<td colspan="2" style="border-top: none; border-bottom: 1px solid #000000; border-left: none; border-right: 1px solid #000000">
			<p>GET /bigdata/tx</p>
		</td>
		<td style="border-top: none; border-bottom: 1px solid #000000; border-left: none; border-right: 1px solid #000000">
			<p align="center">-</p>
		</td>
	</tr>
	<tr>
		<td style="border-top: none; border-bottom: 1px solid #000000; border-left: 1px solid #000000; border-right: 1px solid #000000">
			<p><b>CREATE-TX</b></p>
		</td>
		<td colspan="2" style="border-top: none; border-bottom: 1px solid #000000; border-left: none; border-right: 1px solid #000000">
			<p>POST /bigdata/tx(?timestamp=TIMESTAMP)</p>
		</td>
		<td style="border-top: none; border-bottom: 1px solid #000000; border-left: none; border-right: 1px solid #000000">
			<p align="center">+</p>
		</td>
	</tr>
	<tr>
		<td style="border-top: none; border-bottom: 1px solid #000000; border-left: 1px solid #000000; border-right: 1px solid #000000">
			<p><b>STATUS-TX</b></p>
		</td>
		<td colspan="2" style="border-top: none; border-bottom: 1px solid #000000; border-left: none; border-right: 1px solid #000000">
			<p>POST /bigdata/tx/txId?STATUS</p>
		</td>
		<td style="border-top: none; border-bottom: 1px solid #000000; border-left: none; border-right: 1px solid #000000">
			<p align="center">-</p>
		</td>
	</tr>
	<tr>
		<td style="border-top: none; border-bottom: 1px solid #000000; border-left: 1px solid #000000; border-right: 1px solid #000000">
			<p><b>ABORT-TX</b></p>
		</td>
		<td colspan="2" style="border-top: none; border-bottom: 1px solid #000000; border-left: none; border-right: 1px solid #000000">
			<p>POST /bigdata/tx/txId?ABORT</p>
		</td>
		<td style="border-top: none; border-bottom: 1px solid #000000; border-left: none; border-right: 1px solid #000000">
			<p align="center">+</p>
		</td>
	</tr>
	<tr>
		<td style="border-top: none; border-bottom: 1px solid #000000; border-left: 1px solid #000000; border-right: 1px solid #000000">
			<p><b>PREPARE-TX</b></p>
		</td>
		<td colspan="2" style="border-top: none; border-bottom: 1px solid #000000; border-left: none; border-right: 1px solid #000000">
			<p>POST /bigdata/tx/txId?PREPARE</p>
		</td>
		<td style="border-top: none; border-bottom: 1px solid #000000; border-left: none; border-right: 1px solid #000000">
			<p align="center">-</p>
		</td>
	</tr>
	<tr>
		<td colspan="4" bgcolor="#d9d2e9" style="border-top: none; border-bottom: 1px solid #000000; border-left: 1px solid #000000; border-right: none">
			<p><b>Access Path Operations</b></p>
		</td>
	</tr>
	<tr>
		<td style="border-top: none; border-bottom: 1px solid #000000; border-left: 1px solid #000000; border-right: 1px solid #000000">
			<p><b>FAST RANGE COUNTS</b></p>
		</td>
		<td colspan="2" style="border-top: none; border-bottom: 1px solid #000000; border-left: none; border-right: 1px solid #000000">
			<p>GET Request-URI
			?ESTCARD&amp;([s|p|o|c]=(uri|literal))[&amp;exact=(true|false)+</p>
		</td>
		<td style="border-top: none; border-bottom: 1px solid #000000; border-left: none; border-right: 1px solid #000000">
			<p align="center">-</p>
		</td>
	</tr>
	<tr>
		<td style="border-top: none; border-bottom: 1px solid #000000; border-left: 1px solid #000000; border-right: 1px solid #000000">
			<p><b>HASSTMT</b></p>
		</td>
		<td colspan="2" style="border-top: none; border-bottom: 1px solid #000000; border-left: none; border-right: 1px solid #000000">
			<p>GET Request-URI
			?HASSTMT&amp;([s|p|o|c]=(uri|literal))[&amp;includeInferred=(true|false)+</p>
		</td>
		<td style="border-top: none; border-bottom: 1px solid #000000; border-left: none; border-right: 1px solid #000000">
			<p align="center">-</p>
		</td>
	</tr>
	<tr>
		<td rowspan="2" style="border-top: none; border-bottom: 1px solid #000000; border-left: 1px solid #000000; border-right: 1px solid #000000">
			<p><font color="#000000"><font face="Arial"><b>GETSTMTS</b></font></font></p>
		</td>
		<td colspan="2" style="border-top: none; border-bottom: 1px solid #000000; border-left: none; border-right: 1px solid #000000">
			<p>GET Request-URI ?GETSTMTS<br>...<br>Content-Type<br>...</p>
		</td>
		<td style="border-top: none; border-bottom: 1px solid #000000; border-left: none; border-right: 1px solid #000000">
			<p align="center">-</p>
		</td>
	</tr>
	<tr>
		<td colspan="2" style="border-top: none; border-bottom: 1px solid #000000; border-left: none; border-right: 1px solid #000000">
			<p>POST Request-URI ?GETSTMTS<br>...<br>Content-Type<br>…</p>
		</td>
		<td style="border-top: none; border-bottom: 1px solid #000000; border-left: none; border-right: 1px solid #000000">
			<p align="center">-</p>
		</td>
	</tr>
	<tr>
		<td style="border-top: none; border-bottom: 1px solid #000000; border-left: 1px solid #000000; border-right: 1px solid #000000">
			<p><font color="#000000"><font face="Arial"><b>DELETE with Access
			Path</b></font></font></p>
		</td>
		<td colspan="2" style="border-top: none; border-bottom: 1px solid #000000; border-left: none; border-right: 1px solid #000000">
			<p>DELETE Request-URI ?([s|p|o|c]=(uri|literal))+</p>
		</td>
		<td style="border-top: none; border-bottom: 1px solid #000000; border-left: none; border-right: 1px solid #000000">
			<p align="center">only c param</p>
		</td>
	</tr>
	<tr>
		<td colspan="4" bgcolor="#d9d2e9" style="border-top: none; border-bottom: 1px solid #000000; border-left: 1px solid #000000; border-right: none"></td>
	</tr>
	<tr>
		<td style="border-top: none; border-bottom: 1px solid #000000; border-left: 1px solid #000000; border-right: 1px solid #000000">
			<p><b>STATUS</b></p>
		</td>
		<td colspan="2" style="border-top: none; border-bottom: 1px solid #000000; border-left: none; border-right: 1px solid #000000">
			<p>GET /status</p>
		</td>
		<td style="border-top: none; border-bottom: 1px solid #000000; border-left: none; border-right: 1px solid #000000">
			<p align="center">-</p>
		</td>
	</tr>
	<tr>
		<td style="border-top: none; border-bottom: 1px solid #000000; border-left: 1px solid #000000; border-right: 1px solid #000000">
			<p><b>CANCEL</b></p>
		</td>
		<td colspan="2" style="border-top: none; border-bottom: 1px solid #000000; border-left: none; border-right: 1px solid #000000">
			<p>POST /bigdata/sparql/?cancelQuery&amp;queryId=....</p>
		</td>
		<td style="border-top: none; border-bottom: 1px solid #000000; border-left: none; border-right: 1px solid #000000">
			<p align="center">-</p>
		</td>
	</tr>
</table>

#Usage

Download dll’s from the link and add to the project or include them by the NuGet.
Look at the https://bitbucket.org/dotnetrdf/dotnetrdf/wiki/User%20Guide dotNetRDF documentation.

Examples of using Blazegraph connector:

#Create new graph

BlazegraphConnector connector = new BlazegraphConnector("http://localhost:9999/bigdata/");

Graph newGraph = new Graph();
newGraph.BaseUri = UriFactory.Create("http://example/bookStore");

Triple triple = new Triple(
    newGraph.CreateUriNode(UriFactory.Create("http://example/book1")),
    newGraph.CreateUriNode(UriFactory.Create("http://example.org/ns#price")),
    newGraph.CreateLiteralNode("42", new Uri(XmlSpecsHelper.XmlSchemaDataTypeInteger))
);
newGraph.Assert(triple);

connector.SaveGraph(newGraph);

#Load graph

Graph loadGraph = new Graph();
connector.LoadGraph(loadGraph, UriFactory.Create("http://example/bookStore"));

#Update graph

Triple triple2remove = new Triple(
    newGraph.CreateUriNode(UriFactory.Create("http://example/book1")),
    newGraph.CreateUriNode(UriFactory.Create("http://example.org/ns#price")),
    newGraph.CreateLiteralNode("42", new Uri(XmlSpecsHelper.XmlSchemaDataTypeInteger))
);
Triple triple2add = new Triple(
    newGraph.CreateUriNode(UriFactory.Create("http://example/book1")),
    newGraph.CreateUriNode(UriFactory.Create("http://purl.org/dc/elements/1.1/title")),
    newGraph.CreateLiteralNode("Fundamentals of Compiler Design", new Uri(XmlSpecsHelper.XmlSchemaDataTypeString))
);
connector.UpdateGraph(
    UriFactory.Create("http://example/bookStore"),
    new List<Triple>() { triple2add },
    new List<Triple>() { triple2remove }
);

#Delete graph

connector.DeleteGraph(UriFactory.Create("http://example/bookStore"));

#Query

SparqlResultSet resultSet = (SparqlResultSet)connector.Query("SELECT * { ?s ?p ?o }");
foreach (SparqlResult result in resultSet.Results) {
    Console.WriteLine(result.ToString());
}
