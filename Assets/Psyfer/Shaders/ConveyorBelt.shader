// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "ConveyorBelt"
{
	Properties
	{
		[HideInInspector] __dirty( "", Int ) = 1
		_Color("Color", Color) = (0,0,0,0)
		_brightness("brightness", Float) = 0
		_Min("Min", Float) = 0
		_Max("Max", Float) = 0
		_SlatSize("SlatSize", Float) = 1
		_Speed("Speed", Float) = 0
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows vertex:vertexDataFunc 
		struct Input
		{
			float2 texcoord_0;
		};

		uniform float4 _Color;
		uniform float _Speed;
		uniform float _SlatSize;
		uniform float _Min;
		uniform float _Max;
		uniform float _brightness;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float2 temp_cast_0 = (( ( _Time.y * _Speed ) + 20.0 )).xx;
			o.texcoord_0.xy = v.texcoord.xy * float2( 1,1 ) + temp_cast_0;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float4 temp_output_20_0 = ( ( _Color * min( max( fmod( i.texcoord_0.x , _SlatSize ) , _Min ) , _Max ) ) * _brightness );
			o.Albedo = temp_output_20_0.rgb;
			o.Emission = temp_output_20_0.rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=7003
-1911;69;1904;1004;713.7004;514.2006;1;True;True
Node;AmplifyShaderEditor.TimeNode;4;-381.4016,-390.0005;Float;False;FLOAT4;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;18;-218.4016,-115.0005;Float;False;Property;_Speed;Speed;5;0;0;0;0;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;8;-225.4016,-32.0005;Float;False;Constant;_Float1;Float 1;0;0;20;0;0;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;17;-104.4016,-310.0005;Float;False;0;FLOAT;0.0;False;1;FLOAT;0.0;False;FLOAT
Node;AmplifyShaderEditor.SimpleAddOpNode;7;-51.40155,-157.0005;Float;False;0;FLOAT;0.0;False;1;FLOAT;0.0;False;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;6;17.5984,-28.00049;Float;False;Property;_SlatSize;SlatSize;4;0;1;0;0;FLOAT
Node;AmplifyShaderEditor.TextureCoordinatesNode;2;73.59845,-336.0005;Float;False;0;-1;2;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;FLOAT2;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;15;28.5984,87.99951;Float;False;Property;_Min;Min;2;0;0;0;0;FLOAT
Node;AmplifyShaderEditor.FmodOpNode;5;233.5984,-164.0005;Float;False;0;FLOAT;0.0;False;1;FLOAT;0.0;False;FLOAT
Node;AmplifyShaderEditor.SimpleMaxOp;13;385.5984,-138.0005;Float;False;0;FLOAT;0.0;False;1;FLOAT;0.0;False;FLOAT
Node;AmplifyShaderEditor.RangedFloatNode;16;97.5984,179.9995;Float;False;Property;_Max;Max;3;0;0;0;0;FLOAT
Node;AmplifyShaderEditor.SimpleMinNode;14;297.5984,-5.00049;Float;False;0;FLOAT;0.0;False;1;FLOAT;0.0;False;FLOAT
Node;AmplifyShaderEditor.ColorNode;21;402.5984,-331.0005;Float;False;Property;_Color;Color;0;0;0,0,0,0;COLOR;FLOAT;FLOAT;FLOAT;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;22;459.5984,-37.00049;Float;False;0;COLOR;0.0;False;1;FLOAT;0,0,0,0;False;COLOR
Node;AmplifyShaderEditor.RangedFloatNode;19;102.5984,295.9995;Float;False;Property;_brightness;brightness;1;0;0;0;0;FLOAT
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;20;495.5984,135.9995;Float;False;0;COLOR;0.0;False;1;FLOAT;0.0,0,0,0;False;COLOR
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;644,-202;Float;False;True;2;Float;ASEMaterialInspector;0;Standard;ConveyorBelt;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;0;False;0;0;Opaque;0.5;True;True;0;False;Opaque;Geometry;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;False;0;255;255;0;0;0;0;False;0;4;10;25;False;0.5;True;0;Zero;Zero;0;Zero;Zero;Add;Add;0;False;0;0,0,0,0;VertexOffset;False;Cylindrical;Relative;0;;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0.0;False;4;FLOAT;0.0;False;5;FLOAT;0.0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0.0;False;9;FLOAT;0.0;False;10;OBJECT;0.0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;13;OBJECT;0.0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False
WireConnection;17;0;4;2
WireConnection;17;1;18;0
WireConnection;7;0;17;0
WireConnection;7;1;8;0
WireConnection;2;1;7;0
WireConnection;5;0;2;1
WireConnection;5;1;6;0
WireConnection;13;0;5;0
WireConnection;13;1;15;0
WireConnection;14;0;13;0
WireConnection;14;1;16;0
WireConnection;22;0;21;0
WireConnection;22;1;14;0
WireConnection;20;0;22;0
WireConnection;20;1;19;0
WireConnection;0;0;20;0
WireConnection;0;2;20;0
ASEEND*/
//CHKSM=04A8D7E98C6FA44F5035CFBFC82719A6EA8A4DB4