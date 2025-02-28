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
            Debug.Log("Le personnage " + stats.UnitName + " démarre avec " + stats.HP + " points de vie, " + stats.ATK + " d'attaque, et une défense de " + stats.DEF);
        }
        else
        {
            Debug.LogWarning("Aucun UnitStats assigné pour " + gameObject.name);
        }
    }
}