namespace QLTBYT.API.Models.Common;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public List<string>? Errors { get; set; }

    public static ApiResponse<T> CreateSuccess(T data, string? message = null) =>
        new() { Success = true, Data = data, Message = message };

    public static ApiResponse<T> CreateFail(string message, List<string>? errors = null) =>
        new() { Success = false, Message = message, Errors = errors };
}