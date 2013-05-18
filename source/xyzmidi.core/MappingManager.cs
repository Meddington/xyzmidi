using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Microsoft.Kinect;

namespace xyzmidi.core
{
	public class MappingManager
	{
		public string CurrentFile { get; set; }
		public int CurrentSetIndex { get; set; }
		public int NumMappings { get; set; }

		string _currentSetId;
		int _totalSets;
		char[] _shortcuts;
		XmlDocument _doc;
		XmlNodeList _xmlSets;
		List<Mapping> _mappings = new List<Mapping>();

		public MappingManager()
		{
			CurrentFile = "mappings.xml";
		}

		public void ReadFile(string file)
		{
			ReadSet(file);
		}

		public void ReadSet(string file)
		{
			ReadSet(CurrentFile, 0);
		}

		public void ReadSet(int setId)
		{
			
			ReadSet(CurrentFile, setId);
		}

		void LoadDocument(string file)
		{
			_doc = new XmlDocument();
			_doc.Load(file);

			_xmlSets = _doc.SelectNodes(@"/XyzMidi/MappingSet");
			_totalSets = _xmlSets.Count;
			_shortcuts = GetSetsShortcuts();

		}

		public void ReadSet(string file, int setIndex)
		{
			if (file == null)
				file = CurrentFile;
			
			LoadDocument(file);
			ReadSet(_xmlSets[setIndex]);
		}

		public void ReadSet(string file, string setId)
		{
			if (file == null)
				file = CurrentFile;

			LoadDocument(file);

			foreach (XmlNode set in _xmlSets)
			{
				if (set.Attributes["id"].Value == setId)
				{
					ReadSet(set);
				}
			}

			throw new ApplicationException("Error, set id not found: " + setId);
		}

		char[] GetSetsShortcuts()
		{
			char[] sc = new char[_totalSets];

			for (int cnt = 0; cnt < _xmlSets.Count; cnt++)
			{
				XmlNode node = _xmlSets[cnt];
				sc[cnt] = node.Attributes["shortcut"].Value[0];
			}

			return sc;
		}

		string AttrAsString(XmlNode node, string attribute, string def)
		{
			var attr = node.Attributes.GetNamedItem(attribute);
			if (attr == null)
				return def;

			return attr.Value;
		}

		bool AttrAsBool(XmlNode node, string attribute, bool def)
		{
			var attr = node.Attributes.GetNamedItem(attribute);
			if (attr == null)
				return def;

			if (attr.Value.ToLower() == "true")
				return true;

			return false;
		}

		void ReadSet(XmlNode set)
		{
			if (!AttrAsBool(set, "inScope", true))
			{
				ReadNextSet();
				return;
			}

			_currentSetId = set.Attributes["id"].Value;
			CurrentSetIndex = IndexOfSet(set);
			_mappings.Clear();

			if (!AttrAsBool(set, "permanent", false))
				ProcessSet(set);
		}

		void ProcessSet(XmlNode set)
		{
			var xmlMappings = set.SelectNodes("/Mapping");

			for (int cnt = 0; cnt < xmlMappings.Count; cnt++)
			{
				XmlNode xmlMap = xmlMappings[cnt];

				string id = xmlMap.Attributes["id"].Value;
				string label = AttrAsString(xmlMap, "label", "");

				var processor = ReadXmlProcessor(xmlMap.SelectSingleNode("/Processor"), null);
				var outputs = ReadXmlOutputs(xmlMap.SelectSingleNode("/Output"));

				var mapping = new Mapping(id, label, processor, outputs);
				_mappings.Add(mapping);
			}
		}

		public void ReadPermanentSets()
		{
			//println("---- Begin Read Permanent Sets --------");
			int permanents = 0;
			foreach(XmlNode set in _xmlSets)
			{
				if (AttrAsBool(set,"permanent", false))
				{
					ProcessSet(set);
				}
			}
			//println("---- End Read Permenent Sets, total Permanent Sets : " + permanents + "----------");
		}

		MappingProcessor ReadXmlProcessor(XmlNode xmlProc, MappingProcessor parentProcessor)
		{
			string id = AttrAsString(xmlProc, "id", "");
			string label = AttrAsString(xmlProc, "label", "");
			bool showFeedback = AttrAsBool(xmlProc, "showFeedback", true);
			bool labelFeedback = AttrAsBool(xmlProc, "labelFeedback", true);

			string type = xmlProc.Attributes["type"].Value;
			string filter = AttrAsString(xmlProc, "filter", "none");
			string effect = AttrAsString(xmlProc, "effect", "none");
			string op = AttrAsString(xmlProc, "operator", "and");
			int minValue = int.Parse(AttrAsString(xmlProc, "minValue", "-5555"));
			int maxValue = int.Parse(AttrAsString(xmlProc, "maxValue", "-5555"));
			string overflow = AttrAsString(xmlProc, "overflow", "clip");

			string action = AttrAsString(xmlProc, "action", "none");
			string file = AttrAsString(xmlProc, "file", "");
			string setId = AttrAsString(xmlProc, "setId", "");

			string inactive = AttrAsString(xmlProc, "inactive", "null");
			
			string parentAxis = parentProcessor != null ? Tokens.axisToken[parentProcessor.Axis] : "x";
			string axis = AttrAsString(xmlProc, "axis", parentAxis);

			var p = new MappingProcessor(
				id,
				label,
				showFeedback,
				labelFeedback,
				type,
				filter,
				op,
				overflow,
				effect,
				axis,
				action,
				inactive);

			if(minValue != -5555)
				p.MinValue = minValue;
			if(maxValue != -5555)
				p.MaxValue = maxValue;
			if(p.Action != Tokens.NONE)
			{
				p.File = file;
				p.SetId = setId;
			}

			var elems = new List<MappingElement>();
			foreach(XmlNode element in xmlProc.SelectNodes("/Element"))
				elems.Add(ReadXmlElement(element, p));

			if(elems.Count >  0)
			{
				p.elements = elems;
				p.SetGroup(false);
			}

			var procs = new List<MappingProcessor>();
			foreach(XmlNode processor in xmlProc.SelectNodes("/Processor"))
				procs.Add(ReadXmlProcessor(processor, p));

			if(procs.Count > 0)
			{
				p.processors = procs;
				p.SetGroup(true);
			}

			return p;
		}

