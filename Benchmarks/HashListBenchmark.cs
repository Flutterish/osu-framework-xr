using BenchmarkDotNet.Attributes;
using osu.Framework.XR.Collections;
using System.Runtime.InteropServices;

namespace Benchmarks;

public class HashListBenchmark {
	HashSet<int> hashSet = new();
	List<int> list = new();
	HashList<int> hashList = new();

	[GlobalSetup]
	public void Setup () {
		for ( int i = 0; i < 10_000_000; i++ ) {
			hashSet.Add( i );
			list.Add( i );
			hashList.Add( i );
		}
	}

	[Benchmark]
	public void List () {
		foreach ( var i in list ) { }
	}

	[Benchmark]
	public void HashSet () {
		foreach ( var i in hashSet ) { }
	}

	[Benchmark]
	public void HashList () {
		foreach ( var i in hashList ) { }
	}

	[Benchmark]
	public void ListSpan () {
		foreach ( var i in CollectionsMarshal.AsSpan( list ) ) { }
	}

	[Benchmark]
	public void HashListSpan () {
		foreach ( var i in hashList.AsSpan() ) { }
	}

	[Benchmark]
	public void ListFor () {
		for ( int i = 0; i < list.Count; i++ ) { var k = list[i]; }
	}

	[Benchmark]
	public void HashListFor () {
		for ( int i = 0; i < hashList.Count; i++ ) { var k = hashList[i]; }
	}

	[Benchmark]
	public void ListSpanFor () {
		var span = CollectionsMarshal.AsSpan(list);
		for ( int i = 0; i < span.Length; i++ ) { var k = span[i]; }
	}

	[Benchmark]
	public void HashListSpanFor () {
		var span = hashList.AsSpan();
		for ( int i = 0; i < span.Length; i++ ) { var k = span[i]; }
	}
}
