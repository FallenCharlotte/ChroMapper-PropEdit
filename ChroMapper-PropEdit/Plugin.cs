using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

using ChroMapper_PropEdit.UserInterface;

namespace ChroMapper_PropEdit {

[Plugin("PropEdit")]
public class Plugin {
	public static MainWindow? main = null;
	public static MapSettingsWindow? map_settings = null;
	public static PluginSettingsWindow? plugin_settings = null;
	public static InputAction? toggle_window = null;
	public static InputAction? array_insert = null;
	
	[Init]
	private void Init() {
		try {
			SceneManager.sceneLoaded += SceneLoaded;
			
			var map = CMInputCallbackInstaller.InputInstance.asset.actionMaps
				.Where(x => x.name == "Node Editor")
				.FirstOrDefault();
			CMInputCallbackInstaller.InputInstance.Disable();
			
			toggle_window = map.AddAction("Prop Editor", type: InputActionType.Button);
			toggle_window.AddCompositeBinding("ButtonWithOneModifier")
				.With("Button", "<Keyboard>/n")
				.With("Modifier", "<Keyboard>/shift");
			
			array_insert = map.AddAction("Array Insert", type: InputActionType.Button);
			array_insert.AddCompositeBinding("ButtonWithOneModifier")
				.With("Button", "<Keyboard>/enter")
				.With("Modifier", "<Keyboard>/shift");
			
			CMInputCallbackInstaller.InputInstance.Enable();
		}
		catch (System.Exception e) {
			Debug.LogException(e);
		}
	}
	
	private void SceneLoaded(Scene scene, LoadSceneMode mode) {
		if (scene.buildIndex == 3) {
			// Map Edit
			var mapEditorUI = Object.FindObjectOfType<MapEditorUI>();
			main = UIWindow.Create<MainWindow>(mapEditorUI);
			map_settings = UIWindow.Create<MapSettingsWindow>(mapEditorUI);
			plugin_settings = UIWindow.Create<PluginSettingsWindow>(mapEditorUI);
		}
	}
	
	[Exit]
	private void Exit() {
		
	}
	
	// For extra debug logging that shouldn't be included in releases
	public static void Trace(object message) {
#if EXTRA_LOGGING
		Debug.Log(message);
#endif
	}
}

}
