using osu.Framework.Graphics.Textures;
using osu.Framework.XR.Graphics.Shaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Framework.XR.Graphics.Materials;

public static class UnlitMaterial {
	public const string Position = Material.StandardPositionAttributeName;
	public const string UV = Material.StandardUvAttributeName;
	public const string Texture = Material.StandardTextureName;
	public const string TextureRect = Material.StandardTextureRectName;
	public const string Tint = Material.StandardTintName;
	public const string UseGamma = Material.StandardUseGammaName;
}
