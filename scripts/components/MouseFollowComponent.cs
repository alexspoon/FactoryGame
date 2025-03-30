using Godot;
using System;

public partial class MouseFollowComponent : Node
{
    private Camera2D Camera;
    private Node2D Parent;
    private GpuParticles2D MouseParticles;
    
    public override void _Ready()
    {
        Parent = GetParent<Node2D>();
        Camera = GetTree().GetRoot().GetNode<Camera2D>("Main/Player/Camera");
        MouseParticles = Parent.GetNode<GpuParticles2D>("Particles/MouseParticles");
        Input.MouseMode = Input.MouseModeEnum.Hidden;
    }

    public override void _PhysicsProcess(double delta)
    {
        var mousePos = Camera.GetGlobalMousePosition();
        MouseParticles.GlobalPosition = mousePos;
        Parent.GlobalPosition = mousePos;
    }
}
