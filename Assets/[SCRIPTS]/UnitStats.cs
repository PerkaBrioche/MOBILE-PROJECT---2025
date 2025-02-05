using UnityEngine;

[CreateAssetMenu(fileName = "NewUnitStats", menuName = "Unit/Unit Stats", order = 1)]
public class UnitStats : ScriptableObject
{
    [Header("Informations générales")]
    [SerializeField] private string _unitName;
    public string UnitName
    
    {
        get { return _unitName; }
        set { _unitName = value; }
    }

    [Header("Statistiques de combat")]
    [SerializeField] private int hp;
    public int HP
    {
        get { return hp; }
        set { hp = value; }
    }

    [SerializeField] private int atk;
    public int ATK
    {
        get { return atk; }
        set { atk = value; }
    }

    [SerializeField] private float def;
    public float DEF
    {
        get { return def; }
        set { def = value; }
    }

    [Header("Mobilité et portée")]
    [SerializeField] private int walkDist;
    public int WalkDist
    {
        get { return walkDist; }
        set { walkDist = value; }
    }

    [SerializeField] private int atkRange;
    public int AtkRange
    {
        get { return atkRange; }
        set { atkRange = value; }
    }
}