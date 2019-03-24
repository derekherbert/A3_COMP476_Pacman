using System;
using System.Collections.Generic;

public class ClosedListItem
{
	public ClosedListItem(Node node, float costSoFar, List<Connection> connections)
	{
        Node = node;
        CostSoFar = costSoFar;
        Connections = connections;
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
}
