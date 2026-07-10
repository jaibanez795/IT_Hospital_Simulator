using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class HideZone : MonoBehaviour
{
    [SerializeField] float estresReductionPerSecond = 3f;
    [SerializeField] float catchCheckInterval = 5f;
    [SerializeField] float catchChance = 0.2f;
    [SerializeField] float sospechaOnCatch = 25f;

    readonly HashSet<PlayerController> playersInside = new HashSet<PlayerController>();
    readonly Dictionary<PlayerController, float> catchTimers = new Dictionary<PlayerController, float>();

    void Awake()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsPlaying)
        {
            return;
        }

        if (playersInside.Count == 0)
        {
            return;
        }

        ReduceStressForPlayersInside();
        CheckForCatch();
    }

    void ReduceStressForPlayersInside()
    {
        foreach (PlayerController player in playersInside)
        {
            if (player == null || !player.IsActive || player.Stats == null)
            {
                continue;
            }

            player.Stats.AddEstres(-estresReductionPerSecond * Time.deltaTime);
        }
    }

    void CheckForCatch()
    {
        List<PlayerController> players = new List<PlayerController>(playersInside);

        foreach (PlayerController player in players)
        {
            if (player == null || !player.IsActive)
            {
                continue;
            }

            if (!catchTimers.ContainsKey(player))
            {
                catchTimers[player] = 0f;
            }

            catchTimers[player] += Time.deltaTime;

            if (catchTimers[player] < catchCheckInterval)
            {
                continue;
            }

            catchTimers[player] = 0f;

            if (Random.value <= catchChance)
            {
                player.AddSospecha(sospechaOnCatch);
                string label = player.Stats != null ? player.Stats.GetLabel() : "J?";
                GameManager.Instance.ShowTemporaryMessage($"{label} fue cachado descansando en el site");
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null || !player.IsActive)
        {
            return;
        }

        playersInside.Add(player);
        player.SetInHideZone(true);
        catchTimers[player] = 0f;
    }

    void OnTriggerExit(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null)
        {
            return;
        }

        playersInside.Remove(player);
        player.SetInHideZone(false);
        catchTimers.Remove(player);
    }
}
