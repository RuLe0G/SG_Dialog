using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using SD.Text;
using System;

public class RichText : MonoBehaviour
{
    [SerializeField]
    private TMP_Text mainText;
    public TMP_Text inputText;

    [SerializeField]
    private float delayBettweenLetter = 0.05f;

    public TextData _txtData;

    private readonly List<int> _waveList = new List<int>();
    private readonly List<int> _shakeList = new List<int>();
    private readonly List<int> _errorList = new List<int>();
    private readonly List<int> _flickerList = new List<int>();



    [SerializeField]
    private AudioClip clip;

    private TMP_MeshInfo[] _currentMeshInfo;

    [Header("Shake stng")]
    [SerializeField]
    [Range(0f, 1f)]
    private float shakeTime = 0.3f;
    [SerializeField]
    [Range(0f, 30f)]
    private float shakeScale = 10f;

    [Header("Wave stng")]
    [SerializeField]
    [Range(0f, 1f)]
    private float amplWave = 0.2f;
    [SerializeField]
    [Range(1f, 10f)]
    private float periodWave = 5;
    public void AddTextToMain()
    {
        TextData td = new TextData()
        {
            text = inputText.text
        };
        td.ParseScript();
        SetText(td);
        StopAllCoroutines();
        ShowText();
    }
    private void OnEnable()
    {
        TMPro_EventManager.TEXT_CHANGED_EVENT.Add(new Action<UnityEngine.Object>(this.ON_TEXT_CHANGED));
    }

    private void OnDisable()
    {
        TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(new Action<UnityEngine.Object>(this.ON_TEXT_CHANGED));        
    }
    private void ON_TEXT_CHANGED(UnityEngine.Object obj)
    {
        if (obj == this.mainText)
        {
            this._currentMeshInfo = this.mainText.textInfo.CopyMeshInfoVertexData();
            this.mainText.UpdateVertexData();
        }
    }
    public void ClearMainText()
    {
        mainText.text = "";
        inputText.text = "";
        _waveList.Clear();
        _shakeList.Clear();
        _errorList.Clear();
        _flickerList.Clear();
    }

    public void SetText(TextData txtData)
    {
        this._txtData = txtData;
        this.mainText.text = this._txtData.text;                
        this.mainText.ForceMeshUpdate();
        SetTextColor();
    }

    private void SetTextColor()
    {
        TMP_TextInfo textInfo = mainText.textInfo;
        foreach (var i in textInfo.characterInfo)
        {
            int materialIndex = textInfo.characterInfo[i.index].materialReferenceIndex;
            Color32[] hiddenVertexColors = textInfo.meshInfo[materialIndex].colors32;
            int vertexIndex = textInfo.characterInfo[i.index].vertexIndex;
            byte alpha = 0;
            for (int j = 0; j < 4; j++)
            {
                hiddenVertexColors[vertexIndex + j].a = alpha;
            }            
        }
        mainText.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
    }

    public void ShowText()
    {
        base.StartCoroutine(this.ShowTextRoutine());
    }

    private IEnumerator ShowTextRoutine()
    {

        WaitForSeconds delay = new WaitForSeconds(delayBettweenLetter);
        TMP_TextInfo textInfo = mainText.textInfo;
        int curIndex = 0;
        StartCoroutine(Shake(textInfo));
        StartCoroutine(Wave(textInfo));
        while (curIndex < textInfo.characterCount)
        {            
            if (textInfo.characterInfo[curIndex].isVisible)
            {
                ShowCharacter(curIndex);
                
                GetComponent<AudioSource>().Play();
                yield return delay;
            }
            curIndex++;
        }

        yield break;
    }
    private void ShowCharacter(int index)
    {
        TMP_TextInfo txtInfo = this.mainText.textInfo;
        TMP_CharacterInfo[] charInfo = txtInfo.characterInfo;
        int materialReferenceIndex = charInfo[index].materialReferenceIndex;
        int vertexIndex = charInfo[index].vertexIndex;
        if (this._txtData.WaveList.Contains(index))
        {
            this._waveList.Add(index);
        }
        if (this._txtData.ShakeList.Contains(index))
        {
            this._shakeList.Add(index);
        }
        if (this._txtData.ErrorList.Contains(index))
        {
            this._errorList.Add(index);
        }
        if (this._txtData.FlickerList.Contains(index))
        {
            this._flickerList.Add(index);
        }
        StartCoroutine(FadeCharacter(materialReferenceIndex, vertexIndex, txtInfo));
    }


