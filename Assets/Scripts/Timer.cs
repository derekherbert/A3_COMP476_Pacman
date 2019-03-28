using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    bool startTimer = false;
    float timerValue;
    float startTime;
    int timerDuration = 180;
    ExitGames.Client.Photon.Hashtable CustomValue;
    string prettyTime;

    void Start()
    {
        if (PhotonNetwork.player.IsMasterClient)
        {
            CustomValue = new ExitGames.Client.Photon.Hashtable();
            startTime = (float) PhotonNetwork.time;
            startTimer = true;
            CustomValue.Add("StartTime", startTime);
            PhotonNetwork.room.SetCustomProperties(CustomValue);
        }
        else
        {
            startTime = float.Parse(PhotonNetwork.room.CustomProperties["StartTime"].ToString());
            startTimer = true;
        }
    }

    void Update()
    {

        if (!startTimer) return;

        timerValue = (int)((float)PhotonNetwork.time - startTime);

        Debug.Log("Time Left: " + timerValue);

        PhotonView.Find(63).GetComponent<Text>().text = "Time Left : " + getPrettyTime(timerDuration - (int)timerValue);

        //Timer Completed
        if (timerValue >= timerDuration)
        {
            PhotonNetwork.Disconnect();
            Debug.Log("YOU'VE BEEN DISCONNECTED");
        }
    }

    private string getPrettyTime(int time)
    {
        prettyTime = (int)time / 60 + ":" + time % 60;

        if (time%60 == 0)
        {
            prettyTime += "0";
        }

        return prettyTime;


    }
}
