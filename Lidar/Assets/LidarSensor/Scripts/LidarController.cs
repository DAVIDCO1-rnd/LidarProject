using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LidarController : MonoBehaviour
{
    private const float CAMERA_FOV_DEG = 15f;
    private const float CAMERA_FOV_RAD = CAMERA_FOV_DEG * Mathf.Deg2Rad;
    private const float CAMERA_RANGE_MIN = 0.2f;
    private const float CAMERA_RANGE_MAX = 30f;
    private Color INFINITE_DISTANCE_COLOR = new Color(-1.0f, -1.0f, -1.0f, -1.0f);

    // 
    public GameObject CamRange;
    public float camerasRadius;
    public int numOfCameras;


    private int numOfLidarSimulatedCameras = 920;

    //
    private List<Camera> m_camArray;

    private void SetupCamera(Camera cam, int target)
    {
        // Update camera setup
        //cam.enabled = false;

        // Update generic data
        cam.fieldOfView = CAMERA_FOV_DEG;
        cam.nearClipPlane = CAMERA_RANGE_MIN - 0.05F;
        cam.farClipPlane = CAMERA_RANGE_MAX + 0.05f;
        //cam.enabled = true;
        //if (target < Display.displays.Length)
        cam.targetDisplay = target;

        // Force camera to render depth buffer
        // Unity disables it by default for faster renders
        cam.depthTextureMode |= DepthTextureMode.Depth;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = INFINITE_DISTANCE_COLOR;
        cam.fieldOfView = 360.0f / numOfCameras;

        //Add post processing shader which renders depth images
        //int cameraWidth = numOfLidarSimulatedCameras / numOfCameras;
        
        int cameraWidth = 50; //TODO - david - temporary 
        cam.fieldOfView = 20.0f; //TODO - david - temporary 
        cam.gameObject.AddComponent<RenderDistFromCamera>().mWidth = cameraWidth;
        //renderDistFromCamera.mWidth = cameraWidth;
        //renderDistFromCamera.mHeight = 3;

    }

    private void CreateCameraArray()
    {
        for (int i = 0; i < numOfCameras; ++i)
        {
            // calculate position and rotation
            float rad = i * (2 * Mathf.PI / numOfCameras);
            Vector3 pos = new Vector3(Mathf.Sin(rad) * camerasRadius,
                                    0,
                                    Mathf.Cos(rad) * camerasRadius);
            Quaternion rot = Quaternion.Euler(0, i * (360f / numOfCameras), 0);

            // Initialize camera gameobject
            GameObject obj = new GameObject();
            obj.transform.parent = CamRange.transform;
            obj.transform.localPosition = pos;
            obj.transform.localRotation = rot;

            // Add camera
            Camera cam = obj.AddComponent<Camera>();
            m_camArray.Add(cam);

            SetupCamera(cam, i+1);
        }
    }

    private void Start()
    {
        m_camArray = new List<Camera>();
        //cameraWidth = 360.0f / numOfCameras;
        //cameraHeight = cameraWidth;
        //cameraHeight = 3.0f;

        // Setup cameras
        CreateCameraArray();


        //for (int i = 1; i < Display.displays.Length; i++)
        //{
        //    Display.displays[i].Activate();
        //}
    }
}
