using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] int playerIndex = 1;
    [SerializeField] float moveSpeed = 18f;
    [SerializeField] float interactionRadius = 2f;

    float sospecha;
    Rigidbody rb;
    IInteractable nearbyInteractable;

    public int PlayerIndex => playerIndex;
    public float Sospecha => sospecha;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying)
        {
            return;
        }

        HandleMovement();
        DetectNearbyInteractable();
        HandleInteraction();
    }

    void HandleMovement()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
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
