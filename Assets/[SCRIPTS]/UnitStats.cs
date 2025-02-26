using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NewUnitStats", menuName = "Unit/Unit Stats", order = 1)]
public class UnitStats : ScriptableObject
{
    [Header("Informations générales")]
    
    [SerializeField] private Sprite _unitAllyIcon;
    public Sprite _unitallyIcon
    {
        get { return _unitAllyIcon; }
        set { _unitAllyIcon = value; }
    }
    [SerializeField] private Sprite _unitEnemyIcon;
    public Sprite _unitenemyIcon
    {
        get { return _unitEnemyIcon; }
        set { _unitEnemyIcon = value; }
    }
    [SerializeField] private string _unitName;
    public string UnitName
    {
        get { return _unitName; }
        set { _unitName = value; }
    }
    
    [SerializeField] private Component _unitType;

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
    [SerializeField] private int _cooldownAttack;
    public int CooldownAttack
    {
        get { return _cooldownAttack; }
    }
}