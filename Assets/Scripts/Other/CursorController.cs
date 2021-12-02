﻿using System.Collections.Generic;
using Player_Control;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Other {
	public class CursorController : MonoBehaviour {
		public GameObject selectedInteractableObject;

		public PlayerController player;

		public Color baseHighlightColor;
		public Color selectedHighlightColor = Color.green;
		public Color notEnabledHighlightColor;

		[SerializeField] private float maxRayLength = 100f;

		[SerializeField] private List<Interactable> interactablesInRange;

		private Camera _mainCamera;

		private PlayerInputActions _playerInputActions;

		private Mouse   _mouse;
		private Vector3 _cursorPosition;

		private void Start() {
			_mainCamera = Camera.main;
			_mouse      = Mouse.current;

			_playerInputActions = player.PlayerInputActions;

			_playerInputActions.Player.Interact.performed += TriggerInteract;
		}

		private void Update() {
			bool hasModifiedInteractableList = false;
			foreach (Interactable interactable in FindObjectsOfType<Interactable>()) {
				if (player.GetDistanceToObject(interactable.gameObject) <= player.interactDistance) {
					if (!interactablesInRange.Contains(interactable)) {
						interactablesInRange.Add(interactable);
						hasModifiedInteractableList = true;
					}
				} else {
					if (interactablesInRange.Remove(interactable)) {
						interactable.SetHighlight(false, Color.clear);
						hasModifiedInteractableList = true;
					}
				}
			}

			if (hasModifiedInteractableList) ComputeInteractableOutlines();
			SetMousePos();
		}

		private void ComputeInteractableOutlines() {
			for (int i = 0; i < interactablesInRange.Count; i++) {
				Interactable interactable       = interactablesInRange[i];
				GameObject   interactableObject = interactable.gameObject;
				interactable.SetHighlight(
					true,
					interactableObject == selectedInteractableObject
						? interactable.isEnabled ? selectedHighlightColor : notEnabledHighlightColor
						: baseHighlightColor,
					interactableObject == selectedInteractableObject ? 5.0f : 2.0f);
			}
		}


		private void SetMousePos() {
			Vector2 mousePos = _mouse.position.ReadValue();
			Ray     ray      = _mainCamera.ScreenPointToRay(mousePos);

			LayerMask colliderMask = LayerMask.GetMask("Interactable");
			if (Physics.Raycast(ray, out RaycastHit hit, maxRayLength, colliderMask)) {
				_cursorPosition = hit.point;
				Interactable highlighted = hit.collider.gameObject.GetComponent<Interactable>();
				if (!highlighted) {
					Debug.LogWarning(
						$"Object {hit.collider.gameObject} is in the Interactable layer, but does not have the Interactable component!");
					if (selectedInteractableObject != null) OnHighlightStop();
					selectedInteractableObject = null;
				} else {
					if (!hit.collider.gameObject.Equals(selectedInteractableObject))
						OnHighlightStart(hit.collider.gameObject);
				}
			} else {
				_cursorPosition = ray.direction * maxRayLength + _mainCamera.transform.position;
				if (selectedInteractableObject != null) OnHighlightStop();
			}

			transform.position = _cursorPosition;
		}

		private void TriggerInteract(InputAction.CallbackContext context) {
			if (selectedInteractableObject) {
				selectedInteractableObject.GetComponent<Interactable>().onInteractEvent.Invoke();
			}
		}

		private void OnHighlightStart(GameObject newInteractable) {
			if (player.GetDistanceToObject(newInteractable) > player.interactDistance) return;
			selectedInteractableObject = newInteractable;
			ComputeInteractableOutlines();
		}

		private void OnHighlightStop() {
			selectedInteractableObject = null;
			ComputeInteractableOutlines();
		}
	}
}