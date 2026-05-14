#nullable enable
using Godot;
using System;

public partial class GameBoardAutoPlay : GameBoard
{
	private Timer? _autoPlayTimer;

public override void _Ready()
{
	_boardSize = 8;
	Random r = new Random();
	int col = new int[] { 0, 2, 3 }[new Random().Next(3)];
    GenerateBoard(col);
    SpawnNewCube();

    _autoPlayTimer = new Timer();
    _autoPlayTimer.WaitTime = 0.6f;
    _autoPlayTimer.Timeout += DoRandomMove;
    AddChild(_autoPlayTimer);
    _autoPlayTimer.Start();

}

private void DoRandomMove()
{
    var dirs = new[] {
        new Vector2I(-1, 0), new Vector2I(1, 0),
        new Vector2I(0, -1), new Vector2I(0, 1)
    };
    Move(dirs[new Random().Next(4)]);
}

protected override void GameOver()
	{
		ResetBoard();
	}
    protected override void WinFunc()
    {
        ResetBoard();
    }

// блокируем _Input в режиме автопилота
public override void _Input(InputEvent @event)
	{
		return;
	}
}
