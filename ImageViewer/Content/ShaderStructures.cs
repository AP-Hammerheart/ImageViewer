// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Numerics;

namespace ImageViewer.Content
{
    internal struct ModelConstantBuffer
    {
        internal Matrix4x4 model;
    }

    internal struct VertexPlane
    {
        internal VertexPlane(Vector3 pos, Vector2 uv)
        {
            this.pos = pos;
            this.uv = uv;
        }

        internal Vector3 pos;
        internal Vector2 uv;
    };

    internal struct VertexCube
    {
        public VertexCube(Vector3 pos)
        {
            this.pos = pos;
        }

        public Vector3 pos;
    };
}
