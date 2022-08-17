using Microsoft.Win32.SafeHandles;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace CgeGames;

public abstract class Cge
{
    #region P/Invoke

    #region Windows Structs

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    struct ConsoleFontInfoEx
    {
        public uint Size;
        public uint Font;
        public Coord FontSize;
        public int FontFamily;
        public int FontWeight;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string FaceName;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct Coord
    {
        public short X;
        public short Y;

        public Coord(short X, short Y)
        {
            this.X = X;
            this.Y = Y;
        }
    }

    [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode)]
    struct CharInfo
    {
        [FieldOffset(0)] public char UnicodeChar;
        [FieldOffset(0)] public byte AsciiChar;
        [FieldOffset(2)] public short Attributes;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct SmallRect
    {
        public short Left;
        public short Top;
        public short Right;
        public short Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct ConsoleScreenBufferInfo
    {
        public Coord Size;
        public Coord CursorPosition;
        public short Attributes;
        public SmallRect RectWindow;
        public Coord MaximumSize;
    }

    #endregion

    #region WinApi Methods

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern IntPtr GetStdHandle(int nStdHandle);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern int SetCurrentConsoleFontEx(IntPtr consoleOutput, bool maxWindow, ref ConsoleFontInfoEx consoleFont);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool WriteConsoleOutputW(SafeFileHandle hConsoleOutput, CharInfo[] lpBuffer, Coord dwBufferSize, Coord dwBufferCoord, ref SmallRect lpWriteRegion);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    static extern SafeFileHandle CreateFile(string fileName, [MarshalAs(UnmanagedType.U4)] uint fileAccess, [MarshalAs(UnmanagedType.U4)] uint fileShare,
        IntPtr securityAttributes, [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition, [MarshalAs(UnmanagedType.U4)] int flags, IntPtr template);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool SetConsoleScreenBufferSize(IntPtr handle, Coord coord);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool SetConsoleActiveScreenBuffer(IntPtr handle);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool SetConsoleWindowInfo(IntPtr handle, bool x, ref SmallRect rect);

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool GetConsoleScreenBufferInfo(IntPtr handle, out ConsoleScreenBufferInfo info);

    [DllImport("user32.dll", SetLastError = true)]
    static extern short GetAsyncKeyState(int key);

    #endregion

    #endregion

    #region Private Members

    readonly short[] _oldKeys = new short[256];
    readonly short[] _newKeys = new short[256];
    readonly IntPtr _stdInputHandle;
    readonly IntPtr _stdOutputHandle;
    readonly CharInfo[] _charInfoBuffer;
    readonly SafeFileHandle _fileHandle;

    SmallRect _rect;

    bool Blit() =>
       WriteConsoleOutputW(_fileHandle, _charInfoBuffer,
           new() { X = Width, Y = Height },
           new() { X = 0, Y = 0 }, ref _rect);

    void HandleKeys()
    {
        for (var i = 0; i < 256; i++)
        {
            _oldKeys[i] = _newKeys[i];
            _newKeys[i] = GetAsyncKeyState(i);
        }
    }

    #endregion

    #region Protected Members

    const char DEFAULT_CHAR = '█';

    public string Title { get; set; }
    public short Width { get; private set; }
    public short Height { get; private set; }
    public short FontWidth { get; private set; }
    public short FontHeight { get; private set; }
    
    protected short DefaultColor { get; set; } = 0x0000;

    #region Protected Delegates
    /// <summary>
    /// Delegate called when the game starts. Use to load the game resources.
    /// </summary>
    /// <returns>boolean indicating the success of the operation</returns>
    protected abstract bool OnCreate();

    /// <summary>
    /// Delegate called every frame. Use to update the game state and draw to the framebuffer.
    /// </summary>
    /// <returns>boolean indicating the success of the operation</returns>
    protected abstract bool OnUpdate(float deltaTime);
    #endregion

    /// <summary>
    /// Method that handles if the specified key was just pressed.
    /// </summary>
    /// <param name="key">key code</param>
    /// <returns>boolean indicating if the key was pressed</returns>
    protected bool KeyPressed(int key) => (_newKeys[key] & 0x8000) > 0 && (_oldKeys[key] & 0x8000) == 0;

    /// <summary>
    /// Method that handles if the specified key was held.
    /// </summary>
    /// <param name="key">key code</param>
    /// <returns>boolean indicating if the key was held</returns>
    protected bool KeyHeld(int key) => (_newKeys[key] & 0x8000) > 0 && (_oldKeys[key] & 0x8000) > 0;

    /// <summary>
    /// Method that handles if the specified key was released.
    /// </summary>
    /// <param name="key">key code</param>
    /// <returns>boolean indicating if the key was released</returns>
    protected bool KeyReleased(int key) => (_newKeys[key] & 0x8000) == 0 && (_oldKeys[key] & 0x8000) > 0;

    /// <summary>
    /// Draws a pixel to the screen.
    /// </summary>
    /// <param name="x">X position</param>
    /// <param name="y">Y position</param>
    /// <param name="color">pixel color</param>
    /// <param name="c">char to write. optional, default = DEFAULT_CHAR</param>
    protected void DrawPixel(int x, int y, short color, char c = DEFAULT_CHAR)
    {
        var i = y * Width + x;
        if (i < 0 || i >= _charInfoBuffer.Length) return;
        _charInfoBuffer[i].Attributes = color;
        _charInfoBuffer[i].UnicodeChar = c;
    }

    /// <summary>
    /// Writes text to the screen.
    /// </summary>
    /// <param name="x">X position</param>
    /// <param name="y">Y position</param>
    /// <param name="color">pixel color</param>
    /// <param name="text">Text to write</param>
    protected void DrawText(int x, int y, short color, string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return;
        for (var i = 0; i < text.Length; i++)
        {
            if (x + i >= Width) return;
            DrawPixel(x + i, y, color, text[i]);
        }
    }

    /// <summary>
    /// Paints the whole screen black.
    /// </summary>
    protected void ClearScreen()
    {
        for (var i = 0; i < _charInfoBuffer.Length; i++)
        {
            _charInfoBuffer[i].Attributes = DefaultColor;
            _charInfoBuffer[i].UnicodeChar = DEFAULT_CHAR;
        }
    }
    
    #endregion

    #region Public Methods

    /// <summary>
    /// Starts the game loop.
    /// </summary>
    /// <exception cref="Exception">if OnUpdate returns false, this exception is thrown</exception>
    public void Run()
    {
        var lastTime = DateTime.UtcNow;
        while (true)
        {
            var diff = DateTime.UtcNow - lastTime;
            var deltaTime = (float)diff.TotalSeconds;
            lastTime = DateTime.UtcNow;
            HandleKeys();
            var fps = 1.0f / deltaTime;
            Console.Title = $"{Title} - FPS: {fps:0.00}";
            if (!OnUpdate(deltaTime)) throw new Exception("OnUpdate failed.");
            Blit();
        }
    }

    /// <summary>
    /// Constructor. Inherit to create the console.
    /// </summary>
    /// <param name="title">Window title</param>
    /// <param name="width">Window width (in characters)</param>
    /// <param name="height">Window height (in characters)</param>
    /// <param name="fontWidth">Font width (in pixels)</param>
    /// <param name="fontHeight">Font height (in pixels)</param>
    /// <exception cref="Exception">if OnCreate returns false, this exception is thrown</exception>
    public Cge(string title, short width, short height, short fontWidth, short fontHeight)
    {
        Title = title;
        Width = width;
        Height = height;
        FontWidth = fontWidth;
        FontHeight = fontHeight;
        _stdInputHandle = GetStdHandle(-10);
        _stdOutputHandle = GetStdHandle(-11);

        _rect = new() { Left = 0, Top = 0, Right = Width, Bottom = Height };

        Console.Title = Title;
        Console.CursorVisible = false;

        var coord = new Coord() { X = Width, Y = Height };
        SetConsoleScreenBufferSize(_stdOutputHandle, coord);
        SetConsoleActiveScreenBuffer(_stdOutputHandle);

        var cfi = new ConsoleFontInfoEx();
        cfi.Size = (uint)Marshal.SizeOf(cfi);
        cfi.Font = 0;
        cfi.FontSize.X = FontWidth;
        cfi.FontSize.Y = FontHeight;
        cfi.FaceName = FontWidth < 4 || FontHeight < 4 ? "Consolas" : "Terminal";
        _ = SetCurrentConsoleFontEx(_stdOutputHandle, false, ref cfi);

        GetConsoleScreenBufferInfo(_stdOutputHandle, out ConsoleScreenBufferInfo info);

        if (Width > info.MaximumSize.X) throw new Exception($"Width > info.MaximumSize.X ({info.MaximumSize.X})");
        if (Height > info.MaximumSize.Y) throw new Exception($"Height > info.MaximumSize.Y ({info.MaximumSize.Y})");

        var rect = new SmallRect { Top = 0, Left = 0, Right = (short)(Width - 1), Bottom = (short)(Height - 1) };
        if (!SetConsoleWindowInfo(_stdOutputHandle, true, ref rect)) throw new Exception("Erro in SetConsoleWindowInfo");

        SetConsoleMode(_stdInputHandle, 0x0080);
        _charInfoBuffer = new CharInfo[Width * Height];
        _fileHandle = CreateFile("CONOUT$", 0x40000000, 2, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero);

        if (!OnCreate()) throw new Exception("OnCreate failed.");
    }

    #endregion
}