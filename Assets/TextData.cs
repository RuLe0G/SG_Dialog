using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text.RegularExpressions;

namespace SD.Text
{
    public class TextData
    {
        public string text;

        protected List<int> _shakeList;
        protected List<int> _waveList;
        protected List<int> _errorList;
        protected List<int> _flickerList;
        protected List<string> _actionKeyList;
        protected Dictionary<int, string> _generateImpulseDic;
        protected Dictionary<int, float> _delayDic;


        public List<int> ShakeList
        {
            get
            {
                if (this._shakeList == null)
                {
                    this._shakeList = new List<int>();
                }
                return this._shakeList;
            }
        }
        public List<int> WaveList
        {
            get
            {
                if (this._waveList == null)
                {
                    this._waveList = new List<int>();
                }
                return this._waveList;
            }
        }
        public List<int> ErrorList
        {
            get
            {
                if (this._errorList == null)
                {
                    this._errorList = new List<int>();
                }
                return this._errorList;
            }
        }
        public List<int> FlickerList
        {
            get
            {
                if (this._flickerList == null)
                {
                    this._flickerList = new List<int>();
                }
                return this._flickerList;
            }
        }

        //
        //
        //

        public void ParseScript()
        {
            bool Shakeflag = false;
            bool Waveflag2 = false;
            bool Errorflag3 = false;
            bool Flickerflag4 = false;
            int i = 0;
            int num = 0;
            while (i < this.text.Length)
            {
                if (this.text[i] == '[')
                {
                    int num2 = i;
                    int num3 = this.text.IndexOf(']', num2);
                    if (num3 == -1)
                    {
                        return;
                    }
                    int num4 = num3 - num2 + 1;
                    string subText = this.text.Substring(num2 + 1, num4 - 2).ToLower();
                    if (subText == "s")
                    {
                        Shakeflag = true;
                    }
                    else if (subText == "w")
                    {
                        Waveflag2 = true;
                    }
                    else if (subText == "e")
                    {
                        Errorflag3 = true;
                    }
                    else if (subText == "f")
                    {
                        Flickerflag4 = true;
                    }
                    else if (subText == "/s")
                    {
                        Shakeflag = false;
                    }
                    else if (subText == "/w")
                    {
                        Waveflag2 = false;
                    }
                    else if (subText == "/e")
                    {
                        Errorflag3 = false;
                    }
                    else if (subText == "/f")
                    {
                        Flickerflag4 = false;
                    }
                    text = text.Remove(num2, num4);
                }
                else
                {
                    if (Shakeflag)
                    {
                        this.ShakeList.Add(num);
                    }
                    if (Waveflag2)
                    {
                        this.WaveList.Add(num);
                    }
                    if (Errorflag3)
                    {
                        this.ErrorList.Add(num);
                    }
                    if (Flickerflag4)
                    {
                        this.FlickerList.Add(num);
                    }
                    num++;
                    i++;
                }
            }
        }
    }

}