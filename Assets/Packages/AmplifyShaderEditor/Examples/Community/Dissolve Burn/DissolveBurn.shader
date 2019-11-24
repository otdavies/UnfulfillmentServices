// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Dissolve"
{
	Properties
	{
		_Albedo("Albedo", 2D) = "white" {}
		_Normal("Normal", 2D) = "bump" {}
		_DisolveGuide("Disolve Guide", 2D) = "white" {}
		_BurnRamp("Burn Ramp", 2D) = "white" {}
		_DissolveAmount("Dissolve Amount", Range( 0 , 1)) = 0
		_Color("Color", Color) = (0,0,0,0)
		_TextureSample0("Texture Sample 0", 2D) = "white" {}
		_LavaBrightness("LavaBrightness", Range( 1 , 2)) = 1
		_GlowHeight("GlowHeight", Range( 0 , 5)) = 0
		_falloff("falloff", Range( 1 , 8)) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "AlphaTest+0" "IsEmissive" = "true"  }
		Cull Off
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
			float3 worldPos;
		};

		uniform sampler2D _Normal;
		uniform float4 _Normal_ST;
		uniform float4 _Color;
		uniform sampler2D _Albedo;
		uniform float4 _Albedo_ST;
		uniform float _DissolveAmount;
		uniform sampler2D _DisolveGuide;
		uniform float4 _DisolveGuide_ST;
		uniform float LavaHeight;
		uniform float _GlowHeight;
		uniform float _falloff;
		uniform sampler2D _BurnRamp;
		uniform sampler2D _TextureSample0;
		uniform float _LavaBrightness;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_Normal = i.uv_texcoord * _Normal_ST.xy + _Normal_ST.zw;
			o.Normal = UnpackNormal( tex2D( _Normal, uv_Normal ) );
			float2 uv_Albedo = i.uv_texcoord * _Albedo_ST.xy + _Albedo_ST.zw;
			o.Albedo = ( _Color * tex2D( _Albedo, uv_Albedo ) ).rgb;
			float2 uv_DisolveGuide = i.uv_texcoord * _DisolveGuide_ST.xy + _DisolveGuide_ST.zw;
			float3 ase_worldPos = i.worldPos;
			float clampResult166 = clamp( ( 1.0 - (0.0 + (ase_worldPos.y - LavaHeight) * (1.0 - 0.0) / (( _GlowHeight + LavaHeight ) - LavaHeight)) ) , 0.0 , 1.0 );
			float clampResult113 = clamp( ( (-4.0 + (( (-0.6 + (( 1.0 - _DissolveAmount ) - 0.0) * (0.6 - -0.6) / (1.0 - 0.0)) + tex2D( _DisolveGuide, uv_DisolveGuide ).r ) - 0.0) * (4.0 - -4.0) / (1.0 - 0.0)) * pow( clampResult166 , _falloff ) ) , 0.0 , 0.95 );
			float2 appendResult115 = (float2(clampResult113 , 0.0));
			float2 panner138 = ( 1.0 * _Time.y * float2( 0,-0.01 ) + i.uv_texcoord);
			o.Emission = ( ( ( clampResult113 * tex2D( _BurnRamp, appendResult115 ) ) * tex2D( _TextureSample0, panner138 ) ) * _LavaBrightness ).rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17300
