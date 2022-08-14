using UnityEngine;
using System.Collections;
using System.Xml.Serialization;
using System.IO;

namespace SD.DialogText
{
    [XmlRoot("dialogue")]
    public class DialogXML
    {
        [XmlElement("node")]
        public Node[] nodes;

        public static DialogXML Load(TextAsset _text)
        {
            XmlSerializer serializer = new XmlSerializer(typeof (DialogXML));
            StringReader reader = new StringReader(_text.text);
            DialogXML dialog = serializer.Deserialize(reader) as DialogXML;
            reader.Close();
            return dialog;
        }
    }

    [System.Serializable]
    public class Node
    {
        [XmlElement("nodeText")]
        public string nodeText;

        [XmlElement("dialend")]
        public int end;
    }
}
