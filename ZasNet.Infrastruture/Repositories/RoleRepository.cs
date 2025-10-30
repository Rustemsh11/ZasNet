using ZasNet.Application.Repository;
using ZasNet.Domain.Entities;
using ZasNet.Infrastruture.Persistence;

namespace ZasNet.Infrastruture.Repositories;

public class RoleRepository(ZasNetDbContext zasNetDbContext): Repository<Role>(zasNetDbContext), IRoleRepository;
