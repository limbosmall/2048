using Godot;
using System;
using System.Collections.Generic;

public partial class GameBoard : Node2D
{
	[Export]
	private TileMapLayer tilemap;
	[Export]
	private PackedScene numberCubeScene;

	private Vector2I _startCell;
	protected int _boardSize;
	protected NumberCube[,] Cubes;
	public bool can_play = true;

	public override void _Ready()
	{
		_boardSize = GlobalSettings.Instance.BoardSize;
		Random r = new Random();
		int col = r.Next(0, 3);
		GenerateBoard(col);
		SpawnNewCube();
	}

	public void GenerateBoard(int color)
	{
		Vector2 viewportSize = GetViewport().GetVisibleRect().Size;

		int TilesX = (int)(viewportSize.X / 64);
		int TilesY = (int)(viewportSize.Y / 64);
		_startCell = new Vector2I(
			(int)((TilesX - _boardSize)/2), 
			(int)((TilesY - _boardSize)/2));

		for (int k = 0; k < TilesX+1; k++)
		{
			for (int l = 0; l < TilesY+1; l++)
			{
				tilemap.SetCell(new Vector2I(k, l), 0, new Vector2I(color, 1));
			}
		}
		for (int i = 0; i < _boardSize; i++)
		{
			for (int j = 0; j < _boardSize; j++)
			{
				tilemap.SetCell(_startCell + new Vector2I(i,j), 0, new Vector2I(color,0));
			}
		}
		Cubes = new NumberCube[_boardSize, _boardSize];
	}
	private void CollapseLine(int lineIdx, Vector2I dir)
	{
		bool movingPositive = (dir.X + dir.Y > 0);
		int step = movingPositive ? -1 : 1;
		int start = movingPositive ? _boardSize - 1 : 0;

		// собираем ненулевые кубы в порядке от стены
		var line = new List<NumberCube>();
		for (int i = start; i >= 0 && i < _boardSize; i += step)
		{
			NumberCube cube = GetCube(lineIdx, i, dir);
			if (cube != null) line.Add(cube);
		}

		// мержим соседей в списке
		for (int k = 0; k < line.Count - 1; k++)
		{
			if (line[k].curNumber == line[k + 1].curNumber)
			{
				line[k].curNumber *= 2;
				GlobalSettings.Instance.Score += line[k].curNumber * 2;
				if (line[k].curNumber == GlobalSettings.Instance.TargetValue) WinFunc();
				line[k].UpdateLabel();
				
				// анимация слияния
				// Vector2I targetCoord = dir.X != 0
				// 	? _startCell + new Vector2I(start + k * step, lineIdx)
				// 	: _startCell + new Vector2I(lineIdx, start + k * step);
				// Vector2 worldPos = tilemap.ToGlobal(tilemap.MapToLocal(targetCoord));
				Color mergedColor = GetColorForValue((int)line[k].curNumber);
				
				line[k + 1].MoveToCell(line[k].GlobalPosition, 0.15f);
				line[k + 1].PlayMergeDissolve(new Vector2(dir.X, dir.Y), mergedColor, 0.15f);
				line[k].PlayMergeColorShift(mergedColor, 0.15f);
				
				line.RemoveAt(k + 1);
				k++; // пропускаем следующий, он уже смержен
			}
		}

		// расставляем кубы по массиву и анимируем движение
		// Очищаем линию
		for (int i = 0; i < _boardSize; i++)
			SetCube(lineIdx, i, dir, null);

		for (int k = 0; k < line.Count; k++)
		{
			int pos = start + k * step;
			SetCube(lineIdx, pos, dir, line[k]);

			Vector2I tileCoord = dir.X != 0
				? _startCell + new Vector2I(pos, lineIdx)
				: _startCell + new Vector2I(lineIdx, pos);
			Vector2 worldPos = tilemap.ToGlobal(tilemap.MapToLocal(tileCoord));
			line[k].MoveToCell(worldPos, 0.15f);
		}
	}

		// Абстрагируем доступ к Cubes[,] через направление
	protected NumberCube GetCube(int lineIdx, int pos, Vector2I dir)
	{
	    return dir.X != 0 
	        ? Cubes[lineIdx, pos]   // горизонталь: lineIdx = строка
	        : Cubes[pos, lineIdx];  // вертикаль:   lineIdx = столбец
	}

	private void SetCube(int lineIdx, int pos, Vector2I dir, NumberCube cube)
	{
	    if (dir.X != 0) Cubes[lineIdx, pos] = cube;
	    else            Cubes[pos, lineIdx] = cube;
	}

