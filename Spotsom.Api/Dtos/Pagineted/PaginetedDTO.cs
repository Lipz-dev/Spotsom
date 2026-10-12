namespace SpotSom.Api.Dtos;

public record PaginetedDto<T>(
    List<T> Items,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages

);
