using System.Collections.Generic;
using System.Linq;
using PlatformerCharacter.InputSystem;
using UnityEngine;

namespace PlatformerCharacter.Player
{
	public class PlayerController : MonoBehaviour, IPlayerController
	{
		[SerializeField] private JoystickController joystickController;

		public Vector3 Velocity { get; private set; }
		public FrameInput Input { get; private set; }
		public bool JumpingThisFrame { get; private set; }
		public bool LandingThisFrame { get; private set; }
		public Vector3 RawMovement { get; private set; }
		public bool Grounded => colDown;

		private Vector3 lastPosition;
		private float currentHorizontalSpeed, currentVerticalSpeed;
		private bool active;

		private void Awake() => Invoke(nameof(Activate), 0.5f);
		private void Activate() => active = true;

		private void Start()
		{
			joystickController.OnInput += OnInput;
		}

		private void OnInput(FrameInput obj)
		{
			Input = obj;
		}

		private bool switchTest;

		private void Update()
		{
			if (!active) return;

			Velocity = (transform.position - lastPosition) / Time.deltaTime;
			lastPosition = transform.position;

			GatherInput();
			RunCollisionChecks();
			CalculateWalk();
			CalculateJumpApex();
			CalculateGravity();
			CalculateJump();
			MoveCharacter();
		}

		#region Gather Input

		private void GatherInput()
		{
			if (Input.JumpDown)
			{
				// TODO: Fix the jump buffer issue
				lastJumpPressed = Time.time;
			}
		}

		#endregion

		#region Collisions

		[Header("COLLISION")]
		[SerializeField] private Bounds characterBounds;
		[SerializeField] private LayerMask groundLayer;
		[SerializeField] private int detectorCount = 3;
		[SerializeField] private float detectionRayLength = 0.1f;
		[SerializeField] [Range(0.1f, 0.3f)] private float rayBuffer = 0.1f; // Prevents side detectors hitting the ground

		private RayRange raysUp, raysRight, raysDown, raysLeft;
		private bool colUp, colRight, colDown, colLeft;
		private float timeLeftGrounded;

		// We use these raycast checks for pre-collision information
		private void RunCollisionChecks()
		{
			CalculateRayRanged();

			LandingThisFrame = false;
			var groundedCheck = RunDetection(raysDown);
			if (colDown && !groundedCheck) timeLeftGrounded = Time.time; // Only trigger when first leaving
			else if (!colDown && groundedCheck)
			{
				coyoteUsable = true; // Only trigger when first touching
				LandingThisFrame = true;
			}

			colDown = groundedCheck;

			colUp = RunDetection(raysUp);
			colLeft = RunDetection(raysLeft);
			colRight = RunDetection(raysRight);

			bool RunDetection(RayRange range)
			{
				return EvaluateRayPositions(range).Any(point => Physics2D.Raycast(point, range.Dir, detectionRayLength, groundLayer));
			}
		}

		private void CalculateRayRanged()
		{
			// TODO refactor 
			var b = new Bounds(transform.position, characterBounds.size);

			raysDown = new RayRange(b.min.x + rayBuffer, b.min.y, b.max.x - rayBuffer, b.min.y, Vector2.down);
			raysUp = new RayRange(b.min.x + rayBuffer, b.max.y, b.max.x - rayBuffer, b.max.y, Vector2.up);
			raysLeft = new RayRange(b.min.x, b.min.y + rayBuffer, b.min.x, b.max.y - rayBuffer, Vector2.left);
			raysRight = new RayRange(b.max.x, b.min.y + rayBuffer, b.max.x, b.max.y - rayBuffer, Vector2.right);
		}

		private IEnumerable<Vector2> EvaluateRayPositions(RayRange range)
		{
			for (var i = 0; i < detectorCount; i++)
			{
				var t = (float)i / (detectorCount - 1);
				yield return Vector2.Lerp(range.Start, range.End, t);
			}
		}

		private void OnDrawGizmos()
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireCube(transform.position + characterBounds.center, characterBounds.size);

			if (!Application.isPlaying)
			{
				CalculateRayRanged();
				Gizmos.color = Color.blue;
				foreach (var range in new List<RayRange> { raysUp, raysRight, raysDown, raysLeft })
				{
					foreach (var point in EvaluateRayPositions(range))
					{
						Gizmos.DrawRay(point, range.Dir * detectionRayLength);
					}
				}
			}

			if (!Application.isPlaying) return;

			Gizmos.color = Color.red;
			var move = new Vector3(currentHorizontalSpeed, currentVerticalSpeed) * Time.deltaTime;
			Gizmos.DrawWireCube(transform.position + move, characterBounds.size);
		}

		#endregion

		#region Walk

		[Header("WALKING")]
		[SerializeField] private float acceleration = 90;
		[SerializeField] private float moveClamp = 13;
		[SerializeField] private float deAcceleration = 60f;
		[SerializeField] private float apexBonus = 2;

