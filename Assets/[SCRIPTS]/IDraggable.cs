using UnityEngine;

public interface IDraggable
{
    void OnBeginDrag();
    void OnDrag(Vector3 position);
    void OnEndDrag();
}