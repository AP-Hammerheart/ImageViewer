// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ImageViewer.Common;
using SharpDX.Direct3D11;
using System.Numerics;

namespace ImageViewer.Content
{
    internal class PyramidRenderer : BaseCubeRenderer
    {
        internal PyramidRenderer(
            DeviceResources deviceResources, 
            TextureLoader loader)
            : base(deviceResources, loader)
        {
        }

        internal override void LoadGeometry()
        {
            VertexCube[] vertices =
            {
                new VertexCube(new Vector3(-0.005f, -0.005f, 0.03f)),
                new VertexCube(new Vector3(-0.005f,  0.005f, 0.03f)),
                new VertexCube(new Vector3( 0.005f, -0.005f, 0.03f)),
                new VertexCube(new Vector3( 0.005f,  0.005f, 0.03f)),
                new VertexCube(new Vector3( 0.0f,    0.0f,   0.0f))
            };

            vertexBuffer = ToDispose(Buffer.Create(deviceResources.D3DDevice, BindFlags.VertexBuffer, vertices));

            ushort[] vertexIndices =
            {
                1,2,0,
                3,2,1,
                1,0,4,
                3,1,4,
                2,3,4,
                0,2,4,
            };

            indexCount = vertexIndices.Length;
            indexBuffer = ToDispose(Buffer.Create(deviceResources.D3DDevice, BindFlags.IndexBuffer, vertexIndices));

            modelConstantBuffer = ToDispose(Buffer.Create(
                deviceResources.D3DDevice,
                BindFlags.ConstantBuffer,
                ref modelConstantBufferData));
        }
    }
}
