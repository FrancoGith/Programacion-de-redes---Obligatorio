using Dominio;

namespace LogServerData
{
    public class LogData
    {
        private static LogData _instance;
        private static object _singletonPadlock = new object();

        private List<Log> _logs;

        public static LogData Instance()
        {
            lock (_singletonPadlock)
            {
                if (_instance == null)
                {
                    _instance = new LogData();
                }
            }

            return _instance;
        }

        private LogData()
        {
            _logs = new List<Log>();
            _singletonPadlock = new object();
        }

        public void AddLog(Log log)
        {
            lock (_singletonPadlock)
            {
                _logs.Add(log);
            }
        }

        public void RemoveLog(Log log)
        {
            lock (_singletonPadlock)
            {
                _logs.Remove(log);
            }
        }

        public List<Log> GetLogs()
        {
            lock (_singletonPadlock)
            { 
                return _logs;
            }
        }
    }
}