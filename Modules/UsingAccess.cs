using System.Data;
using System.Data.OleDb;

namespace KursProject.Modules
{
    /// <summary>
    /// Упрощенное взаимодействие с БД Access
    /// </summary>
    public class UsingAccess
    {
        #region простой участок кода

        /// <summary>
        /// Провайдер доступа. Например: Microsoft.Jet.OLEDB.4.0 / Microsoft.ACE.OLEDB.12.0
        /// </summary>
        public string Provider { get; set; } = null;

        /// <summary>
        /// Путь к базе данных
        /// </summary>
        public string DataSource { get; set; } = null;

        /// <summary>
        /// Смесь Provider и DataSource
        /// </summary>
        public string ConnectString
        {
            get
            {
                return $"Provider={this.Provider};Data Source={this.DataSource};";
            }
        }   

        /// <summary>
        /// Автоматическое подключение/отключение для SQL запросов
        /// </summary>
        public bool AutoOpen = false;

        /// <summary>
        /// Ссылка на экземпляр класса OleDbConnection для соединения с БД
        /// </summary>
        private OleDbConnection myConnection;
        
        /// <summary>
        /// Открывает соединение с БД
        /// </summary>
        public void ConnectOpen()
        {
            myConnection.Open();
        }

        /// <summary>
        /// Закрывает соединение с БД
        /// </summary>
        public void ConnectClose()
        {
            myConnection.Close();
        }

        /// <summary>
        /// Проверяет БД путем открытия и закрытия БД (Если ошибка, то выдает исключение)
        /// </summary>
        public void ConnectChech()
        {
            myConnection.Open();
            myConnection.Close();
        }

        #endregion

        /// <summary>
        /// Упрощенное взаимодействие с БД Access
        /// </summary>
        /// <param name="Provider">Провайдер доступа. Например: Microsoft.Jet.OLEDB.4.0 / Microsoft.ACE.OLEDB.12.0</param>
        /// <param name="DataSource">Путь к БД</param>
        public UsingAccess(string Provider, string DataSource)
        {
            this.Provider = Provider;
            this.DataSource = DataSource;

            myConnection = new OleDbConnection(this.ConnectString);
            ConnectChech();
        }

        /// <summary>
        /// Упрощенное взаимодействие с БД Access. Пробует подключатся через Jet.OLEDB.4.0, если ошибка, то ACE.OLEDB.12.0
        /// </summary>
        /// <param name="DataSource">Путь к бд</param>
        public UsingAccess(string DataSource)
        {
            this.DataSource = DataSource;

            try
            {
                this.Provider = "Microsoft.Jet.OLEDB.4.0";
                myConnection = new OleDbConnection(this.ConnectString);
                ConnectChech();
            }
            catch (System.Data.OleDb.OleDbException)
            {
                this.Provider = "Microsoft.ACE.OLEDB.12.0";
                myConnection = new OleDbConnection(this.ConnectString);
                ConnectChech();
            }
        }        

        /// <summary>
        /// Выполняет SQL запрос с возвратом данных 
        /// </summary>
        /// <param name="SQLRequest">SQL запрос</param>
        /// <returns>Таблицу (DataView)</returns>
        public DataView Request(string SQLRequest)
        {
            if (AutoOpen)
                ConnectOpen();

            OleDbCommand command = new OleDbCommand
            {
                Connection = myConnection,
                CommandText = SQLRequest
            };

            OleDbDataAdapter da = new OleDbDataAdapter(command);
            DataTable dt = new DataTable();
            da.Fill(dt);

            if (AutoOpen)
                ConnectClose();

            return dt.DefaultView;
        }

        /// <summary>
        /// Выполняет SQL запрос без возврата данных
        /// </summary>
        /// <param name="SQLRequest">SQL запрос</param>
        public void RequestWithResponse(string SQLRequest)
        {
            OleDbCommand command = new OleDbCommand(SQLRequest, myConnection);

            if (AutoOpen)
                ConnectOpen();

            command.ExecuteNonQuery();

            if (AutoOpen)
                ConnectClose();
        }
    }
}
