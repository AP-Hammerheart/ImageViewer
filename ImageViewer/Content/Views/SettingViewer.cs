using ImageViewer.Common;
using ImageViewer.Content.Renderers;
using System;
using System.Numerics;
using System.Threading.Tasks;
using Windows.UI;

namespace ImageViewer.Content.Views
{
    internal class SettingViewer : IDisposable
    {
        private Settings settings;
        private readonly TextRenderer[] settingItems;

        private bool refreshNeeded = true;

        internal SettingViewer(
            ImageViewerMain main,
            DeviceResources deviceResources,
            TextureLoader loader)
        {
            settings = new Settings();
            settingItems = new TextRenderer[2];

            string[] texts = {
                "Case ID",
                "Left image ",
                "Right image",
                "Right image X offset ",
                "Right image Y offset",
                "Scaler",
                "Store textures",
                "Use Jpeg",
                "Raw texture",
                "Local server",
                "Address"
            };

            settingItems[0] = new TextRenderer(
                    deviceResources: deviceResources,
                    loader: loader,
                    bottomLeft: new Vector3(-0.6f, -0.3f, 0.0f),
                    topLeft: new Vector3(-0.6f, 0.3f, 0.0f),
                    bottomRight: new Vector3(-0.2f, -0.3f, 0.0f),
                    topRight: new Vector3(-0.2f, 0.3f, 0.0f))
            {
                Position = new Vector3(0.0f, 0.0f, Settings.DistanceFromUser),
                TextPosition = new Vector2(20, 10),
                Lines = texts,
                LineHeight = 60,
                FontSize = 40,
                ImageWidth = 640,
                ImageHeight = 960,
                BackgroundColor = Colors.White
            };

            settingItems[1] = new TextRenderer(
                    deviceResources: deviceResources,
                    loader: loader,
                    bottomLeft: new Vector3(-0.2f, -0.3f, 0.0f),
                    topLeft: new Vector3(-0.2f, 0.3f, 0.0f),
                    bottomRight: new Vector3(0.6f, -0.3f, 0.0f),
                    topRight: new Vector3(0.6f, 0.3f, 0.0f))
            {
                Position = new Vector3(0.0f, 0.0f, Settings.DistanceFromUser),
                TextPosition = new Vector2(20, 10),
                Lines = GetValues(),
                LineHeight = 60,
                FontSize = 30.0f,
                ImageWidth = 1280,
                ImageHeight = 960,
                Index = 0,
                BackgroundColor = Colors.White
            };
        }

        internal void SetItem(int type)
        {
            switch (type)
            {
                case 0: settingItems[1].Index = Math.Max(0, settingItems[1].Index - 1); break;
                case 1: settingItems[1].Index = Math.Min(settingItems[1].Lines.Length - 1, settingItems[1].Index + 1); break;
                case 2: switch (settingItems[1].Index)
                    {
                        case 0:
                            Settings.CaseID = settings.NextCase(Settings.CaseID, false);
                            Settings.Image1 = settings.FirstImage();
                            Settings.Image2 = settings.FirstImage();
                            SetImageProperties();
                            break;
                        case 1:
                            Settings.Image1 = settings.NextImage(Settings.Image1, false);
                            SetImageProperties();
                            break;
                        case 2:
                            Settings.Image2 = settings.NextImage(Settings.Image2, false);
                            SetImageProperties();
                            break;
                        case 3: Settings.Image2offsetX -= Settings.Scaler  * 1; break;
                        case 4: Settings.Image2offsetY -= Settings.Scaler * 1; break;
                        case 5: Settings.Scaler =
                                Settings.Scaler == 1000 ? 500 :
                                Settings.Scaler == 500 ? 100 :
                                Settings.Scaler == 100 ? 10 : 
                                Settings.Scaler == 10 ? 5 : 
                                Settings.Scaler == 5 ? 1 : 1000;
                            break;
                        case 6: Settings.SaveTexture = !Settings.SaveTexture; break;
                        case 7: Settings.UseJpeg = !Settings.UseJpeg; break;
                        case 8: Settings.DownloadRaw = !Settings.DownloadRaw;
                            if (Settings.DownloadRaw) Settings.UseJpeg = false;
                            break;
                        case 9: Settings.LocalServer = !Settings.LocalServer; break;
                    }
                    break;
                case 3:
                    switch (settingItems[1].Index)
                    {
                        case 0:
                            Settings.CaseID = settings.NextCase(Settings.CaseID, true);
                            Settings.Image1 = settings.FirstImage();
                            Settings.Image2 = settings.FirstImage();
                            SetImageProperties();
                            break;
                        case 1:
                            Settings.Image1 = settings.NextImage(Settings.Image1, true);
                            SetImageProperties();
                            break;
                        case 2:
                            Settings.Image2 = settings.NextImage(Settings.Image2, true);
                            SetImageProperties();
                            break;
                        case 3: Settings.Image2offsetX += Settings.Scaler * 1; break;
                        case 4: Settings.Image2offsetY += Settings.Scaler * 1; break;
                        case 5: Settings.Scaler = 
                                Settings.Scaler == 1 ? 5 : 
                                Settings.Scaler == 5 ? 10 :
                                Settings.Scaler == 10 ? 100 :
                                Settings.Scaler == 100 ? 500 :
                                Settings.Scaler == 500 ? 1000 : 1;
                            break;
                        case 6: Settings.SaveTexture = !Settings.SaveTexture; break;
                        case 7: Settings.UseJpeg = !Settings.UseJpeg; break;
                        case 8: Settings.DownloadRaw = !Settings.DownloadRaw;
                            if (Settings.DownloadRaw) Settings.UseJpeg = false;
                            break;
                        case 9: Settings.LocalServer = !Settings.LocalServer; break;
                    }
                    break;
            }

            Update();
        }

