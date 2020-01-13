using Unity.VectorGraphics;
using UnityEngine;

namespace Jigsaw
{
	public class Jigsaw : MonoBehaviour
	{
		[SerializeField, Range(2, 10)]
		private int _separateHorizontalCount = 2;
		[SerializeField, Range(2, 10)]
		private int _separateVerticalCount = 2;
		[SerializeField]
		private Material _material = null;
		[SerializeField]
		private Texture _texture = null;

		private Transform[] _pieces;
	
		private void Awake()
		{
			JigsawGenerateUtil.GenerateJigsaw(new JigsawParam
			{
				Parent = transform,
				Horizontal = _separateHorizontalCount,
				Vertical = _separateVerticalCount,
				Option = new VectorUtils.TessellationOptions
				{
					StepDistance = 100,
					MaxCordDeviation = 0.05f,
					MaxTanAngleDeviation = 0.05f,
					SamplingStepSize = 0.1f
				}, 
				Material = _material,
				Thickness = 0.1f,
				Texture = _texture,
			});
		}
	}
}



