using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerStats : MonoBehaviour
{
    [SerializeField] int playerId = 1;
    [SerializeField] float estres = 20f;
    [SerializeField] float sospecha;
    [SerializeField] float desempenoVisible = 30f;
    [SerializeField] int actas;
    [SerializeField] bool isActive = true;
    [SerializeField] PlayerIncapacitationReason incapacitationReason = PlayerIncapacitationReason.None;

    [Header("Debug")]
    [SerializeField] bool enableDebugKeys;

    public int PlayerId => playerId;
    public float Estres => estres;
    public float Sospecha => sospecha;
    public float DesempenoVisible => desempenoVisible;
    public int Actas => actas;
    public bool IsActive => isActive;
    public bool IsIncapacitated => !isActive && incapacitationReason != PlayerIncapacitationReason.None;
    public PlayerIncapacitationReason IncapacitationReason => incapacitationReason;

    public void Configure(int id)
    {
        playerId = id;
    }

    public string GetLabel()
    {
        return playerId == 1 ? "J1" : "J2";
    }

    public string GetStatusLabel()
    {
        if (isActive)
        {
            return "ACTIVO";
        }

        return incapacitationReason switch
        {
            PlayerIncapacitationReason.Burnout => "INHABILITADO (Burnout)",
            PlayerIncapacitationReason.Promotion => "INHABILITADO (Promoción)",
            PlayerIncapacitationReason.RhDismissal => "INHABILITADO (RH)",
            _ => "INHABILITADO"
        };
    }

    void Update()
    {
        if (!enableDebugKeys)
        {
            return;
        }

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        if (playerId == 1)
        {
            if (keyboard.f1Key.wasPressedThisFrame)
            {
                DebugForceBurnout();
            }
            else if (keyboard.f2Key.wasPressedThisFrame)
            {
                DebugForcePromotion();
            }
            else if (keyboard.f3Key.wasPressedThisFrame)
            {
                DebugForceRhDismissal();
            }
        }
        else if (playerId == 2)
        {
            if (keyboard.f4Key.wasPressedThisFrame)
            {
                DebugForceBurnout();
            }
            else if (keyboard.f5Key.wasPressedThisFrame)
            {
                DebugForcePromotion();
            }
            else if (keyboard.f6Key.wasPressedThisFrame)
            {
                DebugForceRhDismissal();
            }
        }
    }

    public void AddEstres(float amount)
    {
        if (!isActive)
        {
            return;
        }

        estres = Mathf.Clamp(estres + amount, 0f, 100f);
        CheckThresholds();
    }

    public void AddSospecha(float amount)
    {
        if (!isActive)
        {
            return;
        }

        sospecha = Mathf.Clamp(sospecha + amount, 0f, 100f);
        CheckThresholds();
    }

    public void AddDesempeno(float amount)
    {
        if (!isActive)
        {
            return;
        }

        desempenoVisible = Mathf.Clamp(desempenoVisible + amount, 0f, 100f);
        CheckThresholds();
    }

    public void AddActa()
    {
        if (!isActive)
        {
            return;
        }

        actas++;
    }

    public void RevivePlayer()
    {
        if (isActive || incapacitationReason == PlayerIncapacitationReason.None)
        {
            return;
        }

        PlayerIncapacitationReason reason = incapacitationReason;
        string message = ApplyRevivalMetrics(reason);

        isActive = true;
        incapacitationReason = PlayerIncapacitationReason.None;

        PlayerController controller = GetComponent<PlayerController>();
        controller?.OnRevived();

        GameManager.Instance?.SetReviveProgress(null);
        GameManager.Instance?.ShowTemporaryMessage(message);
        GameManager.Instance?.OnPlayerRevived();
        GameManager.Instance?.RefreshUI();
    }

    string ApplyRevivalMetrics(PlayerIncapacitationReason reason)
    {
        string label = GetLabel();

        switch (reason)
        {
            case PlayerIncapacitationReason.Burnout:
                estres = 65f;
                sospecha = Mathf.Clamp(sospecha + 5f, 0f, 100f);
                return $"{label} volvió del burnout";

            case PlayerIncapacitationReason.Promotion:
                desempenoVisible = 70f;
                sospecha = Mathf.Clamp(sospecha + 10f, 0f, 100f);
                return "Lo bajaron de la junta, pero ya lo tienen visto";

            case PlayerIncapacitationReason.RhDismissal:
                actas = 2;
                sospecha = 70f;
                estres = Mathf.Clamp(estres + 10f, 0f, 100f);
                return "RH te dio una última oportunidad";

            default:
                estres = Mathf.Min(estres, 65f);
                desempenoVisible = Mathf.Min(desempenoVisible, 70f);
                sospecha = Mathf.Min(sospecha, 70f);
                actas = Mathf.Min(actas, 2);
                return $"{label} volvió a estar activo";
        }
    }

    void IncapacitatePlayer(PlayerIncapacitationReason reason, string message)
    {
        if (!isActive)
        {
            return;
        }

        isActive = false;
        incapacitationReason = reason;

        PlayerController controller = GetComponent<PlayerController>();
        controller?.OnIncapacitated();

        GameManager.Instance?.ShowTemporaryMessage(message);
        GameManager.Instance?.OnPlayerIncapacitated();
        GameManager.Instance?.RefreshUI();
    }

    void CheckThresholds()
    {
        if (!isActive)
        {
            return;
        }

        if (desempenoVisible >= 100f)
        {
            IncapacitatePlayer(
                PlayerIncapacitationReason.Promotion,
                $"{GetLabel()} fue promovido temporalmente a junta eterna");
            return;
        }

        if (estres >= 100f)
        {
            IncapacitatePlayer(
                PlayerIncapacitationReason.Burnout,
                $"{GetLabel()} quedó inhabilitado por burnout");
            return;
        }

        if (sospecha >= 100f)
        {
            AddActa();
            sospecha = 40f;
            GameManager.Instance?.ShowTemporaryMessage($"{GetLabel()} recibió acta administrativa");

            if (actas >= 3)
            {
                IncapacitatePlayer(
                    PlayerIncapacitationReason.RhDismissal,
                    $"{GetLabel()} quedó atorado con RH");
            }
            else
            {
                GameManager.Instance?.RefreshUI();
            }
        }
    }

    [ContextMenu("Debug/Force Burnout")]
    public void DebugForceBurnout()
    {
        if (!isActive)
        {
            return;
        }

        estres = 100f;
        CheckThresholds();
    }

    [ContextMenu("Debug/Force Promotion")]
    public void DebugForcePromotion()
    {
        if (!isActive)
        {
            return;
        }

        desempenoVisible = 100f;
        CheckThresholds();
    }

    [ContextMenu("Debug/Force RH Dismissal")]
    public void DebugForceRhDismissal()
    {
        if (!isActive)
        {
            return;
        }

        actas = 3;
        IncapacitatePlayer(
            PlayerIncapacitationReason.RhDismissal,
            $"{GetLabel()} quedó atorado con RH");
    }
}
