using Sdcb.FFmpeg.Raw;
using SDL3;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static Sdcb.FFmpeg.Raw.ffmpeg;

namespace SDLPal;

public unsafe class MoviePlayer : IDisposable
{
    public const ulong      ErrorCode = 0xFFFF_FFFF_FFFF_FFFFu;

    AVFormatContext*        _pFormatContext;
    int                     _videoStreamIndex, _audioStreamIndex, _buffSize;
    double                  _timeBase;
    AVCodecContext*         _pCodecContext, _pAudioCodecContext;
    AVFrame*                _pFrame, _pFrameABGR, _pAudioFrame;
    byte*                   _pOutBuffer;
    AVPacket*               _pPacket;
    SwsContext*             _pSwsContext;
    SwrContext*             _pSwrContext;
    SDL.AudioSpec           _audioSpec;

    string FilePath { get; set; } = null!;
    uint AudioDeviceId { get; set; }
    nint AudioStreamId { get; set; }
    byte[]? AudioBuffer { get; set; }
    long LastPts { get; set; } = AV_NOPTS_VALUE;
    bool IsUnevenVideo { get; set; }
    bool IsPlaying { get; set; }
    public nint MovieSurface { get; set; }
    public nint MovieScreen { get; set; }

    /// <summary>
    /// 初始化视频模块
    /// </summary>
    public MoviePlayer(string filePath) => Init(filePath);

    public void Init(string filePath)
    {
        //
        // 初始化视频流
        //
        InitVideoStream(FilePath = filePath);

        //
        // 初始化解码器
        //
        InitDecoder();

        //
        // 初始化 SDL 音频
        //
        InitAudio();

        //
        // 初始化单次获取帧的状态
        //
        NeedContinueReadingPackage = false;
        NeedContinueReadingFrame = false;
        FreamReadingResult = 0;
    }

    public void ReLoad() => Init(FilePath);

    ~MoviePlayer() => Dispose();
    /// <summary>
    /// 销毁视频模块
    /// </summary>
    public void Dispose()
    {
        //
        // 强制停止正在播放的视频
        //
        IsPlaying = false;

        GC.SuppressFinalize(this);
        PalGlobal.MoviePlayer = null!;

        //
        // 释放 SDL 资源
        //
        SDL.DestroyAudioStream(AudioStreamId);
        SDL.CloseAudioDevice(AudioDeviceId);
        SDL.DestroyTexture(MovieSurface);
        SDL.DestroyTexture(MovieScreen);

        //
        // 释放 FFmpeg 资源
        //
        sws_freeContext(_pSwsContext);
        av_free(_pOutBuffer);

        fixed (AVPacket** p2Packet = &_pPacket)
        fixed (AVFrame** p2FrameABGR = &_pFrameABGR)
        fixed (AVFrame** p2Frame = &_pFrame)
        fixed (AVCodecContext** p2CodecContext = &_pCodecContext)
        fixed (AVFormatContext** p2FormatContext = &_pFormatContext)
        fixed (AVFrame** p2AudioFrame = &_pAudioFrame)
        fixed (AVCodecContext** p2AudioCodecContext = &_pAudioCodecContext)
        fixed (SwrContext** pSwr = &_pSwrContext)
        {
            av_packet_free(p2Packet);
            av_frame_free(p2FrameABGR);
            av_frame_free(p2Frame);
            avcodec_free_context(p2CodecContext);
            av_frame_free(p2AudioFrame);
            avcodec_free_context(p2AudioCodecContext);
            avformat_close_input(p2FormatContext);
            swr_free(pSwr);
        }
    }

