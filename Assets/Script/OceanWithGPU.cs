using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OceanWithGPU : MonoBehaviour
{
    #region public
    public bool hasChanged = false;
    public float Speed = 1f;
    public int EdgeScale;
    public float Q = 1f;
    [SerializeField]
    Vector2 Wind;
    [Range(0f, 1f)]
    public float A;
    public bool isForce = true;
    #region Shaders
    public Shader initialShader;
    #endregion
    #region Material
    private Material initialMat;
    #endregion
    #endregion
    #region RenderTexture
    public RenderTexture initialTexture;
    #endregion

    private int edgelen;
    // Start is called before the first frame update
    void Start()
    {
        edgelen = 1 << EdgeScale;
        initialMat = new Material(initialShader);
        initialTexture = new RenderTexture(edgelen, edgelen, 0, RenderTextureFormat.ARGBFloat);
        initialMat.SetFloat("_A", A);
        initialMat.SetInt("_Len", edgelen);
        initialMat.SetVector("_Wind", Wind);
        Graphics.Blit(null, initialTexture, initialMat);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
