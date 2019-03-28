using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Assets.Scripts
{
    public class PlayerManager : Photon.PunBehaviour, IPunObservable
    {
        #region Public Variables

        [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
        public static PhotonView LocalPlayerInstance;
        
        public Vector3 startingPosition;
        public static string playerName;
        public float moveSpeed = 2.0f;
        public float rotationSpeed = 3;
        public bool recentlyCollided = false;
        public int recentlyCollidedCtr = 0;

        #endregion

        #region Private Variables

        private bool isRotating = false;
        private bool isAligning = false;
        private Vector3 currentDirection;
        private KeyCode lastKeyPressed;
        private Node nodeInFront, north, south, east, west;
        private float rotation = 0.0f;
        private GameObject pelletBeingDestroyed;
        private int playerNumber;
        private Color playerColor, syncColor;
        private Vector3 tempColor;

        #endregion

        #region MonoBehaviour CallBacks

        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity during early initialization phase.
        /// </summary>
        void Awake()
        {
            // #Important
            // used in GameManager.cs: we keep track of the localPlayer instance to prevent instantiation when levels are synchronized
            if (photonView.isMine)
            {
                PlayerManager.LocalPlayerInstance = this.photonView;
            }
            // #Critical
            // we flag as don't destroy on load so that instance survives level synchronization, thus giving a seamless experience when levels load.
            DontDestroyOnLoad(this.gameObject);
        }

        void Start()
        {
            LocalPlayerInstance.GetComponent<SphereCollider>().isTrigger = true;
        }

        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity on every frame.
        /// </summary>
        void Update()
        {            
            if (photonView.isMine)
            {
                //Check preemptively for collision with another entity
                RaycastHit hit;
                if (Physics.Raycast(new Vector3(transform.position.x, 0.85f, transform.position.z), transform.forward, out hit, 1.7f))
                {
                    if (hit.collider.tag == "Pacman")
                    {
                        recentlyCollided = true;
                        recentlyCollidedCtr = 0;

                        Debug.Log("ABOUT TO COLLIDE WITH ANOTHER PACMAN");
                        Debug.Log("Current Rotation: " + GetComponent<Rigidbody>().rotation.eulerAngles.y);

                        Quaternion rotation = Quaternion.Euler(new Vector3(0, GetComponent<Rigidbody>().rotation.eulerAngles.y - 180f, 0));
                        GetComponent<Rigidbody>().rotation = rotation;

                        Debug.Log("New Rotation: " + GetComponent<Rigidbody>().rotation.eulerAngles.y);

                        GetComponent<Rigidbody>().MovePosition(transform.position + transform.forward * Time.deltaTime * 5);
                    }
                }

                ProcessInputs();                        
            }
        }

        public static void NewPlayerSpawn(string playerName, int actorID, int playerViewID)
        {
            LocalPlayerInstance.RPC("NewPlayerSpawnRPC", PhotonTargets.All, playerName, actorID, playerViewID);
        }            

        [PunRPC]
        void NewPlayerSpawnRPC(string playerName, int playerActorID, int playerViewID, PhotonMessageInfo info)
        {            
            PhotonView playerView = PhotonView.Find(playerViewID);
                
            //Set player's GameObject name
            playerView.name = "Pacman_" + playerName;

            //Set player number
            playerNumber = PhotonNetwork.playerList.Length - 1;

            //Set player's color
            playerView.GetComponent<Renderer>().material.color = GameManager.colors[playerNumber];
            this.playerColor = GameManager.colors[playerNumber];

            PhotonPlayer thisPlayer = null;
            foreach (PhotonPlayer player in PhotonNetwork.playerList)
            {
                if (player.NickName == playerName)
                {
                    thisPlayer = player;
                }
            }

            GameManager.playerInfoList[playerNumber] = new PlayerInfo(playerName, thisPlayer.ID, playerView.viewID, GameManager.playerScoreTextBoxPhotonIDs[playerNumber], GameManager.colors[playerNumber]);
        }

        [PunRPC]
        void PelletEaten(int pelletViewID, PhotonMessageInfo info)
        {
            if (PhotonNetwork.isMasterClient)
            {
                Debug.Log("IN PELLET EATENNNNNNNNN: " + PhotonView.Find(pelletViewID).gameObject.name);

                PhotonNetwork.Destroy(PhotonView.Find(pelletViewID));
            }            
        }

        /// <summary>
        /// MonoBehaviour method called when the Collider 'other' enters the trigger.
        /// If it is another player, both should bounce back
        /// </summary>
        void OnTriggerEnter(Collider other)
        {           
            //Only care about local player
            if (!photonView.isMine)
            {
                return;
            }
                        
            //Pacman collision: Both players bounce back and rotate towards where they were coming from
            if (other.gameObject.tag == "Pacman")
            {
                recentlyCollided = true;
                recentlyCollidedCtr = 0;

                Debug.Log("COLLIDING WITH ANOTHER PACMAN");
                Debug.Log("Current Rotation: " + GetComponent<Rigidbody>().rotation.eulerAngles.y);

                Quaternion rotation = Quaternion.Euler(new Vector3(0, - GetComponent<Rigidbody>().rotation.eulerAngles.y, 0));
                GetComponent<Rigidbody>().rotation = rotation;

                Debug.Log("New Rotation: " + GetComponent<Rigidbody>().rotation.eulerAngles.y);
                                

                GetComponent<Rigidbody>().MovePosition(transform.position + transform.forward  * Time.deltaTime * 4);

                isAligning = true;
                alignPlayer();
            }

            //Ghost collision: Player is kicked from the game. 
            else if (other.gameObject.tag == "Ghost")
            {
                GameManager.Instance.LeaveRoom();
            }

            //Pellet collision: Pellet removed from map, player score increases
            else if (other.gameObject.tag == "Pellet")
            {
                if (pelletBeingDestroyed == null || pelletBeingDestroyed != other.gameObject)
                {
                    photonView.RPC("PelletEaten", PhotonTargets.MasterClient, other.GetComponent<PhotonView>().viewID);

                    PhotonNetwork.player.AddScore(1);

                    Debug.Log("Local player score: " + PhotonNetwork.player.GetScore());

                    pelletBeingDestroyed = other.gameObject;

                    //Update score on UI
                    PhotonView.Find(GameManager.playerScoreTextBoxPhotonIDs[playerNumber]).GetComponent<Text>().text = PhotonNetwork.player.NickName + ": " + PhotonNetwork.player.GetScore();

                }
            }
        }

        #endregion

        #region Custom

        /// <summary>
        /// Processes the inputs. Maintain a flag representing when the user is pressing Fire.
        /// </summary>
        void ProcessInputs()
        {
            if (!isAligning)
            {
                if (recentlyCollided && recentlyCollidedCtr < 15)
                {
                    GetComponent<Rigidbody>().MovePosition(transform.position + transform.forward * Time.deltaTime * 4);
                    recentlyCollidedCtr++;                    
                }
                else
                {
                    GetComponent<Rigidbody>().MovePosition(transform.position + transform.forward * Time.deltaTime * moveSpeed);
                    recentlyCollidedCtr = 0;
                    recentlyCollided = false;
                }

                //Set last key pressed
                if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
                {
                    lastKeyPressed = KeyCode.W;
                }
                else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
                {
                    lastKeyPressed = KeyCode.S;
                }
                else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    lastKeyPressed = KeyCode.A;
                }
                else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
                {
                    lastKeyPressed = KeyCode.D;
                }

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

                                //User facing south
                                if (transform.forward.z < -0.98 && transform.forward.z > -1.02)
                                {
                                    //User tries to move west, turn west if they can
                                    if (lastKeyPressed == KeyCode.A && west != null && distanceToNode < 0.5f)
                                    {
                                        rotation = 90;
                                        rotationSpeed = 1.5f;
                                        lastKeyPressed = KeyCode.Space; //Assign random meaningless key
                                        currentDirection = transform.forward;
                                        isRotating = true;
                                    }
                                    //User tries to move east, turn east if they can
                                    else if (lastKeyPressed == KeyCode.D && east != null && distanceToNode < 0.5f)
                                    {
                                        rotation = -90;
                                        rotationSpeed = 1.5f;
                                        lastKeyPressed = KeyCode.Space; //Assign random meaningless key
                                        currentDirection = transform.forward;
                                        isRotating = true;
                                    }
                                }
                                //User facing north
                                else if (transform.forward.z > 0.98 && transform.forward.z < 1.02)
                                {
                                    //User tries to move west, turn west if they can
                                    if (lastKeyPressed == KeyCode.A && west != null && distanceToNode < 0.5f)
                                    {
                                        rotation = -90;
                                        rotationSpeed = 1.5f;
                                        lastKeyPressed = KeyCode.Space; //Assign random meaningless key
                                        currentDirection = transform.forward;
                                        isRotating = true;
                                    }
                                    //User tries to move east, turn east if they can
                                    else if (lastKeyPressed == KeyCode.D && east != null && distanceToNode < 0.5f)
                                    {
                                        rotation = 90;
                                        rotationSpeed = 1.5f;
                                        lastKeyPressed = KeyCode.Space; //Assign random meaningless key
                                        currentDirection = transform.forward;
                                        isRotating = true;
                                    }
                                }
                                //User facing west
                                else if (transform.forward.x < -0.98 && transform.forward.x > -1.02)
                                {
                                    //User tries to move north, turn north if they can
                                    if (lastKeyPressed == KeyCode.W && north != null && distanceToNode < 0.5f)
                                    {
                                        rotation = 90;
                                        rotationSpeed = 1.5f;
                                        lastKeyPressed = KeyCode.Space; //Assign random meaningless key
                                        currentDirection = transform.forward;
                                        isRotating = true;
                                    }
                                    //User tries to move right, turn right if they can
                                    else if (lastKeyPressed == KeyCode.S && south != null && distanceToNode < 0.5f)
                                    {
                                        rotation = -90;
                                        rotationSpeed = 1.5f;
                                        lastKeyPressed = KeyCode.Space; //Assign random meaningless key
                                        currentDirection = transform.forward;
                                        isRotating = true;
                                    }
                                }
                                //User facing east
                                else if (transform.forward.x > 0.98 && transform.forward.x < 1.02)
                                {
                                    //User tries to move north, turn north if they can
                                    if (lastKeyPressed == KeyCode.W && north != null && distanceToNode < 0.5f)
                                    {
                                        rotation = -90;
                                        rotationSpeed = 1.5f;
                                        lastKeyPressed = KeyCode.Space; //Assign random meaningless key
                                        currentDirection = transform.forward;
                                        isRotating = true;
                                    }
                                    //User tries to move south, turn south if they can
                                    else if (lastKeyPressed == KeyCode.S && south != null && distanceToNode < 0.5f)
                                    {
                                        rotation = 90;
                                        rotationSpeed = 1.5f;
                                        lastKeyPressed = KeyCode.Space; //Assign random meaningless key
                                        currentDirection = transform.forward;
                                        isRotating = true;
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
                                //User tries to move west, turn west if they can
                                if (lastKeyPressed == KeyCode.A && west != null)
                                {
                                    rotation = 90;
                                    rotationSpeed = 5;
                                    lastKeyPressed = KeyCode.Space; //Assign random meaningless key
                                }
                                //User tries to move east, turn east if they can
                                else if (lastKeyPressed == KeyCode.D && east != null)
                                {
                                    rotation = -90;
                                    rotationSpeed = 5;
                                    lastKeyPressed = KeyCode.Space; //Assign random meaningless key
                                }
                                //Invalid or no user input, turn automatically (west by default)
                                else if (west != null)
                                {
                                    rotation = 90;
                                    rotationSpeed = 5;
                                    lastKeyPressed = KeyCode.Space; //Assign random meaningless key
                                }
                                //Turn right automatically if can't turn left
                                else if (east != null)
                                {
                                    rotation = -90;
                                    rotationSpeed = 5;
                                    lastKeyPressed = KeyCode.Space; //Assign random meaningless key
                                }

                                currentDirection = transform.forward;
                                isRotating = true;
                            }
                            //User facing north
                            else if (transform.forward.z > 0.98 && transform.forward.z < 1.02)
                            {
                                //User tries to move west, turn west if they can
                                if (lastKeyPressed == KeyCode.A && west != null)
                                {
                                    rotation = -90;
                                    rotationSpeed = 5;
                                    lastKeyPressed = KeyCode.Space; //Assign random meaningless key
                                }
                                //User tries to move east, turn east if they can
                                else if (lastKeyPressed == KeyCode.D && east != null)
                                {
                                    rotation = -0;
                                    rotationSpeed = 5;
                                    lastKeyPressed = KeyCode.Space; //Assign random meaningless key
                                }
                                //Invalid or no user input, turn automatically (west by default)
                                else if (west != null)
                                {
                                    rotation = -90;
                                    rotationSpeed = 5;
                                    lastKeyPressed = KeyCode.Space; //Assign random meaningless key
                                }
                                //Turn right automatically if can't turn east
                                else if (east != null)
                                {
                                    rotation = 90;
                                    rotationSpeed = 5;
                                    lastKeyPressed = KeyCode.Space; //Assign random meaningless key
                                }

                                currentDirection = transform.forward;
                                isRotating = true;
                            }
                            //User facing west
                            else if (transform.forward.x < -0.98 && transform.forward.x > -1.02)
                            {
                                //User tries to move north, turn north if they can
                                if (lastKeyPressed == KeyCode.W && north != null)
                                {
                                    rotation = 90;
                                    rotationSpeed = 5;
                                    lastKeyPressed = KeyCode.Space; //Assign random meaningless key
                                }
                                //User tries to move right, turn right if they can
                                else if (lastKeyPressed == KeyCode.S && south != null)
                                {
                                    rotation = -90;
                                    rotationSpeed = 5;
                                    lastKeyPressed = KeyCode.Space; //Assign random meaningless key
                                }
                                //Invalid or no user input, turn automatically (left by default)
                                else if (north != null)
                                {
                                    rotation = 90;
                                    rotationSpeed = 5;
                                    lastKeyPressed = KeyCode.Space; //Assign random meaningless key
                                }
                                //Turn right automatically if can't turn left
                                else if (south != null)
                                {
                                    rotation = -90;
                                    rotationSpeed = 5;
                                    lastKeyPressed = KeyCode.Space; //Assign random meaningless key
                                }

                                currentDirection = transform.forward;
                                isRotating = true;
                            }
                            //User facing east
                            else if (transform.forward.x > 0.98 && transform.forward.x < 1.02)
                            {
                                //User tries to move north, turn north if they can
                                if (lastKeyPressed == KeyCode.W && north != null)
                                {
                                    rotation = -90;
                                    rotationSpeed = 5;
                                    lastKeyPressed = KeyCode.Space; //Assign random meaningless key
                                }
                                //User tries to move south, turn south if they can
                                else if (lastKeyPressed == KeyCode.S && south != null)
                                {
                                    rotation = 90;
                                    rotationSpeed = 5;
                                    lastKeyPressed = KeyCode.Space; //Assign random meaningless key
                                }
                                //Invalid or no user input, turn automatically (north by default)
                                else if (north != null)
                                {
                                    rotation = -90;
                                    rotationSpeed = 5;
                                    lastKeyPressed = KeyCode.Space; //Assign random meaningless key
                                }
                                //Turn south automatically if can't turn north
                                else if (south != null)
                                {
                                    rotation = 90;
                                    rotationSpeed = 5;
                                    lastKeyPressed = KeyCode.Space; //Assign random meaningless key
                                }

                                currentDirection = transform.forward;
                                isRotating = true;
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

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.isWriting)
            {
                //send color
                //tempColor = new Vector3(playerColor.r, playerColor.g, playerColor.b);

               // stream.Serialize(ref tempColor);
            }
            else
            {
                //get color
                //stream.Serialize(ref tempColor);

                //syncColor = new Color(tempColor.x, tempColor.y, tempColor.z, 1.0f);

            }

        }
        #endregion
    }
}