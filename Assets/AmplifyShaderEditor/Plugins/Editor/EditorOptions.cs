using UnityEditor;
using UnityEngine;

namespace AmplifyShaderEditor
{
	[System.Serializable]
	public class OptionsWindow
	{
		private bool m_coloredPorts = false;
		private bool m_multiLinePorts = false;

		public OptionsWindow()
		{
			//Load ();
		}

		public void Init()
		{
			Load();
		}

		public void Destroy()
		{
			Save();
		}

		public void Save()
		{
			EditorPrefs.SetBool( "ColoredPorts", ColoredPorts );
			EditorPrefs.SetBool( "MultiLinePorts", UIUtils.CurrentWindow.ToggleMultiLine );
			EditorPrefs.SetBool( "ExpandedStencil", UIUtils.CurrentWindow.ExpandedStencil );
			EditorPrefs.SetBool( "ExpandedTesselation", UIUtils.CurrentWindow.ExpandedTesselation );
			EditorPrefs.SetBool( "ExpandedDepth", UIUtils.CurrentWindow.ExpandedDepth );
			EditorPrefs.SetBool( "ExpandedRenderingOptions", UIUtils.CurrentWindow.ExpandedRenderingOptions );
			EditorPrefs.SetBool( "ExpandedRenderingPlatforms", UIUtils.CurrentWindow.ExpandedRenderingPlatforms );
			EditorPrefs.SetBool( "ExpandedProperties", UIUtils.CurrentWindow.ExpandedProperties );
		}

		public void Load()
		{
			ColoredPorts = EditorPrefs.GetBool( "ColoredPorts" );
			UIUtils.CurrentWindow.ToggleMultiLine = EditorPrefs.GetBool( "MultiLinePorts" );
			MultiLinePorts = UIUtils.CurrentWindow.ToggleMultiLine;
			UIUtils.CurrentWindow.ExpandedStencil = EditorPrefs.GetBool( "ExpandedStencil" );
			UIUtils.CurrentWindow.ExpandedTesselation = EditorPrefs.GetBool( "ExpandedTesselation" );
			UIUtils.CurrentWindow.ExpandedDepth = EditorPrefs.GetBool( "ExpandedDepth" );
			UIUtils.CurrentWindow.ExpandedRenderingOptions = EditorPrefs.GetBool( "ExpandedRenderingOptions" );
			UIUtils.CurrentWindow.ExpandedRenderingPlatforms = EditorPrefs.GetBool( "ExpandedRenderingPlatforms" );
			UIUtils.CurrentWindow.ExpandedProperties = EditorPrefs.GetBool( "ExpandedProperties" );
		}

		public bool ColoredPorts
		{
			get { return m_coloredPorts; }
			set
			{
				if ( m_coloredPorts != value )
					EditorPrefs.SetBool( "ColoredPorts", value );

				m_coloredPorts = value;
			}
		}

		public bool MultiLinePorts
		{
			get { return m_multiLinePorts; }
			set
			{
				if ( m_multiLinePorts != value )
					EditorPrefs.SetBool( "MultiLinePorts", value );

				m_multiLinePorts = value;
			}
		}
	}
}
