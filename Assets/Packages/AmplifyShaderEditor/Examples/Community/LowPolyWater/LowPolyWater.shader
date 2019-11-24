// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "ASESampleShaders/Community/TFHC/Low Poly Water"
{
	Properties
	{
		_WaterColor("Water Color", Color) = (0.4926471,0.8740366,1,1)
		_WaveGuide("Wave Guide", 2D) = "white" {}
		_WaveSpeed("Wave Speed", Range( 0 , 5)) = 0
		_WaveHeight("Wave Height", Range( 0 , 5)) = 0
		_FoamColor("Foam Color", Color) = (1,1,1,0)
		_Foam("Foam", 2D) = "white" {}
		_FoamDistortion("Foam Distortion", 2D) = "white" {}
		_FoamDist("Foam Dist", Range( 0 , 1)) = 0.1
		_Opacity("Opacity", Range( 0 , 1)) = 0
		[Toggle]_LowPoly("Low Poly", Float) = 1
		_NormalOnlyNoPolyMode("Normal (Only No Poly Mode)", 2D) = "bump" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Off
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#include "UnityCG.cginc"
		#pragma target 3.0
		#pragma surface surf Standard alpha:fade keepalpha noshadow vertex:vertexDataFunc 
		struct Input
		{
			float2 uv_texcoord;
			float3 worldPos;
			float4 screenPos;
		};

		uniform sampler2D _WaveGuide;
		uniform float _WaveSpeed;
		uniform float _WaveHeight;
		uniform float _LowPoly;
		uniform sampler2D _NormalOnlyNoPolyMode;
		uniform float4 _NormalOnlyNoPolyMode_ST;
		uniform float4 _WaterColor;
		uniform float4 _FoamColor;
		uniform sampler2D _Foam;
		uniform float4 _Foam_ST;
		uniform sampler2D _FoamDistortion;
		UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
		uniform float4 _CameraDepthTexture_TexelSize;
		uniform float _FoamDist;
		uniform float _Opacity;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float4 speed183 = ( _Time * _WaveSpeed );
			float3 ase_vertex3Pos = v.vertex.xyz;
			float2 uv_TexCoord96 = v.texcoord.xy + ( speed183 + (ase_vertex3Pos).y ).xy;
			float3 ase_vertexNormal = v.normal.xyz;
			float3 VertexAnimation127 = ( ( tex2Dlod( _WaveGuide, float4( uv_TexCoord96, 0, 1.0) ).r - 0.5 ) * ( ase_vertexNormal * _WaveHeight ) );
			v.vertex.xyz += VertexAnimation127;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_NormalOnlyNoPolyMode = i.uv_texcoord * _NormalOnlyNoPolyMode_ST.xy + _NormalOnlyNoPolyMode_ST.zw;
			float3 ase_worldPos = i.worldPos;
			float3 normalizeResult123 = normalize( ( cross( ddx( ase_worldPos ) , ddy( ase_worldPos ) ) + float3( 1E-09,0,0 ) ) );
			float3 Normal124 = (( _LowPoly )?( normalizeResult123 ):( UnpackNormal( tex2D( _NormalOnlyNoPolyMode, uv_NormalOnlyNoPolyMode ) ) ));
			o.Normal = Normal124;
			float4 Albedo131 = _WaterColor;
			o.Albedo = Albedo131.rgb;
			float4 speed183 = ( _Time * _WaveSpeed );
			float2 uv0_Foam = i.uv_texcoord * _Foam_ST.xy + _Foam_ST.zw;
			float2 panner177 = ( speed183.x * float2( 0.5,0.5 ) + uv0_Foam);
			float cos182 = cos( speed183.x );
			float sin182 = sin( speed183.x );
			float2 rotator182 = mul( panner177 - float2( 0,0 ) , float2x2( cos182 , -sin182 , sin182 , cos182 )) + float2( 0,0 );
			float clampResult181 = clamp( tex2D( _FoamDistortion, rotator182 ).r , 0.0 , 1.0 );
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			float screenDepth164 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
			float distanceDepth164 = abs( ( screenDepth164 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( _FoamDist ) );
			float clampResult191 = clamp( ( clampResult181 * distanceDepth164 ) , 0.0 , 1.0 );
			float4 lerpResult157 = lerp( ( _FoamColor * tex2D( _Foam, rotator182 ) ) , float4(0,0,0,0) , clampResult191);
			float4 Emission162 = lerpResult157;
			o.Emission = Emission162.rgb;
			o.Alpha = _Opacity;
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=17300
7;29;1906;1004;2696.374;-2.37388;1.07286;True;True
Node;AmplifyShaderEditor.CommentaryNode;199;-2827.374,-925.0059;Inherit;False;914.394;362.5317;Comment;4;89;15;88;183;Wave Speed;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;15;-2777.374,-677.473;Float;False;Property;_WaveSpeed;Wave Speed;2;0;Create;True;0;0;False;0;0;0;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.TimeNode;89;-2706.477,-875.0057;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;88;-2377.44,-739.9845;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.CommentaryNode;192;-2804.44,147.8661;Inherit;False;2009.663;867.9782;Comment;16;176;177;182;179;181;161;174;169;191;159;170;157;162;164;167;184;Emission;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;183;-2155.98,-832.3298;Float;False;speed;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;176;-2786.44,327.7948;Inherit;False;0;169;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;184;-2755.296,577.5368;Inherit;False;183;speed;1;0;OBJECT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.CommentaryNode;197;-2751.606,-436.2369;Inherit;False;2321.461;426.9865;Comment;12;53;118;47;96;86;43;54;44;36;29;127;195;Vertex Animation;1,1,1,1;0;0
Node;AmplifyShaderEditor.PannerNode;177;-2454.748,382.3567;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0.5,0.5;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PosVertexDataNode;53;-2701.606,-286.774;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;204;-1883.495,-920.8318;Inherit;False;1244.412;443.4576;Comment;9;119;121;120;122;202;123;200;124;205;Normal;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;195;-2377.985,-347.0552;Inherit;False;183;speed;1;0;OBJECT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RotatorNode;182;-2377.307,563.6425;Inherit;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ComponentMaskNode;118;-2462.842,-267.5019;Inherit;False;False;True;False;True;1;0;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;119;-1872,-656;Float;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;167;-2382.478,814.4856;Float;False;Property;_FoamDist;Foam Dist;7;0;Create;True;0;0;False;0;0.1;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;47;-2177.163,-350.3377;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SamplerNode;179;-2143.537,609.8837;Inherit;True;Property;_FoamDistortion;Foam Distortion;6;0;Create;True;0;0;False;0;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DdxOpNode;120;-1664,-672;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DdyOpNode;121;-1664,-576;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ClampOpNode;181;-1851.366,695.3312;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.DepthFade;164;-2020.088,829.655;Inherit;False;True;False;True;2;1;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.CrossProductOpNode;122;-1536,-640;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;96;-1988.552,-386.2369;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;169;-1928.3,373.9261;Inherit;True;Property;_Foam;Foam;5;0;Create;True;0;0;False;0;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;43;-1398.183,-124.2504;Float;False;Property;_WaveHeight;Wave Height;3;0;Create;True;0;0;False;0;0;0;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;161;-1881.812,197.8662;Float;False;Property;_FoamColor;Foam Color;4;0;Create;True;0;0;False;0;1,1,1,0;0,0,0,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;174;-1683.11,819.9097;Inherit;False;2;2;0;FLOAT;0.075;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;205;-1367.996,-605.7512;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;1E-09,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;86;-1709.089,-380.2153;Inherit;True;Property;_WaveGuide;Wave Guide;1;0;Create;True;0;0;False;0;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.NormalVertexDataNode;54;-1353.867,-280.0608;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleSubtractOpNode;36;-1021.508,-352.6445;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;170;-1513.108,437.6683;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;203;-2809.492,-1303.385;Inherit;False;566.4452;257;Comment;2;2;131;Albedo;1,1,1,1;0;0
Node;AmplifyShaderEditor.ColorNode;159;-1656.591,619.5612;Float;False;Constant;_Color0;Color 0;9;0;Create;True;0;0;False;0;0,0,0,0;0,0,0,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;202;-1509.38,-870.8318;Inherit;True;Property;_NormalOnlyNoPolyMode;Normal (Only No Poly Mode);10;0;Create;True;0;0;False;0;-1;None;None;True;0;False;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.NormalizeNode;123;-1232,-576;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ClampOpNode;191;-1486.683,783.1451;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;44;-1026.813,-198.9792;Inherit;False;2;2;0;FLOAT3;1,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ToggleSwitchNode;200;-1084.856,-668.6083;Float;False;Property;_LowPoly;Low Poly;9;0;Create;True;0;0;False;0;1;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;157;-1218.507,670.1462;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;29;-859.5037,-220.1143;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ColorNode;2;-2759.492,-1253.385;Float;False;Property;_WaterColor;Water Color;0;0;Create;True;0;0;False;0;0.4926471,0.8740366,1,1;0,0,0,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;162;-1037.777,716.2555;Float;False;Emission;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;124;-882.0831,-657.6036;Float;False;Normal;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;131;-2486.047,-1232.555;Float;False;Albedo;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;127;-706.1451,-228.0923;Float;False;VertexAnimation;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;125;-619.788,244.9404;Inherit;False;124;Normal;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;163;-631.3639,329.4067;Inherit;False;162;Emission;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;134;-632.9377,159.7002;Inherit;False;131;Albedo;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;11;-673.6442,428.0303;Float;False;Property;_Opacity;Opacity;8;0;Create;True;0;0;False;0;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;128;-657.9543,520.383;Inherit;False;127;VertexAnimation;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;-358.2882,224.1705;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;ASESampleShaders/Community/TFHC/Low Poly Water;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Off;0;False;-1;3;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;False;0;False;Transparent;;Transparent;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;0;4;10;25;False;0.5;False;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;1;False;-1;1;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;88;0;89;0
WireConnection;88;1;15;0
WireConnection;183;0;88;0
WireConnection;177;0;176;0
WireConnection;177;1;184;0
WireConnection;182;0;177;0
WireConnection;182;2;184;0
WireConnection;118;0;53;0
WireConnection;47;0;195;0
WireConnection;47;1;118;0
WireConnection;179;1;182;0
WireConnection;120;0;119;0
WireConnection;121;0;119;0
WireConnection;181;0;179;1
WireConnection;164;0;167;0
WireConnection;122;0;120;0
WireConnection;122;1;121;0
WireConnection;96;1;47;0
WireConnection;169;1;182;0
WireConnection;174;0;181;0
WireConnection;174;1;164;0
WireConnection;205;0;122;0
WireConnection;86;1;96;0
WireConnection;36;0;86;1
WireConnection;170;0;161;0
WireConnection;170;1;169;0
WireConnection;123;0;205;0
WireConnection;191;0;174;0
WireConnection;44;0;54;0
WireConnection;44;1;43;0
WireConnection;200;0;202;0
WireConnection;200;1;123;0
WireConnection;157;0;170;0
WireConnection;157;1;159;0
WireConnection;157;2;191;0
WireConnection;29;0;36;0
WireConnection;29;1;44;0
WireConnection;162;0;157;0
WireConnection;124;0;200;0
WireConnection;131;0;2;0
WireConnection;127;0;29;0
WireConnection;0;0;134;0
WireConnection;0;1;125;0
WireConnection;0;2;163;0
WireConnection;0;9;11;0
WireConnection;0;11;128;0
ASEEND*/
//CHKSM=BD4BFA51A96200E852F6A31FAA1EE086BFA9636B