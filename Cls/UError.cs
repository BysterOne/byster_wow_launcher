using Cls.Enums;
using Cls.Exceptions;
using Launcher.Cls;

namespace Cls.Errors
{
    //public class UError
    //{
    //    public Enum Code { get; }
    //    public string Message { get; set; }
    //    public UError? PreviousError { get; }

    //    public UError(Enum code, string message, UError? previousError = null)
    //    {
    //        Code = code;
    //        Message = message;
    //        PreviousError = previousError;
    //    }

    //    private List<Enum> _elist()
    //    {
    //        var errorCodes = new List<Enum> { Code };

    //        if (PreviousError is not null)
    //        {
    //            UError? currentError = PreviousError;

    //            while (currentError is not null)
    //            {
    //                errorCodes.Add(currentError.Code);

    //                currentError = currentError.PreviousError;
    //            }
    //        }

    //        return errorCodes;
    //    }

    //    public int GetUCode()
    //    {
    //        var errorCodes = _elist();
    //        errorCodes.Reverse();
    //        int ucode = 0;

    //        for (int i = 0; i < errorCodes.Count; i++)
    //        {
    //            ucode += (int)(object)errorCodes[i] * (int)(Math.Pow(100, i));
    //        }
    //        return ucode;
    //    }

    //    private List<string> GetETreeList(string tab = "    ")
    //    {
    //        var tree = new List<string>() { $"{tab}{Code.GetType().Name.TrimStart('E')} - {Code} ({(int)(object)Code}) - {Message}" };

    //        if (PreviousError is not null)
    //        {
    //            UError? currentError = PreviousError;

    //            while (currentError is not null)
    //            {
    //                tree.Add($"{tab}{currentError.Code.GetType().Name.TrimStart('E')} - {currentError.Code} ({(int)(object)currentError.Code}) - {currentError.Message}");

    //                currentError = currentError.PreviousError;
    //            }
    //        }
    //        return tree;
    //    }
    //    public string GetETree(string tab = "    ")
    //    {
    //        return String.Join("⤵️\n", GetETreeList(tab));
    //    }
    //    private List<string> _fullInfoList(string tab = "  ")
    //    {
    //        var t = new List<string>()
    //        {
    //            $"{tab}Ошибка: {Message}",
    //            $"{tab}Код: {Code.GetType().Name} - {Code} ({(int)(object)Code})",
    //            $"{tab}UКод: {GetUCode()}",
    //            $"{tab}Дерево:"
    //        };
    //        GetETreeList($"{tab}    ").ForEach(x => t.Add($"{x}⤵️"));
    //        return t;
    //    }
    //    public string GetFullInfo()
    //    {
    //        var tab = "     ";
    //        var t = _fullInfoList(tab);
    //        return $"\n{String.Join("\n", t)}\n";
    //    }

    //    public void ToLog(LogBox proc)
    //    {
    //        var t = _fullInfoList();
    //        t.ForEach(x => { proc.Log($"{x}", ELogType.Trace); });
    //    }
    //}
}
