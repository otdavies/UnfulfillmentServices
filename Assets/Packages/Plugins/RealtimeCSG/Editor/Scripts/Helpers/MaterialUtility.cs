using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using InternalRealtimeCSG;

namespace RealtimeCSG
{
	public static class MaterialUtility
	{
		internal const string ShaderNameRoot						= "Hidden/CSG/internal/";
		internal const string SpecialSurfaceShader					= "specialSurface";
		internal const string SpecialSurfaceShaderName				= ShaderNameRoot + SpecialSurfaceShader;
		internal const string TransparentSpecialSurfaceShader		= "transparentSpecialSurface";
		internal const string TransparentSpecialSurfaceShaderName	= ShaderNameRoot + TransparentSpecialSurfaceShader;

		internal const string HiddenName				= "hidden";
		internal const string CulledName				= "culled";
		internal const string ColliderName				= "collider";
		internal const string TriggerName				= "trigger";
		internal const string ShadowOnlyName            = "shadowOnly";
		internal const string CastShadowsName			= "castShadows";
		internal const string ReceiveShadowsName        = "receiveShadows";

		internal const string HiddenMaterialName			= TransparentSpecialSurfaceShader + "_" + HiddenName;
		internal const string CulledMaterialName			= TransparentSpecialSurfaceShader + "_" + CulledName;
		internal const string ColliderMaterialName			= SpecialSurfaceShader + "_" + ColliderName;
		internal const string TriggerMaterialName			= TransparentSpecialSurfaceShader + "_" + TriggerName;
		internal const string ShadowOnlyMaterialName		= SpecialSurfaceShader + "_" + ShadowOnlyName;
		internal const string CastShadowsMaterialName		= SpecialSurfaceShader + "_" + CastShadowsName;
		internal const string ReceiveShadowsMaterialName	= SpecialSurfaceShader + "_" + ReceiveShadowsName;


		
		private static readonly Dictionary<string, Material> EditorMaterials = new Dictionary<string, Material>();

		private static bool _shadersInitialized;		//= false;
		private static int	_pixelsPerPointId			= -1;
		private static int	_lineThicknessMultiplierId	= -1; 
		private static int	_lineDashMultiplierId		= -1; 
		private static int	_lineAlphaMultiplierId		= -1;
		 
		private static void ShaderInit()
		{
			_shadersInitialized = true;
	
			_pixelsPerPointId			= Shader.PropertyToID("_pixelsPerPoint");
			_lineThicknessMultiplierId	= Shader.PropertyToID("_thicknessMultiplier");
			_lineDashMultiplierId		= Shader.PropertyToID("_dashMultiplier");
			_lineAlphaMultiplierId		= Shader.PropertyToID("_alphaMultiplier");
		}

		internal static Material GenerateEditorMaterial(string shaderName, string textureName = null, string materialName = null)
		{
			Material material;
			var name = shaderName + ":" + textureName;
			if (EditorMaterials.TryGetValue(name, out material))
			{
				// just in case one of many unity bugs destroyed the material
				if (!material)
				{
					EditorMaterials.Remove(name);
				} else
					return material;
			}

			if (materialName == null)
				materialName = name.Replace(':', '_');


			var shader = Shader.Find(ShaderNameRoot + shaderName);
			if (!shader)
			{
				Debug.LogWarning("Could not find internal shader: " + ShaderNameRoot + shaderName);
				return null;
			}

			material = new Material(shader)
			{
				name = materialName,
				hideFlags = HideFlags.DontSaveInBuild | HideFlags.DontUnloadUnusedAsset
			};
			if (textureName != null)
			{
				string filename;
#if DEMO
				filename = "Assets/Plugins/RealtimeCSGDemo/Editor/Resources/Textures/" + textureName + ".png";
#else
				filename = "Assets/Plugins/RealtimeCSG/Editor/Resources/Textures/" + textureName + ".png";
#endif
				material.mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(filename);
				if (!material.mainTexture)
					Debug.LogWarning("Could not find internal texture: " + filename);
			}
			EditorMaterials.Add(name, material);
			return material;
		}

		internal static Material GenerateEditorColorMaterial(Color color)
		{
			var name = "Color:" + color;

			Material material;
			if (EditorMaterials.TryGetValue(name, out material))
			{
				// just in case one of many unity bugs destroyed the material
				if (!material)
				{
					EditorMaterials.Remove(name);
				} else
					return material;
			}

			var shader = Shader.Find("Unlit/Color");
			if (!shader)
				return null;

			material = new Material(shader)
			{
				name		= name.Replace(':', '_'),
				hideFlags	= HideFlags.None | HideFlags.DontUnloadUnusedAsset
			};
			material.SetColor("_Color", color);

			EditorMaterials.Add(name, material);
			return material;
		}


		internal static bool EqualInternalMaterial(Material o, Material n)
		{
			return	((bool)o == (bool)n) &&
					o.shader == n.shader && 
					o.mainTexture == n.mainTexture && 
					//o.Equals(n);
					o.name == n.name;
		}

