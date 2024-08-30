using System;

/// <summary>
/// 퍼즐 데이터
/// <br/> id, puzzledType, prefabName(임시 문자열)
/// </summary>
[Serializable]
public struct PuzzleData : ICsvReadable
{
    public int id;
    public PuzzleType puzzledType;
    public string prefabName;

    public void CsvRead(string[] elements)
    {
        int index = 0;

        id = int.Parse(elements[index++]);
        puzzledType = (PuzzleType)int.Parse(elements[index++]);
        prefabName = elements[index++];
    }
}

/// <summary>
/// 퍼즐 타입에 대한 열거형
/// <br/>한붓그리기, 협동 퍼즐, 무궁화 꽃, 뒤집기, OX퀴즈
/// </summary>
public enum PuzzleType
{
    OneStrokeDrawing,
    CooperativePuzzle,
    RoseofSharonhasblommed,
    Flip,
    OXQuiz
}

/// <summary>
/// OX퀴즈 리스트
/// <br/> quizID, quizText(string), quizAnswer(bool)
/// </summary>
[Serializable]
public struct OXQuizList : ICsvReadable
{
    public int quizID;
    public string quizText;
    public bool quizAnswer;

    public void CsvRead(string[] elements)
    {
        int index = 0;

        quizID = int.Parse(elements[index++]);
        quizText = elements[index++];
        quizAnswer = bool.Parse(elements[index++]);
    }
}