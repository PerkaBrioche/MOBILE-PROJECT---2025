using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class ResetTurnManager : MonoBehaviour
{
    public static ResetTurnManager Instance;
    private List<ShipController> recordedShips = new List<ShipController>();
    private List<SpecialTileState> recordedSpecialTiles = new List<SpecialTileState>();

    [System.Serializable]
    public class SpecialTileState
    {
        public TilesController tile;
        public Vector3 position;
        public TilesController.tileType tileType;
    }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }

    public void RecordStartingPositions()
    {
        StartCoroutine(WaitForSave());

    }
    
    private IEnumerator WaitForSave()
    {
        yield return new WaitForSeconds(0.5f);
        recordedShips.Clear();
        foreach (ShipController ship in ShipManager.Instance.GetAllships())
        {
            if (ship.gameObject.activeInHierarchy)
            {
                ship.SaveStartingState();
                if (!recordedShips.Contains(ship))
                {
                    recordedShips.Add(ship);
                }
            }
        }
        recordedSpecialTiles.Clear();
        TilesController[] allTiles = Resources.FindObjectsOfTypeAll<TilesController>();
        foreach (TilesController tile in allTiles)
        {
            if (tile.GetTileType() == TilesController.tileType.HealTile || tile.GetTileType() == TilesController.tileType.DamageTile)
            {
                SpecialTileState sts = new SpecialTileState();
                sts.tile = tile;
                sts.position = tile.transform.position;
                sts.tileType = tile.GetTileType();
                recordedSpecialTiles.Add(sts);
            }
        }
    }

    public void ResetTurn()
    {
        if (!TurnManager.Instance.IsPlayerTurn() || TurnManager.Instance.IsEndGame())
        {
            return;
        }
        foreach (ShipController ship in recordedShips)
        {
            if (ship.WasAliveAtTurnStart)
            {
                if (ship.IsAnEnemy())
                {
                    if (!ship.gameObject.activeInHierarchy)
                        ship.gameObject.SetActive(true);
                    ship.ResetTurnState();
                }
                else
                {
                    if (ship.gameObject.activeInHierarchy)
                        ship.ResetTurnState();
                }
            }
        }
        TilesController[] allTiles = Resources.FindObjectsOfTypeAll<TilesController>();
        foreach (TilesController tile in allTiles)
        {
            tile.ResetTiles();
        }
        foreach (SpecialTileState sts in recordedSpecialTiles)
        {
            if (sts.tile != null)
            {
                sts.tile.gameObject.SetActive(true);
                sts.tile.transform.position = sts.position;
                sts.tile.SetTileType(sts.tileType);
                sts.tile.ResetTiles();
            }
        }
    }
}
