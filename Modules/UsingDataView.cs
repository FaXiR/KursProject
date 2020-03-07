using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KursProject.Modules
{
    /// <summary>
    /// Упрощенное взаимодейстеие с таблицой
    /// </summary>
    class UsingDataView
    {
        /// <summary>
        /// Упрощенное взаимодейстевие с Access
        /// </summary>
        private UsingAccess UsAc;

        /// <summary>
        /// Таблица
        /// </summary>
        public DataView Table { get; private set; }

        /// <summary>
        /// Поля для отображения
        /// </summary>
        public string Select { get; set; }

        /// <summary>
        /// Таблица из которой берутся поля
        /// </summary>
        public string From { get; set; }

        /// <summary>
        /// Условие отбора
        /// </summary>
        public string Where { get; set; }

        /// <summary>
        /// Сортировка по одному полю (Если задается одинаковое поле, то добавляется DESC)
        /// </summary>
        public string OrderBy
        {
            get
            {
                return orderBy;
            }
            set
            {
                if (value == null)
                {
                    orderBy = null;
                }
                else if (orderBy == value)
                {
                    orderBy = value + " DESC";
                }
                else
                {
                    orderBy = value;
                }
            }
        }
        private string orderBy = null;

        /// <summary>
        /// Формируемый запрос для таблицы
        /// </summary>
        public string SQLRequest { get; private set; }

        /// <summary>
        /// Обновление данных в Таблице
        /// </summary>
        public void UpdateTable()
        {
            SQLRequest = null;

            if (Select != null)
            {
                SQLRequest += $"Select {Select} ";
            }
            if (From != null)
            {
                SQLRequest += $"from {From} ";
            }
            if (Where != null)
            {
                SQLRequest += $"where {Where} ";
            }
            if (orderBy != null)
            {
                SQLRequest += $"ORDER BY {orderBy}";
            }

            Table = UsAc.Request(SQLRequest);
        }

        /// <summary>
        /// Упрощенное взаимодейстеие с таблицой
        /// </summary>
        /// <param name="UsAc">Для обновления содержимого таблицы</param>
        /// <param name="Select">Поля</param>
        /// <param name="From">Таблицы</param>
        /// <param name="Where">Условия</param>
        /// <param name="OrderBy">Сортировка</param>
        public UsingDataView(UsingAccess UsAc, string Select, string From, string Where, string OrderBy)
        {
            this.UsAc = UsAc;
            this.Select = Select;
            this.From = From;
            this.Where = Where;
            this.OrderBy = OrderBy;
        }
    }
}
