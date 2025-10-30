namespace ZasNet.Domain.Entities
{
    public abstract class LockedItemBase : BaseItem
    {
        public int? LockedByUserId { get; set; }

        public DateTime? LockedAt { get; set; }

        public bool IsLocked => this.LockedByUserId.HasValue && LockedAt.HasValue;
    }
}
