// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. 
// See LICENSE file in the project root for full license information.

using ImageViewer.Common;
using ImageViewer.Content.Utils;
using SharpDX.Direct3D11;
using System;
using System.Numerics;
using System.Threading.Tasks;

namespace ImageViewer.Content.Renderers.Base
{
    /// <summary>
    /// This sample renderer instantiates a basic rendering pipeline.
    /// </summary>
    internal abstract class BaseRenderer : Disposer
    {
        private readonly string VERTEX_SHADER;
        private readonly string VPRT_VERTEX_SHADER;
        private readonly string GEOMETRY_SHADER;
        private readonly string PIXEL_SHADER;

        private Vector3 position = Vector3.Zero;
        private Matrix4x4 globalRotator = Matrix4x4.Identity;
        private Matrix4x4 viewRotator = Matrix4x4.Identity;

        private float rotationX = 0.0f;
        private float rotationY = 0.0f;
        private float rotationZ = 0.0f;

        // Cached reference to device resources.
        protected DeviceResources deviceResources;

        // Direct3D resources.
        protected SharpDX.Direct3D11.Buffer vertexBuffer;
        protected SharpDX.Direct3D11.Buffer indexBuffer;
        protected SharpDX.Direct3D11.Buffer modelConstantBuffer;

        protected SharpDX.DXGI.Format indexBufferFormat = SharpDX.DXGI.Format.R16_UInt;

        private InputLayout inputLayout;
        private VertexShader vertexShader;
        private GeometryShader geometryShader;
        private PixelShader pixelShader; 
        private SamplerState samplerState;

        // System resources.
        protected ModelConstantBuffer modelConstantBufferData;
        protected int indexCount = 0;

        // Variables used with the rendering loop.
        private bool loadingComplete = false;
        protected bool refreshNeeded = true;

        // If the current D3D Device supports VPRT, 
        // we can avoid using a geometry
        // shader just to set the render target array index.
        private bool usingVprtShaders = false;

        /// <summary>
        /// Loads vertex and pixel shaders from files.
        /// </summary>
        internal BaseRenderer(
            DeviceResources deviceResources,
            string vertexShader,
            string VPRTvertexShader,
            string geometryShader,
            string pixelShader
            )
        {
            VERTEX_SHADER = vertexShader;
            VPRT_VERTEX_SHADER = VPRTvertexShader;
            GEOMETRY_SHADER = geometryShader;
            PIXEL_SHADER = pixelShader;

            this.deviceResources = deviceResources;
        }

        protected virtual void UpdateTransform()
        {
            var modelRotationX = Matrix4x4.CreateRotationX(rotationX);
            var modelRotationY = Matrix4x4.CreateRotationY(rotationY);
            var modelRotationZ = Matrix4x4.CreateRotationZ(rotationZ);

            var modelTranslation = Matrix4x4.CreateTranslation(Position);
            var modelTransform = modelRotationX 
                * modelRotationY 
                * modelRotationZ 
                * modelTranslation 
                * viewRotator 
                * globalRotator;

            modelConstantBufferData.model = Matrix4x4.Transpose(modelTransform);

            // Use the D3D device context to update Direct3D device-based resources.
            var context = deviceResources.D3DDeviceContext;

            // Update the model transform buffer for the hologram.
            context.UpdateSubresource(ref modelConstantBufferData, modelConstantBuffer);
            refreshNeeded = false;
        }

        internal virtual void Update(StepTimer timer)
        {
        }

        /// <summary>
        /// Renders one frame using the vertex and pixel shaders.
        /// On devices that do not support the D3D11_FEATURE_D3D11_OPTIONS3::
        /// VPAndRTArrayIndexFromAnyShaderFeedingRasterizer optional feature,
        /// a pass-through geometry shader is also used to set the render 
        /// target array index.
        /// </summary>
        internal virtual void Render()
        {
            // Loading is asynchronous. Resources must be created before drawing can occur.
            if (!loadingComplete || !TextureReady)
            {
                return;
            }

            if (refreshNeeded)
            {
                UpdateTransform();
            }

            var context = deviceResources.D3DDeviceContext;
            var bufferBinding = new VertexBufferBinding(vertexBuffer, VertexSize, 0);

            context.InputAssembler.SetVertexBuffers(0, bufferBinding);
            context.InputAssembler.SetIndexBuffer(indexBuffer, indexBufferFormat, 0);

            context.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleList;
            context.InputAssembler.InputLayout = inputLayout;

            context.VertexShader.SetShader(vertexShader, null, 0);
            context.VertexShader.SetConstantBuffers(0, modelConstantBuffer);

            if (!usingVprtShaders)
            {
                context.GeometryShader.SetShader(geometryShader, null, 0);
            }

            context.PixelShader.SetSampler(0, samplerState);
            SetTextureResource(context.PixelShader);

            context.PixelShader.SetShader(pixelShader, null, 0);

            context.DrawIndexedInstanced(indexCount, 2, 0, 0, 0);
        }

