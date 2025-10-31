using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NaturalShop.API.Data;
using NaturalShop.API.Models;
using NaturalShop.API.DTOs;
using AutoMapper;
using AutoMapper.QueryableExtensions;

namespace NaturalShop.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<ProductController> _logger;

        public ProductController(AppDbContext context, IMapper mapper, ILogger<ProductController> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetAll()
        {
            var products = await _context.Products
                .AsNoTracking()
                .ProjectTo<ProductDto>(_mapper.ConfigurationProvider)
                .ToListAsync();
            return Ok(products);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ProductDto>> GetById(int id)
        {
            var product = await _context.Products
                .AsNoTracking()
                .Where(p => p.Id == id)
                .ProjectTo<ProductDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }

        [HttpPost]
        public async Task<ActionResult<ProductDto>> Create(CreateProductDto createProductDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var product = _mapper.Map<Product>(createProductDto);

            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            var productDto = _mapper.Map<ProductDto>(product);
            return CreatedAtAction(nameof(GetById), new { id = product.Id }, productDto);
        }
        
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, UpdateProductDto updated)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                _logger.LogWarning("Product not found — ID: {Id}", id);
                return NotFound();
            }

            _logger.LogInformation("Updating product — ID: {Id}, Old Name: {Name}, Old Price: {Price}, Old Stock: {Stock}", 
                id, product.Name, product.Price, product.Stock);

            _mapper.Map(updated, product);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Product updated successfully — ID: {Id}, New Name: {Name}, New Price: {Price}, New Stock: {Stock}", 
                id, product.Name, product.Price, product.Stock);

            return Ok(product);
        }


        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return NoContent();//Http 204 No Content
        }
    }
}


