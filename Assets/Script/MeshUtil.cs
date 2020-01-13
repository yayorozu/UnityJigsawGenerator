using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Jigsaw
{
	public static class MeshUtil
	{
		public static void PushMesh(Mesh mesh, float thickness)
		{
			var vertexLength = mesh.vertices.Length;
			var vertices = new List<Vector3>(mesh.vertices);
			foreach (var vertex in mesh.vertices)
			{
				var cache = vertex;  
				cache.z += thickness;
				vertices.Add(cache);
			}
		
			var triangles = new List<int>(mesh.triangles);
			// 背面のポリゴン
			triangles.Reverse();
			for (var i = 0; i < triangles.Count; i++)
				triangles[i] += vertexLength;
		
			triangles.AddRange(mesh.triangles);

			foreach (var edge in GetOutlineEdge(mesh.triangles))
			{
				triangles.Add(edge.y + vertexLength);
				triangles.Add(edge.x);
				triangles.Add(edge.x + vertexLength);
				
				triangles.Add(edge.y);
				triangles.Add(edge.x);
				triangles.Add(edge.y + vertexLength);
			}
		
			mesh.vertices = vertices.ToArray();
			mesh.triangles = triangles.ToArray();
		}
	
		private static Vector2Int[] GetOutlineEdge(int[] triangles)
		{
			var dic = new Dictionary<Vector3Int, int>();
			var loopIndexes = new[]
			{
				new Vector2Int(0, 1),
				new Vector2Int(1, 2),
				new Vector2Int(2, 0),
			};
			for (var i = 0; i < triangles.Length; i += 3)
			{
				foreach (var index in loopIndexes)
				{
					// 反転判定用にzを利用する
					var key = triangles[i + index.x] > triangles[i + index.y]
						? new Vector3Int(triangles[i + index.y], triangles[i + index.x], 1)
						: new Vector3Int(triangles[i + index.x], triangles[i + index.y], 0);

					var find = dic.Keys.FirstOrDefault(k => k.x == key.x && k.y == key.y);
					if (find == default)
						dic.Add(key, 1);
					else
						dic[find] += 1;
				}
			}

			return dic.Where(p => p.Value == 1)
				.Select(p => p.Key.z >= 1 ? new Vector2Int(p.Key.y, p.Key.x) : new Vector2Int(p.Key.x, p.Key.y)).
				ToArray();
		}
	}
}