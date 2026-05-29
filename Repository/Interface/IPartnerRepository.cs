using TravelEaseServer.Dto;
using TravelEaseServer.Model;

namespace TravelEaseServer.Repository.Interface
{
    public interface IPartnerRepository
    {
        Task<PartnerResponseDto> CreatePartnerAsync(Partner partner);
        Task<PartnerResponseDto> GetPartnerByIdAsync(long partnerId);
        Task<IEnumerable<PartnerResponseDto>> GetAllPartnersAsync(PartnerSearchDto searchDto);
        Task<PartnerResponseDto> UpdatePartnerAsync(Partner partner);
        Task<bool> DeletePartnerAsync(long partnerId);
        Task<PartnerResponseDto> UpdatePartnerStatusAsync(long partnerId, int status);
    }
}
