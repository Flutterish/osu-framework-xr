namespace osu.Framework.XR;

public static class EnumerableExtensions {
	public static Vector3 Average ( this IEnumerable<Vector3> vectors ) {
		int count = 0;
		Vector3 sum = Vector3.Zero;
		foreach ( var i in vectors ) {
			count++;
			sum += i;
		}

		return sum / count;
	}

	public static Vector3 Sum ( this IEnumerable<Vector3> vectors ) {
		Vector3 sum = Vector3.Zero;
		foreach ( var i in vectors ) {
			sum += i;
		}

		return sum;
	}
}
