using System;
using Microsoft.Xna.Framework.Input;
using System.Xml.Serialization;
using CaveGame.Common;

namespace CaveGame.Client;


public enum FramerateLimiterOptions
{
    NoCap,
    Cap240,
    Cap144,
    Cap120,
    Cap90,
    Cap60,
    Cap30,
}

public enum FullscreenResolutions
{
    // 4 by 3
    Res640X480,
    Res800X600,
    Res1024X768,
    Res1152X864,
    Res1280X960,
    Res1440X1050,
    Res1600X1200,
    Res2048X1536,

    // 16 by 9
    Res854X480,
    Res1280X720,
    Res1366X768,
    Res1600X900,
    Res1920X1080,
    Res2560X1440,
    Res3840X2160,

    UseMonitorSize
}

/// <summary>
/// Janky UI Interfacing stuff
/// </summary>
/// <typeparam name="T"></typeparam>
public class SliderIndex<T>
{
    public string Display;
    public T Value;

    public SliderIndex(string display, T val)
    {
        Display = display;
        Value = val;
    }

    public static SliderIndex<int>[] GetIntArray(int minimum, int maximum, int increment = 1)
    {
        SliderIndex<int>[] arr = new SliderIndex<int>[maximum - minimum];
        for (int i = 0; i < (maximum - minimum); i++)
        {
            arr[i] = new SliderIndex<int>((minimum + (increment * i)).ToString(), minimum + (increment * i));
        }

        return arr;
    }

    public static SliderIndex<float>[] GetFloatArray(int minimum, int maximum, float increment)
    {
        SliderIndex<float>[] arr = new SliderIndex<float>[maximum - minimum];
        for (int i = 0; i < (maximum - minimum); i++)
        {
            arr[i] = new SliderIndex<float>((minimum + (increment * i)).ToString(), minimum + (increment * i));
        }

        return arr;
    }
}

public class SettingChangedEventArgs<T> : EventArgs
{
    public T OldValue { get; init; }
    public T NewValue { get; init; }


    public SettingChangedEventArgs(T oldval, T newval)
    {
        OldValue = oldval;
        NewValue = newval;
    }
}

// TODO: Create Wrapper Thingy to represent s
// List<T> where T: Keys, GamepadButton, etc.
public class InputActionWrapping
{
    
}

/// <summary>
/// Cave Game Settings File
/// </summary>
[XmlRoot("XGameSettings")]
public class GameSettings : ConfigFile
{
    #region Settings
    public bool Particles
    {
        get => _particles;
        set
        {
            OnParticlesEnabledChanged?.Invoke(this, new(_particles, value));
            _particles = value;
        }
    }
    public float UserInterfaceScale
    {
        get => _uiScale;
        set
        {
            OnUiScaleChanged?.Invoke(this, new(_uiScale, value));
            _uiScale = value;
        }
    }
    public bool Fullscreen
    {
        get => _fullscreen;
        set
        {
            OnFullscreenStateChanged?.Invoke(this, new(_fullscreen, value));
            _fullscreen = value;
        }
    }
    public FullscreenResolutions FullscreenResolution { get; set; }
    public bool VSync
    {
        get => _vsync;
        set
        {
            OnVSyncEnabledChanged?.Invoke(this, new(_vsync, value));
            _vsync = value;
        }
    }
    public bool CameraShake
    {
        get => _cameraShake;
        set
        {
            OnCameraShakeEnabledChanged?.Invoke(this, new(_vsync, value));
            _cameraShake = value;
        }
    }
    public int FramerateLimit
    {
        get => _fpsLimit;
        set
        {
            OnFpsLimitChanged?.Invoke(this, new(_fpsLimit, value));
            _fpsLimit = value;
        }
    }
    public int MasterVolume
    {
        get => _masterVolume;
        set
        {
            OnMasterVolumeChanged?.Invoke(this, new(_masterVolume, value));
            _masterVolume = value;
        }
    }
    public int MusicVolume
    {
        get => _musicVolume;
        set
        {
            OnMusicVolumeChanged?.Invoke(this, new(_musicVolume, value));
            _musicVolume = value;
            // AudioManager.MusicVolume = value / 100.0f;
        }
    }
    public int SfxVolume
    {
        get => _sfxVolume;
        set
        {
            OnSfxVolumeChanged?.Invoke(this, new(_sfxVolume, value));
            _sfxVolume = value;
        }
    }
    public int AmbienceVolume
    {
        get => _ambienceVolume;
        set
        {
            OnAmbienceVolumeChanged?.Invoke(this, new(_ambienceVolume, value));
            _ambienceVolume = value;
        }
    }
    public Keys MoveLeftKey { get; set; }
    public Keys MoveRightKey { get; set; }
    public Keys MoveDownKey { get; set; }
    public Keys MoveUpKey { get; set; }
    public Keys JumpKey { get; set; }
    public GameChatSize ChatSize { get; set; }
    public string TexturePackName { get; set; }
    #endregion
    