    private IEnumerator FadeCharacter(int matIndex, int vertIndex, TMP_TextInfo charInfo)
    {
        float durationTime = delayBettweenLetter;
        float countTime = 0f;

        while (true)
        {
            Color32[] newVertexColors = charInfo.meshInfo[matIndex].colors32;
            float num = Mathf.Clamp(countTime / durationTime, 0f, 1f);
            for (int i = 0; i < 4; i++)
            {
                byte lerpAlpha = (byte)Mathf.Lerp(0f, 255f, num);
                newVertexColors[vertIndex+i].a = lerpAlpha;
            }
            if (num >= 1)
            {
                break;
            }
            countTime += Time.deltaTime;
            for (int i = 0; i < charInfo.meshInfo.Length; i++)
            {
                var meshInfo = charInfo.meshInfo[i];
                meshInfo.mesh.vertices = meshInfo.vertices;
                mainText.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
                yield return meshInfo;
            }
        }

        yield break;
    }

    private IEnumerator Wave(TMP_TextInfo txtData)
    {
        while (true)
        {
            foreach(int ch in this._waveList)
            {
                var charInfo = txtData.characterInfo[ch];

                if (!charInfo.isVisible)
                {
                    continue;
                }

                var verts = txtData.meshInfo[charInfo.materialReferenceIndex].vertices;

                for (int j = 0; j < 4; j++)
                {
                    var orig = verts[charInfo.vertexIndex + j];
                    verts[charInfo.vertexIndex + j] = orig + new Vector3(0, amplWave/10 * Mathf.Sin((Time.time*2f + orig.x*0.01f)*periodWave), 0);
                }
            }
            for (int i = 0; i < txtData.meshInfo.Length; i++)
            {
                var meshInfo = txtData.meshInfo[i];
                meshInfo.mesh.vertices = meshInfo.vertices;
                mainText.UpdateGeometry(meshInfo.mesh, i);
                yield return meshInfo;
            }

        }
        yield break;
    }

    private IEnumerator Shake(TMP_TextInfo txtData)
    {
        while (true)
        {

            WaitForSeconds shakeDelay = new WaitForSeconds(shakeTime/10);
            foreach (int ch in this._shakeList)
            {
                Vector3 a = new Vector3(UnityEngine.Random.Range(-0.25f, 0.25f), UnityEngine.Random.Range(-0.25f, 0.25f), 0f);
                int materialReferenceIndex = txtData.characterInfo[ch].materialReferenceIndex;
                int verIndex = txtData.characterInfo[ch].vertexIndex;
                Matrix4x4 matrix = Matrix4x4.TRS(a * shakeScale, Quaternion.identity, Vector3.one);
                var charInfo = txtData.characterInfo[ch];

                if (!charInfo.isVisible)
                {
                    continue;
                }
                var verts = txtData.meshInfo[charInfo.materialReferenceIndex].vertices;

                for (int j = 0; j < 4; j++)
                {
                    var orig = verts[charInfo.vertexIndex + j];
                    verts[charInfo.vertexIndex + j] = matrix.MultiplyPoint3x4(this._currentMeshInfo[materialReferenceIndex].vertices[verIndex + j]);
                }
            }
            for (int i = 0; i < txtData.meshInfo.Length; i++)
            {
                var meshInfo = txtData.meshInfo[i];
                meshInfo.mesh.vertices = meshInfo.vertices;
                mainText.UpdateGeometry(meshInfo.mesh, i);
                yield return meshInfo;
            }
            yield return shakeDelay;

        }
        yield break;
    }
}