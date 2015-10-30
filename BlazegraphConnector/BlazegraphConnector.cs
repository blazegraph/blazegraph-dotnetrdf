using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Xml;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query;
using VDS.RDF.Writing;

namespace VDS.RDF.Storage
{
    /// <summary>
    /// Class for connecting to a Blazegraph Store
    /// </summary>
    public class BlazegraphConnector : BaseAsyncHttpConnector, IAsyncQueryableTimeoutStorage, IQueryableTimeoutStorage, IConfigurationSerializable, IAsyncTransactionalStorage, ITransactionalStorage
    {
        private const string BlazeGraphQueryTimeoutHeader = "X-BIGDATA-MAX-QUERY-MILLIS";

        /// <summary>
        /// Constant for the default Blazegraph namespace
        /// </summary>
        public const string DefaultNamespace = "kb";

        protected string _baseUri, _ns;

        protected string _activeTrans = null;

        protected object transLock = new object();

        protected TriGWriter _writer = new TriGWriter();

        /// <summary>
        /// Creates a new Connection to a Blazegraph store with default namespace
        /// </summary>
        /// <param name="baseUri ">Base Uri of the Blazegraph</param>
        public BlazegraphConnector(string baseUri) : this(baseUri, DefaultNamespace)
        {
        }

        /// <summary>
        /// Creates a new Connection to a Blazegraph store
        /// </summary>
        /// <param name="baseUri ">Base Uri of the Blazegraph</param>
        /// /// <param name="ns">Namespace</param>
        public BlazegraphConnector(string baseUri, string ns)
        {
            this._baseUri = baseUri.TrimEnd(new char[] { '/' });
            this._ns = ns;
        }

        /// <summary>
        /// Gets the Base URI of the Blazegraph store
        /// </summary>
        public string BaseUri
        {
            get { return this._baseUri; }
        }

        /// <summary>
        /// Gets the current namespace of the Blazegraph store
        /// </summary>
        public string Namespace
        {
            get { return this._ns; }
        }

        public override bool DeleteSupported
        {
            get { return true; }
        }

        public override IOBehaviour IOBehaviour
        {
            get { return IOBehaviour.GraphStore | IOBehaviour.CanUpdateTriples; }
        }

        public override bool IsReadOnly
        {
            get { return false; }
        }

        public override bool IsReady
        {
            get { return true; }
        }

        public override bool ListGraphsSupported
        {
            get { return true; }
        }

        public override bool UpdateSupported
        {
            get { return true; }
        }

        public override void Dispose()
        {
            //No Dispose actions
        }

        /// <summary>
        /// Helper method for creating HTTP Requests to the Store
        /// </summary>
        /// <param name="servicePath">Path to the Service requested</param>
        /// <param name="accept">Acceptable Content Types</param>
        /// <param name="method">HTTP Method</param>
        /// <param name="requestParams">Querystring Parameters</param>
        /// <returns></returns>
        protected HttpWebRequest CreateRequest(string servicePath, string accept, string method, Dictionary<string, string> requestParams, bool withNS = true)
        {
            //Build the Request Uri
            string requestUri;
            if (withNS)
            {
                requestUri = this._baseUri + "/namespace/" + this._ns + servicePath;
            }
            else
            {
                requestUri = this._baseUri + servicePath;
            }

            string tID = null;
            lock (transLock)
            {
                tID = this._activeTrans;
            }
            if (tID != null)
            {
                if (requestParams == null)
                {
                    requestParams = new Dictionary<string, string>();
                }
                requestParams.Add("timestamp", tID);
            }

            bool wasParams = servicePath.Contains('?');
            if (!ReferenceEquals(requestParams, null) && requestParams.Count > 0)
            {
                StringBuilder requestParamsSb = new StringBuilder();
                foreach (string p in requestParams.Keys)
                {
                    requestParamsSb.Append(p).Append("=").Append(HttpUtility.UrlEncode(requestParams[p])).Append("&");
                }
                requestUri += (wasParams ? "&" : "?") + requestParamsSb.ToString();
            }

            //Create our Request
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUri);
            request.Accept = accept;
            request.Method = method;
            request = ApplyRequestOptions(request);

            return request;
        }

        #region IQueryableTimeoutStorage

        /// <summary>
        /// Makes a SPARQL Query against the underlying Store using whatever reasoning mode is currently in-use
        /// </summary>
        /// <param name="sparqlQuery">Sparql Query</param>
        /// <returns></returns>
        public virtual object Query(String sparqlQuery)
        {
            return this.Query(sparqlQuery, -1);
        }

        /// <summary>
        /// Makes a SPARQL Query against the underlying Store using whatever reasoning mode is currently in-use processing the results using an appropriate handler from those provided
        /// </summary>
        /// <param name="rdfHandler">RDF Handler</param>
        /// <param name="resultsHandler">Results Handler</param>
        /// <param name="sparqlQuery">SPARQL Query</param>
        /// <returns></returns>
        public virtual void Query(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, string sparqlQuery)
        {
            this.Query(rdfHandler, resultsHandler, sparqlQuery, -1);
        }

        /// <summary>
        /// Makes a SPARQL Query against the underlying Store using whatever reasoning mode is currently in-use
        /// </summary>
        /// <param name="sparqlQuery">Sparql Query</param>
        /// <param name="timeoutMilliseconds">Query timeout in milliseconds</param>
        /// <returns></returns>
        public virtual object Query(string sparqlQuery, long timeoutMilliseconds)
        {
            Graph g = new Graph();
            SparqlResultSet results = new SparqlResultSet();
            this.Query(new GraphHandler(g), new ResultSetHandler(results), sparqlQuery, timeoutMilliseconds);

            if (results.ResultsType != SparqlResultsType.Unknown)
            {
                return results;
            }
            else
            {
                return g;
            }
        }

