using TravelEaseServer.Constant;
using TravelEaseServer.Dto;
using TravelEaseServer.Model;
using TravelEaseServer.Repository.Interface;
using TravelEaseServer.Service.Interface;

namespace TravelEaseServer.Service.Implementation
{
    public class PartnerService : IPartnerService
    {
        private readonly IPartnerRepository _partnerRepository;

        public PartnerService(IPartnerRepository partnerRepository)
        {
            _partnerRepository = partnerRepository;
        }

        public async Task<PartnerResponseDto> CreatePartnerAsync(PartnerRequestDto partnerDto)
        {
            try
            {
                var partner = new Partner
                {
                    Name = partnerDto.Name,
                    Type = partnerDto.Type,
                    ContactEmail = partnerDto.ContactEmail,
                    ContactPhone = partnerDto.ContactPhone,
                    Address = partnerDto.Address,
                    Status = (int)Enum.PartnerStatus.Active,
                    CreatedDate = DateTime.UtcNow
                };

                return await _partnerRepository.CreatePartnerAsync(partner);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(PartnerConstants.PartnerCreatedSuccess, ex);
            }
        }

        public async Task<PartnerResponseDto> GetPartnerByIdAsync(long partnerId)
        {
            return await _partnerRepository.GetPartnerByIdAsync(partnerId);
        }

        public async Task<IEnumerable<PartnerResponseDto>> GetAllPartnersAsync(PartnerSearchDto searchDto)
        {
            return await _partnerRepository.GetAllPartnersAsync(searchDto);
        }

        public async Task<PartnerResponseDto> UpdatePartnerAsync(long partnerId, PartnerRequestDto partnerDto)
        {
            try
            {
                var partner = new Partner
                {
                    PartnerId = partnerId,
                    Name = partnerDto.Name,
                    Type = partnerDto.Type,
                    ContactEmail = partnerDto.ContactEmail,
                    ContactPhone = partnerDto.ContactPhone,
                    Address = partnerDto.Address,
                    ModifiedDate = DateTime.UtcNow
                };

                return await _partnerRepository.UpdatePartnerAsync(partner);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(PartnerConstants.PartnerUpdateSuccess, ex);
            }
        }

        public async Task<bool> DeletePartnerAsync(long partnerId)
        {
            return await _partnerRepository.DeletePartnerAsync(partnerId);
        }

        public async Task<PartnerResponseDto> UpdatePartnerStatusAsync(long partnerId, int newStatus)
        {
            return await _partnerRepository.UpdatePartnerStatusAsync(partnerId, newStatus);
        }
    }
}
