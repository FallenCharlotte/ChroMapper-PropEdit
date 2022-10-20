using System.IO;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

using ChroMapper_PropEdit.Component;

namespace ChroMapper_PropEdit.UserInterface {

public class UI {
	public ExtensionButton main_button;
	public GameObject window;
	
	public UI() {
		Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("ChroMapper_PropEdit.Resources.Icon.png");
		byte[] data = new byte[stream.Length];
		stream.Read(data, 0, (int)stream.Length);
		
		Texture2D texture2D = new Texture2D(256, 256);
		texture2D.LoadImage(data);
		
		var _sprite = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), new Vector2(0, 0), 100.0f);
		
		main_button = ExtensionButtons.AddButton(_sprite, "Prop Edit", ToggleWindow);
	}
	
	public void AddWindow(MapEditorUI mapEditorUI) {
		var parent = mapEditorUI.MainUIGroup[5];
		
		window = new GameObject("PropEdit Window");
		window.transform.parent = parent.transform;
		// Window Drag
		window.AddComponent<DragWindowController>();
		window.GetComponent<DragWindowController>().canvas = parent.GetComponent<Canvas>();
		//window.GetComponent<DragWindowController>().OnDragWindow += AnchoredPosSave;

		AttachTransform(window, 220, 256, 1, 0, -5, 5, 1, 0);
		var image = window.AddComponent<Image>();

		image.sprite = PersistentUI.Instance.Sprites.Background;
		image.type = Image.Type.Sliced;
		image.color = new Color(0.24f, 0.24f, 0.24f, 1);
		
		window.SetActive(false);
	}
	
	public void ToggleWindow() {
		window.SetActive(!window.activeSelf);
	}
	
	private RectTransform AttachTransform(GameObject obj, float width, float height, float anchorX, float anchorY, float x, float y, float p1 = 0.5f, float p2 = 0.5f)
	{
		var rectTransform = obj.AddComponent<RectTransform>();
		rectTransform.localScale = new Vector3(1, 1, 1);
		rectTransform.sizeDelta = new Vector2(width, height);
		rectTransform.pivot = new Vector2(p1, p2);
		rectTransform.anchorMin = rectTransform.anchorMax = new Vector2(anchorX, anchorY);
		rectTransform.anchoredPosition = new Vector3(x, y, 0);

		return rectTransform;
	}
}

}
