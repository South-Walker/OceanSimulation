using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComplexNumber
{
    public float ImaginaryPart;
    public float ResistivePart;
    public Vector2 toV2()
    {
        return new Vector2(ResistivePart, ImaginaryPart);
    }
    public ComplexNumber(Vector2 v2)
    {
        ResistivePart = v2.x;
        ImaginaryPart = v2.y;
    }
    public static ComplexNumber operator +(ComplexNumber a, ComplexNumber b)
    {
        ComplexNumber r = new ComplexNumber(a.ImaginaryPart + b.ImaginaryPart, a.ResistivePart + b.ResistivePart);
        return r;
    }
    public static ComplexNumber operator *(ComplexNumber a, ComplexNumber b)
    {
        ComplexNumber r = new ComplexNumber(a);
        r.Multiply(b);
        return r;
    }
    public static ComplexNumber operator -(ComplexNumber a, ComplexNumber b)
    {
        ComplexNumber r = new ComplexNumber(a.ImaginaryPart - b.ImaginaryPart, a.ResistivePart - b.ResistivePart);
        return r;
    }
    public void SetZero()
    {
        ImaginaryPart = 0;
        ResistivePart = 0;
    }
    public void Conjugate()
    {
        ImaginaryPart *= -1;
    }
    public ComplexNumber(float imaginary, float resistive)
    {
        ImaginaryPart = imaginary;
        ResistivePart = resistive;
    }
    public ComplexNumber(ComplexNumber cn)
    {
        ImaginaryPart = cn.ImaginaryPart;
        ResistivePart = cn.ResistivePart;
    }
    public ComplexNumber()
    {
        ImaginaryPart = 0;
        ResistivePart = 0;
    }
    public void Multiply(float a)
    {
        ImaginaryPart *= a;
        ResistivePart *= a;
    }
    public void Multiply(ComplexNumber a)
    {
        float t;
        t = a.ResistivePart * ImaginaryPart + ResistivePart * a.ImaginaryPart;
        ResistivePart = a.ResistivePart * ResistivePart - a.ImaginaryPart * ImaginaryPart;
        ImaginaryPart = t;
    }
    public void Divide(float a)
    {
        ImaginaryPart /= a;
        ResistivePart /= a;
    }
    public void Add(ComplexNumber a)
    {
        ImaginaryPart += a.ImaginaryPart;
        ResistivePart += a.ResistivePart;
    }
}
public static class FFT
{
    const float MIN = 0.001f;
    public static Vector2 w(float k, float n)
    {
        Vector2 r = new Vector2();
        float temp = 2 * Mathf.PI * k / n;
        r.x = Mathf.Cos(temp);
        r.y = -Mathf.Sin(temp);
        return r;
    }
    public static void tfftForHDN(Vector2[,] htilde, int count, out Vector2[,] hs,
        out Vector2[,] dxs, out Vector2[,] dzs,
        out Vector2[,] normalxs, out Vector2[,] normalzs)
    {
        hs = new Vector2[count, count];
        dxs = new Vector2[count, count];
        dzs = new Vector2[count, count];
        normalxs = new Vector2[count, count];
        normalzs = new Vector2[count, count];
        Vector2 k;
        Vector2 ctemp;
        Vector2 newvalue;
        Vector2[] cntemp = new Vector2[count];
        float kx, kz;
        int groupnum = count;
        int unum = 1;
        int[] us = new int[count];
        int[] utemp = new int[count];
        int now;
        for (int x = 0; x < count; x++)
        {
            kx = 2 * Mathf.PI * x / count;
            for (int y = 0; y < count; y++)
            {
                kz = 2 * Mathf.PI * y / count;
                k = new Vector2(kx, kz);
                hs[x, y] = htilde[x, y];

                normalxs[x, y].x = -htilde[x, y].y * kx;
                normalxs[x, y].y = htilde[x, y].x * kx;
                normalzs[x, y].x = -htilde[x, y].y * kz;
                normalzs[x, y].y = htilde[x, y].x * kz;


                if (k.magnitude < MIN)
                {
                    dxs[x, y].x = 0;
                    dxs[x, y].y = 0;
                    dzs[x, y].x = 0;
                    dzs[x, y].y = 0;
                }
                else
                {
                    dxs[x, y].x = htilde[x, y].y * kx / k.magnitude;
                    dxs[x, y].y = htilde[x, y].x * kx / k.magnitude * -1;
                    dzs[x, y].x = htilde[x, y].y * kz / k.magnitude;
                    dzs[x, y].y = htilde[x, y].x * kz / k.magnitude * -1;
                }
            }
        }
        for (int x = 0; x < count; x++)
        {
            cntemp = new Vector2[count];
            groupnum = count;
            unum = 1;
            us = new int[count];
            utemp = new int[count];
            while (unum != count)
            {
                now = 0;
                for (int i = 0; i < unum; i++)
                {
                    for (int j = 0; j < groupnum / 2; j++)
                    {
                        utemp[now++] = us[i * groupnum];
                    }
                    for (int j = 0; j < groupnum / 2; j++)
                    {
                        utemp[now++] = us[i * groupnum] + unum;
                    }
                }
                for (int i = 0; i < count; i++)
                {
                    us[i] = utemp[i];
                }
                unum *= 2;
                groupnum = count / unum;
                ComplexNumber a, b;
                for (int u = 0; u < unum / 2; u++)
                {
                    now = u * groupnum * 2;
                    ctemp = w(us[now], unum);
                    for (int f = 0; f < groupnum; f++)
                    {
                        a = new ComplexNumber(ctemp);
                        b = new ComplexNumber(hs[x, now + f + groupnum]);
                        newvalue = hs[x, now + f] + (a * b).toV2();
                        hs[x, now + f + groupnum] = hs[x, now + f] - (a * b).toV2();
                        hs[x, now + f] = newvalue;

                        b = new ComplexNumber(dxs[x, now + f + groupnum]);
                        newvalue = dxs[x, now + f] + (a * b).toV2();
                        dxs[x, now + f + groupnum] = dxs[x, now + f] - (a * b).toV2();
                        dxs[x, now + f] = newvalue;

                        b = new ComplexNumber(dzs[x, now + f + groupnum]);
                        newvalue = dzs[x, now + f] + (a * b).toV2();
                        dzs[x, now + f + groupnum] = dzs[x, now + f] - (a * b).toV2();
                        dzs[x, now + f] = newvalue;

                        b = new ComplexNumber(normalxs[x, now + f + groupnum]);
                        newvalue = normalxs[x, now + f] + (a * b).toV2();
                        normalxs[x, now + f + groupnum] = normalxs[x, now + f] - (a * b).toV2();
                        normalxs[x, now + f] = newvalue;

                        b = new ComplexNumber(normalzs[x, now + f + groupnum]);
                        newvalue = normalzs[x, now + f] + (a * b).toV2();
                        normalzs[x, now + f + groupnum] = normalzs[x, now + f] - (a * b).toV2();
                        normalzs[x, now + f] = newvalue;
                    }
                }
            }
            for (int i = 0; i < count; i++)
            {
                cntemp[us[i]] = new Vector2(hs[x, i].x, hs[x, i].y);
            }
            for (int y = 0; y < count; y++)
            {
                hs[x, y] = cntemp[y];
            }

            for (int i = 0; i < count; i++)
            {
                cntemp[us[i]] = new Vector2(normalxs[x, i].x, normalxs[x, i].y);
            }
            for (int y = 0; y < count; y++)
            {
                normalxs[x, y] = cntemp[y];
            }

            for (int i = 0; i < count; i++)
            {
                cntemp[us[i]] = new Vector2(normalzs[x, i].x, normalzs[x, i].y);
            }
            for (int y = 0; y < count; y++)
            {
                normalzs[x, y] = cntemp[y];
            }

            for (int i = 0; i < count; i++)
            {
                cntemp[us[i]] = new Vector2(dxs[x, i].x, dxs[x, i].y);
            }
            for (int y = 0; y < count; y++)
            {
                dxs[x, y] = cntemp[y];
            }

            for (int i = 0; i < count; i++)
            {
                cntemp[us[i]] = new Vector2(dzs[x, i].x, dzs[x, i].y);
            }
            for (int y = 0; y < count; y++)
            {
                dzs[x, y] = cntemp[y];
            }
        }


        float flag = 1;
        //傅里叶变换的平移性
        for (int y = 0; y < count; y++)
        {
            cntemp = new Vector2[count];
            groupnum = count;
            unum = 1;
            us = new int[count];
            utemp = new int[count];
            while (unum != count)
            {
                now = 0;
                for (int i = 0; i < unum; i++)
                {
                    for (int j = 0; j < groupnum / 2; j++)
                    {
                        utemp[now++] = us[i * groupnum];
                    }
                    for (int j = 0; j < groupnum / 2; j++)
                    {
                        utemp[now++] = us[i * groupnum] + unum;
                    }
                }
                for (int i = 0; i < count; i++)
                {
                    us[i] = utemp[i];
                }
                unum *= 2;
                groupnum = count / unum;
                ComplexNumber a, b;
                for (int u = 0; u < unum / 2; u++)
                {
                    now = u * groupnum * 2;
                    ctemp = w(us[now], unum);
                    for (int f = 0; f < groupnum; f++)
                    {
                        a = new ComplexNumber(ctemp);
                        b = new ComplexNumber(hs[now + f + groupnum, y]);
                        newvalue = hs[now + f, y] + (a * b).toV2();
                        hs[now + f + groupnum, y] = hs[now + f, y] - (a * b).toV2();
                        hs[now + f, y] = newvalue;

                        b = new ComplexNumber(dxs[now + f + groupnum, y]);
                        newvalue = dxs[now + f, y] + (a * b).toV2();
                        dxs[now + f + groupnum, y] = dxs[now + f, y] - (a * b).toV2();
                        dxs[now + f, y] = newvalue;

                        b = new ComplexNumber(dzs[now + f + groupnum, y]);
                        newvalue = dzs[now + f, y] + (a * b).toV2();
                        dzs[now + f + groupnum, y] = dzs[now + f, y] - (a * b).toV2();
                        dzs[now + f, y] = newvalue;

                        b = new ComplexNumber(normalxs[now + f + groupnum, y]);
                        newvalue = normalxs[now + f, y] + (a * b).toV2();
                        normalxs[now + f + groupnum, y] = normalxs[now + f, y] - (a * b).toV2();
                        normalxs[now + f, y] = newvalue;

                        b = new ComplexNumber(normalzs[now + f + groupnum, y]);
                        newvalue = normalzs[now + f, y] + (a * b).toV2();
                        normalzs[now + f + groupnum, y] = normalzs[now + f, y] - (a * b).toV2();
                        normalzs[now + f, y] = newvalue;
                    }
                }
            }
            for (int i = 0; i < count; i++)
            {
                cntemp[us[i]] = new Vector2(hs[i, y].x, hs[i, y].y);
            }
            for (int x = 0; x < count; x++)
            {
                hs[x, y] = cntemp[x];
            }

            for (int i = 0; i < count; i++)
            {
                cntemp[us[i]] = new Vector2(normalxs[i, y].x, normalxs[i, y].y);
            }
            for (int x = 0; x < count; x++)
            {
                normalxs[x, y] = cntemp[x];
            }

            for (int i = 0; i < count; i++)
            {
                cntemp[us[i]] = new Vector2(normalzs[i, y].x, normalzs[i, y].y);
            }
            for (int x = 0; x < count; x++)
            {
                normalzs[x, y] = cntemp[x];
            }

            for (int i = 0; i < count; i++)
            {
                cntemp[us[i]] = new Vector2(dxs[i, y].x, dxs[i, y].y);
            }
            for (int x = 0; x < count; x++)
            {
                dxs[x, y] = cntemp[x];
            }

            for (int i = 0; i < count; i++)
            {
                cntemp[us[i]] = new Vector2(dzs[i, y].x, dzs[i, y].y);
            }
            for (int x = 0; x < count; x++)
            {
                dzs[x, y] = cntemp[x];
            }

        }
        for (int x = 0; x < count; x++)
        {
            flag *= -1;
            for (int y = 0; y < count; y++)
            {
                flag *= -1;
                hs[x, y] *= flag;
                dxs[x, y] *= flag;
                dzs[x, y] *= flag;
                normalxs[x, y] *= flag;
                normalzs[x, y] *= flag;
            }
        }
    }
    public static void fftForHDN(Vector2[,] htilde, int count, out Vector2[,] hs,
        out Vector2[,] dxs, out Vector2[,] dzs,
        out Vector2[,] normalxs, out Vector2[,] normalzs)
    {
        hs = new Vector2[count, count];
        dxs = new Vector2[count, count];
        dzs = new Vector2[count, count];
        normalxs = new Vector2[count, count];
        normalzs = new Vector2[count, count];
        Vector2 k;
        Vector2 ctemp;
        Vector2 newvalue;
        Vector2[] cntemp = new Vector2[count];
        float kx, kz;
        int groupnum = count;
        int unum = 1;
        int[] us = new int[count];
        int[] utemp = new int[count];
        int now;
        for (int x = 0; x < count; x++)
        {
            kx = 2 * Mathf.PI * x / count;
            for (int y = 0; y < count; y++)
            {
                kz = 2 * Mathf.PI * y / count;
                k = new Vector2(kx, kz);
                hs[x, y] = htilde[x, y];

            }
        }
        for (int x = 0; x < count; x++)
        {
            cntemp = new Vector2[count];
            groupnum = count;
            unum = 1;
            us = new int[count];
            utemp = new int[count];
            while (unum != count)
            {
                now = 0;
                for (int i = 0; i < unum; i++)
                {
                    for (int j = 0; j < groupnum / 2; j++)
                    {
                        utemp[now++] = us[i * groupnum];
                    }
                    for (int j = 0; j < groupnum / 2; j++)
                    {
                        utemp[now++] = us[i * groupnum] + unum;
                    }
                }
                for (int i = 0; i < count; i++)
                {
                    us[i] = utemp[i];
                }
                unum *= 2;
                groupnum = count / unum;
                ComplexNumber a, b;
                for (int u = 0; u < unum / 2; u++)
                {
                    now = u * groupnum * 2;
                    ctemp = w(us[now], unum);
                    for (int f = 0; f < groupnum; f++)
                    {
                        a = new ComplexNumber(ctemp);
                        b = new ComplexNumber(hs[x, now + f + groupnum]);
                        newvalue = hs[x, now + f] + (a * b).toV2();
                        hs[x, now + f + groupnum] = hs[x, now + f] - (a * b).toV2();
                        hs[x, now + f] = newvalue;
                    }
                }
            }
            for (int i = 0; i < count; i++)
            {
                cntemp[us[i]] = new Vector2(hs[x, i].x, hs[x, i].y);
            }
            for (int y = 0; y < count; y++)
            {
                hs[x, y] = cntemp[y];
            }
        }
        //y
        for (int y = 0; y < count; y++)
        {
            cntemp = new Vector2[count];
            groupnum = count;
            unum = 1;
            us = new int[count];
            utemp = new int[count];
            while (unum != count)
            {
                now = 0;
                for (int i = 0; i < unum; i++)
                {
                    for (int j = 0; j < groupnum / 2; j++)
                    {
                        utemp[now++] = us[i * groupnum];
                    }
                    for (int j = 0; j < groupnum / 2; j++)
                    {
                        utemp[now++] = us[i * groupnum] + unum;
                    }
                }
                for (int i = 0; i < count; i++)
                {
                    us[i] = utemp[i];
                }
                unum *= 2;
                groupnum = count / unum;
                ComplexNumber a, b;
                for (int u = 0; u < unum / 2; u++)
                {
                    now = u * groupnum * 2;
                    ctemp = w(us[now], unum);
                    for (int f = 0; f < groupnum; f++)
                    {
                        a = new ComplexNumber(ctemp);
                        b = new ComplexNumber(hs[now + f + groupnum, y]);
                        newvalue = hs[now + f, y] + (a * b).toV2();
                        hs[now + f + groupnum, y] = hs[now + f, y] - (a * b).toV2();
                        hs[now + f, y] = newvalue;
                    }
                }
            }
            for (int i = 0; i < count; i++)
            {
                cntemp[us[i]] = new Vector2(hs[i, y].x, hs[i, y].y);
            }
            for (int x = 0; x < count; x++)
            {
                hs[x, y] = cntemp[x];
            }
        }
    }
}