using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using System.Windows.Media;
using System.Windows;

namespace xyzmidi.core
{
	public class MappingFeedback
	{
		DrawingContext _dc;
		float _value;
		Mapping _mapping;
		int _triggerFade = 255;
		Color _bgColor;
		Color _baseColor = new Color() { R = 12, G = 133, B = 217 };
		Color _activeBgColor = new Color() { R = 30, G = 30, B = 30 };
		Color _inactiveBgColor = new Color() { R = 220, G = 17, B = 50 };
		Color _triggerColor = new Color() { R = 180, G = 220, B = 17 };

		public MappingFeedback(Mapping mapping)
		{
			_mapping = mapping;
			ScreenVecs = new Vector4[2];
		}

		public FeedbackMode Mode { get; set; }
		public bool IsActive { get; set; }
		public Vector4[] ScreenVecs { get; set; }
		public int Effect { get; set; }
		public bool IsBoolean { get; set; }
		public string Label { get; set; }
		public bool ShowLabel { get; set; }

		public void Draw(bool IsActive)
		{
			this.IsActive = IsActive;

			_bgColor = IsActive ? _activeBgColor : _inactiveBgColor;

			if (_value > 0)
				_triggerFade = 255;

			// TODO
			//PushMatrix();
			//PushStyle();

			switch ((int)Mode)
			{
				case Tokens.CIRCLE_X:
				case Tokens.CIRCLE_Y:
				case Tokens.CIRCLE_Z:
					if (IsBoolean)
					{
						DrawBooleanFeedback(screenVecs[0]);
						break;
					}
					DrawSingleFeedback(screenVecs[0]);
					break;

				case Tokens.LINE_DISTANCE:
				case Tokens.CIRCLE_ROTATION:
					Draw2VecsFeedback(ScreenVecs[0], ScreenVecs[1]);
					break;

				default:
					//println("Feedback draw, mode not handled :" + mode);
					break;
			}

			// TODO
			//PopStyle();
			//PopMatrix();

			if (Effect == Tokens.TRIGGER)
			{
				if (_triggerFade > 0)
					_triggerFade -= 40;
				if (_triggerFade < 0)
					_triggerFade = 0;
			}
		}

		void Draw2VecsFeedback(Vector4 v1, Vector4 v2)
		{
			var color = new Color() { R = 0, G = 0, B = 0 }; // TODO
			var brush = new SolidColorBrush(color);
			var pen = new Pen(brush, 3);

			var centerVec = new Vector4() { X = (v1.X + v2.X) / 2, Y = (v1.Y + v2.Y) / 2 };

			_dc.DrawLine(pen, new Point(v1.X, v1.Y), new Point(v2.X, v2.Y));

			double angle = -Math.Atan2(v2.X - v1.X, v2.Y - v1.Y) + Math.PI / 2;

			var trans = new TranslateTransform(centerVec.X, centerVec.Y);
			_dc.PushTransform(trans);

			double distance = Microsoft.Xna.Framework.Vector2.Distance(
				new Microsoft.Xna.Framework.Vector2(v1.X, v1.Y),
				new Microsoft.Xna.Framework.Vector2(v2.X, v2.Y));

			switch ((int)Mode)
			{
				case Tokens.LINE_DISTANCE:
					_dc.PushTransform(new RotateTransform(angle));
					DrawRectValueFeedback(80,20);
					CreateOffsetLabel(30);
					_dc.Pop();
					break;
        
				case Tokens.CIRCLE_ROTATION:
					brush = new SolidColorBrush(_bgColor);
					pen = new Pen(brush, 6);

					//fill(bgColor,50);

					DrawValueFullArc(1,distance/2);
					
					//fill(baseColor,50);
					//stroke(baseColor);
					DrawValueFullArc(value,distance/2);
					//print("rotation :"+value);
					CreateOffsetLabel(10);
					break;
			}
		}

