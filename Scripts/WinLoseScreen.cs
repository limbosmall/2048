using Godot;
using System;
using System.Threading.Tasks;

public partial class WinLoseScreen : MarginContainer
{
	[Export]
	public Label WinLoseLbl;
	[Export]
	Label ScoreLbl;
	[Export]
	Button PlayAgainBtn;
	[Export]
	Button MainMenuBtn;

	public GameBoard board;

	public override async void _Ready()
	{
		Vector2 screen = GetViewport().GetVisibleRect().Size;
		int width = (int)ProjectSettings.GetSetting("display/window/size/viewport_width");
		int height = (int)ProjectSettings.GetSetting("display/window/size/viewport_height");
		if (screen.X != width || screen.Y != height) CallDeferred(nameof(ApplyUiScale), screen, width, height);
		PlayAgainBtn.ButtonUp += PlayAgain;
		MainMenuBtn.ButtonUp += ToMenu;
		uint score = GlobalSettings.Instance.Score;
		for (int i = 0; i <= score; i += 10) 
		{
			ScoreLbl.Text = new string($"Score: {i}");
			float duration = 0.001f + (float)i / score * 0.02f;
			await ToSignal(GetTree().CreateTimer(duration), "timeout");
		}
	}
	public void ToMenu()
	{
		GetTree().ChangeSceneToFile("res://Scenes/MainMenu.tscn");
	}

	public async void PlayAgain()
	{
		if (board != null) 
		{
			board.ResetBoard();
			board.can_play = true;
		}
		Tween tween = CreateTween();
		tween.TweenProperty(this, "global_position", new Vector2(this.GlobalPosition.X, 
		-GetViewport().GetVisibleRect().Size.Y), 0.3f).SetEase(Tween.EaseType.In);
		await ToSignal(tween, Tween.SignalName.Finished);
		this.QueueFree();
	}

	private void ApplyUiScale(Vector2 screenFrom, int widthTo, int heightTo)
	{
		// Отступы чтобы контент занимал размер редактора по центру
		int marginH = (int)((screenFrom.X - widthTo) / 2f);
		int marginV = (int)((screenFrom.Y - heightTo) / 2f);
		
		this.AddThemeConstantOverride("margin_left", marginH + this.GetThemeConstant("margin_left"));
		this.AddThemeConstantOverride("margin_right", marginH + this.GetThemeConstant("margin_right"));
		this.AddThemeConstantOverride("margin_top", marginV + this.GetThemeConstant("margin_top"));
		this.AddThemeConstantOverride("margin_bottom", marginV + this.GetThemeConstant("margin_bottom"));
	}
}
