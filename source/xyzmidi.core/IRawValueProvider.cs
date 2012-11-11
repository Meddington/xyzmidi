using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

namespace xyzmidi.core
{
	public interface IRawValueProvider
	{
		float GetRawValue();
		Vector4? GetRawVector();
		int Axis { get; set; }
	}
}
