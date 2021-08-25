namespace osu.Framework.XR.Parsing {
	/// <summary>
	/// A file or data structure which can be parsed into a collection of models along with their shaders, textures, materials, etc.
	/// </summary>
	public interface IModelFile {
		/// <summary>
		/// Creates a collection of models.
		/// </summary>
		ImportedModelGroup CreateModelGroup ();
	}
}
