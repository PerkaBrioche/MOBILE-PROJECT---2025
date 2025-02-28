using UnityEngine;

public class Character : MonoBehaviour
{
    [SerializeField] private UnitStats stats;
    public UnitStats Stats
    {
        get { return stats; }
        set { stats = value; }
    }

    void Start()
    {
        if (stats != null)
        {
            Debug.Log("Le personnage " + stats.UnitName + " démarre avec " + stats.HP + " points de vie et " + stats.ATK + " d'attaque.");
        }
        else
        {
            Debug.LogWarning("Aucun UnitStats assigné pour " + gameObject.name);
        }
    }
}