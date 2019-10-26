using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class usedft : MonoBehaviour
{
    #region MonoBehaviours


    MeshFilter filter;
    Mesh mesh;

    float timer;
    public int resolution;
    public float A;

    private Vector3[] vertices;
    private int[] indices;
    private Vector3[] normals;
    private Vector2[] vertConj;
    private Vector2[] verttilde;
    private Vector3[] vertMeow;
    private Vector2[] uvs;
    private Vector2[] hds;

    private void Update()
    {
        timer += Time.deltaTime;

        EvaluateWaves(timer);
    }

    private void Awake()
    {
        filter = GetComponent<MeshFilter>();
        mesh = new Mesh();
        filter.mesh = mesh;
        SetParams();
        GenerateMesh();
    }

    #endregion
    private void SetParams()
    {
        vertices = new Vector3[resolution * resolution];
        indices = new int[(resolution - 1) * (resolution - 1) * 6];
        normals = new Vector3[resolution * resolution];
        vertConj = new Vector2[resolution * resolution];
        verttilde = new Vector2[resolution * resolution];
        vertMeow = new Vector3[resolution * resolution];//Meow ~ 
        uvs = new Vector2[resolution * resolution];
    }

    private void GenerateMesh()
    {
        int indiceCount = 0;
        int halfResolution = resolution / 2;
        for (int i = 0; i < resolution; i++)
        {
            float horizontalPosition = (i - halfResolution);
            for (int j = 0; j < resolution; j++)
            {
                int currentIdx = i * (resolution) + j;
                float verticalPosition = (j - halfResolution);
                vertices[currentIdx] = new Vector3(horizontalPosition + (resolution % 2 == 0 ? 1 / 2f : 0f), 0f,
                    verticalPosition + (resolution % 2 == 0 ? 1 / 2f : 0f));
                normals[currentIdx] = new Vector3(0f, 1f, 0f);
                verttilde[currentIdx] = htilde0(i, j);
                Vector2 temp = htilde0(resolution - i, resolution - j);
                vertConj[currentIdx] = new Vector2(temp.x, -temp.y);
                uvs[currentIdx] = new Vector2(i * 1.0f / (resolution - 1), j * 1.0f / (resolution - 1));
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
        mesh.vertices = vertices;
        mesh.SetIndices(indices, MeshTopology.Triangles, 0);
        mesh.normals = normals;
        mesh.uv = uvs;
        filter.mesh = mesh;
    }
    private float Phillips(int n, int m)
    {
        Vector2 wind = new Vector2(0.638f, 0.6f);
        float g = 9.8f;
        Vector2 k = new Vector2(Mathf.PI * n / resolution,
            Mathf.PI * m / resolution);
        float klen = k.magnitude;
        float klen2 = klen * klen;
        float klen4 = klen2 * klen2;
        if (klen < 0.00001)
            return 0;
        float kDotW = Vector2.Dot(k.normalized, wind.normalized);
        float kdotW2 = kDotW * kDotW;
        float wlen = wind.magnitude;
        float l = wlen * wlen / g;
        float l2 = l * l;
        //修正系数
        float damping = 0.01f;
        float L2 = l2 * damping * damping;
        float res = A * Mathf.Exp(-1 / (klen2 * l2));
        res = res / klen4 * kdotW2 * Mathf.Exp(-klen2 * L2);
        return res;
    }
    private Vector2 htilde(float t, int x, int y)
    {
        int index = x * resolution + y;
        Vector2 h0 = htilde0(x, y);

        Vector2 h1 = htilde0(-x, -y);
        h1.y *= -1;
        float omegat = Dispersion(x, y) * t;
        float cos_ = Mathf.Cos(omegat);
        float sin_ = Mathf.Sin(omegat);
        Vector2 c0 = new Vector2(cos_, sin_);
        Vector2 c1 = new Vector2(cos_, -sin_);
        Vector2 res = new Vector2(h0.x * c0.x - h0.y * c0.y + h1.x * c1.x - h1.y * c1.y,
                                    h0.x * c0.y + h0.y * c0.x + h1.x * c1.y + h1.y * c1.x);
        return res;
    }
    private float Dispersion(int x, int y)
    {
        float w_0 = 2.0f * Mathf.PI / 200.0f;
        float kx = Mathf.PI * x / resolution;
        float kz = Mathf.PI * y / resolution;
        return Mathf.Floor(Mathf.Sqrt((float)9.8 * Mathf.Sqrt(kx * kx + kz * kz)) / w_0) * w_0;
    }
    private Vector2 htilde0(int x, int y)
    {
        float[] us = NormalDistribution();
        float phi = Mathf.Sqrt(Phillips(x, y) / 2);

        Vector2 res = new Vector2(us[0] * phi, us[1] * phi);
        return res;
    }
    //todo
    public static float[] NormalDistribution()
    {
        //Box Muller方法
        Random rand = new Random();
        float[] y = new float[2];
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
                y[i++] = v1;
            }
            if (i == 2)
                break;
            v2 = a * Mathf.Sin(b);
            if (v2 <= 1 && v2 >= 0)
            {
                y[i++] = v2;
            }
        }
        return y;
    }
    private Vector3 Displacement(Vector2 x, float t, out Vector3 nor)
    {
        Vector2 h = new Vector2(0f, 0f);
        Vector2 d = new Vector2(0f, 0f);
        Vector3 n = Vector3.zero;
        Vector2 c, htilde_c, k;
        float kx, kz, k_length, kDotX;
        for (int i = 0; i < resolution; i++)
        {
            kx = 2 * Mathf.PI * i / resolution;
            for (int j = 0; j < resolution; j++)
            {
                kz = 2 * Mathf.PI * j / resolution;
                k = new Vector2(kx, kz);
                //幅度
                k_length = k.magnitude;
                kDotX = Vector2.Dot(k, x);
                c = new Vector2(Mathf.Cos(kDotX), Mathf.Sin(kDotX));
                Vector2 temp = htilde(t, i, j);
                //虚数乘法
                htilde_c = new Vector2(temp.x * c.x - temp.y * c.y, temp.x * c.y + temp.y * c.x);
                h += htilde_c;
                n += new Vector3(-kx * htilde_c.y, 0f, -kz * htilde_c.y);
                if (k_length < 0.00001)
                    continue;
                d += new Vector2(kx / k_length * htilde_c.y, -kz / k_length * htilde_c.y);
            }
        }
        nor = Vector3.Normalize(Vector3.up - n);
        return new Vector3(d.x, h.x, d.y);
    }
    private void EvaluateWaves(float t)
    {
        hds = new Vector2[resolution * resolution];

        for (int i = 0; i < resolution; i++)
        {
            for (int j = 0; j < resolution; j++)
            {
                int index = i * resolution + j;
                vertMeow[index] = vertices[index];
            }
        }
        for (int i = 0; i < resolution; i++)
        {
            for (int j = 0; j < resolution; j++)
            {
                int index = i * resolution + j;
                Vector3 nor = new Vector3(0f, 0f, 0f);
                Vector3 hd = Displacement(new Vector2(vertMeow[index].x, vertMeow[index].z), t, out nor);
                vertMeow[index].y = hd.y;
                vertMeow[index].z = vertices[index].z - hd.z;// * choppiness;
                vertMeow[index].x = vertices[index].x - hd.x;// * choppiness;
                normals[index] = nor;
                hds[index] = new Vector2(hd.x, hd.z);
            }
        }

        //利用雅可比行列式修改颜色
        /*
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
                    dDdx = 0.5f * (hds[index] - hds[index + resolution]);
                }
                if (j != resolution - 1)
                {
                    dDdy = 0.5f * (hds[index] - hds[index + 1]);
                }
                float jacobian = (1 + dDdx.x) * (1 + dDdy.y) - dDdx.y * dDdy.x;
                Vector2 noise = new Vector2(Mathf.Abs(normals[index].x), Mathf.Abs(normals[index].z)) * 0.3f;
                float turb = Mathf.Max(1f - jacobian + noise.magnitude, 0f);
                float xx = 1f + 3f * Mathf.SmoothStep(1.2f, 1.8f, turb);
                xx = Mathf.Min(turb, 1.0f);
                xx = Mathf.SmoothStep(0f, 1f, turb);
                colors[index] = new Color(xx, xx, xx, xx);
            }
        }
        */
        mesh.vertices = vertMeow;
        mesh.normals = normals;
        //mesh.colors = colors;
    }
}
