using System;
using System.Collections.Generic;

public class OpenListItem
{
    public OpenListItem(Node node, float costSoFar, List<Node> connectionNodes, float estimatedTotalCost)
    {
        Node = node;
        CostSoFar = costSoFar;
        ConnectionNodes = connectionNodes;
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

    public List<Node> ConnectionNodes
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
