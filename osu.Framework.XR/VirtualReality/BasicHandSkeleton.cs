using osu.Framework.XR.Graphics;
using osu.Framework.XR.Maths;
using osu.Framework.XR.VirtualReality.Devices;

namespace osu.Framework.XR.VirtualReality;

public partial class BasicHandSkeleton : BasicModel {
	HandSkeletonAction source = null!;
	Controller controller;
	Enum name;
	public BasicHandSkeleton ( Controller controller, Enum name ) {
		this.controller = controller;
		this.name = name;
	}

	[BackgroundDependencyLoader]
	private void load ( VrCompositor compositor ) {
		source = compositor.Input.GetAction<HandSkeletonAction>( name, controller );
	}

	protected override void Update () {
		if ( source.FetchData() != true ) {
			if ( Mesh.Indices.Any() ) {
				Mesh.Clear();
				Mesh.CreateFullUpload().Enqueue();
			}
			return;
		}

		Position = controller.Position;
		Rotation = controller.Rotation;
		var offset = Vector3.UnitY * 0.0007f;

		Mesh.Clear();
		for ( int i = 2 /*0 is root bone, 1 is wrist*/; i < source.BoneCount - 5 /*there are 5 aux bones*/; i++ ) {
			var bone = source.GetBoneData( i );
			var parent = source.GetBoneData( source.ParentBoneIndex( i ) );

			Mesh.AddQuad( new Quad3 {
				TL = parent.Position.XyzToOsuTk() + offset,
				TR = parent.Position.XyzToOsuTk() - offset,
				BL = bone.Position.XyzToOsuTk() + offset,
				BR = bone.Position.XyzToOsuTk() - offset
			} );
			Mesh.AddCircle( bone.Position.XyzToOsuTk(), bone.Rotation.ToOsuTk().Apply( Vector3.UnitY ), bone.Rotation.ToOsuTk().Apply( Vector3.UnitX * 0.003f ), 32 );
		}
		Mesh.CreateFullUpload().Enqueue();
	}
}
