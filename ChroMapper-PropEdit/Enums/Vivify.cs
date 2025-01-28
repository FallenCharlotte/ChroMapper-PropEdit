using System.Collections.Generic;

namespace ChroMapper_PropEdit.Enums {

public static class Vivify {
	public static readonly Map<string?> Orders = new Map<string?> {
		{"BeforeMainEffect", "Before Main Effect"},
		{"AfterMainEffect", "After Main Effect"},
	};
	
	public static readonly Map<string?> ColorFormats = new Map<string?> {
		{"ARGB32", "ARGB32"},
		{"Depth", "Depth"},
		{"ARGBHalf", "ARGBHalf"},
		{"Shadowmap", "Shadowmap"},
		{"RGB565", "RGB565"},
		{"ARGB4444", "ARGB4444"},
		{"ARGB1555", "ARGB1555"},
		{"Default", "Default"},
		{"ARGB2101010", "ARGB2101010"},
		{"DefaultHDR", "DefaultHDR"},
		{"ARGB64", "ARGB64"},
		{"ARGBFloat", "ARGBFloat"},
		{"RGFloat", "RGFloat"},
		{"RGHalf", "RGHalf"},
		{"RFloat", "RFloat"},
		{"RHalf", "RHalf"},
		{"R8", "R8"},
		{"ARGBInt", "ARGBInt"},
		{"RGInt", "RGInt"},
		{"RInt", "RInt"},
		{"BGRA32", "BGRA32"},
		{"RGB111110Float", "RGB111110Float"},
		{"RG32", "RG32"},
		{"RGBAUShort", "RGBAUShort"},
		{"RG16", "RG16"},
		{"BGRA10101010_XR", "BGRA10101010_XR"},
		{"BGR101010_XR", "BGR101010_XR"},
		{"R16", "R16"},
	};
	
	public static readonly Map<string?> FilterModes = new Map<string?> {
		{"Point", "Point"},
		{"Bilinear", "Bilinear"},
		{"Trilinear", "Trilinear"},
	};
	
	public static readonly Map<string?> LoadModes = new Map<string?> {
		{"Single", "Single"},
		{"Additive", "Additive"},
	};
	
	public static readonly Map<string?> ClearFlags = new Map<string?> {
		{"Skybox", "Skybox"},
		{"SolidColor", "Solid Color"},
		{"Depth", "Depth"},
		{"Nothing", "Nothing"},
	};
}

}
