using UnityEngine;

namespace DragonDogStudios.UnitySoFunctional.StateMachines.Utils
{
    public class Triangle
    {
        private static Material lineMaterial;
        private static readonly int ZWrite = Shader.PropertyToID("_ZWrite");
        private static readonly int Cull = Shader.PropertyToID("_Cull");
        private static readonly int DstBlend = Shader.PropertyToID("_DstBlend");
        private static readonly int SrcBlend = Shader.PropertyToID("_SrcBlend");

        private static void CreateLineMaterial()
        {
            if (lineMaterial) return;
            // Unity has a built-in shader that is useful for drawing
            // simple colored things.
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            lineMaterial = new Material(shader)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            // Turn on alpha blending
            lineMaterial.SetInt(SrcBlend, (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            lineMaterial.SetInt(DstBlend, (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            // Turn backface culling off
            lineMaterial.SetInt(Cull, (int)UnityEngine.Rendering.CullMode.Off);
            // Turn off depth writes
            lineMaterial.SetInt(ZWrite, 0);
        }

        private readonly Vector2 _a;
        private readonly Vector2 _b;
        private readonly Vector2 _c;

        public Triangle(Vector2 a, Vector2 b, Vector2 c)
        {
            _a = a;
            _b = b;
            _c = c;
        }

        public void Draw(Color color)
        {
            CreateLineMaterial();
            // Apply the line material
            lineMaterial.color = color;
            lineMaterial.SetPass(0);
            //Debug.Log($"a: {_a}, b: {_b}, c: {_c}");
            DrawGLTriangle(_a, _b, _c);
        }

        public bool Contains(Vector2 mousePoint)
        {
            // Compute vectors
            var v0 = _c - _a;
            var v1 = _b - _a;
            var v2 = mousePoint - _a;
            
            // Compute dot products
            var dot00 = Vector2.Dot(v0, v0);
            var dot01 = Vector2.Dot(v0, v1);
            var dot02 = Vector2.Dot(v0, v2);
            var dot11 = Vector2.Dot(v1, v1);
            var dot12 = Vector2.Dot(v1, v2);
            
            // Compute barycentric coordinates
            var invDenom = 1 / (dot00 * dot11 - dot01 * dot01);
            var u = (dot11 * dot02 - dot01 * dot12) * invDenom;
            var v = (dot00 * dot12 - dot01 * dot02) * invDenom;
            
            // Check if point is in triangle
            return (u >= 0) && (v >= 0) && (u + v < 1);
        }

        private static void DrawGLTriangle(Vector3 a, Vector3 b, Vector3 c)
        {
            GL.PushMatrix();
            GL.Begin(GL.TRIANGLES);
            GL.Vertex(a);
            GL.Vertex(b);
            GL.Vertex(c);
            GL.End();
            GL.PopMatrix();
        }
    }
}