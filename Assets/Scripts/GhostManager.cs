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
    public float rotationSpeed = 2f;

    #endregion

    #region Private Variables

    private bool isRotating = false;
    private bool isAligning = false;
    private bool isRotatingToStartingNode = false;
    private bool isOriented = false;
    private Vector3 currentDirection;    
    private Node nodeInFront, north, south, east, west;
    private bool noWallNorth = false;
    private bool noWallSouth = false;
    private bool noWallEast = false;
    private bool noWallWest = false;
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
        foreach (GameObject player in GameObject.FindGameObjectsWithTag("Pacman"))
        {
            if (targetPlayer == null || Vector3.Distance(transform.position, player.transform.position) < Vector3.Distance(transform.position, player.transform.position))
            {
                targetPlayer = player;
            }
        }

        //Find shortest path to closest player
        path = aStar.GetPath(transform.position, targetPlayer.transform.position, Heuristic.EUCLIDIAN);
                
        //Find node in front of player
        RaycastHit hit;
        Vector3 nodePostion = new Vector3(this.gameObject.transform.position.x, 5.0f, this.gameObject.transform.position.z);

        GetComponent<Rigidbody>().MovePosition(transform.position + transform.forward * Time.deltaTime * moveSpeed);

        if (!isAligning && !isRotating && !isRotatingToStartingNode)
        {
            GetComponent<Rigidbody>().MovePosition(transform.position + transform.forward * Time.deltaTime * moveSpeed);

            if (Physics.Raycast(nodePostion, transform.forward, out hit, 1f))
            {
                //Node in front of player
                if (hit.collider.gameObject.tag == "Node")
                {
                    int index = Convert.ToInt32(hit.collider.gameObject.name.Substring(5));
                    bool differentNode = false;
                    float distanceToNode = hit.distance;
                    if (nodeInFront == null || nodeInFront.Index != TileGenerator.Graph.Nodes[index].Index)
                    {
                        nodeInFront = TileGenerator.Graph.Nodes[index];
                        differentNode = true;
                    }

                    if (differentNode || distanceToNode < 0.5f)
                    {
                        //Reset everything
                        north = null;
                        south = null;
                        east = null;
                        west = null;
                        noWallNorth = false;
                        noWallSouth = false;
                        noWallEast = false;
                        noWallWest = false;

                        //Raycast north
                        if (Physics.Raycast(nodeInFront.GameObject.transform.position, new Vector3(0, 0, 1), out hit, 1.5f))
                        {
                            if (hit.collider.gameObject.tag == "Node")
                            {
                                index = Convert.ToInt32(hit.collider.gameObject.name.Substring(5));

                                if (index == path[0].Index)
                                {
                                    north = path[0];
                                }

                                noWallNorth = true;
                            }
                        }
                        //Raycast south
                        if (Physics.Raycast(nodeInFront.GameObject.transform.position, new Vector3(0, 0, -1), out hit, 1.5f))
                        {
                            if (hit.collider.gameObject.tag == "Node")
                            {
                                index = Convert.ToInt32(hit.collider.gameObject.name.Substring(5));

                                if (index == path[0].Index)
                                {
                                    south = path[0];
                                }

                                noWallSouth = true;
                            }
                        }
                        //Raycast east
                        if (Physics.Raycast(nodeInFront.GameObject.transform.position, new Vector3(1, 0, 0), out hit, 1.5f))
                        {
                            if (hit.collider.gameObject.tag == "Node")
                            {
                                index = Convert.ToInt32(hit.collider.gameObject.name.Substring(5));

                                if (index == path[0].Index)
                                {
                                    east = path[0];
                                }

                                noWallEast = true;
                            }
                        }
                        //Raycast west
                        if (Physics.Raycast(nodeInFront.GameObject.transform.position, new Vector3(-1, 0, 0), out hit, 1.5f))
                        {
                            if (hit.collider.gameObject.tag == "Node")
                            {
                                index = Convert.ToInt32(hit.collider.gameObject.name.Substring(5));

                                if (index == path[0].Index)
                                {
                                    west = path[0];
                                }

                                noWallWest = true;
                            }
                        }

                        //User facing south
                        if (transform.forward.z < -0.98 && transform.forward.z > -1.02)
                        {
                            //Check if next turn is west
                            if (west != null)
                            {
                                rotation = 90;
                                rotationSpeed = 1;
                                currentDirection = transform.forward;
                                isRotating = true;
                                Debug.Log("HEADING SOUTH, TURN WEST");
                            }
                            //Check if next turn is east
                            else if (east != null)
                            {
                                rotation = -90;
                                rotationSpeed = 1;
                                currentDirection = transform.forward;
                                isRotating = true;
                                Debug.Log("HEADING SOUTH, TURN EAST");
                            }                            
                        }
                        //User facing north
                        else if (transform.forward.z > 0.98 && transform.forward.z < 1.02)
                        {
                            //Check if next turn is west
                            if (west != null)
                            {
                                rotation = -90;
                                rotationSpeed = 1;
                                currentDirection = transform.forward;
                                isRotating = true;
                                Debug.Log("HEADING NORTH, TURN WEST");
                            }
                            //Check if next turn is east
                            else if (east != null)
                            {
                                rotation = 90;
                                rotationSpeed = 1;
                                currentDirection = transform.forward;
                                isRotating = true;
                                Debug.Log("HEADING NORTH, TURN EAST");
                            }
                        }
                        //User facing west
                        else if (transform.forward.x < -0.98 && transform.forward.x > -1.02)
                        {
                            //Check if next turn is north
                            if (north != null)
                            {
                                rotation = 90;
                                rotationSpeed = 1;
                                currentDirection = transform.forward;
                                isRotating = true;
                                Debug.Log("HEADING WEST, TURN NORTH");
                            }
                            //Check if next turn is south
                            else if (south != null)
                            {
                                rotation = -90;
                                rotationSpeed = 1;
                                currentDirection = transform.forward;
                                isRotating = true;
                                Debug.Log("HEADING WEST, TURN SOUTH");
                            }
                        }
                        //User facing east
                        else if (transform.forward.x > 0.98 && transform.forward.x < 1.02)
                        {
                            //Check if next turn is north
                            if (north != null)
                            {
                                rotation = -90;
                                rotationSpeed = 1;
                                currentDirection = transform.forward;
                                isRotating = true;
                                Debug.Log("HEADING EAST, TURN NORTH");
                            }
                            //Check if next turn is south
                            else if (south != null)
                            {
                                rotation = 90;
                                rotationSpeed = 1;
                                currentDirection = transform.forward;
                                isRotating = true;
                                Debug.Log("HEADING EAST, TURN SOUTH");
                            }
                        }
                    }
                }
                //Wall ahead
                else if (hit.collider.gameObject.tag == "Wall")
                {
                    //User facing south
                    if (transform.forward.z < -0.98 && transform.forward.z > -1.02)
                    {
                        //Turn automatically (west by default)
                        if (noWallWest)
                        {
                            rotation = 90;
                            rotationSpeed = 5;
                        }
                        //Turn right automatically if can't turn left
                        else if (noWallEast)
                        {
                            rotation = -90;
                            rotationSpeed = 5;
                        }

                        currentDirection = transform.forward;
                        isRotating = true;
                    }
                    //User facing north
                    else if (transform.forward.z > 0.98 && transform.forward.z < 1.02)
                    {
                        Debug.Log("ABOUT TO HIT A WALL FACING NORTH");

                        //Turn automatically (west by default)
                        if (noWallWest)
                        {
                            rotation = -90;
                            rotationSpeed = 5;
                        }
                        //Turn right automatically if can't turn east
                        else if (noWallEast)
                        {
                            rotation = 90;
                            rotationSpeed = 5;
                        }

                        currentDirection = transform.forward;
                        isRotating = true;
                    }
                    //User facing west
                    else if (transform.forward.x < -0.98 && transform.forward.x > -1.02)
                    {                        
                        //Turn automatically (left by default)
                        if (noWallNorth)
                        {
                            rotation = 90;
                            rotationSpeed = 5;
                        }
                        //Turn right automatically if can't turn left
                        else if (noWallSouth)
                        {
                            rotation = -90;
                            rotationSpeed = 5;
                        }

                        currentDirection = transform.forward;
                        isRotating = true;
                    }
                    //User facing east
                    else if (transform.forward.x > 0.98 && transform.forward.x < 1.02)
                    {                        
                        //Turn automatically (north by default)
                        if (noWallNorth)
                        {
                            rotation = -90;
                            rotationSpeed = 5;
                        }
                        //Turn south automatically if can't turn north
                        else if (noWallSouth)
                        {
                            rotation = 90;
                            rotationSpeed = 5;
                        }

                        currentDirection = transform.forward;
                        isRotating = true;
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

        float step = 2.0f * Time.deltaTime; // calculate distance to move
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
        if (other.gameObject.tag != "Pacman")
        {
            Physics.IgnoreCollision(other, GetComponent<Collider>());
        }            
    }       
}
