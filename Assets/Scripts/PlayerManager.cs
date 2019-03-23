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