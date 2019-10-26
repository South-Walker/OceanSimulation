Shader "Hidden/s"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
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

			fixed4 _Color;
			struct a2v
			{
				float4 vertex : POSITION;
				half4 color : COLOR;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				half4 color : COLOR;
			};
			v2f vert(a2v v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.color = v.color;
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				return i.color;
			}
		ENDCG
	}
	}
}