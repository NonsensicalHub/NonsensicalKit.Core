using NonsensicalKit.Core;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace NonsensicalKit.Tools.MeshTool
{
    /// <summary>
    /// 构建自定mesh
    /// </summary>
    public class MeshBuilder
    {
        public List<Vector3> Vertices;
        public List<Vector3> Normals;
        public List<Color> Colors;
        public List<Vector2> UV;
        public List<Vector2> UV2;
        public List<Vector2> UV3;
        public List<int> Triangles;

        private Dictionary<uint, int> _newVectices;

        public MeshBuilder(Mesh mesh)
        {
            Vertices = new List<Vector3>(mesh.vertices);
            Normals = new List<Vector3>(mesh.normals);
            Colors = new List<Color>(mesh.colors);
            UV = new List<Vector2>(mesh.uv);
            UV2 = new List<Vector2>(mesh.uv2);
            UV3 = new List<Vector2>(mesh.uv3);
            Triangles = new List<int>(mesh.triangles);
        }

        public MeshBuilder()
        {
            Vertices = new List<Vector3>();
            Normals = new List<Vector3>();
            Colors = new List<Color>();
            UV = new List<Vector2>();
            UV2 = new List<Vector2>();
            UV3 = new List<Vector2>();
            Triangles = new List<int>();
        }

        public Vector3 GetVerticeByTrianglesIndex(int index)
        {
            return Vertices[Triangles[index]];
        }

        public Vector3 this[int index]
        {
            get
            {
                return Vertices[Triangles[index]];
            }
        }

        public MeshBuilder Clone()
        {
            MeshBuilder temp = new MeshBuilder()
            {
                Vertices = new List<Vector3>(this.Vertices.ToArray()),
                Normals = new List<Vector3>(this.Normals.ToArray()),
                Colors = new List<Color>(this.Colors.ToArray()),
                UV = new List<Vector2>(this.UV.ToArray()),
                UV2 = new List<Vector2>(this.UV2.ToArray()),
                UV3 = new List<Vector2>(this.UV3.ToArray()),
                Triangles = new List<int>(this.Triangles.ToArray())
            };

            return temp;
        }

        public void Apply(Mesh mesh)
        {
            mesh.Clear();
            mesh.SetVertices(Vertices);
            mesh.SetNormals(Normals);
            mesh.SetColors(Colors);
            mesh.SetUVs(0, UV);
            mesh.SetUVs(1, UV2);
            mesh.SetUVs(2, UV3);
            mesh.SetTriangles(Triangles, 0);
        }

        public Mesh ToMesh()
        {
            Mesh mesh = new Mesh();

            mesh.SetVertices(Vertices);
            mesh.SetNormals(Normals);
            mesh.SetColors(Colors);
            mesh.SetUVs(0, UV);
            mesh.SetUVs(1, UV2);
            mesh.SetUVs(2, UV3);
            mesh.SetTriangles(Triangles, 0);
            //mesh.RecalculateNormals();    //不使用时效果更加平滑
            return mesh;
        }

        public void Scale(Vector3 scaleRatio)
        {
            for (int i = 0; i < Vertices.Count; i++)
            {
                Vertices[i] = Vector3.Scale(Vertices[i], scaleRatio);
            }
        }

        public void AddTriangle(Vector3[] vertices, Vector3 normal, Vector2 uv)
        {
            int rawLength = Vertices.Count;

            Vertices.Add(vertices[0]);
            Vertices.Add(vertices[1]);
            Vertices.Add(vertices[2]);

            Normals.Add(normal);
            Normals.Add(normal);
            Normals.Add(normal);

            UV.Add(uv);
            UV.Add(uv);
            UV.Add(uv);

            Triangles.Add(rawLength + 0);
            Triangles.Add(rawLength + 1);
            Triangles.Add(rawLength + 2);
        }

        public void AddCube(Float3 center, Float3 size)
        {
            AddCube(center.ToVector3(), size.ToVector3());
        }

        public void AddCube(Vector3 size)
        {
            Vector3[] point = new Vector3[8];

            float sizeX = size.x * 0.5f;
            float sizeY = size.y * 0.5f;
            float sizeZ = size.z * 0.5f;
            point[0] = new Vector3(sizeX, sizeY, sizeZ);
            point[1] = new Vector3(sizeX, sizeY, sizeZ);
            point[2] = new Vector3(sizeX, sizeY, sizeZ);
            point[3] = new Vector3(sizeX, sizeY, sizeZ);
            point[4] = new Vector3(sizeX, sizeY, sizeZ);
            point[5] = new Vector3(sizeX, sizeY, sizeZ);
            point[6] = new Vector3(sizeX, sizeY, sizeZ);
            point[7] = new Vector3(sizeX, sizeY, sizeZ);

            //init
            Vector2 middle = Vector2.one * 0.5f;

            //front
            AddQuad(Vector3.back, middle, point[0], point[1], point[2], point[3]);

            //back
            AddQuad(Vector3.forward, middle, point[7], point[6], point[5], point[4]);

            //left
            AddQuad(Vector3.left, middle, point[4], point[5], point[1], point[0]);

            //right
            AddQuad(Vector3.right, middle, point[3], point[2], point[6], point[7]);

            //down
            AddQuad(Vector3.down, middle, point[0], point[3], point[7], point[4]);

            //up
            AddQuad(Vector3.up, middle, point[1], point[5], point[6], point[2]);
        }

        public void AddCube(Vector3 center, Vector3 size)
        {
            Vector3[] point = new Vector3[8];
            float centerX = center.x;
            float centerY = center.y;
            float centerZ = center.z;
            float sizeX = size.x * 0.5f;
            float sizeY = size.y * 0.5f;
            float sizeZ = size.z * 0.5f;
            point[0] = new Vector3(centerX - sizeX, centerY - sizeY, centerZ - sizeZ);
            point[1] = new Vector3(centerX - sizeX, centerY + sizeY, centerZ - sizeZ);
            point[2] = new Vector3(centerX + sizeX, centerY + sizeY, centerZ - sizeZ);
            point[3] = new Vector3(centerX + sizeX, centerY - sizeY, centerZ - sizeZ);
            point[4] = new Vector3(centerX - sizeX, centerY - sizeY, centerZ + sizeZ);
            point[5] = new Vector3(centerX - sizeX, centerY + sizeY, centerZ + sizeZ);
            point[6] = new Vector3(centerX + sizeX, centerY + sizeY, centerZ + sizeZ);
            point[7] = new Vector3(centerX + sizeX, centerY - sizeY, centerZ + sizeZ);

            //init
            Vector2 middle = Vector2.one * 0.5f;

            //front
            AddQuad(Vector3.back, middle, point[0], point[1], point[2], point[3]);

            //back
            AddQuad(Vector3.forward, middle, point[7], point[6], point[5], point[4]);

            //left
            AddQuad(Vector3.left, middle, point[4], point[5], point[1], point[0]);

            //right
            AddQuad(Vector3.right, middle, point[3], point[2], point[6], point[7]);

            //down
            AddQuad(Vector3.down, middle, point[0], point[3], point[7], point[4]);

            //up
            AddQuad(Vector3.up, middle, point[1], point[5], point[6], point[2]);
        }

        public void AddQuad(Vector3 normal, Vector2 uv, params Vector3[] vertices)
        {
            int rawLength = Vertices.Count;

            Vertices.Add(vertices[0]);
            Vertices.Add(vertices[1]);
            Vertices.Add(vertices[2]);
            Vertices.Add(vertices[3]);

            Normals.Add(normal);
            Normals.Add(normal);
            Normals.Add(normal);
            Normals.Add(normal);

            UV.Add(uv);
            UV.Add(uv);
            UV.Add(uv);
            UV.Add(uv);

            Triangles.Add(rawLength + 0);
            Triangles.Add(rawLength + 1);
            Triangles.Add(rawLength + 3);

            Triangles.Add(rawLength + 1);
            Triangles.Add(rawLength + 2);
            Triangles.Add(rawLength + 3);
        }

        public void AddQuad(Vector3[] vertices, Vector3 normal, Vector2 uv)
        {
            int rawLength = Vertices.Count;

            Vertices.Add(vertices[0]);
            Vertices.Add(vertices[1]);
            Vertices.Add(vertices[2]);
            Vertices.Add(vertices[3]);

            Normals.Add(normal);
            Normals.Add(normal);
            Normals.Add(normal);
            Normals.Add(normal);

            UV.Add(uv);
            UV.Add(uv);
            UV.Add(uv);
            UV.Add(uv);

            Triangles.Add(rawLength + 0);
            Triangles.Add(rawLength + 1);
            Triangles.Add(rawLength + 3);

            Triangles.Add(rawLength + 1);
            Triangles.Add(rawLength + 2);
            Triangles.Add(rawLength + 3);
        }

        public void AddQuad_2(Vector3[] vertices, Vector3 center, Vector2 uv)
        {
            int rawLength = Vertices.Count;

            Vertices.Add(vertices[0]);
            Vertices.Add(vertices[1]);
            Vertices.Add(vertices[2]);
            Vertices.Add(vertices[3]);

            Normals.Add(vertices[0] - center);
            Normals.Add(vertices[1] - center);
            Normals.Add(vertices[2] - center);
            Normals.Add(vertices[3] - center);

            UV.Add(uv);
            UV.Add(uv);
            UV.Add(uv);
            UV.Add(uv);

            Triangles.Add(rawLength + 0);
            Triangles.Add(rawLength + 1);
            Triangles.Add(rawLength + 3);

            Triangles.Add(rawLength + 1);
            Triangles.Add(rawLength + 2);
            Triangles.Add(rawLength + 3);
        }

        public void AddQuad_3(Vector3[] vertices, Vector3[] centers, Vector2 uv)
        {
            int rawLength = Vertices.Count;

            Vertices.Add(vertices[0]);
            Vertices.Add(vertices[1]);
            Vertices.Add(vertices[2]);
            Vertices.Add(vertices[3]);

            Normals.Add(vertices[0] - centers[0]);
            Normals.Add(vertices[1] - centers[1]);
            Normals.Add(vertices[2] - centers[2]);
            Normals.Add(vertices[3] - centers[3]);

            UV.Add(uv);
            UV.Add(uv);
            UV.Add(uv);
            UV.Add(uv);

            Triangles.Add(rawLength + 0);
            Triangles.Add(rawLength + 1);
            Triangles.Add(rawLength + 3);

            Triangles.Add(rawLength + 1);
            Triangles.Add(rawLength + 2);
            Triangles.Add(rawLength + 3);
        }

        public void AddRound(Vector3 center, float radius, Vector3 dir, int smoothness)
        {
            if (smoothness < 3)
            {
                throw new Exception("点数过少");
            }
            Vector3 dir1 = VectorTool.GetCommonVerticalLine(dir, dir);
            Vector3 dir2 = VectorTool.GetCommonVerticalLine(dir, dir1);
            float partAngle = (2 * Mathf.PI) / smoothness;
            Vector3[] pointArray = new Vector3[smoothness];

            for (int i = 0; i < smoothness; i++)
            {
                pointArray[i] = center + radius * dir1 * Mathf.Sin(partAngle * i) + radius * dir2 * Mathf.Cos(partAngle * i);
            }
            for (int i = 0; i < pointArray.Length; i++)
            {
                int next = i + 1;
                if (next >= smoothness)
                {
                    next = 0;
                }
                AddTriangle(new Vector3[] { center, pointArray[i], pointArray[next] }, dir, Vector2.one * 0.5f);
            }
        }

        public void AddRing(Vector3 center, float innerDiameter, float outerDiameter, Vector3 dir, int smoothness)
        {
            if (smoothness < 3)
            {
                throw new Exception("点数过少");
            }
            if (innerDiameter == 0)
            {
                AddRound(center, outerDiameter, dir, smoothness);
            }

            Vector3 dir1 = VectorTool.GetCommonVerticalLine(dir, dir);
            Vector3 dir2 = VectorTool.GetCommonVerticalLine(dir, dir1);
            float partAngle = (2 * Mathf.PI) / smoothness;
            Vector3[] pointArray1 = new Vector3[smoothness];
            Vector3[] pointArray2 = new Vector3[smoothness];

            for (int i = 0; i < smoothness; i++)
            {
                pointArray1[i] = center + innerDiameter * dir1 * Mathf.Sin(partAngle * i) + innerDiameter * dir2 * Mathf.Cos(partAngle * i);
                pointArray2[i] = center + outerDiameter * dir1 * Mathf.Sin(partAngle * i) + outerDiameter * dir2 * Mathf.Cos(partAngle * i);
            }

            for (int i = 0; i < smoothness; i++)
            {
                int next = i + 1;
                if (next >= smoothness)
                {
                    next = 0;
                }

                //AddQuad(new Vector3[] { pointArray1[i], pointArray2[i], pointArray2[next], pointArray1[next] }, (pointArray1[i] + pointArray2[i] + pointArray2[next] + pointArray1[next]) * 0.25f, new Vector2(0.5f, 0.5f));
                AddQuad(new Vector3[] { pointArray1[i], pointArray2[i], pointArray2[next], pointArray1[next] }, -dir, new Vector2(0.5f, 0.5f));
            }
        }

        public void AddRing3D(Vector3 ringSide1, float ringSide1Radius, Vector3 ringSide2, float ringSide2Radius, Vector3 dir, int smoothness)
        {
            if (smoothness < 3)
            {
                throw new Exception("点数过少");
            }

            Vector3 dir1 = VectorTool.GetCommonVerticalLine(dir, dir);
            Vector3 dir2 = VectorTool.GetCommonVerticalLine(dir, dir1);

            float partAngle = (2 * Mathf.PI) / smoothness;
            Vector3[] pointArray1 = new Vector3[smoothness];
            Vector3[] pointArray2 = new Vector3[smoothness];
            for (int i = 0; i < smoothness; i++)
            {
                pointArray1[i] = ringSide1 + ringSide1Radius * dir1 * Mathf.Sin(partAngle * i) + ringSide1Radius * dir2 * Mathf.Cos(partAngle * i);
                pointArray2[i] = ringSide2 + ringSide2Radius * dir1 * Mathf.Sin(partAngle * i) + ringSide2Radius * dir2 * Mathf.Cos(partAngle * i);
            }

            for (int i = 0; i < smoothness; i++)
            {
                int next = i + 1;
                if (next >= smoothness)
                {
                    next = 0;
                }
                // AddQuad(new Vector3[] { pointArray1[i], pointArray2[i], pointArray2[next], pointArray1[next] }, (pointArray1[i] + pointArray2[i] + pointArray2[next] + pointArray1[next]) * 0.25f- (ringSide1+ringSide2)*0.5f, new Vector2(0.5f, 0.5f));
                AddQuad_3(new Vector3[] { pointArray1[i], pointArray2[i], pointArray2[next], pointArray1[next] }, new Vector3[] { ringSide1, ringSide2, ringSide2, ringSide1 }, new Vector2(0.5f, 0.5f));
            }
        }

        /// <summary>
        /// 用两个不平行的圆相连作成环
        /// </summary>
        /// <param name="ringSide1"></param>
        /// <param name="ringSide1Radius"></param>
        /// <param name="dir2"></param>
        /// <param name="ringSide2"></param>
        /// <param name="ringSide2Radius"></param>
        /// <param name="dir2"></param>
        /// <param name="smoothness"></param>
        /// <exception cref="Exception"></exception>
        public void AddRing3D(Vector3 ringSide1, float ringSide1Radius, Vector3 d1, Vector3 ringSide2, float ringSide2Radius, Vector3 d2, int smoothness)
        {
            if (smoothness < 3)
            {
                throw new Exception("点数过少");
            }

            Vector3 d3 = VectorTool.GetCommonVerticalLine(d1, d2);
            Vector3 d1V = VectorTool.GetCommonVerticalLine(d1, d3);
            Vector3 d2V = VectorTool.GetCommonVerticalLine(d2, d3);

            float partAngle = (2 * Mathf.PI) / smoothness;
            Vector3[] pointArray1 = new Vector3[smoothness];
            Vector3[] pointArray2 = new Vector3[smoothness];

            for (int i = 0; i < smoothness; i++)
            {
                pointArray1[i] = ringSide1 + ringSide1Radius * d3 * Mathf.Sin(partAngle * i) + ringSide1Radius * d1V * Mathf.Cos(partAngle * i);
                pointArray2[i] = ringSide2 + ringSide2Radius * d3 * Mathf.Sin(partAngle * i) + ringSide2Radius * d2V * Mathf.Cos(partAngle * i);
            }

            for (int i = 0; i < smoothness; i++)
            {
                int next = i + 1;
                if (next >= smoothness)
                {
                    next = 0;
                }
                // AddQuad(new Vector3[] { pointArray1[i], pointArray2[i], pointArray2[next], pointArray1[next] }, (pointArray1[i] + pointArray2[i] + pointArray2[next] + pointArray1[next]) * 0.25f- (ringSide1+ringSide2)*0.5f, new Vector2(0.5f, 0.5f));
                AddQuad_3(new Vector3[] { pointArray1[i], pointArray2[i], pointArray2[next], pointArray1[next] }, new Vector3[] { ringSide1, ringSide2, ringSide2, ringSide1 }, new Vector2(0.5f, 0.5f));
            }
        }

        #region Subdivide4 (2x2)
        private int GetNewVertex4(int i1, int i2)
        {
            int newIndex = Vertices.Count;
            uint t1 = ((uint)i1 << 16) | (uint)i2;
            uint t2 = ((uint)i2 << 16) | (uint)i1;
            if (_newVectices.ContainsKey(t2))
                return _newVectices[t2];
            if (_newVectices.ContainsKey(t1))
                return _newVectices[t1];

            _newVectices.Add(t1, newIndex);

            Vertices.Add((Vertices[i1] + Vertices[i2]) * 0.5f);
            if (Normals.Count > 0)
                Normals.Add((Normals[i1] + Normals[i2]).normalized);
            if (Colors.Count > 0)
                Colors.Add((Colors[i1] + Colors[i2]) * 0.5f);
            if (UV.Count > 0)
                UV.Add((UV[i1] + UV[i2]) * 0.5f);
            if (UV2.Count > 0)
                UV2.Add((UV2[i1] + UV2[i2]) * 0.5f);
            if (UV3.Count > 0)
                UV3.Add((UV3[i1] + UV3[i2]) * 0.5f);

            return newIndex;
        }


        /// <summary>
        /// Devides each triangles into 4. A quad(2 tris) will be splitted into 2x2 quads( 8 tris )
        /// </summary>
        /// <param name="mesh"></param>
        public void Subdivide4()
        {
            _newVectices = new Dictionary<uint, int>();


            for (int i = 0; i < Triangles.Count; i += 3)
            {
                int i1 = Triangles[i + 0];
                int i2 = Triangles[i + 1];
                int i3 = Triangles[i + 2];

                int a = GetNewVertex4(i1, i2);
                int b = GetNewVertex4(i2, i3);
                int c = GetNewVertex4(i3, i1);
                this.Triangles.Add(i1); this.Triangles.Add(a); this.Triangles.Add(c);
                this.Triangles.Add(i2); this.Triangles.Add(b); this.Triangles.Add(a);
                this.Triangles.Add(i3); this.Triangles.Add(c); this.Triangles.Add(b);
                this.Triangles.Add(a); this.Triangles.Add(b); this.Triangles.Add(c); // center triangle
            }
        }
        #endregion Subdivide4 (2x2)

        #region Subdivide9 (3x3)
        private int GetNewVertex9(int i1, int i2, int i3)
        {
            int newIndex = Vertices.Count;

            // center points don't go into the edge list
            if (i3 == i1 || i3 == i2)
            {
                uint t1 = ((uint)i1 << 16) | (uint)i2;
                if (_newVectices.ContainsKey(t1))
                    return _newVectices[t1];
                _newVectices.Add(t1, newIndex);
            }

            // calculate new vertex
            Vertices.Add((Vertices[i1] + Vertices[i2] + Vertices[i3]) / 3.0f);
            if (Normals.Count > 0)
                Normals.Add((Normals[i1] + Normals[i2] + Normals[i3]).normalized);
            if (Colors.Count > 0)
                Colors.Add((Colors[i1] + Colors[i2] + Colors[i3]) / 3.0f);
            if (UV.Count > 0)
                UV.Add((UV[i1] + UV[i2] + UV[i3]) / 3.0f);
            if (UV2.Count > 0)
                UV2.Add((UV2[i1] + UV2[i2] + UV2[i3]) / 3.0f);
            if (UV3.Count > 0)
                UV3.Add((UV3[i1] + UV3[i2] + UV3[i3]) / 3.0f);
            return newIndex;
        }


        /// <summary>
        /// Devides each triangles into 9. A quad(2 tris) will be splitted into 3x3 quads( 18 tris )
        /// </summary>
        /// <param name="mesh"></param>
        public void Subdivide9()
        {
            _newVectices = new Dictionary<uint, int>();

            for (int i = 0; i < Triangles.Count; i += 3)
            {
                int i1 = Triangles[i + 0];
                int i2 = Triangles[i + 1];
                int i3 = Triangles[i + 2];

                int a1 = GetNewVertex9(i1, i2, i1);
                int a2 = GetNewVertex9(i2, i1, i2);
                int b1 = GetNewVertex9(i2, i3, i2);
                int b2 = GetNewVertex9(i3, i2, i3);
                int c1 = GetNewVertex9(i3, i1, i3);
                int c2 = GetNewVertex9(i1, i3, i1);

                int d = GetNewVertex9(i1, i2, i3);

                this.Triangles.Add(i1); this.Triangles.Add(a1); this.Triangles.Add(c2);
                this.Triangles.Add(i2); this.Triangles.Add(b1); this.Triangles.Add(a2);
                this.Triangles.Add(i3); this.Triangles.Add(c1); this.Triangles.Add(b2);
                this.Triangles.Add(d); this.Triangles.Add(a1); this.Triangles.Add(a2);
                this.Triangles.Add(d); this.Triangles.Add(b1); this.Triangles.Add(b2);
                this.Triangles.Add(d); this.Triangles.Add(c1); this.Triangles.Add(c2);
                this.Triangles.Add(d); this.Triangles.Add(c2); this.Triangles.Add(a1);
                this.Triangles.Add(d); this.Triangles.Add(a2); this.Triangles.Add(b1);
                this.Triangles.Add(d); this.Triangles.Add(b2); this.Triangles.Add(c1);
            }
        }
        #endregion Subdivide9 (3x3)

        #region Subdivide
        /// <summary>
        /// http://wiki.unity3d.com/index.php?title=MeshHelper&oldid=20389
        /// This functions subdivides the mesh based on the level parameter
        /// Note that only the 4 and 9 subdivides are supported so only those divides
        /// are possible. [2,3,4,6,8,9,12,16,18,24,27,32,36,48,64, ...]
        /// The function tried to approximate the desired level 
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="level">Should be a number made up of (2^x * 3^y)
        /// [2,3,4,6,8,9,12,16,18,24,27,32,36,48,64, ...]
        /// </param>
        public void Subdivide(int level)
        {
            if (level < 2)
                return;
            while (level > 1)
            {
                // remove prime factor 3
                while (level % 3 == 0)
                {
                    Subdivide9();
                    level /= 3;
                }
                // remove prime factor 2
                while (level % 2 == 0)
                {
                    Subdivide4();
                    level /= 2;
                }
                // try to approximate. All other primes are increased by one
                // so they can be processed
                if (level > 3)
                    level++;
            }
        }
        #endregion Subdivide

        public void Save(BinaryWriter writer)
        {
            writer.Write(Vertices.Count);
            foreach (var item in Vertices)
            {
                writer.WriteVector3(item);
            }
            writer.Write(Normals.Count);
            foreach (var item in Normals)
            {
                writer.WriteVector3(item);
            }
            writer.Write(Colors.Count);
            foreach (var item in Colors)
            {
                writer.WriteColor(item);
            }
            writer.Write(UV.Count);
            foreach (var item in UV)
            {
                writer.WriteVector2(item);
            }
            writer.Write(UV2.Count);
            foreach (var item in UV2)
            {
                writer.WriteVector2(item);
            }
            writer.Write(UV3.Count);
            foreach (var item in UV3)
            {
                writer.WriteVector2(item);
            }
            writer.Write(Triangles.Count);
            foreach (var item in Triangles)
            {
                writer.Write(item);
            }
        }

        public void Load(BinaryReader reader)
        {
            int verticesCount = reader.ReadInt32();
            Vertices = new List<Vector3>(verticesCount);
            for (int i = 0; i < verticesCount; i++)
            {
                Vertices.Add(reader.ReadVector3());
            }
            int normalsCount = reader.ReadInt32();
            Normals = new List<Vector3>(normalsCount);
            for (int i = 0; i < normalsCount; i++)
            {
                Normals.Add(reader.ReadVector3());
            }
            int colorsCount = reader.ReadInt32();
            Colors = new List<Color>(colorsCount);
            for (int i = 0; i < colorsCount; i++)
            {
                Colors.Add(reader.ReadColor());
            }
            int uvCount = reader.ReadInt32();
            UV = new List<Vector2>(uvCount);
            for (int i = 0; i < uvCount; i++)
            {
                UV.Add(reader.ReadVector2());
            }
            int uv2Count = reader.ReadInt32();
            UV2 = new List<Vector2>(uv2Count);
            for (int i = 0; i < uv2Count; i++)
            {
                UV2.Add(reader.ReadVector2());
            }
            int uv3Count = reader.ReadInt32();
            UV3 = new List<Vector2>(uv3Count);
            for (int i = 0; i < uv3Count; i++)
            {
                UV3.Add(reader.ReadVector2());
            }
            int triangleCount = reader.ReadInt32();
            Triangles = new List<int>(triangleCount);
            for (int i = 0; i < triangleCount; i++)
            {
                Triangles.Add(reader.ReadInt32());
            }
        }
    }
}
