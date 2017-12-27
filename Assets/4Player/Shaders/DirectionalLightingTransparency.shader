// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "DirectionalLightingTransparency"
{
	Properties
	{
		[HideInInspector] __dirty( "", Int ) = 1
		_light_1("light_1", Vector) = (1,0,0,0)
		_Color_1("Color_1", Color) = (0.8088235,0.4222535,0.4222535,1)
		_light_2("light_2", Vector) = (0,1,0,0)
		_Color_2("Color_2", Color) = (0.7099479,0.3073097,0.8529412,1)
		_light_3("light_3", Vector) = (0,0,1,0)
		_Color_3("Color_3", Color) = (0.3740492,0.625,0.1332721,1)
		_light_4("light_4", Vector) = (1,0,0,0)
		_Color_4("Color_4", Color) = (0.8088235,0.4222535,0.4222535,1)
		_light_5("light_5", Vector) = (0,1,0,0)
		_Color_5("Color_5", Color) = (0.7099479,0.3073097,0.8529412,1)
		_light_6("light_6", Vector) = (0,0,1,0)
		_Color_6("Color_6", Color) = (0.3740492,0.625,0.1332721,1)
		_shine("shine", Float) = 0.2
		_translucencyStrength("translucencyStrength", Float) = 0
		_transmissionStrength("transmissionStrength", Float) = 0
		[Header(Translucency)]
		_Translucency("Strength", Range( 0 , 50)) = 1
		_TransNormalDistortion("Normal Distortion", Range( 0 , 1)) = 0.1
		_TransScattering("Scaterring Falloff", Range( 1 , 50)) = 2
		_TransDirect("Direct", Range( 0 , 1)) = 1
		_TransAmbient("Ambient", Range( 0 , 1)) = 0.2
		_TransShadow("Shadow", Range( 0 , 1)) = 0.9
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
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
			float3 worldNormal;
		};

		struct SurfaceOutputStandardCustom
		{
			fixed3 Albedo;
			fixed3 Normal;
			half3 Emission;
			half Metallic;
			half Smoothness;
			half Occlusion;
			fixed Alpha;
			fixed3 Transmission;
			fixed3 Translucency;
		};

		uniform float4 _Color_1;
		uniform float3 _light_1;
		uniform float4 _Color_2;
		uniform float3 _light_2;
		uniform float4 _Color_3;
		uniform float3 _light_3;
		uniform float4 _Color_4;
		uniform float3 _light_4;
		uniform float4 _Color_5;
		uniform float3 _light_5;
		uniform float4 _Color_6;
		uniform float3 _light_6;
		uniform float _shine;
		uniform float _transmissionStrength;
		uniform half _Translucency;
		uniform half _TransNormalDistortion;
		uniform half _TransScattering;
		uniform half _TransDirect;
		uniform half _TransAmbient;
		uniform half _TransShadow;
		uniform float _translucencyStrength;

		inline half4 LightingStandardCustom(SurfaceOutputStandardCustom s, half3 viewDir, UnityGI gi )
		{
			#if !DIRECTIONAL
			float3 lightAtten = gi.light.color;
			#else
			float3 lightAtten = lerp( _LightColor0, gi.light.color, _TransShadow );
			#endif
			half3 lightDir = gi.light.dir + s.Normal * _TransNormalDistortion;
			half transVdotL = pow( saturate( dot( viewDir, -lightDir ) ), _TransScattering );
			half3 translucency = lightAtten * (transVdotL * _TransDirect + gi.indirect.diffuse * _TransAmbient) * s.Translucency;
			half4 c = half4( s.Albedo * translucency * _Translucency, 0 );

			half3 transmission = max(0 , -dot(s.Normal, gi.light.dir)) * gi.light.color * s.Transmission;
			half4 d = half4(s.Albedo * transmission , 0);

			SurfaceOutputStandard r;
			r.Albedo = s.Albedo;
			r.Normal = s.Normal;
			r.Emission = s.Emission;
			r.Metallic = s.Metallic;
			r.Smoothness = s.Smoothness;
			r.Occlusion = s.Occlusion;
			r.Alpha = s.Alpha;
			return LightingStandard (r, viewDir, gi) + c + d;
		}

		inline void LightingStandardCustom_GI(SurfaceOutputStandardCustom s, UnityGIInput data, inout UnityGI gi )
		{
			UNITY_GI(gi, s, data);
		}

		void surf( Input i , inout SurfaceOutputStandardCustom o )
		{
			float4 temp_output_37_0 = ( ( ( ( _Color_1 * clamp( dot( i.worldNormal , _light_1 ) , 0.0 , 1.0 ) ) + ( _Color_2 * clamp( dot( i.worldNormal , _light_2 ) , 0.0 , 1.0 ) ) ) + ( _Color_3 * clamp( dot( i.worldNormal , _light_3 ) , 0.0 , 1.0 ) ) ) + ( ( ( _Color_4 * clamp( dot( i.worldNormal , _light_4 ) , 0.0 , 1.0 ) ) + ( _Color_5 * clamp( dot( i.worldNormal , _light_5 ) , 0.0 , 1.0 ) ) ) + ( _Color_6 * clamp( dot( i.worldNormal , _light_6 ) , 0.0 , 1.0 ) ) ) );
			o.Albedo = temp_output_37_0.rgb;
			o.Emission = ( temp_output_37_0 * _shine ).rgb;
			float3 temp_cast_2 = (_transmissionStrength).xxx;
			o.Transmission = temp_cast_2;
			float3 temp_cast_3 = (_translucencyStrength).xxx;
			o.Translucency = temp_cast_3;
			o.Alpha = 1;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf StandardCustom keepalpha 

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
				surfIN.worldNormal = float3( IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z );
				SurfaceOutputStandardCustom o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandardCustom, o )
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
-1913;29;1906;1004;1427.303;92.22534;1.6;True;True
Node;AmplifyShaderEditor.Vector3Node;5;-765.4,122.1;Float;False;Property;_light_1;light_1;0;0;1,0,0;FLOAT3;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.Vector3Node;31;-784.3475,1162.137;Float;False;Property;_light_5;light_5;8;0;0,1,0;FLOAT3;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.Vector3Node;30;-790.1475,1019.037;Float;False;Property;_light_4;light_4;6;0;1,0,0;FLOAT3;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.Vector3Node;14;-759.6,265.2003;Float;False;Property;_light_2;light_2;2;0;0,1,0;FLOAT3;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.WorldNormalVector;9;-1234.378,564.4238;Float;False;0;FLOAT3;0,0,0;False;FLOAT3;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.DotProductOpNode;4;-618.8,128.5999;Float;False;0;FLOAT3;0.0,0,0;False;1;FLOAT3;0.0,0,0;False;FLOAT
Node;AmplifyShaderEditor.Vector3Node;32;-783.4229,1320.913;Float;False;Property;_light_6;light_6;10;0;0,0,1;FLOAT3;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.DotProductOpNode;24;-643.5474,1025.537;Float;False;0;FLOAT3;0.0,0,0;False;1;FLOAT3;0.0,0,0;False;FLOAT
Node;AmplifyShaderEditor.DotProductOpNode;13;-621.8,256.1004;Float;False;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;FLOAT
Node;AmplifyShaderEditor.DotProductOpNode;25;-646.5474,1153.038;Float;False;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;FLOAT
Node;AmplifyShaderEditor.Vector3Node;20;-758.6755,426.5135;Float;False;Property;_light_3;light_3;4;0;0,0,1;FLOAT3;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.ClampOpNode;44;-510.0221,1127.999;Float;False;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;1.0;False;FLOAT
Node;AmplifyShaderEditor.ClampOpNode;43;-490.3649,975.6569;Float;False;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;1.0;False;FLOAT
Node;AmplifyShaderEditor.DotProductOpNode;27;-645.623,1311.813;Float;False;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;FLOAT
Node;AmplifyShaderEditor.ColorNode;28;-352.8483,737.8361;Float;False;Property;_Color_4;Color_4;7;0;0.8088235,0.4222535,0.4222535,1;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.ClampOpNode;41;-506.6512,116.9146;Float;False;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;1.0;False;FLOAT
Node;AmplifyShaderEditor.ColorNode;29;-341.0481,1024.337;Float;False;Property;_Color_5;Color_5;9;0;0.7099479,0.3073097,0.8529412,1;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.ClampOpNode;40;-495.9466,258.7501;Float;False;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;1.0;False;FLOAT
Node;AmplifyShaderEditor.ColorNode;15;-316.3003,127.4;Float;False;Property;_Color_2;Color_2;3;0;0.7099479,0.3073097,0.8529412,1;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.DotProductOpNode;17;-620.8755,414.8755;Float;False;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;FLOAT
Node;AmplifyShaderEditor.ColorNode;1;-320.5004,-166.6998;Float;False;Property;_Color_1;Color_1;1;0;0.8088235,0.4222535,0.4222535,1;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;26;-339.5478,1188.737;Float;False;0;COLOR;0.0,0,0,0;False;1;FLOAT;0.0,0,0,0;False;COLOR
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;23;-344.7478,899.174;Float;False;0;COLOR;0.0,0,0,0;False;1;FLOAT;0.0,0,0,0;False;COLOR
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;12;-314.8,291.8;Float;False;0;COLOR;0.0,0,0,0;False;1;FLOAT;0.0,0,0,0;False;COLOR
Node;AmplifyShaderEditor.ColorNode;19;-336.7229,401.251;Float;False;Property;_Color_3;Color_3;5;0;0.3740492,0.625,0.1332721,1;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;7;-320,0;Float;False;0;COLOR;0.0,0,0,0;False;1;FLOAT;0.0,0,0,0;False;COLOR
Node;AmplifyShaderEditor.ColorNode;34;-341.8139,1283.446;Float;False;Property;_Color_6;Color_6;11;0;0.3740492,0.625,0.1332721,1;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.ClampOpNode;45;-527.7416,1338.71;Float;False;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;1.0;False;FLOAT
Node;AmplifyShaderEditor.ClampOpNode;42;-486.5797,436.7138;Float;False;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;1.0;False;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;18;-230.3862,634.4498;Float;False;0;COLOR;0.0,0,0,0;False;1;FLOAT;0.0,0,0,0;False;COLOR
Node;AmplifyShaderEditor.SimpleAddOpNode;36;-84.82996,941.7123;Float;False;0;COLOR;0.0,0,0,0;False;1;COLOR;0,0,0,0;False;COLOR
Node;AmplifyShaderEditor.SimpleAddOpNode;16;-84.70087,40.30001;Float;False;0;COLOR;0.0,0,0,0;False;1;COLOR;0,0,0,0;False;COLOR
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;33;-332.1232,1451.122;Float;False;0;COLOR;0.0,0,0,0;False;1;FLOAT;0.0,0,0,0;False;COLOR
Node;AmplifyShaderEditor.SimpleAddOpNode;21;41.76092,186.6099;Float;False;0;COLOR;0,0,0,0;False;1;COLOR;0.0;False;COLOR
Node;AmplifyShaderEditor.SimpleAddOpNode;35;17.01321,1083.547;Float;False;0;COLOR;0,0,0,0;False;1;COLOR;0.0;False;COLOR
Node;AmplifyShaderEditor.SimpleAddOpNode;37;253.6085,634.4438;Float;False;0;COLOR;0.0,0,0,0;False;1;COLOR;0.0,0,0,0;False;COLOR
Node;AmplifyShaderEditor.RangedFloatNode;48;256.6256,896.86;Float;False;Property;_shine;shine;12;0;0.2;0;0;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;52;250.9065,1090.135;Float;False;Property;_translucencyStrength;translucencyStrength;13;0;0;0;0;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;51;275.6069,993.035;Float;False;Property;_transmissionStrength;transmissionStrength;13;0;0;0;0;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;46;318.2251,735.56;Float;False;0;COLOR;0.0,0,0,0;False;1;FLOAT;0,0,0,0;False;COLOR
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;475.3874,521.7082;Float;False;True;2;Float;ASEMaterialInspector;0;Standard;DirectionalLightingTransparency;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;0;False;0;0;Opaque;0.5;True;True;0;False;Opaque;Geometry;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;False;0;255;255;0;0;0;0;False;0;4;10;25;False;0.5;True;0;Zero;Zero;0;Zero;Zero;Add;Add;0;False;0;0,0,0,0;VertexOffset;False;Cylindrical;Relative;0;;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0.0;False;4;FLOAT;0.0;False;5;FLOAT;0.0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0.0;False;9;FLOAT;0.0;False;10;OBJECT;0.0;False;11;FLOAT3;0.0,0,0;False;12;FLOAT3;0.0,0,0;False;13;OBJECT;0.0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False
WireConnection;4;0;9;0
WireConnection;4;1;5;0
WireConnection;24;0;9;0
WireConnection;24;1;30;0
WireConnection;13;0;9;0
WireConnection;13;1;14;0
WireConnection;25;0;9;0
WireConnection;25;1;31;0
WireConnection;44;0;25;0
WireConnection;43;0;24;0
WireConnection;27;0;9;0
WireConnection;27;1;32;0
WireConnection;41;0;4;0
WireConnection;40;0;13;0
WireConnection;17;0;9;0
WireConnection;17;1;20;0
WireConnection;26;0;29;0
WireConnection;26;1;44;0
WireConnection;23;0;28;0
WireConnection;23;1;43;0
WireConnection;12;0;15;0
WireConnection;12;1;40;0
WireConnection;7;0;1;0
WireConnection;7;1;41;0
WireConnection;45;0;27;0
WireConnection;42;0;17;0
WireConnection;18;0;19;0
WireConnection;18;1;42;0
WireConnection;36;0;23;0
WireConnection;36;1;26;0
WireConnection;16;0;7;0
WireConnection;16;1;12;0
WireConnection;33;0;34;0
WireConnection;33;1;45;0
WireConnection;21;0;16;0
WireConnection;21;1;18;0
WireConnection;35;0;36;0
WireConnection;35;1;33;0
WireConnection;37;0;21;0
WireConnection;37;1;35;0
WireConnection;46;0;37;0
WireConnection;46;1;48;0
WireConnection;0;0;37;0
WireConnection;0;2;46;0
WireConnection;0;6;51;0
WireConnection;0;7;52;0
ASEEND*/
//CHKSM=94B9A7666DA795A9ADBBE0F0DF7FE9C76A67483D