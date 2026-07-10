using UnityEngine;

[System.Serializable]
public class NPCData
{
    [SerializeField] string npcId;
    [SerializeField] string displayName;
    [SerializeField] string department;
    [SerializeField] NPCPersonality personality;

    public string NpcId => npcId;
    public string DisplayName => displayName;
    public string Department => department;
    public NPCPersonality Personality => personality;

    public NPCData(string id, string name, string dept, NPCPersonality npcPersonality)
    {
        npcId = id;
        displayName = name;
        department = dept;
        personality = npcPersonality;
    }
}
