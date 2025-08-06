using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PlatformerCharacter.InputSystem
{
	public class JoystickController : MonoBehaviour
	{
		private PlayerControls input;
		public event Action<FrameInput> OnInput;
		private FrameInput frameInput;

		private InputState inputTriggered = InputState.Empty;
		private bool jumpUp;
		private bool jumpDown;
		private float x;

		private enum InputState
		{
			Dirac,
			PostDirac,
			Empty
		}

		private void ResetInput()
		{
			jumpUp = false;
			jumpDown = false;
		}

		private void Awake()
		{
			input = new PlayerControls();
			input.Gameplay.Validate.performed += ctx => OnJumpPressed();
			input.Gameplay.Validate.canceled += ctx => OnJumpReleased();
			input.Gameplay.X.started += OnMoveStarted;
			input.Gameplay.X.canceled += OnMoveStopped;
			input.Gameplay.X.performed += OnMovePerformed;
		}

		private void OnMovePerformed(InputAction.CallbackContext obj)
		{
			inputTriggered = InputState.Dirac;
			x = obj.ReadValue<float>();
		}

		private void OnMoveStopped(InputAction.CallbackContext obj)
		{
			inputTriggered = InputState.Dirac;
			x = obj.ReadValue<float>();
		}

		private void Update()
		{
			if (inputTriggered == InputState.Dirac)
			{
				OnInput?.Invoke(new FrameInput
				{
					JumpUp = jumpUp,
					JumpDown = jumpDown,
					X = x
				});
				ResetInput();
				inputTriggered = InputState.PostDirac;
			}
			else if (inputTriggered == InputState.PostDirac)
			{
				OnInput?.Invoke(new FrameInput
				{
					JumpUp = jumpUp,
					JumpDown = jumpDown,
					X = x
				});
				inputTriggered = InputState.Empty;
			}
		}

		private void OnMoveStarted(InputAction.CallbackContext obj)
		{
			inputTriggered = InputState.Dirac;
			x = obj.ReadValue<float>();
		}

		private void OnJumpReleased()
		{
			inputTriggered = InputState.Dirac;
			jumpUp = true;
		}

		private void OnJumpPressed()
		{
			inputTriggered = InputState.Dirac;
			jumpDown = true;
		}

		private void OnEnable()
		{
			input.Gameplay.Enable();
		}

		private void OnDisable()
		{
			input.Gameplay.Disable();
		}
	}
}