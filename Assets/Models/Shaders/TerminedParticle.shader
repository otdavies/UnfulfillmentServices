// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "TerminatedParticle"
{
	Properties
	{
		_Color("Color", Color) = (0,0,0,0)
		_Albedo("Albedo", 2D) = "white" {}
		_DisolveGuide("Disolve Guide", 2D) = "white" {}
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
		Tags{ "RenderType" = "Custom"  "Queue" = "Overlay+0" "IsEmissive" = "true"  }
		Cull Off
		ZWrite Off
		ZTest LEqual
		Blend SrcAlpha OneMinusSrcAlpha , OneMinusDstColor OneMinusSrcAlpha
		
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard keepalpha noshadow exclude_path:deferred 
		struct Input
		{
			float2 uv_texcoord;
			float3 worldPos;
		};

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
		uniform float _RampBrightness;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_Albedo = i.uv_texcoord * _Albedo_ST.xy + _Albedo_ST.zw;
			float4 tex2DNode78 = tex2D( _Albedo, uv_Albedo );
			float4 temp_output_134_0 = ( _Color * tex2DNode78 );
			float2 uv_DisolveGuide = i.uv_texcoord * _DisolveGuide_ST.xy + _DisolveGuide_ST.zw;
			float3 ase_worldPos = i.worldPos;
			float temp_output_217_0 = ( GlowHeight + _GlowFadeOffset );
			float clampResult166 = clamp( ( 1.0 - (0.0 + (ase_worldPos.y - temp_output_217_0) * (1.0 - 0.0) / (( _GlowFadeHeight + temp_output_217_0 ) - temp_output_217_0)) ) , 0.0 , 1.0 );
			float temp_output_194_0 = ( (-4.0 + (( (-0.6 + (( 1.0 - _DissolveAmount ) - 0.0) * (0.6 - -0.6) / (1.0 - 0.0)) + tex2D( _DisolveGuide, uv_DisolveGuide ).r ) - 0.0) * (4.0 - -4.0) / (1.0 - 0.0)) * pow( clampResult166 , _GlowFalloff ) );
			float clampResult113 = clamp( temp_output_194_0 , 0.0 , 0.95 );
			float2 appendResult115 = (float2(clampResult113 , 0.0));
			float4 temp_output_222_0 = ( temp_output_134_0 * ( clampResult113 * tex2D( _TransitionRamp, appendResult115 ) ) );
			o.Emission = ( temp_output_222_0 * _RampBrightness ).rgb;
			o.Alpha = ( tex2DNode78.a * clampResult166 );
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17300
7;29;1906;1004;1003.386;671.4088;1.388233;True;False
Node;AmplifyShaderEditor.RangedFloatNode;216;-3367.945,1265.238;Inherit;False;Property;_GlowFadeOffset;GlowFadeOffset;11;0;Create;True;0;0;False;0;0;0;-40;40;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;151;-3327.88,1169.547;Inherit;False;Global;GlowHeight;GlowHeight;9;0;Create;True;0;0;True;0;0;1.2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;160;-3187.079,847.1588;Inherit;False;Property;_GlowFadeHeight;GlowFadeHeight;9;0;Create;True;0;0;False;0;0;40;0;40;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;217;-3014.945,1210.238;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;144;-3166.256,955.0196;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleAddOpNode;182;-2826.139,1053.353;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;4;-2704.256,593.2405;Float;False;Property;_DissolveAmount;Dissolve Amount;6;0;Create;True;0;0;False;0;0;0.2;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;189;-2435.94,1004.951;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;71;-2174.064,626.1104;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;190;-2124.203,999.1119;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;111;-1718.752,571.8904;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;-0.6;False;4;FLOAT;0.6;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;2;-1801.715,755.1109;Inherit;True;Property;_DisolveGuide;Disolve Guide;4;0;Create;True;0;0;False;0;-1;None;9fca23c86a0dd0f4bbb5707b8a85a933;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;166;-1839.424,991.6462;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;192;-1942.855,1162.288;Inherit;False;Property;_GlowFalloff;GlowFalloff;10;0;Create;True;0;0;False;0;0;8;1;8;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;73;-1328.231,499.6288;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;112;-1144.444,311.5808;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;-4;False;4;FLOAT;4;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;191;-1536.882,1076.161;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;194;-884.0254,467.6877;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;113;-745.4754,223.7879;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0.95;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;115;-554.705,255.5664;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;78;-535.8436,-310.9727;Inherit;True;Property;_Albedo;Albedo;2;0;Create;True;0;0;False;0;-1;None;87e2fd36aeaf261428bce6d802259b48;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;133;-508.942,-517.8685;Inherit;False;Property;_Color;Color;0;0;Create;True;0;0;False;0;0,0,0,0;0.3308819,0.3308819,0.3308819,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;114;-430.9969,217.9608;Inherit;True;Global;_TransitionRamp;_TransitionRamp;5;0;Create;True;0;0;False;0;-1;None;64e7766099ad46747a07014e44d0aea1;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;0,0;False;1;FLOAT2;1,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;134;292.5178,-87.07939;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;126;-57.3633,252.3658;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;222;207.907,123.6091;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;141;25.89262,403.0989;Inherit;False;Property;_RampBrightness;RampBrightness;8;0;Create;True;0;0;False;0;1;2;1;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;221;472.5055,452.7066;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;197;539.4756,-42.40118;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;140;472.8595,204.4596;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.PannerNode;138;-4033.56,1336.926;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,-0.01;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;131;-543.2733,-77.14977;Inherit;True;Property;_Normal;Normal;3;0;Create;True;0;0;False;0;-1;None;e96e2d8d2edc36148ac7447132fab8b9;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;135;-3687.889,1330.492;Inherit;True;Property;_DetailTexOld;DetailTexOld;7;0;Create;True;0;0;False;0;-1;None;f7e96904e8667e1439548f0f86389447;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;209;-4369.179,1289.187;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;759.4052,126.6526;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;TerminatedParticle;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Off;2;False;-1;3;False;-1;False;0;False;-1;0;False;-1;False;3;Custom;0.66;True;False;0;False;Custom;;Overlay;ForwardOnly;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;0;4;10;25;False;0.5;False;2;5;False;-1;10;False;-1;1;4;False;-1;10;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;1;-1;0;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;192;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;217;0;151;0
WireConnection;217;1;216;0
WireConnection;182;0;160;0
WireConnection;182;1;217;0
WireConnection;189;0;144;2
WireConnection;189;1;217;0
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
WireConnection;114;1;115;0
WireConnection;134;0;133;0
WireConnection;134;1;78;0
WireConnection;126;0;113;0
WireConnection;126;1;114;0
WireConnection;222;0;134;0
WireConnection;222;1;126;0
WireConnection;221;0;78;4
WireConnection;221;1;166;0
WireConnection;197;0;134;0
WireConnection;197;1;222;0
WireConnection;197;2;194;0
WireConnection;140;0;222;0
WireConnection;140;1;141;0
WireConnection;138;0;209;0
WireConnection;135;1;138;0
WireConnection;0;2;140;0
WireConnection;0;9;221;0
ASEEND*/
//CHKSM=41EB7CEC54A500AAFBE78224A8A4203ED8EDF764