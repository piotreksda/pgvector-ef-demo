using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OllamaSharp;
using OllamaSharp.Models;
using Pgvector;
using Pgvector.EntityFrameworkCore;
using Scalar.AspNetCore;
using TextEmbeddingCrud.App.Domain;
using TextEmbeddingCrud.App.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
                npgsqlOptions =>
                {
                    npgsqlOptions.UseVector(); // Chaining the UseVector extension method
                });
        }
    );

builder.Services.AddTransient<DapperDbContext>();

var app = builder.Build();

await MigrateAsync(app.Services);

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(opt =>
    {
        opt.Theme = ScalarTheme.Mars;
    });
}

app.MapGet("outer", async (
    HttpContext httpContext,
    [FromServices] DapperDbContext dapperContext,
    [FromServices] AppDbContext appDbContext
) =>
{
    var avgVector = await dapperContext.Connection.QueryFirstOrDefaultAsync<Vector>(
        "SELECT AVG(\"Embedding\") FROM public.\"Skills\"");
    
    var items = await appDbContext.Skills
        .Where(x => x.Embedding != null)
        .OrderByDescending(x => x.Embedding!.CosineDistance(avgVector))
        .Select(x => x.Name)
        .Take(15)
        .ToListAsync();

    return items;
});

app.MapGet("insert", async (
    HttpContext httpContext,
    [FromServices] AppDbContext appDbContext,
    [FromQuery] string input
    ) =>
{
    var ollamaClient = new OllamaApiClient(new Uri("http://localhost:11434"));
    var test = await ollamaClient.EmbedAsync(new EmbedRequest()
    {
        Model = "mxbai-embed-large",
        Input = [input]
    });
    
    var embedding = new Vector(test.Embeddings[0]);

    appDbContext.Skills.Add(new Skill()
    {
        Name = input,
        Embedding = embedding
    });
    
    await appDbContext.SaveChangesAsync();
});

app.MapGet("simmilar/{id:int}", async (
    HttpContext httpContext,
    [FromServices] AppDbContext appDbContext,
    [FromRoute] int id
) =>
{
    var items = await appDbContext.Skills
        .OrderBy(x => x.Embedding!.CosineDistance(
            appDbContext.Skills
                .Where(y => y.Id == id)
                .Select(y => y.Embedding)
                .FirstOrDefault()!))
        .Select(x => new Similarity(x.Name, x.Embedding!.CosineDistance(
            appDbContext.Skills
                .Where(y => y.Id == id)
                .Select(y => y.Embedding)
                .FirstOrDefault()!)))
        .Take(5)
        .ToListAsync();

    return items;
});

app.MapGet("simmilar", async (
    HttpContext httpContext,
    [FromServices] AppDbContext appDbContext,
    [FromQuery] string input
) =>
{
    var ollamaClient = new OllamaApiClient(new Uri("http://localhost:11434"));
    var test = await ollamaClient.EmbedAsync(new EmbedRequest()
    {
        Model = "mxbai-embed-large",
        Input = [input]
    });
    
    var embedding = new Vector(test.Embeddings[0]);
    
    var items = await appDbContext.Skills
        .OrderBy(x => x.Embedding!.CosineDistance(
            embedding))
        .Select(x => 
                new Similarity(x.Name, x.Embedding!.CosineDistance(
                embedding))
            )
        .Take(5)
        .ToListAsync();

    return items;
});

