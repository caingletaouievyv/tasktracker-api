// TaskTrackerApi/DTO/UpdateUserRolesRequest.cs

public class UpdateUserRolesRequest
{
    public string UserId { get; set; } = null!;
    public List<string> Roles { get; set; } = new();
}

