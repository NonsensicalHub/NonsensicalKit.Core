using System.Collections.Generic;
using System.Linq;
using NonsensicalKit.Core;
using UnityEngine;

namespace NonsensicalKit.Tools.MeshTool
{
    /// <summary>
    /// mesh操作工具类
    /// </summary>
    public static class MeshHelper
    {
        /// <summary>
        /// 清除未被使用的顶点
        /// </summary>
        /// <param name="mesh"></param>
        public static void ClearUnUseVertex(Mesh mesh)
        {
            //值类型转引用类型存储
            List<Vector3> vertexQuote = new List<Vector3>();
            List<Vector2> uv = new List<Vector2>(mesh.uv);
            List<Vector3> normals = new List<Vector3>(mesh.normals);

            foreach (var item in mesh.vertices)
            {
                vertexQuote.Add(item);
            }

            //保存引用类型
            List<TriangleContainer> triangleContainers = new List<TriangleContainer>();

            foreach (var item in mesh.triangles)
            {
                triangleContainers.Add(new TriangleContainer(vertexQuote[item]));
            }

            //获取未被使用的定点
            bool[] use = Enumerable.Repeat(false, mesh.vertexCount).ToArray();

            foreach (var item in mesh.triangles)
            {
                use[item] = true;
            }

            //从链表中清除未被使用的定点
            for (int i = 0, useCount = 0; i < vertexQuote.Count; i++, useCount++)
            {
                if (use[useCount] == false)
                {
                    vertexQuote.RemoveAt(i);
                    uv.RemoveAt(i);
                    normals.RemoveAt(i);
                    i--;
                }
            }

            //获取新三角数组
            List<int> triangles = new List<int>();

            foreach (var item in triangleContainers)
            {
                triangles.Add(vertexQuote.IndexOf(item.Vertice));
            }

            //链表转数组
            List<Vector3> vertices = new List<Vector3>();
            foreach (var item in vertexQuote)
            {
                vertices.Add(item);
            }

            //赋值mesh
            mesh.triangles = triangles.ToArray();
            mesh.vertices = vertices.ToArray();
            mesh.uv = uv.ToArray();
            mesh.normals = normals.ToArray();
        }

        /// <summary>
        /// 根据顶点数组求出质点并返回回偏移量
        /// </summary>
        /// <param name="mesh">需要求质点的mesh</param>
        /// <returns></returns>
        public static Vector3 AutoCentroidShift(Mesh mesh)
        {
            Vector3[] vertices = mesh.vertices;

            Vector3 sum = Vector3.zero;

            foreach (var item in vertices)
            {
                sum += item;
            }

            Vector3 offSet = -sum / vertices.Length;

            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] += offSet;
            }

            return offSet;
        }

        /// <summary>
        /// 网格是否包含坐标,性能较低，但可以判断凹体网格或者三角面方向不正确的网格
        /// </summary>
        public static bool Contain(Vector3[] vertices, int[] triangle, Vector3 posLocal)
        {
            Vector3 dir = Vector3.up;

            List<Vector3> crossPoint = new List<Vector3>();
            for (int i = 0; i < triangle.Length; i += 3)
            {
                Vector3? cross = VectorTool.GetRayTriangleCrossPoint(posLocal, dir, vertices[triangle[i]], vertices[triangle[i + 1]],
                    vertices[triangle[i + 2]]);
                if (cross != null)
                {
                    crossPoint.Add((Vector3)cross);
                }
            }

            VectorTool.SortList(crossPoint);

            for (int i = 0; i < crossPoint.Count - 1; i++)
            {
                if (VectorTool.IsNear(crossPoint[i], crossPoint[i + 1]))
                {
                    crossPoint.RemoveAt(i + 1);
                    i--;
                }
            }

            if (crossPoint.Count % 2 == 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool Contain(this Mesh mesh, Vector3 posLocal)
        {
            Vector3[] vertices = mesh.vertices;
            int[] triangle = mesh.triangles;
            return Contain(vertices, triangle, posLocal);
        }

        public static bool Contain(this MeshBuilder mesh, Float3 posLocal)
        {
            return Contain(mesh, posLocal.ToVector3());
        }

        public static bool Contain(this MeshBuilder mesh, Vector3 posLocal)
        {
            Vector3[] vertices = mesh.Vertices.ToArray();
            int[] triangle = mesh.Triangles.ToArray();

            return Contain(vertices, triangle, posLocal);
        }

        /// <summary>
        /// 网格是否包含坐标，性能较高，但要求网格为非凹体且网格三角面方向完全正确
        /// </summary>
        public static bool Contain_2(Vector3[] vertices, int[] triangle, Vector3 posLocal)
        {
            for (int i = 0; i < triangle.Length; i += 3)
            {
                Vector3 line1 = vertices[triangle[i + 1]] - vertices[triangle[i]];
                Vector3 line2 = vertices[triangle[i + 2]] - vertices[triangle[i]];

                Vector3 normal = Vector3.Cross(line1, line2);

                Vector3 posDir = posLocal - vertices[triangle[i + 1]];

                if (Vector3.Dot(posDir, normal) > 0)
                {
                    return false;
                }
            }

            return true;
        }

        public static bool Contain_2(this Mesh mesh, Vector3 posLocal)
        {
            Vector3[] vertices = mesh.vertices;
            int[] triangle = mesh.triangles;

            return Contain_2(vertices, triangle, posLocal);
        }

        public static bool Contain_2(this MeshBuilder mesh, Vector3 posLocal)
        {
            Vector3[] vertices = mesh.Vertices.ToArray();
            int[] triangle = mesh.Triangles.ToArray();

            return Contain_2(vertices, triangle, posLocal);
        }

        /// <summary>
        /// 进行包含位移变换的矩阵转换
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static Vector3 Operator(Matrix4x4 lhs, Vector3 vector)
        {
            return lhs * new Vector4(vector.x, vector.y, vector.z, 1);
        }

        /// <summary>
        /// 返回一个刚好包住所有子物体的bounds(世界方向)
        /// </summary>
        /// <param name="go"></param>
        public static Bounds GetBoundsWithChild(GameObject go)
        {
            Quaternion old = go.transform.rotation;
            go.transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));

            bool hasBounds = false;
            Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);

            Renderer[] childRenderers = go.transform.GetComponentsInChildren<Renderer>();

            foreach (var item in childRenderers)
            {
                if (hasBounds)
                {
                    bounds.Encapsulate(item.bounds);
                }
                else
                {
                    bounds = item.bounds;
                    hasBounds = true;
                }
            }

            bounds.center = bounds.center - go.transform.position;

            bounds.size = new Vector3(bounds.size.x / go.transform.lossyScale.x, bounds.size.y / go.transform.lossyScale.y,
                bounds.size.z / go.transform.lossyScale.z);

            go.transform.rotation = old;
            return bounds;
        }

        private class TriangleContainer
        {
            public Vector3 Vertice;

            public TriangleContainer(Vector3 vertice)
            {
                Vertice = vertice;
            }
        }
    }
}
