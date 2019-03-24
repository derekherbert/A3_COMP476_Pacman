using System;
using UnityEngine;
using System.Collections.Generic;

public class TileGenerator : MonoBehaviour
{
    int nodeIndex;
    GameObject sampleNode;
    float sampleNodeX;
    float sampleNodeY;
    float sampleNodeZ;
    public static Graph Graph;
    int temp = 0;

    void Start()
    {
        GameObject[] floorObjects = GameObject.FindGameObjectsWithTag("Floor");
        Graph = new Graph(floorObjects.Length, floorObjects.Length * 8);
        nodeIndex = 0;
        sampleNode = GameObject.Find("SampleNode");
        float x = sampleNode.transform.localPosition.x;
        float y = sampleNode.transform.localPosition.y;
        float z = sampleNode.transform.localPosition.z;

        //Add a node to the center of each square of the floor (9 total per floor object)
        foreach (GameObject floor in floorObjects)
        {            
            createNode(floor, new Vector3(x, y, z));        
        }

        //Create connections in Graph
        foreach (Node node in Graph.Nodes)
        {
            //Raycast in 8 directions searching for other nodes
            addConnection(node, new Vector3(1, 0, 1));   //Top-Right Square
            addConnection(node, new Vector3(0, 0, 1));   //Top-Middle Square
            addConnection(node, new Vector3(-1, 0, 1));  //Top-Left Square
            addConnection(node, new Vector3(1, 0, 0));   //Middle-Right Square
            addConnection(node, new Vector3(-1, 0, 0));  //Middle-Left Square
            addConnection(node, new Vector3(1, 0, -1));  //Bottom-Right Square
            addConnection(node, new Vector3(0, 0, -1));  //Bottom-Middle Square
            addConnection(node, new Vector3(-1, 0, -1)); //Bottom-Left Square
        }

        Debug.Log("Connections.Count: " + Graph.Connections.Count);
        Debug.Log("Nodes.Count: " + Graph.Nodes.Count);
        Debug.Log("temp = " + temp);

        //Testing
        //showPath();
    }

    //Testing: Changes color of nodes to show path
    private void showPath()
    {
        AStar aStar = new AStar();
        List<Connection> path = aStar.GetPath(Graph.Nodes[0], Graph.Nodes[10], Heuristic.EUCLIDIAN);
        //List<Connection> path = aStar.GetPath(new Vector3(-6f, 0f, 8f), new Vector3(8f, 0f, 2f), Heuristic.EUCLIDIAN);

        foreach (Connection connection in path)
        {
            connection.ToNode.GameObject.GetComponent<Renderer>().material.color = Color.red;
            connection.FromNode.GameObject.GetComponent<Renderer>().material.color = Color.red;
        }
    }

    //Create Node at desired location, add connections to Graph
    private void createNode(GameObject parent, Vector3 position)
    {
        //Check if this GameObject is valid
        if (isSquareValid(parent.transform.position + position))
        {
            //Create empty GameObject in the scene
            GameObject node = GameObject.Instantiate(sampleNode);
            node.transform.parent = parent.transform;
            node.transform.localPosition = position;
            node.transform.localScale = new Vector3(4f, 4f, 4f);
            node.name = "Node_" + nodeIndex;
                    
            //Add node to Graph
            Graph.Nodes.Add(new Node(nodeIndex, node));
            
            //Update Counter
            nodeIndex++;
        }
    }

    //Determines whether a node should be placed here or if it is blocked somehow
    private bool isSquareValid(Vector3 position)
    {
        //A raycast instersects with a collider up to 1.01 units above node (positioned at center of square)
        if (Physics.Raycast(position, Vector3.up, 1.01f))
        {
            return false;
        }
        
        return true;
    }

    //Sends out a raycast in a specific direction. If another node is hit, a connection is added to the Graph.
    private void addConnection(Node node, Vector3 direction)
    {
        RaycastHit hit;

        if (Physics.Raycast(node.GameObject.transform.position, direction.normalized, out hit, 1.5f))
        {
            Debug.Log(hit.collider.name);
            if (hit.collider.gameObject.name.Contains("Node"))
            {
                temp++;
                int index = Convert.ToInt32(hit.collider.gameObject.name.Substring(5));
                Graph.Connections.Add(new Connection(node, Graph.Nodes[index], hit.distance));
            }
        }
    }
}