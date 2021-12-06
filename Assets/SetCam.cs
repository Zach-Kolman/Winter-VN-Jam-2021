using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class SetCam : MonoBehaviour
{
    public CinemachineVirtualCamera curCam;
    public GameObject camManager;

    //set depth of all cameras to 10 minus 1;
    private void OnTriggerEnter(Collider other)
    {
        GameObject cMan = camManager;
        if(other.gameObject.tag == "Player")
        {
            cMan.GetComponent<CamManager>().SwitchCamera();
            curCam.Priority = cMan.GetComponent<CamManager>().basePriority + 1;

            cMan.GetComponent<CamManager>().curCam = curCam;
        }
    }
}
