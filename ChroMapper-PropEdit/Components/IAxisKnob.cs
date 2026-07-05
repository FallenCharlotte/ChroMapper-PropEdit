// Vaguely based on PaulMapper

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

using Beatmap.Enums;

namespace ChroMapper_PropEdit.Components {

// I hate C#
public abstract class IAxisKnob : MonoBehaviour
{
	// onMove(delta)
	public event Action onDragBegin;
	public event Action<Vector3> onDragMove;
	public event Action onDragEnd;
	
	public Axis axis;
	
	protected virtual void OnMouseDown() {
		onDragBegin?.Invoke();
	}
	
	protected virtual void OnMouseDrag() {
		onDragMove?.Invoke(delta);
	}
	
	protected virtual void OnMouseUp() {
		onDragEnd?.Invoke();
	}
	
	protected abstract Vector3? axis_pos();
	
	protected Vector3 delta;
}

}
