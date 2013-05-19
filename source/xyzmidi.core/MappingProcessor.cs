using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

namespace xyzmidi.core
{
	public class MappingProcessor : IRawValueProvider
	{
		public string Id { get; set; }
		public string Label { get; set; }

		public List<IRawValueProvider> providers = new List<IRawValueProvider>();
		public List<MappingElement> elements = new List<MappingElement>();
		public List<MappingProcessor> processors = new List<MappingProcessor>();

		public bool IsBoolean { get; set; }
		public bool IsGroup { get; set; }
		public bool IsActive { get; set; }

		public int Axis
		{
			get
			{
				return providers[0].Axis; //May be needed to change to avoid hardcoding of the 1st <element> in the scope to be taken.
			}

			set
			{
			}
		}

		public bool ShowFeedback { get; set; }
		public bool LabelFeedback { get; set; }

		public int MinValue { get; set; }
		public int MaxValue { get; set; }
		public int Overflow { get; set; }

		public int Action { get; set; }
		public string File { get; set; }
		public string SetId { get; set; }
		public int Filter { get; set; }
		public int Effect { get; set; }

		public int Inactive { get; set; }
		public int Type { get; set; }
		public int Operator { get; set; }

		float _previousRawValue = 0;
		bool _previousValue = true;
		bool _toggleState = false;

		public MappingProcessor(
			string id,
			string label,
			bool showFeedback,
			bool labelFeedback,
			string type,
			string filter,
			string op,
			string overflow,
			string effect,
			string axis,
			string action,
			string inactive)
		{
			Id = id;
			Label = label;
			ShowFeedback = showFeedback;
			LabelFeedback = labelFeedback;
			Axis = Tokens.axisToken.ToList().IndexOf(axis);
			Type = Tokens.processorTypesToken.ToList().IndexOf(type);
			Inactive = Tokens.processorInactiveToken.ToList().IndexOf(inactive);
			Action = Tokens.processorActionsToken.ToList().IndexOf(action);
			MinValue = -500;
			MaxValue = 500;
			Filter = Tokens.processorFiltersToken.ToList().IndexOf(filter);

			if (Type == Tokens.ACTION)
			{
				Effect = Tokens.TRIGGER;
			}
			else
			{
				Effect = Tokens.processorEffectsToken.ToList().IndexOf(effect);
			}

			switch (Type)
			{
				case Tokens.BOOLEAN:
				case Tokens.CONDITIONNAL:
				case Tokens.ACTION:
					Operator = Tokens.processorOperatorsToken.ToList().IndexOf(op);
					IsBoolean = true;
					MinValue = 0;
					MaxValue = 1;
					break;

				case Tokens.ROTATION:
					MinValue = 0;
					MaxValue = 360;
					break;

				case Tokens.DISTANCE:
					MinValue = 0;
					MaxValue = 1000;
					break;
			}

			Overflow = Tokens.processorOverflowsToken.ToList().IndexOf(overflow);
		}

		public float GetRawValue()
		{
			float rawValue = 0;

			switch (Type)
			{
				case Tokens.DIRECT:
					return providers[0].GetRawValue();

				case Tokens.MAPPED:
					rawValue = providers[0].GetRawValue();
					break;

				case Tokens.DISTANCE:
					rawValue = GetRawDistance();
					break;

				case Tokens.ROTATION:
					rawValue = GetRawRotation();
					break;

				case Tokens.BOOLEAN:
					rawValue = GetBooleanFilterValue() ? MaxValue : MinValue;
					break;

				case Tokens.CONDITIONNAL:
					rawValue = GetConditionnalFilterValue() ? MaxValue : MinValue;
					break;

				case Tokens.FILTERED:
					rawValue = GetFilteredFilterValue();
					break;

				case Tokens.ACTION:
					rawValue = GetActionValue() ? MaxValue : MinValue;
					break;

				default:
					break;
			}


			//Overflow Handling
			switch (Overflow)
			{
				case Tokens.CLIP:
					if (rawValue < MinValue) rawValue = MinValue;
					if (rawValue > MaxValue) rawValue = MaxValue;
					break;

				case Tokens.LOOP:
					rawValue = (((rawValue - MinValue) % (MaxValue - MinValue)) + (MaxValue - MinValue)) % (MaxValue - MinValue) + MinValue;
					break;

				case Tokens.ZERO:
					if (rawValue < MinValue || rawValue > MaxValue)
					{
						rawValue = MinValue;
					}
					break;

				case Tokens.NONE:
					//no overflow handling
					//println("no Overflow");
					break;

				default:
					//println("## MappingProcessor => overflow not handled : " + overflow);
					break;
			}

			return rawValue;
		}

