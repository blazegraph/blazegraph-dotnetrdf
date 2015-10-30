using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF;
using VDS.RDF.Storage;

namespace VDS.RDF.Storage
{
    /// <summary>
    /// Interface for storage providers which allow SPARQL Queries to be made against them. Additionally, supports timeout for the query.
    /// </summary>
    public interface IQueryableTimeoutStorage : IQueryableStorage
    {
        /// <summary>
        /// Makes a SPARQL Query against the underlying store
        /// </summary>
        /// <param name="sparqlQuery">SPARQL Query</param>
        /// <param name="timeoutMilliseconds">Query timeout in milliseconds</param>
        /// <returns><see cref="SparqlResultSet">SparqlResultSet</see> or a <see cref="Graph">Graph</see> depending on the Sparql Query</returns>
        /// <exception cref="RdfQueryException">Thrown if an error occurs performing the query</exception>
        /// <exception cref="RdfStorageException">Thrown if an error occurs performing the query</exception>
        /// <exception cref="RdfParseException">Thrown if the query is invalid when validated by dotNetRDF prior to passing the query request to the store or if the request succeeds but the store returns malformed results</exception>
        /// <exception cref="RdfParserSelectionException">Thrown if the store returns results in a format dotNetRDF does not understand</exception>
        object Query(string sparqlQuery, long timeoutMilliseconds);

        /// <summary>
        /// Makes a SPARQL Query against the underlying store processing the resulting Graph/Result Set with a handler of your choice
        /// </summary>
        /// <param name="rdfHandler">RDF Handler</param>
        /// <param name="resultsHandler">SPARQL Results Handler</param>
        /// <param name="sparqlQuery">SPARQL Query</param>
        /// <param name="timeoutMilliseconds">Query timeout in milliseconds</param>
        /// <exception cref="RdfQueryException">Thrown if an error occurs performing the query</exception>
        /// <exception cref="RdfStorageException">Thrown if an error occurs performing the query</exception>
        /// <exception cref="RdfParseException">Thrown if the query is invalid when validated by dotNetRDF prior to passing the query request to the store or if the request succeeds but the store returns malformed results</exception>
        /// <exception cref="RdfParserSelectionException">Thrown if the store returns results in a format dotNetRDF does not understand</exception>
        void Query(IRdfHandler rdfHandler, ISparqlResultsHandler resultsHandler, string sparqlQuery, long timeoutMilliseconds);
    }
}
