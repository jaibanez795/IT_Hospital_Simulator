using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerStats))]
public class PlayerController : MonoBehaviour
{
    const float ReviveDuration = 3f;

    [SerializeField] int playerIndex = 1;
    [SerializeField] float moveSpeed = 30f;
    [SerializeField] float interactionRadius = 2f;
    [SerializeField] float idleThresholdSeconds = 3f;

    float idleTime;
    bool minigameLocked;
    bool inHideZone;
    bool hasMovementInput;
    Rigidbody rb;
    Vector3 lastFramePosition;
    IInteractable nearbyInteractable;
    PlayerStats stats;
    PlayerController nearbyIncapacitatedPlayer;
    float reviveProgress;

    Transform visualRoot;
    Transform incapacitatedIndicator;
    Renderer[] visualRenderers;
    readonly Dictionary<Renderer, Color> originalRendererColors = new Dictionary<Renderer, Color>();

    Renderer playerRenderer;
    Color originalColor;
    Vector3 originalScale;
    bool hasVisualDefaults;

    public int PlayerIndex => playerIndex;
    public PlayerStats Stats => stats;
    public float Sospecha => stats != null ? stats.Sospecha : 0f;
    public bool IsMinigameLocked => minigameLocked;
    public bool IsInHideZone => inHideZone;
    public bool IsActive => stats != null && stats.IsActive;

    public void SetMinigameLocked(bool locked)
    {
        minigameLocked = locked;
    }

    public void SetInHideZone(bool inside)
    {
        inHideZone = inside;
    }

    public void OnIncapacitated()
    {
        SetInHideZone(false);
        ApplyIncapacitatedVisual();
    }

    public void OnRevived()
    {
        RestoreActiveVisual();
    }

    public bool AppearsOccupied()
    {
        if (!IsActive)
        {
            return true;
        }

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

        stats = GetComponent<PlayerStats>();
        stats.Configure(playerIndex);

        CacheVisualDefaults();
    }

    void CacheVisualDefaults()
    {
        visualRoot = transform.Find("PlayerVisual");
        incapacitatedIndicator = transform.Find("IncapacitatedIndicator");
        originalScale = transform.localScale;
        originalRendererColors.Clear();

        if (visualRoot != null)
        {
            visualRenderers = visualRoot.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < visualRenderers.Length; i++)
            {
                Renderer renderer = visualRenderers[i];
                if (renderer != null)
                {
                    originalRendererColors[renderer] = renderer.material.color;
                }
            }

            hasVisualDefaults = visualRenderers.Length > 0;
            return;
        }

        playerRenderer = GetComponentInChildren<Renderer>();
        if (playerRenderer != null)
        {
            originalColor = playerRenderer.material.color;
            hasVisualDefaults = true;
        }
    }

    void ApplyIncapacitatedVisual()
    {
        if (!hasVisualDefaults)
        {
            CacheVisualDefaults();
        }

        if (visualRenderers != null && visualRenderers.Length > 0)
        {
            for (int i = 0; i < visualRenderers.Length; i++)
            {
                Renderer renderer = visualRenderers[i];
                if (renderer == null)
                {
                    continue;
                }

                renderer.material.color = new Color(0.5f, 0.5f, 0.55f, 0.5f);
            }
        }
        else if (playerRenderer != null)
        {
            playerRenderer.material.color = new Color(0.5f, 0.5f, 0.55f, 0.5f);
        }

        if (incapacitatedIndicator != null)
        {
            incapacitatedIndicator.gameObject.SetActive(true);
        }

        transform.localScale = originalScale * 0.9f;
    }

    void RestoreActiveVisual()
    {
        if (!hasVisualDefaults)
        {
            return;
        }

        if (visualRenderers != null && visualRenderers.Length > 0)
        {
            for (int i = 0; i < visualRenderers.Length; i++)
            {
                Renderer renderer = visualRenderers[i];
                if (renderer == null)
                {
                    continue;
                }

                if (originalRendererColors.TryGetValue(renderer, out Color color))
                {
                    renderer.material.color = color;
                }
            }
        }
        else if (playerRenderer != null)
        {
            playerRenderer.material.color = originalColor;
        }

        if (incapacitatedIndicator != null)
        {
            incapacitatedIndicator.gameObject.SetActive(false);
        }

        transform.localScale = originalScale;
    }

    void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying)
        {
            return;
        }

        if (!IsActive)
        {
            return;
        }

        UpdateOccupancyTracking();
        DetectNearbyIncapacitatedPlayer();

        if (nearbyIncapacitatedPlayer != null && IsInteractHeld())
        {
            HandleReviveTeammate();
            if (!minigameLocked)
            {
                HandleMovement();
            }

            return;
        }

        HandleReviveTeammate();
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

    void DetectNearbyIncapacitatedPlayer()
    {
        nearbyIncapacitatedPlayer = null;
        float closestDistance = float.MaxValue;

        PlayerController[] players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        foreach (PlayerController player in players)
        {
            if (player == null || player == this || player.IsActive)
            {
                continue;
            }

            if (player.Stats == null || !player.Stats.IsIncapacitated)
            {
                continue;
            }

            float distance = Vector3.Distance(transform.position, player.transform.position);
            if (distance <= interactionRadius && distance < closestDistance)
            {
                closestDistance = distance;
                nearbyIncapacitatedPlayer = player;
            }
        }
    }

    void HandleReviveTeammate()
    {
        if (nearbyIncapacitatedPlayer == null || !IsInteractHeld())
        {
            if (reviveProgress > 0f)
            {
                reviveProgress = 0f;
                GameManager.Instance?.SetReviveProgress(null);
            }

            return;
        }

        reviveProgress += Time.deltaTime;
        string helperLabel = stats != null ? stats.GetLabel() : $"J{playerIndex}";
        string targetLabel = nearbyIncapacitatedPlayer.Stats != null
            ? nearbyIncapacitatedPlayer.Stats.GetLabel()
            : $"J{nearbyIncapacitatedPlayer.PlayerIndex}";

        GameManager.Instance?.SetReviveProgress(
            $"{helperLabel} está cubriendo a {targetLabel}: {reviveProgress:0.0}s / {ReviveDuration:0.0}s");

        if (reviveProgress >= ReviveDuration)
        {
            nearbyIncapacitatedPlayer.Stats.RevivePlayer();
            reviveProgress = 0f;
            nearbyIncapacitatedPlayer = null;
            GameManager.Instance?.SetReviveProgress(null);
        }
    }

    bool IsInteractHeld()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return false;
        }

        return playerIndex == 1
            ? keyboard.eKey.isPressed
            : keyboard.rightCtrlKey.isPressed || keyboard.enterKey.isPressed;
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
        if (!IsActive)
        {
            return;
        }

        stats?.AddSospecha(amount);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}
