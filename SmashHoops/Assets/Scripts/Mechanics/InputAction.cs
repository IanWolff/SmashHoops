using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputAction
{
    public ActionItem Action;
    public float Timestamp;
    public static float ActionDuration = 0.8f;    

    public InputAction(ActionItem inputItem, float timeRecieved)
    {
        Action = inputItem;
        Timestamp = timeRecieved;
    }

    public bool IsValid()
    {
        if (Timestamp + ActionDuration >= Time.time)
        {
            return true;
        }
        return false;
    }

    public enum ActionItem
    {
        Jump,
        Punch,
        Kick,
        Special
    }
}
