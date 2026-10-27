namespace dRz.Application.Models
{
    /// <summary>
    /// Описывает параметры столбца таблицы.
    /// </summary>
    public class DefinitionTableColumns
    {
        /// <summary>
        /// Создаёт описание столбца таблицы.
        /// </summary>
        public DefinitionTableColumns()
        {
        }

        /// <summary>
        /// Ширина столбца.
        /// </summary>
        public double ColumnWidth { get; set; }

        /// <summary>
        /// Высота столбца.
        /// </summary>
        public double ColumnHeight { get; set; }

        /// <summary>
        /// Наименование столбца.
        /// </summary>
        public string ColumnName { get; set; }
    }
}