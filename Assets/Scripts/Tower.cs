using UnityEngine;

public class Tower : MonoBehaviour
{
    public bool Placed { get; private set; }
    public BoundsInt Area => _area;

    [SerializeField] private BoundsInt _area;
}