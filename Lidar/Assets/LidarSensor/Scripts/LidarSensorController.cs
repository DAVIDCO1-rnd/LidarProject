using System.Collections.Generic;
using UnityEngine;

namespace Indoors.Sensors.LidarSensor
{
    public class LidarSensorController : TimeableBehaviour
    {
        private const float CAMERA_FOV_DEG = 15f;
        private const float CAMERA_FOV_RAD = CAMERA_FOV_DEG * Mathf.Deg2Rad;
        private const float CAMERA_RANGE_MIN = 0.2f;
        private const float CAMERA_RANGE_MAX = 5f;

        // 
        public GameObject CamRange;
        public float RangeSpread = 0.1f;
        public int RangeSize = 16;
        public Camera TopCamera;
        public Camera BottomCamera;

        //
        private List<Camera> m_camArray;
        private Texture2DPool m_pool;

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
            var pp = cam.gameObject.AddComponent<CameraPostProcessor>();
            pp.Shader = Shader.Find("Hidden/RangeSensor/Depth");
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
            m_pool = new Texture2DPool();
            m_camArray = new List<Camera>();

            // Setup cameras
            CreateCameraArray();
            SetupCamera(TopCamera);
            SetupCamera(BottomCamera);

            // Setup TimeableBehaviour
            this.FrameRate = 25;
            StartCycle();
        }

        private float ProcessCameraToRange(ushort[] data)
        {
            return Mathf.Clamp(data[data.Length / 2] / 1000.0f, CAMERA_RANGE_MIN, CAMERA_RANGE_MAX);
        }

        private float RenderCamera(Camera c)
        {
            // setup render texture
            var rt = RenderTexture.GetTemporary(15, 15, 0, RenderTextureFormat.R16);
            var currentRT = RenderTexture.active;
            RenderTexture.active = rt;
            c.targetTexture = rt;

            // Render the camera's view.
            c.Render();

            // Set texture2D
            var image = m_pool.GetTemporary(15, 15, TextureFormat.R16);
            image.ReadPixels(new Rect(0, 0, 15, 15), 0, 0);
            image.Apply();

            // post-render
            RenderTexture.active = currentRT;
            RenderTexture.ReleaseTemporary(rt);

            // get raw data
            ushort[] data = image.GetRawTextureData<ushort>().ToArray();

            // move to pool
            m_pool.ReleaseTemporary(image);

            return ProcessCameraToRange(data);
        }

        private (float[], float, float) RenderRangeSensor()
        {
            var results = new float[RangeSize];
            var topRange = RenderCamera(TopCamera);
            var bottomRange = RenderCamera(BottomCamera);
            for (var i = 0; i < m_camArray.Count; ++i)
                results[i] = RenderCamera(m_camArray[i]);

            return (results, topRange, bottomRange);
        }

        private void SendResultsToPipe(float[] results, float topRange, float bottomRange)
        {
            // Send range array message
            LaserRangeMessage rangeArrayMessage = new LaserRangeMessage
            {
                angle_min = 0,
                angle_max = (2 * Mathf.PI) - Mathf.Epsilon,
                angle_increment = 2 * Mathf.PI / RangeSize,

                time_increment = 0,
                scan_time = 0,

                range_min = CAMERA_RANGE_MIN,
                range_max = CAMERA_RANGE_MAX,
            };
            PipeSender.Instance.SendLaserRangeMessage("range_array", rangeArrayMessage, results);

            // prepare and send top range message
            RangeMessage topRangeMsg = new RangeMessage
            {
                radiation_type = RadiationType.INFRARED,
                field_of_view = CAMERA_FOV_RAD,
                range_min = CAMERA_RANGE_MIN,
                range_max = CAMERA_RANGE_MAX,
                range = topRange
            };
            PipeSender.Instance.SendRangeMessage("range_top", topRangeMsg);

            // prepare and send top range message
            RangeMessage bottomRangeMsg = new RangeMessage
            {
                radiation_type = RadiationType.INFRARED,
                field_of_view = CAMERA_FOV_RAD,
                range_min = CAMERA_RANGE_MIN,
                range_max = CAMERA_RANGE_MAX,
                range = bottomRange
            };
            PipeSender.Instance.SendRangeMessage("range_bottom", bottomRangeMsg);
        }

        public void RenderToPipe()
        {
            var (results, topRange, bottomRange) = RenderRangeSensor();
            SendResultsToPipe(results, topRange, bottomRange);
        }

        protected override void TimeableCallback()
        {
            RenderToPipe();
        }
    }
}