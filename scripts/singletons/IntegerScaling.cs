using System;
using Godot;

public partial class IntegerScaling : Node2D
{
    private Window _window;
    private Vector2I _windowBaseSize;

    public override void _Ready()
    {
        _window = GetWindow();
        _windowBaseSize = _window.ContentScaleSize;
        
        _window.SizeChanged += WindowSizeChange;
    }

    private void WindowSizeChange()
    {
        var scale = _window.Size / _windowBaseSize;
        _window.ContentScaleSize = _window.Size / Math.Min(scale.Y, scale.X);
    }
}
