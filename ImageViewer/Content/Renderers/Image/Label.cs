// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using ImageViewer.Common;
using ImageViewer.Content.Renderers.Base;
using ImageViewer.Content.Renderers.ThreeD;
using ImageViewer.Content.Utils;
using System;
using System.Numerics;
using System.Threading.Tasks;
using Windows.UI;

namespace ImageViewer.Content.Renderers.Image
{
    internal class Label
    {
        private StatusBarRenderer label;
        private SlideFrameRenderer frame;

        public int Type { get; set; } = 0;

        internal static Color highlighted = Colors.Green;
        internal static Color defaultColor = Colors.Yellow;

        public void HighLightLabel() {
            label.BackgroundColor = highlighted;
            frame.Color = highlighted;
        }

        public void SetColorToDefault() {
            label.BackgroundColor = defaultColor;
            frame.Color = defaultColor;
            System.Diagnostics.Debug.WriteLine( "change color" );
        }

        internal Label(
            DeviceResources deviceResources,
            TextureLoader loader,
            ImageRenderer image,
            int[] coordinates,
            string labelText,
            float offset = 0.005f)
        {
            var corners = GetCorners(
                        image.BottomLeft,
                        image.TopLeft,
                        image.BottomRight,
                        image.TopRight,
                        image.Width,
                        image.Height,
                        coordinates);

            var x_axis = Vector3.Normalize(image.TopRight - image.TopLeft);
            var y_axis = Vector3.Normalize(image.TopLeft - image.BottomLeft);

            var center = corners.Item1 + 0.5f * (corners.Item4 - corners.Item1);

            var d = 0.05f;
            var bl = center - d * x_axis - d * y_axis;
            var tl = center - d * x_axis + d * y_axis;

            var uvbl = GetUV(center, corners.Item1, bl, tl, 2 * d);
            var uvtl = GetUV(center, corners.Item2, bl, tl, 2 * d);
            var uvbr = GetUV(center, corners.Item3, bl, tl, 2 * d);
            var uvtr = GetUV(center, corners.Item4, bl, tl, 2 * d);

            label = new StatusBarRenderer(deviceResources, loader, corners, 0.001f)
            {
                Text = labelText,
                FontSize = 30.0f,
                ImageWidth = 256,
                ImageHeight = 256,
                TextPosition = new Vector2(labelText.Length > 2 ? 107 : labelText.Length > 1 ? 112 : 117, 113),
                BackgroundColor = /*labelText == "Z4" ? highlighted :*/ defaultColor /**/,

                UBL = uvbl.X,
                VBL = uvbl.Y,
                UTL = uvtl.X,
                VTL = uvtl.Y,
                UBR = uvbr.X,
                VBR = uvbr.Y,
                UTR = uvtr.X,
                VTR = uvtr.Y,
            };

            frame = new SlideFrameRenderer(deviceResources, loader, corners.Item1, corners.Item2, corners.Item3, corners.Item4,
                depth: 0.001f,
                thickness: 0.001f)
            {
                Position = new Vector3(0.0f, 0.0f, Constants.DistanceFromUser),
                Color = /*labelText == "Z4" ? highlighted :*/ defaultColor /**/,
            };
        }

        private static Vector2 GetUV(Vector3 center, Vector3 pt, Vector3 bottomLeft, Vector3 topLeft, float d)
        {
            var A2 = Vector3.DistanceSquared(pt, topLeft);
            var B2 = Vector3.DistanceSquared(pt, bottomLeft);

            var y = (B2 - A2 + Math.Pow(d,2)) / (2.0f * d);
            var x = Math.Sqrt(B2 - Math.Pow(y, 2));

            return new Vector2((float)x/d, (float)(d - y)/d);
        }

        private static Vector3 GetPosition(int x, int y, int w, int h,
            Vector3 bottomLeft,
            Vector3 topLeft,
            Vector3 topRight)
        {
            var fx = (float)(x) / (float)w;
            var fy = (float)(y) / (float)h;

            return topLeft + fx * (topRight - topLeft) + fy * (bottomLeft - topLeft);
        }

        private static Tuple<Vector3, Vector3, Vector3, Vector3> GetCorners(
            Vector3 bl,
            Vector3 tl,
            Vector3 br,
            Vector3 tr,
            int w,
            int h,
            int[] c
            )
        {
            var plane = Plane.CreateFromVertices(tl, bl, tr);
            var normal = plane.Normal;

            var p = new Vector3[4];

            for (var i = 0; i < 4; i++)
            {
                p[i] = GetPosition(c[2 * i], c[2 * i + 1], w, h, bl, tl, tr);
            }

            return new Tuple<Vector3, Vector3, Vector3, Vector3>(p[0], p[1], p[2], p[3]);
        }

        internal void Render()
        {
            if (Type == 1)
            {
                frame.Render();
            }
            else if (Type == 2)
            {
                label.Render();
            }          
        }

        internal void Update(StepTimer timer)
        {
            label?.Update(timer);
            frame?.Update(timer);
        }

        internal void SetPosition(Vector3 dp)
        {
            label.Position += dp;
            frame.Position += dp;
        }

        internal void SetRotator(Matrix4x4 rotator)
        {
            label.GlobalRotator = rotator;
            frame.GlobalRotator = rotator;
        }

        internal async Task CreateDeviceDependentResourcesAsync()
        {
            await label.CreateDeviceDependentResourcesAsync();
            await frame.CreateDeviceDependentResourcesAsync();
        }

        internal void ReleaseDeviceDependentResources()
        {
            label.ReleaseDeviceDependentResources();
            frame.ReleaseDeviceDependentResources();
        }

        internal void Dispose()
        {
            label?.Dispose();
            frame?.Dispose();
        }
    }
}
