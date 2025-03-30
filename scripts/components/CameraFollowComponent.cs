using Godot;

public partial class CameraFollowComponent : Node
{
    [Export] private bool _followPlayer = true;
    private Camera2D _parent;
    private RigidBody2D _player;
    private Control _playerUI;
    private Label _fpsLabel;
    
    public override void _Ready()
    {
        _parent = GetParent() as Camera2D;
        _player = _parent.GetParent<RigidBody2D>();
        _playerUI = _player.GetNode<Control>("UILayer/UI");
        _fpsLabel = _playerUI.GetNode<Label>("FPS");
    }

    public override void _PhysicsProcess(double delta)
    {
        _fpsLabel.Text = "" + Engine.GetFramesPerSecond();
        HandleZoom();

        if (_followPlayer) FollowPlayer();
        else FollowMouse();
    }

    private void FollowPlayer()
    {
        _parent.GlobalPosition = _player.GlobalPosition;
    }

    private void FollowMouse()
    {
        _parent.GlobalPosition = _parent.GetGlobalMousePosition();
    }
    
    private void HandleZoom(){
        Vector2 zoom = _parent.Zoom.Clamp(1, 10);
        
        if (Input.IsActionJustPressed("ScrollUp")){
            zoom += new Vector2(0.25f, 0.25f);
        }

        if (Input.IsActionJustPressed("ScrollDown")){
            zoom -= new Vector2(0.25f, 0.25f);
        }

        if (Input.IsActionJustPressed("MiddleClick")){
            zoom = new Vector2(2f, 2f);
        }
        
        // _playerUI.Scale = zoom.Inverse() * 2;
        _parent.Zoom = zoom;
    }
}
