using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Framework.XR.Parsing.WaveFront {
	public class OBJObject {
		public OBJObject ( OBJData source, string name = "Untitled Object" ) {
			Source = source;
			Name = name;
		}

		public readonly OBJData Source;
		private readonly HashSet<OBJGroup> groups = new();
		public IEnumerable<OBJGroup> Groups => groups;
		public readonly string Name;

		public readonly List<uint> Faces = new();
		public readonly List<uint> Lines = new();
		public readonly List<uint> Points = new();

		public void AddToGroup ( OBJGroup group ) {
			if ( groups.Contains( group ) ) return;

			groups.Add( group );
			group.Add( this );
		}

		public void RemoveFromGroup ( OBJGroup group ) {
			if ( groups.Remove( group ) )
				group.Remove( this );
		}
	}
}
