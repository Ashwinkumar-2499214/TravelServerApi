using TravelEaseServer.Dto;
using TravelEaseServer.Model;

namespace TravelEaseServer.Repository.Interface
{
    public interface IItineraryRepository
    {
        Task<ItineraryResponseDto> CreateItineraryAsync(Itinerary itinerary);
        Task<ItineraryResponseDto> GetItineraryByIdAsync(long itineraryId);
        Task<IEnumerable<ItineraryResponseDto>> GetAllItinerariesAsync(ItinerarySearchDto searchDto);
        Task<IEnumerable<ItineraryResponseDto>> GetItinerariesByUserIdAsync(long userId);
        Task<ItineraryResponseDto> UpdateItineraryAsync(Itinerary itinerary);
        Task<bool> DeleteItineraryAsync(long itineraryId);
        Task<ItineraryResponseDto> UpdateItineraryStatusAsync(long itineraryId, int status);
        Task<bool> AddBookingToItineraryAsync(long itineraryId, long bookingId);
        Task<bool> RemoveBookingFromItineraryAsync(long itineraryId, long bookingId);
    }
}
