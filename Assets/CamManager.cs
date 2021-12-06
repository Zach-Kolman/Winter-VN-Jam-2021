using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CamManager : MonoBehaviour
{
    public List<CinemachineVirtualCamera> Cams;

    public int basePriority = 10;

    public CinemachineVirtualCamera curCam;
    // Start is called before the first frame update
    void Start()
    {
         
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SwitchCamera()
    {
        foreach (CinemachineVirtualCamera vcam in Cams)
        {
            vcam.Priority = basePriority - 1;
        }

    }
}
