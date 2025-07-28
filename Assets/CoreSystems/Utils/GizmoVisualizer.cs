using UnityEngine;

namespace Utils
{
	[ExecuteInEditMode]
	public class GizmoVisualizer : MonoBehaviour
	{
		[SerializeField] private string label;
		[SerializeField] private bool displayGizmo = true;
		[SerializeField] private Color gizmoColor = Color.white;
		[SerializeField] private Vector2 center;
		[SerializeField] private float radius = 1f;
		[SerializeField] private float arcAngle;
		[SerializeField] private float startAngle;

		private void OnDrawGizmos()
		{
			if (!displayGizmo) return;

			Gizmos.color = gizmoColor;
			if (arcAngle == 0f)
				CustomGizmos.DrawCircle(center, radius);
			else
				CustomGizmos.DrawArc(transform.position, radius, arcAngle, startAngle);
		}
	}
}