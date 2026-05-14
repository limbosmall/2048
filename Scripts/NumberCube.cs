using Godot;
using System;

public partial class NumberCube : CharacterBody2D
{
	[Export]
	private Label number;
	[Export]
	public Sprite2D sprite;

	public uint curNumber;

public void UpdateLabel()
	{
		number.Text = curNumber.ToString();
	}

	public void MoveToCell(Vector2 targetPos, float duration)
	{
		Tween tween = CreateTween();
		tween.TweenProperty(this, "global_position", targetPos, duration)
			.SetEase(Tween.EaseType.Out)
			.SetTrans(Tween.TransitionType.Quart);
	}

	// Pop-эффект для куба, который входит при слиянии
	public void PlayMergeDissolve(Vector2 dissolveDir, Color targetColor, float duration)
{
    Tween tween = CreateTween();
    tween.SetParallel(true);

    // Нарастающая прозрачность
    tween.TweenProperty(this, "modulate", new Color(1,1,1,0), duration);
    // Цвет спрайта -> цвет итогового куба
    tween.TweenProperty(sprite, "modulate", targetColor, duration)
        .SetEase(Tween.EaseType.In);

    tween.SetParallel(false);
    tween.TweenCallback(Callable.From(() => QueueFree()));
}

public void PlayMergeColorShift(Color targetColor, float duration)
	{
		// Для куба-получателя — просто смена цвета + pop
		Tween tween = CreateTween();
		tween.SetParallel(true);

		tween.TweenProperty(sprite, "modulate", targetColor, duration)
			.SetEase(Tween.EaseType.Out);

		// Pop по scale
		tween.TweenProperty(this, "scale", Vector2.One * 1.25f, duration * 0.5f)
			.SetEase(Tween.EaseType.Out);

		tween.SetParallel(false);
		tween.TweenProperty(this, "scale", Vector2.One, duration * 0.5f)
			.SetEase(Tween.EaseType.In);
	}
	public void SetValue(uint value, Color color)
	{
		curNumber = value;
		number.Text = value.ToString();
		sprite.Modulate = color;
	}
}
