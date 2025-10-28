using Quartz;
using StudentApp.Models;

namespace StudentApp.Services;

public interface IZamanlayiciFactory
{
    Task RecreateJobAsync(ZamanlayiciAyarlar settings);
    Task DeleteJobAsync();
}

