using SysPro.Application.Interfaces;
using SysPro.Domain.Models;

namespace SysPro.Tests.Fakes;

public class FakeAppRepository : IAppRepository
{
    public ImportAuditViewModel? LastAudit { get; private set; }

    public Task<bool> InsertAudit(ImportAuditViewModel model)
    {
        LastAudit = model;
        return Task.FromResult(true);
    }
}