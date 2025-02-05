using UnityEngine;

public class ShipController : MonoBehaviour
{
    [SerializeField] private int right;
    [SerializeField] private int left;
    [SerializeField] private int up;
    [SerializeField] private int down;
    
    [SerializeField] private TilesController _tilesController;

    public void GetPath()
    {
        if(_tilesController == null)
        {
            Debug.LogError("TilesController is null");
            return;
        }
        
        _tilesController.GetUpTiles(up);
        
    }
    
    public void SetTiles(TilesController tilesController)
    {
        _tilesController = tilesController;
    }
    
    public TilesController GetTiles()
    {
        return _tilesController;
    }
}
