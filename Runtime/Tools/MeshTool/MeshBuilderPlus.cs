using NonsensicalKit.Core;
using NonsensicalKit.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace NonsensicalKit.Tools.MeshTool
{
    /// <summary>
    /// 构建自定mesh
    /// </summary>
    public class MeshBuilderPlus
    {
        public Vector3[] Vertices;
        public Vector3[] Normals;
        public Color[] Colors;
        public Vector2[] UV;
        public int[] Triangles;

        public MeshBuilderPlus(Mesh mesh)
        {
            Vertices = mesh.vertices;
            Normals = mesh.normals;
            Colors = mesh.colors;
            UV = mesh.uv;
            Triangles = mesh.triangles;
        }

        public MeshBuilderPlus()
        {
            Vertices = new Vector3[0];
            Normals = new Vector3[0];
            Colors = new Color[0];
            UV = new Vector2[0];
            Triangles = new int[0];
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

        public void Update(Mesh mesh)
        {
            mesh.SetVertices(Vertices);
            mesh.SetNormals(Normals);
            mesh.SetColors(Colors);
            mesh.SetUVs(0, UV);
            mesh.SetTriangles(Triangles, 0);
        }

        public void Apply(Mesh mesh)
        {
            mesh.Clear();
            mesh.SetVertices(Vertices);
            mesh.SetNormals(Normals);
            mesh.SetColors(Colors);
            mesh.SetUVs(0, UV);
            mesh.SetTriangles(Triangles, 0);
        }

        public Mesh ToMesh()
        {
            Mesh mesh = new Mesh();

            mesh.SetVertices(Vertices);
            mesh.SetNormals(Normals);
            mesh.SetColors(Colors);
            mesh.SetUVs(0, UV);
            mesh.SetTriangles(Triangles, 0);
            //mesh.RecalculateNormals();    //不使用时效果更加平滑
            return mesh;
        }

        public void Scale(Vector3 scaleRatio)
        {
            for (int i = 0; i < Vertices.Length; i++)
            {
                Vertices[i] = Vector3.Scale(Vertices[i], scaleRatio);
            }
        }

        public void AddTriangle(Vector3[] vertices, Vector3 normal, Vector2 uv)
        {
            int vLength = Vertices.Length;
            int tLength = Triangles.Length;

            Array.Resize(ref Vertices, vLength + 3);
            Array.Resize(ref Normals, vLength + 3);
            Array.Resize(ref UV, vLength + 3);
            Array.Resize(ref Triangles, tLength + 3);

            Vertices[vLength] = (vertices[0]);
            Vertices[vLength + 1] = (vertices[1]);
            Vertices[vLength + 2] = (vertices[2]);

            Normals[vLength] = (normal);
            Normals[vLength + 1] = (normal);
            Normals[vLength + 2] = (normal);

            UV[vLength] = (uv);
            UV[vLength + 1] = (uv);
            UV[vLength + 2] = (uv);

            Triangles[tLength] = (vLength);
            Triangles[tLength + 1] = (vLength + 1);
            Triangles[tLength + 2] = (vLength + 2);
        }

        public void AddTriangle_2(Vector3[] vertices, Vector3 normal, Vector2 uv, int vLength, int tLength)
        {
            Vertices[vLength] = (vertices[0]);
            Vertices[vLength + 1] = (vertices[1]);
            Vertices[vLength + 2] = (vertices[2]);

            Normals[vLength] = (normal);
            Normals[vLength + 1] = (normal);
            Normals[vLength + 2] = (normal);

            UV[vLength] = (uv);
            UV[vLength + 1] = (uv);
            UV[vLength + 2] = (uv);

            Triangles[tLength] = (vLength);
            Triangles[tLength + 1] = (vLength + 1);
            Triangles[tLength + 2] = (vLength + 2);
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
            int vLength = Vertices.Length;
            int tLength = Triangles.Length;
            Array.Resize(ref Vertices, vLength + 4);
            Array.Resize(ref Normals, vLength + 4);
            Array.Resize(ref UV, vLength + 4);
            Array.Resize(ref Triangles, tLength + 6);

            Vertices[vLength] = (vertices[0]);
            Vertices[vLength + 1] = (vertices[1]);
            Vertices[vLength + 2] = (vertices[2]);
            Vertices[vLength + 3] = (vertices[3]);

            Normals[vLength] = (normal);
            Normals[vLength + 1] = (normal);
            Normals[vLength + 2] = (normal);
            Normals[vLength + 3] = (normal);

            UV[vLength] = (uv);
            UV[vLength + 1] = (uv);
            UV[vLength + 2] = (uv);
            UV[vLength + 3] = (uv);

            Triangles[tLength] = (vLength + 0);
            Triangles[tLength + 1] = (vLength + 1);
            Triangles[tLength + 2] = (vLength + 3);

            Triangles[tLength + 3] = (vLength + 1);
            Triangles[tLength + 4] = (vLength + 2);
            Triangles[tLength + 5] = (vLength + 3);
        }

        public void AddQuad(Vector3[] vertices, Vector3 normal, Vector2 uv)
        {
            int vLength = Vertices.Length;
            int tLength = Triangles.Length;
            Array.Resize(ref Vertices, vLength + 4);
            Array.Resize(ref Normals, vLength + 4);
            Array.Resize(ref UV, vLength + 4);
            Array.Resize(ref Triangles, tLength + 6);

            Vertices[vLength] = (vertices[0]);
            Vertices[vLength + 1] = (vertices[1]);
            Vertices[vLength + 2] = (vertices[2]);
            Vertices[vLength + 3] = (vertices[3]);

            Normals[vLength] = (normal);
            Normals[vLength + 1] = (normal);
            Normals[vLength + 2] = (normal);
            Normals[vLength + 3] = (normal);

            UV[vLength] = (uv);
            UV[vLength + 1] = (uv);
            UV[vLength + 2] = (uv);
            UV[vLength + 3] = (uv);

            Triangles[tLength] = (vLength + 0);
            Triangles[tLength + 1] = (vLength + 1);
            Triangles[tLength + 2] = (vLength + 3);

            Triangles[tLength + 3] = (vLength + 1);
            Triangles[tLength + 4] = (vLength + 2);
            Triangles[tLength + 5] = (vLength + 3);
        }

        public void AddQuad_2(Vector3[] vertices, Vector3 center, Vector2 uv)
        {
            int vLength = Vertices.Length;
            int tLength = Triangles.Length;
            Array.Resize(ref Vertices, vLength + 4);
            Array.Resize(ref Normals, vLength + 4);
            Array.Resize(ref UV, vLength + 4);
            Array.Resize(ref Triangles, tLength + 6);

            Vertices[vLength] = (vertices[0]);
            Vertices[vLength + 1] = (vertices[1]);
            Vertices[vLength + 2] = (vertices[2]);
            Vertices[vLength + 3] = (vertices[3]);

            Normals[vLength] = (vertices[0] - center);
            Normals[vLength + 1] = (vertices[1] - center);
            Normals[vLength + 2] = (vertices[2] - center);
            Normals[vLength + 3] = (vertices[3] - center);

            UV[vLength] = (uv);
            UV[vLength + 1] = (uv);
            UV[vLength + 2] = (uv);
            UV[vLength + 3] = (uv);

            Triangles[tLength] = (vLength + 0);
            Triangles[tLength + 1] = (vLength + 1);
            Triangles[tLength + 2] = (vLength + 3);

            Triangles[tLength + 3] = (vLength + 1);
            Triangles[tLength + 4] = (vLength + 2);
            Triangles[tLength + 5] = (vLength + 3);
        }

        public void AddQuad_3(Vector3[] vertices, Vector3[] centers, Vector2 uv)
        {
            int vLength = Vertices.Length;
            int tLength = Triangles.Length;
            Array.Resize(ref Vertices, vLength + 4);
            Array.Resize(ref Normals, vLength + 4);
            Array.Resize(ref UV, vLength + 4);
            Array.Resize(ref Triangles, tLength + 6);

            Vertices[vLength] = (vertices[0]);
            Vertices[vLength + 1] = (vertices[1]);
            Vertices[vLength + 2] = (vertices[2]);
            Vertices[vLength + 3] = (vertices[3]);

            Normals[vLength] = (vertices[0] - centers[0]);
            Normals[vLength + 1] = (vertices[1] - centers[1]);
            Normals[vLength + 2] = (vertices[2] - centers[2]);
            Normals[vLength + 3] = (vertices[3] - centers[3]);

            UV[vLength] = (uv);
            UV[vLength + 1] = (uv);
            UV[vLength + 2] = (uv);
            UV[vLength + 3] = (uv);

            Triangles[tLength] = (vLength + 0);
            Triangles[tLength + 1] = (vLength + 1);
            Triangles[tLength + 2] = (vLength + 3);

            Triangles[tLength + 3] = (vLength + 1);
            Triangles[tLength + 4] = (vLength + 2);
            Triangles[tLength + 5] = (vLength + 3);
        }

        /// <summary>
        /// 减弱List.Add带来的大量GC造成的性能影响
        /// 需要自行管理index和index2
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="centers"></param>
        /// <param name="uv"></param>
        /// <param name="index"></param>
        /// <param name="index2"></param>
        public void AddQuad_4(Vector3[] vertices, Vector3[] centers, Vector2 uv, int index, int index2)
        {
            int rawLength = index;

            Vertices[index] = (vertices[0]);
            Normals[index] = (vertices[0] - centers[0]);
            UV[index] = (uv);
            index++;

            Vertices[index] = (vertices[1]);
            Normals[index] = (vertices[1] - centers[1]);
            UV[index] = (uv);
            index++;

            Vertices[index] = (vertices[2]);
            Normals[index] = (vertices[2] - centers[2]);
            UV[index] = (uv);
            index++;

            Vertices[index] = (vertices[3]);
            Normals[index] = (vertices[3] - centers[3]);
            UV[index] = (uv);


            Triangles[index2++] = (rawLength + 0);
            Triangles[index2++] = (rawLength + 1);
            Triangles[index2++] = (rawLength + 3);

            Triangles[index2++] = (rawLength + 1);
            Triangles[index2++] = (rawLength + 2);
            Triangles[index2++] = (rawLength + 3);
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

        public void AddRoundPlus(Vector3 center, float radius, Vector3 dir, int smoothness,int vIndex,int tIndex)
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
                AddTriangle_2(new Vector3[] { center, pointArray[i], pointArray[next] }, dir, Vector2.one * 0.5f, vIndex, tIndex);
                vIndex += 3;
                tIndex += 3;
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
            int index = Vertices.Length;
            int index2 = Triangles.Length;

            Array.Resize(ref Vertices, index + smoothness * 4);
            Array.Resize(ref Normals, index + smoothness * 4);
            Array.Resize(ref UV, index + smoothness * 4);
            Array.Resize(ref Triangles, index2 + smoothness * 6);
            for (int i = 0; i < smoothness; i++)
            {
                int next = i + 1;
                if (next >= smoothness)
                {
                    next = 0;
                }
                // AddQuad(new Vector3[] { pointArray1[i], pointArray2[i], pointArray2[next], pointArray1[next] }, (pointArray1[i] + pointArray2[i] + pointArray2[next] + pointArray1[next]) * 0.25f- (ringSide1+ringSide2)*0.5f, new Vector2(0.5f, 0.5f));
                AddQuad_4(new Vector3[] { pointArray1[i], pointArray2[i], pointArray2[next], pointArray1[next] }, new Vector3[] { ringSide1, ringSide2, ringSide2, ringSide1 }, new Vector2(0.5f, 0.5f), index, index2);

                index += 4;
                index2 += 6;
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
            int index = Vertices.Length;
            int index2 = Triangles.Length;

            Array.Resize(ref Vertices, index + smoothness * 4);
            Array.Resize(ref Normals, index + smoothness * 4);
            Array.Resize(ref UV, index + smoothness * 4);
            Array.Resize(ref Triangles, index2 + smoothness * 6);

            for (int i = 0; i < smoothness; i++)
            {
                int next = i + 1;
                if (next >= smoothness)
                {
                    next = 0;
                }
                // AddQuad(new Vector3[] { pointArray1[i], pointArray2[i], pointArray2[next], pointArray1[next] }, (pointArray1[i] + pointArray2[i] + pointArray2[next] + pointArray1[next]) * 0.25f- (ringSide1+ringSide2)*0.5f, new Vector2(0.5f, 0.5f));
                AddQuad_4(new Vector3[] { pointArray1[i], pointArray2[i], pointArray2[next], pointArray1[next] }, new Vector3[] { ringSide1, ringSide2, ringSide2, ringSide1 }, new Vector2(0.5f, 0.5f), index, index2);

                index += 4;
                index2 += 6;
            }
        }

        public void AddRing3D_2(Vector3 ringSide1, float ringSide1Radius, Vector3 d1, Vector3 ringSide2, float ringSide2Radius, Vector3 d2, int smoothness, int vIndex, int tIndex)
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
                AddQuad_4(new Vector3[] { pointArray1[i], pointArray2[i], pointArray2[next], pointArray1[next] }, new Vector3[] { ringSide1, ringSide2, ringSide2, ringSide1 }, new Vector2(0.5f, 0.5f), vIndex, tIndex);

                vIndex += 4;
                tIndex += 6;
            }
        }

        public void Save(BinaryWriter writer)
        {
            writer.Write(Vertices.Length);
            foreach (var item in Vertices)
            {
                writer.WriteVector3(item);
            }
            writer.Write(Normals.Length);
            foreach (var item in Normals)
            {
                writer.WriteVector3(item);
            }
            writer.Write(Colors.Length);
            foreach (var item in Colors)
            {
                writer.WriteColor(item);
            }
            writer.Write(UV.Length);
            foreach (var item in UV)
            {
                writer.WriteVector2(item);
            }
            writer.Write(Triangles.Length);
            foreach (var item in Triangles)
            {
                writer.Write(item);
            }
        }

        public void Load(BinaryReader reader)
        {
            int verticesCount = reader.ReadInt32();
            Vertices = new Vector3[verticesCount];
            for (int i = 0; i < verticesCount; i++)
            {
                Vertices[i] = (reader.ReadVector3());
            }
            int normalsCount = reader.ReadInt32();
            Normals = new Vector3[normalsCount];
            for (int i = 0; i < normalsCount; i++)
            {
                Normals[i] = (reader.ReadVector3());
            }
            int colorsCount = reader.ReadInt32();
            Colors = new Color[colorsCount];
            for (int i = 0; i < colorsCount; i++)
            {
                Colors[i] = (reader.ReadColor());
            }
            int uvCount = reader.ReadInt32();
            UV = new Vector2[uvCount];
            for (int i = 0; i < uvCount; i++)
            {
                UV[i] = (reader.ReadVector2());
            }
            int triangleCount = reader.ReadInt32();
            Triangles = new int[triangleCount];
            for (int i = 0; i < triangleCount; i++)
            {
                Triangles[i] = (reader.ReadInt32());
            }
        }
    }
}
