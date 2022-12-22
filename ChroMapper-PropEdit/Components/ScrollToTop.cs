using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace ChroMapper_PropEdit.Components {

public class ScrollToTop : MonoBehaviour {
	public Scrollbar scrollbar;
	
	public void OnEnable() {
		Trigger();
	}
	
	public void Trigger() {
		if (isActiveAndEnabled) {
			StartCoroutine(Scroll());
		}
	}
	
	private IEnumerator Scroll() {
		yield return new WaitForEndOfFrame();
		scrollbar.value = 1f;
		// For some reason we still have occasional race conditions, even though it's frame-locked...
		yield return new WaitForEndOfFrame();
		scrollbar.value = 1f;
		yield return null;
	}
}

}
