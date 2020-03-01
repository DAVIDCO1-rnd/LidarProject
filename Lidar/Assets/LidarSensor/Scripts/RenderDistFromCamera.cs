using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class RenderDistFromCamera : MonoBehaviour
{
    public Texture2D tex;
    public Texture2D resizedTex;

    Shader replacementShader;
    Camera mCamera;
    public int mWidth;
    public int mHeight = 30;
    public float[] distancesFromCamera;

    private const float INFINITE_DISTANCE = -1.0f;    
    int screenWidth;
    int screenHeight;

    bool debugWriteToFile = true;
    static int debugConter = 0;

    private void Start()
    {
        mCamera = GetComponent<Camera>();
        //I tried using "mCamera.pixelRect = new Rect(0, 0, (float)mWidth, (float)mHeight)" in order to declare camera with the desired resolution. But the shader attached to the camera didn't work well. Therefore, I'm using the shader on the screen resolution and only then I resize the camera resolution.
        screenWidth = mCamera.pixelWidth;
        screenHeight = mCamera.pixelHeight;
        replacementShader = Shader.Find("LidarSensor/Depth");
        if (replacementShader != null)
            mCamera.SetReplacementShader(replacementShader, "");
        tex = new Texture2D(screenWidth, screenHeight, TextureFormat.RGBAFloat, false);
        distancesFromCamera = new float[mWidth];
    }

    public enum ImageFilterMode : int
    {
        Nearest = 0,
        Biliner = 1,
        Average = 2
    }
    public static Texture2D ResizeTexture(Texture2D pSource, ImageFilterMode pFilterMode, float xWidth, float xHeight)
    {

        //*** Variables
        int i;

        //*** Get All the source pixels
        Color[] aSourceColor = pSource.GetPixels(0);
        Vector2 vSourceSize = new Vector2(pSource.width, pSource.height);

        //*** Make New
        Texture2D oNewTex = new Texture2D((int)xWidth, (int)xHeight, TextureFormat.RGBAFloat, false);

        //*** Make destination array
        int xLength = (int)xWidth * (int)xHeight;
        Color[] aColor = new Color[xLength];

        Vector2 vPixelSize = new Vector2(vSourceSize.x / xWidth, vSourceSize.y / xHeight);

        //*** Loop through destination pixels and process
        Vector2 vCenter = new Vector2();
        for (i = 0; i < xLength; i++)
        {

            //*** Figure out x&y
            float xX = (float)i % xWidth;
            float xY = Mathf.Floor((float)i / xWidth);

            //*** Calculate Center
            vCenter.x = (xX / xWidth) * vSourceSize.x;
            vCenter.y = (xY / xHeight) * vSourceSize.y;

            //*** Do Based on mode
            //*** Nearest neighbour (testing)
            if (pFilterMode == ImageFilterMode.Nearest)
            {

                //*** Nearest neighbour (testing)
                vCenter.x = Mathf.Round(vCenter.x);
                vCenter.y = Mathf.Round(vCenter.y);

                //*** Calculate source index
                int xSourceIndex = (int)((vCenter.y * vSourceSize.x) + vCenter.x);

                //*** Copy Pixel
                aColor[i] = aSourceColor[xSourceIndex];
            }

            //*** Bilinear
            else if (pFilterMode == ImageFilterMode.Biliner)
            {

                //*** Get Ratios
                float xRatioX = vCenter.x - Mathf.Floor(vCenter.x);
                float xRatioY = vCenter.y - Mathf.Floor(vCenter.y);

                //*** Get Pixel index's
                int xIndexTL = (int)((Mathf.Floor(vCenter.y) * vSourceSize.x) + Mathf.Floor(vCenter.x));
                int xIndexTR = (int)((Mathf.Floor(vCenter.y) * vSourceSize.x) + Mathf.Ceil(vCenter.x));
                int xIndexBL = (int)((Mathf.Ceil(vCenter.y) * vSourceSize.x) + Mathf.Floor(vCenter.x));
                int xIndexBR = (int)((Mathf.Ceil(vCenter.y) * vSourceSize.x) + Mathf.Ceil(vCenter.x));

                //*** Calculate Color
                aColor[i] = Color.Lerp(
                    Color.Lerp(aSourceColor[xIndexTL], aSourceColor[xIndexTR], xRatioX),
                    Color.Lerp(aSourceColor[xIndexBL], aSourceColor[xIndexBR], xRatioX),
                    xRatioY
                );
            }

            //*** Average
            else if (pFilterMode == ImageFilterMode.Average)
            {

                //*** Calculate grid around point
                int xXFrom = (int)Mathf.Max(Mathf.Floor(vCenter.x - (vPixelSize.x * 0.5f)), 0);
                int xXTo = (int)Mathf.Min(Mathf.Ceil(vCenter.x + (vPixelSize.x * 0.5f)), vSourceSize.x);
                int xYFrom = (int)Mathf.Max(Mathf.Floor(vCenter.y - (vPixelSize.y * 0.5f)), 0);
                int xYTo = (int)Mathf.Min(Mathf.Ceil(vCenter.y + (vPixelSize.y * 0.5f)), vSourceSize.y);

                //*** Loop and accumulate
                Color oColorTemp = new Color();
                float xGridCount = 0;
                for (int iy = xYFrom; iy < xYTo; iy++)
                {
                    for (int ix = xXFrom; ix < xXTo; ix++)
                    {

                        //*** Get Color
                        oColorTemp += aSourceColor[(int)(((float)iy * vSourceSize.x) + ix)];

                        //*** Sum
                        xGridCount++;
                    }
                }

                //*** Average Color
                aColor[i] = oColorTemp / (float)xGridCount;
            }
        }

        //*** Set Pixels
        oNewTex.SetPixels(aColor);
        oNewTex.Apply();

        //*** Return
        return oNewTex;
    }

    private void RenderCamera()
    {
        // setup render texture
        RenderTexture rt = RenderTexture.GetTemporary(screenWidth, screenHeight, 24, RenderTextureFormat.ARGBFloat);
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = rt;
        mCamera.targetTexture = rt;
        
        // Render the camera's view.
        mCamera.Render();
        // Set texture2D
        tex.ReadPixels(new Rect(0, 0, (float)screenWidth, (float)screenHeight), 0, 0);        
        tex.Apply();        
        resizedTex = ResizeTexture(tex, ImageFilterMode.Nearest, mWidth, mHeight);
        //Not destroying tex since it is used in 'Update()' every frame
        // post-render
        RenderTexture.active = currentRT;
        mCamera.targetTexture = currentRT; //show the scene on the screen
        RenderTexture.ReleaseTemporary(rt);
    }


    void calculateDistances()
    {        
        float[] buffer = resizedTex.GetRawTextureData<float>().ToArray();
        int numOfFloatsInPixel = 4; //fragment shader returns float4
        int numOfFloatsInWidth = numOfFloatsInPixel * mWidth;

        int counter;
        float sumDistances;
        int arrIndex = 0;
        for (int x = 0; x < numOfFloatsInWidth; x += numOfFloatsInPixel)
        {
            counter = 0;
            sumDistances = 0;
            for (int y = 0; y < mHeight; y++)
            {
                int index = x + y * numOfFloatsInWidth;
                float rVal = buffer[index]; //fragment shader returns float4(dist, dist, dist, dist) so we can read the distance from any of the 4 components
                if (rVal >= 0) //if rVal<0 it means that it is background (distance is infinite)
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

    void debugWriteTextureToFile()
    {
        //this function is used for debugging. you can use matlab code script_read_image_from_csv.m to see the Texture2D frames of each camera
        string folderPath = "Debug_Images";
        if (debugConter == 1)
        {
            bool isFolderExists = System.IO.Directory.Exists(folderPath);
            if (isFolderExists)
            {
                System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(folderPath);

                foreach (System.IO.FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }
            }
            System.IO.Directory.CreateDirectory(folderPath);
        }

        //This function is used to read the texture in matlab so we can see the Texture2D
        float[] buffer = resizedTex.GetRawTextureData<float>().ToArray();
        string filePath = folderPath + "/" + "matlabImage_" + debugConter.ToString() + ".csv";
        System.IO.StreamWriter writer = new System.IO.StreamWriter(filePath);
        int numOfBytesInWidth = 4 * mWidth;
        for (int j = mHeight-1; j >= 0; j--) //reversing j since the the Y axis in the image is upside down
        {            
            for (int i = 0; i < numOfBytesInWidth; i += 4)
            {
                int index = j * numOfBytesInWidth + i;
                float rVal = buffer[index];
                if (i == numOfBytesInWidth - 4)
                    writer.Write(rVal);
                else
                    writer.Write(rVal + ",");
            }
            writer.Write(System.Environment.NewLine);
        }
        writer.Close();
    }

    void Update()
    {
        RenderCamera();        
        calculateDistances();

        if (debugWriteToFile)
        {
            debugConter++;
            debugWriteTextureToFile();
            debugWriteToFile = false;
        }        
    }
}
