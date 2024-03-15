using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessMan : MonoBehaviour
{
    void OnMouseDown()
    {
        Debug.Log("Clicked on object: " + gameObject.name);
    }
}
