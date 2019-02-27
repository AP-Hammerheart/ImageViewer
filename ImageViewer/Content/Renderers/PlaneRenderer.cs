// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ImageViewer.Common;
using SharpDX.Direct3D11;

namespace ImageViewer.Content.Renderers
{
    internal abstract class PlaneRenderer : BasePlaneRenderer
    {
        protected TextureLoader loader;

        internal PlaneRenderer(
            DeviceResources deviceResources, 
            TextureLoader loader, 
            string id) 
            : base(deviceResources)
        {
            TextureID = id;
            this.loader = loader;
        }

        internal string TextureID { get; set; }

        internal abstract void UpdateGeometry();

        internal override void LoadGeometry()
        {
            UpdateGeometry();

            ushort[] planeIndices =
            {
                1,2,0,
                3,2,1,
            };

            indexCount = planeIndices.Length;
            indexBuffer = ToDispose(Buffer.Create(
                deviceResources.D3DDevice, 
                BindFlags.IndexBuffer, 
                planeIndices));

            modelConstantBuffer = ToDispose(Buffer.Create(
                deviceResources.D3DDevice,
                BindFlags.ConstantBuffer,
                ref modelConstantBufferData));
        }
    }
}
