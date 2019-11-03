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
    Vector2 Wind = new Vector2();
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
    private float timer = 0f;

    void Start()
    {
        SetParams();
        InitRender();
    }

    void Update()
    {
        float deltaTime = Time.deltaTime;
        timer += deltaTime * Speed;
        if(hasChanged)
        {
            InitRender();
            hasChanged = false;
        }
        omegaktMat.SetFloat("_Timer", timer);
        omegaktMat.SetInt("_Len", edgelen);
        
        Graphics.Blit(null, omegaktTexture, omegaktMat);
        htildeMat.SetTexture("_Init", initialTexture);
        htildeMat.SetTexture("_OmegaKT", omegaktTexture);
        Graphics.Blit(null, htildeTexture, htildeMat);
        fftMat.SetFloat("_Len", edgelen);
        int iterations = EdgeScale * 2;
        RenderTexture outputTexture = null, inputTexture = null;
        #region 渲染高度场
        fftMat.EnableKeyword("_HORIZONTAL");
        fftMat.DisableKeyword("_VERTICAL");
        for (int i = 0; i < iterations; i++)
        {
            outputTexture = (i % 2 == 0) ? tempa : tempb;
            inputTexture = (i % 2 == 1) ? tempa : tempb;
            fftMat.SetFloat("_SubLen", Mathf.Pow(2, (i % (iterations / 2)) + 1));
            if (i == 0)
            {
                inputTexture = htildeTexture;
            }
            if (i == iterations - 1)
            {
                outputTexture = heightTexture;
            }
            if (i == iterations / 2)
            {
                fftMat.DisableKeyword("_HORIZONTAL");
                fftMat.EnableKeyword("_VERTICAL");
            }
            fftMat.SetTexture("_Input", inputTexture);
            Graphics.Blit(null, outputTexture, fftMat);
        }
        #endregion
        testmat.SetTexture("_Height", heightTexture);
    }
    private void InitRender()
    {
        initialMat.SetFloat("_A", A / 10000f);
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
}
