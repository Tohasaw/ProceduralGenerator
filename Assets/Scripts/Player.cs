using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }

    public event EventHandler<OnSelectedDecorationChangedArgs> OnSelectedDecorationChanged;

    public class OnSelectedDecorationChangedArgs : EventArgs {
        public Decoration selectedDecoration;
    }

    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private GameInput gameInput;
    [SerializeField] private LayerMask interactLayerMask;

    private bool isWalking;
    private Vector3 lastInteractDir;
    private Decoration selectedDecoration;

    private void Start() {
        gameInput.OnInteractAction += GameInput_OnInteractAction;
    }

    private void Awake() {
        if (Instance != null) {
            Debug.LogError("There is more than one Player instance");
        }
        Instance = this;
    }

    private void GameInput_OnInteractAction(object sender, EventArgs e) {
        if (selectedDecoration != null) {
            Debug.Log(selectedDecoration.transform.position);
        }
    }

    private void Update() {
        HandleMovement();
        HandleInteractions();
    }

    public bool IsWalking() => isWalking;

    private void HandleInteractions() {
        Vector2 inputVector = gameInput.GetMovementVectorNormalized();

        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);

        if (moveDir != Vector3.zero) {
            lastInteractDir = moveDir;
        }

        float interactDistance = 2f;
        if (Physics.Raycast(transform.position, lastInteractDir, out RaycastHit raycastHit, interactDistance, interactLayerMask)) {
            if (raycastHit.transform.TryGetComponent(out Decoration decoration)) {
                // Has ClearCounter
                if (decoration != selectedDecoration) {
                    SetSelectedDecoration(decoration);
                }
            } else {
                SetSelectedDecoration(null);
            }
        } else {
            SetSelectedDecoration(null);
        }
    }

    private void SetSelectedDecoration(Decoration selectedDecoration) {
        this.selectedDecoration = selectedDecoration;

        OnSelectedDecorationChanged?.Invoke(this, new OnSelectedDecorationChangedArgs() {
            selectedDecoration = selectedDecoration
        });
    }

    private void HandleMovement() {
        Vector2 inputVector = gameInput.GetMovementVectorNormalized();

        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);

        bool canMove = CanMove(moveDir);

        if (!canMove) {
            // Cannot move towards moveDir

            // Attempt only X movement
            Vector3 moveDirX = new Vector3(moveDir.x, 0, 0).normalized;
            canMove = (moveDir.x < -.5f || moveDir.x > +.5f) && CanMove(moveDirX);

            if (canMove) {
                // Can move only on the X
                moveDir = moveDirX;
            } else {
                // Cannot move only on the X

                // Attempt only Z movement
                Vector3 moveDirZ = new Vector3(0, 0, moveDir.z).normalized;
                canMove = (moveDir.z < -.5f || moveDir.z > +.5f) && CanMove(moveDirZ);

                if (canMove) {
                    moveDir = moveDirZ;
                }
                // else Cannot move in any direction
            }
        }
        if (canMove) {
            transform.position += moveDir * moveSpeed * Time.deltaTime;
        }

        isWalking = moveDir != Vector3.zero;

        float rotateSpeed = 10f;
        transform.forward = Vector3.Slerp(transform.forward, moveDir, Time.deltaTime * rotateSpeed);
    }

    private bool CanMove(Vector3 moveDir) {
        float moveDistance = moveSpeed * Time.deltaTime;
        float playerRadius = .7f;
        float playerHeight = 2f;

        return !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDir, moveDistance);
    }
}
