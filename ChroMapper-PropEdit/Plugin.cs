using UnityEngine;
using UnityEngine.SceneManagement;

using ChroMapper_PropEdit.UserInterface;

namespace ChroMapper_PropEdit {

[Plugin("PropEdit")]
public class Plugin {
	public static MainWindow? main = null;
	public static MapSettingsWindow? map_settings = null;
	public static PluginSettingsWindow? plugin_settings = null;
	
	[Init]
	private void Init() {
		try {
			SceneManager.sceneLoaded += SceneLoaded;
			main = new MainWindow();
			map_settings = new MapSettingsWindow();
			plugin_settings = new PluginSettingsWindow();
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
			map_settings?.Init(mapEditorUI);
			plugin_settings?.Init(mapEditorUI);
		}
		else {
			main?.Disable();
			map_settings?.Disable();
		}
	}
	
	[Exit]
	private void Exit() {
		
	}
	
	// For extra debug logging that shouldn't be included in releases
	public static void Trace(object message) {
		//Debug.Log(message);
	}
}

}
