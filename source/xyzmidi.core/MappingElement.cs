using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

namespace xyzmidi.core
{
	public class MappingElement :IRawValueProvider
	{
		public int Axis { get; set; }
		public int Type { get; set; }
		public int Target { get; set; }
		public int Property { get; set; }
		public int Value { get; set; }
		public Vector4? Position { get; set; }
		public int UserId { get; set; }
		public bool IsVector { get; set; }

		public MappingElement(string type, int userId, string target, string property, 
			string axis, int value, Vector4? position)
		{
			Type = Tokens.mappingElementTypesToken.ToList().IndexOf(type);
			UserId = userId;
			Property = Tokens.propertiesToken.ToList().IndexOf(property);
			Axis = Tokens.axisToken.ToList().IndexOf(axis);
			Value = value;
			Position = position;

			switch (Type)
			{
				case Tokens.JOINT:
					Target = GetJointIdForToken(target);
				case Tokens.POINT:
					IsVector = true;
					break;

				default:
					IsVector = false;
			}
		}

		public virtual float GetRawValue()
		{
			switch (Type)
			{
				case Tokens.POINT:
					return 0;

				case Tokens.VALUE:
					return Value;

				case Tokens.JOINT:
					return GetJointValue(GetTargetUser(), Target, Axis);
			}

			//println("## MappingElement => getDirectValue : type not handled -> " + type);
			return 0;
		}

		public virtual Vector4? GetRawVector()
		{
			switch (Type)
			{
				case Tokens.POINT:
					return Position;

				case Tokens.VALUE:
					//println("Element Value (value " + value + ") can't be of type value when vector is needed. Is the processor of type distance / rotation ?");
					return null;

				case Tokens.JOINT:
					return GetJoint(GetTargetUser(), Target);

			}

			//println("## MappingElement => getDirectValue : type not handled -> " + type);
			return null;
		}

		public int GetTargetUser()
		{
			return (UserId < 1 || UserId >= NumUsers) ? TrackedUsers[NumUsers - 1] : TrackedUsers[UserId - 1];
		}
	}
}