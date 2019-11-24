using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorTerminator : MonoBehaviour 
{
	public Transform highwaterMarkTarget;
	public float terminatorOffset = 6;

	private Transform _transform;
	private Vector3 _originalPosition;
	private float _highWatermark = 0;

	private void Start () 
	{
		_transform = this.transform;
		_originalPosition = _transform.position;
	}
	
	private void Update () 
	{
		// if(highwaterMarkTarget.position.y > _highWatermark)
		// {
		// 	_highWatermark = highwaterMarkTarget.position.y;
		// }
		_highWatermark += Time.deltaTime * 0.5f;

		_transform.position = Vector3.Lerp(_transform.position, _originalPosition + Vector3.up * (_highWatermark - terminatorOffset), Time.deltaTime);
	}
}