        internal void Update()
        {
            settingItems[1].Lines = GetValues();
            refreshNeeded = true;
        }

        private string[] GetValues()
        {
            var values = new string[11];

            values[0] = Settings.CaseID;
            values[1] = Settings.Image1;
            values[2] = Settings.Image2;
            values[3] = Settings.Image2offsetX.ToString();
            values[4] = Settings.Image2offsetY.ToString();
            values[5] = Settings.Scaler.ToString();
            values[6] = Settings.SaveTexture ? "true" : "false";
            values[7] = Settings.UseJpeg ? "true" : "false";
            values[8] = Settings.DownloadRaw ? "true" : "false";
            values[9] = Settings.LocalServer ? "true" : "false";
            values[10] = Settings.URL;

            return values;
        }


        internal void NextSlide()
        {
            Settings.Image2 = settings.NextImage(Settings.Image2, true);
            SetImageProperties();
        }

        private void SetImageProperties()
        {
            Settings.MaxResolutionX = Math.Max(settings.Resolution(Settings.Image1, true), settings.Resolution(Settings.Image2, true));
            Settings.MaxResolutionY = Math.Max(settings.Resolution(Settings.Image1, false), settings.Resolution(Settings.Image2, false));
            Settings.Multiplier = Math.Max(settings.PixelMultiplier(Settings.Image1), settings.PixelMultiplier(Settings.Image2));
            Settings.MinScale = Math.Min(settings.SmallestScale(Settings.Image1), settings.SmallestScale(Settings.Image2));
            Settings.Image2offsetX = settings.Offset(Settings.Image2, true);
            Settings.Image2offsetY = settings.Offset(Settings.Image2, false);
        }

        internal void Update(StepTimer timer)
        {
            if (refreshNeeded)
            {
                foreach (var renderer in settingItems)
                {
                    renderer?.Update(timer);
                }
                refreshNeeded = false;
            }         
        }

        internal void Render()
        {
            foreach (var renderer in settingItems)
            {
                renderer?.Render();
            }
        }

        internal void SetPosition(Vector3 dp)
        {
            foreach (var item in settingItems)
            {
                var pos = item.Position + dp;
                item.Position = pos;
            }
        }

        internal void SetRotator(Matrix4x4 rotator)
        {
            foreach (var item in settingItems)
            {
                item.GlobalRotator = rotator;
            }
        }

        internal void Dispose()
        {
            foreach (var renderer in settingItems)
            {
                renderer?.Dispose();
            }
        }

        internal async Task CreateDeviceDependentResourcesAsync()
        {
            await settings?.InitializeAsync();

            if (Settings.Online)
            {
                foreach (var renderer in settingItems)
                {
                    await renderer?.CreateDeviceDependentResourcesAsync();
                }
            }       
        }

        internal void ReleaseDeviceDependentResources()
        {
            foreach (var renderer in settingItems)
            {
                renderer?.ReleaseDeviceDependentResources();
            }
        }

        void IDisposable.Dispose()
        {
            foreach (var renderer in settingItems)
            {
                renderer?.Dispose();
            }
        }
    }
}
