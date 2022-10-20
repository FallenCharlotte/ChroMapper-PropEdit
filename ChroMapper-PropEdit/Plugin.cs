using UnityEngine;
using UnityEngine.SceneManagement;

using ChroMapper_PropEdit.UserInterface;

namespace ChroMapper_PropEdit {

[Plugin("Prop Editor")]
public class Plugin {
	public static UI ui;
	
	[Init]
	private void Init() {
		Debug.Log("Prop Edit Plugin has loaded!");
		SceneManager.sceneLoaded += SceneLoaded;
		ui = new UI();
	}
	
	private void SceneLoaded(Scene scene, LoadSceneMode mode) {
		if (scene.buildIndex == 3) {
			// Map Edit
			var mapEditorUI = Object.FindObjectOfType<MapEditorUI>();
			ui.AddWindow(mapEditorUI);
		}
	}
	
	[Exit]
	private void Exit() {
		
	}
}

}
