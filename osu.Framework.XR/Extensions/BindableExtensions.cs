using osu.Framework.Bindables;
using System;

namespace osu.Framework.XR.Extensions {
	public static class BindableExtensions {
		public delegate void UnbindAction ();

		public static UnbindAction BindValuesChanged<Ta, Tb> (
			this (Bindable<Ta> a, Bindable<Tb> b) self,
			Action<Ta, Tb> handler,
			bool runOnceImmediately = false
		) {
			Action<ValueChangedEvent<Ta>> handlerA = _ => handler( self.a.Value, self.b.Value );
			Action<ValueChangedEvent<Tb>> handlerB = _ => handler( self.a.Value, self.b.Value );

			self.a.BindValueChanged( handlerA );
			self.b.BindValueChanged( handlerB );

			if ( runOnceImmediately ) {
				handler( self.a.Value, self.b.Value );
			}

			return () => {
				self.a.ValueChanged -= handlerA;
				self.b.ValueChanged -= handlerB;
			};
		}
		public static UnbindAction BindValuesChanged<Ta, Tb> (
			this (Bindable<Ta> a, Bindable<Tb> b) self,
			Action handler,
			bool runOnceImmediately = false
		) {
			Action<ValueChangedEvent<Ta>> handlerA = _ => handler();
			Action<ValueChangedEvent<Tb>> handlerB = _ => handler();

			self.a.BindValueChanged( handlerA );
			self.b.BindValueChanged( handlerB );

			if ( runOnceImmediately ) {
				handler();
			}

			return () => {
				self.a.ValueChanged -= handlerA;
				self.b.ValueChanged -= handlerB;
			};
		}

		public static UnbindAction BindValuesChanged<Ta, Tb, Tc> (
			this (Bindable<Ta> a, Bindable<Tb> b, Bindable<Tc> c) self,
			Action<Ta, Tb, Tc> handler,
			bool runOnceImmediately = false
		) {
			Action<ValueChangedEvent<Ta>> handlerA = _ => handler( self.a.Value, self.b.Value, self.c.Value );
			Action<ValueChangedEvent<Tb>> handlerB = _ => handler( self.a.Value, self.b.Value, self.c.Value );
			Action<ValueChangedEvent<Tc>> handlerC = _ => handler( self.a.Value, self.b.Value, self.c.Value );

			self.a.BindValueChanged( handlerA );
			self.b.BindValueChanged( handlerB );
			self.c.BindValueChanged( handlerC );

			if ( runOnceImmediately ) {
				handler( self.a.Value, self.b.Value, self.c.Value );
			}

			return () => {
				self.a.ValueChanged -= handlerA;
				self.b.ValueChanged -= handlerB;
				self.c.ValueChanged -= handlerC;
			};
		}
		public static UnbindAction BindValuesChanged<Ta, Tb, Tc> (
			this (Bindable<Ta> a, Bindable<Tb> b, Bindable<Tc> c) self,
			Action handler,
			bool runOnceImmediately = false
		) {
			Action<ValueChangedEvent<Ta>> handlerA = _ => handler();
			Action<ValueChangedEvent<Tb>> handlerB = _ => handler();
			Action<ValueChangedEvent<Tc>> handlerC = _ => handler();

			self.a.BindValueChanged( handlerA );
			self.b.BindValueChanged( handlerB );
			self.c.BindValueChanged( handlerC );

			if ( runOnceImmediately ) {
				handler();
			}

			return () => {
				self.a.ValueChanged -= handlerA;
				self.b.ValueChanged -= handlerB;
				self.c.ValueChanged -= handlerC;
			};
		}

