using TravelEaseServer.Dto;

namespace TravelEaseServer.Service.Interface
{
    public interface IPartnerService
    {
        Task<PartnerResponseDto> CreatePartnerAsync(PartnerRequestDto partnerDto);
        Task<PartnerResponseDto> GetPartnerByIdAsync(long partnerId);
        Task<IEnumerable<PartnerResponseDto>> GetAllPartnersAsync(PartnerSearchDto searchDto);
        Task<PartnerResponseDto> UpdatePartnerAsync(long partnerId, PartnerRequestDto partnerDto);
        Task<bool> DeletePartnerAsync(long partnerId);
        Task<PartnerResponseDto> UpdatePartnerStatusAsync(long partnerId, int newStatus);
    }
}
