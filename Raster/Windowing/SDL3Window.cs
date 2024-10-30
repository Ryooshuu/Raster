using System.Numerics;
using Raster.Graphics;
using Raster.Graphics.SDL3;
using Raster.Memory;
using SDL;
using static SDL.SDL3;

namespace Raster.Windowing;

internal unsafe class SDL3Window : ResourceManager, IWindow
{
    #region Properties

    private bool windowVisible;
    
    public bool Visible
    {
        get => windowVisible;
        set
        {
            if (value)
                Show();
            else
                Hide();
        }
    }
    
    private bool windowFocused;
    
    public bool Focused
    {
        get => windowFocused;
        set
        {
            if (windowFocused == value)
                return;
    
            windowFocused = value;
            if (value)
                _ = SDL_RaiseWindow(SDLWindowHandle);
        }
    }
    
    private bool windowMinimized;
    
    public bool IsMinimized
    {
        get => windowMinimized;
        set
        {
            if (windowMinimized == value)
                return;
    
            windowMinimized = value;
            _ = value ? SDL_MinimizeWindow(SDLWindowHandle) : SDL_RestoreWindow(SDLWindowHandle);
        }
    }
    
    private bool windowFullscreen;
    
    public bool IsFullscreen
    {
        get => windowFullscreen;
        set
        {
            if (windowFullscreen == value)
                return;
    
            ToggleFullscreen();
        }
    }
    
    public int X
    {
        get => (i32)Position.X;
        set => Position = new Vector2(value, Position.Y);
    }
    
    public int Y
    {
        get => (i32)Position.Y;
        set => Position = new Vector2(Position.X, value);
    }
    
    public Vector2 Position
    {
        get
        {
            if (SDLWindowHandle == null)
                return Vector2.Zero;
    
            i32 x, y;
            _ = SDL_GetWindowPosition(SDLWindowHandle, &x, &y);
            return new Vector2(x, y);
        }
        set
        {
            if (SDLWindowHandle == null)
                return;
    
            _ = SDL_SetWindowPosition(SDLWindowHandle, (i32)value.X, (i32)value.Y);
        }
    }
    
    public int Width
    {
        get => (i32)Size.X;
        set => Size = new Vector2(value, Size.Y);
    }
    
    public int Height
    {
        get => (i32)Size.Y;
        set => Size = new Vector2(Size.X, value);
    }
    
    public Vector2 Size
    {
        get
        {
            if (SDLWindowHandle == null)
                return Vector2.Zero;
    
            i32 w, h;
            _ = SDL_GetWindowSize(SDLWindowHandle, &w, &h);
            return new Vector2(w, h);
        }
        set
        {
            if (SDLWindowHandle == null)
                return;
    
            _ = SDL_SetWindowSize(SDLWindowHandle, (i32)value.X, (i32)value.Y);
        }
    }
    
    private string title = null!;
    
    public string Title
    {
        get => title;
        set
        {
            if (value == title)
                return;
    
            _ = SDL_SetWindowTitle(SDLWindowHandle, title);
            title = value;
        }
    }

    #endregion

    public bool Alive => SDLWindowHandle != null; 
    
    public WindowHandle Handle { get; private set; } = null!;
    public IGraphicsDevice GraphicsDevice { get; private set; } = null!;

    internal SDL_Window* SDLWindowHandle;
    
    public void Init(WindowCreateInfo info)
    {
        _ = SDL_Init(SDL_InitFlags.SDL_INIT_VIDEO | SDL_InitFlags.SDL_INIT_EVENTS);
        
        var flags = SDL_WindowFlags.SDL_WINDOW_HIDDEN;
        if (info.Resizable)
            flags |= SDL_WindowFlags.SDL_WINDOW_RESIZABLE;

        SDLWindowHandle = SDL_CreateWindow(info.Title, info.Width, info.Height, flags);

        if (SDLWindowHandle == null)
        {
            throw new InvalidOperationException($"Failed to create SDL window. SDL Error: {SDL_GetError()}");
        }
        
        getWindowHandle();

        GraphicsDevice = new SDL3GraphicsDevice { Window = this };
        GraphicsDevice.Init();
    }

