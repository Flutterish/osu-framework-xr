using osu.Framework.Platform;
using osu.Framework.XR.Testing;
using System;

namespace osu.Framework.XR.Tests {
	public static class VisualTestRunner {
		[STAThread]
		public static int Main ( string[] args ) {
			using ( DesktopGameHost host = Host.GetSuitableHost( @"osu", true ) ) {
				var browser = new TestBrowser3D();
				host.Run( browser );
				return 0;
			}
		}
	}
}
