using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public class PlayerManager : Photon.PunBehaviour, IPunObservable
    {
        #region Public Variables

        public int score;
        public Vector3 startingPosition;
        public float rotationSpeed = 100.0f;

        [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
        public static GameObject LocalPlayerInstance;
        public static string playerName;
        private float rotation = 0.0f;
        private bool isRotating = false;
        private Vector3 currentDirection;
        private KeyCode lastKeyPressed;

        #endregion

        #region Private Variables



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
                PlayerManager.LocalPlayerInstance = this.gameObject;
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
                ProcessInputs();                        
            }
        }

        [PunRPC]
        void NewPlayerSpawn(PhotonMessageInfo info)
        {
            Debug.Log("IN NEW PLAYER SPAWNNNNNNNNNNNNNN");

            //Set player's GameObject name
            this.gameObject.name = "Pacman_" + PhotonNetwork.player.NickName;

            //Set player's color
            this.GetComponent<Renderer>().material.color = GameManager.colors[PhotonNetwork.playerList.Length - 1];

            foreach (Node node in TileGenerator.Graph.Nodes)
            {
                Physics.IgnoreCollision(this.gameObject.GetComponent<SphereCollider>(), node.GameObject.GetComponent<SphereCollider>(), true);
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
            
            if (other.tag == "Node")
            {
                Debug.Log("ONTRIGGERENTER COLLISION WITH A NODE");
                //Physics.IgnoreCollision(this.GetComponent<Collider>(), other.GetComponent<Collider>(), true);
            }

            //Pacman collision: Both players bounce back and rotate towards where they were coming from
            else if (other.name.Contains("Pacman"))
            {
                Debug.Log("OYYYYYYYY");
            }

            //Ghost collision: Player's position is reset to starting point. Score is decreased.
            else if (other.name.Contains("Ghost"))
            {

            }

        }

        void OnCollisionEnter(Collision col)
        {
            if (col.gameObject.tag == "Node")
            {
                Debug.Log("ONCOLLISIONENTER COLLISION WITH A NODE");
                //Physics.IgnoreCollision(this.GetComponent<Collider>(), col.collider, true);
            }
        }

        #endregion

        #region Custom

        /// <summary>
        /// Processes the inputs. Maintain a flag representing when the user is pressing Fire.
        /// </summary>
        void ProcessInputs()
        {   
            GetComponent<Rigidbody>().MovePosition(transform.position + transform.forward * Time.deltaTime * 3);

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
            Node nodeInFront;
            RaycastHit hit;
            Vector3 nodePostion = new Vector3(this.gameObject.transform.position.x, 5.0f, this.gameObject.transform.position.z);

            if (!isRotating)
            {
                Debug.Log("Forward: " + transform.forward);

                if (Physics.Raycast(nodePostion, transform.forward, out hit, 1f))
                {
                    //Node in front of player
                    if (hit.collider.gameObject.tag == "Node")
                    {
                        int index = Convert.ToInt32(hit.collider.gameObject.name.Substring(5));
                        nodeInFront = TileGenerator.Graph.Nodes[index];
                        Debug.Log("Hit: " + index + "\tDistance: " + hit.distance);
                    }
                }
                //No node: Corner!
                else
                {
                    Debug.Log("NO HIT CORNER CORNER CORNER");
                    //User facing south
                    if (transform.forward.z == -1)
                    {
                        //Check for user's last input (must be left or right)
                        if(lastKeyPressed == KeyCode.A)
                        {
                            rotation = 90;
                        }
                        else if (lastKeyPressed == KeyCode.D)
                        {
                            rotation = -90;
                        }
                        
                        currentDirection = transform.forward;
                        isRotating = true;
                    }
                    //User facing north
                    else if (transform.forward.z == 1)
                    {
                        //Check for user's last input (must be left or right)
                        if (lastKeyPressed == KeyCode.A)
                        {
                            rotation = -90;
                        }
                        else if (lastKeyPressed == KeyCode.D)
                        {
                            rotation = 90;
                        }                                                

                        currentDirection = transform.forward;
                        isRotating = true;
                    }
                    //User facing left
                    else if (transform.forward.z == -1)
                    {
                        //Check for user's last input (must be left or right)
                        if (lastKeyPressed == KeyCode.W)
                        {
                            rotation = -90;
                        }
                        else if (lastKeyPressed == KeyCode.S)
                        {
                            rotation = 90;
                        }

                        currentDirection = transform.forward;
                        isRotating = true;
                    }
                    //User facing right
                    else if (transform.forward.x == 1)
                    {
                        //Check for user's last input (must be left or right)
                        if (lastKeyPressed == KeyCode.W)
                        {
                            rotation = 90;
                        }
                        else if (lastKeyPressed == KeyCode.S)
                        {
                            rotation = -90;
                        }

                        currentDirection = transform.forward;
                        isRotating = true;
                    }
                    else
                    {
                        rotation = 90.0f;
                    }
                }
            }

            if (isRotating)
            {
                Debug.Log("Angle: " + Vector3.Angle(currentDirection.normalized, transform.forward.normalized));
                if (Mathf.Abs(Vector3.Angle(currentDirection.normalized, transform.forward.normalized)) <= 90)
                {
                    rotatePlayer(rotation);
                }
                else
                {
                    isRotating = false;

                    //Align user direction perfectly with x and z axis
                    float x = transform.forward.x;
                    float z = transform.forward.z;

                    if (Mathf.Abs(x) * 2 > 1)
                    {
                        if (x > 0)
                        {
                            x = 1;
                        }
                        else
                        {
                            x = -1;
                        }                            
                    }
                    else
                    {
                        x = 0;
                    }
                    if (Mathf.Abs(z) * 2 > 1)
                    {
                        if (z > 0)
                        {
                            z = 1;
                        }
                        else
                        {
                            z = -1;
                        }
                    }
                    else
                    {
                        z = 0;
                    }

                    this.transform.forward = new Vector3(x, 0, z);
                }
            }
        }

        private void rotatePlayer(float rotation)
        {
            Quaternion deltaRotation = Quaternion.Euler(new Vector3(0, rotation, 0) * Time.deltaTime * 4);
            GetComponent<Rigidbody>().MoveRotation(GetComponent<Rigidbody>().rotation * deltaRotation);
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.isWriting)
            {
                // We own this player: send the others our data
               // stream.SendNext(IsFiring);
            }
            else
            {
                // Network player, receive data
                //this.IsFiring = (bool)stream.ReceiveNext();
            }
        }
        #endregion
    }
}