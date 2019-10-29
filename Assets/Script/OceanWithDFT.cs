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
    #endregion

    #region private
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
        for (int i = 0; i < edgelen; i++)
        {
            for (int j = 0; j < edgelen; j++)
            {
                int currentIdx = i * edgelen + j;
                #region DFT
                Vector3 dis = new Vector3(0, 0, 0);
                float height = 0;
                Vector3 nor = new Vector3(0, 1, 0);
                float kx, kz;
                float realpart, imagpart;
                Vector2 ht;
                float modk;
                for (int n = -halfedgelen; n < halfedgelen; n++)
                {
                    kx = 2 * Mathf.PI * n / edgelen;
                    for (int m = -halfedgelen; m < halfedgelen; m++)
                    {
                        ht = Htildes[n + halfedgelen, m + halfedgelen];
                        kz = 2 * Mathf.PI * m / edgelen;

                        realpart = Mathf.Cos(kx * i + kz * j);
                        imagpart = Mathf.Sin(kx * i + kz * j);
                        //高度
                        height += ht.x * realpart - ht.y * imagpart;
                        //法线
                        nor.x = nor.x - kx * (ht.x * imagpart + ht.y * realpart) * -1;
                        nor.z = nor.z - kz * (ht.x * imagpart + ht.y * realpart) * -1;
                        modk = kx * kx + kz * kz;
                        modk = Mathf.Sqrt(modk);
                        if (modk < Min)
                        {
                            continue;
                        }
                        //水平位移
                        dis.x = dis.x + kx * (ht.x * imagpart + ht.y * realpart) / modk;
                        dis.z = dis.z + kz * (ht.x * imagpart + ht.y * realpart) / modk;
                    }
                }
                Verttemp[currentIdx].x += dis.x * -Q;
                Verttemp[currentIdx].y += height;
                Verttemp[currentIdx].z += dis.z * -Q;
                Normals[currentIdx] = nor;
                UVs[currentIdx] = new Vector2(i * 1.0f / (edgelen - 1), j * 1.0f / (edgelen - 1));
                #endregion
            }
        }
        mesh.vertices = Verttemp;
        mesh.normals = Normals;
        mesh.uv = UVs;
        #region 面
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
        filter.mesh = mesh;
        #endregion
    }
    #endregion
}
