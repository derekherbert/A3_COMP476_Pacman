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
    private Vector3 currentDirection;    
    private Node nodeInFront, north, south, east, west;
    private float rotation = 0.0f;
    private GameObject pelletBeingDestroyed;
    List<Node> path;
    GameObject targetPlayer;
    private int ctr = 100;

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
        //Update every 100 calls to Update()
        if (ctr++ > 100)
        {
            ctr = 0;
            
            //Find closest player
            foreach (GameObject player in GameObject.FindGameObjectsWithTag("Pacman"))
            {
                if (targetPlayer == null || Vector3.Distance(transform.position, player.transform.position) < Vector3.Distance(transform.position, player.transform.position))
                {
                    targetPlayer = player;
                }
            }

            //Find shortest path to closest player
            path = aStar.GetPath(transform.position, targetPlayer.transform.position, Heuristic.EUCLIDIAN);
        }

        if (!isAligning)
        {
            GetComponent<Rigidbody>().MovePosition(transform.position + transform.forward * Time.deltaTime * moveSpeed);

            //Find node in front of player
            RaycastHit hit;
            Vector3 nodePostion = new Vector3(this.gameObject.transform.position.x, 5.0f, this.gameObject.transform.position.z);

            if (!isRotating)
            {
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

                            //Raycast north
                            if (Physics.Raycast(nodeInFront.GameObject.transform.position, new Vector3(0, 0, 1), out hit, 1f))
                            {
                                if (hit.collider.gameObject.tag == "Node")
                                {
                                    index = Convert.ToInt32(hit.collider.gameObject.name.Substring(5));
                                    north = TileGenerator.Graph.Nodes[index];
                                }
                            }
                            //Raycast south
                            if (Physics.Raycast(nodeInFront.GameObject.transform.position, new Vector3(0, 0, -1), out hit, 1f))
                            {
                                if (hit.collider.gameObject.tag == "Node")
                                {
                                    index = Convert.ToInt32(hit.collider.gameObject.name.Substring(5));
                                    south = TileGenerator.Graph.Nodes[index];
                                }
                            }
                            //Raycast east
                            if (Physics.Raycast(nodeInFront.GameObject.transform.position, new Vector3(1, 0, 0), out hit, 1f))
                            {
                                if (hit.collider.gameObject.tag == "Node")
                                {
                                    index = Convert.ToInt32(hit.collider.gameObject.name.Substring(5));
                                    east = TileGenerator.Graph.Nodes[index];
                                }
                            }
                            //Raycast west
                            if (Physics.Raycast(nodeInFront.GameObject.transform.position, new Vector3(-1, 0, 0), out hit, 1f))
                            {
                                if (hit.collider.gameObject.tag == "Node")
                                {
                                    index = Convert.ToInt32(hit.collider.gameObject.name.Substring(5));
                                    west = TileGenerator.Graph.Nodes[index];
                                }
                            }


                            //Wall ahead
                            if (hit.collider.gameObject.tag == "Wall")
                            {
                                //User facing south
                                if (transform.forward.z < -0.98 && transform.forward.z > -1.02)
                                {
                                    //Invalid or no user input, turn automatically (west by default)
                                    if (west != null)
                                    {
                                        rotation = 90;
                                        rotationSpeed = 5;
                                    }
                                    //Turn right automatically if can't turn left
                                    else if (east != null)
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
                                    if (west != null)
                                    {
                                        rotation = -90;
                                        rotationSpeed = 5;
                                    }
                                    //Turn right automatically if can't turn east
                                    else if (east != null)
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
                                    if (north != null)
                                    {
                                        rotation = 90;
                                        rotationSpeed = 5;
                                    }
                                    //Turn right automatically if can't turn left
                                    else if (south != null)
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
                                    if (north != null)
                                    {
                                        rotation = -90;
                                        rotationSpeed = 5;
                                    }
                                    //Turn south automatically if can't turn north
                                    else if (south != null)
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
