// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "SixSideLighting"
{
	Properties
	{
		[HideInInspector] __dirty( "", Int ) = 1
		_Color_x("Color_x", Color) = (0.8088235,0.4222535,0.4222535,1)
		_Color_y("Color_y", Color) = (0.7099479,0.3073097,0.8529412,1)
		_Color_z("Color_z", Color) = (0.3740492,0.625,0.1332721,1)
		_Color_x2("Color_x2", Color) = (0.8088235,0.4222535,0.4222535,1)
		_Color_y2("Color_y2", Color) = (0.7099479,0.3073097,0.8529412,1)
		_Color_z2("Color_z2", Color) = (0.3740492,0.625,0.1332721,1)
		_SurfaceShine("SurfaceShine", Float) = 0.2
		_GradientColor("GradientColor", Color) = (0,0,0,0)
		_GradientShine("GradientShine", Float) = 0
		_GradientStart("GradientStart", Float) = 2
		_GradientLength("GradientLength", Float) = 0.5
		_GradientFallOff("GradientFallOff", Range( 0.1 , 2)) = 0.49
		_GradientDirection("GradientDirection", Vector) = (0,1,0,0)
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGINCLUDE
		#include "UnityShaderVariables.cginc"
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
			float3 worldNormal;
			float3 worldPos;
		};

		uniform float4 _Color_x;
		uniform float4 _Color_y;
		uniform float4 _Color_z;
		uniform float4 _Color_x2;
		uniform float4 _Color_y2;
		uniform float4 _Color_z2;
		uniform float4 _GradientColor;
		uniform float3 _GradientDirection;
		uniform float _GradientStart;
		uniform float _GradientFallOff;
		uniform float _GradientLength;
		uniform float _SurfaceShine;
		uniform float _GradientShine;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float4 temp_output_37_0 = ( ( ( ( _Color_x * clamp( dot( i.worldNormal , float3(1,0,0) ) , 0.0 , 1.0 ) ) + ( _Color_y * clamp( dot( i.worldNormal , float3(0,1,0) ) , 0.0 , 1.0 ) ) ) + ( _Color_z * clamp( dot( i.worldNormal , float3(0,0,1) ) , 0.0 , 1.0 ) ) ) + ( ( ( _Color_x2 * clamp( dot( i.worldNormal , float3(-1,0,0) ) , 0.0 , 1.0 ) ) + ( _Color_y2 * clamp( dot( i.worldNormal , float3(0,-1,0) ) , 0.0 , 1.0 ) ) ) + ( _Color_z2 * clamp( dot( i.worldNormal , float3(0,0,-1) ) , 0.0 , 1.0 ) ) ) );
			float3 vertexPos = mul( unity_WorldToObject, float4( i.worldPos , 1 ) );
			float4 temp_output_58_0 = ( ( _GradientColor * 0.5 ) * clamp( ( pow( ( dot( _GradientDirection , vertexPos ) - _GradientStart ) , _GradientFallOff ) * ( 1.0 / _GradientLength ) ) , 0.0 , 1.0 ) );
			o.Albedo = ( saturate( 	max( temp_output_37_0, temp_output_58_0 ) )).rgb;
			o.Emission = ( saturate( 	max( ( temp_output_37_0 * _SurfaceShine ), ( temp_output_58_0 * _GradientShine ) ) )).rgb;
			o.Alpha = 1;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard keepalpha 

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
				float3 worldPos = IN.worldPos;
				fixed3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = float3( IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z );
				SurfaceOutputStandard o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandard, o )
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
-1911;69;1904;1004;1949.163;45.96138;1.6;True;True
Node;AmplifyShaderEditor.Vector3Node;31;-784.3475,1162.137;Float;False;Constant;_light_5;light_5;8;0;0,-1,0;FLOAT3;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.WorldNormalVector;9;-1234.378,564.4238;Float;False;0;FLOAT3;0,0,0;False;FLOAT3;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.Vector3Node;76;148.904,-169.2643;Float;False;Property;_GradientDirection;GradientDirection;12;0;0,1,0;FLOAT3;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.Vector3Node;30;-790.1475,1019.037;Float;False;Constant;_light_4;light_4;6;0;-1,0,0;FLOAT3;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.Vector3Node;14;-759.6,265.2003;Float;False;Constant;_light_2;light_2;2;0;0,1,0;FLOAT3;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.PosVertexDataNode;59;143.8035,0.03576326;Float;False;FLOAT3;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.Vector3Node;5;-765.4,122.1;Float;False;Constant;_light_1;light_1;0;0;1,0,0;FLOAT3;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.DotProductOpNode;25;-646.5474,1153.038;Float;False;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;FLOAT
Node;AmplifyShaderEditor.DotProductOpNode;4;-618.8,128.5999;Float;False;0;FLOAT3;0.0,0,0;False;1;FLOAT3;0.0,0,0;False;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;51;121.4063,304.036;Float;False;Property;_GradientStart;GradientStart;9;0;2;0;0;FLOAT
Node;AmplifyShaderEditor.Vector3Node;20;-758.6755,426.5135;Float;False;Constant;_light_3;light_3;4;0;0,0,1;FLOAT3;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.DotProductOpNode;13;-621.8,256.1004;Float;False;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;FLOAT
Node;AmplifyShaderEditor.DotProductOpNode;24;-643.5474,1025.537;Float;False;0;FLOAT3;0.0,0,0;False;1;FLOAT3;0.0,0,0;False;FLOAT
Node;AmplifyShaderEditor.DotProductOpNode;75;336.105,-81.2643;Float;False;0;FLOAT3;0.0,0,0;False;1;FLOAT3;0,0,0;False;FLOAT
Node;AmplifyShaderEditor.Vector3Node;32;-783.4229,1320.913;Float;False;Constant;_light_6;light_6;10;0;0,0,-1;FLOAT3;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.DotProductOpNode;17;-620.8755,414.8755;Float;False;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;FLOAT
Node;AmplifyShaderEditor.ClampOpNode;40;-495.9466,258.7501;Float;False;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;1.0;False;FLOAT
Node;AmplifyShaderEditor.ClampOpNode;43;-490.3649,975.6569;Float;False;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;1.0;False;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;81;304.91,363.4336;Float;False;Property;_GradientFallOff;GradientFallOff;11;0;0.49;0.1;2;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;55;148.2428,482.1152;Float;False;Property;_GradientLength;GradientLength;10;0;0.5;0;0;FLOAT
Node;AmplifyShaderEditor.ClampOpNode;41;-506.6512,116.9146;Float;False;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;1.0;False;FLOAT
Node;AmplifyShaderEditor.ColorNode;1;-320.5004,-166.6998;Float;False;Property;_Color_x;Color_x;0;0;0.8088235,0.4222535,0.4222535,1;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;66;177.9189,403.0421;Float;False;Constant;_Float0;Float 0;10;0;1;0;0;FLOAT
Node;AmplifyShaderEditor.ColorNode;15;-316.3003,127.4;Float;False;Property;_Color_y;Color_y;1;0;0.7099479,0.3073097,0.8529412,1;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.ColorNode;29;-341.048,1024.337;Float;False;Property;_Color_y2;Color_y2;4;0;0.7099479,0.3073097,0.8529412,1;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.ClampOpNode;44;-510.0221,1127.999;Float;False;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;1.0;False;FLOAT
Node;AmplifyShaderEditor.ColorNode;28;-659.6484,745.4362;Float;False;Property;_Color_x2;Color_x2;3;0;0.8088235,0.4222535,0.4222535,1;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.SimpleSubtractOpNode;62;334.5044,219.5356;Float;False;0;FLOAT;0.0;False;1;FLOAT;0.0;False;FLOAT
Node;AmplifyShaderEditor.DotProductOpNode;27;-645.623,1311.813;Float;False;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;7;-320,0;Float;False;0;COLOR;0.0,0,0,0;False;1;FLOAT;0.0,0,0,0;False;COLOR
Node;AmplifyShaderEditor.ClampOpNode;45;-527.7416,1338.71;Float;False;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;1.0;False;FLOAT
Node;AmplifyShaderEditor.ColorNode;34;-665.8141,1565.647;Float;False;Property;_Color_z2;Color_z2;5;0;0.3740492,0.625,0.1332721,1;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.SimpleDivideOpNode;54;376.1074,431.1358;Float;False;0;FLOAT;0.0;False;1;FLOAT;0.0;False;FLOAT
Node;AmplifyShaderEditor.ColorNode;19;-336.7229,401.251;Float;False;Property;_Color_z;Color_z;2;0;0.3740492,0.625,0.1332721,1;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;23;-344.7478,899.174;Float;False;0;COLOR;0.0,0,0,0;False;1;FLOAT;0.0,0,0,0;False;COLOR
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;26;-241.7991,1199.027;Float;False;0;COLOR;0.0,0,0,0;False;1;FLOAT;0.0,0,0,0;False;COLOR
Node;AmplifyShaderEditor.ClampOpNode;42;-486.5797,436.7138;Float;False;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;1.0;False;FLOAT
Node;AmplifyShaderEditor.PowerNode;71;493.5869,278.0335;Float;False;0;FLOAT;0.0;False;1;FLOAT;1.0;False;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;12;-314.8,291.8;Float;False;0;COLOR;0.0,0,0,0;False;1;FLOAT;0.0,0,0,0;False;COLOR
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;18;-230.3862,634.4498;Float;False;0;COLOR;0.0,0,0,0;False;1;FLOAT;0.0,0,0,0;False;COLOR
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;33;-332.1232,1451.122;Float;False;0;COLOR;0.0,0,0,0;False;1;FLOAT;0.0,0,0,0;False;COLOR
Node;AmplifyShaderEditor.SimpleAddOpNode;36;-84.82996,941.7123;Float;False;0;COLOR;0.0,0,0,0;False;1;COLOR;0,0,0,0;False;COLOR
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;68;553.705,438.7357;Float;False;0;FLOAT;0.0;False;1;FLOAT;0.0;False;FLOAT
Node;AmplifyShaderEditor.SimpleAddOpNode;16;-84.70087,40.30001;Float;False;0;COLOR;0.0,0,0,0;False;1;COLOR;0,0,0,0;False;COLOR
Node;AmplifyShaderEditor.RangedFloatNode;70;678.5059,139.5359;Float;False;Constant;_Float1;Float 1;10;0;0.5;0;0;FLOAT
Node;AmplifyShaderEditor.ColorNode;57;673.3042,210.9364;Float;False;Property;_GradientColor;GradientColor;7;0;0,0,0,0;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.SimpleAddOpNode;35;17.01321,1083.547;Float;False;0;COLOR;0,0,0,0;False;1;COLOR;0.0;False;COLOR
Node;AmplifyShaderEditor.SimpleAddOpNode;21;41.76092,186.6099;Float;False;0;COLOR;0,0,0,0;False;1;COLOR;0.0;False;COLOR
Node;AmplifyShaderEditor.ClampOpNode;63;545.7041,565.1357;Float;False;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;1.0;False;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;69;744.1058,464.3357;Float;False;0;COLOR;0,0,0,0;False;1;FLOAT;0,0,0,0;False;COLOR
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;58;747.0043,560.8359;Float;False;0;COLOR;0.0,0,0,0;False;1;FLOAT;0.0,0,0,0;False;COLOR
Node;AmplifyShaderEditor.RangedFloatNode;48;260.7427,755.764;Float;False;Property;_SurfaceShine;SurfaceShine;6;0;0.2;0;0;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;78;260.9979,887.6682;Float;False;Property;_GradientShine;GradientShine;8;0;0;0;0;FLOAT
Node;AmplifyShaderEditor.SimpleAddOpNode;37;253.6085,634.4438;Float;False;0;COLOR;0.0,0,0,0;False;1;COLOR;0.0,0,0,0;False;COLOR
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;79;492.9243,868.4537;Float;False;0;COLOR;0.0;False;1;FLOAT;0,0,0,0;False;COLOR
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;77;486.067,760.04;Float;False;0;COLOR;0.0;False;1;FLOAT;0.0,0,0,0;False;COLOR
Node;AmplifyShaderEditor.BlendOpsNode;61;776.9061,855.8755;Float;False;Lighten;True;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;COLOR
Node;AmplifyShaderEditor.BlendOpsNode;60;777.4705,746.7618;Float;False;Lighten;True;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;COLOR
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1418.901,640.7928;Float;False;True;2;Float;ASEMaterialInspector;0;Standard;SixSideLighting;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;0;False;0;0;Opaque;0.5;True;True;0;False;Opaque;Geometry;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;False;0;255;255;0;0;0;0;False;0;4;10;25;False;0.5;True;0;SrcAlpha;OneMinusSrcAlpha;0;Zero;Zero;Add;Add;0;False;0.04;0,0,0,0;VertexOffset;False;Cylindrical;Relative;0;;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0.0;False;4;FLOAT;0.0;False;5;FLOAT;0.0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0.0;False;9;FLOAT;0.0;False;10;OBJECT;0.0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;13;OBJECT;0.0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False
WireConnection;25;0;9;0
WireConnection;25;1;31;0
WireConnection;4;0;9;0
WireConnection;4;1;5;0
WireConnection;13;0;9;0
WireConnection;13;1;14;0
WireConnection;24;0;9;0
WireConnection;24;1;30;0
WireConnection;75;0;76;0
WireConnection;75;1;59;0
WireConnection;17;0;9;0
WireConnection;17;1;20;0
WireConnection;40;0;13;0
WireConnection;43;0;24;0
WireConnection;41;0;4;0
WireConnection;44;0;25;0
WireConnection;62;0;75;0
WireConnection;62;1;51;0
WireConnection;27;0;9;0
WireConnection;27;1;32;0
WireConnection;7;0;1;0
WireConnection;7;1;41;0
WireConnection;45;0;27;0
WireConnection;54;0;66;0
WireConnection;54;1;55;0
WireConnection;23;0;28;0
WireConnection;23;1;43;0
WireConnection;26;0;29;0
WireConnection;26;1;44;0
WireConnection;42;0;17;0
WireConnection;71;0;62;0
WireConnection;71;1;81;0
WireConnection;12;0;15;0
WireConnection;12;1;40;0
WireConnection;18;0;19;0
WireConnection;18;1;42;0
WireConnection;33;0;34;0
WireConnection;33;1;45;0
WireConnection;36;0;23;0
WireConnection;36;1;26;0
WireConnection;68;0;71;0
WireConnection;68;1;54;0
WireConnection;16;0;7;0
WireConnection;16;1;12;0
WireConnection;35;0;36;0
WireConnection;35;1;33;0
WireConnection;21;0;16;0
WireConnection;21;1;18;0
WireConnection;63;0;68;0
WireConnection;69;0;57;0
WireConnection;69;1;70;0
WireConnection;58;0;69;0
WireConnection;58;1;63;0
WireConnection;37;0;21;0
WireConnection;37;1;35;0
WireConnection;79;0;58;0
WireConnection;79;1;78;0
WireConnection;77;0;37;0
WireConnection;77;1;48;0
WireConnection;61;0;77;0
WireConnection;61;1;79;0
WireConnection;60;0;37;0
WireConnection;60;1;58;0
WireConnection;0;0;60;0
WireConnection;0;2;61;0
ASEEND*/
//CHKSM=973C4D6BA99013295F98DE0B1FB591BEA9CCEBB6