		public Vector4? GetRawVector()
		{
			//May be needed to change to avoid hardcoding of the 1st <element> in the scope to be taken.
			if (IsGroup || (!IsGroup && elements[0].IsVector)) 
				return providers[0].GetRawVector();
			
			return null;
		}

		public float GetRawDistance()
		{
			Vector4 v1 = providers[0].GetRawVector().Value;
			Vector4 v2 = providers[1].GetRawVector().Value;

			return Microsoft.Xna.Framework.Vector3.Distance(
				new Microsoft.Xna.Framework.Vector3(v1.X, v1.Y, v1.Z),
				new Microsoft.Xna.Framework.Vector3(v2.X, v2.Y, v2.Z));
		}
		

		public float GetRawRotation()
		{
			Vector4 v1 = providers[0].GetRawVector().Value;
			Vector4 v2 = providers[1].GetRawVector().Value;
			float v1c1 = 0;
			float v1c2 = 0;
			float v2c1 = 0;
			float v2c2 = 0;
			switch (providers[0].Axis)
			{
				case Tokens.XY:
					v1c1 = v1.X;
					v1c2 = v1.Y;
					break;
				case Tokens.YZ:
					v1c1 = v1.Y;
					v1c2 = v1.Z;
					break;
				case Tokens.XZ:
					v1c1 = v1.X;
					v1c2 = v1.Z;
					break;
			}

			switch (providers[1].Axis)
			{
				case Tokens.XY:
					v2c1 = v2.X;
					v2c2 = v2.Y;
					break;
				case Tokens.YZ:
					v2c1 = v2.Y;
					v2c2 = v2.Z;
					break;
				case Tokens.XZ:
					v2c1 = v2.X;
					v2c2 = v2.Z;
					break;
			}

			float result = Degrees(Math.Atan2(v2c1 - v1c1, v2c2 - v1c2) + Math.PI);
			//print("rotation degrees :"+result);
			return result;
		}

		public static float Degrees(double angle)
		{
			return (float) (Math.PI * angle / 180.0);
		}


		public bool GetBooleanFilterValue()
		{

			float rawValue = providers[0].GetRawValue();
			float refValue = providers[1].GetRawValue();

			bool result = false;
			switch (Filter)
			{
				case Tokens.GREATER_THAN:
					//println("rawValue :" + rawValue + "/refValue:" + refValue);
					result = (rawValue > refValue);
					break;
				case Tokens.LESS_THAN:
					result = (rawValue < refValue);
					break;
				case Tokens.BETWEEN:
					float ref2Value = providers[2].GetRawValue();
					result = (rawValue > refValue && rawValue < ref2Value);
					break;
				default:
					//println("## MappingProcessor => filter not handled : " + filter);
					break;
			}

			result = ProcessEffect(result);
			return result;
		}

		public bool GetConditionnalFilterValue()
		{
			bool result = (Operator == Tokens.AND);
			for (int i = 0; i < providers.Count; i++)
			{
				if (providers[i].GetRawValue() > 0 && Operator == Tokens.OR)
				{
					result = true;
					break;
				}
				if (providers[i].GetRawValue() == 0 && Operator == Tokens.AND)
				{
					result = false;
					break;
				}
			}

			result = ProcessEffect(result);
			return result;
		}

		protected bool GetActionValue()
		{
			bool result = providers[0].GetRawValue() > 0;
			result = ProcessEffect(result);

			return result;
		}

		protected bool ProcessEffect(bool value)
		{

			bool result = value;

			switch (Effect)
			{
				case Tokens.TRIGGER:
					if (!_previousValue && value)
					{
						result = true;
					}
					else
					{
						result = false;
					}
					break;

				case Tokens.TOGGLE:
					if (!_previousValue && value)
					{
						_toggleState = !_toggleState;
					}
					result = _toggleState;
					break;

				default:
					break;
			}

			_previousValue = value;
			return result;
		}

