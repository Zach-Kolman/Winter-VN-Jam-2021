using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PlayerController : MonoBehaviour
{
    private CharacterController cont;
    public float pAngle;

    public float turnSmoothTime = 0.1f;
    public float turnSmoothVelocity;

    public CinemachineVirtualCamera curCam;

    public GameObject CM;
    public Transform cmt;

    public float speed;
    // Start is called before the first frame update
    void Start()
    {
        cont = gameObject.GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        MovePlayer();

        curCam = CM.GetComponent<CamManager>().curCam;

        cmt = curCam.transform;
    }

    private void Fall()
    {
        Vector3 velocity = Vector3.up * -10;

        if (!cont.isGrounded)
        {
            cont.Move(velocity * Time.deltaTime);
        }
    }

    private void MovePlayer()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 direction = new Vector3(horizontal, 0, vertical).normalized;
        //animator.SetFloat("x", direction.x);
        //animator.SetFloat("y", direction.z);



        if (direction.magnitude >= 0.1f)
        {
            float tragetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cmt.eulerAngles.y;
            pAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, tragetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, pAngle, 0);
            Vector3 moveDir = Quaternion.Euler(0f, tragetAngle, 0f) * Vector3.forward;

            cont.Move(moveDir.normalized * Time.deltaTime * speed);
        }

    }
}
