using SysPro.Domain.Models;

namespace SysPro.Application.Interfaces;

public interface IAppRepository
{
    Task<bool> InsertAudit(ImportAuditViewModel model);
}