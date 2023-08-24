﻿using osu.Framework.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Framework.XR;

/// <summary>
/// Host type for o!f-xr apps that guarantees the legacy opengl renderer is used
/// </summary>
public static class HostXR { // FrameworkEnvironment
	public static DesktopGameHost GetSuitableDesktopHost ( string name, HostOptions? options = null ) {
		Environment.SetEnvironmentVariable( "OSU_GRAPHICS_SURFACE", GraphicsSurfaceType.OpenGL.ToString(), EnvironmentVariableTarget.Process );
		Environment.SetEnvironmentVariable( "OSU_GRAPHICS_RENDERER", "gl", EnvironmentVariableTarget.Process );

		return Host.GetSuitableDesktopHost( name, options );
	}
}
