using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

namespace xyzmidi.core
{
	public abstract class MappingProcessor : IRawValueProvider
	{
		public List<IRawValueProvider> providers = new List<IRawValueProvider>();
		public List<MappingElement> elements = new List<MappingElement>();
		public List<MappingProcessor> processors = new List<MappingProcessor>();

		public bool IsBoolean { get; set; }
		public bool IsGroup { get; set; }
		public bool IsActive { get; set; }

		public abstract float GetRawValue();
		public abstract Vector4? GetRawVector();

		public int Axis { get; set; }
	}
}
