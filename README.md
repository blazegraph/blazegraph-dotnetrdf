# dotNetRDF-blazegraph
Blazegraph connector using dotNetRDF

Usage:
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
