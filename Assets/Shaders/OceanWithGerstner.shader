Shader "Hidden/OceanWithGerstner"
{
	Properties
	{
		_Diffuse("Diffuse", Color) = (1, 1, 1, 1)
		_Specular("Specular", Color) = (1, 1, 1, 1)
		_Gloss("Gloss", Range(8.0, 256)) = 20
	}
		SubShader
	{
		Tags { "RenderType" = "Opaque" }
		Pass
		{
			Tags { "LightMode" = "ForwardBase" }
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "Lighting.cginc"

			fixed4 _Diffuse;
			fixed4 _Specular;
			float _Gloss;
			float _Q[4];
			float _Dx[4];
			float _Dy[4];
			//steepness=A
			float _Steepness[4];
			//Frequency=W
			float _Frequency[4];
			float _Speed[4];
			float _SubColors[3];
			struct a2v
			{
				float4 vertex : POSITION;
				float3 normal : TEXCOORD0;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float3 worldNormal : TEXCOORD1;
				float3 worldPos : TEXCOORD2;
			};
			float4 D;
			float3 B, T;
			float temp;
			float sint, cost, wa;
			float agree;
			v2f vert(a2v v)
			{
				v2f o;
				B = float3(1, 0, 0);
				T = float3(0, 0, 1);
				for (int i = 0; i < 4; i++)
				{
					D = float4(_Dx[i], 0, _Dy[i], 1.0);
					agree = _Frequency[i] * dot(D, v.vertex) + _Speed[i] * _Time.y;
					sint = _Steepness[i] * sin(agree);
					cost = _Steepness[i] * cos(agree);
					v.vertex.x += _Q[i] * _Dx[i] * cost;
					v.vertex.y += sint;
					v.vertex.z += _Q[i] * _Dy[i] * cost;

					sint *= _Frequency[i];
					cost *= _Frequency[i];

					B.x += -_Q[i] * _Dx[i] * _Dx[i] * sint;
					B.y += _Dx[i] * cost;
					B.z += -_Q[i] * _Dx[i] * _Dy[i] * sint;

					T.x += -_Q[i] * _Dx[i] * _Dy[i] * sint;
					T.y += _Dy[i] * cost;
					T.z += -_Q[i] * _Dy[i] * _Dy[i] * sint;
				}
				v.normal.xyz = cross(T, B);
				o.pos = UnityObjectToClipPos(v.vertex);

				o.worldNormal = mul(v.normal, (float3x3)unity_WorldToObject);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				// Get ambient term
				fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz;

				fixed3 worldNormal = normalize(i.worldNormal);
				fixed3 worldLightDir = normalize(_WorldSpaceLightPos0.xyz);

				// Compute diffuse term
				fixed3 diffuse = _LightColor0.rgb * _Diffuse.rgb * saturate(dot(worldNormal, worldLightDir));

				// Get the reflect direction in world space
				fixed3 reflectDir = normalize(reflect(-worldLightDir, worldNormal));
				// Get the view direction in world space
				fixed3 viewDir = normalize(_WorldSpaceCameraPos.xyz - i.worldPos.xyz);
				// Compute specular term
				fixed3 specular = _LightColor0.rgb * _Specular.rgb * pow(saturate(dot(reflectDir, viewDir)), _Gloss);

				return fixed4(ambient + diffuse + specular, 1.0);
		}
		ENDCG
	}
	}
}
