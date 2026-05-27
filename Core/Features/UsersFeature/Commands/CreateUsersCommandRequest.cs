using MediatR;
using naif_katalog.Models;
using System;

namespace naif_katalog.Core.Features.UsersFeature.Commands
{
    public class CreateUsersCommandRequest : IRequest<ResponseDto<CreateUsersCommandResponse>>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CompanyName { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Password { get; set; }
        public int RoleId { get; set; }
        public int CustomerGroupId { get; set; }
        public DateTime? SubscriptionEndDate { get; set; }
        public bool IsLocked { get; set; }
        
        public decimal? CustomMilyem { get; set; }
        public decimal? SalesMultiplier { get; set; }
        public System.Collections.Generic.List<UserStonePriceDto>? CustomStonePrices { get; set; }
        public System.Collections.Generic.List<UserPolishingCostDto>? CustomPolishingCosts { get; set; }
        public System.Collections.Generic.List<UserSettingPriceDto>? CustomSettingPrices { get; set; }
    }
}
