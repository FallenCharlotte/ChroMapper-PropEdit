using UnityEngine;
using UnityEngine.SceneManagement;

namespace ChroMapper_PropEditor {

[Plugin("Prop Editor")]
public class Plugin
{
	[Init]
	private void Init() {
		
	}
	
	private void SceneLoaded(Scene scene, LoadSceneMode mode) {
		if (scene.buildIndex == 3) {
			GameObject obj = new GameObject("PropEditor", typeof(PropEditor));
		}
	}
	
	[Exit]
	private void Exit() {
		
	}
}

}
