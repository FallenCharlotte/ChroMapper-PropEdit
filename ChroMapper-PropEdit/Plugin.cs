using UnityEngine;
using UnityEngine.SceneManagement;

using ChroMapper_PropEdit.UserInterface;

namespace ChroMapper_PropEdit {

[Plugin("Prop Editor")]
public class Plugin {
	public static MainWindow main;
	
	[Init]
	private void Init() {
		Debug.Log("Prop Edit Plugin has loaded!");
		SceneManager.sceneLoaded += SceneLoaded;
		main = new MainWindow();
	}
	
	private void SceneLoaded(Scene scene, LoadSceneMode mode) {
		if (scene.buildIndex == 3) {
			// Map Edit
			var mapEditorUI = Object.FindObjectOfType<MapEditorUI>();
			main.Init(mapEditorUI);
			
			SelectionController.SelectionChangedEvent += main.UpdateSelection;
		}
	}
	
	[Exit]
	private void Exit() {
		
	}
}

}