		public static UnbindAction BindValuesChanged<Ta, Tb, Tc, Td> (
			this (Bindable<Ta> a, Bindable<Tb> b, Bindable<Tc> c, Bindable<Td> d) self,
			Action<Ta, Tb, Tc, Td> handler,
			bool runOnceImmediately = false
		) {
			Action<ValueChangedEvent<Ta>> handlerA = _ => handler( self.a.Value, self.b.Value, self.c.Value, self.d.Value );
			Action<ValueChangedEvent<Tb>> handlerB = _ => handler( self.a.Value, self.b.Value, self.c.Value, self.d.Value );
			Action<ValueChangedEvent<Tc>> handlerC = _ => handler( self.a.Value, self.b.Value, self.c.Value, self.d.Value );
			Action<ValueChangedEvent<Td>> handlerD = _ => handler( self.a.Value, self.b.Value, self.c.Value, self.d.Value );

			self.a.BindValueChanged( handlerA );
			self.b.BindValueChanged( handlerB );
			self.c.BindValueChanged( handlerC );
			self.d.BindValueChanged( handlerD );

			if ( runOnceImmediately ) {
				handler( self.a.Value, self.b.Value, self.c.Value, self.d.Value );
			}

			return () => {
				self.a.ValueChanged -= handlerA;
				self.b.ValueChanged -= handlerB;
				self.c.ValueChanged -= handlerC;
				self.d.ValueChanged -= handlerD;
			};
		}
		public static UnbindAction BindValuesChanged<Ta, Tb, Tc, Td> (
			this (Bindable<Ta> a, Bindable<Tb> b, Bindable<Tc> c, Bindable<Td> d) self,
			Action handler,
			bool runOnceImmediately = false
		) {
			Action<ValueChangedEvent<Ta>> handlerA = _ => handler();
			Action<ValueChangedEvent<Tb>> handlerB = _ => handler();
			Action<ValueChangedEvent<Tc>> handlerC = _ => handler();
			Action<ValueChangedEvent<Td>> handlerD = _ => handler();

			self.a.BindValueChanged( handlerA );
			self.b.BindValueChanged( handlerB );
			self.c.BindValueChanged( handlerC );
			self.d.BindValueChanged( handlerD );

			if ( runOnceImmediately ) {
				handler();
			}

			return () => {
				self.a.ValueChanged -= handlerA;
				self.b.ValueChanged -= handlerB;
				self.c.ValueChanged -= handlerC;
				self.d.ValueChanged -= handlerD;
			};
		}

		public static UnbindAction BindValuesChanged<Ta, Tb, Tc, Td, Te> (
			this (Bindable<Ta> a, Bindable<Tb> b, Bindable<Tc> c, Bindable<Td> d, Bindable<Te> e) self,
			Action<Ta, Tb, Tc, Td, Te> handler,
			bool runOnceImmediately = false
		) {
			Action<ValueChangedEvent<Ta>> handlerA = _ => handler( self.a.Value, self.b.Value, self.c.Value, self.d.Value, self.e.Value );
			Action<ValueChangedEvent<Tb>> handlerB = _ => handler( self.a.Value, self.b.Value, self.c.Value, self.d.Value, self.e.Value );
			Action<ValueChangedEvent<Tc>> handlerC = _ => handler( self.a.Value, self.b.Value, self.c.Value, self.d.Value, self.e.Value );
			Action<ValueChangedEvent<Td>> handlerD = _ => handler( self.a.Value, self.b.Value, self.c.Value, self.d.Value, self.e.Value );
			Action<ValueChangedEvent<Te>> handlerE = _ => handler( self.a.Value, self.b.Value, self.c.Value, self.d.Value, self.e.Value );

			self.a.BindValueChanged( handlerA );
			self.b.BindValueChanged( handlerB );
			self.c.BindValueChanged( handlerC );
			self.d.BindValueChanged( handlerD );
			self.e.BindValueChanged( handlerE );

			if ( runOnceImmediately ) {
				handler( self.a.Value, self.b.Value, self.c.Value, self.d.Value, self.e.Value );
			}

			return () => {
				self.a.ValueChanged -= handlerA;
				self.b.ValueChanged -= handlerB;
				self.c.ValueChanged -= handlerC;
				self.d.ValueChanged -= handlerD;
				self.e.ValueChanged -= handlerE;
			};
		}
		public static UnbindAction BindValuesChanged<Ta, Tb, Tc, Td, Te> (
			this (Bindable<Ta> a, Bindable<Tb> b, Bindable<Tc> c, Bindable<Td> d, Bindable<Te> e) self,
			Action handler,
			bool runOnceImmediately = false
		) {
			Action<ValueChangedEvent<Ta>> handlerA = _ => handler();
			Action<ValueChangedEvent<Tb>> handlerB = _ => handler();
			Action<ValueChangedEvent<Tc>> handlerC = _ => handler();
			Action<ValueChangedEvent<Td>> handlerD = _ => handler();
			Action<ValueChangedEvent<Te>> handlerE = _ => handler();

			self.a.BindValueChanged( handlerA );
			self.b.BindValueChanged( handlerB );
			self.c.BindValueChanged( handlerC );
			self.d.BindValueChanged( handlerD );
			self.e.BindValueChanged( handlerE );

			if ( runOnceImmediately ) {
				handler();
			}

			return () => {
				self.a.ValueChanged -= handlerA;
				self.b.ValueChanged -= handlerB;
				self.c.ValueChanged -= handlerC;
				self.d.ValueChanged -= handlerD;
				self.e.ValueChanged -= handlerE;
			};
		}
	}
}
