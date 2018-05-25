using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FirstPersonCameraController : MonoBehaviour{
    private float m_yRot;
    private float m_xRot;
    private Vector3 rotation;
    private Vector3 m_rotation;
    public float lookSensitivity = 3.0f;

    private Rigidbody rb;

    [Header("The Camera the player looks through")]
    public Camera playerCamera;

    public IEnumerator actionRoutine;

    // Use this for initialization
    private void Start(){
        rb = this.GetComponent<Rigidbody>();
        actionRoutine = Frame();
        StartCoroutine(actionRoutine);
    }

    private IEnumerator Frame(){
        for (;;)
        {
            //mouse movement 
            m_yRot = Input.GetAxisRaw("Mouse X");
            m_rotation = new Vector3(0, m_yRot, 0) * lookSensitivity;

            m_xRot = Input.GetAxisRaw("Mouse Y");
            rotation = new Vector3(m_xRot, 0, 0) * lookSensitivity;

            if (m_rotation != Vector3.zero)
            {
                //rotate the camera of the player
                rb.MoveRotation(rb.rotation * Quaternion.Euler(m_rotation));
            }

            if (playerCamera != null)
            {
                //negate this value so it rotates like a FPS not like a plane
                Quaternion currentR = playerCamera.transform.rotation;
                playerCamera.transform.Rotate(-rotation);
                if(playerCamera.transform.rotation.eulerAngles.x > 50 && playerCamera.transform.rotation.eulerAngles.x < 310){
                    playerCamera.transform.rotation = currentR;
                }
            }
            yield return null;
        }
    }
}