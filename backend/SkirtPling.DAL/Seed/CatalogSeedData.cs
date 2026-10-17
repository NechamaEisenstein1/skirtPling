using SkirtPling.DAL.Entities;

namespace SkirtPling.DAL.Seed;

/// <summary>Deterministic demo catalog. Image files live in frontend/public/images/products.</summary>
internal static class CatalogSeedData
{
    private record ProductSeed(string Sku, string Name, string Brand, string Category, decimal Price, int Stock,
        string Color, string Fabric, string Length, string Description);

    private static readonly (string Name, string Slug, string Description)[] Brands =
    [
        ("Pling Atelier", "pling-atelier", "Our in-house label: timeless cuts, tailored in small batches."),
        ("Modesta", "modesta", "Elegant everyday skirts with generous lengths and soft drape."),
        ("Linen & Loom", "linen-and-loom", "Natural fibres, relaxed silhouettes, made for warm days."),
        ("Velvet Row", "velvet-row", "Rich textures and festive finishes for special occasions."),
        ("Indigo Lane", "indigo-lane", "Durable denim skirts cut for comfort and movement.")
    ];

    private static readonly (string Name, string Slug)[] Categories =
    [
        ("Pleated", "pleated"),
        ("A-Line", "a-line"),
        ("Pencil", "pencil"),
        ("Maxi", "maxi"),
        ("Denim", "denim")
    ];

    private static readonly ProductSeed[] Products =
    [
        new("SP-PL-001", "Classic Pleated Midi", "Pling Atelier", "Pleated", 189.90m, 24, "Navy", "Polyester crepe", "Midi (75 cm)", "Knife pleats that hold their shape all day, with a smooth elastic-back waistband."),
        new("SP-PL-002", "Accordion Pleat Skirt", "Pling Atelier", "Pleated", 209.00m, 12, "Champagne", "Satin", "Midi (78 cm)", "Fine accordion pleats with a soft satin sheen. Fully lined."),
        new("SP-PL-003", "Chiffon Sunray Pleats", "Velvet Row", "Pleated", 249.00m, 6, "Dusty Rose", "Chiffon", "Midi (80 cm)", "Sunray pleats that flare beautifully as you walk."),
        new("SP-PL-004", "Everyday Pleated Skirt", "Modesta", "Pleated", 149.00m, 40, "Black", "Knit jersey", "Midi (76 cm)", "An easy-care knit skirt that goes from office to evening."),
        new("SP-AL-001", "Tailored A-Line Skirt", "Pling Atelier", "A-Line", 169.00m, 30, "Charcoal", "Wool blend", "Below knee (68 cm)", "Structured wool-blend A-line with side pockets and a concealed zip."),
        new("SP-AL-002", "Linen A-Line Skirt", "Linen & Loom", "A-Line", 159.00m, 18, "Sand", "Washed linen", "Midi (74 cm)", "Breathable washed linen with a comfortable drawstring waist."),
        new("SP-AL-003", "Corduroy A-Line", "Modesta", "A-Line", 179.00m, 0, "Camel", "Cotton corduroy", "Below knee (70 cm)", "Soft fine-wale corduroy with a front button placket."),
        new("SP-AL-004", "Floral A-Line Skirt", "Modesta", "A-Line", 139.00m, 22, "Sage", "Viscose", "Midi (75 cm)", "A light floral print on flowing viscose, perfect for spring."),
        new("SP-PE-001", "Office Pencil Skirt", "Pling Atelier", "Pencil", 159.00m, 35, "Black", "Stretch suiting", "Below knee (66 cm)", "A sharp, comfortable pencil skirt with a back kick-pleat."),
        new("SP-PE-002", "Ribbed Knit Pencil", "Modesta", "Pencil", 129.00m, 16, "Burgundy", "Rib knit", "Midi (72 cm)", "Stretchy rib knit that moves with you. No zip, no fuss."),
        new("SP-PE-003", "Velvet Pencil Skirt", "Velvet Row", "Pencil", 219.00m, 4, "Emerald", "Stretch velvet", "Midi (72 cm)", "Plush stretch velvet for festive evenings."),
        new("SP-MX-001", "Flowing Maxi Skirt", "Linen & Loom", "Maxi", 199.00m, 20, "Olive", "Cotton voile", "Maxi (95 cm)", "A tiered cotton maxi with a soft elastic waist."),
        new("SP-MX-002", "Satin Maxi Skirt", "Velvet Row", "Maxi", 269.00m, 9, "Midnight", "Satin", "Maxi (98 cm)", "Bias-cut satin that drapes and shimmers."),
        new("SP-MX-003", "Jersey Maxi Basic", "Modesta", "Maxi", 119.00m, 50, "Heather Grey", "Cotton jersey", "Maxi (96 cm)", "The everyday maxi: soft, opaque and easy to style."),
        new("SP-MX-004", "Tiered Linen Maxi", "Linen & Loom", "Maxi", 229.00m, 3, "Terracotta", "Linen", "Maxi (97 cm)", "Three generous tiers of washed linen in a warm earthy tone."),
        new("SP-DN-001", "Classic Denim Midi", "Indigo Lane", "Denim", 179.00m, 28, "Indigo", "Cotton denim", "Midi (76 cm)", "Mid-wash denim with five pockets and a front slit-free hem."),
        new("SP-DN-002", "Denim Maxi Skirt", "Indigo Lane", "Denim", 209.00m, 14, "Light Wash", "Cotton denim", "Maxi (94 cm)", "A long denim skirt with back pleat for easy walking."),
        new("SP-DN-003", "Black Denim A-Line", "Indigo Lane", "Denim", 169.00m, 0, "Washed Black", "Stretch denim", "Below knee (70 cm)", "Washed black stretch denim in a flattering A-line cut."),
        new("SP-DN-004", "Button-Front Denim", "Indigo Lane", "Denim", 189.00m, 11, "Mid Blue", "Cotton denim", "Midi (78 cm)", "Buttons all the way down with a soft, broken-in feel."),
        new("SP-PL-005", "Metallic Pleated Skirt", "Velvet Row", "Pleated", 289.00m, 7, "Gold", "Lamé", "Midi (80 cm)", "Shimmering metallic pleats for weddings and celebrations.")
    ];

