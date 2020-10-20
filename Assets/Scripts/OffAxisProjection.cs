using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OffAxisProjection : MonoBehaviour {
    public Camera myCamera;

    //public LineRenderer lineRenderer;

    /*
        normalize euler? move from there, depending on the normalize?
        will that avoid gimabls? 

        get scale factor correct
        try some shit to calculate distance correctly
        1. set portalpos as -1 or 0. Gotta find what my portal pos is. Or make my own "portal".
        2. work more similiarly to eye tracking example

        correctly set up left, right, top bottom. In meters? Or do like 1/2 like in other exmaple. 

        devicecam is their portal cam? I think
    */

    public float left, right, bottom, top, near, far;

    public float nearDist;

    float distance;

    Quaternion orientation;

    public GameObject RotationSphere;

    Quaternion offset;

    //Every frame try to get gyroscope commands, until they kick in. 
    IEnumerator InitializeGyro () {
        Input.gyro.enabled = true;
        offset = Input.gyro.attitude;
        while (offset == Quaternion.identity) {
            offset = Input.gyro.attitude;
            yield return null;
        }
        Debug.Log ("Awake" + offset);
    }

    void Awake () {
        StartCoroutine (InitializeGyro ());
    }

    void Start () {
        //RotationSphere.transform.rotation = Random.rotation;
        //offset = Input.gyro.attitude;
        //Debug.Log ("START " + offset.eulerAngles.x + " " + offset.eulerAngles.y + " " + offset.eulerAngles.z);
        //difference between this and identity
    }

    void LateUpdate () {

        orientation = Quaternion.Inverse (offset) * Input.gyro.attitude;
        //Vector3 myCamPos = myCamera.ScreenToWorldPoint ();
        //orientation = Quaternion.Inverse (offset) * RotationSphere.transform.rotation;

        // landscape iPhone X, measures in meters
        left = myCamera.transform.position.x - 0.000f;
        right = myCamera.transform.position.x + 0.135f;
        top = myCamera.transform.position.y + 0.022f;
        bottom = myCamera.transform.position.y - 0.040f;

        distance = Mathf.Abs (myCamera.transform.position.z);

        far = 10f; // may need bigger for bigger scenes, max 10 metres for now
        near = 0.1f;

        Vector3 topLeft = new Vector3 (left, top, near);
        Vector3 topRight = new Vector3 (right, top, near);
        Vector3 bottomLeft = new Vector3 (left, bottom, near);
        Vector3 bottomRight = new Vector3 (right, bottom, near);

        // move near to 0.01 (1 cm from eye)
        float scale_factor = near / distance; //0.01f / near; near was one
        near *= scale_factor;
        left *= scale_factor;
        right *= scale_factor;
        top *= scale_factor;
        bottom *= scale_factor;

        Matrix4x4 m = PerspectiveOffCenter (left, right, bottom, top, near, far);
        myCamera.projectionMatrix = m;

        //Vector3 euler = EulerClamp (orientation);
        //1 - 0.5f * Mathf.Min (Mathf.Abs (myCamera.transform.position.x) + Mathf.Abs (myCamera.transform.position.y), 1)

        /*
        myCamera.transform.position = new Vector3 (
            (-euler.y / 60),
            euler.x / 60, -1f
        );
        */
        Vector3 directionFromQ = orientation * Vector3.forward;
        myCamera.transform.position = directionFromQ;
        myCamera.transform.position = new Vector3 (myCamera.transform.position.x, myCamera.transform.position.y, -1f);
        //Debug.Log ("UPDATE " + directionFromQ.x);
    }

    private Vector3 EulerClamp (Quaternion orientation) {
        Vector3 euler = orientation.eulerAngles;

        if (euler.y >= 180f) {
            euler.y -= 360;
            euler.y = Mathf.Abs (euler.y);
        }
        if (euler.x >= 180f) {
            euler.x -= 360;
            euler.x = Mathf.Abs (euler.x);
        }
        if (euler.z >= 180f) {
            euler.z -= 360;
            euler.z = Mathf.Abs (euler.z);
        }

        return euler;
    }

    private static Quaternion GyroToUnity (Quaternion q) {
        return new Quaternion (q.x, q.y, -q.z, -q.w);
    }

    static Matrix4x4 PerspectiveOffCenter (float left, float right, float bottom, float top, float near, float far) {
        float x = 2.0F * near / (right - left);
        float y = 2.0F * near / (top - bottom);
        float a = (right + left) / (right - left);
        float b = (top + bottom) / (top - bottom);
        float c = -(far + near) / (far - near);
        float d = -(2.0F * far * near) / (far - near);
        float e = -1.0F;
        Matrix4x4 m = new Matrix4x4 ();
        m[0, 0] = x;
        m[0, 1] = 0;
        m[0, 2] = a;
        m[0, 3] = 0;
        m[1, 0] = 0;
        m[1, 1] = y;
        m[1, 2] = b;
        m[1, 3] = 0;
        m[2, 0] = 0;
        m[2, 1] = 0;
        m[2, 2] = c;
        m[2, 3] = d;
        m[3, 0] = 0;
        m[3, 1] = 0;
        m[3, 2] = e;
        m[3, 3] = 0;
        return m;
    }
}