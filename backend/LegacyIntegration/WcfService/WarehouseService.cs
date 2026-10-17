using System;
using System.Collections.Generic;

namespace SkirtPling.Legacy.WcfService
{
    /// <summary>
    /// Simulates the warehouse's own stock ledger with fixed data. Counts differ slightly from the
    /// shop's numbers on purpose, so the admin reconciliation screen has something to show.
    /// </summary>
    public class WarehouseService : IWarehouseService
    {
        private static readonly DateTime LastStockTake = new DateTime(2026, 10, 1, 6, 0, 0, DateTimeKind.Utc);

        private static readonly Dictionary<string, (int OnHand, string Warehouse)> Ledger =
            new Dictionary<string, (int, string)>(StringComparer.OrdinalIgnoreCase)
            {
                ["SP-PL-001"] = (26, "TLV-01"), ["SP-PL-002"] = (12, "TLV-01"), ["SP-PL-003"] = (5, "JLM-02"),
                ["SP-PL-004"] = (40, "TLV-01"), ["SP-PL-005"] = (7, "JLM-02"), ["SP-AL-001"] = (29, "TLV-01"),
                ["SP-AL-002"] = (18, "JLM-02"), ["SP-AL-003"] = (2, "TLV-01"), ["SP-AL-004"] = (22, "TLV-01"),
                ["SP-PE-001"] = (35, "TLV-01"), ["SP-PE-002"] = (16, "JLM-02"), ["SP-PE-003"] = (4, "JLM-02"),
                ["SP-MX-001"] = (21, "TLV-01"), ["SP-MX-002"] = (9, "JLM-02"), ["SP-MX-003"] = (48, "TLV-01"),
                ["SP-MX-004"] = (3, "JLM-02"), ["SP-DN-001"] = (28, "TLV-01"), ["SP-DN-002"] = (14, "TLV-01"),
                ["SP-DN-003"] = (0, "TLV-01"), ["SP-DN-004"] = (12, "JLM-02")
            };

        public WarehouseStock GetStock(string sku)
        {
            if (string.IsNullOrWhiteSpace(sku) || !Ledger.TryGetValue(sku.Trim(), out var entry))
                return null;

            Console.WriteLine($"{DateTime.Now:T} GetStock({sku}) -> {entry.OnHand}");
            return new WarehouseStock
            {
                Sku = sku.Trim().ToUpperInvariant(),
                OnHand = entry.OnHand,
                WarehouseCode = entry.Warehouse,
                LastCountedAt = LastStockTake
            };
        }
    }
}
