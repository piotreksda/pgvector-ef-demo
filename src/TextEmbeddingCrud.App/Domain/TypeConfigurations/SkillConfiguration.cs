using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TextEmbeddingCrud.App.Domain.TypeConfigurations;

public class SkillConfiguration : IEntityTypeConfiguration<Skill>
{
    public void Configure(EntityTypeBuilder<Skill> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired();
        builder.Property(x => x.Embedding).HasColumnType("vector(1024)");
        
        builder
            .HasIndex(i => i.Embedding)
            .HasMethod("hnsw") // hnsw / ivfflat
            .HasOperators("vector_cosine_ops");
    }
}