using FinalAssignment.Therapy.Application.Models;

namespace FinalAssignment.Therapy.Application.Services;

public interface IAdminCatalogService
{
    Task<IReadOnlyList<AdminTherapistDto>> GetTherapistsAsync(CancellationToken cancellationToken = default);

    Task<AdminTherapistDto?> GetTherapistByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<ServiceResult> CreateTherapistAsync(CreateTherapistRequest request, CancellationToken cancellationToken = default);

    Task<ServiceResult> UpdateTherapistAsync(UpdateTherapistRequest request, CancellationToken cancellationToken = default);

    Task<ServiceResult> DeactivateTherapistAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AdminTherapyServiceDto>> GetServicesAsync(CancellationToken cancellationToken = default);

    Task<AdminTherapyServiceDto?> GetServiceByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<ServiceResult> CreateServiceAsync(CreateTherapyServiceRequest request, CancellationToken cancellationToken = default);

    Task<ServiceResult> UpdateServiceAsync(UpdateTherapyServiceRequest request, CancellationToken cancellationToken = default);

    Task<ServiceResult> DeactivateServiceAsync(int id, CancellationToken cancellationToken = default);
}