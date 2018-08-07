using System.Numerics;

namespace ImageViewer.Content
{
    /// <summary>
    /// Constant buffer used to send hologram position transform to the shader pipeline.
    /// </summary>
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
}
