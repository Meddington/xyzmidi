using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

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
		Mapping[] _mappings;

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
			_mappings = new Mapping[0];

			if (!AttrAsBool(set, "permanent", false))
				ProcessSet(set);
		}

		void ProcessSet(XmlNode set)
		{
			var xmlMappings = set.SelectNodes("/Mapping");
			var setMappings = new Mapping[xmlMappings.Count];

			for (int cnt = 0; cnt < xmlMappings.Count; cnt++)
			{
				XmlNode xmlMap = xmlMappings[cnt];

				string id = xmlMap.Attributes["id"].Value;
				string label = AttrAsString(xmlMap, "label", "");

				var processor =   // TODO -- FInish me

			}
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
			
			string parentAxis = parentProcessor != null ? : "x";
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

			// TODO - FInish me
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
