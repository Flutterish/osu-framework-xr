using osu.Framework.XR.Graphics;
using osu.Framework.XR.Graphics.Meshes;
using osu.Framework.XR.Maths;
using osuTK.Graphics;

namespace osu.Framework.XR.Tests.Parsing;

public partial class TestSceneObjCollectionImport : BasicTestScene {
	protected override void LoadComplete () {
		base.LoadComplete();
		var kb = Scene.MeshStore.GetCollection( "keyboard" );
		foreach ( var i in kb.AllObjects ) {
			var mesh = i.MeshParts[0].Mesh.Mesh;
			Scene.Add( new Model {
				Mesh = mesh
			} );

			if ( mesh is not ITriangleMesh tringular )
				continue;

			if ( tringular.FitFlatMesh() is Box3 box ) {
				Scene.Add( new Model {
					Colour = Color4.Green,
					Alpha = 0.4f,
					Mesh = BasicMesh.UnitCube,
					Origin = new( -0.5f ),
					Position = box.Position,
					Scale = box.Size,
					Rotation = box.Rotation
				} );
			}
			else {
				Scene.Add( new Model {
					Colour = Color4.Red,
					Alpha = 0.4f,
					Mesh = BasicMesh.UnitCube,
					Origin = new( -0.5f ),
					Position = tringular.BoundingBox.Min,
					Scale = tringular.BoundingBox.Size
				} );
			}
		}
	}
}
