using System;

public class Connection
{
    public Connection(Node toNode, Node fromNode, float cost)
	{
        ToNode = toNode;
        FromNode = fromNode;
        Cost = cost;        
    }

    public Node ToNode
    {
        get;
        set;
    }

    public Node FromNode
    {
        get;
        set;
    }

    public float Cost
    {
        get;
        set;
    }
}
