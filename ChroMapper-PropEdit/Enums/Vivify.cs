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
	
	public enum PropertyType {
		Texture,
		Float,
		Color,
		Vector,
		Keyword
	};
	public static readonly Map<string?> PropertyTypes = new Map<string?> {
		{"Texture", "Texture"},
		{"Float", "Float"},
		{"Color", "Color"},
		{"Vector", "Vector"},
		{"Keyword", "Keyword"},
	};
	
	// TODO: Point-definition-able types (beeeg project)
	public static readonly Dictionary<string, string> RenderSettings = new Dictionary<string, string> {
		{"renderSettings.ambientEquatorColor", "Ambient Equator Color"},
		{"renderSettings.ambientGroundColor", "Ambient Ground Color"},
		{"renderSettings.ambientIntensity", "Ambient Intensity"},
		{"renderSettings.ambientLight", "Ambient Light"},
		{"renderSettings.ambientMode", "Ambient Mode"},
		{"renderSettings.ambientSkyColor", "Aambient Sky Color"},
		{"renderSettings.defaultReflectionMode", "Default Reflection Mode"},
		{"renderSettings.defaultReflectionResolution", "Default Reflection Resolution"},
		{"renderSettings.flareFadeSpeed", "Flare Fade Speed"},
		{"renderSettings.flareStrength", "Flare Strength"},
		{"renderSettings.fog", "Fog"},
		{"renderSettings.fogColor", "Fog Color"},
		{"renderSettings.fogDensity", "Fog Density"},
		{"renderSettings.fogEndDistance", "Fog End Distance"},
		{"renderSettings.fogMode", "Fog Mode"},
		{"renderSettings.haloStrength", "Halo Strength"},
		{"renderSettings.reflectionBounces", "Reflection Bounces"},
		{"renderSettings.reflectionIntensity", "Reflection Intensity"},
		{"renderSettings.skybox", "Skybox"},
		{"renderSettings.subtractiveShadowColor", "Subtractive Shadow Color"},
		{"renderSettings.sun", "Sun"},
	};
	public static readonly Dictionary<string, string> QualitySettings = new Dictionary<string, string> {
		{"qualitySettings.anisotropicFiltering", "anisotropic Filtering"},
		{"qualitySettings.antiAliasing", "Anti Aliasing"},
		{"qualitySettings.pixelLightCount", "Pixel Light Count"},
		{"qualitySettings.realtimeReflectionProbes", "Realtime Reflection Probes"},
		{"qualitySettings.shadowCascades", "Shadow Cascades"},
		{"qualitySettings.shadowDistance", "Shadow Distance"},
		{"qualitySettings.shadowmaskMode", "Shadowmask Mode"},
		{"qualitySettings.shadowNearPlaneOffset", "Shadow Near Plane Offset"},
		{"qualitySettings.shadowProjection", "Shadow Projection"},
		{"qualitySettings.shadowResolution", "Shadow Resolution"},
		{"qualitySettings.shadows", "Shadows"},
		{"qualitySettings.softParticles", "Soft Particles"},
	};
	
	public static readonly Dictionary<string, string> XRSettings = new Dictionary<string, string> {
		{"xrSettings.useOcclusionMesh", "Use Occlusion Mesh"},
	};
}

}
