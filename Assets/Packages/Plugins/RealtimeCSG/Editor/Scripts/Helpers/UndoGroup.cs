using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RealtimeCSG
{
	internal sealed class UndoGroup : IDisposable
	{
		private CSGBrush[] _brushes;
		private int _undoGroupIndex;

		public UndoGroup(SelectedBrushSurface[] selectedBrushSurfaces, string name, bool ignoreGroup = false)
		{
			if (selectedBrushSurfaces == null)
				return;
			
			var uniqueBrushes	= new HashSet<CSGBrush>();
			var uniqueModels	= new HashSet<CSGModel>();
			for (int i = 0; i < selectedBrushSurfaces.Length; i++)
			{
				if (!selectedBrushSurfaces[i].brush)
					continue;

				var brush = selectedBrushSurfaces[i].brush;
//				var surface_index = selectedBrushSurfaces[i].surfaceIndex;
				if (uniqueBrushes.Add(brush))
				{
					CSGBrushCache brushCache = InternalCSGModelManager.GetBrushCache(brush);
					if (brushCache != null)
					{
						uniqueModels.Add(brushCache.childData.Model);
					}
				}
			}

			_undoGroupIndex = -1;

			_brushes = uniqueBrushes.ToArray();
			if (_brushes.Length > 0)
			{
				if (!ignoreGroup)
				{
					_undoGroupIndex = Undo.GetCurrentGroup();
					Undo.IncrementCurrentGroup();
				}
				Undo.RegisterCompleteObjectUndo(_brushes, name);
				for (int i = 0; i < _brushes.Length; i++)
				{
					if (!_brushes[i])
						continue;
					UnityEditor.EditorUtility.SetDirty(_brushes[i]);
				}
			}
		}
			
		private bool disposedValue = false;
		public void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					if (_brushes == null)
						return;

					if (_brushes.Length > 0)
					{
						for (int i = 0; i < _brushes.Length; i++)
						{
							if (!_brushes[i])
								continue;
							_brushes[i].EnsureInitialized();
							ShapeUtility.CheckMaterials(_brushes[i].Shape);
						}
						for (int i = 0; i < _brushes.Length; i++)
						{
							if (!_brushes[i])
								continue;
							InternalCSGModelManager.CheckSurfaceModifications(_brushes[i], true);
						}
						if (_undoGroupIndex != -1)
						{
							Undo.CollapseUndoOperations(_undoGroupIndex);
							Undo.FlushUndoRecordObjects();
						}
					}
				}
				_brushes = null;
				disposedValue = true;
			}
		}

		public void Dispose() { Dispose(true); }
	}
		
}
