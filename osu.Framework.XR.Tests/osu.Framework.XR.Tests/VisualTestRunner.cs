using osu.Framework;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;
using System;

namespace osu.Framework.XR.Tests {
	public static class VisualTestRunner {
		[STAThread]
		public static int Main ( string[] args ) {
			using ( DesktopGameHost host = Host.GetSuitableHost( @"osu", true ) ) {
				var browser = new OsuXrTestBrowser();
				host.Run( browser );
				return 0;
			}
		}
	}
}
