using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance;
    public float dashSpeed = 10f;
    public float attackDelay = 0.5f;
    public TextMeshProUGUI attackerHPText;
    public TextMeshProUGUI attackerAtkText;
    public TextMeshProUGUI targetHPText;
    public TextMeshProUGUI targetAtkText;
    public Image attackerPilotImage;
    public Image targetPilotImage;
    
    private ShipController _attackerShip;
    private ShipController _targetShip;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    public void StartCombat(ShipController attacker, ShipController target)
    {
        TouchManager tm = FindObjectOfType<TouchManager>();
        if (tm != null) tm.SetInteractionEnabled(false);
        _attackerShip = attacker;
        _targetShip = target;
        StartCoroutine(DashAttack());
    }

    private IEnumerator DashAttack()
    {
        Vector3 originalPosition = _attackerShip.transform.position;
        Vector3 targetPosition = _targetShip.transform.position;
        float distance = Vector3.Distance(originalPosition, targetPosition);
        float dashTime = distance / dashSpeed;
        float elapsed = 0f;
        while (elapsed < dashTime)
        {
            elapsed += Time.deltaTime;
            _attackerShip.transform.position = Vector3.Lerp(originalPosition, targetPosition, elapsed / dashTime);
            yield return null;
        }
        _attackerShip.transform.position = targetPosition;
        int damage = _attackerShip.runtimeStats.ATK;
        _targetShip.TakeDamage(damage);
        if (attackerHPText != null)
        {
            attackerHPText.text = _attackerShip.runtimeStats.HP.ToString();
        }
        if (attackerAtkText != null)
        {
            attackerAtkText.text = _attackerShip.runtimeStats.ATK.ToString();
        }
        if (targetHPText != null)
        {
            targetHPText.text = _targetShip.runtimeStats.HP.ToString();
        }
        if (targetAtkText != null)
        {
            targetAtkText.text = _targetShip.runtimeStats.ATK.ToString();
        }
        if (attackerPilotImage != null)
        {
            attackerPilotImage.sprite = _attackerShip.GetSprite();
        }
        if (targetPilotImage != null)
        {
            targetPilotImage.sprite = _targetShip.GetSprite();
        }
        yield return new WaitForSeconds(attackDelay);
        elapsed = 0f;
        while (elapsed < dashTime)
        {
            elapsed += Time.deltaTime;
            _attackerShip.transform.position = Vector3.Lerp(targetPosition, originalPosition, elapsed / dashTime);
            yield return null;
        }
        _attackerShip.transform.position = originalPosition;
        TouchManager tm = FindObjectOfType<TouchManager>();
        if (tm != null) tm.SetInteractionEnabled(true);
        if (_targetShip.runtimeStats.HP <= 0)
            _targetShip.Die();
    }
}
