using Microsoft.EntityFrameworkCore;
using TravelEaseServer.Dto;
using TravelEaseServer.Model;
using TravelEaseServer.Repository.Interface;

namespace TravelEaseServer.Repository.Implementation
{
    public class ItineraryRepository : IItineraryRepository
    {
        private readonly AppDbContext _context;

        public ItineraryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ItineraryResponseDto> CreateItineraryAsync(Itinerary itinerary)
        {
            try
            {
                _context.Itineraries.Add(itinerary);
                await _context.SaveChangesAsync();
                return await MapItineraryToDtoAsync(itinerary);
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Error creating itinerary in database.", ex);
            }
        }

        public async Task<ItineraryResponseDto> GetItineraryByIdAsync(long itineraryId)
        {
            try
            {
                var itinerary = await _context.Itineraries
                    .AsNoTracking()
                    .FirstOrDefaultAsync(i => i.ItineraryId == itineraryId);

                return itinerary != null ? await MapItineraryToDtoAsync(itinerary) : null;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving itinerary with ID {itineraryId}.", ex);
            }
        }

        public async Task<IEnumerable<ItineraryResponseDto>> GetAllItinerariesAsync(ItinerarySearchDto searchDto)
        {
            try
            {
                var query = _context.Itineraries.AsNoTracking();

                // Apply filters
                if (searchDto.UserId.HasValue)
                {
                    query = query.Where(i => i.UserId == searchDto.UserId.Value);
                }

                if (searchDto.Status.HasValue)
                {
                    query = query.Where(i => i.Status == searchDto.Status.Value);
                }

                if (searchDto.FromDate.HasValue)
                {
                    query = query.Where(i => i.StartDate >= searchDto.FromDate.Value);
                }

                if (searchDto.ToDate.HasValue)
                {
                    query = query.Where(i => i.EndDate <= searchDto.ToDate.Value);
                }

                // Apply pagination
                int skip = (searchDto.PageNumber - 1) * searchDto.PageSize;
                var itineraries = await query
                    .OrderByDescending(i => i.CreatedDate)
                    .Skip(skip)
                    .Take(searchDto.PageSize)
                    .ToListAsync();

                var result = new List<ItineraryResponseDto>();
                foreach (var itinerary in itineraries)
                {
                    result.Add(await MapItineraryToDtoAsync(itinerary));
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error retrieving itineraries.", ex);
            }
        }

        public async Task<IEnumerable<ItineraryResponseDto>> GetItinerariesByUserIdAsync(long userId)
        {
            try
            {
                var itineraries = await _context.Itineraries
                    .AsNoTracking()
                    .Where(i => i.UserId == userId)
                    .OrderByDescending(i => i.CreatedDate)
                    .ToListAsync();

                var result = new List<ItineraryResponseDto>();
                foreach (var itinerary in itineraries)
                {
                    result.Add(await MapItineraryToDtoAsync(itinerary));
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving itineraries for user with ID {userId}.", ex);
            }
        }

        public async Task<ItineraryResponseDto> UpdateItineraryAsync(Itinerary itinerary)
        {
            try
            {
                var existingItinerary = await _context.Itineraries.FirstOrDefaultAsync(i => i.ItineraryId == itinerary.ItineraryId);
                if (existingItinerary == null)
                {
                    throw new KeyNotFoundException($"Itinerary with ID {itinerary.ItineraryId} not found.");
                }

                existingItinerary.Title = itinerary.Title;
                existingItinerary.StartDate = itinerary.StartDate;
                existingItinerary.EndDate = itinerary.EndDate;
                existingItinerary.Status = itinerary.Status;

                _context.Itineraries.Update(existingItinerary);
                await _context.SaveChangesAsync();

                return await MapItineraryToDtoAsync(existingItinerary);
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Error updating itinerary with ID {itinerary.ItineraryId}.", ex);
            }
        }

        public async Task<bool> DeleteItineraryAsync(long itineraryId)
        {
            try
            {
                var itinerary = await _context.Itineraries.FirstOrDefaultAsync(i => i.ItineraryId == itineraryId);
                if (itinerary == null)
                {
                    return false;
                }

                _context.Itineraries.Remove(itinerary);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Error deleting itinerary with ID {itineraryId}.", ex);
            }
        }

        public async Task<ItineraryResponseDto> UpdateItineraryStatusAsync(long itineraryId, int status)
        {
            try
            {
                var itinerary = await _context.Itineraries.FirstOrDefaultAsync(i => i.ItineraryId == itineraryId);
                if (itinerary == null)
                {
                    throw new KeyNotFoundException($"Itinerary with ID {itineraryId} not found.");
                }

                itinerary.Status = status;

                _context.Itineraries.Update(itinerary);
                await _context.SaveChangesAsync();

                return await MapItineraryToDtoAsync(itinerary);
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Error updating itinerary status with ID {itineraryId}.", ex);
            }
        }

        public async Task<bool> AddBookingToItineraryAsync(long itineraryId, long bookingId)
        {
            try
            {
                var itinerary = await _context.Itineraries.FirstOrDefaultAsync(i => i.ItineraryId == itineraryId);
                if (itinerary == null)
                {
                    throw new KeyNotFoundException($"Itinerary with ID {itineraryId} not found.");
                }

                var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.BookingId == bookingId);
                if (booking == null)
                {
                    throw new KeyNotFoundException($"Booking with ID {bookingId} not found.");
                }

                var itineraryBooking = new ItineraryBooking
                {
                    ItineraryId = itineraryId,
                    BookingId = bookingId
                };

                _context.ItineraryBookings.Add(itineraryBooking);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Error adding booking {bookingId} to itinerary {itineraryId}.", ex);
            }
        }

        public async Task<bool> RemoveBookingFromItineraryAsync(long itineraryId, long bookingId)
        {
            try
            {
                var itineraryBooking = await _context.ItineraryBookings
                    .FirstOrDefaultAsync(ib => ib.ItineraryId == itineraryId && ib.BookingId == bookingId);

                if (itineraryBooking == null)
                {
                    return false;
                }

                _context.ItineraryBookings.Remove(itineraryBooking);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Error removing booking {bookingId} from itinerary {itineraryId}.", ex);
            }
        }

        private async Task<ItineraryResponseDto> MapItineraryToDtoAsync(Itinerary itinerary)
        {
            var bookings = await _context.ItineraryBookings
                .AsNoTracking()
                .Where(ib => ib.ItineraryId == itinerary.ItineraryId)
                .Include(ib => ib.Booking)
                .Select(ib => ib.Booking)
                .ToListAsync();

            return new ItineraryResponseDto
            {
                ItineraryId = itinerary.ItineraryId,
                UserId = itinerary.UserId,
                Title = itinerary.Title,
                StartDate = itinerary.StartDate,
                EndDate = itinerary.EndDate,
                Status = itinerary.Status,
                CreatedDate = itinerary.CreatedDate,
                Bookings = bookings.Select(b => new BookingResponseDto
                {
                    BookingId = b.BookingId,
                    UserId = b.UserId,
                    PartnerId = b.PartnerId,
                    InventoryId = b.InventoryId,
                    ItemType = b.ItemType,
                    BookingDate = b.BookingDate,
                    Status = b.Status,
                    Amount = b.Amount,
                    CreatedDate = b.CreatedDate
                }).ToList()
            };
        }
    }
}
