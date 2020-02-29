using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[ExecuteInEditMode]
public class RenderDistFromCamera : MonoBehaviour
{
    public Rect rect;
    public Texture2D tex;
    //public RenderTexture renderTexture;

    Shader replacementShader;
    Camera mCamera;
    public int mWidth;
    public int mHeight = 30;
    public float[] distancesFromCamera;

    private const float INFINITE_DISTANCE = -1.0f;

    private void Start()
    {
        mCamera = GetComponent<Camera>();
        //int widthBefore = mCamera.pixelWidth;
        //int heightBefore = mCamera.pixelHeight;
        //mCamera.pixelRect = new Rect(0, 0, (float)mWidth, (float)mHeight);
        //int widthAfter = mCamera.pixelWidth;
        //int heightAfter = mCamera.pixelHeight;

        replacementShader = Shader.Find("LidarSensor/Depth");

        if (replacementShader != null)
            mCamera.SetReplacementShader(replacementShader, "");


        rect = new Rect(0, 0, (float)mWidth, (float)mHeight);
        //renderTexture = new RenderTexture(mWidth, mHeight, 24);
        tex = new Texture2D(mWidth, mHeight, TextureFormat.RGBAFloat, false);
    }


//    private void OnDestroy()
//    {
//#if UNITY_EDITOR
//        DestroyImmediate(renderTexture);
//        DestroyImmediate(tex);
//#else
//        Destroy(renderTexture);
//        Destroy(tex);
//#endif
//    }

    //private void UpdateScreenshot()
    //{
    //    mCamera.targetTexture = renderTexture;
    //    RenderTexture.active = renderTexture;
    //    mCamera.Render();

    //    tex.ReadPixels(rect, 0, 0);
        

    //    mCamera.targetTexture = null;
    //    RenderTexture.active = null;

    //    renderTexture = null;
    //    tex.Apply();
    //}

    private void RenderCamera()
    {
        // setup render texture
        RenderTexture rt = RenderTexture.GetTemporary(mWidth, mHeight, 24, RenderTextureFormat.ARGBFloat); //new RenderTexture(mWidth, mHeight, 24, UnityEngine.Experimental.Rendering.GraphicsFormat.R32G32B32A32_SFloat); //  RenderTexture.GetTemporary(mWidth, mHeight, 0, RenderTextureFormat.BGRA32);
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = rt;
        mCamera.targetTexture = rt;
        
        // Render the camera's view.
        mCamera.Render();

        // Set texture2D
        tex.ReadPixels(new Rect(0, 0, (float)mWidth, (float)mHeight), 0, 0);
        tex.Apply();

        // post-render
        RenderTexture.active = currentRT;
        mCamera.targetTexture = currentRT; //show the scene on the screen
        RenderTexture.ReleaseTemporary(rt);
    }



    void Update()
    {
        RenderCamera();
        
        //UpdateScreenshot();
        float[] buffer = tex.GetRawTextureData<float>().ToArray();

        // todo: evaulate cache-friendliness
        // todo: postpond division
        distancesFromCamera = new float[mWidth];
        int floatSize = sizeof(float);

        int counter;
        float sumDistances;
        int arrIndex = 0;
        for (int x = 0; x < floatSize * mWidth; x += floatSize)
        {
            counter = 0;
            sumDistances = 0;
            for (int y = 0; y < mHeight; y++)
            {
                int index = x + y * floatSize * mWidth;
                float rVal = buffer[index];
                if (rVal >= 0)
                {
                    counter++;
                    sumDistances += rVal;
                }
            }
            if (counter > 0)
            {
                float averageDistance = sumDistances / counter;
                distancesFromCamera[arrIndex] = averageDistance;
            }
            else
            {
                distancesFromCamera[arrIndex] = INFINITE_DISTANCE;
            }
            arrIndex++;
        }      
    }
}
