using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraPostProcessor : MonoBehaviour
{
    public Shader Shader;
    private Material m_material;

    //private Material m_flipMaterial;

    private void Start()
    {
        if (Shader != null)
            m_material = new Material(Shader);

        //m_flipMaterial = new Material(Shader.Find("FlipShader"));
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (m_material != null)
        {
            //RenderTexture temp = new RenderTexture(src);
            //Graphics.Blit(src, temp, m_material);
            //Graphics.Blit(temp, dest, m_material);

            Graphics.Blit(src, dest, m_material);
        }
            

        //Graphics.Blit(src, src, m_flipMaterial);
    }
}
