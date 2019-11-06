// Amplify Shader Editor - Visual Shader Editing Tool
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

namespace AmplifyShaderEditor
{
	public static class GeneratorUtils
	{
		public const string WorldPositionStr = "ase_worldPos";
		public const string WorldNormalStr = "ase_worldNormal";
		public const string WorldTangentStr = "ase_worldTangent";
		public const string WorldBitangentStr = "ase_worldBitangent";
		public const string WorldToTangentStr = "ase_worldToTangent";
		private const string Float3Format = "float3 {0} = {1};";

		// WORLD POSITION
		static public string GenerateWorldPosition( ref MasterNodeDataCollector dataCollector, int uniqueId )
		{
			string result = Constants.InputVarStr + ".worldPos";

			if ( dataCollector.PortCategory == MasterNodePortCategory.Vertex || dataCollector.PortCategory == MasterNodePortCategory.Tessellation )
				result = "mul(unity_ObjectToWorld, " + Constants.VertexShaderInputStr + ".vertex)";

			dataCollector.AddToLocalVariables( dataCollector.PortCategory, uniqueId, string.Format( Float3Format, WorldPositionStr, result ) );

			return WorldPositionStr;
		}

		// WORLD NORMAL
		static public string GenerateWorldNormal( ref MasterNodeDataCollector dataCollector, int uniqueId )
		{
			string result = string.Empty;
			if ( !dataCollector.DirtyNormal )
				result = Constants.InputVarStr + ".worldNormal";
			else
				result = "WorldNormalVector( " + Constants.InputVarStr + ", float3( 0, 0, 1 ) )";

			if ( dataCollector.PortCategory == MasterNodePortCategory.Vertex || dataCollector.PortCategory == MasterNodePortCategory.Tessellation )
				result = "UnityObjectToWorldNormal( " + Constants.VertexShaderInputStr + ".normal )";

			dataCollector.AddToLocalVariables( dataCollector.PortCategory, uniqueId, string.Format( Float3Format, WorldNormalStr, result ) );
			return WorldNormalStr;
		}

		// WORLD TANGENT
		static public string GenerateWorldTangent( ref MasterNodeDataCollector dataCollector, int uniqueId )
		{
			string result = "WorldNormalVector( " + Constants.InputVarStr + ", float3( 1, 0, 0 ) )";

			if ( dataCollector.PortCategory == MasterNodePortCategory.Vertex || dataCollector.PortCategory == MasterNodePortCategory.Tessellation )
				result = "UnityObjectToWorldDir( " + Constants.VertexShaderInputStr + ".tangent.xyz )";

			dataCollector.AddToLocalVariables( dataCollector.PortCategory, uniqueId, string.Format( Float3Format, WorldTangentStr, result ) );
			return WorldTangentStr;
		}

		// WORLD BITANGENT
		static public string GenerateWorldBitangent( ref MasterNodeDataCollector dataCollector, int uniqueId )
		{
			string result = "WorldNormalVector( " + Constants.InputVarStr + ", float3( 0, 1, 0 ) )";

			if ( dataCollector.PortCategory == MasterNodePortCategory.Vertex || dataCollector.PortCategory == MasterNodePortCategory.Tessellation )
			{
				string worldNormal = GenerateWorldNormal( ref dataCollector, uniqueId );
				string worldTangent = GenerateWorldTangent( ref dataCollector, uniqueId );
				dataCollector.AddToVertexLocalVariables( uniqueId, string.Format( "fixed tangentSign = {0}.tangent.w * unity_WorldTransformParams.w;", Constants.VertexShaderInputStr ) );
				result = "cross( " + worldNormal + ", " + worldTangent + " ) * tangentSign";
			}

			dataCollector.AddToLocalVariables( dataCollector.PortCategory, uniqueId, string.Format( Float3Format, WorldBitangentStr, result ) );
			return WorldBitangentStr;
		}

		// WORLD TO TANGENT MATRIX
		static public string GenerateWorldToTangentMatrix( ref MasterNodeDataCollector dataCollector, int uniqueId, PrecisionType precision )
		{
			string worldNormal = GenerateWorldNormal( ref dataCollector, uniqueId );
			string worldTangent = GenerateWorldTangent( ref dataCollector, uniqueId );
			string worldBitangent = GenerateWorldBitangent( ref dataCollector, uniqueId );

			dataCollector.AddToLocalVariables( dataCollector.PortCategory, uniqueId, precision, WirePortDataType.FLOAT3x3, WorldToTangentStr, "float3x3(" + worldTangent + ", " + worldBitangent + ", " + worldNormal + ")" );
			return WorldToTangentStr;
		}

