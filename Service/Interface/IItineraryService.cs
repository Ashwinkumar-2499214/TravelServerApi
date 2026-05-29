using TravelEaseServer.Dto;

namespace TravelEaseServer.Service.Interface
{
    public interface IItineraryService
    {
        Task<ItineraryResponseDto> CreateItineraryAsync(ItineraryRequestDto itineraryDto);
        Task<ItineraryResponseDto> GetItineraryByIdAsync(long itineraryId);
        Task<IEnumerable<ItineraryResponseDto>> GetAllItinerariesAsync(ItinerarySearchDto searchDto);
        Task<IEnumerable<ItineraryResponseDto>> GetUserItinerariesAsync(long userId);
        Task<ItineraryResponseDto> UpdateItineraryAsync(long itineraryId, ItineraryRequestDto itineraryDto);
        Task<bool> DeleteItineraryAsync(long itineraryId);
        Task<ItineraryResponseDto> UpdateItineraryStatusAsync(long itineraryId, int newStatus);
        Task<bool> AddBookingToItineraryAsync(long itineraryId, long bookingId);
        Task<bool> RemoveBookingFromItineraryAsync(long itineraryId, long bookingId);
    }
}
