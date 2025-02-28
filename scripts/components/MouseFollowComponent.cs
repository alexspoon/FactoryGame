using Godot;
using System;

public partial class MouseFollowComponent : Node
{
    private Camera2D Camera;
    private Node2D Parent;
    
    public override void _Ready()
    {
        Parent = GetParent() as Node2D;
        Camera = GetTree().GetRoot().GetNode("Main/Camera") as Camera2D;
        Input.MouseMode = Input.MouseModeEnum.Hidden;
    }

    public override void _Process(double delta)
    {
        Parent.GlobalPosition = Parent.GlobalPosition.Lerp(Camera.GetGlobalMousePosition(), 20f * (float)delta);
    }
}
