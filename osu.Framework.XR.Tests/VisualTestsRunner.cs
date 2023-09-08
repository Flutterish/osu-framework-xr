using osu.Framework.Platform;
using osu.Framework.XR;
using osu.Framework.XR.Testing;

using DesktopGameHost host = HostXR.GetSuitableDesktopHost( @"osu" );
var browser = new TestBrowser();
host.Run( browser );