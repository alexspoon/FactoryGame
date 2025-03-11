using Godot;

public partial class Smelter : StaticBody2D
{
    private Area2D SmeltArea;

    public override void _Ready()
    {
        SmeltArea = GetNode<Area2D>("SmeltArea");
    }

    public override void _PhysicsProcess(double delta)
    {
        
    }

    private void SmeltItems()
    {
        
    }
}
