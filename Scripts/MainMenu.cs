using Godot;
using System;

public partial class MainMenu : Control
{
	[Export] Button NewGameBtn;
	[Export] Button SettingsBtn;
	[Export] Button QuitBtn;
	[Export]
	private PackedScene SettingsScene;

	public void ToggleControl(bool is_can)
	{
		NewGameBtn.Disabled = is_can;
		SettingsBtn.Disabled = is_can;
		QuitBtn.Disabled = is_can;
	}

    public override void _Ready()
    {
        NewGameBtn.Pressed += OnStartGamePressed;
		SettingsBtn.Pressed += OnSettingsPressed;
		QuitBtn.Pressed += OnQuitPressed;
    }

	public void OnStartGamePressed()
	{
		GetTree().ChangeSceneToFile("res://Scenes/game_board.tscn");
	}

	public void OnSettingsPressed()
	{
		var sets = SettingsScene.Instantiate<SettingsScene>();
		AddChild(sets);
		sets.GlobalPosition = new Vector2(this.GlobalPosition.X, 0-GetViewport().GetVisibleRect().Size.Y);
		Tween tween = CreateTween();
		tween.TweenProperty(sets, "global_position", new Vector2(this.GlobalPosition.X,0), 0.3f);
		ToggleControl(false);
	}

	public void OnQuitPressed()
	{
		GetTree().Quit();
	}

}
