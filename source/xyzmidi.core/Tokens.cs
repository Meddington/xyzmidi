using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace xyzmidi.core
{
	public static class Tokens
	{
		//MAPPING
		public const int NOTE = 0;
		public const int CONTROLLER = 1;

		public const string[] midiTypesToken = { "note", "controller" };
		//PROCESSOR

		public const int NONE = 0; //For multiple use

		public const int DIRECT = 0;
		public const int MAPPED = 1;
		public const int DISTANCE = 2;
		public const int ROTATION = 3;
		public const int BOOLEAN = 4;
		public const int CONDITIONNAL = 5;
		public const int FILTERED = 6;
		public const int ACTION = 7;
		public const int MULTI = 8;
  
		public const int LESS_THAN = 1;
		public const int GREATER_THAN = 2;
		public const int BETWEEN = 3;
		public const int LOWEST = 4;
		public const int GREATEST = 5;
		public const int AVERAGE = 6;
		public const int GATE = 7;
		public const int SUM = 8;
		public const int MINUS = 9;
  
		public const int AND = 0;
		public const int OR = 1;
  
  
		public const int ZERO = 1; //Also used in InactiveTokens.
		public const int CLIP = 2;
		public const int LOOP = 3;
  
  
  
		public const int TRIGGER = 1;
		public const int TOGGLE = 2;
  
		public const int CHANGE_SET = 1;
		public const int NEXT_SET = 2;
		public const int PREV_SET = 3;
  
		public const int KEEP_VALUE = 0;
		public const int STANDBY = 2;

		public const string[] processorTypesToken = { "direct", "mapped", "distance", "rotation", "boolean", "conditionnal", "filtered", "action", "multi" };
		public const string[] processorFiltersToken = { "none", "less_than", "greater_than", "between", "lowest", "greatest", "average", "gate", "sum", "minus" };
		public const string[] processorOperatorsToken = { "and", "or" };
		public const string[] processorOverflowsToken = { "none", "zero", "clip", "loop" };
		public const string[] processorEffectsToken = { "none", "trigger", "toggle" };
		public const string[] processorActionsToken = { "none", "changeSet", "nextSet", "prevSet" };
		public const string[] processorInactiveToken = { "keepValue", "zero", "standby" };
  
		//ELEMENT
  
		public const int JOINT = 0;
		public const int POINT = 1;
		public const int VALUE = 2;
  
		public const int POSITION = 0;
		public const int VELOCITY = 1;
		public const int ACCELERATION=  2;
  
		public const int X = 0;
		public const int Y = 1;
		public const int Z = 2;
		public const int XY = 3;
		public const int XZ = 4;
		public const int YZ = 5;
		public const int VECTOR3D = 6;

		public const string[] mappingElementTypesToken = { "joint", "point", "value" };
		public const string[] propertiesToken = { "position", "velocity", "acceleration" };
		public const string[] axisToken = { "x", "y", "z", "xy", "xz", "yz", "3d" };
  
  
		// FEEDBACK
		public const int NO_FEEDBACK = -1;
		public const int CIRCLE_X = 0; //SAME AS X for direct mapping in MappingProcessor:getFeedbackMode();
		public const int CIRCLE_Y = 1; //SAME AS Y for direct mapping in MappingProcessor:getFeedbackMode();
		public const int CIRCLE_Z = 2; //SAME AS Z for direct mapping in MappingProcessor:getFeedbackMode();
		public const int CIRCLE_ROTATION = 3;
		public const int LINE_DISTANCE = 4;
  
  
		//MIDI OUTPUT
		public const int SINGLE = 0;
		public const int DOUBLE = 1;
		public const int TRIPLE = 2;
  
	}
}
