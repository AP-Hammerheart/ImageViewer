// Copyright (c) Microsoft. All rights reserved.
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
        private Texture2D texture = null;
        private ShaderResourceView resourceView = null;

        protected readonly TextureLoader loader;
        private bool textureReady = false;

        internal ObjRenderer(
            DeviceResources deviceResources,
            TextureLoader loader)
            : base(deviceResources)
        {
            this.loader = loader;
            indexBufferFormat = SharpDX.DXGI.Format.R32_UInt;
            Position = new Vector3(0.8f, 0.0f, -0.5f);
        }

        internal string TextureFile { get; } = "Content\\Textures\\pancreas.jpg";
        
        internal string ModelFile { get; } = "Content\\Models\\pancreas.obj";

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
            var normals = new List<Vector3>();
            var uv = new List<Vector2>();
            var faces = new List<int[]>();

            var dic = new Dictionary<Tuple<int, int>, int>();

            foreach (var line in lines)
            {
                if (line.StartsWith("v "))
                {
                    var s = line.Split(' ');
                    vertices.Add(new Vector3(float.Parse(s[1]), float.Parse(s[2]), float.Parse(s[3])));
                }
                else if (line.StartsWith("vn "))
                {
                    var s = line.Split(' ');
                    normals.Add(new Vector3(float.Parse(s[1]), float.Parse(s[2]), float.Parse(s[3])));
                }
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

            var vertices2 = new List<Vector3>();

            foreach (var face in faces)
            {
                var f = face;

                if (dic.TryGetValue(new Tuple<int, int>(f[0], f[1]), out int index0))
                {
                    f[0] = index0;
                }
                else
                {
                    var idx = vertices2.Count;
                    vertices2.Add(vertices[f[0] - 1]);
                    f[0] = idx;
                }

                if (dic.TryGetValue(new Tuple<int, int>(f[3], f[4]), out int index1))
                {
                    f[3] = index1;
                }
                else
                {
                    var idx = vertices2.Count;
                    vertices2.Add(vertices[f[3] - 1]);
                    f[3] = idx;
                }

                if (dic.TryGetValue(new Tuple<int, int>(f[6], f[7]), out int index2))
                {
                    f[6] = index2;
                }
                else
                {
                    var idx = vertices2.Count;
                    vertices2.Add(vertices[f[6] - 1]);
                    f[6] = idx;
                }

                modFaces.Add(f);
            }

            var planeVertices = new VertexPlane[vertices2.Count];
            var planeIndices = new uint[3 * modFaces.Count];
            
            for (var i = 0; i < modFaces.Count; i++ )
            {
                var face = modFaces[i];
                  
                planeVertices[face[0]] = new VertexPlane(vertices2[face[0]], uv[face[1] - 1]);
                planeVertices[face[3]] = new VertexPlane(vertices2[face[3]], uv[face[4] - 1]);
                planeVertices[face[6]] = new VertexPlane(vertices2[face[6]], uv[face[7] - 1]);

                planeIndices[i * 3] = (uint)(face[0] - 1);
                planeIndices[i * 3 + 2] = (uint)(face[3] - 1);
                planeIndices[i * 3 + 1] = (uint)(face[6] - 1);
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
            texture = ToDispose(loader.Texture2D(deviceResources, TextureFile));

            resourceView = ToDispose(new ShaderResourceView(
                deviceResources.D3DDevice,
                texture,
                shaderResourceDesc));

            textureReady = true;
        }

        internal override void SetTextureResource(PixelShaderStage pixelShader)
        {
            if (textureReady)
            {
                pixelShader.SetShaderResource(0, resourceView);
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
            RemoveAndDispose(ref texture);
            RemoveAndDispose(ref resourceView);
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
                * ViewRotator
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
