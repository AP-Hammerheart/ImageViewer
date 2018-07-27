using System.Numerics;

namespace ImageViewer.Content
{
    /// <summary>
    /// Constant buffer used to send hologram position transform to the shader pipeline.
    /// </summary>
    internal struct ModelConstantBuffer
    {
        public Matrix4x4 model;
    }

    internal struct VertexPlane
    {
        public VertexPlane(Vector3 pos, Vector2 uv)
        {
            this.pos = pos;
            this.uv = uv;
        }

        public Vector3 pos;
        public Vector2 uv;
    };
}
