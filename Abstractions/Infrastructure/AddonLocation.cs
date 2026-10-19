namespace dRz.Abstractions.Infrastructure
{
    /// <summary>
    /// Расположение аддона относительно каталога продукта.
    /// </summary>
    public enum AddonLocation
    {
        /// <summary>
        /// Аддон находится в ожидаемой структуре каталогов продукта.
        /// </summary>
        ProductDirectory,

        /// <summary>
        /// Аддон находится вне ожидаемой структуры каталогов продукта.
        /// </summary>
        ExternalDirectory
    }
}
