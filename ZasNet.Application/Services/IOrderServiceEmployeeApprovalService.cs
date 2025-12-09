namespace ZasNet.Application.Services;

public interface IOrderServiceEmployeeApprovalService
{
    Task<ApprovalResult> ApproveAssignedAsync(int orderServiceEmployeeId, CancellationToken cancellationToken);
    Task UpdateOrderStatusAfterEmployeeApprovalAsync(int orderId, int approvedOrderServiceEmployeeId, CancellationToken cancellationToken);
}

public sealed class ApprovalResult
{
    public bool Success { get; init; }
    public string? Message { get; init; }
}
