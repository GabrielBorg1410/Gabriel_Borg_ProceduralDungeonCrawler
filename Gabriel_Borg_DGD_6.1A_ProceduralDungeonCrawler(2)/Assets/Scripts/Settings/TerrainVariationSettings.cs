using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "TerrainVariationSettings", menuName = "PCG/Terrain Variation Settings")]
public class TerrainVariationSettings : ScriptableObject
{
    [Header("Noise Settings")]
    [SerializeField] private float noiseScale = 0.1f;

    [Header("Terrain Tiles")]
    [SerializeField] private TileBase stoneTile;
    [SerializeField] private TileBase crackedTile;
    [SerializeField] private TileBase mossyTile;
    [SerializeField] private TileBase waterTile;
    [SerializeField] private TileBase lavaTile;

    [Header("Decoration Settings")]
    [SerializeField] private float decorationChance = 0.1f;


    [Header("Decoration Tiles")]
    [SerializeField] private TileBase bonesTile;
    [SerializeField] private TileBase cobwebTile;
    [SerializeField] private TileBase rubbleTile;
    [SerializeField] private TileBase crackDecorationTile;
    [SerializeField] private TileBase torchTile;

    [Header("Decoration Chances")]
    [SerializeField] private float floorDecorationChance = 0.08f;
    [SerializeField] private float wallDecorationChance = 0.12f;

    [Header("Terrain-Based Gameplay Effects")]
    [SerializeField] private float lavaDamage = 10f;
    [SerializeField] private float waterSlowAmount = 0.2f;

    //Using public getters to make the tiles accessible
    public float NoiseScale => noiseScale;

    public TileBase StoneTile => stoneTile;
    public TileBase CrackedTile => crackedTile;
    public TileBase MossyTile => mossyTile;
    public TileBase WaterTile => waterTile;
    public TileBase LavaTile => lavaTile;

    public TileBase BonesTile => bonesTile;
    public TileBase CobwebTile => cobwebTile;
    public TileBase RubbleTile => rubbleTile;
    public TileBase CrackDecorationTile => crackDecorationTile;
    public TileBase TorchTile => torchTile;

    public float FloorDecorationChance => floorDecorationChance;
    public float WallDecorationChance => wallDecorationChance;

    public float DecorationChance => decorationChance;
    public float LavaDamage => lavaDamage;
    public float WaterSlowAmount => waterSlowAmount;
}