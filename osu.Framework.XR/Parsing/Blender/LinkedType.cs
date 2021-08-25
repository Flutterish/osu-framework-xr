using Newtonsoft.Json.Linq;
using osu.Framework.XR.Parsing.Blender.FileBlocks;
using System.Collections.Generic;
using System.Linq;
using Type = osu.Framework.XR.Parsing.Blender.FileBlocks.Type;

namespace osu.Framework.XR.Parsing.Blender {
	public class LinkedType {
		public readonly BlendFile File;
		public readonly Type DNA;
		public readonly JToken Data;

		public LinkedType ( BlendFile file, Type dna, JToken data ) {
			File = file;
			DNA = dna;
			Data = data;
		}

		public object? Value => ( Data is JValue value ) ? value.Value : Data.ToString();

		public override string ToString ()
			=> $"{Value}";
	}

	public class LinkedStructure : LinkedType {
		new public Structure DNA => (Structure)base.DNA;

		public LinkedStructure ( BlendFile file, Structure dna, JToken data ) : base( file, dna, data ) { }

		private Dictionary<Field, LinkedType?>? fields;
		public Dictionary<Field, LinkedType?> Fields {
			get {
				if ( fields is null ) {
					fields = new();
					foreach ( var i in DNA.Fields ) {
						if ( i.Value.IsFunction ) {
							fields.Add( i.Value, null );
						}
						else if ( i.Value.IsPointer ) {
							ulong address = Data[ i.Key ]!.ToObject<ulong>();
							if ( address != 0 && File.MemoryMap.TryGetValue( address, out var field ) ) {
								fields.Add( i.Value, field );
							}
							else {
								fields.Add( i.Value, null );
							}
						}
						else if ( i.Value.IsArray ) {
							if ( i.Value.IsPointerArray ) {
								fields.Add( i.Value, null );
							}
							else {
								if ( i.Value.Type.Name == "char" ) {
									fields.Add( i.Value, new LinkedType( File, i.Value.Type, JToken.FromObject( Data[ i.Key ]!.ToString() ) ) );
								}
								else {
									fields.Add( i.Value, new LinkedArray( File, i.Value.Type, (JArray)Data[ i.Key ]! ) );
								}
							}
						}
						else {
							fields.Add( i.Value, 
								Data[ i.Key ] is JArray ja
								? new LinkedArray( File, i.Value.Type, ja )
								: i.Value.Type is Structure st
								? new LinkedStructure( File, st, Data[ i.Key ]! )
								: new LinkedType( File, i.Value.Type, Data[ i.Key ]! )
							);
						}
					}
				}

				return fields;
			}
		}

		public override string ToString ()
			=> $"{DNA.Name} {{ {string.Join( ", ", Fields.Select( x => $"{x.Key} = {(x.Key.IsPointer ? (x.Value is null ? "NULL" : "&") : x.Value)}" ) )} }}";
	}

	public class LinkedArray : LinkedType {
		new public JArray Data => (JArray)base.Data;

		public LinkedArray ( BlendFile file, Type dna, JArray data ) : base( file, dna, data ) { }

		private List<LinkedType?>? items;
		public List<LinkedType?> Items {
			get {
				if ( items is null ) {
					items = new();

					foreach ( var i in Data ) {
						items.Add(
							i is JArray ja
							? new LinkedArray( File, DNA, ja )
							: DNA is Structure st
							? new LinkedStructure( File, st, i )
							: new LinkedType( File, DNA, i )
						);
					}
				}
				 
				return items;
			}
		}

		public override string ToString ()
			=> $"{DNA.Name} [ {string.Join( ", ", Items )} ]";
	}
}
