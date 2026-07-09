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

        GameManager.Instance.AddEstres(-estresReductionPerSecond * playersInside.Count * Time.deltaTime);
        CheckForCatch();
    }

    void CheckForCatch()
    {
        List<PlayerController> players = new List<PlayerController>(playersInside);

        foreach (PlayerController player in players)
        {
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
                GameManager.Instance.ShowTemporaryMessage("Te cacharon descansando en el site");
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null)
        {
            return;
        }

        playersInside.Add(player);
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
        catchTimers.Remove(player);
    }
}
