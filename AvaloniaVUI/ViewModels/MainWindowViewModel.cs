using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using ReactiveUI;
using Avalonia.Media;

using Pv;
using OpenTK.Audio.OpenAL;


namespace AvaloniaVUI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private bool _isGrapefruit;
        public bool IsGrapefruit
        {
            get => _isGrapefruit;
            set
            {
                this.RaiseAndSetIfChanged(ref _isGrapefruit, value);
                BackgroundColour = _bgColours[0];
            }
        }

        private bool _isGrasshopper;
        public bool IsGrasshopper
        {
            get => _isGrasshopper;
            set
            {
                this.RaiseAndSetIfChanged(ref _isGrasshopper, value);
                BackgroundColour = _bgColours[1];
            }
        }

        private bool _isBumblebee;
        public bool IsBumblebee
        {
            get => _isBumblebee;
            set
            {
                this.RaiseAndSetIfChanged(ref _isBumblebee, value);
                BackgroundColour = _bgColours[2];
            }
        }

        private bool _isBlueberry;
        public bool IsBlueberry
        {
            get => _isBlueberry;
            set
            {
                this.RaiseAndSetIfChanged(ref _isBlueberry, value);
                BackgroundColour = _bgColours[3];
            }
        }

        private Color _backgroundColour = Colors.LightGray;
        public Color BackgroundColour { get => _backgroundColour; set => this.RaiseAndSetIfChanged(ref _backgroundColour, value); }

        private readonly List<Color> _bgColours = new List<Color>() { Colors.LightPink, Colors.LawnGreen, Colors.Yellow, Colors.BlueViolet };

        private readonly CancellationTokenSource _audioLoopCancelSource;

        public MainWindowViewModel()
        {
            _audioLoopCancelSource = new CancellationTokenSource();
            CancellationToken audioLoopCancelToken = _audioLoopCancelSource.Token;
            Task.Factory.StartNew(() =>
            {
                using Porcupine porcupine = Porcupine.Create(keywords: new List<string> { "grapefruit", "grasshopper", "bumblebee", "blueberry" });

                short[] frameBuffer = new short[porcupine.FrameLength];
                ALCaptureDevice captureDevice = ALC.CaptureOpenDevice(null, porcupine.SampleRate, ALFormat.Mono16, porcupine.FrameLength * 2);
                {
                    ALC.CaptureStart(captureDevice);
                    while (!audioLoopCancelToken.IsCancellationRequested)
                    {
                        int samplesAvailable = ALC.GetAvailableSamples(captureDevice);
                        if (samplesAvailable > porcupine.FrameLength)
                        {
                            ALC.CaptureSamples(captureDevice, ref frameBuffer[0], porcupine.FrameLength);

                            int keywordIndex = porcupine.Process(frameBuffer);
                            if (keywordIndex >= 0)
                            {                                
                                switch (keywordIndex)
                                {
                                    case 0:
                                        IsGrapefruit = true;
                                        break;
                                    case 1:
                                        IsGrasshopper = true;
                                        break;
                                    case 2:
                                        IsBumblebee = true;
                                        break;
                                    case 3:
                                        IsBlueberry = true;
                                        break;
                                }                                
                            }
                        }
                        Thread.Yield();
                    }
                }
            }, audioLoopCancelToken);
        }
    }
}