		// AUTOMATIC UVS
		static public string GenerateAutoUVs( ref MasterNodeDataCollector dataCollector, int uniqueId, int index, string propertyName = null, WirePortDataType size = WirePortDataType.FLOAT2 )
		{
			string result = string.Empty;

			if ( dataCollector.PortCategory == MasterNodePortCategory.Fragment || dataCollector.PortCategory == MasterNodePortCategory.Debug )
			{
				string dummyPropUV = "_texcoord" + ( index > 0 ? ( index + 1 ).ToString() : "" );
				string dummyUV = "uv" + ( index > 0 ? ( index + 1 ).ToString() : "" ) + dummyPropUV;

				dataCollector.AddToProperties( uniqueId, "[HideInInspector] " + dummyPropUV + "( \"\", 2D ) = \"white\" {}", 100 );
				dataCollector.AddToInput( uniqueId, UIUtils.WirePortToCgType( size ) + " " + dummyUV, true );

				result = Constants.InputVarStr + "." + dummyUV;
				if ( !string.IsNullOrEmpty( propertyName ) )
				{
					dataCollector.AddToUniforms( uniqueId, "uniform float4 " + propertyName + "_ST;" );
					//dataCollector.AddToLocalVariables( uniqueId, PrecisionType.Float, size, "uv" + propertyName, result + " * " + propertyName + "_ST.xy + " + propertyName + "_ST.zw" );
					dataCollector.AddToLocalVariables(dataCollector.PortCategory, uniqueId, PrecisionType.Float, size, "uv" + propertyName, result + " * " + propertyName + "_ST.xy + " + propertyName + "_ST.zw" );

					result = "uv" + propertyName;
				}
			}
			else
			{
				result = Constants.VertexShaderInputStr + ".texcoord";
				if ( index > 0 )
				{
					result += index.ToString();
				}

				switch ( size )
				{
					default:
					case WirePortDataType.FLOAT2:
					{
						result += ".xy";
					}
					break;
					case WirePortDataType.FLOAT3:
					{
						result += ".xyz";
					}
					break;
					case WirePortDataType.FLOAT4: break;
				}

				if ( !string.IsNullOrEmpty( propertyName ) )
				{
					dataCollector.AddToUniforms( uniqueId, "uniform float4 " + propertyName + "_ST;" );
					dataCollector.AddToLocalVariables( dataCollector.PortCategory, uniqueId, PrecisionType.Float, size, "uv" + propertyName, Constants.VertexShaderInputStr + ".texcoord" + ( index > 0 ? index.ToString() : string.Empty ) + " * " + propertyName + "_ST.xy + " + propertyName + "_ST.zw;" );
					//dataCollector.AddToVertexLocalVariables( uniqueId, UIUtils.WirePortToCgType( size ) + " uv" + propertyName + " = " + Constants.VertexShaderInputStr + ".texcoord" + ( index > 0 ? index.ToString() : string.Empty ) + " * " + propertyName + "_ST.xy + " + propertyName + "_ST.zw;" );
					result = "uv" + propertyName;
				}
			}
			return result;
		}

		// SCREEN POSITION
		static public string GenerateScreenPosition( ref MasterNodeDataCollector dataCollector, int uniqueId, PrecisionType precision, bool addInput = true )
		{
			if( addInput )
				dataCollector.AddToInput( uniqueId, UIUtils.GetInputDeclarationFromType( precision, AvailableSurfaceInputs.SCREEN_POS ), true );

			string result = Constants.InputVarStr + ".screenPos";

			//if ( dataCollector.PortCategory == MasterNodePortCategory.Vertex || dataCollector.PortCategory == MasterNodePortCategory.Tessellation )
			//	result = "mul(unity_ObjectToWorld, " + Constants.VertexShaderInputStr + ".vertex)";

			//dataCollector.AddToLocalVariables( dataCollector.PortCategory, uniqueId, string.Format( Float3Format, WorldPositionStr, result ) );

			return result;
		}
	}
}
