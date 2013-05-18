using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

namespace xyzmidi.core
{
	public class MappingFeedback
	{
		Mapping _mapping;

		public MappingFeedback(Mapping mapping)
		{
			_mapping = mapping;
		}

		public FeedbackMode Mode { get; set; }
		public bool IsActive { get; set; }
		public Vector4[] ScreenVecs { get; set; }

		public void Draw(bool IsActive)
		{
			this.IsActive = IsActive;
		}
	}

	public enum FeedbackMode
	{
		NoFeedback,
		CircleX,
		CircleY,
		CircleZ,
		LineDistance,
		CircleDistance
	}
}
