namespace MotorsShop.Dtos;

public record PagedResult<T> (IEnumerable<T> Items, int Page, 
                                int PageSize, int TotalCount)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;
}
