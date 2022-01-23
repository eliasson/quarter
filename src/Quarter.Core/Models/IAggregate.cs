using Quarter.Core.Utils;

namespace Quarter.Core.Models
{
    public interface IAggregate<TAggregate> where TAggregate : IAggregate<TAggregate>
    {
        /// <summary>
        /// The unique ID for this aggregate
        /// </summary>
        IdOf<TAggregate> Id { get; }

        /// <summary>
        /// The UTC date time this entity was first saved. If null, the entity has not been saved yet.
        /// </summary>
        UtcDateTime Created { get; }

        /// <summary>
        /// The UTC date time this entity was last updated. If null, the entity has never been updated.
        /// </summary>
        UtcDateTime? Updated { get; set; }
    }
}