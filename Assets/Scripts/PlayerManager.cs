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
        public float speed = 10.0f;
        public float rotationSpeed = 100.0f;

        [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
        public static GameObject LocalPlayerInstance;
        public static string playerName;

        #endregion

        #region Private Variables



        #endregion

        #region MonoBehaviour CallBacks

        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity during early initialization phase.
        /// </summary>
        void Awake()
        {
            /*if (Beams == null)
            {
                Debug.LogError("<Color=Red><a>Missing</a></Color> Beams Reference.", this);
            }
            else
            {
                Beams.SetActive(false);
            }*/


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

        /// <summary>
        /// MonoBehaviour method called on GameObject by Unity on every frame.
        /// </summary>
        void Update()
        {
            ProcessInputs();

            // trigger Beams active state
            /*if (Beams != null && IsFiring != Beams.GetActive())
            {
                Beams.SetActive(IsFiring);
            }*/
        }

        [PunRPC]
        void NewPlayerSpawn(PhotonMessageInfo info)
        {
            Debug.Log("RPC CALLLLLLLLLLLLLLLED");

            //Set player's GameObject name
            this.name = "Pacman_" + PhotonNetwork.player.NickName;

            //Set player's color
            this.GetComponent<Renderer>().material.color = GameManager.colors[PhotonNetwork.playerList.Length - 1];
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
            else if (other.name.Contains("Pacman"))
            {

            }

            //Ghost collision: Player's position is reset to starting point. Score is decreased.
            else if (other.name.Contains("Ghost"))
            {

            }
            
        }
        
        #endregion

        #region Custom

        /// <summary>
        /// Processes the inputs. Maintain a flag representing when the user is pressing Fire.
        /// </summary>
        void ProcessInputs()
        {
            float translation = Input.GetAxis("Vertical") * speed * Time.deltaTime;
            float rotation = Input.GetAxis("Horizontal") * rotationSpeed * Time.deltaTime;           

            // Move translation along the object's z-axis
            transform.Translate(0, 0, translation);

            // Rotate around our y-axis
            transform.Rotate(0, rotation, 0);
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