    void InitVideoStream(string fileName)
    {
        int     i;

        //
        // 对网络库进行全局初始化
        //
        //avformat_network_init();

        //
        // 为视频格式上下文分配内存
        //
        if ((_pFormatContext = avformat_alloc_context()) == null)
            throw new Exception("Could not allocate format context.");

        //
        // 打开视频文件
        //
        fixed (AVFormatContext** p2FormatContext = &_pFormatContext)
            if (avformat_open_input(p2FormatContext, fileName, null, null) != 0)
                throw new Exception("Could not open input stream.");

        //
        // 获取流信息
        //
        if (avformat_find_stream_info(_pFormatContext, null) < 0)
            throw new Exception("Could not find stream information.");

        //
        // 检测视频帧分布是否均匀
        //
        switch (Marshal.PtrToStringAnsi((nint)_pFormatContext->iformat->name))
        {
            case "avi":
            case "mpeg":
            case "bink":
                IsUnevenVideo = true;
                break;
        }

        //
        // 查找视频流
        //
        _videoStreamIndex = -1;
        for (i = 0; i < _pFormatContext->nb_streams; i++)
            if (_pFormatContext->streams[i]->codecpar->codec_type == AVMediaType.Video)
            {
                _videoStreamIndex = i;
                break;
            }

        //
        // 没有找到视频流
        //
        if (_videoStreamIndex == -1)
            throw new Exception("Didn't find a video stream.");
    }

    void InitDecoder()
    {
        int                     i;
        AVCodecParameters*      pCodecParameters, audioCodecParameters;
        AVCodec*                pCodec, audioCodec;

        //
        // 获取视频流的编解码器参数、时间基
        //
        pCodecParameters = _pFormatContext->streams[_videoStreamIndex]->codecpar;
        _timeBase = av_q2d(_pFormatContext->streams[_videoStreamIndex]->time_base);

        //
        // 查找解码器
        //
        if ((pCodec = avcodec_find_decoder(pCodecParameters->codec_id)) == null)
            throw new Exception("Codec not found.");

        //
        // 分配编解码器上下文
        //
        if ((_pCodecContext = avcodec_alloc_context3(pCodec)) == null)
            throw new Exception("Could not allocate codec context.");

        //
        // 将编解码器参数复制到编解码器上下文
        //
        if (avcodec_parameters_to_context(_pCodecContext, pCodecParameters) < 0)
            throw new Exception("Could not copy codec parameters to context.");

        //
        // 打开解码器
        //
        if (avcodec_open2(_pCodecContext, pCodec, null) < 0)
            throw new Exception("Could not open codec.");

        //
        // 分配帧对象内存
        //
        if ((_pFrame = av_frame_alloc()) == null || (_pFrameABGR = av_frame_alloc()) == null)
            throw new Exception("Could not allocate frame.");

        //
        // 分配 RGB 缓冲区并设置 AVFrame
        //
        _buffSize = av_image_get_buffer_size(AVPixelFormat.Abgr, _pCodecContext->width, _pCodecContext->height, 1);
        _pOutBuffer = (byte*)av_malloc((ulong)_buffSize);
        av_image_fill_arrays(
            ref Unsafe.As<byte_ptrArray8, byte_ptrArray4>(ref _pFrameABGR->data),
            ref Unsafe.As<int_array8, int_array4>(ref _pFrameABGR->linesize),
            _pOutBuffer, AVPixelFormat.Abgr, _pCodecContext->width, _pCodecContext->height, 1
        );

        //
        // 初始化视频画布
        //
        MovieScreen = COS.Texture(_pCodecContext->width, _pCodecContext->height, PalVideo.DefaultFormat, 2);
        MovieSurface = COS.Surface(_pCodecContext->width, _pCodecContext->height, PalVideo.DefaultFormat);

        //
        // 初始化转换上下文 (YUV 到 ABGR)
        //
        _pSwsContext = sws_getContext(
            _pCodecContext->width, _pCodecContext->height, _pCodecContext->pix_fmt,
            _pCodecContext->width, _pCodecContext->height, AVPixelFormat.Abgr,
            (int)SWS.Bilinear, null, null, null
        );

        //
        // 查找音频流
        //
        for (i = 0; i < _pFormatContext->nb_streams; i++)
        {
            if (_pFormatContext->streams[i]->codecpar->codec_type == AVMediaType.Audio)
            {
                _audioStreamIndex = i;
                break;
            }
        }

        //
        // 初始化音频解码器
        //
        if (_audioStreamIndex != -1)
        {
            audioCodecParameters = _pFormatContext->streams[_audioStreamIndex]->codecpar;
            audioCodec = avcodec_find_decoder(audioCodecParameters->codec_id);

            if (audioCodec != null)
            {
                _pAudioCodecContext = avcodec_alloc_context3(audioCodec);
                if (_pAudioCodecContext != null)
                {
                    if (avcodec_parameters_to_context(_pAudioCodecContext, audioCodecParameters) >= 0)
                    {
                        if (avcodec_open2(_pAudioCodecContext, audioCodec, null) >= 0)
                        {
                            _pAudioFrame = av_frame_alloc();
                        }
                    }
                }
            }
        }

        //
        // 分配 AVPacket 内存
        //
        if ((_pPacket = av_packet_alloc()) == null)
            throw new Exception("Could not allocate AVPacket.");
    }

