using Godot;
using System;

public partial class GlobalSettings : Node
{
	public static GlobalSettings Instance {get; private set;}
    // public Vector2 UiScale { get; private set; }
	public int BoardSize;
    public int TargetValue;
    public uint Score = 0;

    // public void CalculateUiScale()
    // {
    //     Vector2 screen = DisplayServer.ScreenGetSize();
    //     Vector2 baseSize = new Vector2(1152, 648);
    //     UiScale = screen / baseSize;
    // }
    public override void _Ready()
    {
        BoardSize = 4;
        TargetValue = 2048;
        Instance = this;
    }

}
