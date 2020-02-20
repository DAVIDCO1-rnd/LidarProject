using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraPostProcessor : MonoBehaviour
{
    public Shader Shader;
    private Material m_material;

    private Material m_flipMaterial;

    private void Start()
    {
        if (Shader != null)
            m_material = new Material(Shader);

        m_flipMaterial = new Material(Shader.Find("Hidden/FlipShader"));
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (m_material != null)
            Graphics.Blit(src, src, m_material);

        Graphics.Blit(src, dest, m_flipMaterial);
    }
}
