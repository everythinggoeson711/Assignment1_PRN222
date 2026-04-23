using FinalAssignment.Therapy.Application.Models;
using FinalAssignment.Therapy.Core.Common;
using FinalAssignment.Therapy.Core.Entities;
using FinalAssignment.Therapy.Core.Interfaces;

namespace FinalAssignment.Therapy.Application.Services;

public class AdminCatalogService(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher) : IAdminCatalogService
{
    public async Task<IReadOnlyList<AdminTherapistDto>> GetTherapistsAsync(CancellationToken cancellationToken = default)
    {
        var therapists = await unitOfWork.Therapists.GetAllAsync(cancellationToken);
        var usersByTherapistId = new Dictionary<int, string?>();

        foreach (var therapist in therapists)
        {
            var user = await unitOfWork.Users.GetByTherapistProfileIdAsync(therapist.Id, cancellationToken);
            usersByTherapistId[therapist.Id] = user?.Email;
        }

        return therapists.Select(therapist =>
            new AdminTherapistDto
            {
                Id = therapist.Id,
                Name = therapist.Name,
                Specialty = therapist.Specialty,
                PhoneNumber = therapist.PhoneNumber,
                Bio = therapist.Bio,
                IsActive = therapist.IsActive,
                Email = usersByTherapistId.GetValueOrDefault(therapist.Id)
            }).ToList();
    }

    public async Task<AdminTherapistDto?> GetTherapistByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var therapist = await unitOfWork.Therapists.GetByIdAsync(id, cancellationToken);
        if (therapist is null)
        {
            return null;
        }

        var user = await unitOfWork.Users.GetByTherapistProfileIdAsync(id, cancellationToken);
        return new AdminTherapistDto
        {
            Id = therapist.Id,
            Name = therapist.Name,
            Specialty = therapist.Specialty,
            PhoneNumber = therapist.PhoneNumber,
            Bio = therapist.Bio,
            IsActive = therapist.IsActive,
            Email = user?.Email
        };
    }

    public async Task<ServiceResult> CreateTherapistAsync(CreateTherapistRequest request, CancellationToken cancellationToken = default)
    {
        var existingUser = await unitOfWork.Users.GetByEmailAsync(request.Email.Trim(), cancellationToken);
        if (existingUser is not null)
        {
            return ServiceResult.Failure("Email này đã tồn tại.");
        }

        var therapist = new TherapistProfile
        {
            Name = request.Name.Trim(),
            Specialty = request.Specialty.Trim(),
            PhoneNumber = request.PhoneNumber.Trim(),
            Bio = request.Bio.Trim(),
            IsActive = request.IsActive
        };

        await unitOfWork.Therapists.AddAsync(therapist, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await unitOfWork.Users.AddAsync(new AppUser
        {
            FullName = therapist.Name,
            Email = request.Email.Trim(),
            PasswordHash = passwordHasher.Hash(request.Password),
            Role = UserRoles.Therapist,
            TherapistProfileId = therapist.Id,
            IsActive = request.IsActive
        }, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success("Tạo therapist thành công.", therapist.Id);
    }

    public async Task<ServiceResult> UpdateTherapistAsync(UpdateTherapistRequest request, CancellationToken cancellationToken = default)
    {
        var therapist = await unitOfWork.Therapists.GetByIdAsync(request.Id, cancellationToken);
        if (therapist is null)
        {
            return ServiceResult.Failure("Không tìm thấy therapist.");
        }

        therapist.Name = request.Name.Trim();
        therapist.Specialty = request.Specialty.Trim();
        therapist.PhoneNumber = request.PhoneNumber.Trim();
        therapist.Bio = request.Bio.Trim();
        therapist.IsActive = request.IsActive;

        await unitOfWork.Therapists.UpdateAsync(therapist, cancellationToken);

        var user = await unitOfWork.Users.GetByTherapistProfileIdAsync(request.Id, cancellationToken);
        if (user is not null)
        {
            user.FullName = therapist.Name;
            user.IsActive = request.IsActive;
            await unitOfWork.Users.UpdateAsync(user, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success("Cập nhật therapist thành công.");
    }

    public async Task<ServiceResult> DeactivateTherapistAsync(int id, CancellationToken cancellationToken = default)
    {
        var therapist = await unitOfWork.Therapists.GetByIdAsync(id, cancellationToken);
        if (therapist is null)
        {
            return ServiceResult.Failure("Không tìm thấy therapist.");
        }

        therapist.IsActive = false;
        await unitOfWork.Therapists.UpdateAsync(therapist, cancellationToken);

        var user = await unitOfWork.Users.GetByTherapistProfileIdAsync(id, cancellationToken);
        if (user is not null)
        {
            user.IsActive = false;
            await unitOfWork.Users.UpdateAsync(user, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success("Đã vô hiệu hóa therapist.");
    }

    public async Task<IReadOnlyList<AdminTherapyServiceDto>> GetServicesAsync(CancellationToken cancellationToken = default)
    {
        var services = await unitOfWork.TherapyServices.GetAllAsync(cancellationToken);
        return services.Select(service => new AdminTherapyServiceDto
        {
            Id = service.Id,
            Name = service.Name,
            Description = service.Description,
            Price = service.Price,
            DurationMinutes = service.DurationMinutes,
            IsActive = service.IsActive
        }).ToList();
    }

    public async Task<AdminTherapyServiceDto?> GetServiceByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var service = await unitOfWork.TherapyServices.GetByIdAsync(id, cancellationToken);
        return service is null
            ? null
            : new AdminTherapyServiceDto
            {
                Id = service.Id,
                Name = service.Name,
                Description = service.Description,
                Price = service.Price,
                DurationMinutes = service.DurationMinutes,
                IsActive = service.IsActive
            };
    }

    public async Task<ServiceResult> CreateServiceAsync(CreateTherapyServiceRequest request, CancellationToken cancellationToken = default)
    {
        var service = new TherapyService
        {
            Name = request.Name.Trim(),
            Description = request.Description.Trim(),
            Price = request.Price,
            DurationMinutes = request.DurationMinutes,
            IsActive = request.IsActive
        };

        await unitOfWork.TherapyServices.AddAsync(service, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success("Tạo service thành công.", service.Id);
    }

    public async Task<ServiceResult> UpdateServiceAsync(UpdateTherapyServiceRequest request, CancellationToken cancellationToken = default)
    {
        var service = await unitOfWork.TherapyServices.GetByIdAsync(request.Id, cancellationToken);
        if (service is null)
        {
            return ServiceResult.Failure("Không tìm thấy service.");
        }

        service.Name = request.Name.Trim();
        service.Description = request.Description.Trim();
        service.Price = request.Price;
        service.DurationMinutes = request.DurationMinutes;
        service.IsActive = request.IsActive;

        await unitOfWork.TherapyServices.UpdateAsync(service, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success("Cập nhật service thành công.");
    }

    public async Task<ServiceResult> DeactivateServiceAsync(int id, CancellationToken cancellationToken = default)
    {
        var service = await unitOfWork.TherapyServices.GetByIdAsync(id, cancellationToken);
        if (service is null)
        {
            return ServiceResult.Failure("Không tìm thấy service.");
        }

        service.IsActive = false;
        await unitOfWork.TherapyServices.UpdateAsync(service, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return ServiceResult.Success("Đã vô hiệu hóa service.");
    }
}