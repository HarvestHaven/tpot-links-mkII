using TPOT_Links.Models;

namespace TPOT_Links.Controllers;

public interface ICarService
{
    List<Car> ReadAll();
}