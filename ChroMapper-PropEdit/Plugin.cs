using System.Linq;
using System.Runtime.CompilerServices; 
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
	public static ExtensionButton? main_button = null;
	
	[Init]
	private void Init() {
		try {
			var chromper_ver = new System.Version(Application.version);
			if (chromper_ver.Minor < 13) {
				Debug.LogError("This PropEdit version requires ChroMapper 0.13.x! Please install the correct version.");
				return;
			}
			else if (chromper_ver.Minor > 13) {
				Debug.LogWarning("This PropEdit version has only been tested on ChroMapper 0.13.x! There may be problems, good luck!");
			}
			
			SceneManager.sceneLoaded += SceneLoaded;
			
			var map = CMInputCallbackInstaller.InputInstance.asset.actionMaps
				.Where(x => x.name == "Node Editor")
				.FirstOrDefault();
			CMInputCallbackInstaller.InputInstance.Disable();
			
			toggle_window = map.AddAction("Prop Editor", type: InputActionType.Button);
			toggle_window.AddCompositeBinding("ButtonWithOneModifier")
				.With("Modifier", "<Keyboard>/shift")
				.With("Button", "<Keyboard>/n");
			
			array_insert = map.AddAction("Array Insert", type: InputActionType.Button);
			array_insert.AddCompositeBinding("ButtonWithOneModifier")
				.With("Modifier", "<Keyboard>/shift")
				.With("Button", "<Keyboard>/enter");
			
			CMInputCallbackInstaller.InputInstance.Enable();
			
			main_button = ExtensionButtons.AddButton(
				UI.LoadSprite("ChroMapper_PropEdit.Resources.Icon.png"),
				"Prop Edit",
				ToggleWindow);
		}
		catch (System.Exception e) {
			Debug.LogException(e);
		}
	}
	
	private void SceneLoaded(Scene scene, LoadSceneMode mode) {
		try {
			if (scene.buildIndex == 3) {
				// Map Edit
				Utils.Selection.Reset();
				var mapEditorUI = Object.FindObjectOfType<MapEditorUI>();
				main = UIWindow.Create<MainWindow>(mapEditorUI);
				map_settings = UIWindow.Create<MapSettingsWindow>(mapEditorUI);
				plugin_settings = UIWindow.Create<PluginSettingsWindow>(mapEditorUI);
			}
		}
		catch (System.Exception e) {
			Debug.LogException(e);
		}
	}
	
	[Exit]
	private void Exit() {
		
	}
	
	// For extra debug logging that shouldn't be included in releases
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void Trace(object message) {
#if EXTRA_LOGGING
		Debug.Log(message);
#endif
	}
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void StackTrace() {
#if EXTRA_LOGGING
		Debug.Log(new System.Diagnostics.StackTrace(true));
#endif
	}
	
	public static void ToggleWindow() {
		if (main != null) main.ToggleWindow();
	}
}

}
