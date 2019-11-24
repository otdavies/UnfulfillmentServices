// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "TerminatorPlane"
{
	Properties
	{
		_FadeOutColor("FadeOutColor", Color) = (0,0,0,0)
		_RampBrightness("RampBrightness", Range( 0 , 5)) = 0
		_GradientOffset("GradientOffset", Range( -1 , 1)) = 0
		_GradientRange("GradientRange", Range( -1 , 1)) = 0
		_FadeDepth("FadeDepth", Float) = 0
		_DetailTex("DetailTex", 2D) = "white" {}
		_DistortionTex("DistortionTex", 2D) = "white" {}
		_Distortion("Distortion", Range( -0.1 , 0.1)) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Custom"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Back
		ZWrite On
		ZTest LEqual
		Blend SrcAlpha OneMinusSrcAlpha
		
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#include "UnityCG.cginc"
		#pragma target 3.0
		#pragma surface surf Standard keepalpha noshadow exclude_path:deferred 
		struct Input
		{
			float2 uv_texcoord;
			float4 screenPos;
		};

		uniform float4 _FadeOutColor;
		uniform sampler2D _TransitionRamp;
		uniform float _GradientOffset;
		uniform sampler2D _DetailTex;
		uniform sampler2D _DistortionTex;
		uniform float _Distortion;
		uniform float _GradientRange;
		uniform float _RampBrightness;
		UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
		uniform float4 _CameraDepthTexture_TexelSize;
		uniform float _FadeDepth;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 panner57 = ( 0.025 * _Time.y * float2( 0.1,0 ) + float2( 0,0 ));
			float2 uv_TexCoord53 = i.uv_texcoord * float2( 0.5,0.5 );
			float2 panner56 = ( 0.1 * _Time.y * float2( 0.1,-0.05 ) + ( ( uv_TexCoord53 + float2( -0.5,-0.5 ) ) * float2( 2,2 ) ));
			float2 uv_TexCoord46 = i.uv_texcoord + ( tex2D( _DistortionTex, panner56 ) * _Distortion ).rg;
			float3 desaturateInitialColor68 = tex2D( _DetailTex, ( panner57 + uv_TexCoord46 ) ).rgb;
			float desaturateDot68 = dot( desaturateInitialColor68, float3( 0.299, 0.587, 0.114 ));
			float3 desaturateVar68 = lerp( desaturateInitialColor68, desaturateDot68.xxx, 1.0 );
			float2 appendResult72 = (float2(( _GradientOffset + ( desaturateVar68 * _GradientRange ) ).xy));
			o.Emission = ( _FadeOutColor * ( tex2D( _TransitionRamp, appendResult72 ) * _RampBrightness ) ).rgb;
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			float screenDepth41 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
			float distanceDepth41 = saturate( abs( ( screenDepth41 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( _FadeDepth ) ) );
			o.Alpha = distanceDepth41;
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17300
7;29;1906;1004;4129.845;1585.828;2.434272;True;False
Node;AmplifyShaderEditor.TextureCoordinatesNode;53;-4120.1,-48.80159;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;0.5,0.5;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;54;-3758.892,-17.53837;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;-0.5,-0.5;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;55;-3550.312,-28.30991;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;2,2;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;56;-3305.924,-23.05788;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0.1,-0.05;False;1;FLOAT;0.1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;52;-3014.74,215.832;Inherit;False;Property;_Distortion;Distortion;9;0;Create;True;0;0;False;0;0;-0.055;-0.1;0.1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;48;-3016.515,1.228924;Inherit;True;Property;_DistortionTex;DistortionTex;8;0;Create;True;0;0;False;0;-1;None;e28dc97a9541e3642a48c0e3886688c5;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;51;-2620.343,60.22227;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.PannerNode;57;-2578.688,-125.7001;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0.1,0;False;1;FLOAT;0.025;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;46;-2454.838,46.64852;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;58;-2175.07,-96.71952;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;1;-2058.255,-298.3438;Inherit;True;Property;_DetailTex;DetailTex;7;0;Create;True;0;0;False;0;-1;None;f7e96904e8667e1439548f0f86389447;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;76;-1432.639,-124.6823;Inherit;False;Property;_GradientRange;GradientRange;5;0;Create;True;0;0;False;0;0;1;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.DesaturateOpNode;68;-1710.641,-277.2151;Inherit;True;2;0;FLOAT3;0,0,0;False;1;FLOAT;1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;74;-1374.683,-371.7773;Inherit;False;Property;_GradientOffset;GradientOffset;4;0;Create;True;0;0;False;0;0;0;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;75;-1318.444,-282.6822;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;73;-1084.297,-342.4224;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DynamicAppendNode;72;-901.6801,-350.9909;Inherit;False;FLOAT2;4;0;FLOAT2;0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;69;-719.3203,-388.1382;Inherit;True;Global;_TransitionRamp;_TransitionRamp;3;0;Create;True;0;0;False;0;-1;None;64e7766099ad46747a07014e44d0aea1;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;0,0;False;1;FLOAT2;1,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;79;-730.2729,-86.1173;Inherit;False;Property;_RampBrightness;RampBrightness;1;0;Create;True;0;0;False;0;0;2;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;13;-811.0101,140.8209;Inherit;False;Property;_FadeDepth;FadeDepth;6;0;Create;True;0;0;False;0;0;2.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;81;-380.4386,-480.4016;Inherit;False;Property;_FadeOutColor;FadeOutColor;0;0;Create;True;0;0;False;0;0,0,0,0;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;78;-403.6749,-191.3583;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.DepthFade;41;-532.8724,59.1449;Inherit;False;True;True;True;2;1;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;84;-128.307,-224.3557;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;116.0351,-139.1254;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;TerminatorPlane;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Back;1;False;-1;3;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;False;0;True;Custom;;Transparent;ForwardOnly;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;2;5;False;-1;10;False;-1;0;5;False;-1;10;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;2;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;54;0;53;0
WireConnection;55;0;54;0
WireConnection;56;0;55;0
WireConnection;48;1;56;0
WireConnection;51;0;48;0
WireConnection;51;1;52;0
WireConnection;46;1;51;0
WireConnection;58;0;57;0
WireConnection;58;1;46;0
WireConnection;1;1;58;0
WireConnection;68;0;1;0
WireConnection;75;0;68;0
WireConnection;75;1;76;0
WireConnection;73;0;74;0
WireConnection;73;1;75;0
WireConnection;72;0;73;0
WireConnection;69;1;72;0
WireConnection;78;0;69;0
WireConnection;78;1;79;0
WireConnection;41;0;13;0
WireConnection;84;0;81;0
WireConnection;84;1;78;0
WireConnection;0;2;84;0
WireConnection;0;9;41;0
ASEEND*/
//CHKSM=D76E27C2A9CD6598A28B3552A97723FF8A31C74D