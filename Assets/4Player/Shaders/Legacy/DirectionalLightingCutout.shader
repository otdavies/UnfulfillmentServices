// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "DirectionalLightingCutout"
{
	Properties
	{
		[HideInInspector] __dirty( "", Int ) = 1
		_MaskClipValue( "Mask Clip Value", Float ) = 0.5
		[HideInInspector]_SpecColor("SpecularColor",Color)=(1,1,1,1)
		_Color_x("Color_x", Color) = (0.8088235,0.4222535,0.4222535,1)
		_Color_y("Color_y", Color) = (0.7099479,0.3073097,0.8529412,1)
		_Color_z("Color_z", Color) = (0.3740492,0.625,0.1332721,1)
		_Color_x2("Color_x2", Color) = (0.8088235,0.4222535,0.4222535,1)
		_Color_y2("Color_y2", Color) = (0.7099479,0.3073097,0.8529412,1)
		_Color_z2("Color_z2", Color) = (0.3740492,0.625,0.1332721,1)
		_shine("shine", Float) = 0.2
		_TextureSample0("Texture Sample 0", 2D) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
	}

	SubShader
	{
		Tags{ "RenderType" = "TreeTransparentCutout"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGINCLUDE
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#ifdef UNITY_PASS_SHADOWCASTER
			#undef INTERNAL_DATA
			#undef WorldReflectionVector
			#undef WorldNormalVector
			#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
			#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
			#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))
		#endif
		struct Input
		{
			float2 uv_texcoord;
			float3 worldNormal;
		};

		uniform sampler2D _TextureSample0;
		uniform float4 _TextureSample0_ST;
		uniform float4 _Color_x;
		uniform float4 _Color_y;
		uniform float4 _Color_z;
		uniform float4 _Color_x2;
		uniform float4 _Color_y2;
		uniform float4 _Color_z2;
		uniform float _shine;
		uniform float _MaskClipValue = 0.5;

		void surf( Input i , inout SurfaceOutput o )
		{
			float2 uv_TextureSample0 = i.uv_texcoord * _TextureSample0_ST.xy + _TextureSample0_ST.zw;
			float4 tex2DNode77 = tex2D( _TextureSample0,uv_TextureSample0);
			float4 temp_output_37_0 = ( ( ( ( _Color_x * clamp( dot( i.worldNormal , float3(1,0,0) ) , 0.0 , 1.0 ) ) + ( _Color_y * clamp( dot( i.worldNormal , float3(0,1,0) ) , 0.0 , 1.0 ) ) ) + ( _Color_z * clamp( dot( i.worldNormal , float3(0,0,1) ) , 0.0 , 1.0 ) ) ) + ( ( ( _Color_x2 * clamp( dot( i.worldNormal , float3(-1,0,0) ) , 0.0 , 1.0 ) ) + ( _Color_y2 * clamp( dot( i.worldNormal , float3(0,-1,0) ) , 0.0 , 1.0 ) ) ) + ( _Color_z2 * clamp( dot( i.worldNormal , float3(0,0,-1) ) , 0.0 , 1.0 ) ) ) );
			o.Albedo = ( saturate( 	max( tex2DNode77, ( tex2DNode77 * temp_output_37_0 ) ) )).rgb;
			o.Emission = ( tex2DNode77 * ( temp_output_37_0 * _shine ) ).rgb;
			o.Alpha = 1;
			clip( tex2DNode77.a - _MaskClipValue );
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf BlinnPhong keepalpha 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			# include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			sampler3D _DitherMaskLOD;
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float3 worldPos : TEXCOORD6;
				float4 tSpace0 : TEXCOORD1;
				float4 tSpace1 : TEXCOORD2;
				float4 tSpace2 : TEXCOORD3;
				float4 texcoords01 : TEXCOORD4;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				o.texcoords01 = float4( v.texcoord.xy, v.texcoord1.xy );
				o.worldPos = worldPos;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				return o;
			}
			fixed4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.texcoords01.xy;
				float3 worldPos = IN.worldPos;
				fixed3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldNormal = float3( IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z );
				SurfaceOutput o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutput, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=7003
-1913;29;1906;1004;1800.789;412.1742;2.27234;True;True
Node;AmplifyShaderEditor.Vector3Node;5;-765.4,122.1;Float;False;Constant;_light_1;light_1;0;0;1,0,0;FLOAT3;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.Vector3Node;14;-759.6,265.2003;Float;False;Constant;_light_2;light_2;2;0;0,1,0;FLOAT3;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.Vector3Node;31;-833.7515,1168.999;Float;False;Constant;_light_5;light_5;8;0;0,-1,0;FLOAT3;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.WorldNormalVector;9;-1234.378,564.4238;Float;False;0;FLOAT3;0,0,0;False;FLOAT3;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.Vector3Node;30;-825.8279,1028.643;Float;False;Constant;_light_4;light_4;6;0;-1,0,0;FLOAT3;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.Vector3Node;32;-832.8266,1309.934;Float;False;Constant;_light_6;light_6;10;0;0,0,-1;FLOAT3;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.DotProductOpNode;24;-643.5474,1025.537;Float;False;0;FLOAT3;0.0,0,0;False;1;FLOAT3;0.0,0,0;False;FLOAT
Node;AmplifyShaderEditor.DotProductOpNode;13;-621.8,256.1004;Float;False;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;FLOAT
Node;AmplifyShaderEditor.DotProductOpNode;4;-618.8,128.5999;Float;False;0;FLOAT3;0.0,0,0;False;1;FLOAT3;0.0,0,0;False;FLOAT
Node;AmplifyShaderEditor.Vector3Node;20;-758.6755,426.5135;Float;False;Constant;_light_3;light_3;4;0;0,0,1;FLOAT3;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.DotProductOpNode;25;-646.5474,1153.038;Float;False;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;FLOAT
Node;AmplifyShaderEditor.ColorNode;15;-316.3003,127.4;Float;False;Property;_Color_y;Color_y;1;0;0.7099479,0.3073097,0.8529412,1;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.ColorNode;1;-608.6923,-89.84873;Float;False;Property;_Color_x;Color_x;0;0;0.8088235,0.4222535,0.4222535,1;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.ClampOpNode;40;-495.9466,258.7501;Float;False;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;1.0;False;FLOAT
Node;AmplifyShaderEditor.ColorNode;29;-303.9948,1036.688;Float;False;Property;_Color_y2;Color_y2;4;0;0.7099479,0.3073097,0.8529412,1;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.ColorNode;28;-659.6484,745.4362;Float;False;Property;_Color_x2;Color_x2;3;0;0.8088235,0.4222535,0.4222535,1;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.DotProductOpNode;17;-620.8755,414.8755;Float;False;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;FLOAT
Node;AmplifyShaderEditor.ClampOpNode;41;-506.6512,116.9146;Float;False;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;1.0;False;FLOAT
Node;AmplifyShaderEditor.ClampOpNode;44;-510.0221,1127.999;Float;False;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;1.0;False;FLOAT
Node;AmplifyShaderEditor.DotProductOpNode;27;-645.623,1311.813;Float;False;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;FLOAT
Node;AmplifyShaderEditor.ClampOpNode;43;-490.3649,975.6569;Float;False;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;1.0;False;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;7;-320,0;Float;False;0;COLOR;0.0,0,0,0;False;1;FLOAT;0.0,0,0,0;False;COLOR
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;23;-344.7478,899.174;Float;False;0;COLOR;0.0,0,0,0;False;1;FLOAT;0.0,0,0,0;False;COLOR
Node;AmplifyShaderEditor.ColorNode;19;-336.7229,401.251;Float;False;Property;_Color_z;Color_z;2;0;0.3740492,0.625,0.1332721,1;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.ClampOpNode;45;-527.7416,1338.71;Float;False;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;1.0;False;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;26;-78.49059,1258.037;Float;False;0;COLOR;0.0,0,0,0;False;1;FLOAT;0.0,0,0,0;False;COLOR
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;12;-314.8,291.8;Float;False;0;COLOR;0.0,0,0,0;False;1;FLOAT;0.0,0,0,0;False;COLOR
Node;AmplifyShaderEditor.ClampOpNode;42;-486.5797,436.7138;Float;False;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;1.0;False;FLOAT
Node;AmplifyShaderEditor.ColorNode;34;-698.6434,1536.021;Float;False;Property;_Color_z2;Color_z2;5;0;0.3740492,0.625,0.1332721,1;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;33;-83.72953,1400.346;Float;False;0;COLOR;0.0,0,0,0;False;1;FLOAT;0.0,0,0,0;False;COLOR
Node;AmplifyShaderEditor.SimpleAddOpNode;36;30.4466,1043.264;Float;False;0;COLOR;0.0,0,0,0;False;1;COLOR;0,0,0,0;False;COLOR
Node;AmplifyShaderEditor.SimpleAddOpNode;16;-84.70087,40.30001;Float;False;0;COLOR;0.0,0,0,0;False;1;COLOR;0,0,0,0;False;COLOR
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;18;-230.3862,634.4498;Float;False;0;COLOR;0.0,0,0,0;False;1;FLOAT;0.0,0,0,0;False;COLOR
Node;AmplifyShaderEditor.SimpleAddOpNode;21;41.76092,186.6099;Float;False;0;COLOR;0,0,0,0;False;1;COLOR;0.0;False;COLOR
Node;AmplifyShaderEditor.SimpleAddOpNode;35;200.9068,1127.461;Float;False;0;COLOR;0,0,0,0;False;1;COLOR;0.0;False;COLOR
Node;AmplifyShaderEditor.SamplerNode;77;906.0699,338.9162;Float;True;Property;_TextureSample0;Texture Sample 0;11;0;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;1.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;FLOAT4;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;48;256.6256,896.86;Float;False;Property;_shine;shine;6;0;0.2;0;0;FLOAT
Node;AmplifyShaderEditor.SimpleAddOpNode;37;253.6085,634.4438;Float;False;0;COLOR;0.0,0,0,0;False;1;COLOR;0.0,0,0,0;False;COLOR
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;46;404.6253,748.3598;Float;False;0;COLOR;0.0,0,0,0;False;1;FLOAT;0,0,0,0;False;COLOR
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;79;1061.79,721.4549;Float;False;0;FLOAT4;0.0;False;1;COLOR;0,0,0,0;False;COLOR
Node;AmplifyShaderEditor.BlendOpsNode;78;1269.654,607.4106;Float;False;Lighten;True;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;COLOR
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;83;1122.396,864.6061;Float;False;0;FLOAT4;0,0,0,0;False;1;COLOR;0,0,0,0;False;COLOR
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1505.847,644.9991;Float;False;True;2;Float;ASEMaterialInspector;0;BlinnPhong;DirectionalLightingCutout;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;0;False;0;0;Custom;0.5;True;True;0;True;TreeTransparentCutout;Geometry;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;False;0;255;255;0;0;0;0;False;0;4;10;25;False;0.5;True;0;Zero;Zero;0;Zero;Zero;Add;Add;0;False;0.24;0,0,0,0;VertexOffset;False;Spherical;Relative;0;;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0.0;False;4;FLOAT;0.0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0.0;False;9;FLOAT;0.0;False;10;OBJECT;0.0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;13;OBJECT;0.0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False
WireConnection;24;0;9;0
WireConnection;24;1;30;0
WireConnection;13;0;9;0
WireConnection;13;1;14;0
WireConnection;4;0;9;0
WireConnection;4;1;5;0
WireConnection;25;0;9;0
WireConnection;25;1;31;0
WireConnection;40;0;13;0
WireConnection;17;0;9;0
WireConnection;17;1;20;0
WireConnection;41;0;4;0
WireConnection;44;0;25;0
WireConnection;27;0;9;0
WireConnection;27;1;32;0
WireConnection;43;0;24;0
WireConnection;7;0;1;0
WireConnection;7;1;41;0
WireConnection;23;0;28;0
WireConnection;23;1;43;0
WireConnection;45;0;27;0
WireConnection;26;0;29;0
WireConnection;26;1;44;0
WireConnection;12;0;15;0
WireConnection;12;1;40;0
WireConnection;42;0;17;0
WireConnection;33;0;34;0
WireConnection;33;1;45;0
WireConnection;36;0;23;0
WireConnection;36;1;26;0
WireConnection;16;0;7;0
WireConnection;16;1;12;0
WireConnection;18;0;19;0
WireConnection;18;1;42;0
WireConnection;21;0;16;0
WireConnection;21;1;18;0
WireConnection;35;0;36;0
WireConnection;35;1;33;0
WireConnection;37;0;21;0
WireConnection;37;1;35;0
WireConnection;46;0;37;0
WireConnection;46;1;48;0
WireConnection;79;0;77;0
WireConnection;79;1;37;0
WireConnection;78;0;77;0
WireConnection;78;1;79;0
WireConnection;83;0;77;0
WireConnection;83;1;46;0
WireConnection;0;0;78;0
WireConnection;0;2;83;0
WireConnection;0;10;77;4
ASEEND*/
//CHKSM=A12CD82B63579AA1D2EE8B65FEA2B54F3086B029