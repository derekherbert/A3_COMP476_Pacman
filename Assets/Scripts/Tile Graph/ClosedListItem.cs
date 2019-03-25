using System;
using System.Collections.Generic;

public class ClosedListItem
{
	public ClosedListItem(Node node, float costSoFar, List<Node> connectionNodes)
	{
        Node = node;
        CostSoFar = costSoFar;
        ConnectionNodes = connectionNodes;
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
}
