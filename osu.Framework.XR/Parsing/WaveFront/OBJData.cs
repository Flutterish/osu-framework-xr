using osu.Framework.Graphics;
using osu.Framework.XR.Parsing.Materials;
using osuTK;
using System.Collections.Generic;
using System.Linq;

namespace osu.Framework.XR.Parsing.WaveFront {
	// NOTE not everything is implmented yet and some structures are in OBJData rather than their respective classes
	public class OBJData {
		public readonly List<Vector4> Vertices = new();
		public readonly List<Colour4?> VerticeColours = new();
		public readonly List<Vector3> TextureCoordinates = new();
		public readonly List<Vector3> VerticeNormals = new();
		public readonly List<Vector3> ParameterSpaceVertices = new();
		public readonly List<uint> Points = new();
		public readonly List<LineData> Lines = new();
		public readonly List<FaceData> Faces = new();
		public readonly List<CurveData> Curves = new();
		public readonly List<CurveV2Data> V2Curves = new();
		public readonly List<SurfaceData> Surfaces = new();
		public readonly List<CurveSurfaceData> CurveSurfaces = new();
		public readonly List<uint> DegreesU = new();
		public readonly List<uint?> DegreesV = new();
		public readonly List<uint> StepsU = new();
		public readonly List<uint?> StepsV = new();
		public readonly List<float[]> MatricesU = new();
		public readonly List<float[]> MatricesV = new();
		public readonly List<float[]> ParametersU = new();
		public readonly List<float[]> ParametersV = new();
		public readonly List<CurveParameterData[]> Trims = new();
		public readonly List<CurveParameterData[]> Holes = new();
		public readonly List<CurveParameterData[]> SpecialCurves = new();
		public readonly List<MTLFileReference> MTLFiles = new();
		public readonly List<uint[]> SpecialPoints = new();
		public readonly List<ConnectivityData> Connections = new();
		public readonly List<string> Materials = new List<string>();

		public MTLMaterial? FetchMaterial ( int index )
			=> index >= 0 && index < Materials.Count ? FetchMaterial( Materials[ index ] ) : null;
		public MTLMaterial? FetchMaterial ( string name ) {
			foreach ( var i in MTLFiles ) {
				if ( !i.IsLoaded ) continue;

				var match = i.Source.Materials.FirstOrDefault( x => x.Name == name );
				if ( match is not null )
					return match;
			}

			return null;
		}
	}

	public struct FaceData {
		public uint[] Vertices;
		public uint?[] TextureCoordinates;
		public uint?[] Normals;
		public uint Material;

		public FaceData ( uint[] vertices, uint?[] textureCoordinates, uint?[] normals, uint material = 0 ) {
			Vertices = vertices;
			TextureCoordinates = textureCoordinates;
			Normals = normals;
			Material = material;
		}
	}

	public struct LineData {
		public uint[] Vertices;
		public uint?[] TextureCoordinates;

		public LineData ( uint[] vertices, uint?[] textureCoordinates ) {
			Vertices = vertices;
			TextureCoordinates = textureCoordinates;
		}
	}

	public struct CurveData {
		public float StartParam;
		public float EndParam;
		public uint[] Vertices;

		public CurveData ( float startParam, float endParam, uint[] vertices ) {
			StartParam = startParam;
			EndParam = endParam;
			Vertices = vertices;
		}
	}

	public struct CurveV2Data {
		public uint[] ParameterVertices;

		public CurveV2Data ( uint[] parameterVertices ) {
			ParameterVertices = parameterVertices;
		}
	}

	public struct SurfaceData {
		public float Ustart;
		public float Uend;
		public float Vstart;
		public float Vend;

		public uint[] Vertices;
		public uint?[] TextureCoordinates;
		public uint?[] Normals;

		public SurfaceData ( float ustart, float uend, float vstart, float vend, uint[] vertices, uint?[] textureCoordinates, uint?[] normals ) {
			Ustart = ustart;
			Uend = uend;
			Vstart = vstart;
			Vend = vend;
			Vertices = vertices;
			TextureCoordinates = textureCoordinates;
			Normals = normals;
		}
	}

	public struct CurveSurfaceData {
		public CurveType Type;
		public bool IsRational;

		public CurveSurfaceData ( CurveType type, bool isRational ) {
			Type = type;
			IsRational = isRational;
		}
	}

	public struct CurveParameterData {
		public float Start;
		public float End;
		public uint CurveIndex; // NOTE not not transformed

		public CurveParameterData ( float start, float end, uint curveIndex ) {
			Start = start;
			End = end;
			CurveIndex = curveIndex;
		}
	}

	public struct ConnectivityData {
		public uint Surf1; // NOTE not not transformed
		public float Param01;
		public float Param11;
		public uint Curv1; // NOTE not not transformed

		public uint Surf2; // NOTE not not transformed
		public float Param02;
		public float Param12;
		public uint Curv2; // NOTE not not transformed

		public ConnectivityData ( uint surf1, float param01, float param11, uint curv1, uint surf2, float param02, float param12, uint curv2 ) {
			Surf1 = surf1;
			Param01 = param01;
			Param11 = param11;
			Curv1 = curv1;
			Surf2 = surf2;
			Param02 = param02;
			Param12 = param12;
			Curv2 = curv2;
		}
	}
}
