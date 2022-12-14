using andywiecko.BurstMathUtils;
using andywiecko.PBD2D.Components;
using System;
using System.Linq;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace andywiecko.PBD2D.Examples
{
    [CreateAssetMenu(
    fileName = "EdgeMeshSerializedDataTree",
    menuName = "PBD2D-Examples/EdgeMesh/Serialized Data (Tree)"
    )]
    public class TreeData : EdgeMeshSerializedData
    {
        [SerializeField]
        private float start = 0.1f;
        [SerializeField]
        private float stop = 0.9f;
        [SerializeField]
        private int n = 3;
        [SerializeField]
        private float phi0 = math.PI / 2;
        [SerializeField]
        private float phi1 = math.PI / 2;
        [SerializeField]
        private float length = 0.1f;
        [SerializeField]
        private float factor = 0.9f;
        [SerializeField]
        private float maxlen = 0.1f;
        [SerializeField]
        private int refineTopIter = 3;

        private void Awake()
        {
            GeneratePoints();
        }

        private void OnValidate()
        {
            GeneratePoints();
        }

        private void GeneratePoints()
        {
            using var points = new NativeList<float2>(Allocator.Persistent);
            using var edges = new NativeList<int2>(Allocator.Persistent);

            points.Add(new(0, 0));
            points.Add(new(0, 1));
            edges.Add(new(0, 1));
            var dx = (stop - start) / (n - 1);
            var edgeToSplit = math.int2(0, 1);
            var pId = -1;
            for (int i = 0; i < n; i++)
            {
                var x0 = math.float2(0, start + i * dx);

                var id = edges.IndexOf(edgeToSplit);
                edges.RemoveAt(id);

                pId = points.Length;
                points.Add(x0);
                edges.Add(new(edgeToSplit.x, pId));
                edges.Add(edgeToSplit = new(pId, edgeToSplit.y));

                var l = (1 - (float)i / n * factor) * length;
                AddBranches(pId, phi0, phi1, l);
                AddBranches(pId, -phi0, phi1, l);

            }

            RefineTop(pId, 1);
            void RefineTop(int prevId, int nextId)
            {
                using var queue = new NativeQueue<int2>(Allocator.Persistent);
                queue.Enqueue(new(prevId, nextId));
                var i = 0;
                while (queue.TryDequeue(out var e) && i < refineTopIter)
                {
                    var x0 = 0.5f * (points[e.x] + points[e.y]);
                    var id = edges.IndexOf(e);
                    edges.RemoveAt(id);
                    var newId = points.Length;

                    points.Add(x0);
                    var e1 = new int2(e.x, newId);
                    var e2 = new int2(newId, e.y);
                    edges.Add(e1);
                    edges.Add(e2);

                    queue.Enqueue(e1);
                    queue.Enqueue(e2);

                    i++;
                }
            }

            void AddBranches(int pId, float phi0, float phi1, float length)
            {
                var p = points[pId];
                var dir = (Complex.PolarUnit(phi0) * Complex.PolarUnit(-math.PI / 2)).Value;
                var id0 = points.Length;
                points.Add(p + length * dir);
                edges.Add(new(pId, id0));

                var id1 = points.Length;
                points.Add(p + 2 * length * dir);
                edges.Add(new(id0, id1));

                var id2 = points.Length;
                points.Add(p + 3 * length * dir);
                edges.Add(new(id1, id2));

                var dir2a = (Complex.PolarUnit(phi1) * dir).Value;
                var dir2b = (Complex.PolarUnit(-phi1) * dir).Value;
                points.Add(p + length * (dir + dir2a));
                edges.Add(new(id0, points.Length - 1));
                points.Add(p + length * (dir + dir2b));
                edges.Add(new(id0, points.Length - 1));
                points.Add(p + length * (2 * dir + dir2a));
                edges.Add(new(id1, points.Length - 1));
                points.Add(p + length * (2 * dir + dir2b));
                edges.Add(new(id1, points.Length - 1));
            }

            while (TryRefine()) { }

            bool TryRefine()
            {
                for (int i = 0; i < edges.Length; i++)
                {
                    var e = edges[i];
                    var p0 = points[e.x];
                    var p1 = points[e.y];
                    var l = math.distance(p0, p1);
                    if (l > maxlen)
                    {
                        edges.RemoveAt(i);
                        var p2 = 0.5f * (p0 + p1);
                        points.Add(p2);
                        edges.Add(new(e.x, points.Length - 1));
                        edges.Add(new(points.Length - 1, e.y));
                        return true;
                    }
                }
                return false;
            }

            Positions = points.ToArray();
            Edges = edges.ToArray().SelectMany(i => new int[] { i.x, i.y }).ToArray();
        }
    }
}