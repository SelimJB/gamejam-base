using UnityEngine;

namespace Utils
{
	public static class CustomGizmos
	{
		public static void DrawCircle(Vector2 center, float radius = 1f, int segments = 36)
		{
			var lastPoint = center + Vector2.right * radius;

			for (var i = 0; i <= segments; i++)
			{
				var angle = i * Mathf.PI * 2f / segments;
				var newPoint = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
				Gizmos.DrawLine(lastPoint, newPoint);
				lastPoint = newPoint;
			}
		}

		public static void DrawArc(Vector3 center, float radius, float arcAngle, float startAngle, int segments = 36)
		{
			var segmentAngle = arcAngle / segments;
			var previousPoint = center + Quaternion.Euler(0, 0, startAngle) * Vector3.right * radius;
			Vector3 newPoint;

			for (var i = 1; i <= segments; i++)
			{
				var currentAngle = startAngle + segmentAngle * i;
				newPoint = center + Quaternion.Euler(0, 0, currentAngle) * Vector3.right * radius;
				Gizmos.DrawLine(previousPoint, newPoint);
				previousPoint = newPoint;
			}
		}

		public static void DrawCross(Vector2 center, float size = 0.5f)
		{
			Gizmos.DrawLine(center + Vector2.left * size, center + Vector2.right * size);
			Gizmos.DrawLine(center + Vector2.up * size, center + Vector2.down * size);
		}
		
		public static void DrawCross(Vector3 center, float size = 0.5f)
		{
			Gizmos.DrawLine(center + Vector3.left * size, center + Vector3.right * size);
			Gizmos.DrawLine(center + Vector3.up * size, center + Vector3.down * size);
		}
	}
}