        /// <summary>
        /// Makes a SPARQL Query against the underlying Store using whatever reasoning mode is currently in-use processing the results using an appropriate handler from those provided
        /// </summary>
        /// <param name="rdfHandler">RDF Handler</param>
        /// <param name="resultsHandler">Results Handler</param>
        /// <param name="sparqlQuery">SPARQL Query</param>
        /// <param name="timeoutMilliseconds">Query timeout in milliseconds</param>
        /// <returns></returns>
        public virtual void Query(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, string sparqlQuery, long timeoutMilliseconds)
        {
            try
            {
                HttpWebRequest request;

                string accept = MimeTypesHelper.HttpRdfOrSparqlAcceptHeader;

                Dictionary<string, string> queryParams = new Dictionary<string, string>();
                if (sparqlQuery.Length < 2048)
                {
                    queryParams.Add("query", sparqlQuery);

                    request = this.CreateRequest("/sparql", accept, "GET", queryParams);

                    if (timeoutMilliseconds != -1)
                    {
                        request.Headers[BlazeGraphQueryTimeoutHeader] = timeoutMilliseconds.ToString();
                    }
                }
                else
                {
                    request = this.CreateRequest("/sparql", accept, "POST", queryParams);

                    if (timeoutMilliseconds != -1)
                    {
                        request.Headers[BlazeGraphQueryTimeoutHeader] = timeoutMilliseconds.ToString();
                    }

                    //Build the Post Data and add to the Request Body
                    request.ContentType = MimeTypesHelper.Utf8WWWFormURLEncoded;
                    StringBuilder postData = new StringBuilder();
                    postData.Append("query=");
                    postData.Append(HttpUtility.UrlEncode(sparqlQuery));
                    string tID;
                    lock (transLock)
                    {
                        tID = this._activeTrans;
                    }
                    if (tID != null)
                    {
                        postData.Append("&timestamp=" + tID);
                    }
                    using (StreamWriter writer = new StreamWriter(request.GetRequestStream(), new UTF8Encoding()))
                    {
                        writer.Write(postData);
                        writer.Close();
                    }
                }

                Tools.HttpDebugRequest(request);

                //Get the Response and process based on the Content Type
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    Tools.HttpDebugResponse(response);

                    StreamReader data = new StreamReader(response.GetResponseStream());
                    string ctype = response.ContentType;
                    try
                    {
                        //Is the Content Type referring to a Sparql Result Set format?
                        ISparqlResultsReader resreader = MimeTypesHelper.GetSparqlParser(ctype, Regex.IsMatch(sparqlQuery, "ASK", RegexOptions.IgnoreCase));
                        resreader.Load(resultsHandler, data);
                        response.Close();
                    }
                    catch (RdfParserSelectionException)
                    {
                        //If we get a Parser Selection exception then the Content Type isn't valid for a Sparql Result Set

                        //Is the Content Type referring to a RDF format?
                        IRdfReader rdfreader = MimeTypesHelper.GetParser(ctype);
                        rdfreader.Load(rdfHandler, data);
                        response.Close();
                    }
                }
            }
            catch (WebException webEx)
            {
                throw StorageHelper.HandleHttpQueryError(webEx);
            }
        }

        #endregion

        #region IAsyncQueryableStorage

        /// <summary>
        /// Queries the store asynchronously
        /// </summary>
        /// <param name="sparqlQuery">SPARQL Query</param>
        /// <param name="rdfHandler">RDF Handler</param>
        /// <param name="resultsHandler">Results Handler</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        public virtual void Query(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, string sparqlQuery, AsyncStorageCallback callback, object state)
        {
            this.Query(rdfHandler, resultsHandler, sparqlQuery, callback, state, -1);
        }

        /// <summary>
        /// Queries the store asynchronously
        /// </summary>
        /// <param name="sparqlQuery">SPARQL Query</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        public virtual void Query(string sparqlQuery, AsyncStorageCallback callback, object state)
        {
            this.Query(sparqlQuery, callback, state, -1);
        }

        /// <summary>
        /// Queries the store asynchronously
        /// </summary>
        /// <param name="sparqlQuery">SPARQL Query</param>
        /// <param name="rdfHandler">RDF Handler</param>
        /// <param name="resultsHandler">Results Handler</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        /// <param name="timeoutMilliseconds">Query timeout in milliseconds</param>
        public void Query(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, string sparqlQuery, AsyncStorageCallback callback, object state, long timeoutMilliseconds)
        {
            try
            {
                HttpWebRequest request;

                string accept = MimeTypesHelper.HttpRdfOrSparqlAcceptHeader;
                Dictionary<string, string> queryParams = new Dictionary<string, string>();
                request = this.CreateRequest("/sparql", accept, "POST", queryParams);

                if (timeoutMilliseconds != -1)
                {
                    request.Headers[BlazeGraphQueryTimeoutHeader] = timeoutMilliseconds.ToString();
                }

                request.ContentType = MimeTypesHelper.Utf8WWWFormURLEncoded;

                request.BeginGetRequestStream(r =>
                    {
                        try
                        {
                            Stream stream = request.EndGetRequestStream(r);
                            using (StreamWriter writer = new StreamWriter(stream, new UTF8Encoding(Options.UseBomForUtf8)))
                            {
                                writer.Write("query=");
                                writer.Write(HttpUtility.UrlEncode(sparqlQuery));
                                string tID;
                                lock (transLock)
                                {
                                    tID = this._activeTrans;
                                }
                                if (tID != null)
                                {
                                    writer.Write("&timestamp=" + tID);
                                }
                                writer.Close();
                            }

                            Tools.HttpDebugRequest(request);

                            //Get the Response and process based on the Content Type
                            request.BeginGetResponse(r2 =>
                                {
                                    try
                                    {
                                        HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(r2);
                                        Tools.HttpDebugResponse(response);

                                        StreamReader data = new StreamReader(response.GetResponseStream());
                                        string ctype = response.ContentType;

                                        try
                                        {
                                            //Is the Content Type referring to a Sparql Result Set format?
                                            ISparqlResultsReader resreader = MimeTypesHelper.GetSparqlParser(ctype, Regex.IsMatch(sparqlQuery, "ASK", RegexOptions.IgnoreCase));
                                            resreader.Load(resultsHandler, data);
                                            response.Close();
                                        }
                                        catch (RdfParserSelectionException)
                                        {
                                            //If we get a Parser Selection exception then the Content Type isn't valid for a Sparql Result Set

                                            //Is the Content Type referring to a RDF format?
                                            IRdfReader rdfreader = MimeTypesHelper.GetParser(ctype);
                                            rdfreader.Load(rdfHandler, data);
                                            response.Close();
                                        }
                                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQueryWithHandler, sparqlQuery, rdfHandler, resultsHandler), state);
                                    }
                                    catch (WebException webEx)
                                    {
                                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQueryWithHandler, StorageHelper.HandleHttpQueryError(webEx)), state);
                                    }
                                    catch (Exception ex)
                                    {
                                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQueryWithHandler, StorageHelper.HandleQueryError(ex)), state);
                                    }
                                }, state);
                        }
                        catch (WebException webEx)
                        {
                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQueryWithHandler, StorageHelper.HandleHttpQueryError(webEx)), state);
                        }
                        catch (Exception ex)
                        {
                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQueryWithHandler, StorageHelper.HandleQueryError(ex)), state);
                        }
                    }, state);
            }
            catch (WebException webEx)
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQueryWithHandler, StorageHelper.HandleHttpQueryError(webEx)), state);
            }
            catch (Exception ex)
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQueryWithHandler, StorageHelper.HandleQueryError(ex)), state);
            }
        }

        /// <summary>
        /// Queries the store asynchronously
        /// </summary>
        /// <param name="sparqlQuery">SPARQL Query</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        /// <param name="timeoutMilliseconds">Query timeout in milliseconds</param>
        public void Query(string sparqlQuery, AsyncStorageCallback callback, object state, long timeoutMilliseconds)
        {
            Graph g = new Graph();
            SparqlResultSet results = new SparqlResultSet();
            this.Query(new GraphHandler(g), new ResultSetHandler(results), sparqlQuery, (sender, args, st) =>
            {
                if (results.ResultsType != SparqlResultsType.Unknown)
                {
                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQuery, sparqlQuery, results, args.Error), state);
                }
                else
                {
                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SparqlQuery, sparqlQuery, g, args.Error), state);
                }
            }, state, timeoutMilliseconds);
        }

        #endregion

        #region IStorageProvider

        /// <summary>
        /// Deletes a Graph from the Blazegraph store
        /// </summary>
        /// <param name="graphUri">URI of the Graph to delete</param>
        public virtual void DeleteGraph(string graphUri)
        {
            string activeTID;
            string tID = null;
            lock (transLock)
            {
                activeTID = this._activeTrans;
            }

            try
            {
                //Get a Transaction ID, if there is no active Transaction then this operation will be auto-committed
                tID = activeTID ?? this.BeginTransaction();

                Dictionary<string, string> queryParams = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(graphUri))
                {
                    queryParams.Add("c", "<" + graphUri + ">");
                }
                if (activeTID == null)
                {
                    queryParams.Add("timestamp", tID);
                }
                HttpWebRequest request = this.CreateRequest("/sparql", MimeTypesHelper.Any, "DELETE", queryParams);

                request.ContentType = MimeTypesHelper.WWWFormURLEncoded;

                Tools.HttpDebugRequest(request);

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    Tools.HttpDebugResponse(response);
                    //If we get here then the Delete worked OK
                    response.Close();
                }

                //Commit Transaction only if in auto-commit mode (active transaction will be null)
                if (activeTID != null) return;
                try
                {
                    this.CommitTransaction(tID);
                }
                catch (Exception ex)
                {
                    throw new RdfStorageException("Blazegraph failed to commit a Transaction", ex);
                }
            }
            catch (WebException webEx)
            {
                //Rollback Transaction only if got as far as creating a Transaction
                //and in auto-commit mode
                if (tID != null)
                {
                    if (activeTID == null)
                    {
                        try
                        {
                            this.RollbackTransaction(tID);
                        }
                        catch (Exception ex)
                        {
                            StorageHelper.HandleHttpError(webEx, "");
                            throw new RdfStorageException("Blazegraph failed to rollback a Transaction", ex);
                        }
                    }
                }

                throw StorageHelper.HandleHttpError(webEx, "deleting a Graph from");
            }
        }

        /// <summary>
        /// Deletes a Graph from the Blazegraph store
        /// </summary>
        /// <param name="graphUri">URI of the Graph to delete</param>
        public virtual void DeleteGraph(Uri graphUri)
        {
            this.DeleteGraph(graphUri.ToSafeString());
        }

        /// <summary>
        /// Gets the list of Graphs in the Blazegraph store
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<Uri> ListGraphs()
        {
            try
            {
                object results = this.Query("SELECT DISTINCT ?g WHERE { GRAPH ?g { ?s ?p ?o } }");
                if (results is SparqlResultSet)
                {
                    List<Uri> graphs = new List<Uri>();
                    foreach (SparqlResult r in ((SparqlResultSet)results))
                    {
                        if (r.HasValue("g"))
                        {
                            INode temp = r["g"];
                            if (temp.NodeType == NodeType.Uri)
                            {
                                graphs.Add(((IUriNode)temp).Uri);
                            }
                        }
                    }
                    return graphs;
                }
                else
                {
                    return Enumerable.Empty<Uri>();
                }
            }
            catch (Exception ex)
            {
                throw StorageHelper.HandleError(ex, "listing Graphs from");
            }
        }

        /// <summary>
        /// Loads a Graph from the Store
        /// </summary>
        /// <param name="handler">RDF Handler</param>
        /// <param name="graphUri">URI of the Graph to load</param>
        /// <remarks>
        /// If an empty/null URI is specified then the Default Graph of the Store will be loaded
        /// </remarks>
        public virtual void LoadGraph(IRdfHandler handler, string graphUri)
        {
            try
            {
                HttpWebRequest request;
                Dictionary<string, string> serviceParams = new Dictionary<string, string>();
                Uri baseUri = null;

                SparqlParameterizedString construct = new SparqlParameterizedString();
                if (!graphUri.Equals(string.Empty))
                {
                    construct.CommandText = "CONSTRUCT { ?s ?p ?o } WHERE { GRAPH @graph { ?s ?p ?o } }";
                    baseUri = UriFactory.Create(graphUri);
                    construct.SetUri("graph", baseUri);
                }
                else
                {
                    construct.CommandText = "CONSTRUCT { ?s ?p ?o } WHERE { ?s ?p ?o }";
                }
                serviceParams.Add("query", construct.ToString());

                request = this.CreateRequest("/sparql", MimeTypesHelper.HttpAcceptHeader, "GET", serviceParams);

                Tools.HttpDebugRequest(request);

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    Tools.HttpDebugResponse(response);

                    IRdfReader parser = MimeTypesHelper.GetParser(response.ContentType);
                    parser.Load(handler, new StreamReader(response.GetResponseStream()));
                    if (baseUri != null)
                    {
                        handler.StartRdf();
                        handler.HandleBaseUri(baseUri);
                        handler.EndRdf(true);
                    }
                    response.Close();
                }
            }
            catch (WebException webEx)
            {
                throw StorageHelper.HandleHttpError(webEx, "loading a Graph from");
            }
        }

        /// <summary>
        /// Loads a Graph from the Store
        /// </summary>
        /// <param name="handler">RDF Handler</param>
        /// <param name="graphUri">URI of the Graph to load</param>
        /// <remarks>
        /// If an empty/null URI is specified then the Default Graph of the Store will be loaded
        /// </remarks>
        public virtual void LoadGraph(IRdfHandler handler, Uri graphUri)
        {
            this.LoadGraph(handler, graphUri.ToSafeString());
        }

        /// <summary>
        /// Loads a Graph from the Store
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="graphUri">Uri of the Graph to load</param>
        /// <remarks>
        /// If an empty/null Uri is specified then the Default Graph of the Store will be loaded
        /// </remarks>
        public virtual void LoadGraph(IGraph g, string graphUri)
        {
            if (g.IsEmpty && graphUri != null && !graphUri.Equals(String.Empty))
            {
                g.BaseUri = UriFactory.Create(graphUri);
            }
            this.LoadGraph(new GraphHandler(g), graphUri);
        }

        /// <summary>
        /// Loads a Graph from the Store
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="graphUri">URI of the Graph to load</param>
        /// <remarks>
        /// If an empty/null URI is specified then the Default Graph of the Store will be loaded
        /// </remarks>
        public virtual void LoadGraph(IGraph g, Uri graphUri)
        {
            this.LoadGraph(g, graphUri.ToSafeString());
        }

        /// <summary>
        /// Saves a Graph into the Store (see remarks for notes on merge/overwrite behaviour)
        /// </summary>
        /// <param name="g">Graph to save</param>
        /// <remarks>
        /// <para>
        /// If the Graph has no URI then the contents will be appended to the Store's Default Graph. If the Graph has a URI then existing Graph associated with that URI will be replaced. To append to a named Graph use the <see cref="BlazegraphConnector.UpdateGraph(Uri,IEnumerable{Triple},IEnumerable{Triple})">UpdateGraph()</see> method instead
        /// </para>
        /// </remarks>
        public virtual void SaveGraph(IGraph g)
        {
            string activeTID;
            string tID = null;
            lock (transLock)
            {
                activeTID = this._activeTrans;
            }

            try
            {
                //Get a Transaction ID, if there is no active Transaction then this operation will be auto-committed
                tID = activeTID ?? this.BeginTransaction();

                Dictionary<string, string> queryParams = new Dictionary<string, string>();
                if (activeTID == null)
                {
                    queryParams.Add("timestamp", tID);
                }

                HttpWebRequest request;
                string boundary = String.Format("----------{0:N}", Guid.NewGuid());
                StringBuilder sb = new StringBuilder();

                if (g.BaseUri != null)
                {
                    SparqlParameterizedString construct = new SparqlParameterizedString();
                    construct.CommandText = "CONSTRUCT { ?s ?p ?o } WHERE { GRAPH @graph { ?s ?p ?o } }";
                    construct.SetUri("graph", g.BaseUri);
                    queryParams.Add("query", construct.ToString());

                    request = this.CreateRequest("/sparql", MimeTypesHelper.Any, "PUT", queryParams);
                    request.ContentType = MimeTypesHelper.GetDefinitionsByFileExtension(MimeTypesHelper.DefaultTriGExtension).First().MimeTypes.First();
                }
                else
                {
                    request = this.CreateRequest("/sparql?updatePost", MimeTypesHelper.Any, "POST", queryParams);
                    request.ContentType = MimeTypesHelper.FormMultipart + "; boundary=" + boundary;

                    string postData = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\nContent-Type: {2}\r\n\r\n{3}",
                        boundary,
                        "add",
                        MimeTypesHelper.GetDefinitionsByFileExtension(MimeTypesHelper.DefaultTriGExtension).First().MimeTypes.First(),
                        "");
                    sb.Append(postData);
                }

                //Save the Data as TriG to the Request Stream
                TripleStore store = new TripleStore();
                store.Add(g);
                this._writer.Save(store, new System.IO.StringWriter(sb));

                if (g.BaseUri == null)
                {
                    string footer = "\r\n--" + boundary + "--\r\n";
                    sb.Append(footer);
                }

                using (StreamWriter writer = new StreamWriter(request.GetRequestStream(), new UTF8Encoding()))
                {
                    writer.Write(sb.ToString());
                    writer.Close();
                }

                Tools.HttpDebugRequest(request);

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    Tools.HttpDebugResponse(response);
                    //If we get here then it was OK
                    response.Close();
                }

                //Commit Transaction only if in auto-commit mode (active transaction will be null)
                if (activeTID != null) return;
                try
                {
                    this.CommitTransaction(tID);
                }
                catch (Exception ex)
                {
                    throw new RdfStorageException("Blazegraph failed to commit a Transaction", ex);
                }
            }
            catch (WebException webEx)
            {
                //Rollback Transaction only if got as far as creating a Transaction
                //and in auto-commit mode
                if (tID != null)
                {
                    if (activeTID == null)
                    {
                        try
                        {
                            this.RollbackTransaction(tID);
                        }
                        catch (Exception ex)
                        {
                            StorageHelper.HandleHttpError(webEx, "");
                            throw new RdfStorageException("Blazegraph failed to rollback a Transaction", ex);
                        }
                    }
                }

                throw StorageHelper.HandleHttpError(webEx, "saving a Graph to");
            }
        }

        /// <summary>
        /// Updates a Graph in the Blazegraph store
        /// </summary>
        /// <param name="graphUri">Uri of the Graph to update</param>
        /// <param name="additions">Triples to be added</param>
        /// <param name="removals">Triples to be removed</param>
        public virtual void UpdateGraph(string graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            if (graphUri == null || graphUri.Equals(String.Empty))
            {
                this.UpdateGraph((Uri)null, additions, removals);
            }
            else
            {
                this.UpdateGraph(UriFactory.Create(graphUri), additions, removals);
            }
        }

        /// <summary>
        /// Updates a Graph in the Blazegraph Store
        /// </summary>
        /// <param name="graphUri">Uri of the Graph to update</param>
        /// <param name="additions">Triples to be added</param>
        /// <param name="removals">Triples to be removed</param>
        public virtual void UpdateGraph(Uri graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals)
        {
            //If there are no adds or deletes, just return and avoid creating empty transaction
            bool anyData = (removals != null && removals.Any()) || (additions != null && additions.Any());
            if (!anyData) return;

            string activeTID;
            string tID = null;
            lock (transLock)
            {
                activeTID = this._activeTrans;
            }

            try
            {
                //Get a Transaction ID, if there is no active Transaction then this operation will be auto-committed
                tID = activeTID ?? this.BeginTransaction();

                Dictionary<string, string> queryParams = new Dictionary<string, string>();
                if (activeTID == null)
                {
                    queryParams.Add("timestamp", tID);
                }

                HttpWebRequest request;
                string boundary = String.Format("----------{0:N}", Guid.NewGuid());
                StringBuilder sb = new StringBuilder();

                request = this.CreateRequest("/sparql?updatePost", MimeTypesHelper.Any, "POST", queryParams);
                request.ContentType = MimeTypesHelper.FormMultipart + "; boundary=" + boundary;

                string addData = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\nContent-Type: {2}\r\n\r\n{3}",
                    boundary,
                    "add",
                    MimeTypesHelper.GetDefinitionsByFileExtension(MimeTypesHelper.DefaultTriGExtension).First().MimeTypes.First(),
                    "");
                sb.Append(addData);

                //Save the Data as TriG to the Request Stream
                TripleStore store = new TripleStore();
                Graph g = new Graph();
                if (graphUri != null)
                {
                    g.BaseUri = graphUri;
                }
                g.Assert(additions);
                store.Add(g);
                this._writer.Save(store, new System.IO.StringWriter(sb));

                string removeData = string.Format("\r\n--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\nContent-Type: {2}\r\n\r\n{3}",
                    boundary,
                    "remove",
                    MimeTypesHelper.GetDefinitionsByFileExtension(MimeTypesHelper.DefaultTriGExtension).First().MimeTypes.First(),
                    "");
                sb.Append(removeData);

                store = new TripleStore();
                g = new Graph();
                if (graphUri != null)
                {
                    g.BaseUri = graphUri;
                }
                g.Assert(removals);
                store.Add(g);
                this._writer.Save(store, new System.IO.StringWriter(sb));

                string footer = "\r\n--" + boundary + "--\r\n";
                sb.Append(footer);

                using (StreamWriter writer = new StreamWriter(request.GetRequestStream(), new UTF8Encoding()))
                {
                    writer.Write(sb.ToString());
                    writer.Close();
                }

                Tools.HttpDebugRequest(request);

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    Tools.HttpDebugResponse(response);
                    //If we get here then it was OK
                    response.Close();
                }

                //Commit Transaction only if in auto-commit mode (active transaction will be null)
                if (activeTID != null) return;
                try
                {
                    this.CommitTransaction(tID);
                }
                catch (Exception ex)
                {
                    throw new RdfStorageException("Blazegraph failed to commit a Transaction", ex);
                }
            }
            catch (WebException webEx)
            {
                //Rollback Transaction only if got as far as creating a Transaction
                //and in auto-commit mode
                if (tID != null)
                {
                    if (activeTID == null)
                    {
                        try
                        {
                            this.RollbackTransaction(tID);
                        }
                        catch (Exception ex)
                        {
                            StorageHelper.HandleHttpError(webEx, "");
                            throw new RdfStorageException("Blazegraph failed to rollback a Transaction", ex);
                        }
                    }
                }

                throw StorageHelper.HandleHttpError(webEx, "updating a Graph to");
            }
        }

        #endregion

        #region IAsyncStorageProvider

        /// <summary>
        /// Loads a Graph from the Store asynchronously
        /// </summary>
        /// <param name="handler">Handler to load with</param>
        /// <param name="graphUri">URI of the Graph to load</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        public override void LoadGraph(IRdfHandler handler, string graphUri, AsyncStorageCallback callback, object state)
        {
            try
            {
                HttpWebRequest request;
                Dictionary<String, String> serviceParams = new Dictionary<string, string>();
                Uri baseUri = null;

                SparqlParameterizedString construct = new SparqlParameterizedString();
                if (!graphUri.Equals(string.Empty))
                {
                    construct.CommandText = "CONSTRUCT { ?s ?p ?o } WHERE { GRAPH @graph { ?s ?p ?o } }";
                    baseUri = UriFactory.Create(graphUri);
                    construct.SetUri("graph", baseUri);
                }
                else
                {
                    construct.CommandText = "CONSTRUCT { ?s ?p ?o } WHERE { ?s ?p ?o }";
                }
                serviceParams.Add("query", construct.ToString());

                request = this.CreateRequest("/sparql", MimeTypesHelper.HttpAcceptHeader, "GET", serviceParams);

                Tools.HttpDebugRequest(request);

                request.BeginGetResponse(r =>
                {
                    try
                    {
                        using (HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(r))
                        {
                            Tools.HttpDebugResponse(response);

                            IRdfReader parser = MimeTypesHelper.GetParser(response.ContentType);
                            parser.Load(handler, new StreamReader(response.GetResponseStream()));
                            if (baseUri != null)
                            {
                                handler.StartRdf();
                                handler.HandleBaseUri(baseUri);
                                handler.EndRdf(true);
                            }
                            response.Close();
                        }

                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.LoadWithHandler, handler), state);
                    }
                    catch (WebException webEx)
                    {
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.LoadWithHandler, StorageHelper.HandleHttpError(webEx, "loading a Graph from")), state);
                    }
                    catch (Exception ex)
                    {
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.LoadWithHandler, StorageHelper.HandleError(ex, "loading a Graph from")), state);
                    }
                }, state);
            }
            catch (WebException webEx)
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.LoadWithHandler, StorageHelper.HandleHttpError(webEx, "loading a Graph from")), state);
            }
            catch (Exception ex)
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.LoadWithHandler, StorageHelper.HandleError(ex, "loading a Graph from")), state);
            }
        }

        /// <summary>
        /// Saves a Graph to the Store asynchronously
        /// </summary>
        /// <param name="g">Graph to save</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        public override void SaveGraph(IGraph g, AsyncStorageCallback callback, object state)
        {
            this.SaveGraphAsync(g, callback, state);
        }

        protected virtual void SaveGraphAsync(IGraph g, AsyncStorageCallback callback, object state)
        {
            //Get a Transaction ID, if there is no active Transaction then this operation will start a new transaction and be auto-committed
            string activeTID;
            lock (transLock)
            {
                activeTID = this._activeTrans;
            }

            if (activeTID != null)
            {
                this.SaveGraphAsync(false, g, callback, state);
            }
            else
            {
                this.Begin((sender, args, st) =>
                {
                    if (args.WasSuccessful)
                    {
                        this.SaveGraphAsync(true, g, callback, state);
                    }
                    else
                    {
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SaveGraph, g, args.Error), state);
                    }
                }, state);
            }
        }

        protected virtual void SaveGraphAsync(bool autoCommit, IGraph g, AsyncStorageCallback callback, object state)
        {
            try
            {
                Dictionary<string, string> queryParams = new Dictionary<string, string>();

                HttpWebRequest request;
                string boundary = String.Format("----------{0:N}", Guid.NewGuid());
                StringBuilder sb = new StringBuilder();

                if (g.BaseUri != null)
                {
                    SparqlParameterizedString construct = new SparqlParameterizedString();
                    construct.CommandText = "CONSTRUCT { ?s ?p ?o } WHERE { GRAPH @graph { ?s ?p ?o } }";
                    construct.SetUri("graph", g.BaseUri);
                    queryParams.Add("query", construct.ToString());

                    request = this.CreateRequest("/sparql", MimeTypesHelper.Any, "PUT", queryParams);
                    request.ContentType = MimeTypesHelper.GetDefinitionsByFileExtension(MimeTypesHelper.DefaultTriGExtension).First().MimeTypes.First();
                }
                else
                {
                    request = this.CreateRequest("/sparql?updatePost", MimeTypesHelper.Any, "POST", queryParams);
                    request.ContentType = MimeTypesHelper.FormMultipart + "; boundary=" + boundary;

                    string postData = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\nContent-Type: {2}\r\n\r\n{3}",
                        boundary,
                        "add",
                        MimeTypesHelper.GetDefinitionsByFileExtension(MimeTypesHelper.DefaultTriGExtension).First().MimeTypes.First(),
                        "");
                    sb.Append(postData);
                }

                //Save the Data as TriG to the Request Stream
                TripleStore store = new TripleStore();
                store.Add(g);
                this._writer.Save(store, new System.IO.StringWriter(sb));

                if (g.BaseUri == null)
                {
                    string footer = "\r\n--" + boundary + "--\r\n";
                    sb.Append(footer);
                }

                request.BeginGetRequestStream(r =>
                {
                    try
                    {
                        //Save the Data as TriG to the Request Stream
                        using (StreamWriter writer = new StreamWriter(request.EndGetRequestStream(r), new UTF8Encoding()))
                        {
                            writer.Write(sb.ToString());
                            writer.Close();
                        }

                        Tools.HttpDebugRequest(request);
                        request.BeginGetResponse(r2 =>
                        {
                            try
                            {
                                using (HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(r2))
                                {
                                    Tools.HttpDebugResponse(response);

                                    //If we get here then it was OK
                                    response.Close();
                                }

                                //Commit Transaction only if in auto-commit mode (active transaction will be null)
                                if (autoCommit)
                                {
                                    this.Commit((sender, args, st) =>
                                    {
                                        if (args.WasSuccessful)
                                        {
                                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SaveGraph, g), state);
                                        }
                                        else
                                        {
                                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SaveGraph, args.Error), state);
                                        }
                                    }, state);
                                }
                                else
                                {
                                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SaveGraph, g), state);
                                }
                            }
                            catch (WebException webEx)
                            {
                                if (autoCommit)
                                {
                                    //If something went wrong try to rollback, don't care what the rollback response is
                                    this.Rollback((sender, args, st) => { }, state);
                                }
                                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SaveGraph, StorageHelper.HandleHttpError(webEx, "saving a Graph asynchronously to")), state);
                            }
                            catch (Exception ex)
                            {
                                if (autoCommit)
                                {
                                    //If something went wrong try to rollback, don't care what the rollback response is
                                    this.Rollback((sender, args, st) => { }, state);
                                }
                                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SaveGraph, StorageHelper.HandleError(ex, "saving a Graph asynchronously to")), state);
                            }
                        }, state);
                    }
                    catch (WebException webEx)
                    {
                        if (autoCommit)
                        {
                            //If something went wrong try to rollback, don't care what the rollback response is
                            this.Rollback((sender, args, st) => { }, state);
                        }
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SaveGraph, StorageHelper.HandleHttpError(webEx, "saving a Graph asynchronously to")), state);
                    }
                    catch (Exception ex)
                    {
                        if (autoCommit)
                        {
                            //If something went wrong try to rollback, don't care what the rollback response is
                            this.Rollback((sender, args, st) => { }, state);
                        }
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SaveGraph, StorageHelper.HandleError(ex, "saving a Graph asynchronously to")), state);
                    }
                }, state);
            }
            catch (WebException webEx)
            {
                if (autoCommit)
                {
                    //If something went wrong try to rollback, don't care what the rollback response is
                    this.Rollback((sender, args, st) => { }, state);
                }
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SaveGraph, StorageHelper.HandleHttpError(webEx, "saving a Graph asynchronously to")), state);
            }
            catch (Exception ex)
            {
                if (autoCommit)
                {
                    //If something went wrong try to rollback, don't care what the rollback response is
                    this.Rollback((sender, args, st) => { }, state);
                }
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.SaveGraph, StorageHelper.HandleError(ex, "saving a Graph asynchronously to")), state);
            }
        }

        /// <summary>
        /// Updates a Graph in the Store asychronously
        /// </summary>
        /// <param name="graphUri">URI of the Graph to update</param>
        /// <param name="additions">Triples to be added</param>
        /// <param name="removals">Triples to be removed</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        public override void UpdateGraph(string graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals, AsyncStorageCallback callback, object state)
        {
            //If there are no adds or deletes, just callback and avoid creating empty transaction
            bool anyData = false;
            if (removals != null && removals.Any()) anyData = true;
            if (additions != null && additions.Any()) anyData = true;
            if (!anyData)
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, graphUri.ToSafeUri()), state);
            }
            else
            {
                this.UpdateGraphAsync(graphUri, additions, removals, callback, state);
            }
        }

        protected virtual void UpdateGraphAsync(string graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals, AsyncStorageCallback callback, object state)
        {
            //Get a Transaction ID, if there is no active Transaction then this operation will start a new transaction and be auto-committed
            string activeTID;
            lock (transLock)
            {
                activeTID = this._activeTrans;
            }

            if (activeTID != null)
            {
                this.UpdateGraphAsync(false, graphUri, additions, removals, callback, state);
            }
            else
            {
                this.Begin((sender, args, st) =>
                {
                    if (args.WasSuccessful)
                    {
                        this.UpdateGraphAsync(true, graphUri, additions, removals, callback, state);
                    }
                    else
                    {
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, graphUri.ToSafeUri(), args.Error), state);
                    }
                }, state);
            }
        }

        protected virtual void UpdateGraphAsync(bool autoCommit, string graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals, AsyncStorageCallback callback, object state)
        {
            try
            {
                Dictionary<string, string> queryParams = new Dictionary<string, string>();

                HttpWebRequest request;
                string boundary = String.Format("----------{0:N}", Guid.NewGuid());
                StringBuilder sb = new StringBuilder();

                request = this.CreateRequest("/sparql?updatePost", MimeTypesHelper.Any, "POST", queryParams);
                request.ContentType = MimeTypesHelper.FormMultipart + "; boundary=" + boundary;

                string addData = string.Format("--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\nContent-Type: {2}\r\n\r\n{3}",
                    boundary,
                    "add",
                    MimeTypesHelper.GetDefinitionsByFileExtension(MimeTypesHelper.DefaultTriGExtension).First().MimeTypes.First(),
                    "");
                sb.Append(addData);

                //Save the Data as TriG to the Request Stream
                TripleStore store = new TripleStore();
                Graph g = new Graph();
                if (!string.IsNullOrEmpty(graphUri))
                {
                    g.BaseUri = UriFactory.Create(graphUri);
                }
                g.Assert(additions);
                store.Add(g);
                this._writer.Save(store, new System.IO.StringWriter(sb));

                string removeData = string.Format("\r\n--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\nContent-Type: {2}\r\n\r\n{3}",
                    boundary,
                    "remove",
                    MimeTypesHelper.GetDefinitionsByFileExtension(MimeTypesHelper.DefaultTriGExtension).First().MimeTypes.First(),
                    "");
                sb.Append(removeData);

                store = new TripleStore();
                g = new Graph();
                if (!string.IsNullOrEmpty(graphUri))
                {
                    g.BaseUri = UriFactory.Create(graphUri);
                }
                g.Assert(removals);
                store.Add(g);
                this._writer.Save(store, new System.IO.StringWriter(sb));

                string footer = "\r\n--" + boundary + "--\r\n";
                sb.Append(footer);

                request.BeginGetRequestStream(r =>
                {
                    try
                    {
                        //Save the Data as TriG to the Request Stream
                        using (StreamWriter writer = new StreamWriter(request.EndGetRequestStream(r), new UTF8Encoding()))
                        {
                            writer.Write(sb.ToString());
                            writer.Close();
                        }

                        Tools.HttpDebugRequest(request);
                        request.BeginGetResponse(r2 =>
                        {
                            try
                            {
                                using (HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(r2))
                                {
                                    Tools.HttpDebugResponse(response);

                                    //If we get here then it was OK
                                    response.Close();
                                }

                                //Commit Transaction only if in auto-commit mode (active transaction will be null)
                                if (autoCommit)
                                {
                                    this.Commit((sender, args, st) =>
                                    {
                                        if (args.WasSuccessful)
                                        {
                                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, graphUri.ToSafeUri()), state);
                                        }
                                        else
                                        {
                                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, graphUri.ToSafeUri(), args.Error), state);
                                        }
                                    }, state);
                                }
                                else
                                {
                                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, graphUri.ToSafeUri()), state);
                                }
                            }
                            catch (WebException webEx)
                            {
                                if (autoCommit)
                                {
                                    //If something went wrong try to rollback, don't care what the rollback response is
                                    this.Rollback((sender, args, st) => { }, state);
                                }
                                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, StorageHelper.HandleHttpError(webEx, "updating a Graph asynchronously in")), state);
                            }
                            catch (Exception ex)
                            {
                                if (autoCommit)
                                {
                                    //If something went wrong try to rollback, don't care what the rollback response is
                                    this.Rollback((sender, args, st) => { }, state);
                                }
                                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, StorageHelper.HandleError(ex, "updating a Graph asynchronously in")), state);
                            }
                        }, state);
                    }
                    catch (WebException webEx)
                    {
                        if (autoCommit)
                        {
                            //If something went wrong try to rollback, don't care what the rollback response is
                            this.Rollback((sender, args, st) => { }, state);
                        }
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, StorageHelper.HandleHttpError(webEx, "updating a Graph asynchronously in")), state);
                    }
                    catch (Exception ex)
                    {
                        if (autoCommit)
                        {
                            //If something went wrong try to rollback, don't care what the rollback response is
                            this.Rollback((sender, args, st) => { }, state);
                        }
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, StorageHelper.HandleError(ex, "updating a Graph asynchronously in")), state);
                    }
                }, state);
            }
            catch (WebException webEx)
            {
                if (autoCommit)
                {
                    //If something went wrong try to rollback, don't care what the rollback response is
                    this.Rollback((sender, args, st) => { }, state);
                }
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, StorageHelper.HandleHttpError(webEx, "updating a Graph asynchronously in")), state);
            }
            catch (Exception ex)
            {
                if (autoCommit)
                {
                    //If something went wrong try to rollback, don't care what the rollback response is
                    this.Rollback((sender, args, st) => { }, state);
                }
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.UpdateGraph, StorageHelper.HandleError(ex, "updating a Graph asynchronously in")), state);
            }
        }

        /// <summary>
        /// Deletes a Graph from the Store
        /// </summary>
        /// <param name="graphUri">URI of the Graph to delete</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        public override void DeleteGraph(string graphUri, AsyncStorageCallback callback, object state)
        {
            //Get a Transaction ID, if there is no active Transaction then this operation will start a new transaction and be auto-committed
            string activeTID;
            lock (transLock)
            {
                activeTID = this._activeTrans;
            }

            if (activeTID != null)
            {
                this.DeleteGraphAsync(false, graphUri, callback, state);
            }
            else
            {
                this.Begin((sender, args, st) =>
                {
                    if (args.WasSuccessful)
                    {
                        this.DeleteGraphAsync(true, graphUri, callback, state);
                    }
                    else
                    {
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.DeleteGraph, graphUri.ToSafeUri(), args.Error), state);
                    }
                }, state);
            }
        }

        protected virtual void DeleteGraphAsync(bool autoCommit, string graphUri, AsyncStorageCallback callback, object state)
        {
            try
            {
                Uri graph = UriFactory.Create(graphUri);
                Dictionary<string, string> queryParams = new Dictionary<string, string>();
                if (!graphUri.Equals(string.Empty))
                {
                    queryParams.Add("c", "<" + graph.ToSafeString() + ">");
                }
                HttpWebRequest request = this.CreateRequest("/sparql", MimeTypesHelper.Any, "DELETE", queryParams);

                request.ContentType = MimeTypesHelper.WWWFormURLEncoded;

                Tools.HttpDebugRequest(request);
                request.BeginGetResponse(r =>
                {
                    try
                    {
                        using (HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(r))
                        {
                            Tools.HttpDebugResponse(response);
                            //If we get here then the Delete worked OK
                            response.Close();
                        }

                        //Commit Transaction only if in auto-commit mode (active transaction will be null)
                        if (autoCommit)
                        {
                            this.Commit((sender, args, st) =>
                            {
                                if (args.WasSuccessful)
                                {
                                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.DeleteGraph, graphUri.ToSafeUri()), state);
                                }
                                else
                                {
                                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.DeleteGraph, graphUri.ToSafeUri(), args.Error), state);
                                }
                            }, state);
                        }
                        else
                        {
                            callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.DeleteGraph, graphUri.ToSafeUri()), state);
                        }
                    }
                    catch (WebException webEx)
                    {
                        if (autoCommit)
                        {
                            //If something went wrong try to rollback, don't care what the rollback response is
                            this.Rollback((sender, args, st) => { }, state);
                        }
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.DeleteGraph, graphUri.ToSafeUri(), StorageHelper.HandleHttpError(webEx, "deleting a Graph asynchronously from")), state);
                    }
                    catch (Exception ex)
                    {
                        if (autoCommit)
                        {
                            //If something went wrong try to rollback, don't care what the rollback response is
                            this.Rollback((sender, args, st) => { }, state);
                        }
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.DeleteGraph, graphUri.ToSafeUri(), StorageHelper.HandleError(ex, "deleting a Graph asynchronously from")), state);
                    }
                }, state);
            }
            catch (WebException webEx)
            {
                if (autoCommit)
                {
                    //If something went wrong try to rollback, don't care what the rollback response is
                    this.Rollback((sender, args, st) => { }, state);
                }
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.DeleteGraph, graphUri.ToSafeUri(), StorageHelper.HandleHttpError(webEx, "deleting a Graph asynchronously from")), state);
            }
            catch (Exception ex)
            {
                if (autoCommit)
                {
                    //If something went wrong try to rollback, don't care what the rollback response is
                    this.Rollback((sender, args, st) => { }, state);
                }
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.DeleteGraph, graphUri.ToSafeUri(), StorageHelper.HandleError(ex, "deleting a Graph asynchronously from")), state);
            }
        }

        #endregion

        #region IAsyncTransactionalStorage

        /// <summary>
        /// Begins a transaction asynchronously
        /// </summary>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        public virtual void Begin(AsyncStorageCallback callback, object state)
        {
            string tID;
            lock (transLock)
            {
                tID = this._activeTrans;
            }

            try
            {
                if (tID != null)
                {
                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionBegin, new RdfStorageException("Cannot start a new Transaction as there is already an active Transaction")), state);
                }
                else
                {
                    HttpWebRequest request = this.CreateRequest("/tx?timestamp=-1", "application/xml", "POST", new Dictionary<string, string>(), false);
                    request.ContentType = MimeTypesHelper.WWWFormURLEncoded;
                    try
                    {
                        Tools.HttpDebugRequest(request);
                        request.BeginGetResponse(r =>
                        {
                            try
                            {
                                string tResponse = null;
                                using (HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(r))
                                {
                                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                                    {
                                        Tools.HttpDebugResponse(response);
                                        tResponse = reader.ReadToEnd();
                                        reader.Close();
                                    }
                                    response.Close();
                                }

                                XmlDocument doc = new XmlDocument();
                                doc.LoadXml(tResponse);
                                var node = doc.SelectSingleNode("/response/tx/@txId");
                                if (node != null)
                                {
                                    tID = node.Value;
                                }

                                if (string.IsNullOrEmpty(tID))
                                {
                                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionBegin, new RdfStorageException("Blazegraph failed to begin a transaction")), state);
                                }
                                else
                                {
                                    lock (transLock)
                                    {
                                        this._activeTrans = tID;
                                    }
                                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionBegin), state);
                                }
                            }
                            catch (WebException webEx)
                            {
                                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionBegin, StorageHelper.HandleHttpError(webEx, "beginning a Transaction in")), state);
                            }
                            catch (Exception ex)
                            {
                                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionBegin, StorageHelper.HandleError(ex, "beginning a Transaction in")), state);
                            }
                        }, state);
                    }
                    catch (WebException webEx)
                    {
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionBegin, StorageHelper.HandleHttpError(webEx, "beginning a Transaction in")), state);
                    }
                    catch (Exception ex)
                    {
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionBegin, StorageHelper.HandleError(ex, "beginning a Transaction in")), state);
                    }
                }
            }
            catch (WebException webEx)
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionBegin, StorageHelper.HandleHttpError(webEx, "beginning a Transaction in")), state);
            }
            catch (Exception ex)
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionBegin, StorageHelper.HandleError(ex, "beginning a Transaction in")), state);
            }
        }

        /// <summary>
        /// Commits a transaction asynchronously
        /// </summary>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        public virtual void Commit(AsyncStorageCallback callback, object state)
        {
            string tID;
            lock (transLock)
            {
                tID = this._activeTrans;
            }

            try
            {
                if (tID == null)
                {
                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionCommit, new RdfStorageException("Cannot commit a Transaction as there is currently no active Transaction")), state);
                }
                else
                {
                    HttpWebRequest request = this.CreateRequest("/tx/" + tID + "?COMMIT", "application/xml", "POST", new Dictionary<string, string>(), false);
                    request.ContentType = MimeTypesHelper.WWWFormURLEncoded;
                    Tools.HttpDebugRequest(request);
                    try
                    {
                        request.BeginGetResponse(r =>
                        {
                            try
                            {
                                using (HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(r))
                                {
                                    Tools.HttpDebugResponse(response);
                                    response.Close();
                                }

                                lock (transLock)
                                {
                                    if (this._activeTrans != null && this._activeTrans.Equals(tID))
                                    {
                                        this._activeTrans = null;
                                    }
                                }

                                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionCommit), state);
                            }
                            catch (WebException webEx)
                            {
                                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionCommit, StorageHelper.HandleHttpError(webEx, "committing a Transaction to")), state);
                            }
                            catch (Exception ex)
                            {
                                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionCommit, StorageHelper.HandleError(ex, "committing a Transaction to")), state);
                            }
                        }, state);
                    }
                    catch (WebException webEx)
                    {
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionCommit, StorageHelper.HandleHttpError(webEx, "committing a Transaction to")), state);
                    }
                    catch (Exception ex)
                    {
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionCommit, StorageHelper.HandleError(ex, "committing a Transaction to")), state);
                    }
                }
            }
            catch (WebException webEx)
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionCommit, StorageHelper.HandleHttpError(webEx, "committing a Transaction to")), state);
            }
            catch (Exception ex)
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionCommit, StorageHelper.HandleError(ex, "committing a Transaction to")), state);
            }
        }

        /// <summary>
        /// Rolls back a transaction asynchronously
        /// </summary>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        public virtual void Rollback(AsyncStorageCallback callback, object state)
        {
            string tID;
            lock (transLock)
            {
                tID = this._activeTrans;
            }

            try
            {
                if (tID == null)
                {
                    callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionRollback, new RdfStorageException("Cannot rollback a Transaction on the as there is currently no active Transaction")), state);
                }
                else
                {
                    HttpWebRequest request = this.CreateRequest("/tx/" + tID + "?ABORT", "application/xml", "POST", new Dictionary<string, string>(), false);
                    request.ContentType = MimeTypesHelper.WWWFormURLEncoded;
                    try
                    {
                        request.BeginGetResponse(r =>
                        {
                            try
                            {
                                using (HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(r))
                                {
                                    response.Close();
                                }

                                lock (transLock)
                                {
                                    if (this._activeTrans != null && this._activeTrans.Equals(tID))
                                    {
                                        this._activeTrans = null;
                                    }
                                }

                                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionRollback), state);
                            }
                            catch (WebException webEx)
                            {
                                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionRollback, StorageHelper.HandleHttpError(webEx, "rolling back a Transaction from")), state);
                            }
                            catch (Exception ex)
                            {
                                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionRollback, StorageHelper.HandleError(ex, "rolling back a Transaction from")), state);
                            }
                        }, state);
                    }
                    catch (WebException webEx)
                    {
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionRollback, StorageHelper.HandleHttpError(webEx, "rolling back a Transaction from")), state);
                    }
                    catch (Exception ex)
                    {
                        callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionRollback, StorageHelper.HandleError(ex, "rolling back a Transaction from")), state);
                    }
                }
            }
            catch (WebException webEx)
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionRollback, StorageHelper.HandleHttpError(webEx, "rolling back a Transaction from")), state);
            }
            catch (Exception ex)
            {
                callback(this, new AsyncStorageCallbackArgs(AsyncStorageOperation.TransactionRollback, StorageHelper.HandleError(ex, "rolling back a Transaction from")), state);
            }
        }

        #endregion

        #region ITransactionalStorage

        protected virtual string BeginTransaction()
        {
            string tID = null;
            string tResponse = null;

            HttpWebRequest request = this.CreateRequest("/tx?timestamp=-1", "application/xml", "POST", new Dictionary<string, string>(), false);
            request.ContentType = MimeTypesHelper.WWWFormURLEncoded;
            try
            {
                Tools.HttpDebugRequest(request);

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        Tools.HttpDebugResponse(response);

                        tResponse = reader.ReadToEnd();
                        reader.Close();
                    }
                    response.Close();
                }

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(tResponse);
                var node = doc.SelectSingleNode("/response/tx/@txId");
                if (node != null)
                {
                    tID = node.Value;
                }
            }
            catch (Exception ex)
            {
                throw StorageHelper.HandleError(ex, "beginning a Transaction in");
            }

            if (string.IsNullOrEmpty(tID))
            {
                throw new RdfStorageException("Blazegraph failed to begin a Transaction");
            }
            return tID;
        }

        protected virtual void CommitTransaction(string tID)
        {
            HttpWebRequest request = this.CreateRequest("/tx/" + tID + "?COMMIT", "application/xml", "POST", new Dictionary<string, string>(), false);
            request.ContentType = MimeTypesHelper.WWWFormURLEncoded;

            Tools.HttpDebugRequest(request);

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                Tools.HttpDebugResponse(response);
                response.Close();
            }

            //Reset the Active Transaction on this Thread if the IDs match up
            if (this._activeTrans != null && this._activeTrans.Equals(tID))
            {
                this._activeTrans = null;
            }
        }

        protected virtual void RollbackTransaction(string tID)
        {
            HttpWebRequest request = this.CreateRequest("/tx/" + tID + "?ABORT", "application/xml", "POST", new Dictionary<string, string>(), false);
            request.ContentType = MimeTypesHelper.WWWFormURLEncoded;
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                response.Close();
            }

            //Reset the Active Transaction on this Thread if the IDs match up
            if (this._activeTrans != null && this._activeTrans.Equals(tID))
            {
                this._activeTrans = null;
            }
        }

        /// <summary>
        /// Begins a new Transaction
        /// </summary>
        /// <remarks>
        /// A single transaction
        /// </remarks>
        public virtual void Begin()
        {
            lock (transLock)
            {
                if (this._activeTrans != null)
                {
                    throw new RdfStorageException("Cannot start a new Transaction as there is already an active Transaction");
                }
                this._activeTrans = this.BeginTransaction();
            }
        }

        /// <summary>
        /// Commits the active Transaction
        /// </summary>
        /// <exception cref="RdfStorageException">Thrown if there is not an active Transaction on the current Thread</exception>
        /// <remarks>
        /// Transactions are scoped to Managed Threads
        /// </remarks>
        public virtual void Commit()
        {
            lock (transLock)
            {
                if (this._activeTrans == null)
                {
                    throw new RdfStorageException("Cannot commit a Transaction as there is currently no active Transaction");
                }
                this.CommitTransaction(this._activeTrans);
            }
        }

        /// <summary>
        /// Rolls back the active Transaction
        /// </summary>
        /// <exception cref="RdfStorageException">Thrown if there is not an active Transaction on the current Thread</exception>
        /// <remarks>
        /// Transactions are scoped to Managed Threads
        /// </remarks>
        public virtual void Rollback()
        {
            lock (transLock)
            {
                if (this._activeTrans == null)
                {
                    throw new RdfStorageException("Cannot rollback a Transaction on the as there is currently no active Transaction");
                }
                this.RollbackTransaction(this._activeTrans);
            }
        }

        #endregion

        /// <summary>
        /// Gets a String which gives details of the Connection
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "[Blazegraph] Namespace '" + this._ns + "' on Server '" + this._baseUri + "'";
        }

        /// <summary>
        /// Serializes the connection's configuration
        /// </summary>
        /// <param name="context">Configuration Serialization Context</param>
        public virtual void SerializeConfiguration(ConfigurationSerializationContext context)
        {
            INode manager = context.NextSubject;
            INode rdfType = context.Graph.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfType));
            INode rdfsLabel = context.Graph.CreateUriNode(UriFactory.Create(NamespaceMapper.RDFS + "label"));
            INode dnrType = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyType));
            INode genericManager = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.ClassStorageProvider));
            INode server = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyServer));
            INode store = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyStore));
            INode loadMode = context.Graph.CreateUriNode(UriFactory.Create(ConfigurationLoader.PropertyLoadMode));

            //Add Core config
            context.Graph.Assert(new Triple(manager, rdfType, genericManager));
            context.Graph.Assert(new Triple(manager, rdfsLabel, context.Graph.CreateLiteralNode(this.ToString())));
            context.Graph.Assert(new Triple(manager, dnrType, context.Graph.CreateLiteralNode(this.GetType().FullName)));
            context.Graph.Assert(new Triple(manager, server, context.Graph.CreateLiteralNode(this._baseUri)));
            context.Graph.Assert(new Triple(manager, store, context.Graph.CreateLiteralNode(this._ns)));

            base.SerializeStandardConfig(manager, context);
        }
    }
}