		MappingElement ReadXmlElement(XmlNode xmlElem, MappingProcessor parentProcessor)
		{
			return new MappingElement(
				xmlElem.Attributes["type"].Value,
				int.Parse(AttrAsString(xmlElem, "userId", "0")),
				xmlElem.Attributes["target"].Value,
				AttrAsString(xmlElem, "property", "position"),
				AttrAsString(xmlElem, "axis", ((parentProcessor != null) ? Tokens.axisToken[parentProcessor.Axis] : "x")),
				int.Parse(AttrAsString(xmlElem, "value", "0")),
				new Vector4()
					{
						X = int.Parse(AttrAsString(xmlElem, "x", "0")),
						Y = int.Parse(AttrAsString(xmlElem, "y", "0")),
						Z = int.Parse(AttrAsString(xmlElem, "z", "0"))
					}
				);
		}

		List<IMappingOutput> ReadXmlOutputs(XmlNode xmlOutputs)
		{
			List<IMappingOutput> outputs = new List<IMappingOutput>();

			foreach (XmlNode xmlChild in xmlOutputs.ChildNodes)
			{
				if (xmlChild.Name == "Midi")
				{
					// TODO -- 
					//String midiType = xmlOutput.getString("type", "controller");
					//int midiDevice = xmlOutput.getInt("device", 0);
					//int midiChannel = xmlOutput.getInt("channel", 1);
					//int midiVelocity = xmlOutput.getInt("velocity", 0);
					//int midiDeviceMap = xmlOutput.getInt("deviceMap", 0);
					//int midiChannelMap = xmlOutput.getInt("channelMap", 0);
					//int midiVelocityMap = xmlOutput.getInt("velocityMap", 1);
					//int midiMinChannel = xmlOutput.getInt("minChannel", 0);
					//int midiMaxChannel = xmlOutput.getInt("maxChannel", 127);
					//int midiMinVelocity = xmlOutput.getInt("minVelocity", 0);
					//int midiMaxVelocity = xmlOutput.getInt("maxVelocity", 127);

					//Boolean midiDistinctNotes = boolean(xmlOutput.getString("distinctNotes", "true"));

					//output = new MappingMIDIOutput(midiType, midiDevice, midiChannel, midiVelocity, midiDeviceMap, midiChannelMap, midiVelocityMap,
					//								midiMinChannel, midiMaxChannel, midiMinVelocity, midiMaxVelocity, midiDistinctNotes);
				}
				else if (xmlChild.Name == "Osc")
				{
					// TODO -- 
					//String oscHost = xmlOutput.getString("host", oscDefaultHost);
					//String oscAddress = xmlOutput.getString("address", "");
					//int oscPort = xmlOutput.getInt("port", oscDefaultOutPort);
					//output = new MappingOSCOutput(oscHost, oscPort, oscAddressPrefix + oscAddress);
				}
				else if (xmlChild.Name == "Dmx")
				{
					// TODO -- 
					//int startChannel = xmlOutput.getInt("startChannel", 1);
					//int minOut = xmlOutput.getInt("minOut", 0);
					//int maxOut = xmlOutput.getInt("maxOut", 255);
					//output = new MappingDMXOutput(startChannel, minOut, maxOut);
				}
			}

			return outputs;
		}

		public void ProcessMappings(bool drawFeedback)
		{
			float[] values;
			for (int cnt = 0; cnt < _mappings.Count; cnt++)
			{
				values = _mappings[cnt].NormalizedValues;
				var processor = _mappings[cnt].Processor;

				if (processor.Type == Tokens.ACTION)
				{
					if (values[0] == 1)
					{
						switch (processor.Action)
						{
							case Tokens.CHANGE_SET:
								ReadSet(processor.File, processor.SetId);
								return;

							case Tokens.NEXT_SET:
								ReadNextSet();
								return;

							case Tokens.PREV_SET:
								ReadPrevSet();
								return;
						}
					}
				}
				else
				{
					_mappings[cnt].Send(values);
					if (drawFeedback)
						_mappings[cnt].DrawFeedback();
				}
			}
		}

		public void KeyPressed(char keyChar)
		{
			int numShortcuts = _shortcuts.Length;

			for (int i = 0; i < numShortcuts; i++)
			{
				if (_shortcuts[i] == keyChar)
				{
					//println("found shortcut : " + keyChar + " -> setIndex to " + i);
					ReadSet(i);
					break;
				}
			}
		}

		int IndexOfSet(XmlNode set)
		{
			for (int cnt = 0; cnt < _xmlSets.Count; cnt++)
			{
				if (set == _xmlSets[cnt])
					return cnt;
			}

			throw new ApplicationException("Unable to find set in set list!?");
		}

		public void ReadNextSet()
		{
			ReadSet((CurrentSetIndex + 1) % _totalSets);
		}

		public void ReadPrevSet()
		{
			ReadSet((CurrentSetIndex - 1 + _totalSets) % _totalSets);
		}
	}
}
