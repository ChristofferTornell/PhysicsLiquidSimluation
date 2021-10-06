using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floor : MonoBehaviour
{
    public float xPos
    {
        get {
            return transform.position.x;
        }
    }
    public float yPos
    {
        get {
            return transform.position.y;
        }
    }
}
