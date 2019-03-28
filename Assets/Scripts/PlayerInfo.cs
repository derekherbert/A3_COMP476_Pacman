using System;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInfo
{
	public PlayerInfo(string name, int actorID, int viewID, int scoreTextViewID, Color color)
	{
        Name = name;
        ActorID = actorID;
        ViewID = viewID;
        ScoreTextViewID = scoreTextViewID;
        Color = color;
	}

    public string Name
    {
        get;
        set;
    }

    public int ActorID
    {
        get;
        set;
    }

    public int ViewID
    {
        get;
        set;
    }

    public int ScoreTextViewID
    {
        get;
        set;
    }

    public Color Color
    {
        get;
        set;
    }
}
