using System;
using UnityEngine;

public class Node
{
    public Node(int index, GameObject gameObject)
    {
        Index = index;
        GameObject = gameObject;
    }

    public int Index
    {
        get;
        set;
    }

    public GameObject GameObject
    {
        get;
        set;
    }
}
