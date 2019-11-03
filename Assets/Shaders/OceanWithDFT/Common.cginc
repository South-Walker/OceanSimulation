#include "UnityCG.cginc"
static float PI = 3.14159;
static float MIN = 0.0001;
static float G = 9.8;

inline float Random(float2 uv, float salt)
{
	float2 seed = uv + float2(salt, -salt);
	return frac(sin(dot(seed, float2(12.9898, 78.233))) * 43758.5453);
}
inline float2 NormalDistribution(float2 uv, float salt)
{
	float2 UniformDistribution = Random(uv, salt);
	UniformDistribution.x = clamp(UniformDistribution.x, 0.01, 1);
	UniformDistribution.y = clamp(UniformDistribution.y, 0.01, 1);
	float a = sqrt(-2 * log(UniformDistribution.x));
	float b = 2 * PI * UniformDistribution.y;
	return float2(a * cos(b), a * sin(b));
}
inline float2 ComplexNumberMultiply(float2 a, float2 b)
{
	return float2(a.x * b.x - a.y * b.y, a.x * b.y + a.y * b.x);
}
inline float2 Conj(float2 i)
{
	return float2(i.x, -i.y);
}
inline float2 GetK(float2 uv, float len)
{
	uv -= 0.5;
	uv.x = (uv.x < len * 0.5) ? uv.x : uv.x - len;
	uv.y = (uv.y < len * 0.5) ? uv.y : uv.y - len;
	return 2 * PI * float2(uv.x / len, uv.y / len);
}
inline float Phillips(float2 uv, float a, float len, float2 wind)
{
	float2 k = GetK(uv, len);
	float klen = length(k);
	if (klen < MIN)
		return 0;
	float klen2 = klen * klen;
	float klen4 = klen2 * klen2;
	float kdotw = dot(normalize(k), normalize(wind));
	float kdotw2 = kdotw * kdotw;
	float v = length(wind);
	float l = v * v / G;
	float l2 = l * l;
	return a * exp(-1 / klen2 / l2) / klen4 * kdotw2;
}
//·µ»Ø¸´Êý
inline float2 Htilde0(float2 uv, float phi, float salt)
{
	float temp = sqrt(phi / 2);
	float2 r = NormalDistribution(uv, salt);
	return float2(r.x * temp, r.y * temp);
}
inline float GetOmega(float2 uv, float len)
{
	float2 k = GetK(uv, len);
	return sqrt(G * length(k));
}