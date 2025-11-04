using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Beatmap.Base;
using Beatmap.Base.Customs;
using SimpleJSON;

namespace ChroMapper_PropEdit.Utils {

public enum SelectionType {
	None,
	Objects,
	PointDefinitions,
	EnvironmentEnhancements,
	Materials,
};

public class Selection {
	public static SelectionType SelectedType { get; private set; } = SelectionType.None;
	public static IList? Selected { get; private set; } = null;
	
	public static Action? OnSelectionChanged;
	
	public static void Reset() {
		OnSelectionChanged = null;
	}
	
	public static void OnObjectsSelected() {
		if (refreshing) return;
		SelectedType = SelectionType.Objects;
		Selected = SelectionController.HasSelectedObjects()
			? SelectionController.SelectedObjects.Select(it => it).ToList()
			: null;
		OnSelectionChanged?.Invoke();
	}
	
	public static void OnPDsSelected(List<JSONArray> sel) {
		SpecialType(SelectionType.PointDefinitions, sel);
	}
	
	public static void OnMatsSelected(List<BaseMaterial> sel) {
		SpecialType(SelectionType.Materials, sel);
	}
	
	public static void OnDeselectAll() {
		SelectedType = SelectionType.None;
		Selected = null;
	}
	
	private static void SpecialType<T>(SelectionType type, List<T> sel) {
		refreshing = true;
		if (SelectionController.HasSelectedObjects()) {
			SelectionController.DeselectAll();
		}
		SelectedType = type;
		Selected = sel;
		refreshing = false;
		OnSelectionChanged?.Invoke();
	}
	
	private static bool refreshing = false;
};

}
