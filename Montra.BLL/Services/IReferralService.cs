using Montra.DAL.Entities;

namespace Montra.BLL.Services
{
    public interface IReferralService
    {
        Task<ReferralSubmissionResult> CreateAsync(ReferralRequest request);
    }
}