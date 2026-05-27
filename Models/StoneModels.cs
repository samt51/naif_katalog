using System.Collections.Generic;

namespace naif_katalog.Models
{
    public class GetAllStoneQueryResponse
    {
        public int Id { get; set; }
public decimal CostPrice { get; set; }
        public int? StoneSettingId { get; set; }
        public naif_katalog.Core.Features.StoneSettingFeature.Queries.StoneSettingDto StoneSetting { get; set; }
        public int StoneTypeId { get; set; }
        public int UnitId { get; set; }
        public int? StoneCutId { get; set; }
        public int? StoneClarityId { get; set; }
        public int? ColorId { get; set; }
        
        public int? StoneScaleId { get; set; }
        public naif_katalog.Core.Features.DefinitionFeature.Queries.StoneScaleDto StoneScale { get; set; }
    }

    public class CreateStoneCommandResponse { }
    public class UpdateStoneCommandResponse { }
    public class DeleteStoneCommandResponse { }
}

