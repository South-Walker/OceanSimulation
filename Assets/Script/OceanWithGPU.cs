using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class OceanWithGPU : MonoBehaviour
{
    #region public
    public bool hasChanged = false;
    public float Speed = 1f;
    public int ImgScale = 10;
    [Range(1, 8)] 
    public int EdgeScale;
    public float Q = 1f;
    [SerializeField]
    Vector2 Wind = new Vector2();
    [Range(0f, 0.001f)]
    public float A;
    public float WhiteCapThreshold = 0.25f;
    public bool isForce = true;
    #region Shaders
    public Shader initialShader;
    public Shader omegaktShader;
    public Shader htildeShader;
    public Shader displaceShader;
    public Shader normalShader;
    public Shader whitecapShader;
    public Shader fftShader;
    #endregion
    #region Material
    private Material initialMat;
    private Material omegaktMat;
    private Material htildeMat;
    private Material displaceMat;
    private Material normalMat;
    private Material whitecapMat;
    private Material fftMat;
    public Material TargetMat;
    #endregion

    #endregion
    #region RenderTexture
    public RenderTexture initialTexture;
    private RenderTexture omegaktTexture;
    private RenderTexture htildeTexture;
    private RenderTexture heightTexture;
    private RenderTexture displaceTexture;
    private RenderTexture whitecapTexture;
    private RenderTexture normalTexture;
    private RenderTexture tempa;
    private RenderTexture tempb;
    #endregion
    #region private
    private int edgelen;
    private int imglen;
    private float timer = 0f;
    private Vector3[] Vertices;
    private Vector2[] UVs;
    private Vector3[] Normals;
    private int[] Indices;
    private MeshFilter filter;
    private Mesh mesh;

    #endregion
    void Start()
    {
        SetParams();
        SetVertices();
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
        omegaktMat.SetInt("_Len", imglen);
        
        Graphics.Blit(null, omegaktTexture, omegaktMat);

        htildeMat.SetTexture("_Init", initialTexture);
        htildeMat.SetTexture("_OmegaKT", omegaktTexture);
        Graphics.Blit(null, htildeTexture, htildeMat);
        fftMat.SetFloat("_Len", imglen);
        #region 渲染高度场
        int iterations = ImgScale * 2;
        RenderTexture outputTexture = null, inputTexture = null;
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
        //testmat.SetTexture("_Height", heightTexture);
        #region 渲染水平位移
        displaceMat.SetTexture("_Htilde", htildeTexture);
        displaceMat.SetFloat("_Q", Q);
        displaceMat.SetInt("_Len", imglen);
        Graphics.Blit(null, displaceTexture, displaceMat);
        fftMat.EnableKeyword("_HORIZONTAL");
        fftMat.DisableKeyword("_VERTICAL");
        for (int i = 0; i < iterations; i++)
        {
            outputTexture = (i % 2 == 0) ? tempa : tempb;
            inputTexture = (i % 2 == 1) ? tempa : tempb;
            fftMat.SetFloat("_SubLen", Mathf.Pow(2, (i % (iterations / 2)) + 1));
            if (i == 0)
            {
                inputTexture = displaceTexture;
            }
            if (i == iterations - 1)
            {
                outputTexture = displaceTexture;
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
        normalMat.SetInt("_Len", imglen);
        normalMat.SetTexture("_Height", heightTexture);
        normalMat.SetTexture("_Displace", displaceTexture);
        Graphics.Blit(null, normalTexture, normalMat);
        whitecapMat.SetTexture("_Displace", displaceTexture);
        whitecapMat.SetFloat("_Q", Q);
        whitecapMat.SetFloat("_Threshold", WhiteCapThreshold);
        Graphics.Blit(null, whitecapTexture, whitecapMat);
        TargetMat.SetTexture("_Height", heightTexture);
        TargetMat.SetTexture("_Displace", displaceTexture);
        TargetMat.SetTexture("_Normal", normalTexture);
        TargetMat.SetTexture("_WhiteCap", whitecapTexture);
    }
    #region start
    private void SetVertices()
    {
        int halfedgelen = edgelen / 2;
        for (int i = 0; i < edgelen; i++)
        {
            //水平位置在-halfResolution * unitWidth与其相反数之间
            float horizontalPosition = (i - halfedgelen);
            for (int j = 0; j < edgelen; j++)
            {
                int currentIdx = i * edgelen + j;
                float verticalPosition = (j - halfedgelen);
                Vertices[currentIdx] = new Vector3(
                    horizontalPosition + (edgelen % 2 == 0 ? 0.5f : 0f),
                    0,
                    verticalPosition + (edgelen % 2 == 0 ? 0.5f : 0f));
                Normals[currentIdx] = new Vector3(0, 1f, 0);
                UVs[currentIdx] = new Vector2(i * 1.0f / (edgelen - 1), j * 1.0f / (edgelen - 1));
            }
        }
        mesh.vertices = Vertices;
        mesh.normals = Normals;
        mesh.uv = UVs;
        TopologyWithTriangles();
        filter.mesh = mesh;
    }
    private void TopologyWithTriangles()
    {
        int indiceCount = 0;
        for (int i = 0; i < edgelen; i++)
        {
            for (int j = 0; j < edgelen; j++)
            {
                int currentIdx = i * edgelen + j;
                if (j == edgelen - 1)
                    continue;
                if (i != edgelen - 1)
                {
                    Indices[indiceCount++] = currentIdx;
                    Indices[indiceCount++] = currentIdx + 1;
                    Indices[indiceCount++] = currentIdx + edgelen;
                }
                if (i != 0)
                {
                    Indices[indiceCount++] = currentIdx;
                    Indices[indiceCount++] = currentIdx - edgelen + 1;
                    Indices[indiceCount++] = currentIdx + 1;
                }
            }
        }
        mesh.SetIndices(Indices, MeshTopology.Triangles, 0);
    }
    private void InitRender()
    {
        initialMat.SetFloat("_A", A / 10000f);
        initialMat.SetInt("_Len", imglen);
        initialMat.SetVector("_Wind", Wind);
        Graphics.Blit(null, initialTexture, initialMat);
    }
    private void SetParams()
    {
        imglen = 1 << ImgScale;
        edgelen = 1 << EdgeScale;

        Vertices = new Vector3[edgelen * edgelen];
        UVs = new Vector2[edgelen * edgelen];
        Normals = new Vector3[edgelen * edgelen];
        Indices = new int[(edgelen - 1) * (edgelen - 1) * 6];
        filter = GetComponent<MeshFilter>();
        mesh = new Mesh();

        initialMat = new Material(initialShader);
        omegaktMat = new Material(omegaktShader);
        htildeMat = new Material(htildeShader);
        displaceMat = new Material(displaceShader);
        normalMat = new Material(normalShader);
        whitecapMat = new Material(whitecapShader);
        fftMat = new Material(fftShader);

        initialTexture = new RenderTexture(imglen, imglen, 0, RenderTextureFormat.ARGBFloat);
        omegaktTexture = new RenderTexture(imglen, imglen, 0, RenderTextureFormat.RFloat);
        htildeTexture = new RenderTexture(imglen, imglen, 0, RenderTextureFormat.ARGBFloat);
        tempa = new RenderTexture(imglen, imglen, 0, RenderTextureFormat.ARGBFloat);
        tempb = new RenderTexture(imglen, imglen, 0, RenderTextureFormat.ARGBFloat);
        heightTexture = new RenderTexture(imglen, imglen, 0, RenderTextureFormat.ARGBFloat);
        displaceTexture = new RenderTexture(imglen, imglen, 0, RenderTextureFormat.ARGBFloat);
        normalTexture = new RenderTexture(imglen, imglen, 0, RenderTextureFormat.ARGBFloat);
        whitecapTexture = new RenderTexture(imglen, imglen, 0, RenderTextureFormat.ARGBFloat);
    }
    #endregion
}