    void InitAudio()
    {
        AVChannelLayout     inLayout, outLayout;

        // 设置期望的音频规格
        _audioSpec = new()
        {
            Freq = _pFormatContext->streams[_audioStreamIndex]->codecpar->sample_rate,
            Format = SDL.AudioFormat.AudioS16LE,
            Channels = _pAudioCodecContext->ch_layout.nb_channels,
        };

        // 打开音频设备 (SDL3 风格)
        if ((AudioDeviceId = SDL.OpenAudioDevice(SDL.AudioDeviceDefaultPlayback, in _audioSpec)) == 0)
            throw new Exception($"无法打开音频设备: {SDL.GetError()}");

        if ((AudioStreamId = SDL.CreateAudioStream(in _audioSpec, in _audioSpec)) == 0)
            throw new Exception($"无法创建音频流: {SDL.GetError()}");

        if (!SDL.BindAudioStream(AudioDeviceId, AudioStreamId))
            throw new Exception($"无法绑定音频流: {SDL.GetError()}");

        SDL.ResumeAudioDevice(AudioDeviceId);

        // 初始化重采样上下文
        _pSwrContext = ffmpeg.swr_alloc();
        av_channel_layout_default(&inLayout, _pAudioCodecContext->ch_layout.nb_channels);
        av_channel_layout_default(&outLayout, _audioSpec.Channels);
        av_opt_set_chlayout(_pSwrContext, "in_chlayout", &inLayout, 0);
        av_opt_set_chlayout(_pSwrContext, "out_chlayout", &outLayout, 0);
        av_opt_set_int(_pSwrContext, "in_sample_rate", _pAudioCodecContext->sample_rate, 0);
        av_opt_set_int(_pSwrContext, "out_sample_rate", _audioSpec.Freq, 0);
        av_opt_set_sample_fmt(_pSwrContext, "in_sample_fmt", (AVSampleFormat)_pAudioCodecContext->sample_fmt, 0);
        av_opt_set_sample_fmt(_pSwrContext, "out_sample_fmt", AVSampleFormat.S16, 0);
        if (ffmpeg.swr_init(_pSwrContext) < 0)
            throw new Exception("Could not initialize SwrContext for audio resampling.");

        AudioBuffer = new byte[192000];
    }

    ulong GetDefaultFrameDelay(AVCodecContext* pCodecContext)
    {
        double      fps;
        ulong       frameDelay;

        //
        // 默认 25fps
        //
        frameDelay = 1000 / 25;

        if (pCodecContext->framerate.Num > 0 && pCodecContext->framerate.Den > 0)
        {
            fps = pCodecContext->framerate.Num / (double)pCodecContext->framerate.Den;

            if (fps > 0) frameDelay = (ulong)(1000 / fps);
        }

        return frameDelay;
    }

