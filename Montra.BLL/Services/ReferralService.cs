using Microsoft.EntityFrameworkCore;
using Montra.DAL.Context;
using Montra.DAL.Entities;

namespace Montra.BLL.Services
{
    public class ReferralService : IReferralService
    {
        private readonly AppDbContext _db;

        public ReferralService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<ReferralSubmissionResult> CreateAsync(ReferralRequest request)
        {
            if (!request.PrivacyConsent)
            {
                return new ReferralSubmissionResult
                {
                    Success = false,
                    Message = "Vui long xac nhan dong y xu ly thong tin truoc khi gui referral."
                };
            }

            request.Status = "New";
            request.CreatedAt = DateTime.Now;
            request.TrackingCode = await GenerateTrackingCodeAsync();

            _db.ReferralRequests.Add(request);
            await _db.SaveChangesAsync();

            return new ReferralSubmissionResult
            {
                Success = true,
                Message = "Referral da duoc gui. Doi ngu clinical se lien he trong 1-2 ngay lam viec.",
                TrackingCode = request.TrackingCode
            };
        }

        private async Task<string> GenerateTrackingCodeAsync()
        {
            for (var attempt = 0; attempt < 10; attempt++)
            {
                var code = PublicTrackingCodeGenerator.Generate("REF");
                if (!await _db.ReferralRequests.AnyAsync(r => r.TrackingCode == code))
                {
                    return code;
                }
            }

            throw new InvalidOperationException("Could not generate a unique referral tracking code.");
        }
    }
}