		internal static Material GetRuntimeMaterial(string materialName)
		{
#if DEMO
			return AssetDatabase.LoadAssetAtPath<Material>(string.Format("Assets/Plugins/RealtimeCSGDemo/Runtime/Materials/{0}.mat", materialName));
#else
			return AssetDatabase.LoadAssetAtPath<Material>(string.Format("Assets/Plugins/RealtimeCSG/Runtime/Materials/{0}.mat", materialName));
#endif
		}


		private static readonly Dictionary<Color,Material> ColorMaterials = new Dictionary<Color, Material>();
		internal static Material GetColorMaterial(Color color)
		{
			Material material;
			if (ColorMaterials.TryGetValue(color, out material))
			{
				// just in case one of many unity bugs destroyed the material
				if (!material)
				{
					ColorMaterials.Remove(color);
				} else
					return material;
			}
			
			material = GenerateEditorColorMaterial(color);
			if (!material)
				return AssetDatabase.GetBuiltinExtraResource<Material>("Default-Diffuse.mat");

			ColorMaterials.Add(color, material);
			return material;
		}

		private static Material _missingMaterial;
		public static Material MissingMaterial
		{
			get
			{
				if (!_missingMaterial)
				{
					_missingMaterial = GetColorMaterial(Color.magenta);
					if (!_missingMaterial)
						return AssetDatabase.GetBuiltinExtraResource<Material>("Default-Diffuse.mat");
				}
				return _missingMaterial;
			}
		}
		

		private static Material _wallMaterial;
		public static Material WallMaterial
		{
			get
			{
				if (!_wallMaterial)
				{
					_wallMaterial = GetRuntimeMaterial("wall");
					if (!_wallMaterial)
						return AssetDatabase.GetBuiltinExtraResource<Material>("Default-Diffuse.mat");
				}
				return _wallMaterial;
			}
		}

		private static Material _floorMaterial;
		public static Material FloorMaterial
		{
			get
			{
				if (!_floorMaterial)
				{
					_floorMaterial = GetRuntimeMaterial("floor");
					if (!_floorMaterial)
						return AssetDatabase.GetBuiltinExtraResource<Material>("Default-Diffuse.mat");
				}
				return _floorMaterial;
			}
		}

		private static float _lineThicknessMultiplier = 1.0f;
		internal static float LineThicknessMultiplier
		{
			get { return _lineThicknessMultiplier; }
			set
			{
				if (Math.Abs(_lineThicknessMultiplier - value) < MathConstants.EqualityEpsilon)
					return;
				_lineThicknessMultiplier = value;
			}
		}

		private static float _lineDashMultiplier = 1.0f;
		internal static float LineDashMultiplier
		{
			get { return _lineDashMultiplier; }
			set
			{
				if (Math.Abs(_lineDashMultiplier - value) < MathConstants.EqualityEpsilon)
					return;
				_lineDashMultiplier = value;
			}
		}

		private static float _lineAlphaMultiplier = 1.0f;
		internal static float LineAlphaMultiplier
		{
			get { return _lineAlphaMultiplier; }
			set
			{
				if (Math.Abs(_lineAlphaMultiplier - value) < MathConstants.EqualityEpsilon)
					return;
				_lineAlphaMultiplier = value;
			}
		}


		private static Material _zTestGenericLine;
		internal static Material ZTestGenericLine
		{
			get
			{
				if (!_zTestGenericLine)
					_zTestGenericLine = GenerateEditorMaterial("ZTestGenericLine");
				return _zTestGenericLine;
			}
		}

		internal static void InitGenericLineMaterial(Material genericLineMaterial)
		{
			if (!genericLineMaterial)
				return;
			
			if (!_shadersInitialized) ShaderInit();
			if (_pixelsPerPointId != -1)
			{
#if UNITY_5_4_OR_NEWER
				genericLineMaterial.SetFloat(_pixelsPerPointId, EditorGUIUtility.pixelsPerPoint);
#else
				genericLineMaterial.SetFloat(_pixelsPerPointId, 1.0f);
#endif
			}
			if (_lineThicknessMultiplierId != -1) genericLineMaterial.SetFloat(_lineThicknessMultiplierId, _lineThicknessMultiplier);
			if (_lineDashMultiplierId      != -1) genericLineMaterial.SetFloat(_lineDashMultiplierId,      _lineDashMultiplier);
			if (_lineAlphaMultiplierId	   != -1) genericLineMaterial.SetFloat(_lineAlphaMultiplierId,     _lineAlphaMultiplier);
		}

		private static Material _noZTestGenericLine;
		internal static Material NoZTestGenericLine
		{
			get
			{
				if (!_noZTestGenericLine)
				{
					_noZTestGenericLine = GenerateEditorMaterial("NoZTestGenericLine");
				}
				return _noZTestGenericLine;
			}
		}

