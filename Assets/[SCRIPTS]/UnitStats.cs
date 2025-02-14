using UnityEngine;

[CreateAssetMenu(fileName = "NewUnitStats", menuName = "Unit/Unit Stats", order = 1)]
public class UnitStats : ScriptableObject
{
    [Header("Informations générales")]
    
    [SerializeField] private Sprite _unitIcon;
    public Sprite UnitIcon
    {
        get { return _unitIcon; }
        set { _unitIcon = value; }
    }
    
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
    
    [SerializeField] private int atkRange;
    public int AtkRange
    {
        get { return atkRange; }
        set { atkRange = value; }
    }

    [Header("DEPLACEMENT HORIZONTAL ET VERTICAL")]
    [SerializeField] private int _walkDistance;
    public int WalkDistance
    {
        get { return _walkDistance; }
    }
    [SerializeField] private int _attackRange;
    public int AttackRange
    {
        get { return _attackRange; }
    }
    
    [Space(10)]
    [Header("OTHERS")]
    [SerializeField] private bool canMooveAndShoot;
    public bool CanMooveAndShoot
    { 
        get { return canMooveAndShoot; }
    }
    [SerializeField] private int _cooldownAttack;
    public int CooldownAttack
    {
        get { return _cooldownAttack; }
    }
}