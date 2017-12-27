using UnityEngine;
using UnityEditor;

using System;
namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( "Camera Depth Fade", "Generic", "Outputs a 0 - 1 gradient representing the distance between the surface of this object and camera near plane" )]
	public sealed class CameraDepthFade : ParentNode
	{
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			AddInputPort( WirePortDataType.FLOAT, false, "Length" );
			AddInputPort( WirePortDataType.FLOAT, false, "Offset" );
			m_inputPorts[ 0 ].FloatInternalData = 1;
			//m_inputPorts[ 0 ].InternalDataName = "Distance";
			AddOutputPort( WirePortDataType.FLOAT, "Out" );
			m_useInternalPortData = true;
		}

		public override string GenerateShaderForOutput( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalvar )
		{
			string distance = m_inputPorts[ 0 ].GeneratePortInstructions( ref dataCollector );
			string offset = m_inputPorts[ 1 ].GeneratePortInstructions( ref dataCollector );

			if ( dataCollector.PortCategory == MasterNodePortCategory.Vertex || dataCollector.PortCategory == MasterNodePortCategory.Tessellation )
			{
				dataCollector.AddVertexInstruction( "float cameraDepthFade" + m_uniqueId + " = (( -UnityObjectToViewPos( " + Constants.VertexShaderInputStr + ".vertex.xyz ).z -_ProjectionParams.y - " + offset + " ) / " + distance + ");", m_uniqueId );
				return "cameraDepthFade" + m_uniqueId;
			}

			dataCollector.AddToIncludes( m_uniqueId, Constants.UnityShaderVariables );
			dataCollector.AddToInput( m_uniqueId, "float eyeDepth", true );

			string instruction = "-UnityObjectToViewPos( " + Constants.VertexShaderInputStr + ".vertex.xyz ).z";
			dataCollector.AddVertexInstruction( Constants.VertexShaderOutputStr + ".eyeDepth = " + instruction, m_uniqueId );

			//string distance = m_inputPorts[ 0 ].GeneratePortInstructions( ref dataCollector );
			//string offset = m_inputPorts[ 1 ].GeneratePortInstructions( ref dataCollector );

			dataCollector.AddToLocalVariables( m_uniqueId, "float cameraDepthFade" + m_uniqueId + " = (( " + Constants.InputVarStr + ".eyeDepth -_ProjectionParams.y - "+ offset + " ) / " + distance + ");" );
			return "cameraDepthFade" + m_uniqueId;
		}
	}
}
