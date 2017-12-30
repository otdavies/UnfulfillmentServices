// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "GradientLighting"
{
	Properties
	{
		[HideInInspector] __dirty( "", Int ) = 1
		_SurfaceColor("SurfaceColor", Color) = (0,0,0,0)
		_SurfaceShine("SurfaceShine", Range( 0 , 3)) = 0.2
		_GradientColor("GradientColor", Color) = (0,0,0,0)
		_GradientShine("GradientShine", Range( 0 , 3)) = 0
		_GradientStart("GradientStart", Float) = 2
		_GradientLength("GradientLength", Float) = 0.5
		_GradientFallOff("GradientFallOff", Range( 0.1 , 2)) = 0.49
		_GradientDirection("GradientDirection", Vector) = (0,1,0,0)
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float3 worldPos;
		};

		uniform float4 _SurfaceColor;
		uniform float4 _GradientColor;
		uniform float3 _GradientDirection;
		uniform float _GradientStart;
		uniform float _GradientFallOff;
		uniform float _GradientLength;
		uniform float _SurfaceShine;
		uniform float _GradientShine;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float3 vertexPos = mul( unity_WorldToObject, float4( i.worldPos , 1 ) );
			float4 temp_output_58_0 = ( ( _GradientColor * 0.5 ) * clamp( ( pow( ( dot( _GradientDirection , vertexPos ) - _GradientStart ) , _GradientFallOff ) * ( 1.0 / _GradientLength ) ) , 0.0 , 1.0 ) );
			o.Albedo = ( saturate( 	max( _SurfaceColor, temp_output_58_0 ) )).rgb;
			o.Emission = ( saturate( 	max( ( _SurfaceColor * _SurfaceShine ), ( temp_output_58_0 * _GradientShine ) ) )).rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=7003
-1911;69;1904;1004;565.736;283.4665;1.3;True;True
Node;AmplifyShaderEditor.PosVertexDataNode;59;143.8035,0.03576326;Float;False;FLOAT3;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.Vector3Node;76;150.204,-167.9643;Float;False;Property;_GradientDirection;GradientDirection;7;0;0,1,0;FLOAT3;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.DotProductOpNode;75;336.105,-81.2643;Float;False;0;FLOAT3;0.0,0,0;False;1;FLOAT3;0,0,0;False;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;51;114.9063,300.136;Float;False;Property;_GradientStart;GradientStart;4;0;2;0;0;FLOAT
Node;AmplifyShaderEditor.SimpleSubtractOpNode;62;334.5044,219.5356;Float;False;0;FLOAT;0.0;False;1;FLOAT;0.0;False;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;81;304.91,363.4336;Float;False;Property;_GradientFallOff;GradientFallOff;6;0;0.49;0.1;2;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;66;177.9189,403.0421;Float;False;Constant;_Float0;Float 0;10;0;1;0;0;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;55;148.2428,482.1152;Float;False;Property;_GradientLength;GradientLength;5;0;0.5;0;0;FLOAT
Node;AmplifyShaderEditor.PowerNode;71;493.5869,278.0335;Float;False;0;FLOAT;0.0;False;1;FLOAT;1.0;False;FLOAT
Node;AmplifyShaderEditor.SimpleDivideOpNode;54;376.1074,431.1358;Float;False;0;FLOAT;0.0;False;1;FLOAT;0.0;False;FLOAT
Node;AmplifyShaderEditor.ColorNode;57;672.0042,210.9364;Float;False;Property;_GradientColor;GradientColor;2;0;0,0,0,0;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;70;678.5059,139.5359;Float;False;Constant;_Float1;Float 1;10;0;0.5;0;0;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;68;553.705,438.7357;Float;False;0;FLOAT;0.0;False;1;FLOAT;0.0;False;FLOAT
Node;AmplifyShaderEditor.ClampOpNode;63;545.7041,565.1357;Float;False;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT;1.0;False;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;69;744.1058,464.3357;Float;False;0;COLOR;0,0,0,0;False;1;FLOAT;0,0,0,0;False;COLOR
Node;AmplifyShaderEditor.ColorNode;82;257.7629,573.6337;Float;False;Property;_SurfaceColor;SurfaceColor;0;0;0,0,0,0;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;58;747.0043,560.8359;Float;False;0;COLOR;0.0,0,0,0;False;1;FLOAT;0.0,0,0,0;False;COLOR
Node;AmplifyShaderEditor.RangedFloatNode;48;260.7427,755.764;Float;False;Property;_SurfaceShine;SurfaceShine;1;0;0.2;0;3;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;78;258.3979,888.9682;Float;False;Property;_GradientShine;GradientShine;3;0;0;0;3;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;79;492.9243,868.4537;Float;False;0;COLOR;0.0;False;1;FLOAT;0,0,0,0;False;COLOR
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;77;486.067,760.04;Float;False;0;COLOR;0.0;False;1;FLOAT;0.0,0,0,0;False;COLOR
Node;AmplifyShaderEditor.BlendOpsNode;61;777.6061,855.8755;Float;False;Lighten;True;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;COLOR
Node;AmplifyShaderEditor.BlendOpsNode;60;778.4705,746.7618;Float;False;Lighten;True;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;COLOR
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1392.946,640.4204;Float;False;True;2;Float;ASEMaterialInspector;0;Standard;GradientLighting;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;0;False;0;0;Opaque;0.5;True;True;0;False;Opaque;Geometry;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;False;0;255;255;0;0;0;0;False;0;4;10;25;False;0.5;True;0;SrcAlpha;OneMinusSrcAlpha;0;Zero;Zero;Add;Add;0;False;0.04;0,0,0,0;VertexOffset;False;Cylindrical;Relative;0;;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0.0;False;4;FLOAT;0.0;False;5;FLOAT;0.0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0.0;False;9;FLOAT;0.0;False;10;OBJECT;0.0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;13;OBJECT;0.0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False
WireConnection;75;0;76;0
WireConnection;75;1;59;0
WireConnection;62;0;75;0
WireConnection;62;1;51;0
WireConnection;71;0;62;0
WireConnection;71;1;81;0
WireConnection;54;0;66;0
WireConnection;54;1;55;0
WireConnection;68;0;71;0
WireConnection;68;1;54;0
WireConnection;63;0;68;0
WireConnection;69;0;57;0
WireConnection;69;1;70;0
WireConnection;58;0;69;0
WireConnection;58;1;63;0
WireConnection;79;0;58;0
WireConnection;79;1;78;0
WireConnection;77;0;82;0
WireConnection;77;1;48;0
WireConnection;61;0;77;0
WireConnection;61;1;79;0
WireConnection;60;0;82;0
WireConnection;60;1;58;0
WireConnection;0;0;60;0
WireConnection;0;2;61;0
ASEEND*/
//CHKSM=04247EE703A1E21E3E4B8C65BF7181B3029B6249