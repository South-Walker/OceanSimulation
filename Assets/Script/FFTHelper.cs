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
public class FFTHelper
{
    const float MIN = 0.00001f;
    public static Vector2 w(float k, float n)
    {
        Vector2 r = new Vector2();
        float temp = 2 * Mathf.PI * k / n;
        r.x = Mathf.Cos(temp);
        r.y = -Mathf.Sin(temp);
        return r;
    }
    public static void FFT(Vector2[,] f, int count)
    {
        Vector2 ctemp;
        Vector2 newvalue;
        Vector2[] cntemp = new Vector2[count];
        int groupnum = count;
        int unum = 1;
        int[] us = new int[count];
        int[] utemp = new int[count];
        int now;
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
                    for (int g = 0; g < groupnum; g++)
                    {
                        a = new ComplexNumber(ctemp);
                        b = new ComplexNumber(f[x, now + g + groupnum]);
                        newvalue = f[x, now + g] + (a * b).toV2();
                        f[x, now + g + groupnum] = f[x, now + g] - (a * b).toV2();
                        f[x, now + g] = newvalue;
                    }
                }
            }
            for (int i = 0; i < count; i++)
            {
                cntemp[us[i]] = new Vector2(f[x, i].x, f[x, i].y);
            }
            for (int y = 0; y < count; y++)
            {
                f[x, y] = cntemp[y];
            }
        }
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
                    for (int g = 0; g < groupnum; g++)
                    {
                        a = new ComplexNumber(ctemp);
                        b = new ComplexNumber(f[now + g + groupnum, y]);
                        newvalue = f[now + g, y] + (a * b).toV2();
                        f[now + g + groupnum, y] = f[now + g, y] - (a * b).toV2();
                        f[now + g, y] = newvalue;
                    }
                }
            }
            for (int i = 0; i < count; i++)
            {
                cntemp[us[i]] = new Vector2(f[i, y].x, f[i, y].y);
            }
            for (int x = 0; x < count; x++)
            {
                f[x, y] = cntemp[x];
            }
        }
        for (int x = 0; x < count; x++)
        {
            for (int y = 0; y < count; y++)
            {
                if ((x + y) % 2 != 0)
                    f[x, y] *= -1;
            }
        }
    }
}
