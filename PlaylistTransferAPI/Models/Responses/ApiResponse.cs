namespace PlaylistTransferAPI.Models.Responses;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string RequestId { get; set; } = string.Empty;

    public static ApiResponse<T> SuccessResponse(T data, string message = "Success")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Message = message,
            RequestId = Guid.NewGuid().ToString("N")[..8]
        };
    }

    public static ApiResponse<T> ErrorResponse(string message, List<string>? errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Errors = errors ?? new List<string>(),
            RequestId = Guid.NewGuid().ToString("N")[..8]
        };
    }

    public static ApiResponse<T> ErrorResponse(string message, string error)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Errors = new List<string> { error },
            RequestId = Guid.NewGuid().ToString("N")[..8]
        };
    }
}

public class PaginatedResponse<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => PageNumber < TotalPages;
    public bool HasPreviousPage => PageNumber > 1;
}

public class TransferStatusResponse
{
    public string TransferId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int Progress { get; set; }
    public string CurrentTask { get; set; } = string.Empty;
    public int ProcessedTracks { get; set; }
    public int TotalTracks { get; set; }
    public int SuccessfulTracks { get; set; }
    public int FailedTracks { get; set; }
    public DateTime StartedAt { get; set; }
    public TimeSpan? EstimatedTimeRemaining { get; set; }
    public List<string> RecentErrors { get; set; } = new();
}