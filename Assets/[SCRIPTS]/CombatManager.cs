using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance;
    public float dashSpeed = 10f;
    public float dashDistancePercentage = 1f;
    public float attackDelay = 0.5f;
    
    public TextMeshProUGUI AllyHpText;
    public TextMeshProUGUI AllyDamagageText;
    public TextMeshProUGUI EnemyHpText;
    public TextMeshProUGUI targetDamageTaken;
    public TextMeshProUGUI EnemyDamageText;
    public Image AllyPilotImage;
    public Image EnemyPilotImage;
    private ShipController _attackerShip;
    private ShipController _allyShips;
    private bool _isInCombat = false;
    
    private bool AllySelectionned = false;
    private bool EnemySelectionned = false;
    
    private bool prewiewing = false;
    private bool canPrewiew = true;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }

    private void Start()
    {
        ClearPreview();
    }

    public void StartCombat(ShipController attacker, ShipController target)
    {
        TouchManager tm = UnityEngine.Object.FindFirstObjectByType<TouchManager>();
        _attackerShip = attacker;
        _allyShips = target;
        _isInCombat = true;
        StartCoroutine(DashAttack());
    }

    private IEnumerator DashAttack()
    {
        Vector3 originalPosition = _attackerShip.transform.position;
        Vector3 targetPosition = _allyShips.transform.position;
        Vector3 dashTarget = Vector3.Lerp(originalPosition, targetPosition, dashDistancePercentage);
        float distance = Vector3.Distance(originalPosition, dashTarget);
        float dashTime = distance / dashSpeed;
        float elapsed = 0f;
        while (elapsed < dashTime)
        {
            elapsed += Time.deltaTime;
            _attackerShip.transform.position = Vector3.Lerp(originalPosition, dashTarget, elapsed / dashTime);
            yield return null;
        }
        _attackerShip.transform.position = dashTarget;
        int damage = _attackerShip.runtimeStats.ATK;
        if (_attackerShip.HasBonusDamage())
        {
            _attackerShip.SetBonusDamage(false);
        }
        _allyShips.TakeDamage(damage);
        
        yield return new WaitForSeconds(attackDelay);
        elapsed = 0f;
        while (elapsed < dashTime)
        {
            elapsed += Time.deltaTime;
            _attackerShip.transform.position = Vector3.Lerp(dashTarget, originalPosition, elapsed / dashTime);
            yield return null;
        }
        _attackerShip.transform.position = originalPosition;
        TouchManager tm2 = UnityEngine.Object.FindFirstObjectByType<TouchManager>();
        if (tm2 != null)
        {
            
        }
        _isInCombat = false;
        ClearPreview();
    }

    public void PreviewCombat(ShipController ally, ShipController enemy)
    {
        _attackerShip = enemy;
        _allyShips = ally;
        int predictedDamage = _allyShips.runtimeStats.ATK;
        int predictedHP = _attackerShip.runtimeStats.HP - predictedDamage;
        
        if (EnemyHpText != null)
            EnemyHpText.text = predictedHP.ToString() + " HP";
        if (EnemyDamageText != null)
            EnemyDamageText.text = predictedDamage.ToString() + " ATK";
        if (EnemyPilotImage != null)
            EnemyPilotImage.sprite = _attackerShip.GetUnitStats().PiloteEnnemi;
        if (AllyHpText != null)
            AllyHpText.text = _allyShips.runtimeStats.HP.ToString() + " HP";
        if (AllyDamagageText != null)
            AllyDamagageText.text = _allyShips.runtimeStats.ATK.ToString() + " ATK";
        if (AllyPilotImage != null)
            AllyPilotImage.sprite = _allyShips.GetUnitStats().PiloteAllie;
        ChangeImageOpacity(false, AllyPilotImage, EnemyPilotImage);
        prewiewing = true;
    }

    private void Update()
    {
        if (prewiewing)
        {
            if (canPrewiew)
            {
                canPrewiew = false;
                StartCoroutine(BlinkTargetHP());
            }
        }
    }

    private IEnumerator BlinkTargetHP()
    {
        float blinkDuration = 0.4f;
        EnemyHpText.color = Color.red;
        yield return new WaitForSeconds(blinkDuration);
        canPrewiew = true;
    }



    public void ClearPreview()
    {
        prewiewing = false;
        canPrewiew = true;
        ResetColor();
        print("clear preview");
        EnemyHpText.text = "";
        EnemyDamageText.text = "";
        AllyHpText.text = "";
        AllyDamagageText.text = "";
        targetDamageTaken.text = "";
        EnemyPilotImage.sprite = null;
        AllyPilotImage.sprite = null;
        ChangeImageOpacity(true, AllyPilotImage, EnemyPilotImage);
    }

    private void ResetColor()
    {
        EnemyHpText.color = Color.green;
        AllyHpText.color = Color.green;
        AllyDamagageText.color = Color.white;
        EnemyDamageText.color = Color.white;
    }

    private void ChangeImageOpacity(bool diseapear, Image AllyPilotImage = null, Image EnemyPilotImage = null)
    {
        if (diseapear)
        {
            if (AllyPilotImage != null)
            {
                AllyPilotImage.color = new Color(AllyPilotImage.color.r, AllyPilotImage.color.g, AllyPilotImage.color.b, 0);
            }
            if (EnemyPilotImage != null)
            { 
                EnemyPilotImage.color = new Color(EnemyPilotImage.color.r, EnemyPilotImage.color.g, EnemyPilotImage.color.b, 0);
            }
        }
        else
        {
            if (AllyPilotImage  != null)
            {
                AllyPilotImage.color = new Color(AllyPilotImage.color.r, AllyPilotImage.color.g, AllyPilotImage.color.b, 1);
            }
            if (EnemyPilotImage != null)
            {
                EnemyPilotImage.color = new Color(EnemyPilotImage.color.r, EnemyPilotImage.color.g, EnemyPilotImage.color.b, 1);
            }
        }
    }
    
    public void DisplayAttackerStats(ShipController attacker)
    {
        print("Display Attacker Stats");
        ChangeImageOpacity(false, null, EnemyPilotImage);
        _attackerShip = attacker;
        EnemyHpText.text = attacker.runtimeStats.HP.ToString() + " HP";
        EnemyDamageText.text = attacker.runtimeStats.ATK.ToString() + " ATK";
        EnemyPilotImage.sprite = attacker.GetUnitStats().PiloteEnnemi;
    }
    
    public void DisplayAllyStats(ShipController ally)
    {
        ChangeImageOpacity(false, AllyPilotImage, null);
        _allyShips = ally;
        print("DISPLAY ALLY STATS");
        AllyHpText.text = ally.runtimeStats.HP.ToString() + " HP";
        AllyDamagageText.text = ally.runtimeStats.ATK.ToString() + " ATK";
        AllyPilotImage.sprite = ally.GetUnitStats().PiloteAllie;
    }

    public bool IsInCombat()
    {
        return _isInCombat;
    }
}
