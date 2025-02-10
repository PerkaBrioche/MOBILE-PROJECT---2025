using UnityEngine;

public class TilesManager : MonoBehaviour
{
    
    private TilesController tilesController;

    public void SetTiles(TilesController tilesController)
    {
        this.tilesController = tilesController;
    }
    
    public TilesController GetTiles()
    {
        return tilesController;
    }
}
