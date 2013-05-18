using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace xyzmidi.core
{
	public class Mapping
	{
		public string Id { get; set; }
		public string Label { get; set; }

		public MappingProcessor Processor { get; set; }
		public MappingFeedback Feedback { get; set; }

		public List<IMappingOutput> Outputs { get; set; }

		public Mapping(string id, string label, MappingProcessor processor, IMappingOutput[] outputs)
		{
			Id = id;
			Label = label;
			Processor = processor;
			Outputs = new List<IMappingOutput>(outputs);
			Feedback = new MappingFeedback(this);
			Feedback.Mode = (FeedbackMode) processor.GetFeedbackMode();
			Feedback.IsBoolean = processor.IsBoolean;
			Feedback.Effect = processor.Effect;
			Feedback.Label = processor.Label;
			Feedback.ShowLabel = processor.LabelFeedback;
		}

		public float[] NormalizedValues
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public void DrawFeedback()
		{
			if (Feedback.Mode == FeedbackMode.NoFeedback)
				return;

			// TODO
			//Feedback.ScreenVecs = Processor.GetFeedbackVectors();
			Feedback.Draw(Processor.IsActive);
		}

		public void Send(float [] data)
		{
			if(!Processor.IsActive)
				return;

			foreach(var output in Outputs)
			{
				output.Send(data);
			}
		}
	}
}
