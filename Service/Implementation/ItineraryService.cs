using TravelEaseServer.Constant;
using TravelEaseServer.Dto;
using TravelEaseServer.Model;
using TravelEaseServer.Repository.Interface;
using TravelEaseServer.Service.Interface;

namespace TravelEaseServer.Service.Implementation
{
    public class ItineraryService : IItineraryService
    {
        private readonly IItineraryRepository _itineraryRepository;

        public ItineraryService(IItineraryRepository itineraryRepository)
        {
            _itineraryRepository = itineraryRepository;
        }

        public async Task<ItineraryResponseDto> CreateItineraryAsync(ItineraryRequestDto itineraryDto)
        {
            try
            {
                var itinerary = new Itinerary
                {
                    UserId = itineraryDto.UserId,
                    Title = itineraryDto.Title,
                    StartDate = itineraryDto.StartDate,
                    EndDate = itineraryDto.EndDate,
                    Status = (int)Enum.ItineraryStatus.Draft,
                    CreatedDate = DateTime.UtcNow
                };

                return await _itineraryRepository.CreateItineraryAsync(itinerary);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ItineraryConstants.ItineraryCreatedSuccess, ex);
            }
        }

        public async Task<ItineraryResponseDto> GetItineraryByIdAsync(long itineraryId)
        {
            return await _itineraryRepository.GetItineraryByIdAsync(itineraryId);
        }

        public async Task<IEnumerable<ItineraryResponseDto>> GetAllItinerariesAsync(ItinerarySearchDto searchDto)
        {
            return await _itineraryRepository.GetAllItinerariesAsync(searchDto);
        }

        public async Task<IEnumerable<ItineraryResponseDto>> GetUserItinerariesAsync(long userId)
        {
            return await _itineraryRepository.GetItinerariesByUserIdAsync(userId);
        }

        public async Task<ItineraryResponseDto> UpdateItineraryAsync(long itineraryId, ItineraryRequestDto itineraryDto)
        {
            try
            {
                var itinerary = new Itinerary
                {
                    ItineraryId = itineraryId,
                    UserId = itineraryDto.UserId,
                    Title = itineraryDto.Title,
                    StartDate = itineraryDto.StartDate,
                    EndDate = itineraryDto.EndDate,
                    ModifiedDate = DateTime.UtcNow
                };

                return await _itineraryRepository.UpdateItineraryAsync(itinerary);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ItineraryConstants.ItineraryUpdateSuccess, ex);
            }
        }

        public async Task<bool> DeleteItineraryAsync(long itineraryId)
        {
            return await _itineraryRepository.DeleteItineraryAsync(itineraryId);
        }

        public async Task<ItineraryResponseDto> UpdateItineraryStatusAsync(long itineraryId, int newStatus)
        {
            return await _itineraryRepository.UpdateItineraryStatusAsync(itineraryId, newStatus);
        }

        public async Task<bool> AddBookingToItineraryAsync(long itineraryId, long bookingId)
        {
            return await _itineraryRepository.AddBookingToItineraryAsync(itineraryId, bookingId);
        }

        public async Task<bool> RemoveBookingFromItineraryAsync(long itineraryId, long bookingId)
        {
            return await _itineraryRepository.RemoveBookingFromItineraryAsync(itineraryId, bookingId);
        }
    }
}
