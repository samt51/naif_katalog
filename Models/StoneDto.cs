namespace naif_katalog.Models
{
    public class StoneDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? StoneScaleId { get; set; }
        public naif_katalog.Core.Features.DefinitionFeature.Queries.StoneScaleDto StoneScale { get; set; }
        public decimal CostPrice { get; set; }
        public int? StoneSettingId { get; set; }
        public naif_katalog.Core.Features.StoneSettingFeature.Queries.StoneSettingDto StoneSetting { get; set; }
        public int? StoneClarityId { get; set; }
    }
}


