using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] int playerIndex = 1;
    [SerializeField] float moveSpeed = 30f;
    [SerializeField] float interactionRadius = 2f;
    [SerializeField] float idleThresholdSeconds = 3f;

    float sospecha;
    float idleTime;
    bool minigameLocked;
    bool inHideZone;
    bool hasMovementInput;
    Rigidbody rb;
    Vector3 lastFramePosition;
    IInteractable nearbyInteractable;

    public int PlayerIndex => playerIndex;
    public float Sospecha => sospecha;
    public bool IsMinigameLocked => minigameLocked;
    public bool IsInHideZone => inHideZone;

    public void SetMinigameLocked(bool locked)
    {
        minigameLocked = locked;
    }

    public void SetInHideZone(bool inside)
    {
        inHideZone = inside;
    }

    public bool AppearsOccupied()
    {
        if (minigameLocked)
        {
            return true;
        }

        if (inHideZone)
        {
            return false;
        }

        if (IsNearTicket())
        {
            return true;
        }

        if (hasMovementInput || HasMovedRecently())
        {
            return true;
        }

        if (idleTime >= idleThresholdSeconds)
        {
            return false;
        }

        return true;
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        lastFramePosition = transform.position;
    }

    void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying)
        {
            return;
        }

        UpdateOccupancyTracking();
        DetectNearbyInteractable();

        if (minigameLocked)
        {
            return;
        }

        HandleMovement();
        HandleInteraction();
    }

    void UpdateOccupancyTracking()
    {
        hasMovementInput = ReadMovementInput().sqrMagnitude > 0.001f;

        float movedSqr = (transform.position - lastFramePosition).sqrMagnitude;
        if (movedSqr > 0.0004f || hasMovementInput)
        {
            idleTime = 0f;
        }
        else
        {
            idleTime += Time.deltaTime;
        }

        lastFramePosition = transform.position;
    }

    Vector3 ReadMovementInput()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return Vector3.zero;
        }

        Vector3 input = Vector3.zero;

        if (playerIndex == 1)
        {
            if (keyboard.wKey.isPressed) input.z += 1f;
            if (keyboard.sKey.isPressed) input.z -= 1f;
            if (keyboard.aKey.isPressed) input.x -= 1f;
            if (keyboard.dKey.isPressed) input.x += 1f;
        }
        else
        {
            if (keyboard.upArrowKey.isPressed) input.z += 1f;
            if (keyboard.downArrowKey.isPressed) input.z -= 1f;
            if (keyboard.leftArrowKey.isPressed) input.x -= 1f;
            if (keyboard.rightArrowKey.isPressed) input.x += 1f;
        }

        return input;
    }

    bool HasMovedRecently()
    {
        return idleTime < 0.15f;
    }

    bool IsNearTicket()
    {
        if (nearbyInteractable is Ticket)
        {
            return true;
        }

        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            interactionRadius,
            ~0,
            QueryTriggerInteraction.Collide);

        foreach (Collider hit in hits)
        {
            if (hit.transform == transform)
            {
                continue;
            }

            if (hit.GetComponentInParent<Ticket>() != null)
            {
                return true;
            }
        }

        return false;
    }

    void HandleMovement()
    {
        Vector3 input = ReadMovementInput();
        if (input.sqrMagnitude > 1f)
        {
            input.Normalize();
        }

        Vector3 movement = input * moveSpeed * Time.deltaTime;
        rb.MovePosition(rb.position + movement);
    }

    void DetectNearbyInteractable()
    {
        nearbyInteractable = null;
        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            interactionRadius,
            ~0,
            QueryTriggerInteraction.Collide);

        float closestDistance = float.MaxValue;
        foreach (Collider hit in hits)
        {
            if (hit.transform == transform)
            {
                continue;
            }

            IInteractable interactable = hit.GetComponentInParent<IInteractable>();
            if (interactable == null)
            {
                continue;
            }

            float distance = Vector3.Distance(transform.position, hit.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                nearbyInteractable = interactable;
            }
        }
    }

    void HandleInteraction()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        bool interactPressed = playerIndex == 1
            ? keyboard.eKey.wasPressedThisFrame
            : keyboard.rightCtrlKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame;

        if (!interactPressed || nearbyInteractable == null)
        {
            return;
        }

        nearbyInteractable.Interact(this);
    }

    public void AddSospecha(float amount)
    {
        sospecha = Mathf.Clamp(sospecha + amount, 0f, 100f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}
