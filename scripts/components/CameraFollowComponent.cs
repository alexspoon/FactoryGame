using Godot;
using System;

public partial class CameraFollowComponent : Node
{
    private Node2D Main;
    private Camera2D Parent;
    private Node2D PlayerMouse;
    private Vector2 CameraLerp;

    public override void _Ready()
    {
        Parent = GetParent() as Camera2D;
        Main = Parent.GetParent() as Node2D;
        PlayerMouse = Main.GetNode<Node2D>("PlayerMouse");
    }

    public override void _Process(double delta)
    {
        HandleInput();
    }

    private void HandleInput(){
        if (Input.IsActionJustPressed("ScrollUp")){
            Parent.Zoom += new Vector2(0.25f, 0.25f);
        }

        if (Input.IsActionJustPressed("ScrollDown")){
            Parent.Zoom -= new Vector2(0.25f, 0.25f);
        }

        if (Input.IsActionJustPressed("MiddleClick")){
            Parent.Zoom = new Vector2(1f, 1f);
        }

        if (Input.IsActionPressed("RightClick")){
            Parent.GlobalPosition = PlayerMouse.GlobalPosition;
        }
    }
}
