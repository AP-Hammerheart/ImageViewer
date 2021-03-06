﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using ImageViewer.Common;
using ImageViewer.Content.Utils;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ImageViewer.Content.Renderers.Base
{
    internal class ObjRenderer : BasePlaneRenderer
    {
        private Texture2D texture1 = null;
        private Texture2D texture2 = null;

        private ShaderResourceView resourceView1 = null;
        private ShaderResourceView resourceView2 = null;

        protected readonly TextureLoader loader;
        private bool textureReady = false;

        internal ObjRenderer(
            DeviceResources deviceResources,
            TextureLoader loader)
            : base(deviceResources)
        {
            this.loader = loader;
            indexBufferFormat = SharpDX.DXGI.Format.R32_UInt;
        }

        internal bool Colored { get; set; } = false;

        internal string TextureFile1 { get; } = "Content\\Textures\\pancreas_head.jpg";

        internal string TextureFile2 { get; } = "Content\\Textures\\pancreas_head_colored.jpg";
        
        internal string ModelFile { get; } = "Content\\Models\\pancreas_head.model";

        internal override bool TextureReady => textureReady;

        private float scale = 1.0f;

        internal float Scale
        {
            get
            {
                return scale;
            }

            set
            {
                scale = value;
                refreshNeeded = true;
            }
        }

        internal override void LoadGeometry()
        {
            var lines = File.ReadAllLines(ModelFile, Encoding.UTF8);

            var vertices = new List<Vector3>();
            //var normals = new List<Vector3>();
            var uv = new List<Vector2>();
            var faces = new List<int[]>();

            var dictionary = new Dictionary<Tuple<int, int>, int>();

            foreach (var line in lines)
            {
                if (line.StartsWith("v "))
                {
                    var s = line.Split(' ');
                    vertices.Add(new Vector3(float.Parse(s[1]), float.Parse(s[2]), float.Parse(s[3])));
                }
                //else if (line.StartsWith("vn "))
                //{
                //    var s = line.Split(' ');
                //    normals.Add(new Vector3(float.Parse(s[1]), float.Parse(s[2]), float.Parse(s[3])));
                //}
                else if (line.StartsWith("vt "))
                {
                    var s = line.Split(' ');
                    uv.Add(new Vector2(float.Parse(s[1]), float.Parse(s[2])));
                }
                else if (line.StartsWith("f "))
                {
                    var s = line.Split(' ');
                    var s1 = s[1].Split('/');
                    var s2 = s[2].Split('/');
                    var s3 = s[3].Split('/');

                    var face = new int[] {
                        int.Parse(s1[0]), int.Parse(s1[1]), int.Parse(s1[2]),
                        int.Parse(s2[0]), int.Parse(s2[1]), int.Parse(s2[2]),
                        int.Parse(s3[0]), int.Parse(s3[1]), int.Parse(s3[2]),
                    };

                    faces.Add(face);
                }
            }

            var modFaces = new List<int[]>();
            var uvVertices = new List<Vector3>();

            foreach (var face in faces)
            {
                var f = face;

                if (dictionary.TryGetValue(new Tuple<int, int>(f[0], f[1]), out int index0))
                {
                    f[0] = index0;
                }
                else
                {
                    var idx = uvVertices.Count;
                    uvVertices.Add(vertices[f[0] - 1]);
                    f[0] = idx;
                    dictionary.Add(new Tuple<int, int>(f[0], f[1]), idx);
                }

                if (dictionary.TryGetValue(new Tuple<int, int>(f[3], f[4]), out int index1))
                {
                    f[3] = index1;
                }
                else
                {
                    var idx = uvVertices.Count;
                    uvVertices.Add(vertices[f[3] - 1]);
                    f[3] = idx;
                    dictionary.Add(new Tuple<int, int>(f[3], f[4]), idx);
                }

                if (dictionary.TryGetValue(new Tuple<int, int>(f[6], f[7]), out int index2))
                {
                    f[6] = index2;
                }
                else
                {
                    var idx = uvVertices.Count;
                    uvVertices.Add(vertices[f[6] - 1]);
                    f[6] = idx;
                    dictionary.Add(new Tuple<int, int>(f[6], f[7]), idx);
                }

                modFaces.Add(f);
            }

            var planeVertices = new VertexPlane[uvVertices.Count];
            var planeIndices = new uint[3 * modFaces.Count];
            
            for (var i = 0; i < modFaces.Count; i++ )
            {
                var face = modFaces[i];
                  
                planeVertices[face[0]] = new VertexPlane(uvVertices[face[0]], uv[face[1] - 1]);
                planeVertices[face[3]] = new VertexPlane(uvVertices[face[3]], uv[face[4] - 1]);
                planeVertices[face[6]] = new VertexPlane(uvVertices[face[6]], uv[face[7] - 1]);

                planeIndices[i * 3] = (uint)face[0];
                planeIndices[i * 3 + 2] = (uint)face[3];
                planeIndices[i * 3 + 1] = (uint)face[6];
            }

            vertexBuffer = ToDispose(SharpDX.Direct3D11.Buffer.Create(
                deviceResources.D3DDevice,
                BindFlags.VertexBuffer,
                planeVertices));

            indexCount = planeIndices.Length;
            indexBuffer = ToDispose(SharpDX.Direct3D11.Buffer.Create(
                deviceResources.D3DDevice,
                BindFlags.IndexBuffer,
                planeIndices));

            modelConstantBuffer = ToDispose(SharpDX.Direct3D11.Buffer.Create(
                deviceResources.D3DDevice,
                BindFlags.ConstantBuffer,
                ref modelConstantBufferData));
        }

        internal override async Task LoadTextureAsync()
        {
            await base.LoadTextureAsync();

            var shaderResourceDesc = TextureLoader.ShaderDescription();
            texture1 = ToDispose(loader.Texture2D(deviceResources, TextureFile1));

            resourceView1 = ToDispose(new ShaderResourceView(
                deviceResources.D3DDevice,
                texture1,
                shaderResourceDesc));

            texture2 = ToDispose(loader.Texture2D(deviceResources, TextureFile2));

            resourceView2 = ToDispose(new ShaderResourceView(
                deviceResources.D3DDevice,
                texture2,
                shaderResourceDesc));

            textureReady = true;
        }

        internal override void SetTextureResource(PixelShaderStage pixelShader)
        {
            if (textureReady)
            {
                if (Colored)
                {
                    pixelShader.SetShaderResource(0, resourceView2);
                }
                else
                {
                    pixelShader.SetShaderResource(0, resourceView1);
                }  
            }
        }

        internal override void ReleaseDeviceDependentResources()
        {
            base.ReleaseDeviceDependentResources();
            FreeResources();
        }

        protected override void Dispose(bool disposeManagedResources)
        {
            base.Dispose(disposeManagedResources);
            FreeResources();
        }

        private void FreeResources()
        {
            RemoveAndDispose(ref texture1);
            RemoveAndDispose(ref texture2);

            RemoveAndDispose(ref resourceView1);
            RemoveAndDispose(ref resourceView2);
        }

        protected override void UpdateTransform()
        {
            var modelRotationX = Matrix4x4.CreateRotationX(RotationX);
            var modelRotationY = Matrix4x4.CreateRotationY(RotationY);
            var modelRotationZ = Matrix4x4.CreateRotationZ(RotationZ);

            var scale = Matrix4x4.CreateScale(Scale, Position);

            var modelTranslation = Matrix4x4.CreateTranslation(Position);
            var modelTransform = 
                modelRotationX
                * modelRotationY
                * modelRotationZ
                * modelTranslation
                * scale
                * GlobalRotator;

            modelConstantBufferData.model = Matrix4x4.Transpose(modelTransform);

            // Use the D3D device context to update Direct3D device-based resources.
            var context = deviceResources.D3DDeviceContext;

            // Update the model transform buffer for the hologram.
            context.UpdateSubresource(ref modelConstantBufferData, modelConstantBuffer);
            refreshNeeded = false;
        }
    }
}
