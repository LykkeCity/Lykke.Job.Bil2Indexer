using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Job.Bil2Indexer.VerifyingTool.Reporting
{
    /// <summary>
    /// Not thread safe.
    /// </summary>
    public class ReportingContext : IDisposable
    {
        private readonly FileStream _fileStream;
        private readonly Stack<dynamic> _scopeStack = new Stack<dynamic>(10);
        private List<dynamic> _root;

        public ReportingContext(string filePath)
        {
            try
            {
                File.Delete(filePath);
            }
            catch{}

            _fileStream = new FileStream(filePath, FileMode.Append);
            _root = new List<dynamic>();

            CurrentReportObject = _root;
        }

        public dynamic CurrentReportObject { get; private set; }

        public async Task FlushAsync()
        {
            StringBuilder sb = new StringBuilder();
            var rootList = _root.FirstOrDefault();

            if (rootList is List<dynamic>)
            {
                foreach (var item in rootList)
                {
                    var text = Newtonsoft.Json.JsonConvert.SerializeObject(item, Formatting.Indented);
                    sb.AppendLine(text);
                }
            }
            else
            {
                var text = Newtonsoft.Json.JsonConvert.SerializeObject(rootList, Formatting.Indented);
                sb.AppendLine(text);
            }

            await WriteLineAsync(sb.ToString());

            _scopeStack.Clear();
            _root = new List<dynamic>();
            CurrentReportObject = _root;
        }

        public async Task WriteLineAsync(string text)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(text);

            await _fileStream.WriteAsync(buffer, 0, buffer.Length);
        }

        public void Dispose()
        {
            _fileStream.Dispose();
        }

        public void StartListScope(string scopeName = null)
        {
            var previous = CurrentReportObject;
            _scopeStack.Push(previous);

            CurrentReportObject = new List<dynamic>();

            if (!string.IsNullOrEmpty(scopeName))
            {
                var dict = (IDictionary<string, object>)previous;

                dict[scopeName] = CurrentReportObject;
            }
            else
            {
                previous.Add(CurrentReportObject);
            }
        }

        public void StartScope(string scopeName = null)
        {
            var previous = CurrentReportObject;
            _scopeStack.Push(previous);

            CurrentReportObject = new ExpandoObject();

            if (!string.IsNullOrEmpty(scopeName))
            {
                var dict = (IDictionary<string, object>) previous;

                dict[scopeName] = CurrentReportObject;
            }
            else
            {
                previous.Add(CurrentReportObject);
            }
        }

        public void EndScope()
        {
            CurrentReportObject = _scopeStack.Pop();
        }
    }

    public class AssertObject<T>
    {
        public T Indexed { get; set; }

        public T Real { get; set; }
    }
}
