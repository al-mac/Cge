namespace CgeGames.Games.TestGame;

public class SudokuSolver : Cge
{
    float pos = 0;
    public SudokuSolver() : base("TestGame", 80, 40, 16, 16)
    {
    }

    protected override bool OnCreate()
    {
        return true;
    }

    protected override bool OnUpdate(float deltaTime)
    {
        ClearScreen();
        DrawText(20, 0, 0x4F, "Example game. Hold SPACEBAR to move the pixel");

        if (KeyHeld(32)) // SPACEBAR Key
        {
            pos += deltaTime * 20.0f;
            if (Width * Height < pos) pos = 0;
        }

        DrawPixel((int)pos, 0, 0x0E);
        return true;
    }
}