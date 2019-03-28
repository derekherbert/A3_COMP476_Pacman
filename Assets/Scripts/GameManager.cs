﻿using System;
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
        public static int[] playerScoreTextBoxPhotonIDs = { 372, 357 };

        public static List<PlayerInfo> playerInfoList;
        public static Vector3[] fruitPositions = { new Vector3(0.5f, 0.5f, 0.5f),   //Middle
                                                   new Vector3(-9.5f, 0.5f, 10.5f), //Top-left corner
                                                   new Vector3(10.5f, 0.5f, -9.5f), //Bottom-right corner
                                                   new Vector3(10.5f, 0.5f, 10.5f), //Top-right corner
                                                   new Vector3(-9.5f, 0.5f, -9.5f)  //Bottom-left corner
                                                 };
        public static int fruitPositionCtr = 0;

        public static float startTime;
        public int lastFruitSpawnTime = 180;

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

        void Update()
        {
            if (PhotonNetwork.isMasterClient)
            {
                if (lastFruitSpawnTime != Timer.timeLeft && Timer.timeLeft % 30 == 0)
                {
                    lastFruitSpawnTime = Timer.timeLeft;
                    spawnFruit();
                }
            }            
        }

        #endregion

        #region Photon Messages

        public override void OnPhotonPlayerConnected(PhotonPlayer other)
        {
            
        }


        public override void OnPhotonPlayerDisconnected(PhotonPlayer other)
        {
            
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
            //Destroy previous instance (if it exists)
            GameObject[] previousFruits = GameObject.FindGameObjectsWithTag("Fruit");

            foreach (GameObject previousFruit in previousFruits)
            {
                PhotonNetwork.Destroy(previousFruit.GetComponent<PhotonView>());
            }

            //Spawn a new fruit
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
            if (PhotonNetwork.isMasterClient)
            {                
                PhotonNetwork.LoadLevel("Room");
            }           
        }

        #endregion
    }
}
