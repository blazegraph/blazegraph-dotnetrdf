using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF;
using VDS.RDF.Storage;

namespace VDS.RDF.Storage
{
    /// <summary>
    /// Interface for storage providers which allow SPARQL Queries to be made against them asynchronously. Additionally, supports timeout for the query.
    /// </summary>
    public interface IAsyncQueryableTimeoutStorage : IAsyncQueryableStorage
    {
        /// <summary>
        /// Queries the store asynchronously
        /// </summary>
        /// <param name="sparqlQuery">SPARQL Query</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        /// <param name="timeoutMilliseconds">Query timeout in milliseconds</param>
        /// <exception cref="RdfQueryException">Thrown if an error occurs performing the query</exception>
        /// <exception cref="RdfStorageException">Thrown if an error occurs performing the query</exception>
        /// <exception cref="RdfParseException">Thrown if the query is invalid when validated by dotNetRDF prior to passing the query request to the store or if the request succeeds but the store returns malformed results</exception>
        /// <exception cref="RdfParserSelectionException">Thrown if the store returns results in a format dotNetRDF does not understand</exception>
        void Query(String sparqlQuery, AsyncStorageCallback callback, Object state, long timeoutMilliseconds);

        /// <summary>
        /// Queries the store asynchronously
        /// </summary>
        /// <param name="sparqlQuery">SPARQL Query</param>
        /// <param name="rdfHandler">RDF Handler</param>
        /// <param name="resultsHandler">Results Handler</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        /// <param name="timeoutMilliseconds">Query timeout in milliseconds</param>
        /// <exception cref="RdfQueryException">Thrown if an error occurs performing the query</exception>
        /// <exception cref="RdfStorageException">Thrown if an error occurs performing the query</exception>
        /// <exception cref="RdfParseException">Thrown if the query is invalid when validated by dotNetRDF prior to passing the query request to the store or if the request succeeds but the store returns malformed results</exception>
        /// <exception cref="RdfParserSelectionException">Thrown if the store returns results in a format dotNetRDF does not understand</exception>
        void Query(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, String sparqlQuery, AsyncStorageCallback callback, Object state, long timeoutMilliseconds);
    }
}