    #region Internal values
    [XmlIgnore]
    private bool _fullscreen;
    private FullscreenResolutions _fullscreenRes;
    private bool _particles;
    private bool _vsync;
    private int _fpsLimit;
    private int _masterVolume;
    private int _musicVolume;
    private int _ambienceVolume;
    private int _sfxVolume;
    private bool _cameraShake;
    private float _uiScale;
    #endregion
    
    public delegate void SettingChangedEvent<T>(object sender, SettingChangedEventArgs<T> e);

    // Event Handlers
    // CaveGameClient will subscribe to these
    public event SettingChangedEvent<int> OnMasterVolumeChanged;
    public event SettingChangedEvent<int> OnMusicVolumeChanged;
    public event SettingChangedEvent<int> OnAmbienceVolumeChanged;
    public event SettingChangedEvent<int> OnSfxVolumeChanged;
    public event SettingChangedEvent<bool> OnVSyncEnabledChanged;
    public event SettingChangedEvent<int> OnFpsLimitChanged;
    public event SettingChangedEvent<bool> OnFullscreenStateChanged;
    public event SettingChangedEvent<FullscreenResolutions> OnFullscreenResolutionChanged;

    public event SettingChangedEvent<bool> OnCameraShakeEnabledChanged;
    public event SettingChangedEvent<bool> OnParticlesEnabledChanged;
    public event SettingChangedEvent<float> OnUiScaleChanged;


    public static SliderIndex<int>[] VolumeSliderOptions = SliderIndex<int>.GetIntArray(0, 101);

    public static SliderIndex<GameChatSize>[] ChatSizeSliderOptions =
    {
        new SliderIndex<GameChatSize>("Large", GameChatSize.Large),
        new SliderIndex<GameChatSize>("Normal", GameChatSize.Normal),
        new SliderIndex<GameChatSize>("Small", GameChatSize.Small)
    };

    public static SliderIndex<GameChatSize>[] FpsSliderOptionSet =
    {
        new SliderIndex<GameChatSize>("Large", GameChatSize.Large),
        new SliderIndex<GameChatSize>("Normal", GameChatSize.Normal),
        new SliderIndex<GameChatSize>("Small", GameChatSize.Small)
    };
    public static GameSettings CurrentSettings { get; set; }
    public GameSettings()
    {
        CurrentSettings = this;
    }
    
    public void Save() {}
    public void LoadGameSettings()
    {
        FillDefaults();
        // Ur Kidding Me?
        // OK Here's the issue at hand:
        // GameSettings instance must exist before loading the file
        // Because CaveGameDesktopClient connects to it's XSettingChanged
        // Lazy solution: Create a "clone" object with the data
        // and copy properties
        
        // The alternative to this is to load the XML directly and that's nasty.

        var settings = ConfigFile.Load<GameSettings>("settings.xml", true);
        this.FramerateLimit = settings.FramerateLimit;
        this.MoveDownKey = settings.MoveDownKey;
        this.MasterVolume = settings.MasterVolume;
        this.SfxVolume = settings.SfxVolume;
        this.AmbienceVolume = settings.AmbienceVolume;
        this.VSync = settings.VSync;
        this.Fullscreen = settings.Fullscreen;
        this.FullscreenResolution = settings.FullscreenResolution;
        this.UserInterfaceScale = settings.UserInterfaceScale;
        this.CameraShake = settings.CameraShake;
        
    }

    public override void FillDefaults()
    {
        FramerateLimit = 60;
        Fullscreen = false;
        MoveDownKey = Keys.S;
        MoveUpKey = Keys.W;
        JumpKey = Keys.Space;
        MoveLeftKey = Keys.A;
        MoveRightKey = Keys.D;
        ChatSize = GameChatSize.Normal;
        MasterVolume = 100;
        MusicVolume = 50;
        SfxVolume = 75;
        FullscreenResolution = FullscreenResolutions.Res1366X768;
    }
}
