// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using ImageViewer.Common;
using ImageViewer.Content.Renderers.ThreeD;
using ImageViewer.Content.Views;
using System;
using System.Numerics;
using System.Threading.Tasks;

namespace ImageViewer.Content.Utils
{
    internal class Tag : IDisposable
    {
        private PyramidRenderer left;
        private PyramidRenderer right;

        private readonly int X;
        private readonly int Y;
        private bool visible = true;

        internal Tag(
            DeviceResources deviceResources,
            TextureLoader loader,
            int X,
            int Y,
            Matrix4x4 rotator,
            Vector3 positionLeft,
            Vector3 positionRight
            )
        {
            this.X = X;
            this.Y = Y;

            left = new PyramidRenderer(deviceResources, loader)
            {
                Position = positionLeft,
                GlobalRotator = rotator,
                TextureFile = "Content\\Textures\\red.png"
            };

            right = new PyramidRenderer(deviceResources, loader)
            {
                Position = positionRight,
                GlobalRotator = rotator,
                TextureFile = "Content\\Textures\\green.png"
            };
        }

        internal async Task CreateDeviceDependentResourcesAsync()
        {
            await left.CreateDeviceDependentResourcesAsync();
            await right.CreateDeviceDependentResourcesAsync();
        }

        internal void ReleaseDeviceDependentResources()
        {
            visible = false;
            left.ReleaseDeviceDependentResources();
            right.ReleaseDeviceDependentResources();
        }

        internal void Update(NavigationView view, PointerRenderer.Corners corners, D2[] square)
        {       
            if (!Rotator.InsideSquare(square, new D2(X, Y)))
            {
                visible = false;
            }
            else
            {
                // xc = A * ac + B * ec
                // yd = A * bd + B * fd

                float xc = X - view.BottomLeftX;
                float yd = Y - view.BottomLeftY;

                float ac = view.TopLeftX - view.BottomLeftX;
                float bd = view.TopLeftY - view.BottomLeftY;
                
                float ec = view.BottomRightX - view.BottomLeftX;
                float fd = view.BottomRightY - view.BottomLeftY;

                float A, B;

                if (ac == 0)
                {
                    B = xc / ec;
                    A = yd / bd;
                }
                else
                {
                    B = ((xc / ac) * bd - yd) / (((ec / ac) * bd) - fd);
                    A = (xc - B * ec) / ac;
                }
          
                var xx = B * Constants.ViewSize;
                var yy = A * Constants.ViewSize;

                var pL = new Vector3(
                    corners.orig_bottomLeft.X + xx, 
                    corners.orig_bottomLeft.Y + yy, 
                    corners.orig_topLeft.Z);

                var pR = new Vector3(
                    corners.orig_bottomLeft.X + xx + Constants.ViewSize, 
                    corners.orig_bottomLeft.Y + yy, 
                    corners.orig_topLeft.Z);

                var translation = Matrix4x4.CreateTranslation(corners.Position);

                var p = Vector4.Transform(pL, translation);
                left.Position = new Vector3(p.X, p.Y, p.Z);

                p = Vector4.Transform(pR, translation);
                right.Position = new Vector3(p.X, p.Y, p.Z);

                visible = true;
            }
        }

        internal void Render()
        {
            if (visible)
            {
                left.Render();
                right.Render();
            }
        }

        internal void SetPosition(Vector3 dp)
        {
            left.Position += dp;
            right.Position += dp;
        }

        internal void SetRotator(Matrix4x4 rotator)
        {
            left.GlobalRotator = rotator;
            right.GlobalRotator = rotator;
        }

        internal void Dispose()
        {
            left?.Dispose();
            left = null;

            right?.Dispose();
            right = null;
        }

        void IDisposable.Dispose()
        {
            left?.Dispose();
            right?.Dispose();
        }
    }
}
