using RealEstateClassificator.Dal.Entities;

namespace RealEstateClassificator.Dal.Configurations;
internal class CardConfiguration: EntityConfiguration<Card, Guid>
{
    /// <inheritdoc/>
    public override void Configure(EntityTypeBuilder<Card> builder)
    {
        builder.Property(_ => _.MediaFiles)
            .HasJsonConversion();

        base.Configure(builder);
    }
}
