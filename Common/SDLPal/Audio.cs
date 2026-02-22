using SDL3;
using System.Threading;

namespace SDLPal;

public static class PalAudio
{
    static int _trackCount;
    static nint MixerDevice { get; set; }
    static uint TrackPlayerOptions { get; set; }
    static nint Track { get; set; }
    static long FadeMilliseconds { get; set; }
    static readonly Mixer.TrackStoppedCallback TrackStoppedCallback = AutoFreeTrack;

    /// <summary>
    /// 初始化 Audio 子系统
    /// </summary>
    public static void Init()
    {
        //
        // 初始化 SDL-Mixer 引擎
        //
        Mixer.Init();

        //
        // 初始化 Mixer 设备
        //
        MixerDevice = Mixer.CreateMixerDevice(0xFFFFFFFFu, 0);

        //
        // 初始化播放器默认选项
        //
        TrackPlayerOptions = SDL.CreateProperties();
    }

    /// <summary>
    /// 销毁 Audio 子系统
    /// </summary>
    public static void Free()
    {
        //
        // 停止播放音轨
        //
        Mixer.StopAllTracks(MixerDevice, FadeMilliseconds);

        //
        // 等待音轨停止（保证所有音轨都被彻底销毁）
        // 期间扔接受输入和处理事件，但禁止了窗口关闭事件重复触发
        //
        while (_trackCount > 0) PalTimer.Delay(1);

        //
        // 销毁播放器选项
        //
        SDL.DestroyProperties(TrackPlayerOptions);

        //
        // 销毁 Mixer 设备
        //
        Mixer.DestroyMixer(MixerDevice);

        //
        // 销毁 SDL-Mixer 引擎
        //
        Mixer.Quit();
    }

    static void AutoFreeTrack(nint userdata, nint track)
    {
        //
        // 解除绑定并销毁对应的 Audio
        //
        var audio = Mixer.GetTrackAudio(track);
        Mixer.SetTrackAudio(track, 0);
        Mixer.DestroyAudio(audio);

        //
        // 销毁音轨
        //
        Mixer.DestroyTrack(track);

        //
        // 减少正在播放的音轨统计
        //
        Interlocked.Decrement(ref _trackCount);
    }

    static void Play(string audioPath, bool loop = true, long fadeMilliseconds = 1500, bool isMusic = true)
    {
        //
        // 检查音频文件是否存在，不存在则崩溃
        //
        S.FileExist(audioPath);

        //
        // 在 Mixer 设备上创建音轨
        //
        var track = Mixer.CreateTrack(MixerDevice);

        if (isMusic)
        {
            //
            // 先停止正在播放的音乐，同时直接播放另一首音乐
            //
            StopMusic();

            //
            // 记录该音轨
            //
            Track = track;

            //
            // 记录淡入淡出时间
            //
            FadeMilliseconds = fadeMilliseconds;
        }

        //
        // 将音频放入音轨，并设置停止播放是时触发的回调，清理资源
        //
        Mixer.SetTrackAudio(track, Mixer.LoadAudio(MixerDevice, audioPath, false));
        if (!isMusic) Mixer.SetTrackStoppedCallback(track, TrackStoppedCallback, 0);

        //
        // 设置是否循环播放
        //
        SDL.SetNumberProperty(TrackPlayerOptions, Mixer.Props.PlayLoopsNumber, loop ? -1 : 0);

        //
        // 设置音轨淡入淡出时间
        //
        SDL.SetNumberProperty(TrackPlayerOptions, Mixer.Props.PlayFadeInMillisecondsNumber, fadeMilliseconds);

        //
        // 播放音轨
        //
        Mixer.PlayTrack(track, TrackPlayerOptions);

        if (!isMusic)
            //
            // 增加正在播放的音轨统计
            //
            Interlocked.Increment(ref _trackCount);
    }

    public static void StopMusic()
    {
        if (Track == 0)
            //
            // 当前没有正在播放的背景音乐
            //
            return;

        //
        // 停止音轨
        //
        Mixer.StopTrack(Track, Mixer.TrackMSToFrames(Track, 1500));

        //
        // 清除淡入淡出时长
        //
        FadeMilliseconds = 0;

        //
        // 删除音轨句柄
        //
        Track = 0;
    }

    public static void PlayMusic(int musicId, bool loop = true, long fadeMilliseconds = 1500)
    {
        if (musicId == 0)
            //
            // musicId 为 0 时停止背景音乐
            //
            StopMusic();
        else
            Play($@"{S.ModPath.Assets.Music}\{musicId:D3}.mp3", loop: loop, fadeMilliseconds: fadeMilliseconds);
    }

    public static void PlayVoice(int voiceId) =>
        Play($@"{S.ModPath.Assets.Voice}\{voiceId:D5}.wav", loop: false, fadeMilliseconds: 0, isMusic: false);
}
