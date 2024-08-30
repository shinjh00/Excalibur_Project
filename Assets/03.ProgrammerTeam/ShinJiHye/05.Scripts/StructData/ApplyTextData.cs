using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

/// <summary>
/// 개발자 : 신지혜
/// 텍스트 데이터 테이블 적용 스크립트
/// </summary>
public class ApplyTextData : MonoBehaviour
{
    TextTableData textData;
    TextEffectData textEffectData;
    TextBundleData textBundleData;

    /// <summary>
    /// 텍스트 적용 메소드
    /// </summary>
    /// <param name="text"> 텍스트 데이터 적용될 곳 </param>
    /// <param name="id"> TextData ID </param>
    public string ApplyText(TextMeshProUGUI text, int id)
    {
        // *** 텍스트 데이터 ***
        textData = CsvParser.Instance.TextDic[id];

        // 1. text
        text.text = CsvParser.Instance.TextDic[id].kr;

        // 2. textEffect
        ApplyTextEffect(text, textData.textEffectID);

        // 3. 줄바꿈
        if (text.text.Contains("|"))
        {
            text.text = text.text.Replace("|", "\n");
        }
        else
        {
            return text.text;
        }
        return text.text;
    }

    /// <summary>
    /// 텍스트 효과 적용 메소드
    /// </summary>
    /// <param name="text"> 텍스트 데이터 적용될 곳 </param>
    /// <param name="id"> TextEffectData ID </param>
    public void ApplyTextEffect(TextMeshProUGUI text, int id)
    {
        // *** 텍스트 효과 데이터 ***
        textEffectData = CsvParser.Instance.TextEffectDic[id];

        // 1. font
        text.font = SetFont(textEffectData.fontID);
        // 2. size
        text.fontSize = textEffectData.size;
        // 3. colorRGB
        Color basicColor = new Color(textEffectData.colorR, textEffectData.colorG, textEffectData.colorB, 255);
        text.color = basicColor;
        // 4. charEffect (font style)
        text.text = SetFontStyle(text.text, textEffectData.charEffect);
    }

    /// <summary>
    /// 텍스트 번들 적용 메소드
    /// </summary>
    /// <param name="text"> 텍스트 데이터 적용될 곳 </param>
    /// <param name="id"> TextBundleData ID </param>
    public void ApplyTextBundle(TextMeshProUGUI text, int id)
    {
        // *** 텍스트 번들 데이터 ***
        textBundleData = CsvParser.Instance.TextBundleDic[id];
        
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < textBundleData.textCount; i++)
        {
            string sbText = ApplyText(text, textBundleData.textIDs[i]);
            sb.Append($"{sbText} ");
        }
        text.text = sb.ToString();
    }
    

    /// <summary>
    /// fontID에 해당하는 폰트 가져오기
    /// </summary>
    /// <param name="fontID"></param>
    /// <returns></returns>
    private TMP_FontAsset SetFont(int fontID)
    {
        string fontRoute = CsvParser.Instance.returnPath(fontID);
        Font font = Resources.Load(fontRoute) as Font;

        return TMP_FontAsset.CreateFontAsset(font);
    }

    /// <summary>
    /// FontStyle 비트 연산으로 구분하여 적용
    /// </summary>
    /// <param name="content"> 출력될 텍스트 </param>
    /// <param name="charEffect"> 글자 효과 정수값 </param>
    /// <returns></returns>
    private string SetFontStyle(string content, int charEffect)
    {
        /* enum) 1: 굵게, 2: 기울임, 4: 밑줄, 8: 취소선 */

        bool isBold = (charEffect & (int)CharEffect.Bold) == (int)CharEffect.Bold;
        bool isItalic = ((charEffect & (int)CharEffect.Italic) == (int)CharEffect.Italic);
        bool isUnderline = ((charEffect & (int)CharEffect.Underline) == (int)CharEffect.Underline);
        bool isStrikethrough = ((charEffect & (int)CharEffect.Strikethrough) == (int)CharEffect.Strikethrough);

        if (charEffect == 0)
        {
            return content;
        }
        else
        {
            if (isBold)
            {
                content = $"<b>{content}</b>";
            }
            if (isItalic)
            {
                content = $"<i>{content}</i>";
            }
            if (isUnderline)
            {
                content = $"<u>{content}</u>";
            }
            if (isStrikethrough)
            {
                content = $"<s>{content}</s>";
            }
        }
        return content;
    }
}
