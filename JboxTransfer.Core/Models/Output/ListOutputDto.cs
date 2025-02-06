namespace JboxTransfer.Core.Models.Output
{
    public class ListOutputDto<T>
    {
        public int Total { get; set; }
        public List<T> Entities { get; set; }

        public ListOutputDto(List<T> entities)
        {
            Entities = entities;
            Total = entities.Count;
        }
    }
}
