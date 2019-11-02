using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class OceanWithGPU : MonoBehaviour
{
    #region public
    public bool hasChanged = false;
    public float Speed = 1f;
    public int EdgeScale;
    public float Q = 1f;
    [SerializeField]
    Vector2 Wind;
    [Range(0f, 10f)]
    public float A;
    public bool isForce = true;
    #region Shaders
    public Shader initialShader;
    public Shader omegaktShader;
    public Shader htildeShader;
    public Shader fftShader;
    #endregion
    #region Material
    private Material initialMat;
    private Material omegaktMat;
    private Material htildeMat;
    private Material fftMat;
    #endregion
    #endregion
    #region RenderTexture
    public RenderTexture initialTexture;
    public RenderTexture omegaktTexture;
    public RenderTexture htildeTexture;
    public RenderTexture heightTexture;

    private RenderTexture tempa;
    private RenderTexture tempb;
    #endregion
    public Material testmat;
    private int edgelen;

    public bool isSave = true;
    // Start is called before the first frame update
    void Start()
    {
        SetParams();
        InitRender();
    }

    // Update is called once per frame
    void Update()
    {
        float deltaTime = Time.deltaTime;
        omegaktMat.SetFloat("_DeltaT", deltaTime * Speed);
        omegaktMat.SetInt("_Len", edgelen);
        //omegaktMat.SetTexture("_LastT", omegaktTexture);
        
        Graphics.Blit(null, omegaktTexture, omegaktMat);
        htildeMat.SetTexture("_Init", initialTexture);
        htildeMat.SetTexture("_OmegaKT", omegaktTexture);
        Graphics.Blit(null, htildeTexture, htildeMat);
        if (isSave)
        {
            isSave = false;
            SaveRenderTextureToPNG(omegaktTexture, @"C:\Users\lenovo\Desktop\OceanSimulation", "rt");
        }
        fftMat.SetFloat("_Len", (float)edgelen);
        int iterations = EdgeScale * 2;
        #region 渲染高度场
        fftMat.EnableKeyword("_HORIZONTAL");
        fftMat.DisableKeyword("_VERTICAL");
        for (int i = 0; i < iterations; i++)
        {
            RenderTexture blitTarget;
            fftMat.SetFloat("_SubLen", Mathf.Pow(2, (i % (iterations / 2)) + 1));
            if (i == 0)
            {
                fftMat.SetTexture("_Input", htildeTexture);
                blitTarget = tempa;
            }
            else if (i == iterations - 1)
            {
                fftMat.SetTexture("_Input", (i % 2 == 0) ? tempb : tempa);
                blitTarget = heightTexture;
            }
            else if (i % 2 == 1)
            {
                fftMat.SetTexture("_Input", tempa);
                blitTarget = tempb;
            }
            else
            {
                fftMat.SetTexture("_Input", tempb);
                blitTarget = tempa;
            }
            if (i == iterations / 2)
            {
                fftMat.DisableKeyword("_HORIZONTAL");
                fftMat.EnableKeyword("_VERTICAL");
            }
            Graphics.Blit(null, blitTarget, fftMat);
        }
        #endregion
        testmat.SetTexture("_Height", heightTexture);
    }
    private void InitRender()
    {
        initialMat.SetFloat("_A", A);
        initialMat.SetInt("_Len", edgelen);
        initialMat.SetVector("_Wind", Wind);
        Graphics.Blit(null, initialTexture, initialMat);
    }
    private void SetParams()
    {
        edgelen = 1 << EdgeScale;

        initialMat = new Material(initialShader);
        omegaktMat = new Material(omegaktShader);
        htildeMat = new Material(htildeShader);
        fftMat = new Material(fftShader);

        initialTexture = new RenderTexture(edgelen, edgelen, 0, RenderTextureFormat.ARGBFloat);
        omegaktTexture = new RenderTexture(edgelen, edgelen, 0, RenderTextureFormat.RFloat);
        htildeTexture = new RenderTexture(edgelen, edgelen, 0, RenderTextureFormat.ARGBFloat);
        tempa = new RenderTexture(edgelen, edgelen, 0, RenderTextureFormat.ARGBFloat);
        tempb = new RenderTexture(edgelen, edgelen, 0, RenderTextureFormat.ARGBFloat);
        heightTexture = new RenderTexture(edgelen, edgelen, 0, RenderTextureFormat.ARGBFloat);
    }

    public bool SaveRenderTextureToPNG(RenderTexture rt, string contents, string pngName)
    {
        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = rt;
        Texture2D png = new Texture2D(rt.width, rt.height, TextureFormat.RFloat, false);
        png.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        byte[] bytes = png.EncodeToPNG();
        if (!Directory.Exists(contents))
            Directory.CreateDirectory(contents);
        FileStream file = File.Open(contents + "/" + pngName + ".png", FileMode.Create);
        BinaryWriter writer = new BinaryWriter(file);
        writer.Write(bytes);
        file.Close();
        Texture2D.DestroyImmediate(png);
        png = null;
        RenderTexture.active = prev;
        return true;

    }  
}
