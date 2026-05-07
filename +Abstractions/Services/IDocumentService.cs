namespace drz.Abstractions.Services
{
    /// <summary>Информация о активном документе</summary>
    public interface IDocumentService
    {
        /// <summary> Активен ли документ </summary>
        bool IsActive { get; }

        /// <summary> Полный путь к документу. В случае, если документа нет, должен генерировать ошибку </summary>
        string FullPath { get; }

        /// <summary> Имя файла с расширением. Если документа нет, должен генерировать ошибку </summary>
        string FileName { get; }

        /// <summary> Имя файла без расширения. Если документа нет, должен генерировать ошибку </summary>
        string FileNameNoExtension { get; }
    }
}