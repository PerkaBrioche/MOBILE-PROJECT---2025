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
    
    [SerializeField] private int atkRange;
    public int AtkRange
    {
        get { return atkRange; }
        set { atkRange = value; }
    }

    [SerializeField] private bool canMooveAndShoot;
    public bool CanMooveAndShoot
    { 
        get { return canMooveAndShoot; }
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
}

    // [SerializeField] private int right;
    // public int Right
    // {
    //     get { return right; }
    // }
    // [SerializeField] private int left;
    // public int Left
    // {
    //     get { return left; }
    // }
    // [SerializeField] private int up;
    // public int Up
    // {
    //     get { return up; }
    // }
    // [SerializeField] private int down;
    // public int Down
    // {
    //     get { return down; }
    // }
    //
    // [Header("DEPLACEMENT DIAGONAL")]
    // [SerializeField] private int upRight;
    // public int UpRight
    // {
    //     get { return upRight; }
    // }
    // [SerializeField] private int upLeft;
    // public int UpLeft
    // {
    //     get { return upLeft; }
    // }
    // [SerializeField] private int downRight;
    // public int DownRight
    // {
    //     get { return downRight; }
    // }
    // [SerializeField] private int downLeft;
    // public int DownLeft
    // {
    //     get { return downLeft; }
    // }
    