		void DrawBooleanFeedback(Vector4 vec)
		{
			int radius = (Effect == Tokens.TRIGGER) ? 80 : 120;
			_dc.PushTransform(new TranslateTransform(vec.X, vec.Y));
			RotateForIndex(Mode);

			CreateArcLabel(radius - 20);

			//noFill
			//strokeCap(SQUARE)
			//strokeWeight(8);
			//stroke(bgColor);
			DrawValueAxisArc(1, radius);

			if (_value > 0 || Effect == Tokens.TRIGGER)
			{
				//stroke(triggerColor.triggerFade)
				DrawValueAxisArc(1, radius);
			}
		}

		void DrawValueAxisArc(float val, float radius)
		{
			DrawValueArc(val, radius, (Math.PI * 2) / 3, (Math.PI * 2) / 10);
		}

		void DrawValueFullArc(float val, float radius)
		{
			DrawValueArc(val, radius, Math.PI * 2, 0);
		}

		void DrawValueArc(float val, float diameter, float archLength, float gap)
		{

			_dc.DrawArc(
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

	public static class Extensions
	{
		/// <summary>
		/// Draw an Arc of an ellipse or circle. Static extension method of DrawingContext.
		/// </summary>
		/// <param name="dc">DrawingContext</param>
		/// <param name="pen">Pen for outline. set to null for no outline.</param>
		/// <param name="brush">Brush for fill. set to null for no fill.</param>
		/// <param name="rect">Box to hold the whole ellipse described by the arc</param>
		/// <param name="startDegrees">Start angle of the arc degrees within the ellipse. 0 degrees is a line to the right.</param>
		/// <param name="sweepDegrees">Sweep angle, -ve = Counterclockwise, +ve = Clockwise</param>
		public static void DrawArc(this DrawingContext dc, Pen pen, Brush brush, Rect rect, double startDegrees, double sweepDegrees)
		{
			GeometryDrawing arc = CreateArcDrawing(rect, startDegrees, sweepDegrees);
			dc.DrawGeometry(brush, pen, arc.Geometry);
		}

		/// <summary>
		/// Create an Arc geometry drawing of an ellipse or circle
		/// </summary>
		/// <param name="rect">Box to hold the whole ellipse described by the arc</param>
		/// <param name="startDegrees">Start angle of the arc degrees within the ellipse. 0 degrees is a line to the right.</param>
		/// <param name="sweepDegrees">Sweep angle, -ve = Counterclockwise, +ve = Clockwise</param>
		/// <returns>GeometryDrawing object</returns>
		private static GeometryDrawing CreateArcDrawing(Rect rect, double startDegrees, double sweepDegrees)
		{
			// degrees to radians conversion
			double startRadians = startDegrees * Math.PI / 180.0;
			double sweepRadians = sweepDegrees * Math.PI / 180.0;

			// x and y radius
			double dx = rect.Width / 2;
			double dy = rect.Height / 2;

			// determine the start point 
			double xs = rect.X + dx + (Math.Cos(startRadians) * dx);
			double ys = rect.Y + dy + (Math.Sin(startRadians) * dy);

			// determine the end point 
			double xe = rect.X + dx + (Math.Cos(startRadians + sweepRadians) * dx);
			double ye = rect.Y + dy + (Math.Sin(startRadians + sweepRadians) * dy);

			// draw the arc into a stream geometry
			StreamGeometry streamGeom = new StreamGeometry();
			using (StreamGeometryContext ctx = streamGeom.Open())
			{
				bool isLargeArc = Math.Abs(sweepDegrees) > 180;
				SweepDirection sweepDirection = sweepDegrees < 0 ? SweepDirection.Counterclockwise : SweepDirection.Clockwise;

				ctx.BeginFigure(new Point(xs, ys), false, false);
				ctx.ArcTo(new Point(xe, ye), new Size(dx, dy), 0, isLargeArc, sweepDirection, true, false);
			}

			// create the drawing
			GeometryDrawing drawing = new GeometryDrawing();
			drawing.Geometry = streamGeom;
			return drawing;
		}
	}
}
