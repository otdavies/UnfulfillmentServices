using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class EnvironmentControls : MonoBehaviour 
{
	public Texture ramp;
	private void Update () 
	{
		Shader.SetGlobalFloat("GlowHeight", transform.position.y);
		Shader.SetGlobalTexture("_TransitionRamp", ramp);
	}
}
