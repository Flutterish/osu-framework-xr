using osu.Framework.Bindables;
using osu.Framework.Caching;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.XR.Allocation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace osu.Framework.XR.Graphics.Containers {
	public class AdvancedSearchContainer<Tterm> : AdvancedSearchContainer<Drawable,Tterm> { }
	/// <summary>
	/// A container that filters its children based on several criteria.
	/// As opposed to a regular search container, this one can filter recursively.
	/// </summary>
	public class AdvancedSearchContainer<Tchild,Tterm> : FillFlowContainer<Tchild> where Tchild : Drawable {
		readonly BindableWithCurrent<string> searchTerm = new();
		public Bindable<string> Current {
			get => searchTerm;
			set => searchTerm.Current = value;
		}
		public string SearchTerm {
			get => searchTerm.Value;
			set => searchTerm.Value = value;
		}

		protected override void AddInternal ( Drawable drawable ) {
			base.AddInternal( drawable );
			isFilterValid.Invalidate();
		}

		public RecursiveFilterMode RecursionMode { get; init; }

		Cached isFilterValid = new();

		public delegate IEnumerable<Tterm> SearchFunc ( string term, out string newTerm );
		List<SearchFunc> filterFunctions = new();
		public IEnumerable<SearchFunc> FilterFunctions {
			get => filterFunctions;
			set {
				filterFunctions.Clear();
				filterFunctions.AddRange( value );
				isFilterValid.Invalidate();
			}
		}
		public void AddSubstitutingFilterFunction ( string term, Tterm value ) {
			IEnumerable<Tterm> searchFunc ( string a, out string newTerm ) {
				if ( a.Contains( term ) ) {
					newTerm = a.Replace( term, "" );
					return new[] { value };
				}
				else {
					newTerm = a;
					return Array.Empty<Tterm>();
				}
			}

			filterFunctions.Add( searchFunc );
			isFilterValid.Invalidate();
		}

		public AdvancedSearchContainer () {
			Current.ValueChanged += _ => isFilterValid.Invalidate();
		}

		protected override void Update () {
			base.Update();
			if ( !isFilterValid.IsValid ) {
				PerformFilter();
			}
		}

		(IEnumerable<string> stringTerms, IEnumerable<Tterm> terms, bool searchActive) getTerms () {
			var term = searchTerm.Value ?? string.Empty;
			using var terms = ListPool<Tterm>.Shared.Rent();
			foreach ( var filter in filterFunctions ) {
				terms.AddRange( filter( term, out var nextTerm ) );
				term = nextTerm ?? string.Empty;
			}
			var stringTerms = term.Split( new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries );
			var searchActive = stringTerms.Any() || terms.Any();

			return ( stringTerms, terms.ToArray(), searchActive );
		}

		public void PerformFilter () {
			var (stringTerms, terms, searchActive) = getTerms();

			foreach ( var i in Children.OfType<IFilterable>() ) {
				filter( i, stringTerms, terms, searchActive );
			}

			isFilterValid.Validate();
		}

		bool filter ( IFilterable filterable, IEnumerable<string> stringTerms, IEnumerable<Tterm> terms, bool searchActive ) {
			if ( RecursionMode == RecursiveFilterMode.ParentFirst ) {
				filterable.FilteringActive = searchActive;
				return filterable.MatchingFilter = anyMatches( filterable, stringTerms, terms );
			}
			else if ( RecursionMode == RecursiveFilterMode.ChildrenFirst ) {
				bool isMatch = matches( filterable, stringTerms, terms );

				if ( filterable is IHasFilterableChildren fc ) {
					foreach ( var i in fc.FilterableChildren ) {
						if ( filter( i, stringTerms, terms, searchActive ) ) {
							isMatch = true;
						}
					}
				}

				filterable.FilteringActive = searchActive;
				return filterable.MatchingFilter = isMatch;
			}
			else {
				filterable.FilteringActive = searchActive;
				return filterable.MatchingFilter = matches( filterable, stringTerms, terms );
			}
		}

		bool matches ( IFilterable filterable, IEnumerable<string> stringTerms, IEnumerable<Tterm> terms ) {
			if ( filterable is IFilterable<Tterm> a ) a.SetChangeNotificationTarget( filterSingle );

			return
				( !stringTerms.Any() || stringTerms.All( x => filterable.FilterTerms.Any( y => y.Contains( x, StringComparison.InvariantCultureIgnoreCase ) ) ) )
				&& ( ( filterable is not IFilterable<Tterm> ff || !terms.Any() ) || ff.MatchesCustomTerms( terms ) );

		}

		bool anyMatches ( IFilterable filterable, IEnumerable<string> stringTerms, IEnumerable<Tterm> terms ) {
			if ( filterable is IFilterable<Tterm> a ) a.SetChangeNotificationTarget( filterSingle );

			return matches( filterable, stringTerms, terms )
				|| ( filterable is IHasFilterableChildren fc && fc.FilterableChildren.Any( x => anyMatches( x, stringTerms, terms ) ) );
		}

		void filterSingle ( IFilterable filterable ) {
			var (stringTerms, terms, searchActive) = getTerms();

			filter( filterable, stringTerms, terms, searchActive );
		}
	}

	public enum RecursiveFilterMode {
		/// <summary>
		/// No recursion.
		/// </summary>
		Disabled,
		/// <summary>
		/// Recursively match children, dont hide children if the parent matches.
		/// </summary>
		ParentFirst,
		/// <summary>
		/// Recursively match children, hide children and the parent only if all childen are hidden.
		/// </summary>
		ChildrenFirst
	}

	public interface IFilterable<in Tterm> : IFilterable {
		bool MatchesCustomTerms ( IEnumerable<Tterm> terms );
		void SetChangeNotificationTarget ( Action<IFilterable> onChangeHandler );
	}
}
