using OpenVR.NET.Devices;
using OpenVR.NET.Input;
using osu.Framework.XR.Graphics;
using osu.Framework.XR.Maths;

namespace osu.Framework.XR.VirtualReality;

public class BasicHandSkeleton : BasicModel {
	HandSkeletonAction? source;
	Controller controller;
	public BasicHandSkeleton ( Controller controller, Enum name ) {
		this.controller = controller;
		controller.VR.BindActionsLoaded( () => {
			source = controller.GetAction<HandSkeletonAction>( name );
		} );
	}

	protected override void Update () {
		if ( source is null || !source.FetchData() ) {
			if ( Mesh.Indices.Any() ) {
				Mesh.Clear();
				Mesh.CreateFullUpload().Enqueue();
			}
			return;
		}

		Position = controller.Position.ToOsuTk();
		Rotation = controller.Rotation.ToOsuTk();
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