    ulong GetFrameDelay()
    {
        ulong       frameDelay;

        //
        // 计算帧间隔时间
        //
        if (LastPts != AV_NOPTS_VALUE && _pFrame->pts != AV_NOPTS_VALUE)
        {
            frameDelay = (ulong)(((_pFrame->pts - LastPts) * _timeBase) * 1000.0);

            if (frameDelay <= 0 || frameDelay > 1000)
                frameDelay = GetDefaultFrameDelay(_pCodecContext);
        }
        else
            frameDelay = GetDefaultFrameDelay(_pCodecContext);

        if (IsUnevenVideo)
            LastPts = _pFrame->pts;

        return frameDelay;
    }

    void Draw(bool needUpdate = true)
    {
        //
        // 将 ABGR 数据更新到纹理并渲染
        //
        SDL.UpdateTexture(MovieScreen, 0, _pFrameABGR->data[0], _pFrameABGR->linesize[0]);

        if (needUpdate)
        {
            S.CleanUpTex(PalScreen.Main);
            PalScreen.Copy(MovieScreen, PalScreen.Main);
            PalScreen.Update();
        }
    }

    bool NeedContinueReadingPackage { get; set; }
    bool NeedContinueReadingFrame { get; set; }
    int FreamReadingResult { get; set; }

    /// <summary>
    /// 获取一帧的视频
    /// </summary>
    public ulong DrawFrame()
    {
        var time = 0ul;

        if (NeedContinueReadingFrame)
            //
            // 继续读取当前包里的帧
            //
            goto ContinueReadingFrame;

        if (av_read_frame(_pFormatContext, _pPacket) < 0)
            return ErrorCode;

        ContinueReadingPackage:
        if (_pPacket->stream_index != _videoStreamIndex)
            return ErrorCode - 1;

        //
        // 发送视频包到解码器
        //
        if ((FreamReadingResult = avcodec_send_packet(_pCodecContext, _pPacket)) < 0)
            throw new Exception("Error sending packet for decoding.");

        //
        // 接收解码后的帧
        //
        ContinueReadingFrame:
        if (FreamReadingResult >= 0)
        {
            if ((FreamReadingResult = avcodec_receive_frame(_pCodecContext, _pFrame)) == AVERROR(EAGAIN) || FreamReadingResult == AVERROR_EOF)
                //
                // 没有更多帧可接收
                //
                goto ContinueReadingPackage;
            else if (FreamReadingResult < 0) throw new Exception("Error during decoding.");

            time = SDL.GetTicks() + GetFrameDelay();

            //
            // 转换图像格式为 ABGR
            //
            sws_scale(
                _pSwsContext, _pFrame->data.ToRawArray(), _pFrame->linesize.ToArray(),
                0, _pCodecContext->height,
                _pFrameABGR->data.ToRawArray(), _pFrameABGR->linesize.ToArray()
            );

            //
            // 更新 Surface 像素
            //
            Draw(false);
            SDL.LockSurface(MovieSurface);
            {
                var surface = (SDL.Surface*)MovieSurface;
                var destPixels = (byte*)surface->Pixels;
                var dstPitch = surface->Pitch;
                var rowBytes = _pFrameABGR->linesize[0] / surface->Width;
                var src = (byte*)_pFrameABGR->data[0].ToPointer();

                for (int y = 0; y < surface->Height; y++)
                    C.memcpy((nint)(destPixels + y * dstPitch), (nint)(src + y * _pFrameABGR->linesize[0]), dstPitch);
            }
            SDL.UnlockSurface(MovieSurface);

            //
            // 释放帧数据
            //
            av_frame_unref(_pFrame);
        }

        //
        // 返回下次绘制视频帧的时间
        //
        return time;
    }

