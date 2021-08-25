namespace osu.Framework.XR.Parsing {
	/// <summary>
	/// Data which can be converted to a Material
	/// </summary>
	public interface IImportedMaterial {
		ImportedMaterial CreateMaterial ();
	}
}
