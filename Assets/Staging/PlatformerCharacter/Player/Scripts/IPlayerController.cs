using PlatformerCharacter.InputSystem;
using UnityEngine;

namespace PlatformerCharacter.Player
{
	public interface IPlayerController
	{
		public Vector3 Velocity { get; }
		public FrameInput Input { get; }
		public bool JumpingThisFrame { get; }
		public bool LandingThisFrame { get; }
		public Vector3 RawMovement { get; }
		public bool Grounded { get; }
	}

	public interface IExtendedPlayerController : IPlayerController
	{
		public bool DoubleJumpingThisFrame { get; set; }
		public bool Dashing { get; set; }
	}
}