        /// <summary>
        /// Creates device-based resources to store a constant buffer, object
        /// geometry, and vertex and pixel shaders. In some cases this will also 
        /// store a geometry shader.
        /// </summary>
        internal virtual async Task CreateDeviceDependentResourcesAsync()
        {
            ReleaseDeviceDependentResources();

            usingVprtShaders = deviceResources.D3DDeviceSupportsVprt;

            var folder = Windows.ApplicationModel.Package.Current.InstalledLocation;

            var vertexShaderFileName = usingVprtShaders ? VPRT_VERTEX_SHADER : VERTEX_SHADER;
            var vertexShaderByteCode = await DirectXHelper.ReadDataAsync(
                await folder.GetFileAsync(vertexShaderFileName));

            vertexShader = ToDispose(new VertexShader(
                deviceResources.D3DDevice, 
                vertexShaderByteCode));

            var vertexDesc = InputElement;

            inputLayout = ToDispose(new InputLayout(
                deviceResources.D3DDevice, 
                vertexShaderByteCode, 
                vertexDesc));

            if (!usingVprtShaders)
            {
                var geometryShaderByteCode = await DirectXHelper.ReadDataAsync(
                    await folder.GetFileAsync(GEOMETRY_SHADER));

                geometryShader = ToDispose(new GeometryShader(
                    deviceResources.D3DDevice, 
                    geometryShaderByteCode));
            }

            var pixelShaderByteCode = await DirectXHelper.ReadDataAsync(
                await folder.GetFileAsync(PIXEL_SHADER));

            pixelShader = ToDispose(new PixelShader(
                deviceResources.D3DDevice, 
                pixelShaderByteCode));

            samplerState = ToDispose(new SamplerState(
                deviceResources.D3DDevice, 
                TextureLoader.SamplerStateDescription()));
  
            await LoadTextureAsync();
            LoadGeometry();

            refreshNeeded = true;
            loadingComplete = true;
        }

        internal abstract InputElement[] InputElement { get; }   

        internal abstract void SetTextureResource(PixelShaderStage pixelShader);

        internal abstract bool TextureReady { get; }

        internal abstract int VertexSize { get; }

        internal abstract void LoadGeometry();

        internal virtual async Task LoadTextureAsync()
        {
            await Task.FromResult<object>(null);
            return;
        }

        internal virtual void ReleaseDeviceDependentResources()
        {
            loadingComplete = false;
            usingVprtShaders = false;

            RemoveAndDispose(ref vertexShader);
            RemoveAndDispose(ref inputLayout);
            RemoveAndDispose(ref pixelShader);
            RemoveAndDispose(ref geometryShader);
            RemoveAndDispose(ref modelConstantBuffer);
            RemoveAndDispose(ref vertexBuffer);
            RemoveAndDispose(ref indexBuffer);
            RemoveAndDispose(ref samplerState);
        }

        internal Vector3 Position
        {
            get
            {
                return position;
            }

            set
            {
                position = value;
                refreshNeeded = true;               
            }
        } 

        internal float RotationX
        {
            get
            {
                return rotationX * (180.0f / (float)Math.PI);
            }
            set
            {
                rotationX = (float)Math.IEEERemainder(value 
                    * ((float)Math.PI / 180.0f), 2 * Math.PI);

                refreshNeeded = true;
            }
        }

        internal float RotationY
        {
            get
            {
                return rotationY * (180.0f / (float)Math.PI);
            }
            set
            {
                rotationY = (float)Math.IEEERemainder(value 
                    * ((float)Math.PI / 180.0f), 2 * Math.PI);

                refreshNeeded = true;
            }
        }

        internal float RotationZ
        {
            get
            {
                return rotationZ * (180.0f / (float)Math.PI);
            }
            set
            {
                rotationZ = (float)Math.IEEERemainder(value 
                    * ((float)Math.PI / 180.0f), 2 * Math.PI);

                refreshNeeded = true;
            }
        }

        internal virtual Matrix4x4 GlobalRotator
        {
            get
            {
                return globalRotator;
            }

            set
            {
                globalRotator = value;
                refreshNeeded = true;
            }
        }

        internal virtual Matrix4x4 ViewRotator
        {
            get
            {
                return viewRotator;
            }

            set
            {
                viewRotator = value;
                refreshNeeded = true;
            }
        }
    }
}
