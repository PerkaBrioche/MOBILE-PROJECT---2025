using System.Collections;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    public GameObject characterPrefab;
    public Transform leftStartPoint;
    public Transform rightStartPoint;
    public Transform leftCombatPoint;
    public Transform rightCombatPoint;
    public float moveSpeed = 2f;
    public float attackDelay = 1f;

    private GameObject leftUnit;
    private GameObject rightUnit;
    private Character leftCharacter;
    private Character rightCharacter;

    public void StartCombat(UnitStats stats1, UnitStats stats2)
    {
        if (characterPrefab == null)
        {
            Debug.LogError("Prefab manquant");
            return;
        }
        leftUnit = Instantiate(characterPrefab, leftStartPoint.position, Quaternion.identity);
        rightUnit = Instantiate(characterPrefab, rightStartPoint.position, Quaternion.identity);
        leftCharacter = leftUnit.GetComponent<Character>();
        rightCharacter = rightUnit.GetComponent<Character>();
        leftCharacter.Stats = stats1;
        rightCharacter.Stats = stats2;
        StartCoroutine(ApproachAndBattle());
    }

    private IEnumerator ApproachAndBattle()
    {
        bool leftReached = false;
        bool rightReached = false;
        while (!leftReached || !rightReached)
        {
            if (!leftReached)
            {
                leftUnit.transform.position = Vector3.MoveTowards(leftUnit.transform.position, leftCombatPoint.position, moveSpeed * Time.deltaTime);
                if (Vector3.Distance(leftUnit.transform.position, leftCombatPoint.position) < 0.01f)
                {
                    leftReached = true;
                }
            }
            if (!rightReached)
            {
                rightUnit.transform.position = Vector3.MoveTowards(rightUnit.transform.position, rightCombatPoint.position, moveSpeed * Time.deltaTime);
                if (Vector3.Distance(rightUnit.transform.position, rightCombatPoint.position) < 0.01f)
                {
                    rightReached = true;
                }
            }
            yield return null;
        }
        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(Battle());
    }

    private IEnumerator Battle()
    {
        bool leftTurn = true;
        while (leftCharacter.Stats.HP > 0 && rightCharacter.Stats.HP > 0)
        {
            if (leftTurn)
            {
                int damage = leftCharacter.Stats.ATK - Mathf.FloorToInt(rightCharacter.Stats.DEF);
                if (damage <= 0) damage = 1;
                rightCharacter.Stats.HP -= damage;
                Debug.Log(leftCharacter.Stats.UnitName + " inflige " + damage + " dégâts à " + rightCharacter.Stats.UnitName + ". HP restant: " + rightCharacter.Stats.HP);
            }
            else
            {
                int damage = rightCharacter.Stats.ATK - Mathf.FloorToInt(leftCharacter.Stats.DEF);
                if (damage <= 0) damage = 1;
                leftCharacter.Stats.HP -= damage;
                Debug.Log(rightCharacter.Stats.UnitName + " inflige " + damage + " dégâts à " + leftCharacter.Stats.UnitName + ". HP restant: " + leftCharacter.Stats.HP);
            }
            leftTurn = !leftTurn;
            yield return new WaitForSeconds(attackDelay);
        }
        if (leftCharacter.Stats.HP <= 0 && rightCharacter.Stats.HP <= 0)
        {
            Debug.Log("Match nul");
        }
        else if (leftCharacter.Stats.HP <= 0)
        {
            Debug.Log(rightCharacter.Stats.UnitName + " gagne le combat");
        }
        else if (rightCharacter.Stats.HP <= 0)
        {
            Debug.Log(leftCharacter.Stats.UnitName + " gagne le combat");
        }
        yield return null;
    }
}
