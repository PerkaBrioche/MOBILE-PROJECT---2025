using System.Collections;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    public Transform leftStartPoint;
    public Transform rightStartPoint;
    public Transform leftCombatPoint;
    public Transform rightCombatPoint;
    public float moveSpeed = 2f;
    public float attackDelay = 1f;

    private ShipController _attackerShip;
    private ShipController _targetShip;

    public void StartCombat(ShipController attacker, ShipController target)
    {
        TouchManager tm = FindObjectOfType<TouchManager>();
        if (tm != null) tm.SetInteractionEnabled(false);
        _attackerShip = attacker;
        _targetShip = target;
        GameObject attackerVisual = Instantiate(attacker.gameObject, leftStartPoint.position, Quaternion.identity);
        GameObject targetVisual = Instantiate(target.gameObject, rightStartPoint.position, Quaternion.identity);
        if (attackerVisual.TryGetComponent<ShipController>(out ShipController sc1))
            sc1.enabled = false;
        if (targetVisual.TryGetComponent<ShipController>(out ShipController sc2))
            sc2.enabled = false;
        StartCoroutine(ApproachAndBattle(attackerVisual, targetVisual));
    }

    private IEnumerator ApproachAndBattle(GameObject attackerVisual, GameObject targetVisual)
    {
        bool leftReached = false;
        bool rightReached = false;
        while (!leftReached || !rightReached)
        {
            if (!leftReached)
            {
                attackerVisual.transform.position = Vector3.MoveTowards(attackerVisual.transform.position, leftCombatPoint.position, moveSpeed * Time.deltaTime);
                if (Vector3.Distance(attackerVisual.transform.position, leftCombatPoint.position) < 0.01f)
                    leftReached = true;
            }
            if (!rightReached)
            {
                targetVisual.transform.position = Vector3.MoveTowards(targetVisual.transform.position, rightCombatPoint.position, moveSpeed * Time.deltaTime);
                if (Vector3.Distance(targetVisual.transform.position, rightCombatPoint.position) < 0.01f)
                    rightReached = true;
            }
            yield return null;
        }
        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(Battle(attackerVisual, targetVisual));
    }

    private IEnumerator Battle(GameObject attackerVisual, GameObject targetVisual)
    {
        Animator anim = attackerVisual.GetComponent<Animator>();
        if (anim != null)
            anim.SetTrigger("Attack");
        yield return new WaitForSeconds(1f);
        int damage = Mathf.RoundToInt(_attackerShip.runtimeStats.ATK * _targetShip.runtimeStats.DEF);
        if (damage < 1) damage = 1;
        _targetShip.runtimeStats.HP -= damage;
        float defDisplay = 10f - (10f * _targetShip.runtimeStats.DEF);
        Debug.Log(_attackerShip.runtimeStats.UnitName + " inflige " + damage + " dégâts à " + _targetShip.runtimeStats.UnitName 
            + " (défense affichée : " + defDisplay + "). HP restant : " + _targetShip.runtimeStats.HP);
        yield return new WaitForSeconds(0.5f);
        Destroy(attackerVisual);
        Destroy(targetVisual);
        if (_targetShip.runtimeStats.HP <= 0)
            _targetShip.Die();
        TouchManager tm = FindObjectOfType<TouchManager>();
        if (tm != null) tm.SetInteractionEnabled(true);
        yield return null;
    }
}
