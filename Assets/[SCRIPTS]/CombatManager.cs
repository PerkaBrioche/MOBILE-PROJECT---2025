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
    
    public TextMeshProUGUI attackerHPText;
    public TextMeshProUGUI attackerAtkText;
    public TextMeshProUGUI targetHPText;
    public TextMeshProUGUI targetDamageTaken;
    public TextMeshProUGUI targetAtkText;
    public Image attackerPilotImage;
    public Image targetPilotImage;
    private ShipController _attackerShip;
    private ShipController _targetShip;
    private bool _isInCombat = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }

    public void StartCombat(ShipController attacker, ShipController target)
    {
        TouchManager tm = UnityEngine.Object.FindFirstObjectByType<TouchManager>();
        _attackerShip = attacker;
        _targetShip = target;
        _isInCombat = true;
        StartCoroutine(DashAttack());
    }

    private IEnumerator DashAttack()
    {

        Vector3 originalPosition = _attackerShip.transform.position;
        Vector3 targetPosition = _targetShip.transform.position;
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
        _targetShip.TakeDamage(damage);
        if (attackerHPText != null)
            attackerHPText.text = _attackerShip.runtimeStats.HP.ToString();
        if (attackerAtkText != null)
            attackerAtkText.text = _attackerShip.runtimeStats.ATK.ToString();
        if (targetHPText != null)
            targetHPText.text = _targetShip.runtimeStats.HP.ToString();
        if (targetAtkText != null)
            targetAtkText.text = _targetShip.runtimeStats.ATK.ToString();
        if (attackerPilotImage != null)
            attackerPilotImage.sprite = _attackerShip.IsAnEnemy() ? _attackerShip.GetUnitStats().PiloteEnnemi : _attackerShip.GetUnitStats().PiloteAllie;
        if (targetPilotImage != null)
            targetPilotImage.sprite = _targetShip.IsAnEnemy() ? _targetShip.GetUnitStats().PiloteEnnemi : _targetShip.GetUnitStats().PiloteAllie;
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
    }

    public void PreviewCombat(ShipController attacker, ShipController target)
    {
        _attackerShip = attacker;
        _targetShip = target;
        int predictedDamage = attacker.runtimeStats.ATK;
        int predictedHP = target.runtimeStats.HP - predictedDamage;
        if (attackerHPText != null)
            attackerHPText.text = attacker.runtimeStats.HP.ToString() + " HP";
        if (attackerAtkText != null)
            attackerAtkText.text = attacker.runtimeStats.ATK.ToString() + " ATK";
        if (targetHPText != null)
            targetHPText.text = target.runtimeStats.HP.ToString() + " HP";
        if (targetAtkText != null)
            targetAtkText.text = target.runtimeStats.ATK.ToString() + " ATK";
        if(attackerPilotImage != null)
            targetDamageTaken.text = "- " + attacker.runtimeStats.ATK.ToString();
        if (attackerPilotImage != null)
            attackerPilotImage.sprite = attacker.IsAnEnemy() ? attacker.GetUnitStats().PiloteEnnemi : attacker.GetUnitStats().PiloteAllie;
        if (targetPilotImage != null)
            targetPilotImage.sprite = target.IsAnEnemy() ? target.GetUnitStats().PiloteEnnemi : target.GetUnitStats().PiloteAllie;
        StartCoroutine(BlinkTargetHP());
    }

    private IEnumerator BlinkTargetHP()
    {
        float blinkDuration = 0.5f;
        int blinkCount = 3;
        Color originalColor = targetHPText.color;
        for (int i = 0; i < blinkCount; i++)
        {
            targetDamageTaken.color = Color.clear;
            yield return new WaitForSeconds(blinkDuration);
            targetDamageTaken.color = Color.red;
            yield return new WaitForSeconds(blinkDuration);
        }
        targetDamageTaken.color = Color.red;

    }

    public void DisplayAttackerStats(ShipController attacker)
    {
        _attackerShip = attacker;
        if (attackerHPText != null)
            attackerHPText.text = attacker.runtimeStats.HP.ToString();
        if (attackerAtkText != null)
            attackerAtkText.text = attacker.runtimeStats.ATK.ToString();
        if (attackerPilotImage != null)
            attackerPilotImage.sprite = attacker.IsAnEnemy() ? attacker.GetUnitStats().PiloteEnnemi : attacker.GetUnitStats().PiloteAllie;
    }

    public void ClearPreview()
    {
        if (attackerHPText != null)
            attackerHPText.text = "";
        if (attackerAtkText != null)
            attackerAtkText.text = "";
        if (targetHPText != null)
            targetHPText.text = "";
        if (targetAtkText != null)
            targetAtkText.text = "";
        if (attackerPilotImage != null)
            attackerPilotImage.sprite = null;
        if (targetPilotImage != null)
            targetPilotImage.sprite = null;
        if (targetDamageTaken != null)
            targetDamageTaken.text = "";
    }

    public bool IsInCombat()
    {
        return _isInCombat;
    }
}
