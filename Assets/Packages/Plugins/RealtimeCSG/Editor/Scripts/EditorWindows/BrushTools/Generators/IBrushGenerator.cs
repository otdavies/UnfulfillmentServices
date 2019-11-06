using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RealtimeCSG
{
	internal interface IBrushGenerator
	{
		bool HaveBrushes { get; }
		bool CanCommit { get; }

		CSGOperationType CurrentCSGOperationType { get; set; }

		void Init();

		bool HotKeyReleased();

		bool UndoRedoPerformed();
		void PerformDeselectAll();

		void HandleEvents(Rect sceneRect);
		
		bool OnShowGUI(bool isSceneGUI);
		void StartGUI();
		void FinishGUI();

		void DoCancel();
		void DoCommit();

		void OnDefaultMaterialModified();
	}
}
