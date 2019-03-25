using System;
using System.Collections.Generic;
using UnityEngine;

public class AStar
{
    private List<ClosedListItem> closedList;
    private List<OpenListItem> openList;

    public AStar()
	{
        closedList = new List<ClosedListItem>();
        openList = new List<OpenListItem>(); //Ordered by increasing estimatedTotalCost
    }

    //TO DO
    public List<Node> GetPath(Vector3 startPosition, Vector3 endPosition, Heuristic heuristic)
    {
        return GetPath(findClosestNode(startPosition), findClosestNode(endPosition), heuristic);
    }

    public List<Node> GetPath(Node startNode, Node endNode, Heuristic heuristic)
    {
        //Create the first entry in the openList
        openList.Add(new OpenListItem(startNode, 0.0f, new List<Node>(), getEstimatedCostSoFar(startNode, endNode, heuristic)));
        OpenListItem currentItem, childItem;
        List<Node> childItemConnections;
        bool addToClosedList = true;
        bool childItemUpdated = false;

        //Keep running until terminateSearch() returns true
        while (!terminateSearch(endNode))
        {
            //Grab the first element in the openList
            currentItem = openList[0];
            openList.RemoveAt(0);

            //Add all children to the openList ordered by increasing estimatedTotalCost
            foreach (Connection childConnection in TileGenerator.Graph.getConnectedNodes(currentItem.Node))
            {
                if (!currentItem.ConnectionNodes.Contains(childConnection.ToNode))
                { 
                    //Add the parent's connection chain to the child
                    childItemConnections = new List<Node>(currentItem.ConnectionNodes);

                    //Add the connection from the parent to the child
                    childItemConnections.Add(childConnection.ToNode);

                    //Create an OpenListItem for the child
                    childItem = new OpenListItem(childConnection.ToNode, currentItem.CostSoFar + childConnection.Cost, childItemConnections, getEstimatedCostSoFar(childConnection.ToNode, endNode, heuristic));

                    //Check if the childItem's node already exists in the openList
                    for (int i = 0; i < openList.Count; i++)
                    {
                        //Check if both items have the same nodes 
                        if (childItem.Node.Index == openList[i].Node.Index)
                        {
                            // Keep the more optimal path
                            if (childItem.CostSoFar < openList[i].CostSoFar)
                            {
                                openList[i] = childItem;
                                childItemUpdated = true;
                                break;
                            }
                        }
                    }

                    if (!childItemUpdated)
                    {
                        insertIntoOpenList(openList, childItem);
                    }

                    childItemUpdated = false; //Reset flag
                }
            }

            //Check if it exists on the closed list 
            for (int i = 0; i < closedList.Count; i++)
            {
                //Check if both items have the same nodes 
                if (currentItem.Node.Index == closedList[i].Node.Index)
                {
                    // Keep the more optimal path
                    if (currentItem.CostSoFar < closedList[i].CostSoFar)
                    {                                                
                        closedList[i] = new ClosedListItem(currentItem.Node, currentItem.CostSoFar, currentItem.ConnectionNodes);
                        addToClosedList = false;
                        break;
                    }
                }
            }

            if (addToClosedList)
            {
                //Add currentItem to the closedList
                closedList.Add(new ClosedListItem(currentItem.Node, currentItem.CostSoFar, currentItem.ConnectionNodes));
            }

            addToClosedList = true; //Reset flag
        }

        //Return the goalItem's connection list on the closedList
        foreach (ClosedListItem item in closedList)
        {
            if (item.Node.Index == endNode.Index)
            {
                return item.ConnectionNodes;
            }
        }

        return null;
    }

    private void insertIntoOpenList(List<OpenListItem> openList, OpenListItem childItem)
    {
        //Add the childItem to the openList ordered by increasing estimatedTotalCost
        for (int i = 0; i < openList.Count; i++)
        {
            //Different starting and ending points
            if (childItem.EstimatedTotalCost < openList[i].EstimatedTotalCost)
            {
                openList.Insert(i, childItem);
                return;
            }
        }

        //If the childItem has the highest estimatedTotalCost, add it at the end
        openList.Add(childItem);
    }
    
    private float getEstimatedCostSoFar(Node currentNode, Node endNode, Heuristic heuristic)
    {
        if (heuristic == Heuristic.EUCLIDIAN)
        {
            return Vector3.Distance(currentNode.GameObject.transform.position, endNode.GameObject.transform.position);
        }

        return 1f;
    }

    //Check if the search algorithm is ready to terminate. 
    private bool terminateSearch(Node endNode)
    {
        Console.WriteLine("openList.Count = " + openList.Count);

        if (openList.Count != 0)
        {
            ClosedListItem goal = null;

            //Find the goal node in the closedList
            foreach (ClosedListItem item in closedList)
            {
                if (item.Node.Index == endNode.Index)
                {
                    goal = item;
                }
            }
            
            //If the smallest costSoFar element in the openList is greater than the goal item in the closedList
            if (goal != null && openList[0].CostSoFar > goal.CostSoFar)
            {
                return true;
            }

            return false;
        }

        return true;    
    }

    private Node findClosestNode(Vector3 position)
    {
        List<Node> nodes = TileGenerator.Graph.Nodes;
        Node closestNode = nodes[0];
        float closestNodeDistance = Vector3.Distance(closestNode.GameObject.transform.position, position);
        float temp;

        for (int i = 1; i < nodes.Count; i++)
        {
            temp = Vector3.Distance(nodes[i].GameObject.transform.position, position);
            if (temp < closestNodeDistance)
            {
                closestNodeDistance = temp;
                closestNode = nodes[i];
            }
        }

        return closestNode;
    }
}