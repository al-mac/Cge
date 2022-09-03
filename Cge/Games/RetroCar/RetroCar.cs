using System;
using System.Collections.Generic;

namespace CgeGames.Games.RetroCar;

public class RetroCar : Cge
{
    float _carPos = 0.0f;
    float _carDistance = 0.0f;
    List<KeyValuePair<float, float>> _vecTrack = new(); // curvature, distance
    float _curvature = 0.0f;
    float _carSpeed = 0.0f;
    float _trackCurvature = 0.0f;
    float _playerCurvature = 0.0f;
    float _trackDistance = 0.0f;
    readonly int width = 0;
    readonly int height = 0;
    short[] _carSprite;

    public RetroCar() : base("RetroCar", 160, 100, 3, 3)
    {
        width = Width;
        height = Height;
    }

    protected override bool OnCreate()
    {
        _carSprite = new short[21]
        {
            0b000011111111000,
            0b000000011000000, 
            0b000000111100000,
            0b000000111100000,
            0b011100111100111,
            0b011111111111111, 
            0b011100111100111,

            0b000000011111111,
            0b000000000011000,
            0b000000001111000,
            0b000000011110000,
            0b011100111101110,
            0b011111111111110,
            0b011101111001110,

            0b111111110000000,
            0b000110000000000,
            0b000111100000000,
            0b000011110000000,
            0b111001111001110,
            0b111111111111110,
            0b111001111001110
        };

        _vecTrack.Add(new(0.0f, 10.0f)); // start, finish
        _vecTrack.Add(new(0.0f, 200.0f));
        _vecTrack.Add(new(1.0f, 200.0f));
        _vecTrack.Add(new(0.0f, 400.0f));
        _vecTrack.Add(new(-1.0f, 100.0f));
        _vecTrack.Add(new(0.0f, 200.0f));
        _vecTrack.Add(new(-1.0f, 200.0f));
        _vecTrack.Add(new(1.0f, 200.0f));
        _vecTrack.Add(new(0.0f, 200.0f));
        _vecTrack.Add(new(0.2f, 500.0f));
        _vecTrack.Add(new(0.0f, 200.0f));

        foreach (var t in _vecTrack)
            _trackDistance += t.Value;

        return true;
    }

    protected override bool OnUpdate(float deltaTime)
    {
        if (KeyHeld(38)) // UP
            _carSpeed += 2.0f * deltaTime;
        else
            _carSpeed -= 1.0f * deltaTime;

        var sprOffset = 0;
        if (KeyHeld(37))
        {
            sprOffset = 7;
            _playerCurvature -= 0.7f * deltaTime;
        }
        if (KeyHeld(39))
        {
            sprOffset = 14;
            _playerCurvature += 0.7f * deltaTime;
        }

        if (Math.Abs(_playerCurvature - _trackCurvature) >= 0.8f)
            _carSpeed *= 0.8f;

        if (_carSpeed > 1.0f) _carSpeed = 1.0f;
        if (_carSpeed < 0.0f) _carSpeed = 0.0f;

        _carDistance += (70.0f * _carSpeed) * deltaTime;

        var offset = 0.0f;
        var section = 0;

        if (_carDistance >= _trackDistance)
            _carDistance -= _trackDistance;

        while (section < _vecTrack.Count && offset <= _carDistance)
        {
            offset += _vecTrack[section].Value;
            section++;
        }

        var targetCurvature = _vecTrack[section - 1].Key;
        var curvatureDiff = (targetCurvature - _curvature) * deltaTime * _carSpeed;
        _curvature += curvatureDiff;

        _trackCurvature += (_curvature) * deltaTime * _carSpeed;

        // Draw Sky
        for (var y = 0; y < height >> 1; y++)
            for (var x = 0; x < width; x++)
                DrawPixel(x, y, y < height >> 2 ? (byte)0x01 : (byte)0x09);

        // Draw Mountains
        for (var x = 0; x < width; x++)
        {
            var hillHeight = (int)Math.Abs(Math.Sin(x * 0.01f + _trackCurvature) * 16.0f);
            for (var y = (height >> 1) - hillHeight; y < height >> 1; y++)
                DrawPixel(x, y, 0x06);
        }

        for (var y = 0; y < (Height >> 1); y++)
        {
            for (var x = 0; x < width; x++)
            {
                float perspective = y / (Height / 2.0f);

                float middle = 0.5f + _curvature * (float)Math.Pow(1.0f - perspective, 3);
                float road = 0.1f + perspective * 0.8f;
                float clip = road * 0.15f;
                road *= 0.5f;

                var leftGrass = (middle - road - clip) * width;
                var leftClip = (middle - road) * width;

                var rightGrass = (middle + road + clip) * width;
                var rightClip = (middle + road) * width;

                var row = (Height >> 1) + y;
                var grassColor = Math.Sin(20.0f * Math.Pow(1.0f - perspective, 3) + _carDistance * 0.1f) > 0 ? (byte)0x02 : (byte)0x0A;
                var clipColor = Math.Sin(80.0f * Math.Pow(1.0f - perspective, 2) + _carDistance) > 0 ? (byte)0x04 : (byte)0x0F;
                var roadColor = (section - 1) == 0 ? (byte)0x0F : (byte)0x08;

                if (x >= 0 && x < leftGrass)
                    DrawPixel(x, row, grassColor);
                if (x >= leftGrass && x < leftClip)
                    DrawPixel(x, row, clipColor);
                if (x >= leftClip && x < rightClip)
                    DrawPixel(x, row, roadColor);
                if (x >= rightClip && x < rightGrass)
                    DrawPixel(x, row, clipColor);
                if (x >= rightGrass && x < width)
                    DrawPixel(x, row, grassColor);

                // Draw car
                _carPos = _playerCurvature - _trackCurvature;
                var carPos = (int)((width >> 1) + ((int)(width * _carPos) / 2.0f) - 7);

                for (var cy = 0; cy < 7; cy++)
                {
                    for (var cx = 0; cx < 15; cx++)
                    {
                        if (((_carSprite[cy + sprOffset] >> cx) & 1) == 0) continue;
                        DrawPixel(carPos + cx, 80 + cy, 0x0B);
                    }
                }
            }
        }

        return true;
    }
}