		private static Material _coloredPolygonMaterial;
		internal static Material ColoredPolygonMaterial
		{
			get
			{
				if (!_coloredPolygonMaterial)
				{
					_coloredPolygonMaterial = GenerateEditorMaterial("customSurface");
				}
				return _coloredPolygonMaterial;
			}
		}

		private static Material _hiddenMaterial;
		internal static Material HiddenMaterial		{ get { if (!_hiddenMaterial) _hiddenMaterial = GenerateEditorMaterial(TransparentSpecialSurfaceShader, HiddenName, HiddenMaterialName); return _hiddenMaterial; } }

		private static Material _culledMaterial;
		internal static Material CulledMaterial		{ get { if (!_culledMaterial) _culledMaterial = GenerateEditorMaterial(TransparentSpecialSurfaceShader, CulledName, CulledMaterialName); return _culledMaterial; } }

		private static Material _colliderMaterial;
		internal static Material ColliderMaterial	{ get { if (!_colliderMaterial) _colliderMaterial = GenerateEditorMaterial(SpecialSurfaceShader, ColliderName, ColliderMaterialName); return _colliderMaterial; } }
		 
		private static Material _triggerMaterial;
		internal static Material TriggerMaterial	{ get { if (!_triggerMaterial) _triggerMaterial = GenerateEditorMaterial(TransparentSpecialSurfaceShader, TriggerName, TriggerMaterialName); return _triggerMaterial; } }

		private static Material _shadowOnlyMaterial;
		internal static Material ShadowOnlyMaterial { get { if (!_shadowOnlyMaterial) _shadowOnlyMaterial = GenerateEditorMaterial(SpecialSurfaceShader, ShadowOnlyName, ShadowOnlyMaterialName); return _shadowOnlyMaterial; } }

		private static Material _castShadowsMaterial;
		internal static Material CastShadowsMaterial { get { if (!_castShadowsMaterial) _castShadowsMaterial = GenerateEditorMaterial(SpecialSurfaceShader, CastShadowsName, CastShadowsMaterialName); return _castShadowsMaterial; } }

		private static Material _receiveShadowsMaterial;
		internal static Material ReceiveShadowsMaterial { get { if (!_receiveShadowsMaterial) _receiveShadowsMaterial = GenerateEditorMaterial(SpecialSurfaceShader, ReceiveShadowsName, ReceiveShadowsMaterialName); return _receiveShadowsMaterial; } }


		internal static Texture2D CreateSolidColorTexture(int width, int height, Color color)
		{
			var pixels = new Color[width * height];
			for (var i = 0; i < pixels.Length; i++)
				pixels[i] = color;
			var newTexture = new Texture2D(width, height);
            newTexture.hideFlags = HideFlags.DontUnloadUnusedAsset;
			newTexture.SetPixels(pixels);
			newTexture.Apply();
			return newTexture;
		}
		
		internal static RenderSurfaceType GetMaterialSurfaceType(Material material)
		{
			if (!material)
				return RenderSurfaceType.Normal;
			
			var shaderName = material.shader.name;
			if (shaderName != SpecialSurfaceShaderName &&
				shaderName != TransparentSpecialSurfaceShaderName)
				return RenderSurfaceType.Normal;

			switch (material.name)
			{
				case HiddenMaterialName:			return RenderSurfaceType.Hidden;
				case CulledMaterialName:			return RenderSurfaceType.Culled;
				case ColliderMaterialName:			return RenderSurfaceType.Collider;
				case TriggerMaterialName:			return RenderSurfaceType.Trigger;
				case ShadowOnlyMaterialName:		return RenderSurfaceType.ShadowOnly;
				case CastShadowsMaterialName:		return RenderSurfaceType.CastShadows;
				case ReceiveShadowsMaterialName:	return RenderSurfaceType.ReceiveShadows;
			}

			if (shaderName != SpecialSurfaceShaderName)// || shaderName != TransparentSpecialSurfaceShaderName
				return RenderSurfaceType.Normal;


			return RenderSurfaceType.Normal;
		}

		internal static Material GetSurfaceMaterial(RenderSurfaceType renderSurfaceType)
		{
			switch (renderSurfaceType)
			{
				case RenderSurfaceType.Hidden:			return HiddenMaterial; 
				case RenderSurfaceType.Culled:			return CulledMaterial; 
				case RenderSurfaceType.Collider:		return ColliderMaterial; 
				case RenderSurfaceType.Trigger:			return TriggerMaterial; 
				case RenderSurfaceType.ShadowOnly:		return ShadowOnlyMaterial;
				case RenderSurfaceType.CastShadows:		return CastShadowsMaterial;
				case RenderSurfaceType.ReceiveShadows:	return ReceiveShadowsMaterial;
				case RenderSurfaceType.Normal:			return null;
				default:								return null;
			}
		}
	
	}
}