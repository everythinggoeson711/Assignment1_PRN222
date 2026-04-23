namespace FinalAssignment.Therapy.Core.Interfaces;

public interface IUnitOfWork
{
    IAppUserRepository Users { get; }

    ITherapistRepository Therapists { get; }

    ITherapyServiceRepository TherapyServices { get; }

    IAppointmentRepository Appointments { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}