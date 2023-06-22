using UnityEngine;

public class Building : MonoBehaviour
{
    public bool Placed { get; private set; }
    public BoundsInt Area => _area;
    
    [SerializeField] private BoundsInt _area;

    public void ChangeAreaPosition(Vector3Int newPosition)
    {
        _area.position = newPosition;
    }
    
    public bool CanBePlaced()
    {
        var positionInt = GridBuildingService.Instance.GetCellPosition(transform.position);
        var areaTemp = _area;
        areaTemp.position = positionInt;

        if (GridBuildingService.Instance.CanTakeArea(areaTemp))
        {
            return true;
        }

        return false;
    }

    public void Place()
    {
        var positionInt = GridBuildingService.Instance.GetCellPosition(transform.position);
        var areaTemp = _area;
        areaTemp.position = positionInt;
        Placed = true;
        GridBuildingService.Instance.TakeArea(areaTemp);
    }
}