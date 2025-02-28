using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using Random = UnityEngine.Random;

public class GridController : MonoBehaviour
{
    [Header("GRID LAYOUT")]
    [SerializeField] private int rows;
    [SerializeField] private int cols;
    [SerializeField] private float tileSize;
    [SerializeField] private float tileSpacing;

    [Space(20)] 
    [Header("COLOR CHANGE")]

    private List<Color> Colors = new List<Color>(new Color[]
    {
        Color.red, Color.blue, Color.green, Color.yellow, Color.magenta
    });
    
    [SerializeField] private float secondsToBeat;

    [SerializeField] private bool _canChangeColor;
    private bool _isChangingColor;

    [SerializeField] private List<TilesController> _tilesControllers;

    private void Start()
    {
        FindAllTiles();
        ReArangeGrid();
    }

    private void Update()
    {
        if (_canChangeColor)
        {
            if (!_isChangingColor)
            {
                _isChangingColor = true;
                StartCoroutine(ChangeTilesCooldown());
            }
        }
    }

    [Button ("ARRANGE GRID")]
    private void ReArangeGrid()
    {
        int childCount = transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            Transform child = transform.GetChild(i);
            int row = i / cols;
            int col = i % cols;
            float x = col * (tileSize + tileSpacing);
            float y = -row * (tileSize + tileSpacing);
            child.localPosition = new Vector2(x, y);
            if(child.TryGetComponent(out TilesController tc))
            {
                tc.SetColumAndRowPosition( col, row);
            }
        }
        
        // CHANGE GRID POSITION ??
        return;
        float gridW = cols * tileSize;
        float gridH = rows * tileSize;
        transform.position = new Vector2(-gridW / 2 + tileSize / 2, gridH / 2 - tileSize / 2);
    }

    private void OnValidate()
    {
        ReArangeGrid();
        if(secondsToBeat < 1)
        {
            secondsToBeat = 1;
        }
        
    }
    
    [Button ("FIND TILES")]
    private void FindAllTiles()
    {
        _tilesControllers.Clear();
        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).TryGetComponent(out TilesController tc))
            {
                _tilesControllers.Add(tc);
            }
            else
            {
                Debug.LogError("DON'T WORK");
            }
        }
    }

    private IEnumerator ChangeTilesCooldown()
    {
        ChangeColor();
        yield return new WaitForSeconds(secondsToBeat);
        _isChangingColor = false;
    }

    public void ChangeColor()
    {
        foreach (var tiles in _tilesControllers)
        {
            tiles.ChangeTileSprite(TilesController.enumTileSprites.defaultTile);
        }
    }

    public void ShineEffect()
    {
        
    }

    public void ResetAllTiles(bool noBounce = false)
    {
        foreach (var tiles in _tilesControllers)
        {
            tiles.ResetTiles(noBounce);
        }
        print("END TILES RESET");
    } 

    
    
    
    
    
    
}
