// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using ImageViewer.Common;
using ImageViewer.Content.Utils;
using SharpDX.Direct3D11;
using System.Numerics;

namespace ImageViewer.Content.Renderers.ThreeD
{
    internal class SlideFrameRenderer : FrameRenderer
    {
        private readonly Vector3 bottomLeft;
        private readonly Vector3 topLeft;
        private readonly Vector3 bottomRight;
        private readonly Vector3 topRight;

        public Vector3 BottomLeft => bottomLeft;
        public Vector3 TopLeft => topLeft;
        public Vector3 BottomRight => bottomRight;
        public Vector3 TopRight => topRight;

        internal SlideFrameRenderer(
            DeviceResources deviceResources,
            TextureLoader loader,
            Vector3 bottomLeft,
            Vector3 topLeft,
            Vector3 bottomRight,
            Vector3 topRight,
            float depth,
            float thickness)
            : base(deviceResources, loader, depth, thickness)
        {
            this.bottomLeft = bottomLeft;
            this.topLeft = topLeft;
            this.bottomRight = bottomRight;
            this.topRight = topRight;
        }



        internal override void UpdateGeometry()
        {
            if (vertexBuffer != null)
            {
                RemoveAndDispose(ref vertexBuffer);
            }

            var plane = Plane.CreateFromVertices(topLeft, bottomLeft, topRight);
            var normal = plane.Normal;

            var x_axis = Vector3.Normalize((topRight - topLeft));
            var y_axis = Vector3.Normalize((topLeft - bottomLeft));

            VertexPlane[] vertices =
            {
                new VertexPlane(bottomLeft - Thickness * x_axis - Thickness * y_axis, new Vector2(0f,0f)),
                new VertexPlane(bottomLeft - Thickness * x_axis - Thickness * y_axis + Depth * normal, new Vector2(1f,0f)),
                new VertexPlane(bottomRight + Thickness * x_axis - Thickness * y_axis, new Vector2(0f,1f)),
                new VertexPlane(bottomRight + Thickness * x_axis - Thickness * y_axis + Depth * normal, new Vector2(1f,1f)),

                new VertexPlane(topLeft - Thickness * x_axis + Thickness * y_axis, new Vector2(0f,0f)),
                new VertexPlane(topLeft - Thickness * x_axis + Thickness * y_axis + Depth * normal, new Vector2(1f,0f)),
                new VertexPlane(topRight + Thickness * x_axis + Thickness * y_axis, new Vector2(0f,1f)),
                new VertexPlane(topRight + Thickness * x_axis + Thickness * y_axis + Depth * normal, new Vector2(1f,1f)),

                new VertexPlane(bottomLeft, new Vector2(0f,0f)),
                new VertexPlane(bottomLeft + Depth * normal, new Vector2(1f,0f)),
                new VertexPlane(bottomRight, new Vector2(0f,1f)),
                new VertexPlane(bottomRight + Depth * normal, new Vector2(1f,1f)),

                new VertexPlane(topLeft, new Vector2(0f,0f)),
                new VertexPlane(topLeft + Depth * normal, new Vector2(1f,0f)),
                new VertexPlane(topRight, new Vector2(0f,1f)),
                new VertexPlane(topRight + Depth * normal, new Vector2(1f,1f)),       
            };

            vertexBuffer = ToDispose(Buffer.Create(
                deviceResources.D3DDevice,
                BindFlags.VertexBuffer,
                vertices));
        }
    }
}
