using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class SetMeshLayer : MonoBehaviour 
{
    [SerializeField]
    public enum SortingLayers
    {
        Default,
        BackGround,
        Enemy,
        Player,
        Effect
    }

    public SortingLayers SortingLayer = SortingLayers.Effect;
    public int OrderInLayer = 0;

    private MeshRenderer _render;

    private void Awake()
    {
        _render = GetComponent<MeshRenderer>();
        _render.sortingLayerName = SortingLayer.ToString();
        _render.sortingOrder = OrderInLayer;
    }
}