using System;

/// <summary>
/// 텍스트 데이터
/// </summary>
[Serializable]
public struct TextTableData : ICsvReadable
{
    public int id;
    public string name;
    public int textEffectID;
    public string kr;

    public void CsvRead(string[] elements)
    {
        int index = 0;

        id = int.Parse(elements[index++]);
        name = elements[index++];
        textEffectID = int.Parse(elements[index++]);
        kr = elements[index++];
    }
}

/// <summary>
/// 텍스트 효과 데이터
/// </summary>
[Serializable]
public struct TextEffectData : ICsvReadable
{
    public int id;
    public string name;
    public int fontID;
    public int size;
    public int colorR;
    public int colorG;
    public int colorB;
    public int charEffect;

    public void CsvRead(string[] elements)
    {
        int index = 0;

        id = int.Parse(elements[index++]);
        name = elements[index++];
        fontID = int.Parse(elements[index++]);
        size = int.Parse(elements[index++]);
        colorR = int.Parse(elements[index++]);
        colorG = int.Parse(elements[index++]);
        colorB = int.Parse(elements[index++]);
        charEffect = int.Parse(elements[index++]);
    }
}

/// <summary>
/// 텍스트 번들 데이터
/// <br/> id, name, textCount, textIDs(배열)
/// </summary>
[Serializable]
public struct TextBundleData : ICsvReadable
{
    public int id;              // ID
    public string name;         // Name
    public int textCount;    // 텍스트 테이블 ID의 개수
    public int[] textIDs;    // 텍스트 테이블 ID들

    public void CsvRead(string[] elements)
    {
        int index = 0;

        id = int.Parse(elements[index++]);
        name = elements[index++];
        textCount = int.Parse(elements[index++]);
        textIDs = new int[textCount];

        for (int i = 0; i < textCount; i++)
        {
            textIDs[i] = int.Parse(elements[index++]);
        }
    }
}

[Flags]
public enum CharEffect
{
    Bold = 1 << 0,            // 1 : 굵게
    Italic = 1 << 1,          // 2 : 기울임
    Underline = 1 << 2,       // 4 : 밑줄
    Strikethrough = 1 << 3    // 8 : 취소선
}

