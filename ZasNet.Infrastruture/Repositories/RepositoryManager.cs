using ZasNet.Application.Repository;
using ZasNet.Infrastruture.Persistence;

namespace ZasNet.Infrastruture.Repositories;

public class RepositoryManager : IRepositoryManager
{
    private readonly ZasNetDbContext context;
    private readonly Lazy<ICarModelRepository> carModelRepository;
    private readonly Lazy<ICarRepository> carRepository;
    private readonly Lazy<IDocumentRepository> documentRepository;
    private readonly Lazy<IEmployeeRepository> employeeRepository;
    private readonly Lazy<IOrderCarRepository> orderCarRepository;
    private readonly Lazy<IOrderEmployeeRepository> orderEmployeeRepository;
    private readonly Lazy<IOrderRepository> orderRepository;
    private readonly Lazy<IOrderServiceRepository> orderServiceRepository;
    private readonly Lazy<IServiceRepository> serviceRepository;
    private readonly Lazy<IUserRepository> userRepository;
    private readonly Lazy<IRoleRepository> roleRepository;

    public RepositoryManager(ZasNetDbContext zasNetDbContext)
    {
        context = zasNetDbContext;
        carModelRepository = new Lazy<ICarModelRepository>(()=> new CarModelRepository(context));
        carRepository = new Lazy<ICarRepository>(()=> new CarRepository(context));
        documentRepository = new Lazy<IDocumentRepository>(()=> new DocumentRepository(context));
        employeeRepository = new Lazy<IEmployeeRepository>(()=> new EmployeeRepository(context));
        orderCarRepository = new Lazy<IOrderCarRepository>(()=> new OrderCarRepository(context));
        orderEmployeeRepository = new Lazy<IOrderEmployeeRepository>(()=> new OrderEmployeeRepository(context));
        orderRepository = new Lazy<IOrderRepository>(()=> new OrderRepository(context));
        orderServiceRepository = new Lazy<IOrderServiceRepository>(()=> new OrderServiceRepository(context));
        serviceRepository = new Lazy<IServiceRepository>(()=> new ServiceRepository(context));
        userRepository = new Lazy<IUserRepository>(()=> new UserRepository(context));
        roleRepository = new Lazy<IRoleRepository>(()=> new RoleRepository(context));
    }

    public ICarModelRepository CarModelRepository => this.carModelRepository.Value;

    public ICarRepository CarRepository => this.carRepository.Value;

    public IDocumentRepository DocumentRepository => this.documentRepository.Value;

    public IEmployeeRepository EmployeeRepository => this.employeeRepository.Value;

    public IOrderCarRepository OrderCarRepository => this.orderCarRepository.Value;

    public IOrderEmployeeRepository OrderEmployeeRepository => this.orderEmployeeRepository.Value;

    public IOrderRepository OrderRepository => this.orderRepository.Value;

    public IOrderServiceRepository OrderServiceRepository => this.orderServiceRepository.Value;

    public IServiceRepository ServiceRepository => this.serviceRepository.Value;

    public IUserRepository UserRepository => this.userRepository.Value;

    public IRoleRepository RoleRepository => this.roleRepository.Value;

    public async Task SaveAsync(CancellationToken cancellationToken)
    {
        await this.context.SaveChangesAsync(cancellationToken);
    }
}
