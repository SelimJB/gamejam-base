using UnityEngine;

namespace PlatformerCharacter.Player
{
	public class RayRange
	{
		public RayRange(float x1, float y1, float x2, float y2, Vector2 dir)
		{
			Start = new Vector2(x1, y1);
			End = new Vector2(x2, y2);
			Dir = dir;
		}

		public Vector2 Start { get; }
		public Vector2 End { get; }
		public Vector2 Dir { get; }
	}
}