using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [SerializeField] int playerId = 1;
    [SerializeField] float estres = 20f;
    [SerializeField] float sospecha;
    [SerializeField] float desempenoVisible = 30f;
    [SerializeField] int actas;
    [SerializeField] bool isActive = true;
    [SerializeField] bool isPromoted;
    [SerializeField] bool isBurnedOut;
    [SerializeField] bool isFired;

    public int PlayerId => playerId;
    public float Estres => estres;
    public float Sospecha => sospecha;
    public float DesempenoVisible => desempenoVisible;
    public int Actas => actas;
    public bool IsActive => isActive;
    public bool IsPromoted => isPromoted;
    public bool IsBurnedOut => isBurnedOut;
    public bool IsFired => isFired;

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
        if (!isActive)
        {
            if (isPromoted)
            {
                return "INACTIVO (promovido)";
            }

            if (isBurnedOut)
            {
                return "INACTIVO (burnout)";
            }

            if (isFired)
            {
                return "INACTIVO (despedido)";
            }

            return "INACTIVO";
        }

        return "ACTIVO";
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
        actas++;
    }

    public void EliminatePlayer(string reason)
    {
        if (!isActive)
        {
            return;
        }

        isActive = false;
        GameManager.Instance?.ShowTemporaryMessage(reason);
        GameManager.Instance?.CheckTeamElimination();
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
            isPromoted = true;
            EliminatePlayer($"{GetLabel()} fue promovido a Coordinador de Transformación Digital");
            return;
        }

        if (estres >= 100f)
        {
            isBurnedOut = true;
            EliminatePlayer($"{GetLabel()} abrió un ticket para sí mismo");
            return;
        }

        if (sospecha >= 100f)
        {
            AddActa();
            sospecha = 40f;
            GameManager.Instance?.ShowTemporaryMessage($"{GetLabel()} recibió acta administrativa");

            if (actas >= 3)
            {
                isFired = true;
                EliminatePlayer($"{GetLabel()} fue despedido");
            }
            else
            {
                GameManager.Instance?.RefreshUI();
            }
        }
    }
}
