using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OceanWithDFT : MonoBehaviour
{
    const float g = 9.8f;
    const float Min = 0.0001f;
    const float sqrt1_2 = 0.7071f;
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
    #endregion

    #region private
    #region 使用FFT时才会赋值
    bool hasDoneFFT;
    Vector2[,] FFT_H;
    Vector2[,] FFT_Dx;
    Vector2[,] FFT_Dz;
    Vector2[,] FFT_Nx;
    Vector2[,] FFT_Nz;
    #endregion
    Vector3[] Normals, Vertices, Verttemp;
    Vector2[,] Htildes, Htilde0s, RandomNums;
    Vector2[] UVs;
    float[,] Ws, Phillips;
    int[] Indices;
    private float timer;
    private int edgelen;
    MeshFilter filter;
    Mesh mesh;
    #endregion

    private void Awake()
    {
        filter = GetComponent<MeshFilter>();
        mesh = new Mesh();
        timer = 0;
        edgelen = 1 << EdgeScale;
        SetParams();
        AwakeRandomNum();
        AwakeVertices();
        AwakePhillips();
        AwakeHtilde0s();
        AwakeWs();
    }
    private void Update()
    {
        if (hasChanged)
        {
            hasChanged = false;
            Awake();
        }
        timer += Speed * Time.deltaTime;
        hasDoneFFT = false;
        UpdateHtilde();

        GenerateMesh();

    }

    #region Awake Init
    private void SetParams()
    {
        Vertices = new Vector3[edgelen * edgelen];
        Verttemp = new Vector3[edgelen * edgelen];
        Indices = new int[(edgelen - 1) * (edgelen - 1) * 6];
        Normals = new Vector3[edgelen * edgelen];
        Htildes = new Vector2[edgelen, edgelen];
        UVs = new Vector2[edgelen * edgelen];
        Ws = new float[edgelen, edgelen];
        RandomNums = new Vector2[edgelen, edgelen];
        Htilde0s = new Vector2[edgelen, edgelen];
        Phillips = new float[edgelen, edgelen];
    }
    private void AwakeVertices()
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
    }
    private void AwakeRandomNum()
    {
        int halfedgelen = edgelen / 2;
        for (int n = -halfedgelen; n < halfedgelen; n++)
        {
            for (int m = -halfedgelen; m < halfedgelen; m++)
            {
                RandomNums[n + halfedgelen, m + halfedgelen] = NormalDistribution();
            }
        }
    }
    private void AwakePhillips()
    {
        Vector2 k;
        float kdotw2;
        float klen2;
        float L = Wind.magnitude * Wind.magnitude / g;
        float klen2l2;
        float klen4;
        int halfedgelen = edgelen / 2;
        for (int n = -halfedgelen; n < halfedgelen; n++)
        {
            for (int m = -halfedgelen; m < halfedgelen; m++)
            {
                k = new Vector2(Mathf.PI * 2 * n / edgelen, Mathf.PI * 2 * m / edgelen);
                kdotw2 = Vector2.Dot(k.normalized, Wind.normalized);
                kdotw2 = kdotw2 * kdotw2;
                if (k.magnitude < Min)
                {
                    continue;
                }
                klen2 = k.magnitude;
                klen2 = klen2 * klen2;
                klen2l2 = klen2 * L * L;
                klen4 = klen2 * klen2;
                Phillips[n + halfedgelen, m + halfedgelen] = A * Mathf.Exp(-1 / klen2l2) * kdotw2 / klen4;
            }
        }
    }
    private void AwakeHtilde0s()
    {
        int halfedgelen = edgelen / 2;
        Vector2 rf;
        float phi;
        for (int n = -halfedgelen; n < halfedgelen; n++)
        {
            for (int m = -halfedgelen; m < halfedgelen; m++)
            {
                rf = RandomNums[n + halfedgelen, m + halfedgelen];
                phi = Phillips[n + halfedgelen, m + halfedgelen];
                phi = Mathf.Sqrt(phi);
                phi *= sqrt1_2;
                Htilde0s[n + halfedgelen, m + halfedgelen] = new Vector2(rf.x * phi, rf.y * phi);
            }
        }
    }
    private void AwakeWs()
    {
        float nlen, mlen;
        int halfedgelen = edgelen / 2;
        for (int n = -halfedgelen; n < halfedgelen; n++)
        {
            for (int m = -halfedgelen; m < halfedgelen; m++)
            {
                nlen = 2 * Mathf.PI * n / edgelen;
                nlen = nlen * nlen;
                mlen = 2 * Mathf.PI * m / edgelen;
                mlen = mlen * mlen;
                Ws[n + halfedgelen, m + halfedgelen] = Mathf.Sqrt(g * Mathf.Sqrt(nlen + mlen));
            }
        }
    }
    public Vector2 NormalDistribution()
    {
        //Box Muller方法
        Vector2 y = new Vector2();
        float v1 = 0, v2 = 0, a, b;
        v1 = Random.Range(0.001f, 1f);
        v2 = Random.Range(0.001f, 1f);
        a = Mathf.Sqrt(-2f * Mathf.Log(v1));
        b = 2 * Mathf.PI * v2;
        v1 = a * Mathf.Cos(b);
        y.x = v1;
        v2 = a * Mathf.Sin(b);
        y.y = v2;
        return y;
    }
    #endregion
    #region Update
    private void UpdateHtilde()
    {
        Vector2 h0, h1, c0, c1;
        float wt, x, y;
        int halfedgelen = edgelen / 2;
        for (int n = -halfedgelen; n < halfedgelen; n++)
        {
            for (int m = -halfedgelen; m < halfedgelen; m++)
            {
                h0 = Htilde0s[halfedgelen + n, halfedgelen + m];
                //[-edgelen/2,edgelen/2)被映射到[0,edgelen],而k取负对应的是n,m取负
                h1 = Htilde0s[halfedgelen - n - 1, halfedgelen - m - 1];
                h1.y *= -1;
                wt = Ws[halfedgelen + n, halfedgelen + m] * timer;
                c0 = new Vector2(Mathf.Cos(wt), Mathf.Sin(wt));
                c1 = new Vector2(-c0.x, -c0.y);
                x = h0.x * c0.x + h1.x * c1.x - h0.y * c0.y - h1.y * c1.y;
                y = h0.y * c0.x + h0.x * c0.y + h1.y * c1.x + h1.x * c1.y;
                Htildes[n + halfedgelen, m + halfedgelen] = new Vector2(x, y);
            }
        }
    }
    private void GenerateMesh()
    {
        int halfedgelen = edgelen / 2;
        //变化发生在Verttemp上
        for (int i = 0; i < edgelen; i++)
        {
            for (int j = 0; j < edgelen; j++)
            {
                int index = i * edgelen + j;
                Verttemp[index] = new Vector3(Vertices[index].x, Vertices[index].y, Vertices[index].z);
            }
        }

        for (int x = 0; x < edgelen; x++)
        {
            for (int z = 0; z < edgelen; z++)
            {
                int currentIdx = x * edgelen + z;
                Vector3 dis, nor;
                if (isForce)
                    dis = getDFTWithForce(x, z, out nor);
                else
                    dis = getFFT(x, z, out nor);
                Verttemp[currentIdx] += dis;
                Normals[currentIdx] = nor;
                UVs[currentIdx] = new Vector2(x * 1.0f / (edgelen - 1), z * 1.0f / (edgelen - 1));

            }
        }


        mesh.vertices = Verttemp;
        mesh.normals = Normals;
        mesh.uv = UVs;
        TopologyWithTriangles();
        filter.mesh = mesh;
    }
    /// <summary>
    /// 修改这个时记得也要修改对应的数组长度，因为不打算把这个做成可交互的，就算了
    /// </summary>
    private Vector3 getDFTWithForce(int x, int z, out Vector3 normal)
    {
        float kx, kz;
        float realpart, imagpart;
        Vector2 ht;
        float modk;
        int halfedgelen = edgelen / 2;
        Vector3 res = new Vector3(0, 0, 0);
        normal = new Vector3(0, 1, 0);
        for (int n = -halfedgelen; n < halfedgelen; n++)
        {
            kx = 2 * Mathf.PI * n / edgelen;
            for (int m = -halfedgelen; m < halfedgelen; m++)
            {
                ht = Htildes[n + halfedgelen, m + halfedgelen];
                kz = 2 * Mathf.PI * m / edgelen;

                realpart = Mathf.Cos(kx * x + kz * z);
                imagpart = Mathf.Sin(kx * x + kz * z);
                //高度
                res.y += ht.x * realpart - ht.y * imagpart;
                //法线
                normal.x = normal.x - kx * (ht.x * imagpart + ht.y * realpart) * -1;
                normal.z = normal.z - kz * (ht.x * imagpart + ht.y * realpart) * -1;
                modk = kx * kx + kz * kz;
                modk = Mathf.Sqrt(modk);
                if (modk < Min)
                {
                    continue;
                }
                //水平位移
                res.x = res.x + kx * (ht.x * imagpart + ht.y * realpart) / modk;
                res.z = res.z + kz * (ht.x * imagpart + ht.y * realpart) / modk;
            }
        }
        res.x *= -Q;
        res.z *= -Q;
        return res;
    }
    private Vector3 getFFT(int x, int z, out Vector3 normal)
    {
        if (!hasDoneFFT)
        {
            hasDoneFFT = true;
            FFT_H = new Vector2[edgelen, edgelen];
            FFT_Dx = new Vector2[edgelen, edgelen];
            FFT_Dz = new Vector2[edgelen, edgelen];
            FFT_Nx = new Vector2[edgelen, edgelen];
            FFT_Nz = new Vector2[edgelen, edgelen];
            int halfedgelen = edgelen / 2;
            float kx, kz;
            Vector2 k;
            Vector2 ht;
            int uaddh;
            int vaddh;
            for (int u = -halfedgelen; u < halfedgelen; u++)
            {
                kx = 2 * Mathf.PI * u / edgelen;
                for (int v = -halfedgelen; v < halfedgelen; v++)
                {
                    kz = 2 * Mathf.PI * v / edgelen;
                    k = new Vector2(kx, kz);
                    uaddh = u + halfedgelen;
                    vaddh = v + halfedgelen;
                    ht = Htildes[uaddh, vaddh];
                    FFT_H[uaddh, vaddh] = new Vector2(ht.x, ht.y);

                    if (k.magnitude < Min)
                    {
                        FFT_Nx[uaddh, vaddh] = new Vector2(0, 0);
                        FFT_Nz[uaddh, vaddh] = new Vector2(0, 0);
                        FFT_Dx[uaddh, vaddh] = new Vector2(0, 0);
                        FFT_Dz[uaddh, vaddh] = new Vector2(0, 0);
                    }
                    else
                    {
                        FFT_Nx[uaddh, vaddh] = new Vector2(-ht.y * kx, ht.x * kx);
                        FFT_Nz[uaddh, vaddh] = new Vector2(-ht.y * kz, ht.x * kz);
                        FFT_Dx[uaddh, vaddh] = new Vector2(ht.y * kx / k.magnitude, -ht.x * kx / k.magnitude);
                        FFT_Dz[uaddh, vaddh] = new Vector2(ht.y * kz / k.magnitude, -ht.x * kz / k.magnitude);
                    }
                    FFT_H[uaddh, vaddh].y *= -1;
                    FFT_Nx[uaddh, vaddh].y *= -1;
                    FFT_Nz[uaddh, vaddh].y *= -1;
                    FFT_Dx[uaddh, vaddh].y *= -1;
                    FFT_Dz[uaddh, vaddh].y *= -1;
                }
            }
            FFTHelper.FFT(FFT_H, edgelen);
            FFTHelper.FFT(FFT_Nx, edgelen);
            FFTHelper.FFT(FFT_Nz, edgelen);
            FFTHelper.FFT(FFT_Dx, edgelen);
            FFTHelper.FFT(FFT_Dz, edgelen);
        }
        normal = new Vector3(0, 1, 0) - new Vector3(FFT_Nx[x, z].x, 0, FFT_Nz[x, z].x);
        return new Vector3(FFT_Dx[x, z].x * -Q, FFT_H[x, z].x, FFT_Dz[x, z].x * -Q);
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
    #endregion
}
