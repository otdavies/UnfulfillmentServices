using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RealtimeCSG
{
	internal static class LinearStairsGeneratorGUI
	{
		private static readonly GUIContent	StepDepthContent			= new GUIContent("Step Depth");
		private static readonly ToolTip		StepDepthTooltip			= new ToolTip("Step Depth", "Set how deep the steps of the stairs are");
		
		private static readonly GUIContent	StepHeightContent			= new GUIContent("Step Height");
		private static readonly ToolTip		StepHeightTooltip			= new ToolTip("Step Height", "Set how high the steps of the stairs are");
		
		private static readonly GUIContent	StairsWidthContent			= new GUIContent("Stairs Width");
		private static readonly ToolTip		StairsWidthTooltip			= new ToolTip("Stairs Width", "Set how wide the entire staircase is");

		private static readonly GUIContent	StairsHeightContent			= new GUIContent("Stairs Height");
		private static readonly ToolTip		StairsHeightTooltip			= new ToolTip("Stairs Height", "Set how high the entire staircase is");
		
		private static readonly GUIContent	StairsDepthContent			= new GUIContent("Stairs Depth");
		private static readonly ToolTip		StairsDepthTooltip			= new ToolTip("Stairs Depth", "Set how deep the entire staircase is");
		
		private static readonly GUIContent	TotalStepsContent			= new GUIContent("Total Steps");
		private static readonly ToolTip		TotalStepsTooltip			= new ToolTip("Total Steps", "Set the total number of steps in this staircase");
		
		private static readonly GUIContent	ExtraDepthContent			= new GUIContent("Extra Depth");
		private static readonly ToolTip		ExtraDepthTooltip			= new ToolTip("Extra Depth", "Add an additional space before the steps start at the top");
		
		private static readonly GUIContent	ExtraHeightContent			= new GUIContent("Extra Height");
		private static readonly ToolTip		ExtraHeightTooltip			= new ToolTip("Extra Height", "Add additional height to the step at the bottom of the staircase");
		
		private static readonly GUIContent	StairsBottomContent			= new GUIContent("Bottom");
		private static readonly ToolTip		StairsBottomTooltip			= new ToolTip("Bottom", "Sets how the bottom of the stair steps should look");
		
		

		private static readonly GUILayoutOption width25					= GUILayout.Width(25);
		private static readonly GUILayoutOption width65					= GUILayout.Width(65);
		private static readonly GUILayoutOption width80					= GUILayout.Width(80);
		
		static float FloatUnitsSettings(float value, GUIContent content, ToolTip tooltip, bool isSceneGUI)
		{
			var distanceUnit = RealtimeCSG.CSGSettings.DistanceUnit;
			var unitText = Units.GetUnitGUIContent(distanceUnit);

			float newValue;
			EditorGUI.BeginChangeCheck();
			{ 
				GUILayout.BeginHorizontal(GUIStyleUtility.ContentEmpty);
				{
					GUILayout.Label(content, width80);
					if (isSceneGUI)
						TooltipUtility.SetToolTip(tooltip);

					if (!isSceneGUI)
						newValue = Units.DistanceUnitToUnity(distanceUnit, EditorGUILayout.DoubleField(Units.UnityToDistanceUnit(distanceUnit, value)));
					else
						newValue = Units.DistanceUnitToUnity(distanceUnit, EditorGUILayout.DoubleField(Units.UnityToDistanceUnit(distanceUnit, value), width65));

					if (GUILayout.Button(unitText, EditorStyles.miniLabel, width25))
					{
						distanceUnit = Units.CycleToNextUnit(distanceUnit);
						RealtimeCSG.CSGSettings.DistanceUnit = distanceUnit;
						RealtimeCSG.CSGSettings.UpdateSnapSettings();
						RealtimeCSG.CSGSettings.Save();
						SceneView.RepaintAll();
					}
				}
				GUILayout.EndHorizontal();
				if (!isSceneGUI)
					TooltipUtility.SetToolTip(tooltip);
			}
			if (EditorGUI.EndChangeCheck())
				return newValue;
			return value;
		}

		static int IntValueSettings(int value, GUIContent content, ToolTip tooltip, bool isSceneGUI)
		{
			int newValue;
			EditorGUI.BeginChangeCheck();
			{ 
				GUILayout.BeginHorizontal(GUIStyleUtility.ContentEmpty);
				{
					GUILayout.Label(content, width80);
					if (isSceneGUI)
						TooltipUtility.SetToolTip(tooltip);

					if (!isSceneGUI)
						newValue = EditorGUILayout.IntField(value);
					else
						newValue = EditorGUILayout.IntField(value, width65);
				}
				GUILayout.EndHorizontal();
				if (!isSceneGUI)
					TooltipUtility.SetToolTip(tooltip);
			}
			if (EditorGUI.EndChangeCheck())
				return newValue;
			return value;
		}
		

		static StairsBottom EnumValueSettings(StairsBottom value, GUIContent content, ToolTip tooltip, bool isSceneGUI)
		{
			StairsBottom newValue;
			EditorGUI.BeginChangeCheck();
			{ 
				GUILayout.BeginHorizontal(GUIStyleUtility.ContentEmpty);
				{
					GUILayout.Label(content, width80);
					if (isSceneGUI)
						TooltipUtility.SetToolTip(tooltip);

					if (!isSceneGUI)
						newValue = (StairsBottom)EditorGUILayout.EnumPopup(value);
					else
						newValue = (StairsBottom)EditorGUILayout.EnumPopup(value, width65);
				}
				GUILayout.EndHorizontal();
				if (!isSceneGUI)
					TooltipUtility.SetToolTip(tooltip);
			}
			if (EditorGUI.EndChangeCheck())
				return newValue;
			return value;
		}



		static void OnGUIContents(LinearStairsGenerator generator, bool isSceneGUI)
		{	
			GUILayout.BeginVertical(GUIStyleUtility.ContentEmpty);
			{
				EditorGUI.BeginChangeCheck();
				var totalSteps	= Mathf.Max(IntValueSettings  (generator.TotalSteps,	TotalStepsContent,		TotalStepsTooltip,		isSceneGUI), 1);
				if (EditorGUI.EndChangeCheck()) { generator.TotalSteps = totalSteps; }

				EditorGUI.BeginChangeCheck();
				var stepDepth		= Mathf.Max(FloatUnitsSettings(generator.StepDepth,	StepDepthContent,		StepDepthTooltip,		isSceneGUI), LinearStairsSettings.kMinStepDepth);
				if (EditorGUI.EndChangeCheck()) { generator.StepDepth = stepDepth; }

				EditorGUI.BeginChangeCheck();
				var stepHeight	= Mathf.Max(FloatUnitsSettings(generator.StepHeight,	StepHeightContent,		StepHeightTooltip,		isSceneGUI), LinearStairsSettings.kMinStepHeight);
				if (EditorGUI.EndChangeCheck()) { generator.StepHeight = stepHeight; }

				GUILayout.Space(4);

				EditorGUI.BeginChangeCheck();
				var stairsWidth	= Mathf.Max(FloatUnitsSettings(generator.StairsWidth,   StairsWidthContent,		StairsWidthTooltip,		isSceneGUI), 0.01f);
				if (EditorGUI.EndChangeCheck()) { generator.StairsWidth = stairsWidth; }

				EditorGUI.BeginChangeCheck();
				var stairsHeight	= Mathf.Max(FloatUnitsSettings(generator.StairsHeight,  StairsHeightContent,	StairsHeightTooltip,	isSceneGUI), 0.01f);
				if (EditorGUI.EndChangeCheck()) { generator.StairsHeight = stairsHeight; }

				EditorGUI.BeginChangeCheck();
				var stairsDepth	= Mathf.Max(FloatUnitsSettings(generator.StairsDepth,	StairsDepthContent,		StairsDepthTooltip,		isSceneGUI), 0.01f);
				if (EditorGUI.EndChangeCheck()) { generator.StairsDepth = stairsDepth; }

				GUILayout.Space(4);

				EditorGUI.BeginChangeCheck();
				var extraDepth	= Mathf.Max(FloatUnitsSettings(generator.ExtraDepth,	ExtraDepthContent,		ExtraDepthTooltip,		isSceneGUI), 0);
				if (EditorGUI.EndChangeCheck()) { generator.ExtraDepth = extraDepth; }

				EditorGUI.BeginChangeCheck();
				var extraHeight	= Mathf.Max(FloatUnitsSettings(generator.ExtraHeight,	ExtraHeightContent,		ExtraHeightTooltip,		isSceneGUI), 0);				
				if (EditorGUI.EndChangeCheck()) { generator.ExtraHeight = extraHeight; }

				EditorGUI.BeginChangeCheck();
				var bottom = EnumValueSettings(generator.StairsBottom,	StairsBottomContent,	StairsBottomTooltip,	isSceneGUI);
				if (EditorGUI.EndChangeCheck()) { generator.StairsBottom = bottom; }
			}
			GUILayout.EndVertical();
		}
		
		public static bool OnShowGUI(LinearStairsGenerator generator, bool isSceneGUI)
		{
			GUIStyleUtility.InitStyles();
			OnGUIContents(generator, isSceneGUI);
			return true;
		}
	}
}
