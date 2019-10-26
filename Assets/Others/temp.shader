Shader "Hidden/temp"
{
	Properties
	{
		_Diffuse("Diffuse", Color) = (1, 1, 1, 1)
	}
		SubShader
	{
		Cull Off ZWrite Off 
		Tags { "RenderType" = "Opaque" }
		Pass
		{
			Tags { "LightMode" = "ForwardBase" }
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "Lighting.cginc"

			fixed4 _Diffuse;

			float _Dx[4];
			float _Dy[4];
			//steepness=A
			float _Steepness[4];
			//Frequency=W
			float _Frequency[4];
			float _Speed[4];
			float _Q[4];
			float _SubColors[3];
			struct a2v
			{
				float4 vertex : POSITION;
				float3 normal : TEXCOORD1;
				float4 tangent : TEXCOORD2;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float3 worldNormal : TEXCOORD4;
			};
			float4 D;
			float temp;
			float sint, cost, wa;
			float agree;
			v2f vert(a2v v)
			{
				v2f o;
				for (int i = 0; i < 4; i++)
				{
					D = float4(_Dx[i], 0, _Dy[i], 1.0);

					agree = _Frequency[i] * dot(D, v.vertex) + _Speed[i] * _Time.y;
					sint = sin(agree);
					cost = cos(agree);
					v.vertex.y += _Steepness[i] * sint;
					temp = _Q[i] * _Steepness[i] * cost;
					v.vertex.x += D.x * temp;
					v.vertex.z += D.z * temp;

					wa = _Frequency[i] * _Steepness[i];
					sint *= wa;
					cost *= wa;
					v.normal.x += -D.x * cost;
					v.normal.y += -_Q[i] * sint;
					v.normal.z += -D.y * cost;

					v.tangent.x += -_Q[i] * D.x * cost;
					v.tangent.y += D.z * cost;
					v.tangent.z += -_Q[i] * D.y * D.y * sint;
				}
				v.normal.y += 1;
				v.tangent.y += 1;

				o.pos = UnityObjectToClipPos(v.vertex);

				o.worldNormal = mul(v.normal, (float3x3)unity_WorldToObject);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz;

				// Get the normal in world space
				fixed3 worldNormal = normalize(i.worldNormal);
				// Get the light direction in world space
				fixed3 worldLightDir = normalize(_WorldSpaceLightPos0.xyz);

				// Compute diffuse term
				fixed halfLambert = dot(worldNormal, worldLightDir) * 0.5 + 0.5;
				fixed3 diffuse = _LightColor0.rgb * _Diffuse.rgb * halfLambert;
				
				fixed3 color = ambient + diffuse;

				return fixed4(color, 1.0);
			}
			ENDCG
		}
	}
}