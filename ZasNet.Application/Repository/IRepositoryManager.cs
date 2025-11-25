namespace ZasNet.Application.Repository;

public interface IRepositoryManager
{
    ICarModelRepository CarModelRepository { get; }
    ICarRepository CarRepository { get; }
    IDocumentRepository DocumentRepository { get; }
    IEmployeeRepository EmployeeRepository { get; }
    IOrderCarRepository OrderCarRepository { get; }
    IOrderEmployeeRepository OrderEmployeeRepository { get; }
    IOrderRepository OrderRepository { get; }
    IOrderServiceRepository OrderServiceRepository { get; }
    IServiceRepository ServiceRepository { get; }
    IRoleRepository RoleRepository { get; }
    Task SaveAsync(CancellationToken cancellationToken);
}
