using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mysea : MonoBehaviour
{
    MeshFilter filter;
    Mesh mesh;
    float timer;
    public GameObject pre;
    private List<GameObject> allpres = new List<GameObject>();
    const float min = 0.00001f;
    private Vector2[,] RandomNum;
    private Vector2[,] Htildes;
    private Vector2[,] H0s;
    private Vector3[,] Ds;

    private Vector2[,] FFT_Hs;
    private Vector2[,] FFT_Dxs;
    private Vector2[,] FFT_Dzs;
    private Vector2[,] FFT_Normalxs;
    private Vector2[,] FFT_Normalzs;
    private Vector2[,] FFT_Bxs;
    private Vector2[,] FFT_Bys;
    private Vector2[,] FFT_Bzs;
    private Vector2[,] FFT_Txs;
    private Vector2[,] FFT_Tys;
    private Vector2[,] FFT_Tzs;
    private float[,] ws;
    //风向
    private Vector2 cwind = new Vector2(0.638f, 0.6f).normalized;
    //风速
    public float v = 5;
    public float q;
    private float l;

    private Vector2[] uvs;
    private Vector3[] vertices;
    private Vector3[] verttemp;
    private int[] indices;
    private Vector3[] normals;
    [Range(0, 10)]
    public int Resolution = 6;
    private int resolution;
    [SerializeField]
    [Range(0, 2f)] 
    private float Steepness = 0.5f;
    [SerializeField]
    private float Speed = 1f;
    public float JacoValue = 1f;
    private void Awake()
    {
        filter = GetComponent<MeshFilter>();
        mesh = new Mesh();
        timer = 0;
        resolution = 1 << Resolution;
        SetParams();
        AwakeRandomNum();
        AwakeVertices();
    }
    private void Update()
    {
        for (int i = 0; i < allpres.Count; i++)
        {
            Destroy(allpres[i]);
        }
        var s = Random.state;
        l = v * v / 9.8f;
        timer += Speed * Time.deltaTime;
        Random.state = s;

        UpdateH0();
        UpdateW();
        UpdateHtilde();
        FFTHelper.tfftForHDN(Htildes, resolution, out FFT_Hs, out FFT_Dxs, out FFT_Dzs,
            out FFT_Normalxs, out FFT_Normalzs, out FFT_Bxs, out FFT_Bys, out FFT_Bzs,
            out FFT_Txs, out FFT_Tys, out FFT_Tzs);
        GenerateMesh();

    }
    private void SetParams()
    {
        vertices = new Vector3[resolution * resolution];
        verttemp = new Vector3[resolution * resolution];
        indices = new int[(resolution - 1) * (resolution - 1) * 6];
        normals = new Vector3[resolution * resolution];
        H0s = new Vector2[resolution, resolution];
        Htildes = new Vector2[resolution, resolution];
        uvs = new Vector2[resolution * resolution];
        ws = new float[resolution, resolution];
        RandomNum = new Vector2[resolution, resolution];
        Ds = new Vector3[resolution, resolution];
    }
    private float Phillips(float n, float m)
    {
        Vector2 wind = cwind * v;
        Vector2 k = new Vector2(Mathf.PI * (2 * n - resolution) / resolution, Mathf.PI * (2 * m - resolution) / resolution);
        float kdotw2 = Vector2.Dot(k, wind);
        kdotw2 = kdotw2 * kdotw2;
        float klen2 = k.magnitude;

        klen2 = klen2 * klen2;
        float klen2l2 = klen2 * l * l;
        float klen4 = klen2 * klen2;
        float left = Steepness * Mathf.Exp(-1f / klen2l2) / klen4 * kdotw2;
        return left;
    }
    private void UpdateH0()
    {
        int halfresolution = resolution / 2;
        for (int n = -halfresolution; n < halfresolution; n++)
        {
            for (int m = -halfresolution; m < halfresolution; m++)
            {
                H0s[n + halfresolution, m + halfresolution] = htilde0(n, m);
            }
        }
    }
    private void UpdateHtilde()
    {
        int halfresolution = resolution / 2;
        for (int n = -halfresolution; n < halfresolution; n++)
        {
            for (int m = -halfresolution; m < halfresolution; m++)
            {
                Htildes[n + halfresolution, m + halfresolution] = htilde(timer, n, m);
            }
        }
    }
    private void AwakeVertices()
    {
        int halfResolution = resolution / 2;
        for (int i = 0; i < resolution; i++)
        {
            //水平位置在-halfResolution * unitWidth与其相反数之间
            float horizontalPosition = (i - halfResolution);
            for (int j = 0; j < resolution; j++)
            {
                int currentIdx = i * (resolution) + j;
                float verticalPosition = (j - halfResolution);
                vertices[currentIdx] = new Vector3(
                    horizontalPosition + (resolution % 2 == 0 ? 0.5f : 0f),
                    0,
                    verticalPosition + (resolution % 2 == 0 ? 0.5f : 0f));
                normals[currentIdx] = new Vector3(0, 1f, 0);
                uvs[currentIdx] = new Vector2(i * 1.0f / (resolution - 1), j * 1.0f / (resolution - 1));
            }
        }
    }
    private void AwakeRandomNum()
    {
        int halfresolution = resolution / 2;
        for (int n = -halfresolution; n < halfresolution; n++)
        {
            for (int m = -halfresolution; m < halfresolution; m++)
            {
                RandomNum[n + halfresolution, m + halfresolution] = NormalDistribution();
            }
        }
    }
    private void UpdateW()
    {
        int halfresolution = resolution / 2;
        for (int n = -halfresolution; n < halfresolution; n++)
        {
            for (int m = -halfresolution; m < halfresolution; m++)
            {
                ws[n + halfresolution, m + halfresolution] = W(n, m);
            }
        }
    }
    private Vector2 htilde0(int n, int m)
    {
        int halfresolution = resolution / 2;
        Vector2 rf = RandomNum[n + halfresolution, m + halfresolution];
        float sqrt1_2 = 1f / Mathf.Sqrt(2);
        float phi = Phillips(n, m);
        return new Vector2(rf.x * sqrt1_2 * phi, rf.y * sqrt1_2 * phi);
    }
    public Vector2 NormalDistribution()
    {
        //Box Muller方法
        Vector2 y = new Vector2();
        float v1 = 0, v2 = 0, a, b;
        for (int i = 0; i < 2;)
        {
            v1 = Random.Range(0.001f, 1f);
            v2 = Random.Range(0.001f, 1f);
            a = Mathf.Sqrt(-2f * Mathf.Log(v1));
            b = 2 * Mathf.PI * v2;
            v1 = a * Mathf.Cos(b);
            if (v1 <= 1 && v1 >= 0)
            {
                y.x = v1;
                i++;
            }
            if (i == 2)
                break;
            v2 = a * Mathf.Sin(b);
            if (v2 <= 1 && v2 >= 0)
            {
                y.y = v2;
                i++;
            }
        }
        return y;
    }
    /// <summary>
    /// 返回值是复数
    /// </summary>
    private Vector2 htilde(float t, int n, int m)
    {
        int halfresolution = resolution / 2;
        Vector2 h0 = H0s[halfresolution + n, halfresolution + m];
        Vector2 h1 = H0s[halfresolution + n, halfresolution + m];
        h1.y *= -1;
        float wt = ws[halfresolution + n, halfresolution + m] * t;
        Vector2 c0 = new Vector2(Mathf.Cos(wt), Mathf.Sin(wt));
        Vector2 c1 = new Vector2(-c0.x, -c0.y);
        float x = h0.x * c0.x + h1.x * c1.x - h0.y * c0.y - h1.y * c1.y;
        float y = h0.y * c0.x + h0.x * c0.y + h1.y * c1.x + h1.x * c1.y;
        return new Vector2(x, y);
    }
    private float W(int n, int m)
    {
        float nlen = 2 * Mathf.PI * n / resolution;
        nlen = nlen * nlen;
        float mlen = 2 * Mathf.PI * m / resolution;
        mlen = mlen * mlen;
        float res = Mathf.Sqrt(nlen + mlen);
        res *= 9.8f;
        return Mathf.Sqrt(res);
    }
    private void GenerateMesh()
    {
        int halfResolution = resolution / 2;
        for (int i = 0; i < resolution; i++)
        {
            for (int j = 0; j < resolution; j++)
            {
                int index = i * resolution + j;
                verttemp[index] = new Vector3(vertices[index].x, vertices[index].y, vertices[index].z);
            }
        }
        for (int i = 0; i < resolution; i++)
        {
            for (int j = 0; j < resolution; j++)
            {
                int currentIdx = i * (resolution) + j;

                Vector3 normal;
                #region FFT 
                Vector3 dis = new Vector3(FFT_Dxs[i, j].x, FFT_Hs[i, j].x, FFT_Dzs[i, j].x);
                Vector3 B = new Vector3(FFT_Bxs[i, j].x, FFT_Bys[i, j].x, FFT_Bzs[i, j].x);
                B.x *= -q;
                B.x += 1;
                B.z *= -q;
                Vector3 T = new Vector3(FFT_Txs[i, j].x, FFT_Tys[i, j].x, FFT_Tzs[i, j].x);
                T.x *= -q;
                T.z *= -q;
                T.z += 1;
                normal = Vector3.Cross(T, B).normalized;
                //normal = Vector3.up - new Vector3(FFT_Normalxs[i, j].x, 0, FFT_Normalzs[i, j].x);
                normals[currentIdx] = normal;
                #endregion
                verttemp[currentIdx].x += dis.x * -q;
                verttemp[currentIdx].y += dis.y;
                verttemp[currentIdx].z += dis.z * -q;
                //雅可比行列式要用
                Ds[i, j] = dis;
                uvs[currentIdx] = new Vector2(i * 1.0f / (resolution - 1), j * 1.0f / (resolution - 1));

                #region 法线debug
                GameObject preobj;
                if (i % Resolution/2 == 0 && j % Resolution/2 == 0)
                {
                    preobj = GameObject.Instantiate(pre);
                    allpres.Add(preobj);
                    preobj.transform.position = verttemp[currentIdx];
                    preobj.transform.up = normals[currentIdx];
                }
                #endregion
            }
        }
        #region jaco
        Color[] colors = new Color[resolution * resolution];
        /*
        for (int i = 0; i < resolution; i++)//写得并不正确,
        {
            for (int j = 0; j < resolution; j++)
            {
                int index = i * resolution + j;
                Vector2 dDdx = Vector2.zero;
                Vector2 dDdy = Vector2.zero;
                if (i != resolution - 1)
                {
                    dDdx = (Ds[i, j] - Ds[i + 1, j]);
                }
                if (j != resolution - 1)
                {
                    dDdy = (Ds[i, j] - Ds[i, j + 1]);
                }
                float jacobian = (1 + dDdx.x) * (1 + dDdy.y) - dDdx.y * dDdy.x;
                if (jacobian > JacoValue)
                {
                    float xx = Mathf.SmoothStep(0f, 1f, jacobian);
                    colors[index] = new Color(xx, xx, xx, 1.0f);
                }
                else
                {
                    //colors[index] = new Color(0, 0, 0);
                    colors[index] = new Color(0.156863f, 0.89804f, 1f, 1f);
                }

                //Vector2 noise = new Vector2(Mathf.Abs(normals[index].x), Mathf.Abs(normals[index].z)) * 0.3f;
                //float turb = Mathf.Max(1f - jacobian + noise.magnitude, 0f);
                //float xx = Mathf.SmoothStep(0f, 1f, turb);
                //colors[index] = new Color(xx, xx, xx, xx);
            }
        }
        */
        #endregion
        mesh.vertices = verttemp;
        mesh.normals = normals;
        mesh.uv = uvs;
        mesh.colors = colors;
        int indiceCount = 0;
        #region 面
        for (int i = 0; i < resolution; i++)
        {
            for (int j = 0; j < resolution; j++)
            {
                int currentIdx = i * resolution + j;
                if (j == resolution - 1)
                    continue;
                if (i != resolution - 1)
                {
                    indices[indiceCount++] = currentIdx;
                    indices[indiceCount++] = currentIdx + 1;
                    indices[indiceCount++] = currentIdx + resolution;
                }
                if (i != 0)
                {
                    indices[indiceCount++] = currentIdx;
                    indices[indiceCount++] = currentIdx - resolution + 1;
                    indices[indiceCount++] = currentIdx + 1;
                }
            }
        }
        #endregion
        mesh.SetIndices(indices, MeshTopology.Triangles, 0);
        filter.mesh = mesh;
    }
}
