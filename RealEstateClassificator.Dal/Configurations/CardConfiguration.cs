using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealEstateClassificator.Dal.Entities;

namespace RealEstateClassificator.Dal.Configurations;
internal class CardConfiguration: IEntityTypeConfiguration<Card>
{
    public void Configure(EntityTypeBuilder<Card> builder)
    {
        builder.HasKey(_ => _.Id);

        builder.Property(_ => _.Url)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(_ => _.MediaFiles)
            .HasColumnType("text[]");
    }

}
