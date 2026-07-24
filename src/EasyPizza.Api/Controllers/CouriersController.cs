using EasyPizza.Application.Interfaces.Repositories;
using EasyPizza.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace EasyPizza.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CouriersController : ControllerBase
{
    private readonly ICourierRepository _courierRepository;

    public CouriersController(ICourierRepository courierRepository)
    {
        _courierRepository = courierRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var couriers = await _courierRepository.GetAllAsync();
        return Ok(couriers);
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActive()
    {
        var couriers = await _courierRepository.GetActiveCouriersAsync();
        return Ok(couriers);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var courier = await _courierRepository.GetByIdAsync(id);
        if (courier == null) return NotFound();
        return Ok(courier);
    }

    public class CreateCourierRequest
    {
        public string Name { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? VehiclePlate { get; set; }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCourierRequest request)
    {
        var courier = new Courier(request.Name, request.PhoneNumber, request.VehiclePlate);
        await _courierRepository.AddAsync(courier);
        return CreatedAtAction(nameof(GetById), new { id = courier.Id }, courier);
    }

    public class UpdateCourierRequest
    {
        public string Name { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? VehiclePlate { get; set; }
        public bool IsActive { get; set; }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCourierRequest request)
    {
        var courier = await _courierRepository.GetByIdAsync(id);
        if (courier == null) return NotFound();

        courier.UpdateDetails(request.Name, request.PhoneNumber, request.VehiclePlate, request.IsActive);
        await _courierRepository.UpdateAsync(courier);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var courier = await _courierRepository.GetByIdAsync(id);
        if (courier == null) return NotFound();

        await _courierRepository.DeleteAsync(courier);
        return NoContent();
    }
}
