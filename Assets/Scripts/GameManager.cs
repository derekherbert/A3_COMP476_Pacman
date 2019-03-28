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
        static public GameManager Instance;

        public static Vector3[] startingPositions = { new Vector3(-9.5f, 0.5f, 10.5f), //Top-left corner
                                                      new Vector3(10.5f, 0.5f, -9.5f), //Bottom-right corner
                                                      new Vector3(10.5f, 0.5f, 10.5f), //Top-right corner
                                                      new Vector3(-9.5f, 0.5f, -9.5f)  //Bottom-left corner
                                                    };
        public static Vector3[] startingRotations = { new Vector3(0f, 180f, 0f), //Top-left corner
                                                      new Vector3(0f, 0f, 0f),   //Bottom-right corner
                                                      new Vector3(0f, 180f, 0f), //Top-right corner
                                                      new Vector3(0f, 0f, 0f)    //Bottom-left corner
                                                    };
        public static Color[] colors = { Color.yellow,
                                         Color.green,
                                         Color.blue,
                                         Color.red
                                       };
        public static int[] playerScoreTextBoxPhotonIDs = { 63, 357 };

        public static List<PlayerInfo> playerInfoList;
        public static Vector3[] fruitPositions = { new Vector3(0.5f, 0.5f, 0.5f),   //Middle
                                                   new Vector3(-9.5f, 0.5f, 10.5f), //Top-left corner
                                                   new Vector3(10.5f, 0.5f, -9.5f), //Bottom-right corner
                                                   new Vector3(10.5f, 0.5f, 10.5f), //Top-right corner
                                                   new Vector3(-9.5f, 0.5f, -9.5f)  //Bottom-left corner
                                                 };
        public static int fruitPositionCtr = 0;

        #endregion

        #region Private Variables



        #endregion

        #region MonoBehaviour CallBacks

        void Start()
        {
            Instance = this;
            playerInfoList = new List<PlayerInfo>(4);

            //Only instantiate a player if there is no local reference already
            if (PlayerManager.LocalPlayerInstance == null)
            {
                //Create a new player from a prefab
                PhotonView playerPhotonView = PhotonNetwork.Instantiate(this.playerPrefab.name, startingPositions[PhotonNetwork.playerList.Length - 1], Quaternion.Euler(startingRotations[PhotonNetwork.playerList.Length - 1]), 0).GetPhotonView();
                
                PlayerManager.NewPlayerSpawn(PhotonNetwork.playerName, PhotonNetwork.player.ID, playerPhotonView.viewID);
            }
        }        
        
        #endregion

        #region Photon Messages


        /// <summary>
        /// Called when the local player left the room. We need to load the launcher scene.
        /// </summary>
        public override void OnLeftRoom()
        {
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

        public static void addPellets()
        {
            if (PhotonNetwork.isMasterClient)
            {
                GameObject samplePellet = GameObject.Find("SamplePellet");
                GameObject parent = GameObject.Find("Pellets");
                int index = 0;

                foreach (GameObject floorTile in GameObject.FindGameObjectsWithTag("Floor"))
                {
                    GameObject pellet = PhotonNetwork.Instantiate("Pellet", new Vector3(floorTile.transform.position.x, 0.5f, floorTile.transform.position.z), Quaternion.identity, 0);
                    pellet.name = "Pellet_" + index;
                    pellet.transform.parent = parent.transform;
                    index++;
                }
            }
        }

        public static void spawnFruit()
        {
            GameObject fruit = PhotonNetwork.Instantiate("Fruit", fruitPositions[fruitPositionCtr], Quaternion.identity, 0);

            if (fruitPositionCtr < 4)
            {
                fruitPositionCtr++;
            }
            else
            {
                fruitPositionCtr = 0;
            }
            
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
