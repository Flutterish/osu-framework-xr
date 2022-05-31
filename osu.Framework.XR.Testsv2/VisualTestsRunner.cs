using osu.Framework;
using osu.Framework.Platform;
using osu.Framework.XR.Testing;

using DesktopGameHost host = Host.GetSuitableDesktopHost( @"osu" );
var browser = new TestBrowser();
host.Run( browser );