		private void CalculateWalk()
		{
			if (Input.X != 0)
			{
				// Set horizontal move speed
				currentHorizontalSpeed += Input.X * acceleration * Time.deltaTime;

				// clamped by max frame movement
				currentHorizontalSpeed = Mathf.Clamp(currentHorizontalSpeed, -moveClamp, moveClamp);

				// Apply bonus at the apex of a jump
				var apexBonus = Mathf.Sign(Input.X) * this.apexBonus * apexPoint;
				currentHorizontalSpeed += apexBonus * Time.deltaTime;
			}
			else
			{
				// No input. Let's slow the character down
				currentHorizontalSpeed = Mathf.MoveTowards(currentHorizontalSpeed, 0, deAcceleration * Time.deltaTime);
			}

			if (currentHorizontalSpeed > 0 && colRight || currentHorizontalSpeed < 0 && colLeft)
			{
				// Don't walk through walls
				currentHorizontalSpeed = 0;
			}
		}

		#endregion

		#region Gravity

		[Header("GRAVITY")]
		[SerializeField] private float fallClamp = -40f;
		[SerializeField] private float minFallSpeed = 80f;
		[SerializeField] private float maxFallSpeed = 120f;
		private float fallSpeed;

		private void CalculateGravity()
		{
			if (colDown)
			{
				// Move out of the ground
				if (currentVerticalSpeed < 0) currentVerticalSpeed = 0;
			}
			else
			{
				// Add downward force while ascending if we ended the jump early
				var fallSpeed = endedJumpEarly && currentVerticalSpeed > 0 ? this.fallSpeed * jumpEndEarlyGravityModifier : this.fallSpeed;

				// Fall
				currentVerticalSpeed -= fallSpeed * Time.deltaTime;

				// Clamp
				if (currentVerticalSpeed < fallClamp) currentVerticalSpeed = fallClamp;
			}
		}

		#endregion

		#region Jump

		[Header("JUMPING")]
		[SerializeField] private float jumpHeight = 30;
		[SerializeField] private float jumpApexThreshold = 10f;
		[SerializeField] private float coyoteTimeThreshold = 0.1f;
		[SerializeField] private float jumpBuffer = 0.1f;
		[SerializeField] private float jumpEndEarlyGravityModifier = 3;
		private bool coyoteUsable;
		private bool endedJumpEarly = true;
		private float apexPoint; // Becomes 1 at the apex of a jump
		private float lastJumpPressed;
		private bool CanUseCoyote => coyoteUsable && !colDown && timeLeftGrounded + coyoteTimeThreshold > Time.time;
		private bool HasBufferedJump => colDown && jumpInputCount > 0 && lastJumpPressed + jumpBuffer > Time.time;
		private int jumpInputCount;

		private void CalculateJumpApex()
		{
			if (!colDown)
			{
				// Gets stronger the closer to the top of the jump
				apexPoint = Mathf.InverseLerp(jumpApexThreshold, 0, Mathf.Abs(Velocity.y));
				fallSpeed = Mathf.Lerp(minFallSpeed, maxFallSpeed, apexPoint);
			}
			else
			{
				apexPoint = 0;
			}
		}

		private void CalculateJump()
		{
			if (Input.JumpDown)
			{
				switchTest = true;
				jumpInputCount++;
			}

			// Jump if: grounded or within coyote threshold || sufficient jump buffer
			if ((Input.JumpDown && CanUseCoyote) || HasBufferedJump)
			{
				currentVerticalSpeed = jumpHeight;
				endedJumpEarly = false;
				coyoteUsable = false;
				timeLeftGrounded = float.MinValue;
				JumpingThisFrame = true;
			}
			else
			{
				JumpingThisFrame = false;
			}

			if (Grounded && switchTest)
			{
				switchTest = false;
				jumpInputCount = 0;
			}

			// End the jump early if button released
			if (!colDown && Input.JumpUp && !endedJumpEarly && Velocity.y > 0)
			{
				endedJumpEarly = true;
			}

			if (colUp)
			{
				if (currentVerticalSpeed > 0) currentVerticalSpeed = 0;
			}
		}

		#endregion

		#region Move

		[Header("MOVE")]
		[SerializeField, Tooltip("Raising this value increases collision accuracy at the cost of performance.")]
		private int freeColliderIterations = 10;

		// We cast our bounds before moving to avoid future collisions
		private void MoveCharacter()
		{
			var pos = transform.position;
			RawMovement = new Vector3(currentHorizontalSpeed, currentVerticalSpeed); // Used externally
			var move = RawMovement * Time.deltaTime;
			var furthestPoint = pos + move;

			// check furthest movement. If nothing hit, move and don't do extra checks
			var hit = Physics2D.OverlapBox(furthestPoint, characterBounds.size, 0, groundLayer);
			if (!hit)
			{
				transform.position += move;
				return;
			}

			// otherwise increment away from current pos; see what closest position we can move to
			var positionToMoveTo = transform.position;
			for (var i = 1; i < freeColliderIterations; i++)
			{
				// increment to check all but furthestPoint - we did that already
				var t = (float)i / freeColliderIterations;
				var posToTry = Vector2.Lerp(pos, furthestPoint, t);

				if (Physics2D.OverlapBox(posToTry, characterBounds.size, 0, groundLayer))
				{
					transform.position = positionToMoveTo;

					// We've landed on a corner or hit our head on a ledge. Nudge the player gently
					if (i == 1)
					{
						if (currentVerticalSpeed < 0) currentVerticalSpeed = 0;
						var dir = transform.position - hit.transform.position;
						transform.position += dir.normalized * move.magnitude;
					}

					return;
				}

				positionToMoveTo = posToTry;
			}
		}

		#endregion
	}
}