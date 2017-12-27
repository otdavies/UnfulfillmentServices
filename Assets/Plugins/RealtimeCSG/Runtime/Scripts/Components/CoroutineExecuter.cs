using System;
using System.Collections;
using UnityEngine;

namespace InternalRealtimeCSG
{
    [ExecuteInEditMode]
	internal sealed class CoroutineExecuter : MonoBehaviour
    {
        static MonoBehaviour Singleton;
        void Awake() { Singleton = this; }
        void OnDestroy() { Singleton = null; }

        public static MonoBehaviour GetSingleton()
        {
            if (!Singleton || Singleton == null)
            {
                var gameObject = new GameObject();
                gameObject.hideFlags = HideFlags.HideAndDontSave | HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
                Singleton = gameObject.AddComponent<CoroutineExecuter>();
            }
            return Singleton;
        }
		
        public static new Coroutine StartCoroutine(IEnumerator coroutine)
        {
            if (!Singleton || Singleton == null)
            {
                Singleton = CoroutineExecuter.GetSingleton();
                if (!Singleton || Singleton == null)
                {
                    Debug.LogWarning("Tried to execute coroutine but CoroutineExecuter was not initialized.");
                    return null;
                }
            }
			Singleton.StopAllCoroutines();
            return Singleton.StartCoroutine(coroutine);
        }
    }
}
