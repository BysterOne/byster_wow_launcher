using Cls.Enums;
using Cls.Errors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cls.Exceptions
{
    #region EUExcept
    public enum EUExcept
    {
        DefaultException
    }
    #endregion

    #region Node
    public class Node
    {
        public string Title { get; set; }
        public List<Node> Children { get; set; } = new();

        public Node(string title)
        {
            Title = title;
        }
    }
    #endregion

    public class UExcept : Exception
    {
        public Enum Code { get; set; }        

        public UExcept(Enum errorType, string message) : base(message)
        {
            Code = errorType;
        }
        public UExcept(Enum errorType, string message, UExcept innerException) : base(message, innerException)
        {
            Code = errorType;
        }

        public UExcept(Enum errorType, string message, Exception innerException) : base(message, innerException)
        {
            Code = errorType;
        }

        #region Функции
        #region _elist
        private List<Enum> _elist()
        {
            var errorCodes = new List<Enum> { Code };

            Exception? currentException = InnerException;

            while (currentException is not null)
            {
                if (currentException is UExcept uex) errorCodes.Add(uex.Code);
                else errorCodes.Add(EUExcept.DefaultException);

                currentException = currentException.InnerException;
            }

            return errorCodes;
        }
        #endregion
        #region GetUCode
        public int GetUCode()
        {
            var errorCodes = _elist();
            errorCodes.Reverse();
            int ucode = 0;

            for (int i = 0; i < errorCodes.Count; i++)
            {
                ucode += (int)(object)errorCodes[i] * (int)(Math.Pow(100, i));
            }
            return ucode;
        }
        #endregion
        #region GetETreeList
        private List<string> GetETreeList(string tab = "    ")
        {
            var tree = new List<Node>();

            Exception? currentException = this;

            while (currentException is not null)
            {
                #region Заголовок
                var code = currentException is UExcept uex ? uex.Code : EUExcept.DefaultException;
                var currentNode = new Node($"{code.GetType().Name}.{code} [{(int)(object)code}]: {currentException.Message}");
                #endregion
                #region Trace
                if (!String.IsNullOrWhiteSpace(currentException.StackTrace))
                {
                    var ls = currentException.StackTrace.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    var s = ls.Reverse().Take(5).Reverse().ToList();
                    currentNode.Children.Add(new("Trace") { Children = s.Select(x => new Node(x.Trim())).ToList() });
                }
                #endregion
                #region Доп данные
                if (currentException.Data.Count > 0)
                {
                    var node = new Node("Data");
                    foreach (var key in currentException.Data.Keys)
                    {
                        var value = currentException.Data[key];
                        if (value is IEnumerable<object> enumerable) node.Children.Add(new Node($"{key}: {String.Join(", ", enumerable)}"));                        
                        else node.Children.Add(new Node($"{key}: {value}"));                        
                    }
                    currentNode.Children.Add(node);
                }
                #endregion

                tree.Add(currentNode);
                currentException = currentException.InnerException;
            }
            var d = FormatTree(tree, tab);
            return d;
        }
        #endregion
        #region FormatTree
        private static List<string> FormatTree(List<Node> nodes, string prefix = "")
        {
            var lines = new List<string>();

            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                bool isLast = i == nodes.Count - 1;
                var connector = (prefix == "" ? "" : (isLast ? "└─ " : "├─ "));
                lines.Add(prefix + connector + node.Title);

                var childPrefix = prefix + (prefix == "" ? "" : (isLast ? "   " : "│  "));
                if (node.Children.Count > 0)
                    lines.AddRange(FormatTree(node.Children, childPrefix));
            }

            return lines;
        }
        #endregion
        #region GetETree
        public string GetETree(string tab = "    ")
        {
            return String.Join("\n", GetETreeList(tab));
        }
        #endregion
        #region _fullInfoList
        private List<string> _fullInfoList(string tab = "  ")
        {
            var t = new List<string>()
                {
                    $"{tab}Ошибка: {Message}",
                    $"{tab}Код: {Code.GetType().Name} - {Code} ({(int)(object)Code})",
                    $"{tab}UКод: {GetUCode()}",
                    $"{tab}Дерево:"
                };
            GetETreeList($"{tab} ").ForEach(x => t.Add($"{x}"));
            return t;
        }
        #endregion
        #region GetFullInfo
        public string GetFullInfo(string tab = "     ")
        {
            var t = _fullInfoList(tab);
            return $"{String.Join("\n", t)}";
        }
        #endregion
        #region ToLog
        public void ToLog(LogBox proc)
        {
            var t = _fullInfoList();
            t.ForEach(x => { proc.Log($"{x}", ELogType.Trace); });
        }
        #endregion
        #endregion
    }
}
