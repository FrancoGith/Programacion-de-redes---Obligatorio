using Dominio;
using LogServerData;
using System.Linq;

namespace LogServerLogic
{
    public class LogLogic
    {
        public void AddLog(Log log)
        {
            LogData data = LogData.Instance();
            data.AddLog(log);
        }

        public void RemoveLog(Log log)
        {
            LogData data = LogData.Instance();
            data.RemoveLog(log);
        }

        public List<Log> GetLogs()
        { 
            LogData data = LogData.Instance();

            return data.GetLogs();
        }

        public List<Log> FilterByUsername(string username)
        {
            LogData data = LogData.Instance();

            List<Log> filteredList = data.GetLogs().Where(x => x.User == username).ToList();
            return filteredList;
        }

        public List<Log> FilterByCategory(string category)
        {
            LogData data = LogData.Instance();

            List<Log> filteredList = data.GetLogs().Where(x => x.Category == category).ToList();
            return filteredList;
        }

        public List<Log> FilterByContent(string content)
        {
            LogData data = LogData.Instance();

            List<Log> filteredList = data.GetLogs().Where(x => x.Content == content).ToList();
            return filteredList;
        }
    }
}