	private Color GetColorForValue(int value)
	{
		return value switch
		{
			2      => new Color("cdc1b4"), // бежевый
			4      => new Color("eee4da"),
			8      => new Color("f2b179"), // персиковый
			16     => new Color("f59563"),
			32     => new Color("f67c5f"),
			64     => new Color("f65e3b"), // оранжевый
			128    => new Color("edcf72"), // золотой
			256    => new Color("edcc61"),
			512    => new Color("edc850"),
			1024   => new Color("edc53f"),
			2048   => new Color("edc22e"), // насыщенное золото
			4096   => new Color("b5ead7"), // мятный
			8192   => new Color("6bcba5"),
			16384  => new Color("3aaf85"), // зелёный
			32768  => new Color("5bc8f5"), // голубой
			65536  => new Color("4aa8e0"),
			131072 => new Color("6a7ff5"), // синий
			262144 => new Color("9b6af5"), // фиолетовый
			524288 => new Color("d46af5"), // пурпурный
			1048576=> new Color("f56a9b"), // розовый — 2^20
			_      => new Color("ff4444")  // красный для > 2^20
		};
	}

	public void Move(Vector2I direction)
	{
		for (int i = 0; i < _boardSize; i++)
			CollapseLine(i, direction);

		SpawnNewCube(); // вызывается сразу, массив Cubes уже обновлён
	}
	protected void SpawnNewCube()
	{
		// Собираем все пустые клетки
		var emptyCells = new List<Vector2I>();
		int size = _boardSize;

		for (int i = 0; i < size; i++)
			for (int j = 0; j < size; j++)
				if (Cubes[i, j] == null)
					emptyCells.Add(new Vector2I(i, j));

		if (emptyCells.Count == 0)
		{
			GameOver();
			return; 
		}

		var rng = new Random();
		Vector2I cell = emptyCells[rng.Next(emptyCells.Count)];

		var cube = numberCubeScene.Instantiate<NumberCube>();
		AddChild(cube);

		// Позиция
		Vector2I tileCoord = _startCell + new Vector2I(cell.Y, cell.X);
		cube.GlobalPosition = tilemap.ToGlobal(tilemap.MapToLocal(tileCoord));

		// Значение: 90% шанс 2, 10% шанс 4
		uint value = rng.Next(10) < 9 ? 2u : 4u;
		cube.SetValue(value, GetColorForValue((int)value));

		// Анимация появления
		cube.Scale = Vector2.Zero;
		Tween tween = cube.CreateTween();
		tween.TweenProperty(cube, "scale", Vector2.One * 1.1f, 0.1f)
			.SetEase(Tween.EaseType.Out);
		tween.TweenProperty(cube, "scale", Vector2.One, 0.08f)
			.SetEase(Tween.EaseType.In);

		Cubes[cell.X, cell.Y] = cube;
	}

	protected virtual void GameOver()
	{
		InitialiseWinLose(false);
	}

	protected virtual void WinFunc()
	{
		InitialiseWinLose(true);
	}


	public void InitialiseWinLose(bool wonlose)
	{
		PackedScene scene = GD.Load<PackedScene>("res://Scenes/WinLoseScreen.tscn");
		WinLoseScreen winlosescr = (WinLoseScreen)scene.Instantiate();
		winlosescr.board = this;
		GetTree().Root.AddChild(winlosescr); 
		winlosescr.WinLoseLbl.Text = wonlose ? "You Won!" : "You Lose!";
		can_play = false;
		Vector2 screen = GetViewport().GetVisibleRect().Size;
		winlosescr.GlobalPosition = new Vector2(0, -screen.Y);
		Tween tween = CreateTween();
		tween.TweenProperty(winlosescr, "global_position", Vector2.Zero, 0.3f)
			.SetEase(Tween.EaseType.Out)
			.SetTrans(Tween.TransitionType.Quart);
	}
	public void ResetBoard()
	{
		for (int i = 0; i < _boardSize; i++)
		{
			for (int j = 0; j < _boardSize; j++)
			{
				NumberCube cube = Cubes[i, j];
				if (cube != null)
				{
					Cubes[i, j] = null;
					cube.QueueFree();
				}
			}
		}
		Random r = new Random();
		int col = r.Next(0, 3);
		GenerateBoard(col);
		SpawnNewCube();
	}
    public override void _Input(InputEvent @event)
    {
		if (can_play)
		{
			if (@event.IsActionReleased("ui_accept")) GameOver();
			if (@event.IsActionReleased("left")) Move(new Vector2I(-1, 0));
			else if (@event.IsActionReleased("right")) Move(new Vector2I(1, 0));
			else if (@event.IsActionReleased("up")) Move(new Vector2I(0, -1));
			else if (@event.IsActionReleased("down")) Move(new Vector2I(0, 1));
		}
    }

}