    private void getWindowHandle()
    {
        var isWayland = SDL_GetCurrentVideoDriver() == "wayland";
        var props = SDL_GetWindowProperties(SDLWindowHandle);

        var ndt = IntPtr.Zero;
        var nwh = IntPtr.Zero;

        switch (RuntimeInfo.OS)
        {
            case RuntimeInfo.Platform.Windows:
                ndt = SDL_GetPointerProperty(props, SDL_PROP_WINDOW_WIN32_HWND_POINTER, IntPtr.Zero);
                break;

            case RuntimeInfo.Platform.Linux:
                if (isWayland)
                {
                    ndt = SDL_GetPointerProperty(props, SDL_PROP_WINDOW_WAYLAND_SURFACE_POINTER, IntPtr.Zero);
                    nwh = SDL_GetPointerProperty(props, SDL_PROP_WINDOW_WAYLAND_DISPLAY_POINTER, IntPtr.Zero);
                    break;
                }

                if (SDL_GetCurrentVideoDriver() == "x11")
                {
                    ndt = new IntPtr(SDL_GetNumberProperty(props, SDL_PROP_WINDOW_X11_WINDOW_NUMBER, 0));
                    nwh = SDL_GetPointerProperty(props, SDL_PROP_WINDOW_X11_DISPLAY_POINTER, IntPtr.Zero);
                    break;
                }

                break;

            case RuntimeInfo.Platform.MacOS:
                ndt = SDL_GetPointerProperty(props, SDL_PROP_WINDOW_COCOA_WINDOW_POINTER, IntPtr.Zero);
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        Handle = new WindowHandle(this, ndt, nwh);
    }

    public void Show()
    {
        if (windowVisible || !Alive)
            return;

        _ = SDL_ShowWindow(SDLWindowHandle);
        windowVisible = true;
    }

    public void Hide()
    {
        if (!windowVisible)
            return;

        _ = SDL_HideWindow(SDLWindowHandle);
        windowVisible = false;
    }

    public void ToggleFullscreen()
    {
        windowFullscreen = !windowFullscreen;

        if (windowFullscreen)
        {
            SDL_Rect bounds = new();
            _ = SDL_GetDisplayBounds(0, &bounds);
            _ = SDL_RestoreWindow(SDLWindowHandle);
            _ = SDL_SetWindowSize(SDLWindowHandle, bounds.w, bounds.h);
            _ = SDL_SetWindowPosition(SDLWindowHandle, (i32)SDL_WINDOWPOS_CENTERED, (i32)SDL_WINDOWPOS_CENTERED);
        }
        else
        {
            _ = SDL_RestoreWindow(SDLWindowHandle);
            _ = SDL_MaximizeWindow(SDLWindowHandle);
        }
    }

    private const i32 events_per_peep = 64;
    private readonly SDL_Event[] events = new SDL_Event[events_per_peep];
    
    public void Poll()
    {
        if (!Alive)
            return;
        
        SDL_PumpEvents();

        i32 eventsRead;

        do
        {
            eventsRead = SDL_PeepEvents(events, SDL_EventAction.SDL_GETEVENT, SDL_EventType.SDL_EVENT_FIRST,
                SDL_EventType.SDL_EVENT_LAST);

            for (var i = 0; i < eventsRead; i++)
                handleEvents(events[i]);
        } while (eventsRead == events_per_peep);
    }

    private void handleEvents(SDL_Event e)
    {
        switch (e.Type)
        {
            case SDL_EventType.SDL_EVENT_QUIT:
                Close();
                break;
            
            case SDL_EventType.SDL_EVENT_WINDOW_FOCUS_LOST:
                windowFocused = false;
                break;
            
            case SDL_EventType.SDL_EVENT_WINDOW_FOCUS_GAINED:
                windowFocused = true;
                break;
            
            case SDL_EventType.SDL_EVENT_WINDOW_MINIMIZED:
                windowMinimized = true;
                break;
            
            case SDL_EventType.SDL_EVENT_WINDOW_RESTORED:
                windowMinimized = false;
                break;
        }
    }

    public void Close()
    {
        Dispose();
    }

    protected override void DisposeResources()
    {
        GraphicsDevice.Dispose();
        
        SDL_DestroyWindow(SDLWindowHandle);
        SDLWindowHandle = null;
    }

}
