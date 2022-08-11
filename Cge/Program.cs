namespace ConSge;

public class TestGame : ConSge
{
    float pos = 0;
    public TestGame() : base("TestGame", 128, 64, 10, 10)
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
            pos += deltaTime * 50.0f;
            if (Width * Height < pos) pos = 0;
        }

        DrawPixel((int)pos, 0, 0x0E);
        return true;
    }
}

public class Program
{
    static void Main()
    {
        var game = new TestGame();
        game.Run();
    }
}