app.MapPost("seed", async ([FromServices] AppDbContext appDbContext) =>
{
    string[] jobSkills = new string[]
    {
        "Communication",
        "Teamwork",
        "Problem Solving",
        "Time Management",
        "Adaptability",
        "Critical Thinking",
        "Creativity",
        "Leadership",
        "Conflict Resolution",
        "Negotiation",
        "Decision Making",
        "Project Management",
        "Customer Service",
        "Public Speaking",
        "Technical Writing",
        "Research",
        "Data Analysis",
        "Programming",
        "Web Development",
        "Database Management",
        "Cybersecurity",
        "Networking",
        "Cloud Computing",
        "AI/ML",
        "UI/UX Design",
        "SEO",
        "Digital Marketing",
        "Content Creation",
        "Social Media Management",
        "Sales",
        "Accounting",
        "Bookkeeping",
        "Financial Analysis",
        "Budgeting",
        "Tax Preparation",
        "Supply Chain Management",
        "Inventory Management",
        "Manufacturing",
        "Quality Assurance",
        "Operations Management",
        "HR Management",
        "Recruitment",
        "Onboarding",
        "Training and Development",
        "Performance Evaluation",
        "Employee Relations",
        "Legal Compliance",
        "Market Research",
        "Business Strategy",
        "Product Development",
        "Brand Management",
        "Event Planning",
        "Graphic Design",
        "Photography",
        "Video Editing",
        "Copywriting",
        "Technical Support",
        "Help Desk",
        "Troubleshooting",
        "IT Support",
        "System Administration",
        "DevOps",
        "Agile Methodologies",
        "Scrum",
        "Kanban",
        "Lean Management",
        "Test Automation",
        "Software Testing",
        "Game Development",
        "Mobile App Development",
        "E-commerce",
        "Email Marketing",
        "Google Analytics",
        "Data Visualization",
        "Big Data",
        "Data Mining",
        "ETL Processes",
        "Business Intelligence",
        "CRM Systems",
        "ERP Systems",
        "Machine Learning Algorithms",
        "Natural Language Processing",
        "Deep Learning",
        "Robotics",
        "IoT",
        "Blockchain",
        "Smart Contracts",
        "Quantitative Analysis",
        "Statistical Modeling",
        "Mathematical Optimization",
        "Actuarial Analysis",
        "Survey Design",
        "Environmental Science",
        "Lab Techniques",
        "Regulatory Compliance",
        "Healthcare Administration",
        "Clinical Trials",
        "Patient Care",
        "Nursing",
        "Pharmacy",
        "Medical Coding",
        "Emergency Response",
        "Education",
        "Curriculum Development",
        "Classroom Management",
        "Lesson Planning",
        "Student Counseling",
        "Language Translation",
        "Interpreting",
        "Sign Language",
        "Logistics",
        "Fleet Management",
        "Freight Coordination",
        "Customs Compliance",
        "Procurement",
        "Contract Negotiation",
        "Vendor Management",
        "Energy Management",
        "Renewable Energy",
        "HVAC",
        "Electrical Engineering",
        "Mechanical Engineering",
        "Civil Engineering",
        "Architectural Design",
        "Construction Management",
        "Welding",
        "Plumbing",
        "Carpentry",
        "Auto Repair",
        "Aviation Maintenance",
        "Driving",
        "Forklift Operation",
        "Warehouse Operations",
        "Security",
        "Surveillance",
        "Risk Assessment",
        "Emergency Management",
        "Fire Safety",
        "Cleaning",
        "Housekeeping",
        "Landscaping",
        "Gardening",
        "Agriculture",
        "Animal Care",
        "Childcare",
        "Elder Care",
        "Personal Training",
        "Fitness Coaching",
        "Diet and Nutrition",
        "Hairdressing",
        "Cosmetology",
        "Makeup Artistry",
        "Tattooing",
        "Retail Sales",
        "Visual Merchandising",
        "Cash Handling",
        "Point of Sale (POS) Systems",
        "Inventory Control",
        "Restaurant Management",
        "Culinary Arts",
        "Bartending",
        "Wine Knowledge"
    };

    foreach (var skill in jobSkills)
    {
        var ollamaClient = new OllamaApiClient(new Uri("http://localhost:11434"));
        var test = await ollamaClient.EmbedAsync(new EmbedRequest()
        {
            Model = "mxbai-embed-large",
            Input = [skill]
        });

        var embedding = new Vector(test.Embeddings[0]);

        appDbContext.Skills.Add(new Skill()
        {
            Name = skill,
            Embedding = embedding
        });
    }

    await appDbContext.SaveChangesAsync();
});

app.UseHttpsRedirection();

app.Run();

return;

static async Task MigrateAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();
}

public sealed record Similarity(string Name, double Distance);
public class AverageEmbedding
{
    public float[]? Embedding { get; set; }
}