// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "TerminatedRock"
{
	Properties
	{
		_Color("Color", Color) = (0,0,0,0)
		_Albedo("Albedo", 2D) = "white" {}
		_Normal("Normal", 2D) = "bump" {}
		_DisolveGuide("Disolve Guide", 2D) = "white" {}
		_DetailTex("DetailTex", 2D) = "white" {}
		_GradientOffset("GradientOffset", Range( -1 , 1)) = 0
		_GradientRange("GradientRange", Range( -1 , 1)) = 0
		_DissolveAmount("Dissolve Amount", Range( 0 , 1)) = 0
		_RampBrightness("RampBrightness", Range( 1 , 5)) = 1
		_GlowFadeHeight("GlowFadeHeight", Range( 0 , 40)) = 0
		_GlowFalloff("GlowFalloff", Range( 1 , 8)) = 0
		_GlowFadeOffset("GlowFadeOffset", Range( -40 , 40)) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Off
		ZTest LEqual
		CGINCLUDE
		#include "UnityShaderVariables.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#define ASE_TEXTURE_PARAMS(textureName) textureName

		#ifdef UNITY_PASS_SHADOWCASTER
			#undef INTERNAL_DATA
			#undef WorldReflectionVector
			#undef WorldNormalVector
			#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
			#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
			#define WorldNormalVector(data,normal) half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))
		#endif
		struct Input
		{
			float2 uv_texcoord;
			float3 worldPos;
			float3 worldNormal;
			INTERNAL_DATA
		};

		uniform sampler2D _Normal;
		uniform float4 _Normal_ST;
		uniform float4 _Color;
		uniform sampler2D _Albedo;
		uniform float4 _Albedo_ST;
		uniform float _DissolveAmount;
		uniform sampler2D _DisolveGuide;
		uniform float4 _DisolveGuide_ST;
		uniform float GlowHeight;
		uniform float _GlowFadeOffset;
		uniform float _GlowFadeHeight;
		uniform float _GlowFalloff;
		uniform sampler2D _TransitionRamp;
		uniform float _GradientOffset;
		uniform sampler2D _DetailTex;
		uniform float _GradientRange;
		uniform float _RampBrightness;


		inline float4 TriplanarSamplingSF( sampler2D topTexMap, float3 worldPos, float3 worldNormal, float falloff, float2 tiling, float3 normalScale, float3 index )
		{
			float3 projNormal = ( pow( abs( worldNormal ), falloff ) );
			projNormal /= ( projNormal.x + projNormal.y + projNormal.z ) + 0.00001;
			float3 nsign = sign( worldNormal );
			half4 xNorm; half4 yNorm; half4 zNorm;
			xNorm = ( tex2D( ASE_TEXTURE_PARAMS( topTexMap ), tiling * worldPos.zy * float2( nsign.x, 1.0 ) ) );
			yNorm = ( tex2D( ASE_TEXTURE_PARAMS( topTexMap ), tiling * worldPos.xz * float2( nsign.y, 1.0 ) ) );
			zNorm = ( tex2D( ASE_TEXTURE_PARAMS( topTexMap ), tiling * worldPos.xy * float2( -nsign.z, 1.0 ) ) );
			return xNorm * projNormal.x + yNorm * projNormal.y + zNorm * projNormal.z;
		}


		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_Normal = i.uv_texcoord * _Normal_ST.xy + _Normal_ST.zw;
			o.Normal = UnpackNormal( tex2D( _Normal, uv_Normal ) );
			float2 uv_Albedo = i.uv_texcoord * _Albedo_ST.xy + _Albedo_ST.zw;
			float2 uv_DisolveGuide = i.uv_texcoord * _DisolveGuide_ST.xy + _DisolveGuide_ST.zw;
			float3 ase_worldPos = i.worldPos;
			float temp_output_217_0 = ( GlowHeight + _GlowFadeOffset );
			float clampResult166 = clamp( ( 1.0 - (0.0 + (ase_worldPos.y - temp_output_217_0) * (1.0 - 0.0) / (( _GlowFadeHeight + temp_output_217_0 ) - temp_output_217_0)) ) , 0.0 , 1.0 );
			float temp_output_194_0 = ( (-4.0 + (( (-0.6 + (( 1.0 - _DissolveAmount ) - 0.0) * (0.6 - -0.6) / (1.0 - 0.0)) + tex2D( _DisolveGuide, uv_DisolveGuide ).r ) - 0.0) * (4.0 - -4.0) / (1.0 - 0.0)) * pow( clampResult166 , _GlowFalloff ) );
			float clampResult113 = clamp( temp_output_194_0 , 0.0 , 0.95 );
			float2 appendResult115 = (float2(clampResult113 , 0.0));
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float4 triplanar207 = TriplanarSamplingSF( _DetailTex, ( ase_worldPos + ( float3(0,0.2,0.1) * _Time.y ) ), ase_worldNormal, 2.0, float2( 0.025,0.025 ), 1.0, 0 );
			float3 desaturateInitialColor204 = triplanar207.xyz;
			float desaturateDot204 = dot( desaturateInitialColor204, float3( 0.299, 0.587, 0.114 ));
			float3 desaturateVar204 = lerp( desaturateInitialColor204, desaturateDot204.xxx, 1.0 );
			float3 clampResult206 = clamp( desaturateVar204 , float3( 0,0,0 ) , float3( 1,0,0 ) );
			float2 appendResult198 = (float2(( _GradientOffset + ( clampResult206 * _GradientRange ) ).xy));
			float4 temp_output_137_0 = ( ( clampResult113 * tex2D( _TransitionRamp, appendResult115 ) ) * tex2D( _TransitionRamp, appendResult198 ) );
			float4 lerpResult197 = lerp( ( _Color * tex2D( _Albedo, uv_Albedo ) ) , temp_output_137_0 , temp_output_194_0);
			o.Albedo = lerpResult197.rgb;
			o.Emission = ( temp_output_137_0 * _RampBrightness ).rgb;
			o.Alpha = 1;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard keepalpha fullforwardshadows 

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
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float4 tSpace0 : TEXCOORD2;
				float4 tSpace1 : TEXCOORD3;
				float4 tSpace2 : TEXCOORD4;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				half3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				half3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.customPack1.xy;
				float3 worldPos = float3( IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w );
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = float3( IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z );
				surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
				surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
				surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
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
Version=17300
7;29;1906;1004;3639.264;-226.7807;1;True;False
Node;AmplifyShaderEditor.RangedFloatNode;216;-3367.945,1265.238;Inherit;False;Property;_GlowFadeOffset;GlowFadeOffset;13;0;Create;True;0;0;False;0;0;0;-40;40;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;151;-3327.88,1169.547;Inherit;False;Global;GlowHeight;GlowHeight;9;0;Create;True;0;0;True;0;0;5.685919;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;160;-3187.079,847.1588;Inherit;False;Property;_GlowFadeHeight;GlowFadeHeight;11;0;Create;True;0;0;False;0;0;40;0;40;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;217;-3014.945,1210.238;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;4;-2704.256,593.2405;Float;False;Property;_DissolveAmount;Dissolve Amount;8;0;Create;True;0;0;False;0;0;0.2;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;212;-2992.093,1624.497;Inherit;False;Constant;_Vector0;Vector 0;14;0;Create;True;0;0;False;0;0,0.2,0.1;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleTimeNode;214;-2977.315,1783.879;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;182;-2826.139,1053.353;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;144;-3166.256,955.0196;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldPosInputsNode;208;-2872.792,1455.011;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.TFHCRemapNode;189;-2435.94,1004.951;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;213;-2696.181,1687.04;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.OneMinusNode;71;-2174.064,626.1104;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;111;-1718.752,571.8904;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;-0.6;False;4;FLOAT;0.6;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;2;-1767.125,742.5328;Inherit;True;Property;_DisolveGuide;Disolve Guide;3;0;Create;True;0;0;False;0;-1;None;9fca23c86a0dd0f4bbb5707b8a85a933;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;210;-2625.761,1525.336;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.OneMinusNode;190;-2124.203,999.1119;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;73;-1328.231,499.6288;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TriplanarNode;207;-2474.082,1442.691;Inherit;True;Spherical;World;False;DetailTex;_DetailTex;white;4;None;Mid Texture 0;_MidTexture0;white;-1;None;Bot Texture 0;_BotTexture0;white;-1;None;Triplanar Sampler;False;10;0;SAMPLER2D;;False;5;FLOAT;1;False;1;SAMPLER2D;;False;6;FLOAT;0;False;2;SAMPLER2D;;False;7;FLOAT;0;False;9;FLOAT3;0,0,0;False;8;FLOAT;1;False;3;FLOAT2;0.025,0.025;False;4;FLOAT;2;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;192;-1942.855,1162.288;Inherit;False;Property;_GlowFalloff;GlowFalloff;12;0;Create;True;0;0;False;0;0;8;1;8;0;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;166;-1839.424,991.6462;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.DesaturateOpNode;204;-1986.038,1461.743;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TFHCRemapNode;112;-1144.444,311.5808;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;-4;False;4;FLOAT;4;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;191;-1536.882,1076.161;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;205;-1998.779,1643.636;Inherit;False;Property;_GradientRange;GradientRange;7;0;Create;True;0;0;False;0;0;0.9;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;206;-1610.722,1457.899;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;1,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;194;-884.0254,467.6877;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;202;-1462.254,1550.45;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ClampOpNode;113;-745.4754,223.7879;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0.95;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;200;-1954.919,1349.615;Inherit;False;Property;_GradientOffset;GradientOffset;6;0;Create;True;0;0;False;0;0;0.3;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;201;-1425.261,1379.477;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DynamicAppendNode;115;-530.7379,327.6015;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;114;-327.6432,281.8498;Inherit;True;Global;_TransitionRamp;_TransitionRamp;5;0;Create;True;0;0;False;0;-1;None;64e7766099ad46747a07014e44d0aea1;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;0,0;False;1;FLOAT2;1,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;198;-1183.582,1385.613;Inherit;False;FLOAT2;4;0;FLOAT2;0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ColorNode;133;-81.88253,-384.7786;Inherit;False;Property;_Color;Color;0;0;Create;True;0;0;False;0;0,0,0,0;0.3308819,0.3308819,0.3308819,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;78;-535.8436,-310.9727;Inherit;True;Property;_Albedo;Albedo;1;0;Create;True;0;0;False;0;-1;None;87e2fd36aeaf261428bce6d802259b48;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;126;-19.3633,213.3658;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;199;-997.7412,1354.721;Inherit;True;Property;_TextureSample1;Texture Sample 1;5;0;Create;True;0;0;False;0;-1;None;2df85a5b75677a7439c55bf1ff004d37;True;0;False;white;Auto;False;Instance;114;Auto;Texture2D;6;0;SAMPLER2D;0,0;False;1;FLOAT2;1,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;141;58.38425,725.6427;Inherit;False;Property;_RampBrightness;RampBrightness;10;0;Create;True;0;0;False;0;1;2;1;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;137;183.6517,347.2855;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;134;292.5178,-87.07939;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;140;425.2669,700.0273;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.PannerNode;138;-4033.56,1336.926;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,-0.01;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.LerpOp;197;520.4248,8.333839;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;209;-4369.179,1289.187;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;131;-528.9641,-103.127;Inherit;True;Property;_Normal;Normal;2;0;Create;True;0;0;False;0;-1;None;e96e2d8d2edc36148ac7447132fab8b9;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LengthOpNode;215;-1788.029,1538.26;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;135;-3687.889,1330.492;Inherit;True;Property;_DetailTexOld;DetailTexOld;9;0;Create;True;0;0;False;0;-1;None;f7e96904e8667e1439548f0f86389447;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;759.4052,126.6526;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;TerminatedRock;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Off;0;False;-1;3;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;0;4;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;5;False;-1;10;False;-1;1;False;-1;1;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;2;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;217;0;151;0
WireConnection;217;1;216;0
WireConnection;182;0;160;0
WireConnection;182;1;217;0
WireConnection;189;0;144;2
WireConnection;189;1;217;0
WireConnection;189;2;182;0
WireConnection;213;0;212;0
WireConnection;213;1;214;0
WireConnection;71;0;4;0
WireConnection;111;0;71;0
WireConnection;210;0;208;0
WireConnection;210;1;213;0
WireConnection;190;0;189;0
WireConnection;73;0;111;0
WireConnection;73;1;2;1
WireConnection;207;9;210;0
WireConnection;166;0;190;0
WireConnection;204;0;207;0
WireConnection;112;0;73;0
WireConnection;191;0;166;0
WireConnection;191;1;192;0
WireConnection;206;0;204;0
WireConnection;194;0;112;0
WireConnection;194;1;191;0
WireConnection;202;0;206;0
WireConnection;202;1;205;0
WireConnection;113;0;194;0
WireConnection;201;0;200;0
WireConnection;201;1;202;0
WireConnection;115;0;113;0
WireConnection;114;1;115;0
WireConnection;198;0;201;0
WireConnection;126;0;113;0
WireConnection;126;1;114;0
WireConnection;199;1;198;0
WireConnection;137;0;126;0
WireConnection;137;1;199;0
WireConnection;134;0;133;0
WireConnection;134;1;78;0
WireConnection;140;0;137;0
WireConnection;140;1;141;0
WireConnection;138;0;209;0
WireConnection;197;0;134;0
WireConnection;197;1;137;0
WireConnection;197;2;194;0
WireConnection;215;0;204;0
WireConnection;135;1;138;0
WireConnection;0;0;197;0
WireConnection;0;1;131;0
WireConnection;0;2;140;0
ASEEND*/
//CHKSM=E0373D9E19F5B689E701B04FD39E4E0BE9B4987E