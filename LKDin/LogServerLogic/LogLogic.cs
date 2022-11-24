using Dominio;
using DTOs;
using LogServerData;

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

        public List<Log> ApplyFilters(FilterDTO filter)
        {
            LogData data = LogData.Instance();
            List<Log> filteredList = data.GetLogs();

            DateTime dateTime = ProcessDateText(filter.DateText);

            if (filter.FilterDate)
            {
                filteredList = FilterByDate(dateTime, filteredList);
            }
            if (filter.FilterCategory)
            {
                filteredList = FilterByCategory(filter.CategoryText, filteredList);
            }
            if (filter.FilterContent)
            {
                filteredList = FilterByContent(filter.ContentText, filteredList);
            }

            return filteredList;
        }

        private List<Log> FilterByDate(DateTime date, List<Log> logs)
        {
            List<Log> filteredList = logs.Where(x => x.Date == date).ToList();
            return filteredList;
        }

        private List<Log> FilterByCategory(string category, List<Log> logs)
        {
            List<Log> filteredList = logs.Where(x => x.Category == category).ToList();
            return filteredList;
        }

        private List<Log> FilterByContent(string content, List<Log> logs)
        {
            List<Log> filteredList = logs.Where(x => x.Content == content).ToList();
            return filteredList;
        }

        private DateTime ProcessDateText(string text)
        {
            try
            {
                DateTime date = Convert.ToDateTime(text);
                return date;
            }
            catch (FormatException)
            {
                throw new FormatException("Formato de fecha inválido");
            }
        }
    }
}