using System;

namespace CgeGames.Games.Pong;

public class Pong : Cge
{
    static Random _rand = new(DateTime.Now.Millisecond);

    #region Classes
    class Vec
    {
        public float X { get; set; }
        public float Y { get; set; }
        public Vec(float x, float y)
        {
            X = x;
            Y = y;
        }
    }
    class Rect : Vec
    {
        public int W { get; set; }
        public int H { get; set; }
        public int B => (int)Y + H;
        public int R => (int)X + W;
        public Rect(float x, float y, int w, int h) : base(x, y)
        {
            W = w;
            H = h;
        }
    }
    class Sprite
    {
        public short Color { get; set; } = 0x0F;
        public Rect Rect { get; set; }
        public Sprite(short color, Rect rect)
        {
            Color = color;
            Rect = rect;
        }
        public void Move(float x, float y)
        {
            Rect.X += x;
            Rect.Y += y;
        }
    }
    #endregion

    const float _paddleSpeed = 60.0f;
    readonly int[] _scores = new int[2];

    Sprite _ball;
    readonly Vec _ballSpeed = new(0, 0);
    Sprite _player;
    Sprite _cpu;
    Sprite _middle;

    public Pong() : base("Pong", 120, 60, 11, 11) { }

    protected override bool OnCreate()
    {
        _middle = new(0x0F, new(MiddleX(1), 0, 1, Height));
        _player = new(0x0B, new(2, MiddleY(12), 3, 12));
        _cpu = new(0x0A, new(Width - 3 - 2, MiddleY(12), 3, 12));
        _ball = new(0x01, new(MiddleX(3), MiddleY(3), 3, 3));
        ResetBall(false);
        return true;
    }

    void ResetBall(bool updateScore)
    {
        if (updateScore) _scores[_ball.Rect.X < 0 ? 1 : 0]++;
        _ball.Color = 0x01;
        _ball.Rect.X = MiddleX(3);
        _ball.Rect.Y = MiddleY(3);
        _ballSpeed.X = (_rand.Next(0, 10) + (40 * (_rand.Next(0, 2) == 1 ? -1 : 1)));
        _ballSpeed.Y = (_rand.Next(0, 10) + (40 * (_rand.Next(0, 2) == 1 ? -1 : 1)));
    }

    protected override bool OnUpdate(float deltaTime)
    {
        // update player
        if (KeyHeld(38)) _player.Move(0, _player.Rect.Y <= 0 ? -_player.Rect.Y : -_paddleSpeed * deltaTime);
        if (KeyHeld(40)) _player.Move(0, (_player.Rect.B >= Height ? Height - _player.Rect.B : _paddleSpeed * deltaTime));

        // update ball
        if (_ball.Rect.Y <= 0) { _ball.Rect.Y = 0; _ballSpeed.Y *= -1; }
        if (_ball.Rect.B > Height) { _ball.Rect.Y = Height - _ball.Rect.H; _ballSpeed.Y *= -1; }
        if (_ball.Rect.X < 0 || _ball.Rect.R > Width) ResetBall(true);
        _ball.Move(_ballSpeed.X * deltaTime, _ballSpeed.Y * deltaTime);
        if (
            (_ballSpeed.X > 0 && _ball.Rect.R > _cpu.Rect.X && _ball.Rect.B >= _cpu.Rect.Y && _ball.Rect.Y <= _cpu.Rect.B) ||
            (_ballSpeed.X < 0 && _ball.Rect.X <= _player.Rect.R && _ball.Rect.B >= _player.Rect.Y && _ball.Rect.Y <= _player.Rect.B)
        )
        {
            _ballSpeed.X *= -1;
            _ballSpeed.X *= 1 + (float)(_rand.NextDouble() / 8);
            _ballSpeed.Y *= 1 + (float)(_rand.NextDouble() / 8);
            _ball.Color += (short)(_ball.Color > 0x0F ? 0 : 1);
        }

        // update cpu
        if (_ballSpeed.X > 0 && _rand.Next(0, 10) >= 3)
        {
            var centerBallY = _ball.Rect.Y + (_ball.Rect.H >> 1);
            var centerPaddleY = _cpu.Rect.Y + (_cpu.Rect.H >> 1);
            if (centerBallY != centerPaddleY)
                _cpu.Move(0, centerBallY < centerPaddleY
                    ? _cpu.Rect.Y <= 0 ? -_cpu.Rect.Y : -_paddleSpeed * deltaTime
                    : _cpu.Rect.B >= Height ? Height - _cpu.Rect.B : _paddleSpeed * deltaTime
                );
        }

        // draw
        ClearScreen();
        DrawText((Width >> 1) - 4, 1, 0x0F, $"{_scores[0]}       {_scores[1]}");
        DrawSprite(_player);
        DrawSprite(_cpu);
        DrawSprite(_middle);
        DrawSprite(_ball);
        return true;
    }

    #region Helpers
    int MiddleY(int h) => (Height >> 1) - (h >> 1);
    int MiddleX(int w) => (Width >> 1) - (w >> 1);
    void DrawSprite(Sprite s)
    {
        for (int y = (int)s.Rect.Y; y < s.Rect.B; y++)
            for (int x = (int)s.Rect.X; x < s.Rect.R; x++)
                DrawPixel(x, y, s.Color);
    }
    #endregion
}