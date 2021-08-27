using osu.Framework.Graphics;
using osu.Framework.XR.Extensions;
using osu.Framework.XR.Testing;
using osu.Framework.XR.Testing.Components;
using osuTK;
using System;

namespace osu.Framework.XR.Tests.Physics.Lines {
	public class TestSceneClosestPoint : TestScene3D {
		LineIndicator line;
		PointIndicator point;

		PointIndicator closest;
		DashedLineVisual dash;

		private bool approx ( float a, float b, float delta = 0.0001f )
			=> MathF.Abs( a - b ) < delta;

		public TestSceneClosestPoint () {
			Add( line = new LineIndicator( Scene ) { Colour = Colour4.Blue, Tint = Colour4.Cyan } );
			Add( point = new PointIndicator( Scene ) { Colour = Colour4.Red } );
			Add( closest = new PointIndicator( Scene ) { Colour = Colour4.Violet, AllowDragging = false } );
			Add( dash = new DashedLineVisual( Scene ) { Colour = Colour4.Violet } );

			line.PointA.Value = new Vector3( -1, 0, 0 );
			line.PointB.Value = new Vector3( 1, 0, 0 );

			point.Current.Value = new Vector3( 0, 1, 1 );

			dash.PointA = closest.Current;
			dash.PointB = point.Current;

			(line.PointA, line.PointB, point.Current).BindValuesChanged( (a,b,c) => {
				closest.Current.Value = XR.Physics.Raycast.ClosestPoint( a, b, c );
			}, true );


			AddStep( "Position along Y axis", () => {
				line.PointA.Value = new Vector3( 0, -0.5f, 0 );
				line.PointB.Value = new Vector3( 0, 0.5f, 0 );
			} );

			AddAssert( "Evaluate along Y axis", () => {
				for ( int i = 0; i < 100; i++ ) {
					point.Current.Value = StatelessRandom.RandomVector( i );
					if ( !approx( closest.Current.Value.Y, Math.Clamp( point.Current.Value.Y, -0.5f, 0.5f ) ) )
						return false;
				}

				return true;
			} );

			AddStep( "Position along X axis", () => {
				line.PointA.Value = new Vector3( -0.5f, 0, 0 );
				line.PointB.Value = new Vector3( 0.5f, 0, 0 );
			} );

			AddAssert( "Evaluate along X axis", () => {
				for ( int i = 0; i < 100; i++ ) {
					point.Current.Value = StatelessRandom.RandomVector( i );
					if ( !approx( closest.Current.Value.X, Math.Clamp( point.Current.Value.X, -0.5f, 0.5f ) ) )
						return false;
				}

				return true;
			} );

			AddStep( "Position along Z axis", () => {
				line.PointA.Value = new Vector3( 0, 0, -0.5f );
				line.PointB.Value = new Vector3( 0, 0, 0.5f );
			} );

			AddAssert( "Evaluate along Z axis", () => {
				for ( int i = 0; i < 100; i++ ) {
					point.Current.Value = StatelessRandom.RandomVector( i );
					if ( !approx( closest.Current.Value.Z, Math.Clamp( point.Current.Value.Z, -0.5f, 0.5f ) ) )
						return false;
				}

				return true;
			} );

			AddStep( "Position on diagonal", () => {
				line.PointA.Value = new Vector3( -0.5f, -0.5f, -0.5f );
				line.PointB.Value = new Vector3( 0.5f, 0.5f, 0.5f );
			} );

			AddAssert( "Evaluate along diagonal", () => {
				for ( int i = 0; i < 100; i++ ) {
					point.Current.Value = ( new Vector3( i, i, i ) / 50 ) - Vector3.One;
					if ( !approx( closest.Current.Value.X, closest.Current.Value.Y ) 
						|| !approx( closest.Current.Value.Y, closest.Current.Value.Z )
						|| !approx( closest.Current.Value.Z, Math.Clamp( point.Current.Value.Z, -0.5f, 0.5f ) )
					) {
						return false;
					}
				}

				return true;
			} );
		}
	}
}
