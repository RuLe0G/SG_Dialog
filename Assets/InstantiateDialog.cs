using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace SD.DialogText
{
    public class InstantiateDialog : MonoBehaviour
    {
        [SerializeField]
        private UnityEngine.TextAsset ta;
        [SerializeField]
        private DialogXML dialog;

        public int currentNode;

        [SerializeField]
        private TMP_Text mainText;

        public bool ShowDialogue;

        public TextData _txtData;

        public RichText RichText;

        private void Start()
        {
            dialog = DialogXML.Load(ta);
        }        

        public void showDialog()
        {
            if (currentNode < dialog.nodes.Length)
            {
                ShowDialogue = true;
                RichText.initDlg(mainText, buildText());
                currentNode++;
            }
            else
            {
                mainText.text = "";
                ShowDialogue = false;
                RichText.gameObject.SetActive(false);
            }
        }


        private TextData buildText()
        {
            TextData td = new TextData()
            {
                text = dialog.nodes[currentNode].nodeText
            };
            td.ParseScript();

            return td;
        }
    }
}