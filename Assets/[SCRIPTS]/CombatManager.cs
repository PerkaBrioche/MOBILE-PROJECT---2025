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
    public static CombatManager Instance;
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
        if (tm != null)
            tm.SetInteractionEnabled(false);
        _attackerShip = attacker;
        _targetShip = target;
        _isInCombat = true;
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
            attackerHPText.text = _attackerShip.runtimeStats.HP.ToString();
        if (attackerAtkText != null)
            attackerAtkText.text = _attackerShip.runtimeStats.ATK.ToString();
        if (targetHPText != null)
            targetHPText.text = _targetShip.runtimeStats.HP.ToString();
        if (targetAtkText != null)
            targetAtkText.text = _targetShip.runtimeStats.ATK.ToString();
        if (attackerPilotImage != null)
            attackerPilotImage.sprite = _attackerShip.GetSprite();
        if (targetPilotImage != null)
            targetPilotImage.sprite = _targetShip.GetSprite();
        yield return new WaitForSeconds(attackDelay);
        elapsed = 0f;
        while (elapsed < dashTime)
        {
            elapsed += Time.deltaTime;
            _attackerShip.transform.position = Vector3.Lerp(targetPosition, originalPosition, elapsed / dashTime);
            yield return null;
        }
        _attackerShip.transform.position = originalPosition;
        TouchManager tm2 = UnityEngine.Object.FindFirstObjectByType<TouchManager>();
        if (tm2 != null)
            tm2.SetInteractionEnabled(true);
        if (_targetShip.runtimeStats.HP <= 0)
            _targetShip.Die();
        _isInCombat = false;
    }
        Fight();
    }

    // private IEnumerator CombatSequence()
    // {
    //     _isInCombat = true;
    //     yield return StartCoroutine(AnimateBordersIn());
    //     GameObject attackerVisual = Instantiate(_shipContainer.gameObject, leftStartPoint.position, Quaternion.identity);
    //     GameObject targetVisual = Instantiate(_shipContainer.gameObject, rightStartPoint.position, Quaternion.identity);
    //
    //     attackerVisual.GetComponent<SpriteRenderer>().sprite = _attackerShip.GetSprite();
    //     targetVisual.GetComponent<SpriteRenderer>().sprite = _targetShip.GetSprite();
    //     
    //     if (attackerVisual.TryGetComponent<ShipController>(out ShipController sc1))
    //         sc1.enabled = false;
    //     if (targetVisual.TryGetComponent<ShipController>(out ShipController sc2))
    //         sc2.enabled = false;
    //     yield return StartCoroutine(ApproachAndBattle(attackerVisual, targetVisual));
    //     yield return StartCoroutine(AnimateBordersOut());
    //     TouchManager tm = FindObjectOfType<TouchManager>();
    //     if (tm != null) tm.SetInteractionEnabled(true);
    // }

    // private IEnumerator ApproachAndBattle(GameObject attackerVisual, GameObject targetVisual)
    // {
    //     bool leftReached = false;
    //     bool rightReached = false;
    //     while (!leftReached || !rightReached)
    //     {
    //         if (!leftReached)
    //         {
    //             attackerVisual.transform.position = Vector3.MoveTowards(attackerVisual.transform.position, leftCombatPoint.position, moveSpeed * Time.deltaTime);
    //             if (Vector3.Distance(attackerVisual.transform.position, leftCombatPoint.position) < 0.01f)
    //                 leftReached = true;
    //         }
    //         if (!rightReached)
    //         {
    //             targetVisual.transform.position = Vector3.MoveTowards(targetVisual.transform.position, rightCombatPoint.position, moveSpeed * Time.deltaTime);
    //             if (Vector3.Distance(targetVisual.transform.position, rightCombatPoint.position) < 0.01f)
    //                 rightReached = true;
    //         }
    //         yield return null;
    //     }
    //     yield return new WaitForSeconds(0.5f);
    //     yield return StartCoroutine(Battle(attackerVisual, targetVisual));
    // }

    // private IEnumerator Battle(GameObject attackerVisual, GameObject targetVisual)
    // {
    //     // Animator anim = attackerVisual.GetComponent<Animator>();
    //     // if (anim != null) 
    //     //     anim.SetTrigger("Attack");
    //     yield return new WaitForSeconds(1f);
    //    
    //     yield return new WaitForSeconds(0.5f);
    //     Destroy(attackerVisual);
    //     Destroy(targetVisual);
    //
    // }

    private void Fight()
    {
        int damage = _attackerShip.runtimeStats.ATK - _targetShip.runtimeStats.DEF;
        if (damage < 1) damage = 1;
        _targetShip.TakeDamage(damage);
        // if (_targetShip.runtimeStats.HP <= 0)
        //     _targetShip.Die();
    }

    // private IEnumerator AnimateBordersIn()
    // {
    //     float elapsed = 0f;
    //     Vector2 topStart = topBorder.sizeDelta;
    //     Vector2 bottomStart = bottomBorder.sizeDelta;
    //     Vector2 topTarget = new Vector2(topStart.x, targetBorderHeight);
    //     Vector2 bottomTarget = new Vector2(bottomStart.x, targetBorderHeight);
    //     while (elapsed < borderAnimationDuration)
    //     {
    //         elapsed += Time.deltaTime;
    //         float t = elapsed / borderAnimationDuration;
    //         topBorder.sizeDelta = Vector2.Lerp(topStart, topTarget, t);
    //         bottomBorder.sizeDelta = Vector2.Lerp(bottomStart, bottomTarget, t);
    //         _bgFight.color = new Color(0.2f, 0.2f, 0.2f,  t/1.5f);
    //         yield return null;
    //     }
    //     topBorder.sizeDelta = topTarget;
    //     bottomBorder.sizeDelta = bottomTarget;
    // }

    // private IEnumerator AnimateBordersOut()
    // {
    //     float elapsed = 0f;
    //     Vector2 topStart = topBorder.sizeDelta;
    //     Vector2 bottomStart = bottomBorder.sizeDelta;
    //     Vector2 topTarget = new Vector2(topStart.x, 0f);
    //     Vector2 bottomTarget = new Vector2(bottomStart.x, 0f);
    //     while (elapsed < borderAnimationDuration)
    //     {
    //         elapsed += Time.deltaTime;
    //         float t = elapsed / borderAnimationDuration;
    //         topBorder.sizeDelta = Vector2.Lerp(topStart, topTarget, t);
    //         bottomBorder.sizeDelta = Vector2.Lerp(bottomStart, bottomTarget, t);
    //         _bgFight.color = new Color(0.2f, 0.2f, 0.2f, 1 -  t/1.5f);
    //         yield return null;
    //     }
    //     _bgFight.color = new Color(0.2f, 0.2f, 0.2f, 0);
    //
    //     topBorder.sizeDelta = topTarget;
    //     bottomBorder.sizeDelta = bottomTarget;
    //     _isInCombat = false;
    // }

    private void Reset()
    {
        _attackerShip = null;
        _targetShip = null;
    }
    
    public bool IsInCombat()
    {
        return _isInCombat;
    }
}
