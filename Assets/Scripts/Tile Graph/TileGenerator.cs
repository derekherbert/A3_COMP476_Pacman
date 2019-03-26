using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

public class TileGenerator : Photon.PunBehaviour
{
    GameObject sampleNode;
    float sampleNodeX;
    float sampleNodeY;
    float sampleNodeZ;
    public static Graph Graph;
    bool runTest = true;

    void Start()
    {        
        GameObject[] floorObjects = GameObject.FindGameObjectsWithTag("Floor");
        Graph = new Graph(floorObjects.Length, floorObjects.Length * 8);
        sampleNode = GameObject.Find("SampleNode");
        
        //Add a node to the center of each floor square
        foreach (GameObject floor in floorObjects)
        {
            //Create empty GameObject in the scene
            GameObject node = GameObject.Instantiate(sampleNode);
            node.transform.position = new Vector3(floor.transform.position.x, 5f, floor.transform.position.z);
            node.name = "Node_" + Graph.Nodes.Count;
            node.GetComponent<SphereCollider>().enabled = true;

            //Add node to Graph
            Graph.Nodes.Add(new Node(Graph.Nodes.Count, node));
        }                
    }

    void Update()
    {
        if (Graph.Connections.Count == 0)
        {
            //Create connections in Graph
            foreach (Node node in Graph.Nodes)
            {
                //Raycast in 8 directions searching for other nodes
                addConnection(node, new Vector3(1f, 0, 1f));   //Top-Right Square
                addConnection(node, new Vector3(0f, 0f, 1f));   //Top-Middle Square
                addConnection(node, new Vector3(-1f, 0f, 1f));  //Top-Left Square
                addConnection(node, new Vector3(1f, 0f, 0f));   //Middle-Right Square
                addConnection(node, new Vector3(-1f, 0f, 0f));  //Middle-Left Square
                addConnection(node, new Vector3(1f, 0f, -1f));  //Bottom-Right Square
                addConnection(node, new Vector3(0f, 0f, -1f));  //Bottom-Middle Square
                addConnection(node, new Vector3(-1f, 0f, -1f)); //Bottom-Left Square
            }

            Debug.Log("Connections.Count: " + Graph.Connections.Count);
            Debug.Log("Nodes.Count: " + Graph.Nodes.Count);                       
        }

        //Testing
        if (Graph.Connections.Count == 928 && runTest)
        {
            //showPath();
            runTest = false;
        }
    }

    //Testing: Changes color of nodes to show path
    private void showPath()
    {
        AStar aStar = new AStar();
        //List<Node> path = aStar.GetPath(Graph.Nodes[0], Graph.Nodes[215], Heuristic.EUCLIDIAN);
        List<Node> path = aStar.GetPath(new Vector3(-8f, 1f, 10f), new Vector3(8.2f, 0f, -4.3f), Heuristic.EUCLIDIAN);
        
        foreach (Node node in path)
        {
            node.GameObject.GetComponent<Renderer>().material.color = Color.red;
            node.GameObject.GetComponent<Renderer>().material.color = Color.red;
        }
    }          

    //Sends out a raycast in a specific direction. If another node is hit, a connection is added to the Graph.
    private void addConnection(Node node, Vector3 direction)
    {
        RaycastHit hit;

        Debug.DrawRay(node.GameObject.transform.position, direction.normalized, Color.white, 5000f);

        if (Physics.Raycast(node.GameObject.transform.position, direction.normalized, out hit, 1.2f))
        {
            if (hit.collider.gameObject.tag == "Node")
            {
                int index = Convert.ToInt32(hit.collider.gameObject.name.Substring(5));
                Graph.Connections.Add(new Connection(node, Graph.Nodes[index], hit.distance));
            }
        }
    }    
}