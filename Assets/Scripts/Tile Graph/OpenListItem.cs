using System;
using System.Collections.Generic;

public class OpenListItem
{
    public OpenListItem(Node node, float costSoFar, List<Connection> connections, float estimatedTotalCost)
    {
        Node = node;
        CostSoFar = costSoFar;
        Connections = connections;
        EstimatedTotalCost = estimatedTotalCost;
    }

    public Node Node
    {
        get;
        set;
    }

    public float CostSoFar
    {
        get;
        set;
    }

    public List<Connection> Connections
    {
        get;
        set;
    }

    public float EstimatedTotalCost
    {
        get;
        set;
    }
}