		public float GetFilteredFilterValue()
		{
			int numProviders = providers.Count;
			float filteredValue = providers[0].GetRawValue();
			float newValue = 0;

			switch (Filter)
			{
				case Tokens.LOWEST:
					for (int i = 1; i < numProviders; i++)
					{
						newValue = providers[i].GetRawValue();
						if (newValue < filteredValue) filteredValue = newValue;
					}
					break;

				case Tokens.GREATEST:
					for (int i = 1; i < numProviders; i++)
					{
						newValue = providers[i].GetRawValue();
						if (newValue > filteredValue) filteredValue = newValue;
					}
					break;

				case Tokens.GATE:
					if (providers[1].GetRawValue() == 0)
					{
						switch (Inactive)
						{
							case Tokens.ZERO:
								//println("Zero Inactive");
								filteredValue = 0;
								break;

							case Tokens.KEEP_VALUE:
								//println("keepValue");
								filteredValue = _previousRawValue;
								break;

							case Tokens.STANDBY:
								//println("standby, inactive");
								filteredValue = 0;
								IsActive = false;
								break;
						}
					}
					else
					{
						IsActive = true;
					}
					break;

				case Tokens.AVERAGE:
					for (int i = 1; i < numProviders; i++)
					{
						filteredValue += providers[i].GetRawValue();
					}
					filteredValue /= numProviders;
					break;

				case Tokens.SUM:
					for (int i = 1; i < numProviders; i++)
					{
						filteredValue += providers[i].GetRawValue();
					}

					break;

				case Tokens.MINUS:
					//println("first value :" + filteredValue);
					for (int i = 1; i < numProviders; i++)
					{
						//println("next Value :" + providers[i].getRawValue());
						filteredValue -= providers[i].GetRawValue();
						//println("result value :" + filteredValue);
					}

					break;

				default:
					//print("MappingProcessor => getFilteredFilterValue, filter not handled " + filter);
					return 0;
			}


			return filteredValue;
		}

		public Vector4[] GetFeedbackVectors()
		{
			if (IsGroup && Type != Tokens.DISTANCE && Type != Tokens.ROTATION)
				return processors[0].GetFeedbackVectors();

			
			var vectors = new List<Vector4>();
			
			for (int i = 0; i < providers.Count; i++)
			{
				Vector4 rawVector;

				if(IsGroup || (!IsGroup && elements[i].IsVector))
				{
					if(providers[i].GetRawVector() != null)
						vectors.Add(providers[i].GetRawVector().Value);
				}
				else
				{
					rawVector = new Vector4();
					Context.convertRealWorldToProjective(rawVector, vectors[i]);
					//println("*** Feedback vector "+vectors[i]);
				}
			}

			return vectors.ToArray();
		}

		public int GetFeedbackMode()
		{
			if (!ShowFeedback) return Tokens.NO_FEEDBACK;

			switch (Type)
			{
				case Tokens.DIRECT:
				case Tokens.MAPPED:
				case Tokens.BOOLEAN:
				case Tokens.CONDITIONNAL:
				case Tokens.FILTERED:
				case Tokens.MULTI:
				case Tokens.ACTION:
					if (IsGroup) return processors[0].GetFeedbackMode();

					switch (elements[0].Type)
					{
						case Tokens.JOINT:  // If the element targets a vector
						case Tokens.POINT:
							if (elements[0].Axis < 3)
							{ //If axis is either x, y or z
								int token = elements[0].Axis;
								return token;
							}
							return Tokens.NO_FEEDBACK;

						default:
							return Tokens.NO_FEEDBACK;
					}

				case Tokens.DISTANCE:
					if ((elements[0].Type == Tokens.JOINT || elements[0].Type == Tokens.POINT)
					   && (elements[1].Type == Tokens.JOINT || elements[1].Type == Tokens.POINT))
					{
						return Tokens.LINE_DISTANCE;
					}
					else
					{
						return Tokens.NO_FEEDBACK;
					}

				case Tokens.ROTATION:
					if ((elements[0].Type == Tokens.JOINT || elements[0].Type == Tokens.POINT)
					   && (elements[1].Type == Tokens.JOINT || elements[1].Type == Tokens.POINT))
					{
						return Tokens.CIRCLE_ROTATION;
					}
					else
					{
						return Tokens.NO_FEEDBACK;
					}

				default:
					return Tokens.NO_FEEDBACK;
			}
		}

		public void SetGroup(Boolean value)
		{
			IsGroup = value;
			providers = new List<IRawValueProvider>(IsGroup ? processors.ToArray<IRawValueProvider>() : elements.ToArray < IRawValueProvider>());

			//Hack for default Min/Max values
			/*if(getAxis() == Tokens.Z && minValue < 0)
			{
			  minValue = 1000;
			  maxValue = 2000;
			}*/
		}
	}
}
