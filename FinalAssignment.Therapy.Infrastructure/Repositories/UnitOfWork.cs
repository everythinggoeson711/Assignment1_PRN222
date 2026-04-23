using FinalAssignment.Therapy.Core.Interfaces;
using FinalAssignment.Therapy.Infrastructure.Data;

namespace FinalAssignment.Therapy.Infrastructure.Repositories;

public class UnitOfWork(
    TherapyDbContext dbContext,
    IAppUserRepository appUserRepository,
    ITherapistRepository therapistRepository,
    ITherapyServiceRepository therapyServiceRepository,
    IAppointmentRepository appointmentRepository) : IUnitOfWork
{
    public IAppUserRepository Users => appUserRepository;

    public ITherapistRepository Therapists => therapistRepository;

    public ITherapyServiceRepository TherapyServices => therapyServiceRepository;

    public IAppointmentRepository Appointments => appointmentRepository;

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await dbContext.SaveChangesAsync(cancellationToken);
}