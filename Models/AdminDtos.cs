using System;

namespace naif_katalog.Models
{
    public class UserStonePriceDto
    {
        public int StoneId { get; set; }
        public decimal? CustomPrice { get; set; }
        public decimal? CustomSettingPrice { get; set; }
    }

    public class UserPolishingCostDto
    {
        public int? CategoryId { get; set; }
        public decimal CustomPrice { get; set; }
    }
    
    public class UserPricingProfileDto
    {
        public decimal? CustomMilyem { get; set; }
        public decimal? SalesMultiplier { get; set; }
    }

    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ParentId { get; set; }
        public int OrderIndex { get; set; }
    }

    public class UsersDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ParentId { get; set; }
        public int OrderIndex { get; set; }
        public string Surname { get; set; }
        public string CompanyName { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public int RoleId { get; set; }
        public DateTime? MembershipExpiryDate { get; set; }
        public bool IsLocked { get; set; }
        public UserPricingProfileDto PricingProfile { get; set; }
        public System.Collections.Generic.List<UserStonePriceDto> CustomStonePrices { get; set; }
        public System.Collections.Generic.List<UserPolishingCostDto> CustomPolishingCosts { get; set; }
        public System.Collections.Generic.List<UserSettingPriceDto> CustomSettingPrices { get; set; }
    }

    public class UserSettingPriceDto
    {
        public int StoneSettingId { get; set; }
        public decimal? CustomPrice { get; set; }
    }
}
