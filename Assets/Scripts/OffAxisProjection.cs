using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OffAxisProjection : MonoBehaviour {

    private Quaternion Orientation, GyroOffset;
    private float left, right, bottom, top, near, far;

    [SerializeField]
    private Camera OffAxisCamera;
    [SerializeField]
    private float NearUserInput, FarUserInput;

    //Gyroscope hardware workaround. Attempt to get Gyroscope input, until your device enables it
    IEnumerator InitializeGyro () {
        Input.gyro.enabled = true;
        GyroOffset = Input.gyro.attitude;
        while (GyroOffset == Quaternion.identity) {
            GyroOffset = Input.gyro.attitude;
            yield return null;
        }
    }

    void Awake () {
        StartCoroutine (InitializeGyro ());
    }

    void LateUpdate () {

        Orientation = Quaternion.Inverse (GyroOffset) * Input.gyro.attitude;

        near = NearUserInput; //1f
        far = FarUserInput; //3f

        left = OffAxisCamera.transform.position.x - 16f;
        right = OffAxisCamera.transform.position.x + 16f;
        top = OffAxisCamera.transform.position.y + 14f;
        bottom = OffAxisCamera.transform.position.y - 14f;

        Vector3 topLeft = new Vector3 (left, top, near);
        Vector3 topRight = new Vector3 (right, top, near);
        Vector3 bottomLeft = new Vector3 (left, bottom, near);
        Vector3 bottomRight = new Vector3 (right, bottom, near);

        float scaleFactor = 0.01f / near; 
        near *= scaleFactor;
        left *= scaleFactor;
        right *= scaleFactor;
        top *= scaleFactor; 
        bottom *= scaleFactor;

        OffAxisCamera.projectionMatrix = PerspectiveOffCenter (left, right, bottom, top, near, far);

        OffAxisCamera.transform.position = Orientation * Vector3.forward;
        OffAxisCamera.transform.position = new Vector3 (OffAxisCamera.transform.position.x / 2, OffAxisCamera.transform.position.y / 2, 0f);
    }

    //https://docs.unity3d.com/520/Documentation/ScriptReference/Camera-projectionMatrix.html
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

    //Unused, but potentially useful in the future
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
}