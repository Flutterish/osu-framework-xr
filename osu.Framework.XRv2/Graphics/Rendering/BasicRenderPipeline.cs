using osu.Framework.XR.Collections;

namespace osu.Framework.XR.Graphics.Rendering;

/// <summary>
/// A basic render pipeline which groups drawables into one stage each
/// </summary>
public class BasicRenderPiepline : Scene.RenderPiepline {
	Dictionary<Enum, HashList<Drawable3D>> renderStages = new();
	protected IEnumerable<Enum> RenderStages => renderStages.Keys;
	protected ReadOnlySpan<Drawable3D> GetRenderStage ( Enum stage ) => renderStages[stage].AsSpan();
	protected bool TryGetRenderStage ( Enum stage, out ReadOnlySpan<Drawable3D> drawables ) {
		if ( renderStages.TryGetValue( stage, out var hashList ) ) {
			drawables = hashList.AsSpan();
			return true;
		}
		drawables = default;
		return false;
	}

	protected override void AddDrawable ( Drawable3D drawable, Enum stage ) {
		if ( !renderStages.TryGetValue( stage, out var set ) )
			renderStages.Add( stage, set = new() );

		set.Add( drawable );
	}
	protected override void RemoveDrawable ( Drawable3D drawable, Enum stage ) {
		if ( !renderStages.TryGetValue( stage, out var set ) )
			renderStages.Add( stage, set = new() );

		set.Remove( drawable );
	}

	public BasicRenderPiepline ( Scene source ) : base( source ) { }

	protected override void Draw ( int subtreeIndex, Matrix4 projectionMatrix ) {
		foreach ( var stage in RenderStages ) {
			foreach ( var i in GetRenderStage( stage ) ) {
				i.GetDrawNodeAtSubtree( subtreeIndex )?.Draw();
			}
		}
	}
}