using osu.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Framework.XR.Testing.Components {
	public enum Kind {
		/// <summary>
		/// White, editable.
		/// </summary>
		Default,

		/// <summary>
		/// Viotet, not editable. Result of a computation.
		/// </summary>
		Result,

		/// <summary>
		/// Blue, editable. Background test element.
		/// </summary>
		Component,

		/// <summary>
		/// Red, editable. Main user control.
		/// </summary>
		Control
	}

	public static class KindExtensions {
		public static bool IsEditable ( this Kind kind ) => kind switch {
			Kind.Default or Kind.Component or Kind.Control => true,
			_ => false
		};

		public static Colour4 MainColour ( this Kind kind ) => kind switch {
			Kind.Result => Colour4.Violet,
			Kind.Component => Colour4.Blue,
			Kind.Control => Colour4.Red,
			Kind.Default or _ => Colour4.White
		};

		public static Colour4 AccentColour ( this Kind kind ) => kind switch {
			Kind.Result => Colour4.PaleVioletRed,
			Kind.Component => Colour4.Cyan,
			Kind.Control => Colour4.Orange,
			Kind.Default or _ => Colour4.Gray
		};

		public static Colour4 SecondaryColour ( this Kind kind ) => kind switch {
			Kind.Result => Colour4.White,
			Kind.Component => Colour4.White,
			Kind.Control => Colour4.White,
			Kind.Default or _ => Colour4.White
		};
	}
}
