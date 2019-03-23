using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts
{
    public class GameManager : Photon.PunBehaviour
    {
        #region Public Variables

        [Tooltip("The prefab to use for representing the player")]
        public GameObject playerPrefab;
        public int numberOfPlayers = 0;
        public Vector3[] startingPositions = { new Vector3(-9.5f, 0.5f, 10.5f), //Top-left corner
                                               new Vector3(10.5f, 0.5f, -9.5f), //Bottom-right corner
                                               new Vector3(10.5f, 0.5f, 10.5f), //Top-right corner
                                               new Vector3(-9.5f, 0.5f, -9.5f)  //Bottom-left corner
                                             };

        #endregion

        #region Private Variables



        #endregion

        #region MonoBehaviour CallBacks

        void Start()
        {
            if (PlayerManager.LocalPlayerInstance == null)
            {    
                Debug.Log("We are Instantiating LocalPlayer from " + Application.loadedLevelName);
                // we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
                PhotonNetwork.Instantiate(this.playerPrefab.name, startingPositions[numberOfPlayers], Quaternion.identity, 0);
                numberOfPlayers++;
            }
            else
            {
                Debug.Log("Ignoring scene load for " + Application.loadedLevelName);
            }
        }

        #endregion

        #region Photon Messages


        /// <summary>
        /// Called when the local player left the room. We need to load the launcher scene.
        /// </summary>
        public override void OnLeftRoom()
        {
            numberOfPlayers--;
            SceneManager.LoadScene(0);
        }

        public override void OnPhotonPlayerConnected(PhotonPlayer other)
        {
            Debug.Log("OnPhotonPlayerConnected() " + other.NickName); // not seen if you're the player connecting


            if (PhotonNetwork.isMasterClient)
            {
                Debug.Log("OnPhotonPlayerConnected isMasterClient " + PhotonNetwork.isMasterClient); // called before OnPhotonPlayerDisconnected


                LoadArena();
            }
        }


        public override void OnPhotonPlayerDisconnected(PhotonPlayer other)
        {
            Debug.Log("OnPhotonPlayerDisconnected() " + other.NickName); // seen when other disconnects


            if (PhotonNetwork.isMasterClient)
            {
                Debug.Log("OnPhotonPlayerDisonnected isMasterClient " + PhotonNetwork.isMasterClient); // called before OnPhotonPlayerDisconnected


                LoadArena();
            }
        }

        #endregion


        #region Public Methods


        public void LeaveRoom()
        {
            PhotonNetwork.LeaveRoom();
        }


        #endregion

        #region Private Methods


        void LoadArena()
        {
            if (!PhotonNetwork.isMasterClient)
            {
                Debug.LogError("PhotonNetwork : Trying to Load a level but we are not the master Client");
            }
            Debug.Log("PhotonNetwork : Loading Level : Room");
            PhotonNetwork.LoadLevel("Room");
        }


        #endregion
    }
}
