using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Query;
using VDS.RDF.Storage;

namespace BlazegraphConnectorTest
{
    class Program
    {
        private BlazegraphConnector connector = new BlazegraphConnector("http://localhost:9999/bigdata/");

        static void Main(string[] args)
        {
            Program program = new Program();

            program.RemoveAllData();
            program.CreateExampleGraph("http://example/bookStore");
            program.LoadExampleGraph("http://example/bookStore");
            program.UpdateExampleGraph("http://example/bookStore");
            program.QueryExample("SELECT * { ?s ?p ?o } LIMIT 1");
            program.QueryWithTimeoutExample("SELECT * { ?s ?p ?o } LIMIT 1", 1);
            program.DeleteGraphExample("http://example/bookStore");

            Console.ReadKey();
        }

        public void RemoveAllData()
        {
            connector.DeleteGraph((string)null);
        }

        public Graph CreateExampleGraph(string uri)
        {
            Graph newGraph = new Graph();
            newGraph.BaseUri = UriFactory.Create(uri);

            Triple triple = new Triple(
                newGraph.CreateUriNode(UriFactory.Create("http://example/book1")),
                newGraph.CreateUriNode(UriFactory.Create("http://example.org/ns#price")),
                newGraph.CreateLiteralNode("42", new Uri(XmlSpecsHelper.XmlSchemaDataTypeInteger))
                );
            newGraph.Assert(triple);

            connector.SaveGraph(newGraph);

            return newGraph;
        }

        public Graph LoadExampleGraph(string uri)
        {
            Graph loadGraph = new Graph();
            connector.LoadGraph(loadGraph, UriFactory.Create(uri));
            return loadGraph;
        }

        public void UpdateExampleGraph(string uri)
        {
            Graph newGraph = new Graph();
            newGraph.BaseUri = UriFactory.Create(uri);

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
                UriFactory.Create(uri),
                new List<Triple>() { triple2add },
                new List<Triple>() { triple2remove }
                );
        }

        public void QueryExample(string query)
        {
            SparqlResultSet resultSet = (SparqlResultSet)connector.Query(query);
            foreach (SparqlResult result in resultSet.Results)
            {
                Console.WriteLine(result);
            }
        }

        public void QueryWithTimeoutExample(string query, long timeoutMs)
        {
            try
            {
                SparqlResultSet resultSet = (SparqlResultSet)connector.Query(query, timeoutMs);
                foreach (SparqlResult result in resultSet.Results)
                {
                    Console.WriteLine(result);
                }
            }
            catch (RdfQueryException e)
            {
                Console.WriteLine("Query timeout " + e.Message);
            }
        }

        public void DeleteGraphExample(string uri)
        {
            connector.DeleteGraph(UriFactory.Create(uri));
        }
    }
}
