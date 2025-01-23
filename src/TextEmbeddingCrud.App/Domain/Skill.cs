using Pgvector;

namespace TextEmbeddingCrud.App.Domain;

public class Skill
{
    public Skill()
    {
        
    }
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Vector? Embedding { get; set; }
}