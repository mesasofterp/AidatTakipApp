using Quartz;
using StudentApp.Models;

namespace StudentApp.Services;

public interface IZamanlayiciFactory
{
    Task CreateJobAsync(ZamanlayiciAyarlar settings);
    Task UpdateJobAsync(ZamanlayiciAyarlar settings);
    Task DeleteJobAsync(long schedulerId);
}

