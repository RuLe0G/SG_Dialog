using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using SD.Text;
using System;

public class RichText : MonoBehaviour
{
    public TMP_Text mainText;
    public TMP_Text inputText;

    public string sentence;

    [SerializeField]
    private float delayBettweenLetter = 0.1f;

    public TextData _txtData;

    private readonly List<int> _waveList = new List<int>();
    private readonly List<int> _shakeList = new List<int>();
    private readonly List<int> _errorList = new List<int>();
    private readonly List<int> _flickerList = new List<int>();

    [SerializeField]
    private TMP_Text textMeshPro;

    [SerializeField]
    private AudioClip clip;

    private TMP_MeshInfo[] _currentMeshInfo;

    [SerializeField]
    private float shakeTime = 0.1f;
    [SerializeField]
    private float shakeScale = 10f;
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
        if (obj == this.textMeshPro)
        {
            this._currentMeshInfo = this.textMeshPro.textInfo.CopyMeshInfoVertexData();
            this.textMeshPro.UpdateVertexData();
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

    IEnumerator TypeAnimate(string text)
    {
        foreach (char letter in text.ToCharArray())
        {
            mainText.text += letter;
            yield return new WaitForSeconds(delayBettweenLetter);
        }
    }
    public void SetText(TextData txtData)
    {
        this._txtData = txtData;
        this.textMeshPro.text = this._txtData.text;
        this.textMeshPro.ForceMeshUpdate();
    }

    public void ShowText()
    {
        base.StartCoroutine(this.ShowTextRoutine());
    }

    private IEnumerator ShowTextRoutine()
    {
        WaitForSeconds delay = new WaitForSeconds(delayBettweenLetter);
        TMP_TextInfo textInfo = textMeshPro.textInfo;
        int curIndex = 0;

        

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
        StartCoroutine(Shake(textInfo));
        StartCoroutine(Wave(textInfo));
        yield break;
    }
    private void ShowCharacter(int index)
    {
        TMP_TextInfo txtInfo = this.textMeshPro.textInfo;
        TMP_CharacterInfo[] charInfo = txtInfo.characterInfo;
        int materialReferenceIndex = charInfo[index].materialReferenceIndex;
        int vertexIndex = charInfo[index].vertexIndex;
        bool isMove = true;
        if (this._txtData.WaveList.Contains(index))
        {
            this._waveList.Add(index);
            isMove = false;
        }
        if (this._txtData.ShakeList.Contains(index))
        {
            this._shakeList.Add(index);
            isMove = false;
        }
        if (this._txtData.ErrorList.Contains(index))
        {
            this._errorList.Add(index);
        }
        if (this._txtData.FlickerList.Contains(index))
        {
            this._flickerList.Add(index);
        }
    }
    
    private IEnumerator Wave(TMP_TextInfo txtData)
    {
        while (true)
        {
            //for (int ch = 0; ch < txtData.characterCount; ch++)
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
                    verts[charInfo.vertexIndex + j] = orig + new Vector3(0, a1 * Mathf.Sin((Time.time*2f + orig.x*0.01f)*a2), 0);
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
    public float a1 = 1;
    public float a2 = 1;
    private IEnumerator Shake(TMP_TextInfo txtData)
    {
        while (true)
        {

            WaitForSeconds shakeDelay = new WaitForSeconds(shakeTime);
            //for (int ch = 0; ch < txtData.characterCount; ch++)
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