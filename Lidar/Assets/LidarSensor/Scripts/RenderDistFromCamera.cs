using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[ExecuteInEditMode]
public class RenderDistFromCamera : MonoBehaviour
{
    public Rect rect;
    public Texture2D tex;
    public RenderTexture renderTexture;

    Shader replacementShader;
    Camera mCamera;
    public int mWidth;
    public int mHeight = 30;
    public float[] distancesFromCamera;

    void Start()
    {        
        replacementShader = Shader.Find("LidarSensor/Depth");
        mCamera = GetComponent<Camera>();
        mCamera.pixelRect = new Rect(0, 0, (float)mWidth, (float)mHeight);
        if (replacementShader != null)
            mCamera.SetReplacementShader(replacementShader, "");


        rect = new Rect(0, 0, (float)mWidth, (float)mHeight);
        renderTexture = new RenderTexture(mWidth, mHeight, 24);
        tex = new Texture2D(mWidth, mHeight, TextureFormat.RGBAFloat, false);
    }

    private void OnDestroy()
    {
#if UNITY_EDITOR
        DestroyImmediate(renderTexture);
        DestroyImmediate(tex);
#else
        Destroy(renderTexture);
        Destroy(tex);
#endif
    }

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
          
        for (int x = 0; x < mWidth; ++x)
            for (int y = 0; y < mHeight; ++y)
            {
                float currentDist = buffer[x * mHeight + y];
                distancesFromCamera[x] += currentDist / (float)mHeight;
            }                
    }
}
