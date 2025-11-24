using ZasNet.Domain.Entities;

namespace ZasNet.Application.Repository;

public interface IDocumentRepository : ILockedItemRepository<Document>
{
}
