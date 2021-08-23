using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Framework.XR.Parsing.WaveFront {
	public class OBJGroup {
		public OBJGroup ( string name = "Untitled Group", GroupType type = GroupType.Regular ) {
			Name = name;
			Type = type;
		}

		public readonly string Name;
		public readonly GroupType Type;
		private readonly HashSet<OBJObject> objects = new();
		public IEnumerable<OBJObject> Objects => objects;

		public void Add ( OBJObject obj ) {
			if ( objects.Contains( obj ) ) return;

			objects.Add( obj );
			obj.AddToGroup( this );
		}

		public void Remove ( OBJObject obj ) {
			if ( objects.Remove( obj ) )
				obj.RemoveFromGroup( this );
		}
	}

	public class MergingGroup : OBJGroup {
		public MergingGroup ( string name = "Untitled Group", float resolution ) : base( name, GroupType.Merging ) {
			Resolution = resolution;
		}

		public readonly float Resolution;
	}

	public enum GroupType {
		Regular,
		Smoothing,
		Merging
	}
}
