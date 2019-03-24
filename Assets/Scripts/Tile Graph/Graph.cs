using System;
using System.Collections.Generic;

public class Graph
{
    public Graph(int nodesCount, int connectionsCount)
    {
        Nodes = new List<Node>(nodesCount);
        Connections = new List<Connection>(connectionsCount);
    }

    public List<Connection> Connections
    {
        get;
    }

    public List<Node> Nodes
    {
        get;
    }

    public List<Connection> getConnectedNodes(Node fromNode)
    {
        List<Connection> connectedNodes = new List<Connection>(8);

        for (int i = 0; i < Connections.Count; i++)
        {
            if(Connections[i].FromNode.Index == fromNode.Index)
            {
                connectedNodes.Add(Connections[i]);
            }
        }

        return connectedNodes;
    }
}