using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
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
    bool writeToFile = true;

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
        distancesFromCamera = new float[mWidth];
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

    //void OnGUI()
    //{
    //    if (!tex)
    //    {
    //        Debug.LogError("Assign a Texture in the inspector.");
    //        return;
    //    }

    //    //GUI.DrawTexture(new Rect(0, 0, (float)mWidth, (float)mHeight), tex, ScaleMode.ScaleToFit, true, 10.0F);

    //    if (Event.current.type.Equals(EventType.Repaint))
    //    {
    //        Graphics.DrawTexture(new Rect(0, 0, (float)mWidth, (float)mHeight), tex);
    //    }
    //}

    void calculateDistances()
    {
        float[] buffer = tex.GetRawTextureData<float>().ToArray();
        // todo: evaulate cache-friendliness
        // todo: postpond division

        int floatSize = sizeof(float);
        int numOfBytesInWidth = floatSize * mWidth;

        int counter;
        float sumDistances;
        int arrIndex = 0;
        for (int x = 0; x < numOfBytesInWidth; x += floatSize)
        {
            counter = 0;
            sumDistances = 0;
            for (int y = 0; y < mHeight; y++)
            {
                int index = x + y * numOfBytesInWidth;
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

    void writeTextureToFile()
    {
        float[] buffer = tex.GetRawTextureData<float>().ToArray();
        string filePath = "david.csv";
        System.IO.StreamWriter writer = new System.IO.StreamWriter(filePath);
        int numOfBytesInWidth = 4 * mWidth;
        for (int j = 0; j < mHeight; j++)
        {            
            for (int i = 0; i < numOfBytesInWidth; i += 4)
            {
                int index = j * numOfBytesInWidth + i;
                float rVal = buffer[index];
                writer.Write(rVal + ",");
            }
            writer.Write(System.Environment.NewLine);
        }
    }

    void Update()
    {
        RenderCamera();
        
        //UpdateScreenshot();
        
        calculateDistances();
        if (writeToFile)
        {
            writeTextureToFile();
            writeToFile = false;
        }
        
    }
}
