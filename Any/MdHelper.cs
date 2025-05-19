using Markdig;
using Markdig.Wpf;
using Markdig.Syntax;
using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Threading;
using Markdown = Markdig.Markdown;
using Cls.Any;
using System.Diagnostics;
using static System.Net.Mime.MediaTypeNames;

namespace Launcher.Any
{
    public class MdHelper
    {
        private static MarkdownPipeline Pipeline { get; set; } = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
        private static Style DocStyle { get; set; } = (Style)Functions.GetResourceDic("Styles/MarkdownStyle.xaml")["CustomDocumentStyleSasabune"];
              

        public static async Task<FlowDocument> BuildDocMarkdigAsync(
            string text,
            Action<FlowDocument> applyOnUi)
        {
            await Task.Run(() => { });
            Stopwatch wDoc = new Stopwatch(); 
            wDoc.Start();
            var doc = Markdig.Wpf.Markdown.ToFlowDocument(text, Pipeline);
            doc.Style = DocStyle;
            wDoc.Stop();
            Debug.WriteLine($"Makrdown time: {wDoc.ElapsedMilliseconds} ms");
            applyOnUi(doc);
            return doc;
        }
    }
}
