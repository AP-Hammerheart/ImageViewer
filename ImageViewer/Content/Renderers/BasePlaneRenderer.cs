// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ImageViewer.Common;
using SharpDX.Direct3D11;

namespace ImageViewer.Content.Renderers
{
    internal abstract class BasePlaneRenderer : BaseRenderer
    {
        internal BasePlaneRenderer(
            DeviceResources deviceResources)
            : base(
                deviceResources: deviceResources,
                vertexShader: "Content\\Shaders\\VertexShaderPlane.cso",
                VPRTvertexShader: "Content\\Shaders\\VPRTVertexShaderPlane.cso",
                geometryShader: "Content\\Shaders\\GeometryShaderPlane.cso",
                pixelShader: "Content\\Shaders\\PixelShaderPlane.cso") {}

        internal override InputElement[] InputElement => new InputElement[]
            {
                new InputElement(
                    "POSITION", 0,
                    SharpDX.DXGI.Format.R32G32B32_Float, 0, 0,
                    InputClassification.PerVertexData, 0),
                new InputElement(
                    "TEXCOORD", 0,
                    SharpDX.DXGI.Format.R32G32_Float, 12, 0,
                    InputClassification.PerVertexData, 0),
            };

        internal override int VertexSize
            => SharpDX.Utilities.SizeOf<VertexPlane>();
    }
}
