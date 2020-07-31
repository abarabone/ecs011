
Shader "Custom/mesh_nolit_cs"
{
	
	Properties
	{
		[NoScaleOffset]
		_MainTex("Texture", 2D) = "white" {}

		_Color("Color", Color) = (1, 1, 1, 1)
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
	}
	
	
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		
		LOD 200
		
		Pass
		{
			Lighting Off
			LOD 200
			

			Tags
			{
				"Queue"				= "Geometry"
				"IgnoreProjector"	= "True"
				"RenderType"		= "Opaque"
			//	"LightMode"			= "Vertex"
			}

			CGPROGRAM

			//#pragma target 5.0
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing
			//#pragma multi_compile_fog
			#include "UnityCG.cginc"
			//#include "AutoLight.cginc"


			struct appdata
			{
				float4	vertex	: POSITION;
				float3	normal	: NORMAL;
				float2	uv		: TEXCOORD0;
				fixed4	part_index : COLOR;
			};
			
			struct v2f
			{
				float4 vertex	: SV_POSITION;
				float2 uv		: TEXCOORD0;
				fixed4 color	: COLOR;

			//	UNITY_FOG_COORDS(2)
			};


			StructuredBuffer<float4> BoneVectorBuffer;
			int	BoneLengthEveryInstance;
			int BoneVectorOffset;

			fixed4		_Color;
			sampler2D	_MainTex;
			
			float4 rot( float4 v, float4 q )
			{
				float3 qv = cross(v.xyz, q.xyz) - v.xyz * q.w;
				float3 rv = v.xyz + 2.0f * cross(qv, q.xyz);

				return float4( rv, 0.0f );
			}

			

			static const uint4 element_mask_table[] =
			{
				{1,0,0,0}, {0,1,0,0}, {0,0,1,0}, {0,0,0,1}
			};
			
			uint get_part_bit(int iInstance, fixed2 part_id)
			{
				const int offset = part_id.w >> 2;
				const int elemnt = (part_id.w & 0x3);
				const int bitmask = 1 << part_id.z;
				
				const uint4 xyzw = asuint(BoneVectorBuffer[i_instance + offset]);
				const uint  = dot(xyzw, element_mask_table[element]);
				return element & bitmask;
			}
			

			v2f vert(appdata v , uint i : SV_InstanceID )
			{
				v2f o;

				int ibone = i;// * 2;//BoneLengthEveryInstance;
				int ivec = BoneVectorOffset + ibone * 2;

				float4 wpos = BoneVectorBuffer[ivec + 0];
				float4 wrot = BoneVectorBuffer[ivec + 1];

				float4	lvt = v.vertex;
				float4	rvt = rot( lvt, wrot );
				float4	tvt = rvt + wpos;
				float4	wvt = mul(UNITY_MATRIX_VP, float4(tvt.xyz, 1.0f));
				//float4	wvt = UnityObjectToClipPos(tvt);

				o.vertex = wvt;
				o.uv = v.uv;
				o.color = float4(1,1,1,1);

				return o;
			}

		
		
			fixed4 frag( v2f i ) : SV_Target
			{
				fixed4 texcol = tex2D(_MainTex, i.uv);
				
				fixed4 col = fixed4(texcol * i.color.xyz, 1.0) * _Color;

			//	UNITY_APPLY_FOG(i.fogCoord, col);

				return col;
			}

			ENDCG
			
		}
		
	}

}
