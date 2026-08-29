using Allumeria;
using Allumeria.DataManagement.AssetLoading;
using Allumeria.Meshing;
using Allumeria.Rendering;
using OpenTK.Mathematics;

namespace CinematicCamera.Utils;

internal static class LineRenderer
{
    private const float MinimumSegmentLengthSquared = 1e-6f;
    private const float PrismRadius = 0.035f;

    private static Texture? texture;
    private static Mesh? mesh;
    private static Shader? shader;
    private static Vector4 lineColor = Vector4.One;

    internal static void SetColor(Vector4 value) => lineColor = value;

    internal static void Initialize(Shader pathShader, string textureName)
    {
        shader ??= pathShader;
        texture ??= AssetManager.textures.GetValueOrDefault(textureName);
    }

    internal static void SetPath(IReadOnlyList<Vector3> path)
    {
        mesh?.Unload();

        var meshGen = new MeshGenerator();
        meshGen.Initialise();

        for (var index = 0; index < path.Count - 1; index++)
        {
            var start = path[index];
            var end = path[index + 1];
            if ((end - start).LengthSquared < MinimumSegmentLengthSquared)
                continue;

            AddSegmentPrism(meshGen, start, end, PrismRadius);
        }

        mesh = meshGen.GenerateMesh();
    }

    internal static void Clear()
    {
        mesh?.Unload();
        mesh = null;
    }

    internal static void Render()
    {
        if (shader == null || texture == null || mesh == null || mesh?.indices.Length == 0)
            return;

        shader.SetUniformMat4("view", Game.camera.viewMatrix);
        shader.SetUniformMat4("projection", Game.camera.projectionMatrix);
        shader.SetUniformVec4("lineColor", lineColor);
        mesh?.Draw(shader, texture, Vector3.Zero, Matrix4.Identity);
    }

    internal static void Dispose()
    {
        mesh?.Unload();
        mesh = null;

        shader?.Dispose();
        shader = null;
    }

    private static void AddSegmentPrism(
        MeshGenerator meshGen,
        Vector3 start,
        Vector3 end,
        float radius
    )
    {
        var segment = end - start;
        if (segment.LengthSquared < MinimumSegmentLengthSquared)
            return;

        var forward = Vector3.Normalize(segment);
        var right = Vector3.Cross(forward, Vector3.UnitY);
        if (right.LengthSquared < MinimumSegmentLengthSquared)
            right = Vector3.Cross(forward, Vector3.UnitX);

        var rawRight = Vector3.Normalize(right);
        var rawUp = Vector3.Normalize(Vector3.Cross(rawRight, forward));

        // Rotate the cross-section 45 degrees around the segment axis.
        const float cos45 = 0.70710678f;
        const float sin45 = 0.70710678f;
        right = (rawRight * cos45 - rawUp * sin45) * radius;
        var up = (rawRight * sin45 + rawUp * cos45) * radius;

        var a = start - right - up;
        var b = start + right - up;
        var c = start + right + up;
        var d = start - right + up;
        var e = end - right - up;
        var f = end + right - up;
        var g = end + right + up;
        var h = end - right + up;
        var uv = new[]
        {
            new Vector2(1f, 0f),
            new Vector2(1f, 1f),
            new Vector2(0f, 1f),
            new Vector2(0f, 0f),
        };

        meshGen.AddQuadWithUV([a, b, f, e], uv);
        meshGen.AddQuadWithUV([b, c, g, f], uv);
        meshGen.AddQuadWithUV([c, d, h, g], uv);
        meshGen.AddQuadWithUV([d, a, e, h], uv);
    }
}
