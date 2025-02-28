using Godot;
using System;

public partial class CameraFollowComponent : Node
{
    private Node2D Main;
    private Camera2D Parent;
    private Node2D PlayerMouse;

    public override void _Ready()
    {
        Parent = GetParent() as Camera2D;
        Main = Parent.GetParent() as Node2D;
        PlayerMouse = Main.GetNode<Node2D>("PlayerMouse");
    }

    public override void _Process(double delta)
    {
        Parent.GlobalPosition = PlayerMouse.GlobalPosition;
    }
}
