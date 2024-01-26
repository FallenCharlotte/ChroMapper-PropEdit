using UnityEngine;
using UnityEngine.SceneManagement;

using ChroMapper_PropEdit.UserInterface;

namespace ChroMapper_PropEdit {

[Plugin("PropEdit")]
public class Plugin {
	public static MainWindow? main = null;
	public static SettingsWindow? settings = null;
	
	[Init]
	private void Init() {
		try {
			SceneManager.sceneLoaded += SceneLoaded;
			main = new MainWindow();
			settings = new SettingsWindow();
		}
		catch (System.Exception e) {
			Debug.LogException(e);
		}
	}
	
	private void SceneLoaded(Scene scene, LoadSceneMode mode) {
		if (scene.buildIndex == 3) {
			// Map Edit
			var mapEditorUI = Object.FindObjectOfType<MapEditorUI>();
			main?.Init(mapEditorUI);
			settings?.Init(mapEditorUI);
		}
		else {
			main?.Disable();
		}
	}
	
	[Exit]
	private void Exit() {
		
	}
}

}
