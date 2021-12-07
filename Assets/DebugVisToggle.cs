using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugVisToggle : MonoBehaviour
{
    public List<GameObject> triggs;

    public bool debugActive = false;

    private void Start()
    {
        foreach(GameObject coll in GameObject.FindGameObjectsWithTag("debug"))
        {
            triggs.Add(coll);
        }
    }

    private void Update()
    {
        EnableDebugRenderers();
    }

    void EnableDebugRenderers()
    {
        if(Input.GetButtonDown("Toggle Debug Render"))
        {
            debugActive = !debugActive;

            if(!debugActive)
            {
                foreach(GameObject coll in triggs)
                {
                    coll.GetComponent<MeshRenderer>().enabled = false;
                }
            }

            else
            {
                foreach (GameObject coll in triggs)
                {
                    coll.GetComponent<MeshRenderer>().enabled = true;
                }
            }
        }
    }
}
