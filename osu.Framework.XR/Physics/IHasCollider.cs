using osu.Framework.XR.Graphics;
using System;

namespace osu.Framework.XR.Physics {
	public interface IHasCollider {
		Mesh Mesh { get; }
		bool IsColliderEnabled { get; }
		PhysicsLayer PhysicsLayer { get; }
	}

	[Flags]
	public enum PhysicsLayer : ulong {
		All     = ( 1uL << 63 ) + ( 1uL << 63 - 1 ),
		None    = 0,
		Layer0  = 1uL << 0 ,
		Layer1  = 1uL << 1 ,
		Layer2  = 1uL << 2 ,
		Layer3  = 1uL << 3 ,
		Layer4  = 1uL << 4 ,
		Layer5  = 1uL << 5 ,
		Layer6  = 1uL << 6 ,
		Layer7  = 1uL << 7 ,
		Layer8  = 1uL << 8 ,
		Layer9  = 1uL << 9 ,
		Layer10 = 1uL << 10,
		Layer11 = 1uL << 11,
		Layer12 = 1uL << 12,
		Layer13 = 1uL << 13,
		Layer14 = 1uL << 14,
		Layer15 = 1uL << 15,
		Layer16 = 1uL << 16,
		Layer17 = 1uL << 17,
		Layer18 = 1uL << 18,
		Layer19 = 1uL << 19,
		Layer20 = 1uL << 20,
		Layer21 = 1uL << 21,
		Layer22 = 1uL << 22,
		Layer23 = 1uL << 23,
		Layer24 = 1uL << 24,
		Layer25 = 1uL << 25,
		Layer26 = 1uL << 26,
		Layer27 = 1uL << 27,
		Layer28 = 1uL << 28,
		Layer29 = 1uL << 29,
		Layer30 = 1uL << 30,
		Layer31 = 1uL << 31,
		Layer32 = 1uL << 32,
		Layer33 = 1uL << 33,
		Layer34 = 1uL << 34,
		Layer35 = 1uL << 35,
		Layer36 = 1uL << 36,
		Layer37 = 1uL << 37,
		Layer38 = 1uL << 38,
		Layer39 = 1uL << 39,
		Layer40 = 1uL << 40,
		Layer41 = 1uL << 41,
		Layer42 = 1uL << 42,
		Layer43 = 1uL << 43,
		Layer44 = 1uL << 44,
		Layer45 = 1uL << 45,
		Layer46 = 1uL << 46,
		Layer47 = 1uL << 47,
		Layer48 = 1uL << 48,
		Layer49 = 1uL << 49,
		Layer50 = 1uL << 50,
		Layer51 = 1uL << 51,
		Layer52 = 1uL << 52,
		Layer53 = 1uL << 53,
		Layer54 = 1uL << 54,
		Layer55 = 1uL << 55,
		Layer56 = 1uL << 56,
		Layer57 = 1uL << 57,
		Layer58 = 1uL << 58,
		Layer59 = 1uL << 59,
		Layer60 = 1uL << 60,
		Layer61 = 1uL << 61,
		Layer62 = 1uL << 62,
		Layer63 = 1uL << 63
	}

	public static class PhysicsLayerExtensions {
		public static PhysicsLayer And ( this PhysicsLayer a, PhysicsLayer b )
			=> a | b;

		public static PhysicsLayer Intersect ( this PhysicsLayer a, PhysicsLayer b )
			=> a & b;

		public static PhysicsLayer Except ( this PhysicsLayer a, PhysicsLayer b )
			=> a & ( ~b );
	}
}
