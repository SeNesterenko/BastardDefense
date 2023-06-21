using UnityEngine;

public class Building : MonoBehaviour
{
    public bool Placed { get; private set; }
    public BoundsInt area;

    public bool CanBePlaced()
    {
        var positionInt = GridBuildingService.Instance.GetCellPosition(transform.position);
        var areaTemp = area;
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
        var areaTemp = area;
        areaTemp.position = positionInt;
        Placed = true;
        GridBuildingService.Instance.TakeArea(areaTemp);
    }
}