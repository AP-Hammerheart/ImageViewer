// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using ImageViewer.Common;
using SharpDX.Direct3D11;
using System.Threading.Tasks;

namespace ImageViewer.Content
{
    internal abstract class BaseCubeRenderer : BaseRenderer
    {
        private Texture2D texture = null;
        private ShaderResourceView resourceView = null;

        protected readonly TextureLoader loader;
        private bool textureReady = false;

        internal BaseCubeRenderer(DeviceResources deviceResources, TextureLoader loader)
            : base(
                deviceResources,
                "Content\\Shaders\\VertexShaderCube.cso",
                "Content\\Shaders\\VPRTVertexShaderCube.cso",
                "Content\\Shaders\\GeometryShaderCube.cso",
                "Content\\Shaders\\PixelShaderCube.cso") => this.loader = loader;

        internal override InputElement[] InputElement => new InputElement[]
            {
                new InputElement("POSITION", 0, SharpDX.DXGI.Format.R32G32B32_Float,  0, 0, InputClassification.PerVertexData, 0)
            };

        internal override int VertexSize => SharpDX.Utilities.SizeOf<VertexCube>();

        internal override bool TextureReady => textureReady;

        internal string TextureFile { get; set; } = "Content\\Textures\\test.png";

        internal override void SetTextureResource(PixelShaderStage pixelShader)
        {
            if (textureReady)
            {
                pixelShader.SetShaderResource(0, resourceView);
            }
        }

        internal override async Task LoadTextureAsync()
        {
            await base.LoadTextureAsync();

            var shaderResourceDesc = TextureLoader.ShaderDescriptionCube();
            texture = ToDispose(loader.TextureCube(deviceResources, TextureFile));
            resourceView = ToDispose(new ShaderResourceView(deviceResources.D3DDevice, texture, shaderResourceDesc));
            textureReady = true;
        }

        internal override void ReleaseDeviceDependentResources()
        {
            base.ReleaseDeviceDependentResources();

            RemoveAndDispose(ref texture);
            RemoveAndDispose(ref resourceView);
        }
    }
}
