namespace Ura.ViewModels
{
    /// <summary>
    /// Фильтруемая сущность.
    /// </summary>
    public interface IFiltrated
    {
        bool Filter(string query);
        string Represent { get; }
    }
}