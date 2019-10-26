using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mysea : MonoBehaviour
{
    MeshFilter filter;
    Mesh mesh;
    float timer;

    const float min = 0.0001f;
    private Vector2[,] RandomNum;
    private Vector2[,] Htildes;
    private Vector2[,] H0s;
    private Vector3[,] Ds;

    private Vector2[,] FFT_Hs;
    private Vector2[,] FFT_Dxs;
    private Vector2[,] FFT_Dzs;
    private Vector2[,] FFT_Normalxs;
    private Vector2[,] FFT_Normalzs;
    private float[,] ws;
    //风向
    private Vector2 wind = new Vector2(0.638f, 0.6f);
    //风速
    public float v = 5;
    //xoz平面位移剧烈程度
    public float choppiness = 1;
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
    [Range(0, 1f)] 
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
        var s = Random.state;
        l = v * v / 9.8f;
        timer += Speed * Time.deltaTime;
        Random.state = s;

        UpdateH0();
        UpdateW();
        UpdateHtilde();
        FFT.tfftForHDN(Htildes, resolution, out FFT_Hs, out FFT_Dxs, out FFT_Dzs, out FFT_Normalxs, out FFT_Normalzs);
        GenerateMesh();
    }
    private void SetParams()
    {
        vertices = new Vector3[resolution * resolution];
        verttemp = new Vector3[resolution * resolution];
        indices = new int[(resolution - 1) * (resolution - 1) * 4];
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
        Vector2 k = new Vector2(2 * Mathf.PI * n / resolution, 2 * Mathf.PI * m / resolution);
        float kdotw2 = Vector2.Dot(k, wind);
        kdotw2 = kdotw2 * kdotw2;
        float klen2 = k.magnitude;
        if (klen2 < min)
        {
            return 0;
        }
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
    /// <param name="t"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    private Vector2 htilde(float t, int n, int m)
    {
        int halfresolution = resolution / 2;
        Vector2 h0 = H0s[halfresolution + n, halfresolution + m];
        Vector2 h1 = H0s[halfresolution - n - 1, halfresolution - m - 1];
        h1.y *= -1;
        float wt = ws[halfresolution + n, halfresolution + m] * t;
        Vector2 c0 = new Vector2(Mathf.Cos(wt), Mathf.Sin(wt));
        Vector2 c1 = new Vector2(c0.x, -c0.y);
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
    private Vector3 Displacement(float x, float y, float t, out Vector3 normal)
    {
        int halfresolution = resolution / 2;
        Vector2 ht;
        Vector2 k;
        Vector2 expc;
        Vector2 htexpc;
        Vector3 nor = new Vector3(0, 0, 0);
        float kdotx;
        float klen;
        float kx, ky;
        Vector3 res = new Vector3(0, 0, 0);
        for (int n = -halfresolution; n < halfresolution; n++)
        {
            kx = 2 * Mathf.PI * n / resolution;
            for (int m = -halfresolution; m < halfresolution; m++)
            {
                ky = 2 * Mathf.PI * m / resolution;
                k = new Vector2(kx, ky);
                klen = k.magnitude;
                if (klen < min)
                {
                    continue;
                }
                kdotx = k.x * x + k.y * y;
                expc = new Vector2(Mathf.Cos(kdotx), Mathf.Sin(kdotx));
                ht = Htildes[n + halfresolution, m + halfresolution];
                htexpc = new Vector2(ht.x * expc.x - ht.y * expc.y,
                    ht.x * expc.y + ht.y * expc.x);
                //只有虚部乘以i才是实部
                res.x += kx / klen * htexpc.y;
                res.y += htexpc.x;
                res.z += ky / klen * htexpc.y;
                //只有虚部乘以i才是实部
                nor += new Vector3(-kx * htexpc.y, 0f, -ky * htexpc.y);
            }
        }
        normal = Vector3.up - nor;
        return res;
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
                #region 暴力解DFT
                //Vector3 dis = Displacement(i, j, timer, out normal);
                #endregion
                #region FFT 
                Vector3 dis = new Vector3(FFT_Dxs[i, j].x, FFT_Hs[i, j].x, FFT_Dzs[i, j].x);
                normal = Vector3.up - new Vector3(FFT_Normalxs[i, j].x, 0, FFT_Normalzs[i, j].x);
                #endregion
                verttemp[currentIdx].x += dis.x * -choppiness;
                verttemp[currentIdx].y += dis.y;
                verttemp[currentIdx].z += dis.z * -choppiness;
                //雅可比行列式要用
                Ds[i, j] = dis;
                normals[currentIdx] = normal;
                uvs[currentIdx] = new Vector2(i * 1.0f / (resolution - 1), j * 1.0f / (resolution - 1));
            }
        }
        Color[] colors = new Color[resolution * resolution];

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
        mesh.vertices = verttemp;
        mesh.normals = normals;
        mesh.uv = uvs;
        mesh.colors = colors;
        int indicescount = 0;
        #region 面
        
        for (int i = 1; i < resolution; i++)
        {
            for (int j = 1; j < resolution; j++)
            {
                int currentIdx = i * (resolution) + j;
                indices[indicescount++] = currentIdx - 1 - resolution;
                indices[indicescount++] = currentIdx - resolution;
                indices[indicescount++] = currentIdx;
                indices[indicescount++] = currentIdx - 1;
            }
        }
        
        #endregion
        #region 线
        /*
        indices = new int[4 * resolution * resolution - 4 * resolution];
        for (int i = 1; i < resolution; i++)
        {
            for (int j = 1; j < resolution; j++)
            {
                int currentIdx = i * resolution + j;
                indices[indicescount++] = currentIdx;
                indices[indicescount++] = currentIdx - resolution;
                indices[indicescount++] = currentIdx;
                indices[indicescount++] = currentIdx - 1;
            }
        }
        for (int i = 1; i < resolution; i++)
        {
            indices[indicescount++] = i;
            indices[indicescount++] = i - 1;
        }
        for (int i = 1; i < resolution; i++)
        {
            indices[indicescount++] = i * resolution;
            indices[indicescount++] = i * resolution - resolution;
        }
        */
        #endregion
        mesh.SetIndices(indices, MeshTopology.Quads, 0);
        //mesh.SetIndices(indices, MeshTopology.Points, 0);
        //mesh.SetIndices(indices, MeshTopology.Lines, 0);
        filter.mesh = mesh;
    }
}
