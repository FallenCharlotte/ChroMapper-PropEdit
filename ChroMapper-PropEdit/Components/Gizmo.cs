using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Beatmap.Base;
using Beatmap.Containers;
using Beatmap.Enums;
using Beatmap.Helper;

using  ChroMapper_PropEdit.UserInterface;

namespace ChroMapper_PropEdit.Components {

public class Gizmo<T> : Component where T : IAxisKnob {
	public event Action onDragBegin;
	public event Action<Vector3> onDragMove;
	public event Action onDragEnd;
	
	public Dictionary<Axis, T> knobs;
	public PrimitiveType knob_shape = PrimitiveType.Cube;
	
	public bool ball_visible {
		get { return _ball; }
		set {
			_ball = value;
			ball?.SetActive(value);
		}
	}
	private bool _ball = true;
	
	public GameObject ball;
	
	public void Start() {
		ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		ball?.SetActive(_ball);
		ball.transform.parent = this.gameObject.transform;
		ball.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
		ball.transform.localPosition = new Vector3();
		AddAxis(Axis.X, new Vector3(1, 0, 0), new Vector3(0, 0, 90));
		AddAxis(Axis.Y, new Vector3(0, 1, 0), new Vector3(0, 0, 0));
		AddAxis(Axis.Z, new Vector3(0, 0, 1), new Vector3(90, 0, 0));
	}
	
	private void AddAxis(Axis axis, Vector3 pos, Vector3 rot) {
		GameObject knob = GameObject.CreatePrimitive(knob_shape);
		knob.transform.parent = this.gameObject.transform;
		knob.transform.localPosition = pos;
		knob.transform.localEulerAngles = rot;
		knob.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
		var am = knob.AddComponent<T>();
		am.axis = axis;
		am.onDragBegin += OnDragBegin;
		am.onDragMove += OnDragMove;
		am.onDragEnd += OnDragEnd;
		knobs[axis] = am;
	}
	
	void OnDragBegin() {
		onDragBegin?.Invoke();
	}
	
	void OnDragMove(Vector3 delta) {
		onDragMove?.Invoke(delta);
	}
	
	void OnDragEnd() {
		onDragEnd?.Invoke();
	}
	
	
}

}