7;29;1906;1004;2552.229;349.4697;1.813497;True;True
Node;AmplifyShaderEditor.RangedFloatNode;151;-3131.506,1182.89;Inherit;False;Global;LavaHeight;LavaHeight;9;0;Create;True;0;0;True;0;0;-6.3;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;160;-3176.836,817.8019;Inherit;False;Property;_GlowHeight;GlowHeight;9;0;Create;True;0;0;False;0;0;5;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;144;-3166.256,955.0196;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleAddOpNode;182;-2826.139,1053.353;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;4;-2704.256,593.2405;Float;False;Property;_DissolveAmount;Dissolve Amount;5;0;Create;True;0;0;False;0;0;0.2;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;189;-2435.94,1004.951;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;71;-2174.064,626.1104;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;190;-2124.203,999.1119;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;2;-1767.125,742.5328;Inherit;True;Property;_DisolveGuide;Disolve Guide;3;0;Create;True;0;0;False;0;-1;None;9fca23c86a0dd0f4bbb5707b8a85a933;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TFHCRemapNode;111;-1718.752,571.8904;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;-0.6;False;4;FLOAT;0.6;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;166;-1839.424,991.6462;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;73;-1328.231,499.6288;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;192;-1954.808,1235.712;Inherit;False;Property;_falloff;falloff;10;0;Create;True;0;0;False;0;0;3.21;1;8;0;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;112;-1104.444,313.5808;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;-4;False;4;FLOAT;4;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;191;-1536.882,1076.161;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;194;-929.149,470.2311;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;113;-795.6348,316.4494;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0.95;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;139;-1112.623,651.5538;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;115;-516.438,321.1016;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;138;-780.6232,651.5538;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,-0.01;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;114;-334.1431,322.0128;Inherit;True;Property;_BurnRamp;Burn Ramp;4;0;Create;True;0;0;False;0;-1;None;64e7766099ad46747a07014e44d0aea1;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;0,0;False;1;FLOAT2;1,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;135;-507.2264,621.6376;Inherit;True;Property;_TextureSample0;Texture Sample 0;7;0;Create;True;0;0;False;0;-1;None;f7e96904e8667e1439548f0f86389447;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;126;-27.3633,195.7657;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;78;-535.8436,-310.9727;Inherit;True;Property;_Albedo;Albedo;1;0;Create;True;0;0;False;0;-1;None;87e2fd36aeaf261428bce6d802259b48;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;137;124.246,312.6536;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;133;-81.88253,-384.7786;Inherit;False;Property;_Color;Color;6;0;Create;True;0;0;False;0;0,0,0,0;0.4264691,0.4264691,0.4264691,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;141;-71.05408,700.0431;Inherit;False;Property;_LavaBrightness;LavaBrightness;8;0;Create;True;0;0;False;0;1;2;1;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;131;-528.9641,-103.127;Inherit;True;Property;_Normal;Normal;2;0;Create;True;0;0;False;0;-1;None;e96e2d8d2edc36148ac7447132fab8b9;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;140;425.2669,700.0273;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;134;292.5178,-87.07939;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;647.579,101.2376;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;Dissolve;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Off;0;False;-1;3;False;-1;False;0;False;-1;0;False;-1;False;0;Masked;0.5;True;True;0;False;TransparentCutout;;AlphaTest;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;0;4;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;1;False;-1;1;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;0;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;182;0;160;0
WireConnection;182;1;151;0
WireConnection;189;0;144;2
WireConnection;189;1;151;0
WireConnection;189;2;182;0
WireConnection;71;0;4;0
WireConnection;190;0;189;0
WireConnection;111;0;71;0
WireConnection;166;0;190;0
WireConnection;73;0;111;0
WireConnection;73;1;2;1
WireConnection;112;0;73;0
WireConnection;191;0;166;0
WireConnection;191;1;192;0
WireConnection;194;0;112;0
WireConnection;194;1;191;0
WireConnection;113;0;194;0
WireConnection;115;0;113;0
WireConnection;138;0;139;0
WireConnection;114;1;115;0
WireConnection;135;1;138;0
WireConnection;126;0;113;0
WireConnection;126;1;114;0
WireConnection;137;0;126;0
WireConnection;137;1;135;0
WireConnection;140;0;137;0
WireConnection;140;1;141;0
WireConnection;134;0;133;0
WireConnection;134;1;78;0
WireConnection;0;0;134;0
WireConnection;0;1;131;0
WireConnection;0;2;140;0
ASEEND*/
//CHKSM=364EACC733B6241B7E8C2018D2D9594322DC2C6A