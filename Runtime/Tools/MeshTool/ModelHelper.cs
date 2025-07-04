using System.Collections.Generic;
using NonsensicalKit.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace NonsensicalKit.Tools.MeshTool
{
    /// <summary>
    /// mesh获取工具类
    /// </summary>
    public static class ModelHelper
    {
        public static Material DefaultMaterial;
        public static Material DefaultMaterialEditor;

        public static Material GetDefaultMaterial()
        {
            if (DefaultMaterial == null)
            {
                GameObject primitive = GameObject.CreatePrimitive(PrimitiveType.Cube);
                primitive.SetActive(false);
                DefaultMaterial = primitive.GetComponent<MeshRenderer>().sharedMaterial;
                primitive.Destroy();
            }

            return DefaultMaterial;
        }

        public static GameObject MergeMesh<T>(IEnumerable<T> components) where T : Component
        {
            GameObject mergeGo = new GameObject("Merge");

            MeshFilter meshFilter = mergeGo.AddComponent<MeshFilter>();
            meshFilter.mesh = new Mesh();
            meshFilter.sharedMesh.indexFormat = IndexFormat.UInt32;
            MeshRenderer meshRenderer = mergeGo.AddComponent<MeshRenderer>();

            List<Material> materials = new List<Material>();

            Dictionary<Material, List<CombineInstance>> combinesDic = new Dictionary<Material, List<CombineInstance>>();

            Bounds bounds = default;
            bool first = true;

            foreach (var item in components)
            {
                MeshFilter[] meshFilters = item.GetComponentsInChildren<MeshFilter>();
                foreach (var t in meshFilters)
                {
                    MeshRenderer renderer = t.GetComponent<MeshRenderer>();
                    if (renderer == null) continue;

                    Material mat = renderer.sharedMaterial;
                    if (materials.Contains(mat) == false)
                    {
                        materials.Add(mat);
                    }

                    CombineInstance combine = new CombineInstance();
                    if (first)
                    {
                        bounds = renderer.bounds;
                        first = false;
                    }
                    else
                    {
                        bounds.Encapsulate(renderer.bounds);
                    }

                    combine.mesh = t.sharedMesh;
                    combine.transform = t.transform.localToWorldMatrix;

                    combinesDic.ListAdd(mat, combine);
                }
            }

            meshRenderer.sharedMaterials = materials.ToArray();
            List<CombineInstance> combines = new List<CombineInstance>();
            mergeGo.transform.position = bounds.center - new Vector3(0, bounds.extents.y, 0);
            foreach (var item in materials)
            {
                var crtMesh = new Mesh();
                crtMesh.indexFormat = IndexFormat.UInt32;
                CombineInstance combine = new CombineInstance();
                crtMesh.CombineMeshes(combinesDic[item].ToArray(), true, true);
                combine.mesh = crtMesh;
                combine.transform = mergeGo.transform.worldToLocalMatrix;
                combines.Add(combine);
            }

            meshFilter.sharedMesh.CombineMeshes(combines.ToArray(), false, true);

            return mergeGo;
        }

        public static GameObject MergeMesh(IEnumerable<GameObject> gos)
        {
            GameObject mergeGo = new GameObject("Merge");

            MeshFilter meshFilter = mergeGo.AddComponent<MeshFilter>();
            meshFilter.mesh = new Mesh();
            meshFilter.sharedMesh.indexFormat = IndexFormat.UInt32;
            MeshRenderer meshRenderer = mergeGo.AddComponent<MeshRenderer>();

            List<Material> materials = new List<Material>();

            Dictionary<Material, List<CombineInstance>> combinesDic = new Dictionary<Material, List<CombineInstance>>();

            Bounds bounds = default;
            bool first = true;

            foreach (var item in gos)
            {
                MeshFilter[] meshFilters = item.GetComponentsInChildren<MeshFilter>();
                for (int i = 0; i < meshFilters.Length; i++)
                {
                    MeshRenderer renderer = meshFilters[i].GetComponent<MeshRenderer>();
                    if (renderer == null) continue;

                    Material mat = renderer.sharedMaterial;
                    if (materials.Contains(mat) == false)
                    {
                        materials.Add(mat);
                    }

                    CombineInstance combine = new CombineInstance();
                    if (first)
                    {
                        bounds = renderer.bounds;
                        first = false;
                    }
                    else
                    {
                        bounds.Encapsulate(renderer.bounds);
                    }

                    combine.mesh = meshFilters[i].sharedMesh;
                    combine.transform = meshFilters[i].transform.localToWorldMatrix;

                    combinesDic.ListAdd(mat, combine);
                }
            }

            mergeGo.transform.position = bounds.center - new Vector3(0, bounds.extents.y, 0);

            meshRenderer.sharedMaterials = materials.ToArray();
            List<CombineInstance> combines = new List<CombineInstance>();
            foreach (var item in materials)
            {
                var crtMesh = new Mesh
                {
                    indexFormat = IndexFormat.UInt32
                };
                CombineInstance combine = new CombineInstance();
                crtMesh.CombineMeshes(combinesDic[item].ToArray(), true, true);
                combine.mesh = crtMesh;
                combine.transform = mergeGo.transform.worldToLocalMatrix;
                combines.Add(combine);
            }

            meshFilter.sharedMesh.CombineMeshes(combines.ToArray(), false, true);

            return mergeGo;
        }

        /// <summary>
        /// 自定义大小的盒子
        /// </summary>
        /// <param name="state"></param>
        /// <param name="singleSize"></param>
        /// <returns></returns>
        public static Mesh GetCustomCube(Array3<bool> state, float singleSize)
        {
            MeshBuilder crtMeshBuffer = new MeshBuilder();

            Array4<bool> bool6Side = new Array4<bool>(state.m_Length0, state.m_Length1, state.m_Length2, 6);

            for (int i = 0; i < state.m_Length0; i++)
            {
                for (int j = 0; j < state.m_Length1; j++)
                {
                    for (int k = 0; k < state.m_Length2; k++)
                    {
                        if (state[i, j, k])
                        {
                            if (i == 0 || (i > 0 && state[i - 1, j, k] == false))
                            {
                                if (bool6Side[i, j, k, 0] == false)
                                {
                                    AddFace(crtMeshBuffer, state, 1, new Int3(i, j, k), bool6Side, singleSize);
                                }
                            }

                            if (i == state.m_Length0 - 1 || (i < state.m_Length0 - 1 && state[i + 1, j, k] == false))
                            {
                                if (bool6Side[i, j, k, 1] == false)
                                {
                                    AddFace(crtMeshBuffer, state, 2, new Int3(i, j, k), bool6Side, singleSize);
                                }
                            }

                            if (j == 0 || (j > 0 && state[i, j - 1, k] == false))
                            {
                                if (bool6Side[i, j, k, 2] == false)
                                {
                                    AddFace(crtMeshBuffer, state, 3, new Int3(i, j, k), bool6Side, singleSize);
                                }
                            }

                            if (j == state.m_Length1 - 1 || (j < state.m_Length1 - 1 && state[i, j + 1, k] == false))
                            {
                                if (bool6Side[i, j, k, 3] == false)
                                {
                                    AddFace(crtMeshBuffer, state, 4, new Int3(i, j, k), bool6Side, singleSize);
                                }
                            }

                            if (k == 0 || (k > 0 && state[i, j, k - 1] == false))
                            {
                                if (bool6Side[i, j, k, 4] == false)
                                {
                                    AddFace(crtMeshBuffer, state, 5, new Int3(i, j, k), bool6Side, singleSize);
                                }
                            }

                            if (k == state.m_Length2 - 1 || (k < state.m_Length2 - 1 && state[i, j, k + 1] == false))
                            {
                                if (bool6Side[i, j, k, 5] == false)
                                {
                                    AddFace(crtMeshBuffer, state, 6, new Int3(i, j, k), bool6Side, singleSize);
                                }
                            }
                        }
                    }
                }
            }

            return crtMeshBuffer.ToMesh();
        }

        private static int GetPointValue(Int3 a, Int3 b)
        {
            if (a.I1 != 0)
            {
                return b.I1;
            }

            if (a.I2 != 0)
            {
                return b.I2;
            }

            return a.I3 != 0 ? b.I3 : 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="meshBuffer"></param>
        /// <param name="state"></param>
        /// <param name="dir">1到6分别为x负，x正，y负，y正，z负，z正</param>
        /// <param name="crtPoint"></param>
        /// <param name="bool6Side"></param>
        /// <param name="singleSize"></param>
        private static void AddFace(MeshBuilder meshBuffer, Array3<bool> state, int dir, Int3 crtPoint, Array4<bool> bool6Side, float singleSize)
        {
            Int3 dir1;
            Int3 dir2;
            Int3 normal;

            switch (dir)
            {
                case 1:
                {
                    dir1 = new Int3(0, 1, 0);
                    dir2 = new Int3(0, 0, 1);
                    normal = new Int3(-1, 0, 0);
                }
                    break;
                case 2:
                {
                    dir1 = new Int3(0, 1, 0);
                    dir2 = new Int3(0, 0, 1);
                    normal = new Int3(1, 0, 0);
                }
                    break;
                case 3:
                {
                    dir1 = new Int3(1, 0, 0);
                    dir2 = new Int3(0, 0, 1);
                    normal = new Int3(0, -1, 0);
                }
                    break;
                case 4:
                {
                    dir1 = new Int3(1, 0, 0);
                    dir2 = new Int3(0, 0, 1);
                    normal = new Int3(0, 1, 0);
                }
                    break;
                case 5:
                {
                    dir1 = new Int3(1, 0, 0);
                    dir2 = new Int3(0, 1, 0);
                    normal = new Int3(0, 0, -1);
                }
                    break;
                case 6:
                {
                    dir1 = new Int3(1, 0, 0);
                    dir2 = new Int3(0, 1, 0);
                    normal = new Int3(0, 0, 1);
                }
                    break;
                default:
                    return;
            }

            Stack<Int3> points = new Stack<Int3>();

            int minDir1Limit = -1;
            int maxDir1Limit = 2147483647;
            int minDir2Limit = -1;
            int maxDir2Limit = 2147483647;

            points.Push(crtPoint);

            Array3<bool> buffer = new Array3<bool>(state.m_Length0, state.m_Length1, state.m_Length2);
            int arrMax1 = state.m_Length0 - 1;
            int arrMax2 = state.m_Length1 - 1;
            int arrMax3 = state.m_Length2 - 1;

            while (points.Count > 0)
            {
                Int3 point = points.Pop();

                int dir1Value = GetPointValue(dir1, point);
                int dir2Value = GetPointValue(dir2, point);

                if (dir1Value < minDir1Limit || dir1Value > maxDir1Limit
                                             || dir2Value < minDir2Limit || dir2Value > maxDir2Limit)
                {
                    continue;
                }

                bool6Side[point.I1, point.I2, point.I3, dir - 1] = true;
                buffer[point.I1, point.I2, point.I3] = true;

                Int3 dir1Negative = point + (-dir1);
                Int3 dir1Positive = point + dir1;
                Int3 dir2Negative = point + (-dir2);
                Int3 dir2Positive = point + dir2;
                Int3 dir1NegativeFace = dir1Negative + normal;
                Int3 dir1PositiveFace = dir1Positive + normal;
                Int3 dir2NegativeFace = dir2Negative + normal;
                Int3 dir2PositiveFace = dir2Positive + normal;

                if (dir1Negative.CheckBound(arrMax1, arrMax2, arrMax3))
                {
                    if (state[dir1Negative]
                        && bool6Side[dir1Negative, dir - 1] == false)
                    {
                        if (dir1NegativeFace.CheckBound(arrMax1, arrMax2, arrMax3) && state[dir1NegativeFace])
                        {
                            if (dir1Value > minDir1Limit)
                            {
                                minDir1Limit = dir1Value;
                            }
                        }
                        else
                        {
                            points.Push(dir1Negative);
                        }
                    }
                    else if (dir1Value > minDir1Limit && buffer[dir1Negative] == false)
                    {
                        minDir1Limit = dir1Value;
                    }
                }
                else
                {
                    if (dir1Value > minDir1Limit)
                    {
                        minDir1Limit = dir1Value;
                    }
                }

                if (dir1Positive.CheckBound(arrMax1, arrMax2, arrMax3))
                {
                    if (state[dir1Positive]
                        && bool6Side[dir1Positive, dir - 1] == false)
                    {
                        if (dir1PositiveFace.CheckBound(arrMax1, arrMax2, arrMax3) && state[dir1PositiveFace])
                        {
                            if (dir1Value < maxDir1Limit)
                            {
                                maxDir1Limit = dir1Value;
                            }
                        }
                        else
                        {
                            points.Push(dir1Positive);
                        }
                    }
                    else if (dir1Value < maxDir1Limit && buffer[dir1Positive] == false)
                    {
                        maxDir1Limit = dir1Value;
                    }
                }
                else
                {
                    if (dir1Value < maxDir1Limit)
                    {
                        maxDir1Limit = dir1Value;
                    }
                }

                if (dir2Negative.CheckBound(arrMax1, arrMax2, arrMax3))
                {
                    if (state[dir2Negative]
                        && bool6Side[dir2Negative, dir - 1] == false)
                    {
                        if (dir2NegativeFace.CheckBound(arrMax1, arrMax2, arrMax3) && state[dir2NegativeFace])
                        {
                            if (dir2Value > minDir2Limit)
                            {
                                minDir2Limit = dir2Value;
                            }
                        }
                        else
                        {
                            points.Push(dir2Negative);
                        }
                    }
                    else if (dir2Value > minDir2Limit && buffer[dir2Negative] == false)
                    {
                        minDir2Limit = dir2Value;
                    }
                }
                else
                {
                    if (dir2Value > minDir2Limit)
                    {
                        minDir2Limit = dir2Value;
                    }
                }

                if (dir2Positive.CheckBound(arrMax1, arrMax2, arrMax3))
                {
                    if (state[dir2Positive]
                        && bool6Side[dir2Positive, dir - 1] == false)
                    {
                        if (dir2PositiveFace.CheckBound(arrMax1, arrMax2, arrMax3) && state[dir2PositiveFace])
                        {
                            if (dir2Value < maxDir2Limit)
                            {
                                maxDir2Limit = dir2Value;
                            }
                        }
                        else
                        {
                            points.Push(dir2Positive);
                        }
                    }
                    else if (dir2Value < maxDir2Limit && buffer[dir2Positive] == false)
                    {
                        maxDir2Limit = dir2Value;
                    }
                }
                else
                {
                    if (dir2Value < maxDir2Limit)
                    {
                        maxDir2Limit = dir2Value;
                    }
                }
            }

            for (int i = 0; i < state.m_Length0; i++)
            {
                for (int j = 0; j < state.m_Length1; j++)
                {
                    for (int k = 0; k < state.m_Length2; k++)
                    {
                        if (buffer[i, j, k])
                        {
                            Int3 point = new Int3(i, j, k);
                            int dir1Value = GetPointValue(dir1, point);
                            int dir2Value = GetPointValue(dir2, point);

                            if (dir1Value < minDir1Limit || dir1Value > maxDir1Limit
                                                         || dir2Value < minDir2Limit || dir2Value > maxDir2Limit)
                            {
                                bool6Side[point.I1, point.I2, point.I3, dir - 1] = false;
                            }
                        }
                    }
                }
            }

            Vector3 normalVector3 = new Vector3(normal.I1, normal.I2, normal.I3);

            Vector3[] point4 = new Vector3[4];
            float step = singleSize;
            float distance = step * 0.5f;
            Vector3 offset = new Vector3((crtPoint.I1 + 0.5f), (crtPoint.I2 + 0.5f), (crtPoint.I3 + 0.5f)) * step;
            Vector3 origin = offset;
            Vector3 faceCenterPoint = origin + normalVector3 * distance;
            Vector3 dir1V3 = new Vector3(dir1.I1, dir1.I2, dir1.I3);
            Vector3 dir2V3 = new Vector3(dir2.I1, dir2.I2, dir2.I3);

            Vector3 dir1MinOffset = (minDir1Limit - GetPointValue(dir1, crtPoint)) * step * dir1V3;
            Vector3 dir1MaxOffset = (maxDir1Limit - GetPointValue(dir1, crtPoint)) * step * dir1V3;
            Vector3 dir2MinOffset = (minDir2Limit - GetPointValue(dir2, crtPoint)) * step * dir2V3;
            Vector3 dir2MaxOffset = (maxDir2Limit - GetPointValue(dir2, crtPoint)) * step * dir2V3;

            point4[0] = faceCenterPoint + dir1MinOffset + dir2MinOffset + -dir1V3 * distance + -dir2V3 * distance;
            point4[1] = faceCenterPoint + dir1MaxOffset + dir2MinOffset + dir1V3 * distance + -dir2V3 * distance;
            point4[2] = faceCenterPoint + dir1MaxOffset + dir2MaxOffset + dir1V3 * distance + dir2V3 * distance;
            point4[3] = faceCenterPoint + dir1MinOffset + dir2MaxOffset + -dir1V3 * distance + dir2V3 * distance;

            if (dir == 2 || dir == 3 || dir == 6)
            {
                meshBuffer.AddQuad(point4, normalVector3, Vector2.one * 0.5f);
            }
            else
            {
                meshBuffer.AddQuad(new[] { point4[2], point4[1], point4[0], point4[3] }, normalVector3, Vector2.one * 0.5f);
            }
        }

        public static Mesh CreateCustomCubeSimple(Array3<bool> state, float singleSize)
        {
            MeshBuilder meshBuilder = new MeshBuilder();
            int width = state.m_Length0;
            int height = state.m_Length1;
            int thickness = state.m_Length2;

            Array3<bool> temp = new Array3<bool>(width, height, thickness);
            for (int w = 0; w < width; w++)
            {
                for (int h = 0; h < height; h++)
                {
                    for (int t = 0; t < thickness; t++)
                    {
                        if (state[w, h, t] && temp[w, h, t] == false)
                        {
                            AddPartCube(meshBuilder, state, temp, w, h, t, singleSize);
                        }
                    }
                }
            }

            return meshBuilder.ToMesh();
        }

        private static void AddPartCube(MeshBuilder meshbuffer, Array3<bool> state, Array3<bool> temp, int w, int h, int t, float singleSize)
        {
            int width = state.m_Length0;
            int height = state.m_Length1;
            int thickness = state.m_Length2;

            int left = w;
            int right = left;
            int down = h;
            int up = down;
            int front = t;
            int back = front;

            temp[w, h, t] = true;

            bool wFlag = right + 1 >= width;
            bool hFlag = up + 1 >= height;
            bool tFlag = back + 1 >= thickness;
            while (true)
            {
                if (!wFlag)
                {
                    if (CheckWidth(down, up, front, back, right + 1, state, temp))
                    {
                        wFlag = true;
                    }
                    else
                    {
                        right++;
                        if (right + 1 >= width)
                        {
                            wFlag = true;
                        }
                    }
                }

                if (!hFlag)
                {
                    if (CheckHeight(left, right, front, back, up + 1, state, temp))
                    {
                        hFlag = true;
                    }
                    else
                    {
                        up++;
                        if (up + 1 >= height)
                        {
                            hFlag = true;
                        }
                    }
                }

                if (!tFlag)
                {
                    if (CheckThickness(left, right, down, up, back + 1, state, temp))
                    {
                        tFlag = true;
                    }
                    else
                    {
                        back++;
                        if (back + 1 >= thickness)
                        {
                            tFlag = true;
                        }
                    }
                }

                if (wFlag && hFlag && tFlag)
                {
                    break;
                }
            }

            for (int i = left; i <= right; i++)
            {
                for (int j = down; j <= up; j++)
                {
                    for (int k = front; k <= back; k++)
                    {
                        temp[i, j, k] = true;
                    }
                }
            }

            float leftPos = (left - width * 0.5f) * singleSize;
            float rightPos = (right - width * 0.5f + 1) * singleSize;
            float downPos = down * singleSize;
            float upPos = (up + 1) * singleSize;
            float frontPos = front * singleSize;
            float backPos = (back + 1) * singleSize;

            Vector3 center = new Vector3((leftPos + rightPos) * 0.5f, (downPos + upPos) * 0.5f, (frontPos + backPos) * 0.5f);
            Vector3 size = new Vector3(rightPos - leftPos, upPos - downPos, backPos - frontPos);
            meshbuffer.AddCube(center, size);
        }

        private static bool CheckWidth(int down, int up, int front, int back, int rightPlus, Array3<bool> state, Array3<bool> temp)
        {
            for (int j = down; j <= up; j++)
            {
                for (int k = front; k <= back; k++)
                {
                    if (state[rightPlus, j, k] == false || temp[rightPlus, j, k])
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool CheckHeight(int left, int right, int front, int back, int upPlus, Array3<bool> state, Array3<bool> temp)
        {
            for (int j = left; j <= right; j++)
            {
                for (int k = front; k <= back; k++)
                {
                    if (state[j, upPlus, k] == false || temp[j, upPlus, k])
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool CheckThickness(int left, int right, int down, int up, int backPlus, Array3<bool> state, Array3<bool> temp)
        {
            for (int j = left; j <= right; j++)
            {
                for (int k = down; k <= up; k++)
                {
                    if (state[j, k, backPlus] == false || temp[j, k, backPlus])
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 不平整的地面
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="singleSize"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static Mesh CreateUnevenPlane(Vector3 offset, float singleSize, int count)
        {
            MeshBuilder meshbuffer = new MeshBuilder();
            float half = count * 0.5f;

            Vector3 crtPos;

            crtPos.x = offset.x - half * singleSize;
            crtPos.y = offset.y;
            crtPos.z = offset.z - half * singleSize;

            Vector3 p1Offset = new Vector3(0, 0, singleSize);
            Vector3 p2Offset = new Vector3(singleSize, 0, singleSize);
            Vector3 p3Offset = new Vector3(singleSize, 0, 0);

            int row = 0;
            int column = 0;
            int all = count * count;
            for (int i = 0; i < all; i++)
            {
                float x = (float)row / count;
                float y = (float)column / count;
                meshbuffer.AddQuad(
                    new[]
                    {
                        PerlinNoise(crtPos), PerlinNoise(crtPos + p1Offset), PerlinNoise(crtPos + p2Offset), PerlinNoise(crtPos + p3Offset)
                    },
                    new Vector3(0, 1, 0),
                    new Vector3(x, y));
                column++;
                crtPos.x += singleSize;
                if (column == count)
                {
                    column = 0;
                    crtPos.x = offset.x - half * singleSize;
                    row++;
                    crtPos.z += singleSize;
                }
            }

            return meshbuffer.ToMesh();
        }

        private static Vector3 PerlinNoise(Vector3 origin)
        {
            origin.y += Mathf.PerlinNoise((origin.x + 100) / 15, (origin.z + 100) / 15);
            return origin;
        }

        public static Mesh CreateCube(Vector3 size)
        {
            MeshBuilder meshBuilder = new MeshBuilder();
            meshBuilder.AddCube(size);
            return meshBuilder.ToMesh();
        }

        public static Mesh CreateCube(Vector3 offset, Vector3 size)
        {
            MeshBuilder meshbuffer = new MeshBuilder();
            meshbuffer.AddCube(offset, size);
            return meshbuffer.ToMesh();
        }

        public static Mesh CreateCube(float width, float height, float depth)
        {
            return CreateCube(new Vector3(width, height, depth));
        }

        public static Mesh CreateCube(float offsetX, float offsetY, float offsetZ, float width, float height, float depth)
        {
            return CreateCube(new Vector3(offsetX, offsetY, offsetZ), new Vector3(width, height, depth));
        }

        public static Mesh CreateCylinder(float radius, float height, int smoothness = 32)
        {
            MeshBuilder meshbuffer = new MeshBuilder();

            meshbuffer.AddRound(Vector3.zero, radius, Vector3.up, smoothness);
            meshbuffer.AddRing3D(Vector3.zero, radius, new Vector3(0, height, 0), radius, Vector3.up, smoothness);
            meshbuffer.AddRound(new Vector3(0, height, 0), radius, -Vector3.up, smoothness);

            return meshbuffer.ToMesh();
        }

        public static void CreateCylinder(Mesh mesh, float radius, float height, int smoothness = 32)
        {
            MeshBuilder meshbuffer = new MeshBuilder();

            meshbuffer.AddRound(Vector3.zero, radius, Vector3.up, smoothness);
            meshbuffer.AddRing3D(Vector3.zero, radius, new Vector3(0, height, 0), radius, Vector3.up, smoothness);
            meshbuffer.AddRound(new Vector3(0, height, 0), radius, -Vector3.up, smoothness);

            meshbuffer.Apply(mesh);
        }

        public static Mesh CreateLine(Vector3 start, Vector3 end, float radius, int smoothness = 32)
        {
            MeshBuilder meshbuffer = new MeshBuilder();

            meshbuffer.AddRound(start, radius, end - start, smoothness);
            meshbuffer.AddRing3D(start, radius, end, radius, end - start, smoothness);
            meshbuffer.AddRound(end, radius, start - end, smoothness);

            return meshbuffer.ToMesh();
        }

        /// <summary>
        /// 创建三元贝塞尔曲线
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="radius"></param>
        /// <param name="segmentNum"></param>
        /// <param name="smoothness"></param>
        /// <returns></returns>
        public static Mesh CreateBezierCurve(Vector3 start, Vector3 p1, Vector3 p2, Vector3 end, float radius, int segmentNum = 16,
            int smoothness = 16)
        {
            MeshBuilder meshbuffer = new MeshBuilder();

            var v = BezierTool.GetCubicBezierListWithSlope(start, p1, p2, end, segmentNum);

            var point = v[0];
            var slopes = v[1];

            meshbuffer.AddRound(start, radius, slopes[0], smoothness);

            for (int i = 0; i < point.Length - 1; i++)
            {
                meshbuffer.AddRing3D(point[i], radius, slopes[i], point[i + 1], radius, slopes[i + 1], smoothness);
            }

            meshbuffer.AddRound(end, radius, -slopes[segmentNum - 1], smoothness);

            return meshbuffer.ToMesh();
        }

        /// <summary>
        /// 更新贝塞尔曲线
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="radius"></param>
        /// <param name="segmentNum"></param>
        /// <param name="smoothness"></param>
        /// <returns></returns>
        public static void CreateBezierCurve(Mesh mesh, Vector3 start, Vector3 p1, Vector3 p2, Vector3 end, float radius, int segmentNum = 16,
            int smoothness = 16)
        {
            MeshBuilder meshBuilder = new MeshBuilder();

            var v = BezierTool.GetCubicBezierListWithSlope(start, p1, p2, end, segmentNum);

            var point = v[0];
            var slopes = v[1];

            meshBuilder.AddRound(start, radius, slopes[0], smoothness);

            for (int i = 0; i < point.Length - 1; i++)
            {
                meshBuilder.AddRing3D(point[i], radius, slopes[i], point[i + 1], radius, slopes[i + 1], smoothness);
            }

            meshBuilder.AddRound(end, radius, -slopes[segmentNum - 1], smoothness);

            meshBuilder.Apply(mesh);
        }

        /// <summary>
        /// 更新贝塞尔曲线
        /// </summary>
        /// <param name="mesh">需要更新的mesh</param>
        /// <param name="rotate">rotation</param>
        /// <param name="start">本地相对偏移</param>
        /// <param name="p1">本地相对偏移</param>
        /// <param name="p2">本地相对偏移</param>
        /// <param name="end">本地相对偏移</param>
        /// <param name="radius"></param>
        /// <param name="segmentNum"></param>
        /// <param name="smoothness"></param>
        /// <returns></returns>
        public static void CreateBezierCurve(Mesh mesh, Quaternion rotate, Vector3 start, Vector3 p1, Vector3 p2, Vector3 end, float radius,
            int segmentNum = 16, int smoothness = 16)
        {
            MeshBuilder meshbuffer = new MeshBuilder();

            rotate = Quaternion.Inverse(rotate);
            var v = BezierTool.GetCubicBezierListWithSlope(rotate * start, rotate * p1, rotate * p2, rotate * end, segmentNum);

            var point = v[0];
            var slopes = v[1];

            meshbuffer.AddRound(rotate * start, radius, slopes[0], smoothness);

            for (int i = 0; i < point.Length - 1; i++)
            {
                Debug.DrawLine(point[i], point[i + 1], Color.red);
                meshbuffer.AddRing3D(point[i], radius, slopes[i], point[i + 1], radius, slopes[i + 1], smoothness);
            }

            meshbuffer.AddRound(rotate * end, radius, -slopes[segmentNum - 1], smoothness);

            meshbuffer.Apply(mesh);
        }

        /// <summary>
        /// 考虑旋转的创建线
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="rotation">所属物体的rotation</param>
        /// <param name="radius"></param>
        /// <param name="smoothness"></param>
        /// <returns></returns>
        public static Mesh CreateLine(Vector3 start, Vector3 end, Quaternion rotation, float radius, int smoothness = 32)
        {
            MeshBuilder meshbuffer = new MeshBuilder();

            meshbuffer.AddRound(rotation * start, radius, rotation * (end - start), smoothness);
            meshbuffer.AddRing3D(rotation * start, radius, rotation * end, radius, rotation * (end - start), smoothness);
            meshbuffer.AddRound(rotation * end, radius, rotation * (start - end), smoothness);

            return meshbuffer.ToMesh();
        }
    }
}
