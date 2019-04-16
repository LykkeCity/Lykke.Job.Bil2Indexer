using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
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
            _fileStream = new FileStream(filePath, FileMode.OpenOrCreate);
        }

        public dynamic CurrentReportObject;
        //public ReportingContextScope CurrentScope { get; }

        public async Task FlushAsync()
        {
            var text = Newtonsoft.Json.JsonConvert.SerializeObject(_root);
            await WriteLineAsync(text);
            _scopeStack.Clear();
            _root = new List<dynamic>();
        }

        public async Task WriteLineAsync(string text)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(text);

            await _fileStream.WriteAsync(buffer, (int)_fileStream.Position, buffer.Length);
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
                previous[scopeName] = CurrentReportObject;
            }
            else
            {
                previous.Add(CurrentReportObject);
            }
        }

        //TODO: How to create reporting scope
        public void StartScope(string scopeName = null)
        {
            var previous = CurrentReportObject;
            _scopeStack.Push(previous);

            CurrentReportObject = new ExpandoObject();

            if (!string.IsNullOrEmpty(scopeName))
            {
                previous[scopeName] = CurrentReportObject;
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
