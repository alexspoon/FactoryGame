using Godot;

public partial class FPSCounter : Label
{
     [Export] private int MaxFPS;
     
     public override void _Ready()
     {
          Engine.MaxFps = MaxFPS;
     }

     public override void _Process(double delta)
     {
          Text = "FPS: " + Engine.GetFramesPerSecond();
     }
}
