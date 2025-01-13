using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionalRenderer : MonoBehaviour
{
    // Script to handle the rendering of 5 objects based on the selected part
    public List<GameObject> objectsToRender;

    public void SetObject(int i)
    {
        foreach (GameObject obj in objectsToRender)
        {
            obj.SetActive(false);
        }
        objectsToRender[i].SetActive(true);
        
    }
}
