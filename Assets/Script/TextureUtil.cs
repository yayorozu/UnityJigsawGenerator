using System.Collections.Generic;
using Unity.VectorGraphics;
using UnityEditor;
using UnityEngine;

namespace Jigsaw
{
	internal static class TextureUtil
	{
		internal static Texture2D CreateTexture(JigsawParam param, List<Border> borders)
		{
			var tangent = Vector2.zero;
			var normal = Vector2.zero;
			var drawMap = new Dictionary<Vector2Int, float>();
			var sepRatio = Vector2.one / (param.TextureSize / param.Size);
			var positions = new List<Vector2>();

			foreach (var border in borders)
			{
				// 橋は塗らないのでスキップ
				if (
					border.Begin.x == 0 && border.End.x == 0 || 
					border.Begin.y == 0 && border.End.y == 0 || 
					border.Begin.x == param.Horizontal && border.End.x == param.Horizontal ||
					border.Begin.y == param.Vertical && border.End.y == param.Vertical
				)
					continue;

				var index = border.Begin.x != border.End.x ? 0 : 1;
				// Endの座標は確保してないので追加
				var segments = new List<BezierPathSegment>(border.Segments)
				{
					new BezierPathSegment
					{
						P0 = border.End / param.Size,
					}
				};
				
				foreach (var seg in VectorUtils.SegmentsInPath(segments))
				{
					for (var t = 0f; t <= 1f; t += sepRatio[index])
					{
						var pos = VectorUtils.EvalFull(seg, t, out tangent, out normal);
						var end = sepRatio[index] * param.DrawThickness;
						
						for (var offset = 0f; offset <= end; offset += sepRatio[index] / 3f)
						{
							positions.Clear();
							var b = Mathf.InverseLerp(end, 0f, offset);
							
							// 最初と最後は円で描画しないと抜けが出る
							if (t <= 0f || t >= 1f)
							{
								for (var angle = 0f; angle <= 360f; angle += 2f)
								{
									positions.Add(pos + new Vector2(
										Mathf.Cos(angle * Mathf.Deg2Rad) * offset, 
										Mathf.Sin(angle * Mathf.Deg2Rad) * offset
									));
								}
							}
							positions.Add(pos + normal * offset);
							positions.Add(pos + normal * -offset);
							
							foreach (var position in positions)
							{
								var key = Vector2Int.RoundToInt(position * param.TextureSize);
								if (key.x < 0 || key.y < 0 || key.x > param.TextureSize.x || key.y > param.TextureSize.y)
									continue;
				
								if (!drawMap.ContainsKey(key))
									drawMap.Add(key, b);
								else if (drawMap[key] < b) 
									drawMap[key] = b;
							}
						}
					}
				}
			}

			// 線付きテクスチャを生成
			var texture = new Texture2D((int) param.TextureSize.x, (int) param.TextureSize.y, TextureFormat.RGB24, false);
			var renderTexture = new RenderTexture(texture.width, texture.height, 32);
			Graphics.Blit(param.Texture, renderTexture);
			RenderTexture.active = renderTexture;
			texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
			RenderTexture.active = null;
			RenderTexture.DestroyImmediate(renderTexture);
			
			foreach (var pair in drawMap)
			{
				texture.SetPixel(
					pair.Key.x, 
					pair.Key.y, 
					Color.Lerp(texture.GetPixel(pair.Key.x, pair.Key.y), param.LineColor, pair.Value)
				);
			}
			
			texture.Apply();

			var path = AssetDatabase.GetAssetPath(param.Texture);

			var root = System.IO.Path.GetDirectoryName(path);
			var fileName = System.IO.Path.GetFileNameWithoutExtension(path);
			var savePath = root + "/" + fileName + "_draw.png"; 
			System.IO.File.WriteAllBytes(savePath, texture.EncodeToPNG());
			
			AssetDatabase.Refresh();

			return AssetDatabase.LoadAssetAtPath<Texture2D>(savePath);
		}
	}
}