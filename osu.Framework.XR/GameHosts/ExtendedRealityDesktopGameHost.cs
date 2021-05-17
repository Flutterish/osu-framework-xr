using osu.Framework.Input.Handlers;
using osu.Framework.Platform;
using osuTK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace osu.Framework.XR.GameHosts {
	public abstract class ExtendedRealityDesktopGameHost : ExtendedRealityGameHost {
		private TcpIpcProvider? ipcProvider;
		private readonly bool bindIPCPort;
		private Thread? ipcThread;

		internal bool UseOsuTK { get; }

		protected ExtendedRealityDesktopGameHost ( string gameName = @"", bool bindIPCPort = false, bool portableInstallation = false, bool useOsuTK = false )
			: base( gameName ) 
		{
			this.bindIPCPort = bindIPCPort;
			IsPortableInstallation = portableInstallation;
			UseOsuTK = useOsuTK;
		}

		protected sealed override Storage GetDefaultGameStorage () {
			if ( IsPortableInstallation || File.Exists( Path.Combine( RuntimeInfo.StartupDirectory, @"framework.ini" ) ) )
				return GetStorage( RuntimeInfo.StartupDirectory );

			return base.GetDefaultGameStorage();
		}

		public sealed override Storage GetStorage ( string path ) => new NativeStorage( path, this );

		protected override void SetupForRun () {
			if ( bindIPCPort )
				startIPC();

			base.SetupForRun();
		}

		private void startIPC () {
			Debug.Assert( ipcProvider == null );

			ipcProvider = new TcpIpcProvider();
			IsPrimaryInstance = ipcProvider.Bind();

			if ( IsPrimaryInstance ) {
				ipcProvider.MessageReceived += OnMessageReceived;

				ipcThread = new Thread( () => ipcProvider.StartAsync().Wait() ) {
					Name = "IPC",
					IsBackground = true
				};

				ipcThread.Start();
			}
		}

		public bool IsPortableInstallation { get; }

		private void openUsingShellExecute ( string path ) => Process.Start( new ProcessStartInfo {
			FileName = path,
			UseShellExecute = true //see https://github.com/dotnet/corefx/issues/10361
		} );

		protected override IEnumerable<InputHandler> CreateAvailableInputHandlers ()
			=> Array.Empty<InputHandler>();

		public override Task SendMessageAsync ( IpcMessage message ) => (ipcProvider ?? throw new InvalidOperationException( $"Tried to send a message before starting IPC" )).SendMessageAsync( message );

		protected override void Dispose ( bool isDisposing ) {
			ipcProvider?.Dispose();
			ipcThread?.Join( 50 );
			base.Dispose( isDisposing );
		}
	}
}
