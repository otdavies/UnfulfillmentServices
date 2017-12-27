using System;

namespace AmplifyShaderEditor
{
	[Serializable]
	[NodeAttributes( "World To Tangent Matrix", "Transform", "World to tangent transform matrix")]
	public sealed class WorldToTangentMatrix : ParentNode
	{
		protected override void CommonInit( int uniqueId )
		{
			base.CommonInit( uniqueId );
			AddOutputPort( WirePortDataType.FLOAT3x3, "Out" );
			UIUtils.AddNormalDependentCount();
			m_drawPreview = false;
		}

		public override void Destroy()
		{
			base.Destroy();
			UIUtils.RemoveNormalDependentCount();
		}

		public override void PropagateNodeData( NodeData nodeData )
		{
			base.PropagateNodeData( nodeData );
			UIUtils.CurrentDataCollector.DirtyNormal = true;
		}

		public override string GenerateShaderForOutput( int outputId, ref MasterNodeDataCollector dataCollector, bool ignoreLocalVar )
		{
			dataCollector.ForceNormal = true;

			dataCollector.AddToInput( m_uniqueId, UIUtils.GetInputDeclarationFromType( m_currentPrecisionType, AvailableSurfaceInputs.WORLD_NORMAL ), true );
			dataCollector.AddToInput( m_uniqueId, Constants.InternalData, false );

			GeneratorUtils.GenerateWorldToTangentMatrix( ref dataCollector, m_uniqueId, m_currentPrecisionType );

			return GeneratorUtils.WorldToTangentStr;
		}
	}
}
