using System;
using System.Collections.Generic;

namespace SudokuSolver;

public class Board
{
    public bool Solved { get; set; }
    public List<Cell> Cells { get; set; }

    public Board(string data = null)
    {
        Cells = new List<Cell>();

        for (var y = 1; y < 10; y++)
        {
            for (var x = 1; x < 10; x++)
            {
                var v = 0;
                var c = data[((y - 1) * 9) + (x - 1)];
                if (data != null) v = int.Parse(c.ToString());
                var cell = new Cell(x, y, v);
                Cells.Add(cell);
            }
        }
    }

    Cell GetCell(int px, int py)
    {
        for (var i = 0; i < Cells.Count; i++)
        {
            if (Cells[i].X != px) continue;
            if (Cells[i].Y != py) continue;
            return Cells[i];
        }
        return null;
    }

    public Cell[] GetRow(int row)
    {
        var ret = new Cell[9];
        for (var i = 0; i < Cells.Count; i++)
        {
            if (Cells[i].Y != row) continue;
            ret[Cells[i].X - 1] = Cells[i];
        }
        return ret;
    }

    public Cell[] GetCol(int col)
    {
        var ret = new Cell[9];
        for (var i = 0; i < Cells.Count; i++)
        {
            if (Cells[i].X != col) continue;
            ret[Cells[i].Y - 1] = Cells[i];
        }
        return ret;
    }

    public Cell[] GetGroup(int group)
    {
        var ret = new List<Cell>();
        for (var i = 0; i < Cells.Count; i++)
        {
            if (Cells[i].Group != group) continue;
            ret.Add(Cells[i]);
        }
        return ret.ToArray();
    }

    public void Print()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("┌───────────────────────────────────┐");
        for (var y = 1; y < 10; y++)
        {
            for (var x = 1; x < 10; x++)
            {
                var cell = GetCell(x, y);

                Console.Write($"│ ");
                Console.ForegroundColor = cell.IsInitial ? ConsoleColor.White : ConsoleColor.Green;
                Console.Write($"{(cell.Value == 0 ? " " : cell.Value)} ");
                Console.ForegroundColor = ConsoleColor.White;
            }
            if (y < 9)
                Console.WriteLine("│\n┌───────────────────────────────────┐");
            else
                Console.WriteLine("│\n└───────────────────────────────────┘");
        }
    }

    public void Step()
    {
        for (var y = 1; y < 10; y++)
        {
            for (var x = 1; x < 10; x++)
            {
                var cell = GetCell(x, y);
                if (cell.Value > 0) continue;
                cell.InitPencilValues();

                var row = GetRow(y);
                foreach (var c in row) cell.PencilValues[c.Value] = false;

                var col = GetCol(x);
                foreach (var r in col) cell.PencilValues[r.Value] = false;

                var grp = GetGroup(cell.Group);
                foreach (var g in grp) cell.PencilValues[g.Value] = false;
            }
        }

        foreach (var cell in Cells)
        {
            if (cell.Value == 0 && cell.HasOnlyOnePossibility)
            {
                cell.Value = cell.GetPossibility();
                return;
            }
        }
    }
}
