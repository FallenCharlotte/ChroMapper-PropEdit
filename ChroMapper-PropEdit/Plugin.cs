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
	
#if CHROMPER_13
	const bool DEV = false;
	const int TARGET_VER = 13;
#else
	const bool DEV = true;
	const int TARGET_VER = 14;
#endif
	
	[Init]
	private void Init() {
		try {
			var chromper_ver = new System.Version(Application.version);
			
			Plugin.Trace($"{chromper_ver.Minor} vs {TARGET_VER}");
			
			if (chromper_ver.Minor != TARGET_VER) {
				var bypass =  Settings.Get("BypassVersion");
				// Allow testing in uncharted territory
				bool allowBypass = DEV && (chromper_ver.Minor > TARGET_VER);
				
				if (bypass == false) {
					Debug.Log("Not loading PropEdit on incompatible version!");
					return;
				}
				if (bypass == null || !allowBypass) {
					var dialog = PersistentUI.Instance.CreateNewDialogBox().WithNoTitle();
					dialog.AddComponent<TextComponent>().WithInitialValue($"This PropEdit is made for ChroMapper v0.{TARGET_VER}.x! Please install the correct version." + (allowBypass ? " Do you want to try to run it anyways?" : ""));
					if (allowBypass) {
						dialog.AddFooterButton(() => {
							Settings.Set("BypassVersion", false);
							Debug.Log("Not loading PropEdit on incompatible version!");
						}, "No");
						dialog.AddFooterButton(() => {
							Debug.Log("PropEdit version check bypassed!");
							DoInit();
						}, "Yes");
						dialog.AddFooterButton(() => {
							Settings.Set("BypassVersion", true);
							Debug.Log("PropEdit version check bypassed!");
							DoInit();
						}, "Always");
					}
					else {
						dialog.AddFooterButton(() => {}, "Okay");
					}
					dialog.Open();
					
					return;
				}
				else {
					Debug.Log("PropEdit version check bypassed!");
				}
			}
			
			DoInit();
		}
		catch (System.Exception e) {
			Debug.LogException(e);
		}
	}
	
	private void DoInit() {
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
	
	private void SceneLoaded(Scene scene, LoadSceneMode mode) {
		try {
			if (scene.buildIndex == 3) {
				// Map Edit
				Utils.Selection.Reset();
				var mapEditorUI = (MapEditorUI)Object.FindFirstObjectByType(typeof(MapEditorUI));
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
		var st = new System.Diagnostics.StackTrace(true);
		var caller = st.GetFrame(1);
		Debug.Log($"{caller.GetFileName()}:{caller.GetFileLineNumber()} {message}");
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
