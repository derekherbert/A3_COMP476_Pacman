using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GhostManager : Photon.PunBehaviour
{
    #region Public Variables

    public float moveSpeed = 2.0f;
    public float rotationSpeed = 3;

    #endregion

    #region Private Variables

    private bool isRotating = false;
    private bool isAligning = false;
    private bool isRotatingToStartingNode = false;
    private bool isOriented = false;
    private Vector3 currentDirection;    
    private Node nodeInFront, northEast, northWest, southEast, southWest;
    private float rotation = 0.0f;
    private GameObject pelletBeingDestroyed;
    List<Node> path;
    GameObject targetPlayer;

    AStar aStar;    

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        aStar = new AStar();
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("In Update()");

        foreach (GameObject player in GameObject.FindGameObjectsWithTag("Pacman"))
        {
            if (targetPlayer == null || Vector3.Distance(transform.position, player.transform.position) < Vector3.Distance(transform.position, player.transform.position))
            {
                targetPlayer = player;
            }
        }

        //Find shortest path to closest player
        path = aStar.GetPath(transform.position, targetPlayer.transform.position, Heuristic.EUCLIDIAN);

        Debug.Log("AStar Path Length: " + path.Count);
        Debug.Log("path[0] = " + path[0].Index + "\tpath[1] = " + path[1].Index);
                
        //Find node in front of player
        RaycastHit hit;
        Vector3 nodePostion = new Vector3(this.gameObject.transform.position.x, 5.0f, this.gameObject.transform.position.z);

        /*if (Physics.Raycast(nodePostion, transform.forward, out hit, 1f))
        {
            if (hit.collider.gameObject.tag == "Node")
            {
                int index = Convert.ToInt32(hit.collider.gameObject.name.Substring(5));

                Debug.Log("Node In Front of Player = " + index);

                if (index != path[0].Index)
                {
                    Debug.Log("Start rotating player to starting node");

                    //Turn around to face the first node in the path if not already facing it
                    isRotatingToStartingNode = true;
                    rotateToStartingNode(path[0]);
                }
            }
        }*/

        if (!isAligning && !isRotating && !isRotatingToStartingNode)
        {
            Debug.Log("START MOVING THERE EH BUDDY");
            GetComponent<Rigidbody>().MovePosition(transform.position + transform.forward * Time.deltaTime * moveSpeed);

            if (Physics.Raycast(nodePostion, transform.forward, out hit, 1f))
            {
                //Node in front of player
                if (hit.collider.gameObject.tag == "Node")
                {
                    int index = Convert.ToInt32(hit.collider.gameObject.name.Substring(5));


                    Debug.Log("The Raycast Hit Node " + index);


                    bool differentNode = false;
                    float distanceToNode = hit.distance;
                    if (nodeInFront == null || nodeInFront.Index != TileGenerator.Graph.Nodes[index].Index)
                    {
                        nodeInFront = TileGenerator.Graph.Nodes[index];
                        differentNode = true;
                    }

                    if (differentNode || distanceToNode < 0.5f)
                    {
                        Debug.Log("START CASTING RAYZZZZZZ");

                        //Reset everything
                        northEast = null;
                        northWest = null;
                        southEast = null;
                        southWest = null;

                        //Raycast northEast
                        if (Physics.Raycast(nodeInFront.GameObject.transform.position, new Vector3(1, 0, 1), out hit, 1.5f))
                        {
                            if (hit.collider.gameObject.tag == "Node")
                            {
                                index = Convert.ToInt32(hit.collider.gameObject.name.Substring(5));
                                if (index == path[1].Index)
                                {
                                    northEast = path[1];
                                }
                            }
                        }
                        //Raycast northWest
                        if (Physics.Raycast(nodeInFront.GameObject.transform.position, new Vector3(-1, 0, 1), out hit, 1.5f))
                        {
                            if (hit.collider.gameObject.tag == "Node")
                            {
                                index = Convert.ToInt32(hit.collider.gameObject.name.Substring(5));
                                if (index == path[1].Index)
                                {
                                    northWest = path[1];
                                }
                            }
                        }
                        //Raycast southEast
                        if (Physics.Raycast(nodeInFront.GameObject.transform.position, new Vector3(1, 0, -1), out hit, 1.5f))
                        {
                            if (hit.collider.gameObject.tag == "Node")
                            {
                                index = Convert.ToInt32(hit.collider.gameObject.name.Substring(5));
                                if (index == path[1].Index)
                                {
                                    southEast = path[1];
                                }
                            }
                        }
                        //Raycast southWest
                        if (Physics.Raycast(nodeInFront.GameObject.transform.position, new Vector3(-1, 0, -1), out hit, 1.5f))
                        {
                            if (hit.collider.gameObject.tag == "Node")
                            {
                                index = Convert.ToInt32(hit.collider.gameObject.name.Substring(5));
                                if (index == path[1].Index)
                                {
                                    southWest = path[1];
                                }
                            }
                        }

                        if (northEast == null) Debug.Log("northEast: " + northEast); else Debug.Log("northEast: " + northEast.Index);
                        if (northWest == null) Debug.Log("northWest: " + northWest); else Debug.Log("northWest: " + northWest.Index);
                        if (southEast == null) Debug.Log("southEast: " + southEast); else Debug.Log("southEast: " + southEast.Index);
                        if (southWest == null) Debug.Log("southWest: " + southWest); else Debug.Log("southWest: " + southWest.Index);

                        //User facing south
                        if (transform.forward.z < -0.98 && transform.forward.z > -1.02)
                        {
                            Debug.Log("HEADING OUT WEST BABY");

                            //Check if next turn is west
                            if (southWest != null)
                            {
                                rotation = 90;
                                rotationSpeed = 5;
                                currentDirection = transform.forward;
                                isRotating = true;
                            }
                            //Check if next turn is east
                            else if (southEast != null)
                            {
                                rotation = -90;
                                rotationSpeed = 5;
                                currentDirection = transform.forward;
                                isRotating = true;
                            }

                            
                        }
                        //User facing north
                        else if (transform.forward.z > 0.98 && transform.forward.z < 1.02)
                        {
                            //Check if next turn is west
                            if (northWest != null)
                            {
                                rotation = -90;
                                rotationSpeed = 5;
                                currentDirection = transform.forward;
                                isRotating = true;
                            }
                            //Check if next turn is east
                            else if (northEast != null)
                            {
                                rotation = 90;
                                rotationSpeed = 5;
                                currentDirection = transform.forward;
                                isRotating = true;
                            }
                        }
                        //User facing west
                        else if (transform.forward.x < -0.98 && transform.forward.x > -1.02)
                        {
                            //Check if next turn is north
                            if (northWest != null)
                            {
                                rotation = 90;
                                rotationSpeed = 5;
                                currentDirection = transform.forward;
                                isRotating = true;
                            }
                            //Check if next turn is south
                            else if (southWest != null)
                            {
                                rotation = -90;
                                rotationSpeed = 5;
                                currentDirection = transform.forward;
                                isRotating = true;
                            }
                        }
                        //User facing east
                        else if (transform.forward.x > 0.98 && transform.forward.x < 1.02)
                        {
                            //Check if next turn is north
                            if (northEast != null)
                            {
                                rotation = -90;
                                rotationSpeed = 5;
                                currentDirection = transform.forward;
                                isRotating = true;
                            }
                            //Check if next turn is south
                            else if (southEast != null)
                            {
                                rotation = 90;
                                rotationSpeed = 5;
                                currentDirection = transform.forward;
                                isRotating = true;
                            }
                        }
                    }
                }
            }
        }

        if (isAligning)
        {
            alignPlayer();
        }

        if (isRotating)
        {
            if (Mathf.Abs(Vector3.Angle(currentDirection.normalized, transform.forward.normalized)) <= 90)
            {
                rotatePlayer(rotation, rotationSpeed);
            }
            else
            {
                isRotating = false;

                isAligning = true;
                alignPlayer();
            }
        }
    }

    private void rotateToStartingNode(Node node)
    {
        Vector3 targetDir = node.GameObject.transform.position - transform.position;

        Vector3 newDir = Vector3.RotateTowards(transform.forward, targetDir, rotationSpeed * Time.deltaTime, 0.0f);
        //Debug.DrawRay(transform.position, newDir, Color.red);

        // Move our position a step closer to the target.
        transform.rotation = Quaternion.LookRotation(newDir);

        if (Vector3.Angle(node.GameObject.transform.position, transform.forward) < 0.01f)
        {
            isRotatingToStartingNode = false;
            isAligning = true;
            alignPlayer();
        }
    }

    private void rotatePlayer(float rotation, float speed)
    {
        Quaternion deltaRotation = Quaternion.Euler(new Vector3(0, rotation, 0) * Time.deltaTime * speed);
        GetComponent<Rigidbody>().MoveRotation(GetComponent<Rigidbody>().rotation * deltaRotation);
    }

    private void alignPlayer()
    {
        //Align user direction perfectly with x and z axis
        float x = transform.forward.x;
        float z = transform.forward.z;

        if (Mathf.Abs(x) * 2 > 1)
        {
            if (x > 0)
            {
                x = 1.0f;
            }
            else
            {
                x = -1.0f;
            }
        }
        else
        {
            x = 0.0f;
        }
        if (Mathf.Abs(z) * 2 > 1)
        {
            if (z > 0)
            {
                z = 1.0f;
            }
            else
            {
                z = -1.0f;
            }
        }
        else
        {
            z = 0.0f;
        }

        this.transform.forward = new Vector3(x, 0.0f, z);

        x = transform.position.x;
        z = transform.position.z;

        if (x < 0)
        {
            x = (int)x - 0.5f;
        }
        else if (x > 0)
        {
            x = (int)x + 0.5f;
        }
        if (z < 0)
        {
            z = (int)z - 0.5f;
        }
        else if (z > 0)
        {
            z = (int)z + 0.5f;
        }

        float step = 3.0f * Time.deltaTime; // calculate distance to move
        transform.position = Vector3.MoveTowards(transform.position, new Vector3(x, 0.5f, z), step);

        // Check if the position of the cube and sphere are approximately equal.
        if (Vector3.Distance(transform.position, new Vector3(x, 0.5f, z)) < 0.1f)
        {
            isAligning = false;
            GetComponent<Rigidbody>().position = new Vector3(x, 0.5f, z);
            GetComponent<Rigidbody>().MovePosition(transform.position + transform.forward * Time.deltaTime * moveSpeed);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Physics.IgnoreCollision(other, GetComponent<Collider>());
    }
}
