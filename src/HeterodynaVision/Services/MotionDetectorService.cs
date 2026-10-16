using System;
using System.Drawing;

namespace HeterodynaVision.Services;

public sealed class MotionDetectorService
{
	private double[]? _previous;

	private readonly int _gridX;

	private readonly int _gridY;

	private readonly double _threshold;

	public MotionDetectorService(int gridX = 16, int gridY = 12, double threshold = 18.0)
	{
		_gridX = gridX;
		_gridY = gridY;
		_threshold = threshold;
	}

	public bool Detect(Image image, out double score)
	{
		score = 0.0;
		using Bitmap bitmap = new Bitmap(image, new Size(_gridX, _gridY));
		double[] current = new double[_gridX * _gridY];
		int idx = 0;
		for (int y = 0; y < _gridY; y++)
		{
			for (int x = 0; x < _gridX; x++)
			{
				Color c = bitmap.GetPixel(x, y);
				current[idx++] = (double)(c.R + c.G + c.B) / 3.0;
			}
		}
		if (_previous == null)
		{
			_previous = current;
			return false;
		}
		double total = 0.0;
		for (int i = 0; i < current.Length; i++)
		{
			total += Math.Abs(current[i] - _previous[i]);
		}
		score = total / (double)current.Length;
		_previous = current;
		return score >= _threshold;
	}

	public void Reset()
	{
		_previous = null;
	}
}
