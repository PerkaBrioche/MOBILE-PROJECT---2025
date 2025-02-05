using UnityEngine;

public class DraggableUnit : MonoBehaviour, IDraggable
{
    public void OnBeginDrag()
    {
        Debug.Log("Début du drag sur " + gameObject.name);
    }

    public void OnDrag(Vector3 position)
    {
        transform.position = position;
    }

    public void OnEndDrag()
    {
        Debug.Log("Fin du drag sur " + gameObject.name);
    }
}