    public static List<Product> CreateProducts()
    {
        var brands = Brands.ToDictionary(b => b.Name, b => new Brand { Name = b.Name, Slug = b.Slug, Description = b.Description });
        var categories = Categories.ToDictionary(c => c.Name, c => new Category { Name = c.Name, Slug = c.Slug });
        var firstDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        return Products.Select((seed, index) => new Product
        {
            Sku = seed.Sku,
            Name = seed.Name,
            Description = seed.Description,
            Price = seed.Price,
            StockQuantity = seed.Stock,
            IsActive = true,
            CreatedAt = firstDate.AddDays(index),
            Brand = brands[seed.Brand],
            Category = categories[seed.Category],
            Images =
            [
                new ProductImage { Url = $"/images/products/{seed.Sku.ToLowerInvariant()}-1.svg", AltText = $"{seed.Name}, front view", SortOrder = 0 },
                new ProductImage { Url = $"/images/products/{seed.Sku.ToLowerInvariant()}-2.svg", AltText = $"{seed.Name}, fabric detail", SortOrder = 1 }
            ],
            Specifications =
            [
                new ProductSpecification { Name = "Color", Value = seed.Color },
                new ProductSpecification { Name = "Fabric", Value = seed.Fabric },
                new ProductSpecification { Name = "Length", Value = seed.Length },
                new ProductSpecification { Name = "Care", Value = "Machine wash cold, hang to dry" }
            ]
        }).ToList();
    }
}