    /// <summary>
    /// 开始播放视频
    /// </summary>
    public void Play()
    {
        int     result;

        //
        // 设置播放状态为允许播放
        //
        IsPlaying = true;

        //
        // 对于音视频帧分布不均匀的视频，关闭窗口边框，防止用户操作导致音画不同步
        //
        SDL.GetWindowPosition(PalVideo.Window, out var x, out var y);
        //if (IsUnevenVideo)
        {
            SDL.SetWindowBordered(PalVideo.Window, false);
            SDL.SetWindowPosition(PalVideo.Window, (int)SDL.WindowPosCenteredMask, (int)SDL.WindowPosCenteredMask);
        }

        //
        // 事件循环
        //
        var time = SDL.GetTicks();
        while (IsPlaying)
        {
            if (av_read_frame(_pFormatContext, _pPacket) < 0) break;

            if (_pPacket->stream_index == _videoStreamIndex)
            {
                //
                // 发送视频包到解码器
                //
                if ((result = avcodec_send_packet(_pCodecContext, _pPacket)) < 0)
                    throw new Exception("Error sending packet for decoding.");

                //
                // 接收解码后的帧
                //
                while (result >= 0 && IsPlaying)
                {
                    if ((result = avcodec_receive_frame(_pCodecContext, _pFrame)) == AVERROR(EAGAIN) || result == AVERROR_EOF)
                        //
                        // 没有更多帧可接收
                        //
                        break;
                    else if (result < 0) throw new Exception("Error during decoding.");

                    //
                    // 延迟相应的毫秒数，控制帧率
                    //
                    PalTimer.DelayUntil(time);
                    time = SDL.GetTicks() + GetFrameDelay();

                    //
                    // 转换图像格式为 ABGR
                    //
                    sws_scale(
                        _pSwsContext, _pFrame->data.ToRawArray(), _pFrame->linesize.ToArray(),
                        0, _pCodecContext->height,
                        _pFrameABGR->data.ToRawArray(), _pFrameABGR->linesize.ToArray()
                    );

                    //
                    // 更新窗口画面
                    //
                    Draw();

                    //
                    // 释放帧数据
                    //
                    av_frame_unref(_pFrame);
                }
            }
            else if (_audioStreamIndex != -1 && _pPacket->stream_index == _audioStreamIndex && _pAudioCodecContext != null)
            {
                if ((result = avcodec_send_packet(_pAudioCodecContext, _pPacket)) < 0)
                    throw new Exception("Error sending audio packet for decoding.");

                while (result >= 0 && IsPlaying)
                {
                    if ((result = avcodec_receive_frame(_pAudioCodecContext, _pAudioFrame)) == AVERROR(EAGAIN) || result == AVERROR_EOF)
                        //
                        // 没有更多帧可接收
                        //
                        break;
                    else if (result < 0) throw new Exception("Error during audio decoding.");

                    //
                    // 对于音视频帧分布不均匀的视频，记录当前音频帧的时间戳，让视频帧与之同步
                    //
                    if (!IsUnevenVideo)
                        LastPts = _pAudioFrame->pts;

                    // 重采样到 S16 格式
                    fixed (byte* pBuffer = AudioBuffer)
                    {
                        byte*       outPtrs;
                        int         outSamples, outBufferSize;

                        outPtrs = pBuffer;
                        outSamples = ffmpeg.swr_convert(
                            _pSwrContext,
                            &outPtrs,
                            _pAudioFrame->nb_samples,
                            _pAudioFrame->extended_data,
                            _pAudioFrame->nb_samples
                        );

                        // 2 bytes per sample (S16)
                        if ((outBufferSize = outSamples * _audioSpec.Channels * 2) > 0)
                            SDL.PutAudioStreamData(AudioStreamId, (nint)pBuffer, outBufferSize);
                    }

                    av_frame_unref(_pAudioFrame);
                }
            }

            //
            // 释放数据包
            //
            av_packet_unref(_pPacket);
        }

        //if (IsUnevenVideo)
        {
            //
            // 对于音视频帧分布不均匀的视频，将窗口边框设置回原来的样子
            //
            SDL.SetWindowPosition(PalVideo.Window, x, y);
            SDL.SetWindowBordered(PalVideo.Window, true);
        }

        //
        // 销毁视频播放器
        //
        Dispose();

        //
        // 关闭播放状态
        //
        Stop();
    }

    public void Stop() => IsPlaying = false;
}
