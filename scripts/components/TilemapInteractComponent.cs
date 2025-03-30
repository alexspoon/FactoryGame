using Godot;

public partial class TilemapInteractComponent : Node
{
    private TileMap _tileMap;
    private Node2D _main;
    private Camera2D _camera;
    private bool _buildMode;
    [Export] private int _tileDamage;
    private enum _placeableTiles
    {
        Stone,
        Dirt,
        Grass
    }
    
    [Export] private _placeableTiles _selectedTile;
    private Sprite2D _tileHighlight;
    
    public override void _Ready()
    {
        _main = GetTree().GetRoot().GetNode<Node2D>("Main");
        _camera = _main.GetNode<Camera2D>("Player/Camera");
        _tileHighlight = GetParent().GetNode<Sprite2D>("TileHighlight");
        _tileMap = _main.GetNode<TileMap>("TileMap");
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Input.IsActionJustPressed("KeyE")) _buildMode = !_buildMode;

        _tileHighlight.Visible = _buildMode;
        _tileHighlight.GlobalPosition = _tileMap.ToGlobal(_tileMap.MapToLocal(_tileMap.LocalToMap(_camera.GetGlobalMousePosition())));
        if (_buildMode) BreakAndPlace();
    }
    
    private void BreakAndPlace()
    {
        var tileMousePos = _tileMap.LocalToMap(_camera.GetGlobalMousePosition());
        // if (_tileMap.GetCellSourceId(tileMousePos) == -1) return;
        
        if (Input.IsActionPressed("LeftClick"))
        {
            _tileMap.DamageTile(tileMousePos, _tileDamage);
        }
        if (Input.IsActionPressed("RightClick"))
        {
            _tileMap.PlaceTile(tileMousePos, _selectedTile.ToString());
        }
    }
}
