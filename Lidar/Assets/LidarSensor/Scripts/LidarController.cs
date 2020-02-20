using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LidarController : MonoBehaviour
{
        private const float CAMERA_FOV_DEG = 15f;
        private const float CAMERA_FOV_RAD = CAMERA_FOV_DEG * Mathf.Deg2Rad;
        private const float CAMERA_RANGE_MIN = 0.2f;
        private const float CAMERA_RANGE_MAX = 5f;

        // 
        public GameObject CamRange;
        public float RangeSpread = 0.1f;
        public int RangeSize = 16;

        //
        private List<Camera> m_camArray;

        private void SetupCamera(Camera cam)
        {
            // Update camera setup
            cam.enabled = false;

            // Update generic data
            cam.fieldOfView = CAMERA_FOV_DEG;
            cam.nearClipPlane = CAMERA_RANGE_MIN - 0.05F;
            cam.farClipPlane = CAMERA_RANGE_MAX + 0.05f;

            // Force camera to render depth buffer
            // Unity disables it by default for faster renders
            cam.depthTextureMode |= DepthTextureMode.Depth;

            // Add post processing shader which renders depth images
            // var pp = cam.gameObject.AddComponent<CameraPostProcessor>();
            // pp.Shader = Shader.Find("Hidden/RangeSensor/Depth");
        }

        private void CreateCameraArray()
        {
            for (int i = 0; i < RangeSize; ++i)
            {
                // calculate position and rotation
                var rad = i * (2 * Mathf.PI / RangeSize);
                var pos = new Vector3(Mathf.Sin(rad) * RangeSpread,
                                      0,
                                      Mathf.Cos(rad) * RangeSpread);
                var rot = Quaternion.Euler(0, i * (360f / RangeSize), 0);

                // Initialize camera gameobject
                GameObject obj = new GameObject();
                obj.transform.parent = CamRange.transform;
                obj.transform.localPosition = pos;
                obj.transform.localRotation = rot;

                // Add camera
                var cam = obj.AddComponent<Camera>();
                m_camArray.Add(cam);

                SetupCamera(cam);
            }
        }

        private void Start()
        {
            m_camArray = new List<Camera>();

            // Setup cameras
            CreateCameraArray();
        }
}
