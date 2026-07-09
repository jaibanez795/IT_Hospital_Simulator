using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] Text operacionText;
    [SerializeField] Text estresText;
    [SerializeField] Text desempenoText;
    [SerializeField] Text timerText;
    [SerializeField] Text sospechaJ1Text;
    [SerializeField] Text sospechaJ2Text;

    [Header("Messages")]
    [SerializeField] Text temporaryMessageText;
    [SerializeField] Text globalEventBannerText;
    [SerializeField] GameObject endScreenPanel;
    [SerializeField] Text endScreenTitleText;
    [SerializeField] Text endScreenReasonText;

    PlayerController player1;
    PlayerController player2;

    void Start()
    {
        PlayerController[] players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        foreach (PlayerController player in players)
        {
            if (player.PlayerIndex == 1)
            {
                player1 = player;
            }
            else if (player.PlayerIndex == 2)
            {
                player2 = player;
            }
        }

        if (endScreenPanel != null)
        {
            endScreenPanel.SetActive(false);
        }

        ClearTemporaryMessage();
    }

    void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsPlaying)
        {
            return;
        }

        RefreshStats(
            GameManager.Instance.Operacion,
            GameManager.Instance.Estres,
            GameManager.Instance.Desempeno,
            GameManager.Instance.TimeRemaining);

        RefreshSospecha();
    }

    public void RefreshStats(float operacion, float estres, float desempeno, float timeRemaining)
    {
        SetText(operacionText, $"Operación: {operacion:0}");
        SetText(estresText, $"Estrés: {estres:0}");
        SetText(desempenoText, $"Desempeño: {desempeno:0}");
        SetText(timerText, $"Tiempo: {Mathf.CeilToInt(timeRemaining)}s");
    }

    void RefreshSospecha()
    {
        SetText(sospechaJ1Text, $"Sospecha J1: {(player1 != null ? player1.Sospecha : 0f):0}");
        SetText(sospechaJ2Text, $"Sospecha J2: {(player2 != null ? player2.Sospecha : 0f):0}");
    }

    public void SetTemporaryMessage(string message)
    {
        SetText(temporaryMessageText, message);
    }

    public void ClearTemporaryMessage()
    {
        SetText(temporaryMessageText, string.Empty);
    }

    public void SetGlobalEventBanner(string message)
    {
        SetText(globalEventBannerText, message);
    }

    public void ClearGlobalEventBanner()
    {
        SetText(globalEventBannerText, string.Empty);
    }

    public void ShowEndScreen(GameState state, string reason)
    {
        if (endScreenPanel != null)
        {
            endScreenPanel.SetActive(true);
        }

        string title = state == GameState.Won ? "VICTORIA" : "DERROTA";
        SetText(endScreenTitleText, title);
        SetText(endScreenReasonText, reason);

        RefreshStats(
            GameManager.Instance.Operacion,
            GameManager.Instance.Estres,
            GameManager.Instance.Desempeno,
            GameManager.Instance.TimeRemaining);
        RefreshSospecha();
    }

    static void SetText(Text label, string value)
    {
        if (label != null)
        {
            label.text = value;
        }
    }
}
