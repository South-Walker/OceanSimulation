using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Script
{
    public class ShaderWave
    {
        public static List<float> Steepnesses = new List<float>();
        public static List<float> Frequencys = new List<float>();
        public static List<float> Dxs = new List<float>();
        public static List<float> Dys = new List<float>();
        public static List<float> Speeds = new List<float>();
        public static List<float> Qs = new List<float>();

        public static float stdSteepness, stdFrequency, stdSpeed, stdQ;
        public float Steepness;
        public float Frequency;
        public float Dx, Dy;
        public float Speed;
        public float Q;
        public static void Clear()
        {
            Steepnesses.Clear();
            Frequencys.Clear();
            Dxs.Clear();
            Dys.Clear();
            Speeds.Clear();
            Qs.Clear();
        }
        public static ShaderWave RandomWave()
        {
            float steepness, frequency, speed, q;
            float agree;
            steepness = Random.Range(stdSteepness / 2, stdSteepness * 2);
            frequency = stdFrequency / stdSteepness * steepness;
            speed = Random.Range(stdSpeed / 2, stdSpeed * 2);
            q = Random.Range(stdQ / 2, stdQ * 2);
            agree = Random.Range(0, Mathf.PI / 2);
            return new ShaderWave(steepness, frequency, agree, speed, q);
        }
        public ShaderWave(float steepness, float frequency, float agree, float speed, float q)
        {
            Steepness = steepness;
            Frequency = frequency;
            Dx = Mathf.Cos(agree);
            Dy = Mathf.Sin(agree);
            Speed = speed;
            Q = q;

            Steepnesses.Add(Steepness);
            Frequencys.Add(Frequency);
            Dxs.Add(Dx);
            Dys.Add(Dy);
            Speeds.Add(speed);
            Qs.Add(Q);
        }
    }
}
