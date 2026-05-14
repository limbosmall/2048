using Godot;
using System;

public partial class SettingsScene : MarginContainer
{
	[Export] Label TargetNum;
	[Export] Label BoardSize;
	[Export] HSlider DiffSlider;
	[Export] Button BackBtn; 
	
	public MainMenu menu;
	public override void _Ready()
	{
		Vector2 screen = GetViewport().GetVisibleRect().Size;
		int width = (int)ProjectSettings.GetSetting("display/window/size/viewport_width");
		int height = (int)ProjectSettings.GetSetting("display/window/size/viewport_height");
		if (screen.X != width || screen.Y != height) CallDeferred(nameof(ApplyUiScale), screen, width, height);

		UpdateLabel();
		DiffSlider.ValueChanged += (double value) => SetUpdatedValue((int)value);
		BackBtn.ButtonUp += SelfDel;
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
		float s = Mathf.Max(screenFrom.X / widthTo, screenFrom.Y / heightTo);

		TargetNum.AddThemeConstantOverride("font_size", (int)(TargetNum.GetThemeConstant("font_size") * s));
		BoardSize.AddThemeConstantOverride("font_size", (int)(BoardSize.GetThemeConstant("font_size") * s));
		BackBtn.AddThemeConstantOverride("font_size", (int)(BackBtn.GetThemeConstant("font_size") * s));
	}
	public void SetUpdatedValue(int value)
	{
		int brdsize;
		int trgVal = (int)Math.Pow(2, value);
		if (trgVal <= 4096) brdsize = 4;
		else if (trgVal <= 32768) brdsize = 5;
		else if (trgVal <= 131072) brdsize = 6;
		else if (trgVal <= 524288) brdsize = 7;
		else brdsize = 8; 
		GlobalSettings.Instance.TargetValue = trgVal;
		GlobalSettings.Instance.BoardSize = brdsize;
		UpdateLabel();
	}

	public async void SelfDel()
	{
		if (menu != null) menu.ToggleControl(true);
		Tween tween = CreateTween();
		tween.TweenProperty(this, "global_position", new Vector2(this.GlobalPosition.X, 
		-GetViewport().GetVisibleRect().Size.Y), 0.3f).SetEase(Tween.EaseType.In);
		await ToSignal(tween, Tween.SignalName.Finished);
		this.QueueFree();
	}

	public void UpdateLabel()
	{
		int brdsize = GlobalSettings.Instance.BoardSize;
		TargetNum.Text = $"Target: {GlobalSettings.Instance.TargetValue.ToString()}";
		BoardSize.Text = $"Board Size: {brdsize} x {brdsize}";
	}

}
