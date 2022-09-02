namespace SudokuSolver;

public class Cell
{
    public int X { get; set; }
    public int Y { get; set; }

    public int Value { get; set; }
    public bool IsInitial { get; set; }
    public bool[] PencilValues { get; set; }

    public void InitPencilValues()
    {
        for (var i = 0; i < PencilValues.Length; i++)
            PencilValues[i] = true;
    }

    public bool HasOnlyOnePossibility
    {
        get
        {
            var x = 0;
            foreach (var value in PencilValues)
                if (value) x++;
            return x == 1;
        }
    }

    public int GetPossibility()
    {
        for (var i = 0; i < PencilValues.Length; i++)
        {
            if (!PencilValues[i]) continue;
            return i;
        }
        return 0;
    }

    public int Group
    {
        get
        {
            var g = 1;
            if (((float)X / 3) > 1) g++;
            if (((float)X / 3) > 2) g++;
            if (((float)Y / 3) > 1) g += 3;
            if (((float)Y / 3) > 2) g += 3;
            return g;
        }
    }

    public Cell(int x, int y, int value)
    {
        PencilValues = new bool[10];
        X = x;
        Y = y;
        Value = value;
        IsInitial = value > 0;
    }
}
