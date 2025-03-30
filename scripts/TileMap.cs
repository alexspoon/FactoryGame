using Godot;
using Godot.Collections;

public partial class TileMap : TileMapLayer
{
    public Array<Vector2I> UsedTiles = [];
    public Dictionary<Vector2I, int> TileHealthValues = [];
    public Dictionary<string, Vector2I> TileTypes = new Dictionary<string, Vector2I>()
    {
        {"Stone", new Vector2I(0,0)},
        {"Dirt", new Vector2I(1,0)},
        {"Grass", new Vector2I(2,0)}
    };

    [Export] private Array<PackedScene> RigidTiles;
    [Export] private NoiseTexture2D ProceduralNoiseTexture;
    [Export] private uint ProceduralNoiseSeed;
    private RandomNumberGenerator RNG =  new RandomNumberGenerator();
    private Noise GeneratedNoise;
    [Export] private int Width;
    [Export] private int Height;
    private ColorRect Background;

    
    
    public override void _Ready()
    {
        Background = GetParent().GetNode<ColorRect>("Background");
        GeneratedNoise = ProceduralNoiseTexture.Noise;
        
        GenerateWorld();
    }

    private void GenerateWorld()
    {
        var bgSize = Background.Size;
        bgSize.X = Width * 16;
        bgSize.Y = Height * 16;
        Background.SetSize(bgSize);
        
        RNG.Randomize();
        ProceduralNoiseSeed = RNG.Randi();
        GeneratedNoise.Set("seed",  ProceduralNoiseSeed);
        GeneratedNoise.Set("width", Width);
        GeneratedNoise.Set("height", Height);
        UsedTiles.Clear();
        TileHealthValues.Clear();
        
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                EraseCell(new  Vector2I(x, y));
                
                var noiseValue = GeneratedNoise.GetNoise2D(x, y);
                
                if (noiseValue < -0.25)
                {
                    SetCell(new  Vector2I(x, y), 0, TileTypes["Stone"]);
                }
                else if (noiseValue < -0.15f)
                {
                    SetCell(new  Vector2I(x, y), 0, TileTypes["Dirt"]);
                }
                else if (noiseValue < -0.05f)
                {
                    SetCell(new  Vector2I(x, y), 0, TileTypes["Grass"]);
                }
                
                if (x == Width-1 || x == 0 || y == Height-1 || y == 0) SetCell(new  Vector2I(x, y), 0, TileTypes["Stone"]);
            }
        }
        
        UsedTiles = GetUsedCells();
        GD.Print("Amount of tiles: " + UsedTiles.Count);
        InitializeExistingTiles();
    }
    
    public override void _PhysicsProcess(double delta)
    {
        if (Input.IsActionJustPressed("ui_up"))
        {
            GenerateWorld();
        }
    }
    
    private void InitializeExistingTiles()
    {
        foreach (var tile in UsedTiles)
        {
            var tileType = CheckExistingType(tile);
            var tileHealth = CheckInitialHealth(tileType);
            TileHealthValues.Add(tile, tileHealth);
        }
        
        GD.Print("Amount of tiles with health value: " + TileHealthValues.Count);
    }

    private void CheckAllTilesHealth()
    {
        foreach (var tile in UsedTiles)
        {
            var tileHealth = TileHealthValues[tile];
            if  (tileHealth <= 0) BreakTile(tile);
        }
    }
    
    public void DamageTile(Vector2I tile, int damage)
    {
        if (GetCellSourceId(tile) == -1) return;
        var tileHealth = TileHealthValues[tile];
        GD.Print(tileHealth);
        tileHealth -= damage;
        TileHealthValues[tile] = tileHealth;
        GD.Print(tileHealth);
        if  (tileHealth <= 0) BreakTile(tile);
    }

    public void PlaceTile(Vector2I tile, string tileType)
    {
        if  (GetCellSourceId(tile) != -1) return;
        InitializeTile(tile, tileType);
        SetCell(tile, 0, TileTypes[tileType]);
    }

    private void InitializeTile(Vector2I tile, string tileType)
    {
        var tileHealth = CheckInitialHealth(tileType);
        UsedTiles.Add(tile);
        TileHealthValues.Add(tile, tileHealth);
    }
    
    private void BreakTile(Vector2I tile)
    {
        var tileType = CheckExistingType(tile);
        var rigidTileIdx = TileTypes[tileType].X;
        TileHealthValues.Remove(tile);
        UsedTiles.Remove(tile);
        EraseCell(tile);
        var rigidTile = RigidTiles[rigidTileIdx].Instantiate() as RigidBody2D;
        rigidTile.GlobalPosition = ToGlobal(MapToLocal(tile));
        AddSibling(rigidTile);
    }
    
    private string CheckExistingType(Vector2I tile)
    {
        var tileData = GetCellTileData(tile);
        if (tileData == null) return null;
        if (!tileData.HasCustomData("TileType")) return null;
        var tileType = (string)tileData.GetCustomData("TileType");
        return tileType;
    }

    private int CheckInitialHealth(string tileType)
    {
        switch (tileType)
        {
            case "Stone" : return 10;
            case "Dirt" : return 5;
            case "Grass" : return 2;
        }
        return 0;
    }
}
