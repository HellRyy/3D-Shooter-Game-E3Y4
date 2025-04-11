using System;
using UnityEditor.Rendering.LookDev;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private PlayerControls controls;
    private CharacterController characterController;
    private Animator animator;

    [Header("Movement")]
    private float speed;
    public Vector3 movementDirection;
    public float walkSpeed;
    public float runSpeed;
    private float verticalVelocity;
    private bool isRunning;

    [Header("Aim info")]
    public LayerMask aimLayerMask;
    private Vector3 lookingDirection;
    public Transform aim;

    public Vector2 moveInput;
    public Vector2 aimInput;

    private void Awake()
    {
        AssignInputEvents();
    }


    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();

        speed = walkSpeed;
    }

    private void Update()
    {
        ApplyMovement();
        AimTowardsMouse();
        AnimatorControllers();
    }

    private void AnimatorControllers()
    {
        float xVelocity = Vector3.Dot(movementDirection.normalized, transform.right);
        float zVelocity = Vector3.Dot(movementDirection.normalized, transform.forward);

        animator.SetFloat("xVelocity", xVelocity, .1f, Time.deltaTime);
        animator.SetFloat("zVelocity", zVelocity, .1f, Time.deltaTime);
        animator.SetBool("isRunning", isRunning);
    }

    private void AimTowardsMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(aimInput);

        if (Physics.Raycast(ray, out var hitInfo, Mathf.Infinity, aimLayerMask))
        {
            lookingDirection = hitInfo.point - transform.position;
            lookingDirection.y = 0f;
            lookingDirection.Normalize();

            transform.forward = lookingDirection;

            aim.position = new Vector3(hitInfo.point.x, transform.position.y, hitInfo.point.z);
        }
    }

    private void ApplyMovement()
    {
        movementDirection = new Vector3(moveInput.x, 0, moveInput.y);
        ApplyGravity();

        if (movementDirection.magnitude > 0)
        {
            characterController.Move(movementDirection * Time.deltaTime * speed);
        }
    }

    private void AssignInputEvents()
    {
        controls = new PlayerControls();

        controls.Character.Fire.performed += Context => Shoot();

        controls.Character.Movement.performed += Context => moveInput = Context.ReadValue<Vector2>();
        controls.Character.Movement.canceled += Context => moveInput = Vector2.zero;

        controls.Character.Aim.performed += Context => aimInput = Context.ReadValue<Vector2>();
        controls.Character.Aim.canceled += Context => aimInput = Vector2.zero;

        controls.Character.Run.performed += context =>
        {
            speed = runSpeed;
            isRunning = true;
        };

        controls.Character.Run.performed += context =>
        {
            speed = walkSpeed;
            isRunning = false;
        };

        controls.Character.Fire.performed += ctx => Shoot();
    }
    private void ApplyGravity()
    {
        if (characterController.isGrounded == false)
        {
            verticalVelocity = verticalVelocity - 9.81f * Time.deltaTime;
            movementDirection.y = verticalVelocity;
        }
        else
            verticalVelocity = -.5f;
    }

    private void Shoot()
    {
        Debug.Log("FIRE!");
        animator.SetTrigger("Fire");
    }
    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }
}
