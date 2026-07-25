using EasyPizza.Application.Interfaces.Repositories;
using EasyPizza.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace EasyPizza.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CouponsController : ControllerBase
{
    private readonly ICouponRepository _couponRepository;

    public CouponsController(ICouponRepository couponRepository)
    {
        _couponRepository = couponRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var coupons = await _couponRepository.GetAllAsync();
        return Ok(coupons);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var coupon = await _couponRepository.GetByIdAsync(id);
        if (coupon == null) return NotFound();
        return Ok(coupon);
    }

    [HttpGet("validate/{code}")]
    public async Task<IActionResult> Validate(string code)
    {
        var coupon = await _couponRepository.GetByCodeAsync(code);
        if (coupon == null) return NotFound(new { error = "Cupom não encontrado." });
        if (!coupon.IsValid()) return BadRequest(new { error = "Cupom inválido ou expirado." });

        return Ok(coupon);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCouponRequest request)
    {
        var existing = await _couponRepository.GetByCodeAsync(request.Code);
        if (existing != null) return BadRequest("Cupom com este código já existe.");

        var coupon = new Coupon(request.Code, request.DiscountPercentage, request.DiscountFixedAmount, request.ExpiresAt, request.UsageLimit);
        await _couponRepository.AddAsync(coupon);
        await _couponRepository.SaveChangesAsync();
        
        return CreatedAtAction(nameof(GetById), new { id = coupon.Id }, coupon);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCouponRequest request)
    {
        var coupon = await _couponRepository.GetByIdAsync(id);
        if (coupon == null) return NotFound();

        coupon.UpdateDetails(request.DiscountPercentage, request.DiscountFixedAmount, request.ExpiresAt, request.UsageLimit, request.IsActive);
        await _couponRepository.UpdateAsync(coupon);
        await _couponRepository.SaveChangesAsync();
        
        return NoContent();
    }
}

public class CreateCouponRequest
{
    public string Code { get; set; } = string.Empty;
    public decimal? DiscountPercentage { get; set; }
    public decimal? DiscountFixedAmount { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public int UsageLimit { get; set; }
}

public class UpdateCouponRequest : CreateCouponRequest
{
    public bool IsActive { get; set; }
}
